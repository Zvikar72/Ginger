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
using Amdocs.Ginger.Common;
using Ginger.UserControls;
using GingerCore;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Activities
{
    /// <summary>
    /// Interaction logic for ConfigureErrorListPage.xaml
    /// </summary>
    public partial class ConfigureErrorListPage : Page
    {
        GenericWindow _pageGenericWin = null;
        ErrorHandler mErrorHandler;
        public ConfigureErrorListPage(ErrorHandler errorHandler)
        {
            InitializeComponent();
            mErrorHandler = errorHandler;
            SetGridsView();
        }
        private void SetGridsView()
        {

            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(ErrorDetails.IsSelected),StyleType= GridColView.eGridColStyleType.CheckBox, MaxWidth = 20, Header = " " });
            defView.GridColsView.Add(new GridColView() { Field =nameof(ErrorDetails.ErrorString), WidthWeight = 15, Header = "Error String" });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ErrorDetails.ErrorDescription), WidthWeight = 15, Header = "Description" });


            xErrorListConfigurationGrd.SetAllColumnsDefaultView(defView);
            xErrorListConfigurationGrd.InitViewItems();
            xErrorListConfigurationGrd.btnMarkAll.Visibility = Visibility.Visible;

            xErrorListConfigurationGrd.MarkUnMarkAllActive += XErrorListConfigurationGrd_MarkUnMarkAllActive; ;
            xErrorListConfigurationGrd.btnAdd.Click += BtnAdd_Click;
            
            xErrorListConfigurationGrd.DataSourceList = mErrorHandler.ErrorStringList;
        }

        private void XErrorListConfigurationGrd_MarkUnMarkAllActive(bool Status)
        {
            if (mErrorHandler.ErrorStringList.Count > 0)
            {
                foreach (ErrorDetails elem in mErrorHandler.ErrorStringList)
                {
                    elem.IsSelected = Status;
                }
            }
        }


        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            mErrorHandler.ErrorStringList.Add(new ErrorDetails() { ErrorString=string.Empty,ErrorDescription = string.Empty,IsSelected=true });
            xErrorListConfigurationGrd.DataSourceList = mErrorHandler.ErrorStringList;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            //Button okBtn = new Button();
            //okBtn.Content = "Ok";
            //okBtn.Click += new RoutedEventHandler(OkBtn_Click);

            Button closeBtn = new Button();
            closeBtn.Content = "Close";
            closeBtn.Click += new RoutedEventHandler(CloseBtn_Click);

            ObservableList<Button> winButtons = new ObservableList<Button>();

            winButtons.Add(closeBtn); //winButtons.Add(okBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle,"Error String Configuration", this, winButtons, false, string.Empty, CloseWinClicked);
        }

        private void CloseWinClicked(object sender, RoutedEventArgs e)
        {
            _pageGenericWin.Close();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            _pageGenericWin.Close();
        }

        //private void OkBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    _pageGenericWin.Close();
        //}
    }
}