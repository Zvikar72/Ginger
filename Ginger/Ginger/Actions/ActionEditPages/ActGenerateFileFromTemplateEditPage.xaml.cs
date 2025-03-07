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
using GingerCore.Actions;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for XLSReadDataToVariablesPage.xaml
    /// </summary>
    public partial class ActGenerateFileFromTemplateEditPage : Page
    {

        ActGenerateFileFromTemplate mAct { get; set; }

        private string TemplatesFilesPath;
        private string OutputFilesPath;
        public ActGenerateFileFromTemplateEditPage(Act act)
        {
            InitializeComponent();
            TemplatesFilesPath = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"Documents\Templates\");
            OutputFilesPath = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"Documents\OutputFiles\");
            this.mAct = (ActGenerateFileFromTemplate)act;
            GingerCore.General.FillComboFromEnumObj(FileActionComboBox, mAct.FileAction);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(FileActionComboBox, ComboBox.TextProperty, mAct, ActGenerateFileFromTemplate.Fields.FileAction);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DataFileNameTextBox, TextBox.TextProperty, mAct, ActGenerateFileFromTemplate.Fields.DataFileName);
            TemplateFileNamComboBox.SelectedValue = System.IO.Path.GetFileName(mAct.TemplateFileName) != null ? System.IO.Path.GetFileName(mAct.TemplateFileName) : null;
            OutputFileNameTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActGenerateFileFromTemplate.Fields.OutputFileName));

            FillTemplateFileNamComboBox();
            if (!Directory.Exists(OutputFilesPath))
            {
                Directory.CreateDirectory(OutputFilesPath);
            }
        }

        private void FillTemplateFileNamComboBox()
        {
            TemplateFileNamComboBox.Items.Clear();
            if (!Directory.Exists(TemplatesFilesPath))
            {
                Directory.CreateDirectory(TemplatesFilesPath);
            }

            string[] fileEntries = Directory.EnumerateFiles(TemplatesFilesPath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".txt") || s.EndsWith(".tpl") || s.EndsWith(".temp")).ToArray();
            foreach (string file in fileEntries)
            {
                string s = file.Replace(TemplatesFilesPath, "");
                TemplateFileNamComboBox.Items.Add(s);
            }
        }

        private void BrowseDataFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                DefaultExt = "*.xlsx",
                Filter = "Excel Files (*.xlsx)|*.xlsx"
            }) is string fileName)
            {
                DataFileNameTextBox.Text = fileName;
            }
        }

        private void OutputFileNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            mAct.OutputFileName = OutputFilesPath + OutputFileNameTextBox.ValueTextBox.Text;
        }

        private void TemplateFileNamComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mAct.TemplateFileName = TemplatesFilesPath + TemplateFileNamComboBox.SelectedValue;
        }
    }
}