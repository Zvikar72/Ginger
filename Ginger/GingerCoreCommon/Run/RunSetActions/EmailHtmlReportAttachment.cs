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

using Amdocs.Ginger.Repository;

namespace Ginger.Run.RunSetActions
{
    public class EmailHtmlReportAttachment : EmailAttachment
    {
        [IsSerializedForLocalRepository]
        public int SelectedHTMLReportTemplateID { get; set; }

        bool mIsLinkEnabled;
        [IsSerializedForLocalRepository]
        public bool IsLinkEnabled
        {
            get
            {
                return mIsLinkEnabled;
            }
            set
            {
                mIsLinkEnabled = value;
                OnPropertyChanged(nameof(IsLinkEnabled));
            }
        }

        bool mIsAccountReportLinkEnabled;
        [IsSerializedForLocalRepository]
        public bool IsAccountReportLinkEnabled
        {
            get
            {
                return mIsAccountReportLinkEnabled;
            }
            set
            {
                mIsAccountReportLinkEnabled = value;
                OnPropertyChanged(nameof(IsAccountReportLinkEnabled));
            }
        }

        bool mIsZipFolderAttachementEnabled;
        [IsSerializedForLocalRepository]
        public bool IsZipFolderAttachementEnabled
        {
            get
            {
                return mIsZipFolderAttachementEnabled;
            }
            set
            {
                mIsZipFolderAttachementEnabled = value;
                OnPropertyChanged(nameof(IsZipFolderAttachementEnabled));
            }
        }

        [IsSerializedForLocalRepository]
        public bool IsAlternameFolderUsed { get; set; }
        public override string GetNameForFileName() { return Name; }
        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
    }
}
