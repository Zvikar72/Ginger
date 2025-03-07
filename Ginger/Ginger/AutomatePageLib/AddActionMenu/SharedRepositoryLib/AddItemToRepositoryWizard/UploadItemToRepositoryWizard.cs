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
using Amdocs.Ginger.Repository;
using Ginger.Repository.ItemToRepositoryWizard;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Variables;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using static Ginger.Repository.ItemToRepositoryWizard.UploadItemSelection;

namespace Ginger.Repository.AddItemToRepositoryWizard
{
    public class UploadItemToRepositoryWizard : WizardBase
    {
        public override string Title { get { return "Add Items to Shared Repository"; } }
        bool isConvert = false;

        public UploadItemToRepositoryWizard(IEnumerable<UploadItemSelection> items)
        {
            UploadItemSelection.mSelectedItems.Clear();
            foreach (UploadItemSelection item in items)
            {
                mSelectedItems.Add(item);
            }
            InitializeWizardPages();
        }
        /// <summary>
        /// Constructor For Uploading List of Repository items
        /// </summary>
        /// <param name="context"></param>
        /// <param name="items"></param>
        public UploadItemToRepositoryWizard(Context context, IEnumerable<object> items)
        {
            UploadItemSelection.mSelectedItems.Clear();
            foreach (object i in items)
            {
                UploadItemSelection.mSelectedItems.Add(CreateUploadItem((RepositoryItemBase)i, context));
            }
            InitializeWizardPages();
        }
        /// <summary>
        /// Constructor For Uploading Single Repository Item
        /// <param name="context"></param>
        /// <param name="item"></param>
        public UploadItemToRepositoryWizard(Context context, RepositoryItemBase item, bool IsConvert = false, eActivityInstanceType ConvertType = eActivityInstanceType.LinkInstance)
        {
            UploadItemSelection.mSelectedItems.Clear();
            isConvert = IsConvert;
            UploadItemSelection.mSelectedItems.Add(CreateUploadItem(item, context, IsConvert, ConvertType));
            InitializeWizardPages();
        }

        private void InitializeWizardPages()
        {
            AddPage(Name: "Items Selection", Title: "Item/s Selection", SubTitle: "Selected items to be added to Shared Repository", Page: new UploadItemsSelectionPage(UploadItemSelection.mSelectedItems, isConvert));
            AddPage(Name: "Items Validation", Title: "Item/s Validation", SubTitle: "Validate the items to be added to Shared Repository", Page: new UploadItemsValidationPage());
            AddPage(Name: "Items Status", Title: "Item/s Status", SubTitle: "Upload Item Status", Page: new UploadStatusPage());
            DisableBackBtnOnLastPage = true;
        }

