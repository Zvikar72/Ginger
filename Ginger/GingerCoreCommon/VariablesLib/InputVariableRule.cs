﻿using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.Variables
{
    public class InputVariableRule : RepositoryItemBase
    {    

        public enum eInputVariableOperation
        {
            [EnumValueDescription("Set Visibility")]
            SetVisibility,
            [EnumValueDescription("Set Value")]
            SetValue,
            [EnumValueDescription("Set Optional Values")]
            SetOptionalValues
        }

        public enum eInputVariableOperator
        {
            [EnumValueDescription("=")]
            Equals,
            [EnumValueDescription("<>")]
            NotEquals,
            [EnumValueDescription(">")]
            GreaterThan,
            [EnumValueDescription(">=")]
            GreaterThanEquals,
            [EnumValueDescription("<")]
            LessThan,
            [EnumValueDescription("<=")]
            LessThanEquals,
            [EnumValueDescription("Contains")]
            Contains,
            [EnumValueDescription("Does Not Contains")]
            DoesNotContains,
            Evaluate           
        }

        public enum eVisibilityOptions
        {
            Show,
            Hide
        }

        ObservableList<VariableBase> mSourceVariableList = new ObservableList<VariableBase>();
        public ObservableList<VariableBase> SourceVariableList
        {
            get
            {
                return mSourceVariableList;
            }
            set
            {
                mSourceVariableList = value;
                OnPropertyChanged(nameof(SourceVariableList));
            }
        }
        
        public ObservableList<VariableBase> TargetVariableList
        {
            get
            {
                if(SourceVariableGuid!=null)
                {
                    return new ObservableList<VariableBase>(SourceVariableList.Where(x => x.Guid != SourceVariableGuid));
                }  
                else
                {
                    return null;
                }    
            }           
        }

        ObservableList<string> mVariableValuesList = new ObservableList<string>();
        public ObservableList<string> variableValueList
        {
            get
            {
                return mVariableValuesList;
            }
            set
            {
                mVariableValuesList = value;
                OnPropertyChanged(nameof(variableValueList));
            }
        }
        
        public VariableBase SelectedSourceVariable
        {
            get
            {
                return SourceVariableList.Where(x=> x.Guid == SourceVariableGuid).FirstOrDefault();
            }           
        }

        private VariableBase mSelectedTargetVariable;
        public VariableBase SelectedTargetVariable
        {
            get
            {
                return TargetVariableList.Where(x => x.Guid== TargetVariableGuid).FirstOrDefault();                
            }
            set
            {
                mSelectedTargetVariable = value;
            }
        }


        private bool mActive;
        [IsSerializedForLocalRepository]
        public bool Active 
        { 
            get
            {
                return mActive;
            }
            set
            {
                if (value != mActive)
                {
                    mActive = value;
                    OnPropertyChanged(nameof(Active));
                }
            }
        }

        private Guid mSourceVariableGuid;
        [IsSerializedForLocalRepository]
        public Guid SourceVariableGuid 
        {
            get
            {
                return mSourceVariableGuid;
            }

            set
            {
                if(value!= mSourceVariableGuid)
                {
                    mSourceVariableGuid = value;
                    OnPropertyChanged(nameof(SelectedSourceVariable));
                    OnPropertyChanged(nameof(TargetVariableList));
                    OnPropertyChanged(nameof(SourceVariableGuid));
                }                            
            }
        }

        private string mTriggerValue;
        [IsSerializedForLocalRepository]
        public string TriggerValue 
        { 
            get
            {
                return mTriggerValue;
            }
            set
            {
                if(value != mTriggerValue)
                {
                    mTriggerValue = value;
                    OnPropertyChanged(nameof(TriggerValue));
                }
            }
        }

        private Guid mTargetVariableGuid;
        [IsSerializedForLocalRepository]
        public Guid TargetVariableGuid 
        {
            get
            {
                return mTargetVariableGuid;
            }

            set
            {
                if (value != mTargetVariableGuid)
                {
                    mTargetVariableGuid = value;
                    OnPropertyChanged(nameof(SelectedTargetVariable)); 
                }
            } 
        }

        private eInputVariableOperation mOperationType;
        [IsSerializedForLocalRepository]
        public eInputVariableOperation OperationType 
        { 
            get
            {
                return mOperationType;
            }
            set
            {
                if (value != mOperationType)
                {
                    mOperationType = value;
                    OnPropertyChanged(nameof(OperationType)); 
                }
            }
        }

        private eInputVariableOperator mOperator;
        [IsSerializedForLocalRepository]
        public eInputVariableOperator Operator
        {
            get
            {
                return mOperator;
            }
            set
            {
                if (value != mOperator)
                {
                    mOperator = value;
                    OnPropertyChanged(nameof(Operator)); 
                }
            }
        }

        private string mOperationValue;
        [IsSerializedForLocalRepository]
        public string OperationValue 
        { 
            get
            {
                return mOperationValue;
            }
            set
            {
                if (value != mOperationValue)
                {
                    mOperationValue = value;
                    OnPropertyChanged(nameof(OperationValue)); 
                }
            }
        }

        private ObservableList<OperationValues> mOperationValueList;
        [IsSerializedForLocalRepository]
        public ObservableList<OperationValues> OperationValueList
        {
            get
            {
                return mOperationValueList;
            }
            set
            {
                if (value != mOperationValueList)
                {
                    mOperationValueList = value;
                    OnPropertyChanged(nameof(OperationValueList)); 
                }
            }
        }
      
        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }
    }
}