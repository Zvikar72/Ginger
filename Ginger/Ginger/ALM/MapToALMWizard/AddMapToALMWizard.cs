﻿#region License
/*
Copyright © 2014-2021 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.WizardLib;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM.QC;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace Ginger.ALM.MapToALMWizard
{
    /// <summary>
    /// AddMapToALMWizard
    /// Wizard for manual mapping Alm Test Set to Ginger Business Flow.
    /// User will map the test objects and will get link between Alm & Ginger
    /// For Extra oerations.
    /// </summary>
    class AddMapToALMWizard : WizardBase, INotifyPropertyChanged
    {
        internal BusinessFlow mapBusinessFlow { get; private set; }
        private ALMTestSet almTestSetDetails;
        private Dictionary<string, ALMTSTest> reMapActivitiesGroupsDic = new Dictionary<string, ALMTSTest>();

        public void SetReMapAGSDic(string key, ALMTSTest value)
        {
            if (reMapActivitiesGroupsDic.ContainsKey(key))
            {
                reMapActivitiesGroupsDic[key] = value;
            }
            else
            {
                reMapActivitiesGroupsDic.Add(key, value);
            }
        }

        public ALMTSTest GetReMapAGSDic(string key)
        {
            ALMTSTest result = null;

            if (reMapActivitiesGroupsDic.ContainsKey(key))
            {
                result = reMapActivitiesGroupsDic[key];
            }

            return result;
        }
        public bool IsReMapAGSDicContainsKey(string key)
        {
            return reMapActivitiesGroupsDic.ContainsKey(key);
        }
        public ALMTestSet AlmTestSetData
        {
            get
            {
                if (almTestSetDetails == null)
                {
                    almTestSetDetails = new ALMTestSet();
                }
                return almTestSetDetails;
            }
            set
            {
                almTestSetDetails = value;
            }
        }
        public ObservableList<ALMTestCaseManualMappingConfig> testCasesMappingList = new ObservableList<ALMTestCaseManualMappingConfig>();

        public ObservableList<ALMTSTest> testCasesUnMappedList = new ObservableList<ALMTSTest>();

        public Dictionary<String, ObservableList<ALMTSTestStep>> testCaseUnmappedStepsDic = new Dictionary<string, ObservableList<ALMTSTestStep>>();
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        public override string Title { get { return String.Format("Map To {0} Wizard.", ALMIntegration.Instance.GetALMType()); } }

        public AddMapToALMWizard(BusinessFlow businessFlow)
        {
            ALMIntegration.Instance.GetDefaultAlmConfig();
            mapBusinessFlow = businessFlow;
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Map To ALM Introduction", Page: new WizardIntroPage("/ALM/MapToALMWizard/MappedToALMIntro.md"));
            AddPage(Name: "Test Set Mapping", Title: "Test Set Mapping", SubTitle: GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, $"Select matching ALM Test Set to Ginger ‘{mapBusinessFlow.Name}’"), Page: new TestSetMappingPage());
            AddPage(Name: "Test Cases Mapping", Title: "Test Cases Mapping", SubTitle: GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup, "Select matching ALM Test Case foreach Ginger"), Page: new TestCasesMappingPage(mapBusinessFlow));
            AddPage(Name: "Test Steps Mapping", Title: "Test Steps Mapping", SubTitle: GingerDicser.GetTermResValue(eTermResKey.Activity, $"Select matching ALM Steps foreach Ginger"), Page: new TestStepMappingPage());
        }
        /// <summary>
        /// 1. Add business Flow activities groups to testCasesMappingList.
        /// 2. Add Mapped Activities Group ExternalID's to reMapActivitiesGroupsDic.
        /// </summary>
        public void AddActivitiesGroupsInitialMapping()
        {
            foreach (ActivitiesGroup ag in mapBusinessFlow.ActivitiesGroups)
            {
                testCasesMappingList.Add(new ALMTestCaseManualMappingConfig() { activitiesGroup = ag });
                if (!String.IsNullOrEmpty(ag.ExternalID))
                {
                    SetReMapAGSDic(ag.ExternalID, null);
                }
            }
        }
        /// <summary>
        /// Set AlmTestSetData property , Mapped Business Flow Data.
        /// Field To Set: TestSetID, TestSetInternalID2, TestSetName, List of Tests.
        /// </summary>
        internal void SetMappedALMTestSetData()
        {
            AlmTestSetData.TestSetID = mapBusinessFlow.ExternalID;
            AlmTestSetData.TestSetInternalID2 = mapBusinessFlow.ExternalID2;
            ALMIntegration.Instance.GetALMTestSetData(AlmTestSetData);
        }
        /// <summary>
        /// Mapping results after Mapping finished.
        /// </summary>
        public override void Finish()
        {
            try
            {
                // Map Test Set
                mapBusinessFlow.ExternalID = AlmTestSetData.TestSetID;
                mapBusinessFlow.ExternalID2 = AlmTestSetData.TestSetInternalID2;
                RemoveActivitiesGroupsExternalIds();
                MapActivitiesGroupsExternalIds();
                Reporter.ToStatus(eStatusMsgKey.SaveItem, null, mapBusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mapBusinessFlow);
                Reporter.HideStatusMessage();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Test Set Id's Failed mapping to ");
            }
        }
        /// <summary>
        /// Map Activities Groups/Activities externals ids Respectively Test Cases/Test Steps ids. 
        /// </summary>
        private void MapActivitiesGroupsExternalIds()
        {
            foreach (ALMTestCaseManualMappingConfig mappedTestCase in testCasesMappingList)
            {
                if (mappedTestCase.aLMTSTest is not null && mappedTestCase.aLMTSTest.TestName is not null)
                {
                    mappedTestCase.activitiesGroup.ExternalID = mappedTestCase.aLMTSTest.TestID;
                    mappedTestCase.activitiesGroup.ExternalID2 = mappedTestCase.aLMTSTest.LinkedTestID;
                    // Map Test Step
                    foreach (ALMTestStepManualMappingConfig mappedTestStep in mappedTestCase.testStepsMappingList)
                    {
                        if (mappedTestStep.almTestStep != null)
                        {
                            mappedTestStep.activity.ActivityExternalID = mappedTestStep.almTestStep.StepID;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove activities Groups & Activities external ids.
        /// </summary>
        private void RemoveActivitiesGroupsExternalIds()
        {
            foreach (ActivitiesGroup ag in mapBusinessFlow.ActivitiesGroups)
            {
                foreach (ActivityIdentifiers act in ag.ActivitiesIdentifiers)
                {
                    act.ExternalID = null;
                    act.ActivityExternalID = null;
                }
                ag.ExternalID = null;
                ag.ExternalID2 = null;
            }
        }
        /// <summary>
        /// 1. Update Mapped test cases dictionary.
        /// 2. Update testCases UnMapped List.
        /// </summary>
        internal void UpdateMappedTestCasescollections()
        {
            foreach (ALMTSTest testCase in AlmTestSetData.Tests)
            {
                if (IsReMapAGSDicContainsKey(testCase.TestID))
                {
                    SetReMapAGSDic(testCase.TestID, testCase);
                }
                else
                {
                    testCasesUnMappedList.Add(testCase);
                }
            }
        }
        /// <summary>
        /// 1. Remap testCasesMappingList.
        /// 2. Remap test Steps Lists.
        /// </summary>
        internal void RemapTestCasesLists()
        {
            foreach (ALMTestCaseManualMappingConfig tc in testCasesMappingList)
            {
                if (!String.IsNullOrEmpty(tc.activitiesGroup.ExternalID))
                {
                    tc.aLMTSTest = GetReMapAGSDic(tc.activitiesGroup.ExternalID);
                    ReMapTestSteps(tc);
                }
            }
        }
        /// <summary>
        /// 1. Remap testStepsMappingList.
        /// 2. Set testCaseUnmappedStepsDic.
        /// </summary>
        /// <param name="tc">Mapped Test Case</param>
        private void ReMapTestSteps(ALMTestCaseManualMappingConfig tc)
        {
            Dictionary<string, ALMTSTestStep> almStepsDic = new Dictionary<string, ALMTSTestStep>();
            foreach (ALMTSTestStep tStep in tc.aLMTSTest.Steps)
            {
                almStepsDic.Add(tStep.StepID, tStep);
            }
            // Remap test steps
            foreach (ActivityIdentifiers act in tc.activitiesGroup.ActivitiesIdentifiers)
            {
                tc.testStepsMappingList.Add(new ALMTestStepManualMappingConfig() { activity = act });
                if (!String.IsNullOrEmpty(act.ActivityExternalID))
                {
                    if (almStepsDic.ContainsKey(act.ActivityExternalID))
                    {
                        tc.testStepsMappingList[tc.testStepsMappingList.Count - 1].almTestStep = almStepsDic[act.ActivityExternalID];
                        almStepsDic.Remove(act.ActivityExternalID);
                    }
                }
            }
            // Set unmapped steps list after remove all mapped steps.
            testCaseUnmappedStepsDic.Add(tc.activitiesGroup.ExternalID, new ObservableList<ALMTSTestStep>());
            foreach (ALMTSTestStep unmappedTStep in almStepsDic.Values)
            {
                testCaseUnmappedStepsDic[tc.activitiesGroup.ExternalID].Add(unmappedTStep);
            }
        }
        internal void SetSelectedTreeTestSetData(Page win)
        {
            dynamic SelectedTestSetData = ALMIntegration.Instance.GetSelectedImportTestSetData(win);
            if (SelectedTestSetData is not null)
            {
                if (AlmTestSetData.TestSetID == null || AlmTestSetData.TestSetID != SelectedTestSetData.Id)
                {
                    testCasesUnMappedList.Clear();
                    AlmTestSetData.Tests.Clear();
                    foreach (ALMTestCaseManualMappingConfig testCaseMapping in testCasesMappingList)
                    {
                        testCaseMapping.Clear();
                    }
                    SetSelectedALMTestSetData(SelectedTestSetData);
                    foreach (ALMTSTest testCase in AlmTestSetData.Tests)
                    {
                        testCasesUnMappedList.Add(testCase);
                    }
                }
            }
            else
            {
                // Add validation
            }
        }
        /// <summary>
        /// Set new selected ALM Test Set data.
        /// </summary>
        /// <param name="SelectedTestSetData">ALM selected Test Set object</param>
        private void SetSelectedALMTestSetData(dynamic SelectedTestSetData)
        {
            AlmTestSetData.TestSetID = SelectedTestSetData.Id;
            AlmTestSetData.TestSetName = SelectedTestSetData.Name;
            AlmTestSetData.TestSetPath = SelectedTestSetData.Path;
            AlmTestSetData.TestSetInternalID2 = SelectedTestSetData.TestSetID;
            AlmTestSetData = ALMIntegration.Instance.GetALMTestCases(AlmTestSetData);
        }
    }
}