        public static UploadItemSelection CreateUploadItem(RepositoryItemBase item, Context context, bool IsConvert = false, eActivityInstanceType ConvertType = eActivityInstanceType.LinkInstance)
        {
            string strComment = "";
            UploadItemSelection uploadItem = new UploadItemSelection
            {
                Context = context,
                Selected = true
            };
            UploadItemSelection.eExistingItemType existingItemType = UploadItemSelection.eExistingItemType.NA;
            RepositoryItemBase existingItem = ExistingItemCheck(item, ref strComment, ref existingItemType);
            if (existingItem != null)
            {
                uploadItem.ItemUploadType = UploadItemSelection.eItemUploadType.Overwrite;
                uploadItem.ExistingItem = existingItem;
                uploadItem.ExistingItemType = existingItemType;
                uploadItem.Comment = strComment;
                if (IsConvert)
                { uploadItem.ReplaceType = ConvertType; }
                else if (item.IsLinkedItem) { uploadItem.ReplaceType = UploadItemSelection.eActivityInstanceType.LinkInstance; }
                else { uploadItem.ReplaceType = UploadItemSelection.eActivityInstanceType.RegularInstance; }

            }
            else
            {
                uploadItem.ItemUploadType = UploadItemSelection.eItemUploadType.New;
                uploadItem.ReplaceType = UploadItemSelection.eActivityInstanceType.LinkInstance;
            }
            if (item is Activity activity)
            {
                if (activity.ActivitiesGroupID != null && activity.ActivitiesGroupID != string.Empty)
                {
                    ActivitiesGroup? group = context.BusinessFlow.ActivitiesGroups.FirstOrDefault(x => string.Equals(x.Name, activity.ActivitiesGroupID));
                    if (group != null)
                    {
                        ObservableList<ActivitiesGroup> repoGroups = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
                        ActivitiesGroup? repoGroup = repoGroups.FirstOrDefault(x => (x.Guid == group.Guid) || (x.Guid == group.ParentGuid) || (group.ExternalID != null &&
                        group.ExternalID != string.Empty && string.Equals(x.ExternalID, group.ExternalID)));
                        if (repoGroup == null)
                        {
                            uploadItem.Comment = "It is recommended to also add parent activity group: " + group.ItemName + " to repository";
                        }
                    }
                }
            }
            else
            {
                uploadItem.ReplaceType = UploadItemSelection.eActivityInstanceType.RegularInstance;
            }
            uploadItem.ItemName = item.ItemName;
            uploadItem.ItemGUID = item.Guid;
            uploadItem.SetItemPartesFromEnum(GetTypeOfItemParts(item));
            uploadItem.UsageItem = item;

            return uploadItem;
        }

        public static Type GetTypeOfItemParts(RepositoryItemBase item)
        {
            if (item is Activity)
            {
                return typeof(eItemParts);
            }
            else if (item is Act)
            {
                return typeof(Act.eItemParts);
            }
            else if (item is ActivitiesGroup)
            {
                return typeof(ActivitiesGroup.eItemParts);
            }
            else if (item is VariableBase)
            {
                return typeof(VariableBase.eItemParts);
            }
            else
            {
                return null;
            }

        }

        public static RepositoryItemBase ExistingItemCheck(object item, ref string strComment, ref UploadItemSelection.eExistingItemType existingItemType)
        {
            IEnumerable<object> existingRepoItems = new ObservableList<RepositoryItem>();
            bool existingItemIsExternalID = false;
            bool existingItemIsParent = false;
            string existingItemFileName = string.Empty;

            ObservableList<ActivitiesGroup> activitiesGroup = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            if (item is ActivitiesGroup)
            {
                existingRepoItems = activitiesGroup;
            }
            else if (item is Activity)
            {
                existingRepoItems = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            }
            else if (item is Act)
            {
                existingRepoItems = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
            }
            else if (item is VariableBase)
            {
                existingRepoItems = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();
            }

            RepositoryItemBase exsitingItem = SharedRepositoryOperations.GetMatchingRepoItem((RepositoryItemBase)item, existingRepoItems, ref existingItemIsExternalID, ref existingItemIsParent);

            if (exsitingItem != null)
            {
                existingItemFileName = exsitingItem.FileName;

                if (existingItemIsExternalID)
                {
                    strComment = "Item with Same External Id Exist. Back up of existing item will be saved in PrevVersion folder.Change the item upload type if you want to upload it as new item";
                    existingItemType = UploadItemSelection.eExistingItemType.ExistingItemIsExternalID;
                }
                else if (existingItemIsParent)
                {
                    strComment = "Parent item exist in repository. Back up of existing item will be saved in PrevVersion folder.Change the item upload type if you want to upload it as new item";
                    existingItemType = UploadItemSelection.eExistingItemType.ExistingItemIsParent;
                }
                else
                {
                    strComment = "Item already exist in repository. Back up of existing item will be saved in PrevVersion folder.Change the item upload type if you want to upload it as new item";
                    existingItemType = UploadItemSelection.eExistingItemType.ExistingItemIsDuplicate;
                }
            }
            return exsitingItem;
        }

        public override void Finish()
        {
            mWizardWindow.Close();
        }
    }
}
