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
using Amdocs.Ginger.Common.InterfacesLib;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Ginger.Run
{
    public partial class ApplicationAgentSelectionPage : Page
    {
        GenericWindow _pageGenericWin = null;
        ApplicationAgent mApplicationAgent;
        GingerExecutionEngine mGingerRunner;

        public ApplicationAgentSelectionPage(GingerExecutionEngine gingerRunner, ApplicationAgent applicationAgent)
        {
            InitializeComponent();

            mGingerRunner = gingerRunner;
            mApplicationAgent = applicationAgent;

            SetPossibleAgentsGridView();
            SetPossibleAgentsGridData();
        }

        private void SetPossibleAgentsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(Agent.Name), Header = "Agent Name", WidthWeight = 100, ReadOnly = true },
            ]
            };
            grdPossibleAgents.SetAllColumnsDefaultView(defView);
            grdPossibleAgents.InitViewItems();

            grdPossibleAgents.Grid.SelectionMode = DataGridSelectionMode.Single;
            WeakEventManager<ucGrid, EventArgs>.AddHandler(source: grdPossibleAgents, eventName: nameof(ucGrid.RowDoubleClick), handler: grdPossibleAgents_RowDoubleClick);
        }

        private void SetPossibleAgentsGridData()
        {
            ObservableList<Agent> optionalAgents = [];
            if (mApplicationAgent != null)
            {
                //find out the target application platform
                ApplicationPlatform ap = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == mApplicationAgent.AppName select x).FirstOrDefault();
                if (ap != null)
                {
                    ePlatformType appPlatform = ap.Platform;

                    //get the solution Agents which match to this platform
                    //List<Agent> optionalAgentsList = (from p in  WorkSpace.Instance.Solution.Agents where p.Platform == appPlatform select p).ToList();
                    List<Agent> optionalAgentsList = (from p in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where p.Platform == appPlatform select p).ToList();
                    if (optionalAgentsList != null && mGingerRunner != null)
                    {
                        //remove already mapped agents
                        List<IApplicationAgent> mappedApps = mGingerRunner.GingerRunner.ApplicationAgents.Where(x => x.Agent != null).ToList();
                        foreach (ApplicationAgent mappedApp in mappedApps)
                        {
                            if (mappedApp.Agent.Platform == appPlatform && mappedApp != mApplicationAgent)
                            {
                                optionalAgentsList.Remove(mappedApp.Agent);
                            }
                        }

                        foreach (Agent agent in optionalAgentsList)
                        {
                            optionalAgents.Add(agent);
                        }
                    }
                }
            }


            // FIXME : !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // Add Plugin agents
            // if (mApplicationAgent.target - plugin...) search based on type
            // Search plugins            
            var list = from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.ServiceId == mApplicationAgent.AppName select x;
            foreach (Agent agent in list)
            {
                optionalAgents.Add(agent);
            }

            if (optionalAgents.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.NoOptionalAgent);
            }

            grdPossibleAgents.DataSourceList = optionalAgents;

            //select the current mapped agent in the list
            foreach (Agent agent in optionalAgents)
            {
                if (agent == mApplicationAgent.Agent)
                {
                    grdPossibleAgents.Grid.SelectedItem = agent;
                }
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button mapBtn = new Button
            {
                Content = "Map"
            };
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: mapBtn, eventName: nameof(ButtonBase.Click), handler: mapBtn_Click);

            ObservableList<Button> winButtons = [mapBtn];

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "'" + mApplicationAgent.AppName + "'- Agent Mapping", this, winButtons, true, "Cancel");
        }

        private void mapBtn_Click(object sender, RoutedEventArgs e)
        {
            MapSelectedAgent();
        }

        private void grdPossibleAgents_RowDoubleClick(object sender, EventArgs e)
        {
            MapSelectedAgent();
        }

        private void MapSelectedAgent()
        {
            if (grdPossibleAgents.Grid.SelectedItem == null)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }
            else
            {
                Agent selectedAgent = (Agent)grdPossibleAgents.Grid.SelectedItem;
                mApplicationAgent.Agent = selectedAgent;

                //save last used agent on the Solution Target Applications
                ApplicationPlatform ap = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == mApplicationAgent.AppName);
                if (ap != null)
                {
                    ap.LastMappedAgentName = selectedAgent.Name;
                }
            }

            _pageGenericWin.Close();
        }
    }
}
