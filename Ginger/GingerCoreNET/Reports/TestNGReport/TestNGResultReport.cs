#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCoreNET.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginger.Reports
{
    class TestNGResultReport : XMLReportBase
    {
        public override string CreateReport(ReportInfo RI, bool statusByGroupActivity, ObservableList<ActInputValue> DynamicParameters)
        {

            base.RI = RI;
            string runSetName = WorkSpace.Instance.RunsetExecutor.RunSetConfig.ItemName;
            string runGroupName = "GingerRunGroup";
            string executionTime = RI.DateCreated;
            string environment = RI.ExecutionEnv;

            List<BusinessFlowReport> BizFlows = RI.BusinessFlows;
            int passCount = 0, failedCount = 0, blockedCount = 0, totalCount = 0;
            if (BizFlows.Count == 0)
            {
                return "There are no " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + " to create report";
            }

            string reportHeader = @"<?xml version=""1.0"" encoding=""UTF-8""?>";
            string reportGroups = @"
        <groups>
            <group name=""" + runGroupName + @""">";
            string reportClasses = "";
            string BFNames = "";

            foreach (BusinessFlowReport BFR in BizFlows)
            {
                string BFResults = "";
                BusinessFlow BF = BFR.GetBusinessFlow();
                totalCount = totalCount + BF.Activities.Count;
                reportClasses = reportClasses + @"
            <class name=""" + BF.Name + @".Ginger Business Flow"">";
                if (!statusByGroupActivity)
                {
                    BFResults = CraeteActivitiesStatusXml(BF, ref passCount, ref failedCount, ref blockedCount);
                }
                else
                {
                    BFResults = CraeteGroupActivitiesStatusXml(BF, ref passCount, ref failedCount, ref blockedCount);
                }

                reportClasses = reportClasses + BFResults + @"
            </class>";

                BFNames = BFNames + @"
                <method signature=""" + General.ConvertInvalidXMLCharacters(BF.Name) + @""" name=""" + General.ConvertInvalidXMLCharacters(BF.Name) + @""" class=""" + General.ConvertInvalidXMLCharacters(BF.Name) + ".Ginger Business Flow" + @"""/>";
            }
            reportHeader = reportHeader + @"
<testng-results Runset=""" + runSetName + @""" ExecutionTime=""" + executionTime + @""" Environment=""" + environment + @""" blocked=""" + blockedCount + @""" failed=""" + failedCount + @""" total=""" + totalCount + @""" passed=""" + passCount + @""">";

            if (DynamicParameters.Count > 0)
            {
                reportHeader = reportHeader + @"
    <dynamic-parameters>";
                foreach (ActInputValue actInput in DynamicParameters)
                {
                    reportHeader = reportHeader + @"
        <paramter name =""" + actInput.Param + @""" value=""" + actInput.ValueForDriver + @"""/>";
                }
                reportHeader = reportHeader + @"
    </dynamic-parameters>";

            }

            reportHeader = reportHeader + @"
        <suite name=""" + General.ConvertInvalidXMLCharacters(runSetName) + @""">";
            reportGroups = reportGroups + BFNames + @"
                </group>
            </groups>";
            string xml = "";

            xml = xml + reportHeader + reportGroups + @"
            <test name=""GingerTest"">" + reportClasses;

            xml = xml + @"
            </test>
        </suite>
</testng-results>
";
            return xml;
        }

        private string CraeteGroupActivitiesStatusXml(BusinessFlow BF, ref int passCount, ref int failedCount, ref int blockedCount)
        {
            string BFResults = string.Empty;
            long? elapsed = 0;
            DateTime startedAt = new DateTime();
            DateTime finishedAt = new DateTime();

            if (BF.ActivitiesGroups.Count > 0)
            {
                foreach (var item in BF.ActivitiesGroups)
                {
                    if (item.ActivitiesIdentifiers.Count > 0)
                    {
                        List<Activity> acts = item.ActivitiesIdentifiers.Select(a => a.IdentifiedActivity).ToList();
                        eRunStatus status = getGroupActivityStatus(acts, ref elapsed, ref startedAt, ref finishedAt);
                        BFResults += buildXml(status, ref passCount, ref failedCount, ref blockedCount, General.ConvertInvalidXMLCharacters(BF.Name), General.ConvertInvalidXMLCharacters(item.Name), General.ConvertInvalidXMLCharacters(item.Description), elapsed, startedAt, finishedAt);
                    }
                }

                //create ungrouped for activities that are not in any group
                List<Activity> unGroupedAct = [];
                foreach (var item in BF.Activities)
                {
                    if (item.ActivitiesGroupID == string.Empty)
                    {
                        unGroupedAct.Add(item);
                    }
                }
                if (unGroupedAct.Count > 0)
                {
                    eRunStatus status = getGroupActivityStatus(unGroupedAct, ref elapsed, ref startedAt, ref finishedAt);
                    BFResults += buildXml(status, ref passCount, ref failedCount, ref blockedCount, General.ConvertInvalidXMLCharacters(BF.Name), "Ungrouped", "Ungrouped", elapsed, startedAt, finishedAt);
                }
            }
            else//if there are no groups create default group
            {
                eRunStatus status = getGroupActivityStatus(BF.Activities.ToList(), ref elapsed, ref startedAt, ref finishedAt);
                BFResults += buildXml(status, ref passCount, ref failedCount, ref blockedCount, General.ConvertInvalidXMLCharacters(BF.Name), "Ungrouped", "Ungrouped", elapsed, startedAt, finishedAt);
            }

            return BFResults;
        }

        private eRunStatus getGroupActivityStatus(List<Activity> activityList, ref long? elapsed, ref DateTime startedAt, ref DateTime finishedAt)
        {
            eRunStatus status = eRunStatus.Skipped;

            //if there is one fail then Activity status is fail
            if (activityList.FirstOrDefault(x => x.Status == eRunStatus.Failed) != null)
            {
                status = eRunStatus.Failed;
            }
            else
            {
                // If we have at least 1 pass then it passed, otherwise will remain Skipped
                if (activityList.FirstOrDefault(x => x.Status == eRunStatus.Passed) != null)
                {
                    status = eRunStatus.Passed;
                }
            }

            foreach (var item in activityList)
            {
                elapsed += item.Elapsed;
            }
            if (activityList.Any(x => (x.Status is eRunStatus.Passed or eRunStatus.Failed)))
            {
                startedAt = activityList.FirstOrDefault(x => (x.Status is eRunStatus.Passed or eRunStatus.Failed)).StartTimeStamp;
                finishedAt = activityList.LastOrDefault(x => (x.Status is eRunStatus.Passed or eRunStatus.Failed)).EndTimeStamp;
            }

            return status;
        }


        private string CraeteActivitiesStatusXml(BusinessFlow BF, ref int passCount, ref int failedCount, ref int blockedCount)
        {
            string BFResults = string.Empty;
            foreach (Activity activity in BF.Activities)
            {
                BFResults += buildXml(activity.Status, ref passCount, ref failedCount, ref blockedCount, General.ConvertInvalidXMLCharacters(BF.Name), General.ConvertInvalidXMLCharacters(activity.ActivityName), General.ConvertInvalidXMLCharacters(activity.Description), activity.Elapsed, activity.StartTimeStamp, activity.EndTimeStamp);
            }
            return BFResults;
        }

        private string buildXml(eRunStatus? status, ref int passCount, ref int failedCount, ref int blockedCount, string BFName, string activityName, string description, long? elapsed, DateTime startedAt, DateTime finishedAt)
        {
            string BFResults = string.Empty;
            if (status == eRunStatus.Passed)
            {
                passCount++;
                BFResults = BFResults + @"
                 <test-method status=""PASS"" signature=""Ginger RunSet " + BFName + @""" name=""" + activityName +
                 @""" duration-ms=""" + elapsed + @""" started-at=""" + startedAt + @""" description=""" + description + @""" finished-at=""" + finishedAt + @""">
                 </test-method>";
            }
            else if (status == eRunStatus.Failed)
            {
                failedCount++;
                BFResults = BFResults + @"
                 <test-method status=""FAIL"" signature=""Ginger RunSet " + BFName + @""" name=""" + activityName +
                 @""" duration-ms=""" + elapsed + @""" started-at=""" + startedAt + @""" description=""" + description + @""" finished-at=""" + finishedAt + @""">
                 </test-method>";
            }
            else if (status is eRunStatus.Blocked or eRunStatus.Pending or eRunStatus.Skipped)
            {
                blockedCount++;
                BFResults = BFResults + @"
                 <test-method status=""SKIP"" signature=""Ginger RunSet " + BFName + @""" name=""" + activityName +
                  @""" duration-ms=""" + elapsed + @""" started-at=""" + startedAt + @""" description=""" + description + @""" finished-at=""" + finishedAt + @""">
                 </test-method>";
            }
            return BFResults;
        }
    }
}
