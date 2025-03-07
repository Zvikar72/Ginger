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
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Ginger.ExtensionMethods;
using static GingerCore.Agent;

namespace Ginger.Agents.AddAgentWizardLib
{
    /// <summary>
    /// Interaction logic for AddAgentDetailsPage.xaml
    /// </summary>
    public partial class AddAgentDetailsPage : Page, IWizardPage
    {
        AddAgentWizard mWizard;
        public readonly ObservableList<PluginPackage> Plugins;
        private List<DriverInfo> DriversforPlatform = [];

        public AddAgentDetailsPage()
        {
            InitializeComponent();
            Plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            mWizard = (AddAgentWizard)WizardEventArgs.Wizard;
            PropertyChangedEventManager.AddHandler(mWizard.Agent, Agent_PropertyChanged, propertyName: string.Empty);
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    xAgentNameTextBox.BindControl(mWizard.Agent, nameof(Agent.Name));
                    xAgentNameTextBox.AddValidationRule(new AgentNameValidationRule());
                    xAgentNameTextBox.Focus();

                    xAgentDescriptionTextBox.BindControl(mWizard.Agent, nameof(Agent.Notes));
                    xAgentTagsViewer.Init(mWizard.Agent.Tags);

                    //Removing ASCF from platform combobox                    
                    List<dynamic> platformesToExclude = [GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.ASCF];
                    //GingerCore.General.FillComboFromEnumObj(xPlatformTypeComboBox, mWizard.Agent.Platform, excludeList:platformesToExclude);//All
                    //Only platforms used in Solution (having Target Application for)
                    List<object> onlySolutionPlatforms = WorkSpace.Instance.Solution.ApplicationPlatforms.Select(x => x.Platform).Distinct().ToList().Cast<object>().ToList();
                    GingerCore.General.FillComboFromEnumObj(xPlatformTypeComboBox, mWizard.Agent.Platform, values: onlySolutionPlatforms, excludeList: platformesToExclude);

                    xPlatformTypeComboBox.SelectionChanged += xPlatformTypeComboBox_SelectionChanged;
                    ////set Web as default
                    //foreach(object platform in xPlatformTypeComboBox.Items)
                    //{
                    //    if(platform.ToString() == GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Web.ToString())
                    //    {
                    //        xPlatformTypeComboBox.SelectedItem = platform;
                    //        break;
                    //    }
                    //}
                    xPlatformTypeComboBox.SelectedIndex = 0;

                    xDriverTypeComboBox.BindControl(mWizard.Agent.AgentOperations, nameof(AgentOperations.DriverInfo));
                    xDriverTypeComboBox.SelectionChanged += xDriverTypeComboBox_SelectionChanged;
                    if (xDriverTypeComboBox.Items.Count > 0)
                    {
                        xDriverTypeComboBox.SelectedItem = xDriverTypeComboBox.Items[0];
                    }

                    UpdateDriverTypeCombobox();
                    UpdateDriverSubtypeCombobox();

                    xDriverTypeComboBox.AddValidationRule(eValidationRule.CannotBeEmpty);

                    xDriverTypeComboBox.Visibility = Visibility.Visible;

                    break;
            }

        }

