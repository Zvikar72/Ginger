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

extern alias UIAComWrapperNetstandard;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Ginger.WindowExplorer.Java;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace Ginger.WindowExplorer.Common
{
    // This class need to be common for table action for all type of drivers!!!!
    // do not put specific driver code here - OO thinking

    //TODO: cleanup non generic driver code

    /// <summary>
    /// Interaction logic for TableActionsPage.xaml
    /// </summary>
    public partial class TableActionsPage
    {
        List<String> mColNames = null;
        int mRowCount = 0;

        private UIAuto.AutomationElement AEControl;
        private int rowCount = -1;
        private int columnCount = 0;

        // TOOD: need to be OO and generic
        UIAuto.AutomationElement[,] gridArray;

        ElementInfo mElementInfo;
        ObservableList<Act> mActions = null;
        ObservableList<Act> mOriginalActions = null;

        public TableActionsPage(ElementInfo ElementInfo, ObservableList<Act> Actions)
        {
            mElementInfo = ElementInfo;
            mActions = Actions;
            InitializeComponent();
            InitTableInfo();
            ShowCellActions();
        }

        private void InitTableInfo()
        {
            //TODO: return common table info which all driver can return
            //TableInfo - columns etc...

            object o = mElementInfo.GetElementData();

            //Create sample columns
            mColNames = ["aaa", "bbb", "cc"];
            ColName.ItemsSource = mColNames;
        }

        void ShowCellActions()
        {
            // Keep original actions 
            if (mOriginalActions == null)
            {
                mOriginalActions = [.. mActions];
            }
        }

        private enum Platform
        {
            Java,
            PowerBuilder,
            Windows
        }

        // TOOD: need to be OO and generic can pass ElementInfo
        public TableActionsPage(UIAuto.AutomationElement AE)
        {
            AEControl = AE;
            LoadGridToArray();
            LoadColumnNameCombo();
            mRowCount = rowCount;
            InitializeComponent();
            SetComponents();
        }

        // TOOD: need to be OO and generic can pass ElementInfo
        public TableActionsPage(JavaTableTreeItem JT, List<String> ColNames, int RowCount, Object Obj = null)
        {
            mColNames = ColNames;
            mRowCount = RowCount;

            InitializeComponent();
            SetComponents();
        }

        private void SetComponents()
        {
            ColName.ItemsSource = mColNames;
            for (int i = 0; i <= mRowCount; i++)
            {
                Row.Items.Add(i.ToString());
            }
            ActTableElement ACJT = new ActTableElement();
            GingerCore.General.FillComboFromEnumObj(RunActionOn, ACJT.RunActionOn);
        }

        private void Colomn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRelatedActions();
            int i = 0;
            foreach (string S in mColNames)
            {
                if (S == ColName.SelectedValue.ToString())
                {
                    ColNum.Text = i.ToString();
                    break;
                }
                i++;
            }
        }

        private void Row_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRelatedActions();
        }

        private void RunActionOn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRelatedActions();
        }

        private void UpdateRelatedActions()
        {
            RestoreOriginalActions();
            //This is just example to show the actions can change
            // need to be per selected filter user did

            string SelectCol = ColName.SelectedValue.ToString();
            // Add some sample for specific cell
            mActions.Add(new ActTableElement() { Description = "Get Value of Cell: " + SelectCol + " Row:4" });
            mActions.Add(new ActTableElement() { Description = "Set Value of Cell: " + SelectCol + " Row:4" });
            mActions.Add(new ActTableElement() { Description = "Click Cell:" + SelectCol + " Row:4" });

            // Add unique actions for the selected filter/cell
        }

        private void RestoreOriginalActions()
        {
            mActions.Clear();
            foreach (Act a in mOriginalActions)
            {
                mActions.Add(a);
            }
        }

        private void LoadGridToArray()
        {
            UIAuto.AutomationElement tempElement;
            gridArray = new UIAuto.AutomationElement[rowCount, columnCount];
            tempElement = UIAuto.TreeWalker.ContentViewWalker.GetFirstChild(AEControl);
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    gridArray[i, j] = tempElement;
                    tempElement = UIAuto.TreeWalker.ContentViewWalker.GetNextSibling(tempElement);
                }
            }
        }

        private void LoadColumnNameCombo()
        {
            UIAuto.AutomationElement headerElement;
            mColNames = [];
            int k = 0;
            while (k < columnCount)
            {
                headerElement = gridArray[0, k];
                mColNames.Add(headerElement.Current.Name);
                k++;
            }
        }
    }
}
