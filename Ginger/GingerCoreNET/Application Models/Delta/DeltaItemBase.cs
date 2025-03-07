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

using Amdocs.Ginger.Common.Enums;
using System.ComponentModel;

namespace GingerCoreNET.Application_Models
{
    public enum eDeltaStatus
    {
        Unknown,
        Unchanged,
        Changed,
        Deleted,
        Added,
        Avoided,
    }

    public class DeltaItemBase : INotifyPropertyChanged
    {
        private bool mIsSelected = false;
        public bool IsSelected
        {
            get
            {
                return mIsSelected;
            }
            set
            {
                if (mIsSelected != value)
                {
                    mIsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private eDeltaStatus mDeltaStatus;
        public eDeltaStatus DeltaStatus
        {
            get
            {
                return mDeltaStatus;
            }
            set
            {
                mDeltaStatus = value;
                OnPropertyChanged(nameof(DeltaStatus));
                OnPropertyChanged(nameof(IsNotEqual));
                OnPropertyChanged(nameof(DeltaStatusIcon));
            }
        }

        public eImageType DeltaStatusIcon
        {
            get
            {
                return DeltaStatus switch
                {
                    eDeltaStatus.Unknown => eImageType.Unknown,
                    eDeltaStatus.Deleted => eImageType.Deleted,
                    eDeltaStatus.Changed => eImageType.Changed,
                    eDeltaStatus.Added => eImageType.Added,
                    eDeltaStatus.Unchanged => eImageType.Unchanged,
                    eDeltaStatus.Avoided => eImageType.Avoided,
                    _ => eImageType.Unknown,
                };
            }
        }

        public bool IsNotEqual
        {
            get
            {
                if (DeltaStatus == eDeltaStatus.Unchanged)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private string mDeltaExtraDetails;
        public string DeltaExtraDetails
        {
            get
            {
                return mDeltaExtraDetails;
            }
            set
            {
                mDeltaExtraDetails = value;
                OnPropertyChanged(nameof(DeltaExtraDetails));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}