        private void Agent_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(Agent.DriverType)))
            {
                mWizard.Agent.AgentOperations.InitDriverConfigs();
                if (mWizard.Agent.Platform == ePlatformType.Web && DriverSupportMultipleBrowsers(mWizard.Agent.DriverType))
                {
                    PopulateBrowserTypeComboBox();
                    BrowserTypePanel.Visibility = Visibility.Visible;
                }
                else
                {
                    BrowserTypeComboBox.Items.Clear();
                    BrowserTypePanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private bool DriverSupportMultipleBrowsers(eDriverType driverType)
        {
            return driverType is eDriverType.Selenium or eDriverType.Playwright;
        }

        private void xPlatformTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDriverTypeCombobox();
        }

        private void UpdateDriverTypeCombobox()
        {
            xDriverTypeComboBox.SelectedItem = null;
            xDriverTypeComboBox.Items.Clear();
            mWizard.Agent.Platform = (GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType)Enum.Parse(typeof(GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType), xPlatformTypeComboBox.SelectedValue.ToString());
            DriversforPlatform = DriverInfo.GetDriversforPlatform(mWizard.Agent.Platform);

            foreach (DriverInfo driverInfo in DriversforPlatform)
            {
                xDriverTypeComboBox.Items.Add(driverInfo);
            }

            if (xDriverTypeComboBox.Items.Count > 0)
            {
                //mWizard.Agent.DriverInfo = DriversforPlatform[0];
                xDriverTypeComboBox.SelectedItem = xDriverTypeComboBox.Items[0];
            }

            if (mWizard.Agent.Platform == ePlatformType.Web)
            {
                PopulateBrowserTypeComboBox();
                BrowserTypePanel.Visibility = Visibility.Visible;
            }
            else
            {
                BrowserTypePanel.Visibility = Visibility.Collapsed;
                BrowserTypeComboBox.Items.Clear();
            }
        }

        private void PopulateBrowserTypeComboBox()
        {
            BrowserTypeComboBox.Items.Clear();
            foreach (WebBrowserType browserType in GingerWebDriver.GetSupportedBrowserTypes(mWizard.Agent.DriverType))
            {
                BrowserTypeComboBox.Items.Add(new ComboEnumItem()
                {
                    text = browserType.ToString(),
                    Value = browserType
                });
            }
            BrowserTypeComboBox.SelectedValuePath = nameof(ComboEnumItem.Value);
            BrowserTypeComboBox.SelectedIndex = 0;
        }

        private void BrowserTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
            {
                return;
            }

            WebBrowserType selectedBrowserType = (WebBrowserType)BrowserTypeComboBox.SelectedValue;
            DriverConfigParam browserTypeParam = mWizard.Agent.GetOrCreateParam(nameof(GingerWebDriver.BrowserType));
            if (!string.Equals(browserTypeParam.Value, selectedBrowserType.ToString()))
            {
                browserTypeParam.Value = selectedBrowserType.ToString();
            }
        }

        private void xDriverTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDriverSubtypeCombobox();

        }

        private void UpdateDriverSubtypeCombobox()
        {
            xDriverSubTypeComboBox.SelectionChanged -= XDriverSubTypeComboBox_SelectionChanged;
            xDriverSubTypeComboBox.Items.Clear();
            xDriverSubTypeStackPanel.Visibility = Visibility.Visible;
            if (xDriverTypeComboBox.SelectedItem != null)
            {

                if (xDriverTypeComboBox.SelectedItem is DriverInfo DI)
                {
                    ((AgentOperations)mWizard.Agent.AgentOperations).DriverInfo = DI;

                    if (DI.isDriverPlugin)
                    {
                        mWizard.Agent.AgentType = Agent.eAgentType.Service;
                        mWizard.Agent.PluginId = DI.Name;
                    }
                    else
                    {
                        mWizard.Agent.AgentType = Agent.eAgentType.Driver;
                    }

                    foreach (var service in ((AgentOperations)mWizard.Agent.AgentOperations).DriverInfo.services)
                    {
                        xDriverSubTypeComboBox.Items.Add(service);
                    }


                    xDriverSubTypeComboBox.SelectionChanged += XDriverSubTypeComboBox_SelectionChanged;
                    xDriverSubTypeComboBox.SelectedItem = xDriverSubTypeComboBox.Items[0];


                    if (DI.services.Count == 0)
                    {
                        mWizard.Agent.AgentOperations.InitDriverConfigs();
                    }
                }
            }
        }

        private void XDriverSubTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xDriverTypeComboBox.SelectedItem is DriverInfo DI && xDriverSubTypeComboBox != null)
            {
                ((AgentOperations)mWizard.Agent.AgentOperations).DriverInfo = DI;
                //foreach (var service in mWizard.Agent.DriverInfo.services)
                //{
                //    xDriverSubTypeComboBox.Items.Add(service);
                //}
                string SubdriverType = xDriverSubTypeComboBox.SelectedItem.ToString();
                if (DI.isDriverPlugin)
                {
                    mWizard.Agent.ServiceId = SubdriverType;
                }
                else
                {
                    mWizard.Agent.DriverType = (eDriverType)Enum.Parse(typeof(eDriverType), SubdriverType);
                }
                mWizard.Agent.AgentOperations.InitDriverConfigs();
            }
        }

        void ShowConfig()
        {
            xDriverConfigStackPanel.Visibility = Visibility.Visible;
        }


    }
}
