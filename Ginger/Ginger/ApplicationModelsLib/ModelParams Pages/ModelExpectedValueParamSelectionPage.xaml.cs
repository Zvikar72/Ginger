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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.UserControls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace GingerWPF.ApplicationModelsLib.ModelParams_Pages
{
    public partial class ModelExpectedValueParamSelectionPage : Page
    {
        public ObservableList<AppModelParameter> mApplicationModelParamsList;
        GenericWindow mGenericWindow = null;
        AppModelParameter SelectedParameter = null;

        public ModelExpectedValueParamSelectionPage(ObservableList<AppModelParameter> ApplicationModelParamsList)
        {
            InitializeComponent();
            mApplicationModelParamsList = ApplicationModelParamsList;
            InitParamSelectionGrid();
        }

        public void InitParamSelectionGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(AppModelParameter.ParamLevel), Header = "Level", WidthWeight = 35, AllowSorting = true },
                new GridColView() { Field = nameof(AppModelParameter.PlaceHolder), Header = "Place Holder", WidthWeight = 100, AllowSorting = true },
                new GridColView() { Field = nameof(AppModelParameter.Description), Header = "Description", WidthWeight = 150, AllowSorting = true },
                new GridColView() { Field = nameof(AppModelParameter.OptionalValuesString), Header = "Optional Values", WidthWeight = 80, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true },
            ]
            };

            xModelParamSelectionGrid.ShowTitle = Visibility.Collapsed;
            xModelParamSelectionGrid.SetAllColumnsDefaultView(view);
            xModelParamSelectionGrid.InitViewItems();
            xModelParamSelectionGrid.grdMain.SelectionMode = DataGridSelectionMode.Single;

            xModelParamSelectionGrid.DataSourceList = mApplicationModelParamsList;
        }

        public AppModelParameter ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button selectBtn = new Button
            {
                Content = "Select"
            };
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: selectBtn, eventName: nameof(ButtonBase.Click), handler: selectBtn_Click);

            ObservableList<Button> winButtons = [selectBtn];

            xModelParamSelectionGrid.ShowToolsBar = Visibility.Collapsed;
            xModelParamSelectionGrid.Grid.IsReadOnly = true;
            WeakEventManager<Control, MouseButtonEventArgs>.AddHandler(source: xModelParamSelectionGrid, eventName: nameof(Control.MouseDoubleClick), handler: selectBtn_Click);


            GenericWindow.LoadGenericWindow(ref mGenericWindow, null, windowStyle, "Expected Value Parameter Selection", this, winButtons, true, "Cancel", CloseWinClicked);
            return SelectedParameter;
        }

        private void selectBtn_Click(object sender, RoutedEventArgs e)
        {
            SelectedParameter = (AppModelParameter)xModelParamSelectionGrid.Grid.SelectedItem;

            if (mGenericWindow != null)
            {
                mGenericWindow.Close();
            }
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            SelectedParameter = null;
            mGenericWindow.Close();
        }
    }
}
