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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Actions.Common
{
    public class ActUIElement : Act, IActPluginExecution
    {
        public const string EElementActionTypeGeneric = "Generic";

        public override string ActionDescription { get { return "UIElement Action"; } }
        public override string ActionUserDescription { get { return "UIElement Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("UI Element Action");
        }

        public override string ActionEditPage { get { return "_Common.ActUIElementLib.ActUIElementEditPage"; } }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }

        public override bool ValueConfigsNeeded { get { return false; } }

        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    //mPlatforms.Add(ePlatformType.AndroidDevice);
                    mPlatforms.Add(ePlatformType.Java);
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                    mPlatforms.Add(ePlatformType.Windows);
                    mPlatforms.Add(ePlatformType.Mobile);

                    //TODO: to see the Web impl uncomment
                    // DO NOT remove comment before we have Selenium support this action and converter for old action
                    mPlatforms.Add(ePlatformType.Web);
                }
                return mPlatforms;
            }
        }

        public new static partial class Fields
        {
            public static string ElementType = "ElementType";
            public static string ElementAction = "ElementAction";
            public static string ElementLocateBy = "ElementLocateBy";
            public static string SubElementType = "SubElementType";
            public static string SubElementAction = "SubElementAction";
            public static string ElementLocateValue = "ElementLocateValue";
            public static string ElementLocateByXValue = "ElementLocateByXValue";
            public static string ElementLocateByYValue = "ElementLocateByYValue";
            public static string XCoordinate = "XCoordinate";
            public static string YCoordinate = "YCoordinate";
            public static string ValueToSelect = "ValueToSelect";
            public static string Value = "Value";

            //Used for Drag & Drop Action
            public static string TargetElementType = "TargetElementType";
            public static string TargetLocateBy = "TargetLocateBy";
            public static string TargetLocateValue = "TargetLocateValue";
            public static string SourceDragXY = "SourceDragXY";
            public static string TargetDropXY = "TargetDropXY";

            public static string DragDropType = "DragDropType";

            //used for Java
            public static string WaitforIdle = "WaitforIdle";
            public static string IsWidgetsElement = "IsWidgetsElement";
            public static string IsMouseEvent = "IsMouseEvent";

            //used to support switch action sync time
            public static string SyncTime = "SyncTime";

            //used for TableElementAction
            public static string ControlAction = "ControlAction";
            public static string ControlActionValue = "ControlActionValue";
            public static string RunActionOn = "RunActionOn";
            public static string ColSelectorValue = "ColSelectorValue";
            public static string WhereOperator = "WhereOperator";
            public static string WhereColumnVal = "WhereColumnVal";
            public static string WhereProperty = "WhereProperty";

            public static string LocateColTitle = "LocateColTitle";
            public static string LocateRowType = "LocateRowType";
            public static string LocateRowValue = "LocateRowValue";

            public static string ByRowNum = "ByRowNum";
            public static string ByRandRow = "ByRandRow";
            public static string BySelectedRow = "BySelectedRow";
            public static string ByWhere = "ByWhere";

            public static string WhereColSelector = "WhereColSelector";
            public static string WhereColumnTitle = "WhereColumnTitle";
            public static string WhereColumnValue = "WhereColumnValue";

            //used for MouseClickAndValidate
            public static string ClickType = "ClickType";
            public static string ValidationType = "ValidationType";
            public static string ValidationElement = "ValidationElement";
            public static string ValidationElementLocateBy = "ValidationElementLocateBy";
            public static string ValidationElementLocatorValue = "ValidationElementLocatorValue";
            public static string LoopThroughClicks = "LoopThroughClicks";

            //used for SendKeysAndValidate
            public static string DefineHandleAction = "DefineHandleAction";
            public static string HandleActionType = "HandleActionType";
            public static string HandleElementType = "HandleElementType";
            public static string HandleElementLocateBy = "HandleElementLocateBy";
            public static string HandleElementLocatorValue = "HandleElementLocatorValue";
            public static string ValidationElementValue = "ValidationElementValue";

            //used for SelectandValidate
            public static string SubElementLocateBy = "SubElementLocateBy";
            public static string SubElementLocatorValue = "SubElementLocatorValue";

            //used for ScrollToElement
            public static string VerticalScrollAlignment = "VerticalScrollAlignment";
        }

        // Fields Helper for specific action, will create AIV with param name based on enum

        public enum CheckBoxSetValue
        {
            Checked,
            UnChecked
        }

        public object ElementData = null;

        public enum eLocateRowTypeOptions
        {
            [EnumValueDescription("Row Number")]
            RowNumber,
            [EnumValueDescription("Any Row")]
            AnyRow,
            [EnumValueDescription("By Selected Row")]
            BySelectedRow,
            [EnumValueDescription("Where")]
            Where
        }

        public enum eSubElementType
        {
            [EnumValueDescription("HTML Table")]
            HTMLTable,
            [EnumValueDescription("Pane")]
            Pane,
        }

        public enum eElementProperty
        {
            [EnumValueDescription("Is Enabled")]
            Enabled,
            [EnumValueDescription("Visible")]
            Visible,
            [EnumValueDescription("Is Selected")]
            IsSelected,
            [EnumValueDescription("Color")]
            Color,
            [EnumValueDescription("Text")]
            Text,
            [EnumValueDescription("Tool Tip")]
            ToolTip,
            [EnumValueDescription("Type")]
            Type,
            [EnumValueDescription("Value")]
            Value,
            [EnumValueDescription("Date Time Value")]
            DateTimeValue,
            [EnumValueDescription("HTML")]
            HTML,
            [EnumValueDescription("List")]
            List,
            [EnumValueDescription("Toggle State")]
            ToggleState,
            [EnumValueDescription("Name")]
            Name,
            [EnumValueDescription("Class Name")]
            ClassName,
            [EnumValueDescription("Automation Id")]
            AutomationId,
            [EnumValueDescription("Is Password")]
            IsPassword,
            [EnumValueDescription("Process Id")]
            ProcessId,
            [EnumValueDescription("X Coordinate")]
            XCoordinate,
            [EnumValueDescription("Y Coordinate")]
            YCoordinate
        }

        public enum eElementAction
        {
            #region Generic Action Types
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("")]
            Unknown,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Activate Hover")]
            Hover,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set as Visible")]
            Visible,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Click")]
            Click,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Custom Attribute")]
            GetCustomAttribute,//keeping for backward support
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Async Click")]
            AsyncClick,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Win Click")]             // not here at all ?
            WinClick,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Mouse Click")]
            MouseClick,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Mouse Press/Release")]
            MousePressRelease,                              // JAVA ?
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Double Click")]
            DoubleClick,                                     // JAVA ?
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Java Script Click")]
            JavaScriptClick,                                 // JAVA ?
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Click and Validate")]
            ClickAndValidate,                                // JAVA ?
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Send Keys and Validate")]
            SendKeysAndValidate,                             // JAVA ?
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Click X,Y")]
            ClickXY,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set Text")]
            SetText,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Text")]
            GetText,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set Value")]
            SetValue,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Context Menu Click")]
            ContextMenuClick,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Value")]
            GetValue,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get X,Y")]
            GetXY,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Size")]
            GetSize,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Open Drop Down")]
            OpenDropDown,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Select and Validate")]
            SelectandValidate,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Close Drop Down")]
            CloseDropDown,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get All Values")]
            GetAllValues,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Attribute Value")]
            GetAttrValue,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set Focus")]
            SetFocus,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Is Enabled")]
            IsEnabled,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Toggle")]
            Toggle,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Select")]
            Select,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Is Visible")]
            IsVisible,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Is Mandatory")]
            IsMandatory,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Exist")]
            Exist,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Does not exist")]
            NotExist,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Enabled")]  //Need to check ???
            Enabled,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Name")]
            GetName,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Dialog Text")]
            GetDialogText,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Accept Dialog")]
            AcceptDialog,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Dismiss Dialog")]
            DismissDialog,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set Date")]
            SetDate,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Scroll Up")]
            ScrollUp,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Scroll Down")]
            ScrollDown,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Scroll Left")]
            ScrollLeft,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Scroll Right")]
            ScrollRight,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Select By Index")]
            SelectByIndex,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Value By Index")]
            GetValueByIndex,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Item Count")]
            GetItemCount,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Send Keys")]
            SendKeys,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Send Key Press Release")]
            SendKeyPressRelease,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Wait Until Display")]
            [EnumValueExtDescriptionAttribute("Wait Until Display")]
            WaitUntilDisplay,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Wait")]
            Wait,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Wait Until Disappear")]
            WaitUntilDisappear,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Drag and Drop")]
            DragDrop,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Expand")]
            Expand,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Collapse")]
            Collapse,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Close")]
            CloseWindow,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Is Exist")]
            IsExist,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Maximize")]
            Maximize,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Minimize")]
            Minimize,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Control Property")]
            GetControlProperty,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Initialize Editor Pane")]
            InitializeJEditorPane,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("JEditor Pane Element Action")]
            JEditorPaneElementAction,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Refresh")]
            Refresh,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Contexts")]
            GetContexts,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set Contexts")]
            SetContext,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Select From Dijit List")]
            SelectFromDijitList,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Mouse Right Click")]
            MouseRightClick,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Font")]
            GetFont,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Width")]
            GetWidth,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Height")]
            GetHeight,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Style")]
            GetStyle,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Async Select From Drop Down (By Index)")]
            AsyncSelectFromDropDownByIndex,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Multi Clicks")]
            MultiClicks,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Multi Set Value")]
            MultiSetValue,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Window Title")]
            GetWindowTitle,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set As Disabled")]
            IsDisabled,
            [EnumValueDescription("Switch")]
            Switch,
            [EnumValueDescription("Double Click using XY")]
            DoubleClickXY,
            [EnumValueDescription("Right Click using XY")]
            RightClickXY,
            [EnumValueDescription("Send Keys using XY")]
            SendKeysXY,
            #endregion Generic Action Types

            #region TextBox Action Types
            [EnumValueDescription("Clear Value")]
            ClearValue,
            [EnumValueDescription("Get Text Length")]
            GetTextLength,
            #endregion TextBox Action Types

            #region Table Action Types
            [EnumValueDescription("Table Row Action")]
            TableRowAction,
            [EnumValueDescription("Table Cell Action")]
            TableCellAction,
            [EnumValueDescription("Table Action")]
            TableAction,
            [EnumValueDescription("Click Using Invoke Pattern")]
            InvokeClick,
            [EnumValueDescription("Click Using Legacy Pattern")]
            LegacyClick,
            [EnumValueDescription("Get Row Count")]
            GetRowCount,
            [EnumValueDescription("Get Selected Row")]
            GetSelectedRow,
            [EnumValueDescription("Draw Object")]
            DrawObject,
            [EnumValueDescription("Win Double Click")]
            winDoubleClick,
            [EnumValueDescription("Async Select")]
            AsyncSelect,
            [EnumValueDescription("Scroll to Element")]
            ScrollToElement,
            #endregion Table Action Types

            #region ComboBox related Types
            [EnumValueDescription("Set Selected Value By Index")]
            SetSelectedValueByIndex,
            [EnumValueDescription("Select By Text")]
            SelectByText,
            [EnumValueDescription("Get Valid Values")]
            GetValidValues,
            [EnumValueDescription("Get Selected Value")]
            GetSelectedValue,
            [EnumValueDescription("Is Value Populated")]
            IsValuePopulated,
            #endregion Usable Action Types

            [EnumValueDescription("Submit")]
            Submit,
            [EnumValueDescription("Run Java Script")]
            RunJavaScript,

            //Adding For java driver checkbox element
            [EnumValueDescription("Is Checked")]
            IsChecked,

            [EnumValueDescription("Get selected node child Items")]
            GetSelectedNodeChildItems,

            [EnumValueDescription("Trigger JavaScript Event")]
            TriggerJavaScriptEvent,

            //Below should NOT be used- only kept for old action types support
            #region NOT TO USE Action Types
            [EnumValueDescription("Simple Click")]
            SimpleClick,
            [EnumValueDescription("Click At")]
            ClickAt,
            [EnumValueDescription("Long Click")]  // For Android
            LongClick,
            #endregion NOT TO USE Action Types
        }

        public enum eElementDragDropType
        {
            [EnumValueDescription("Using JavaScript")]
            DragDropJS,
            [EnumValueDescription("Using Web Driver")]
            DragDropSelenium,
            [EnumValueDescription("Mouse Drag Drop")]
            MouseDragDrop,
        }


        public eElementType ElementType
        {
            get { return GetOrCreateInputParam<eElementType>(Fields.ElementType, eElementType.Unknown); }
            set
            {
                GetOrCreateInputParam(Fields.ElementType).Value = value.ToString();

                OnPropertyChanged(nameof(ActUIElement.ElementType));

            }
        }


        public eElementAction ElementAction
        {
            get { return GetOrCreateInputParam<eElementAction>(Fields.ElementAction, eElementAction.Unknown); }
            set
            {
                GetOrCreateInputParam(Fields.ElementAction).Value = value.ToString();

                OnPropertyChanged(nameof(ActUIElement.ElementAction));

            }
        }


        public eLocateBy ElementLocateBy
        {
            get { return GetOrCreateInputParam<eLocateBy>(Fields.ElementLocateBy, eLocateBy.NA); }
            set
            {
                GetOrCreateInputParam(Fields.ElementLocateBy).Value = value.ToString();

                OnPropertyChanged(nameof(ActUIElement.ElementLocateBy));

            }
        }


        public eLocateBy TargetLocateBy
        {
            get { return GetOrCreateInputParam<eLocateBy>(Fields.TargetLocateBy, eLocateBy.NA); }
            set
            {
                GetOrCreateInputParam(Fields.TargetLocateBy).Value = value.ToString();

                OnPropertyChanged(nameof(ActUIElement.TargetLocateBy));

            }
        }

        public eElementType TargetElementType
        {
            get { return GetOrCreateInputParam<eElementType>(Fields.TargetElementType, eElementType.Unknown); }
            set
            {
                GetOrCreateInputParam(Fields.TargetElementType).Value = value.ToString();

                OnPropertyChanged(nameof(ActUIElement.TargetElementType));

            }
        }

        public eElementType HandleElementType
        {
            get { return GetOrCreateInputParam<eElementType>(Fields.HandleElementType, eElementType.Unknown); }
            set
            {
                GetOrCreateInputParam(Fields.HandleElementType).Value = value.ToString();

                OnPropertyChanged(nameof(ActUIElement.HandleElementType));

            }
        }

        public eElementAction HandleActionType
        {
            get { return GetOrCreateInputParam<eElementAction>(Fields.HandleActionType, eElementAction.Unknown); }
            set
            {
                GetOrCreateInputParam(Fields.HandleActionType).Value = value.ToString();

                OnPropertyChanged(nameof(ActUIElement.HandleActionType));

            }
        }
        #region TableElementConfigs
        public enum eTableElementRunColSelectorValue
        {
            [EnumValueDescription("Column Title")]
            ColTitle,
            [EnumValueDescription("Column Name")]
            ColName,
            [EnumValueDescription("Column Number")]
            ColNum
        }

        public enum eTableElementRunColPropertyValue
        {
            [EnumValueDescription("Value")]
            Value,
            [EnumValueDescription("Path")]
            Path,
            [EnumValueDescription("HTML")]
            HTML,
            [EnumValueDescription("Text")]
            Text,
            [EnumValueDescription("ToolTip")]
            ToolTip,
            [EnumValueDescription("Enabled")]
            Enabled,
            [EnumValueDescription("Visible")]
            Visible,
            [EnumValueDescription("Type")]
            Type,
            [EnumValueDescription("List")]
            List,
            [EnumValueDescription("DateTimeValue")]
            DateTimeValue,
            [EnumValueDescription("isSelected")]
            isSelected
        }

        public enum eTableElementRunColOperator
        {
            [EnumValueDescription("Equals")]
            Equals,
            [EnumValueDescription("Not Equals")]
            NotEquals,
            [EnumValueDescription("Contains")]
            Contains,
            [EnumValueDescription("Not Contains")]
            NotContains,
            [EnumValueDescription("Starts With")]
            StartsWith,
            [EnumValueDescription("Not Starts With")]
            NotStartsWith,
            [EnumValueDescription("Ends With")]
            EndsWith,
            [EnumValueDescription("Not Ends With")]
            NotEndsWith
        }

        public enum eTableElementRunActionOn
        {
            [EnumValueDescription("On Cell with Row Num & Column Num")]
            OnCellRowNumColNum,
            [EnumValueDescription("On Cell with Row Num & Column Name")]
            OnCellRowNumColName,
            [EnumValueDescription("On Column By Name")]
            OnColumnByName,
            [EnumValueDescription("On Column By Num")]
            OnColumnByNum,
            [EnumValueDescription("On Row By Num")]
            OnRowByNum,
            [EnumValueDescription("On Column Name where matches Column Name")]
            OnColNameWhereColName

        }
        #endregion TableElementConfigs

        // The simple will be: Value = 'abc';
        // but since it is list can also support: X=100, Y=200
        // or multiple attrs: ID='a123' Text='abc' class='TBC'
        // We keep it seperated from Params, but will process Value for driver for each
        public ObservableList<UIElementPropertyValueLocator> PropertyValueLocatrs = [];
        public enum eWaitForIdle
        {
            [EnumValueDescription("0. None - default and recommended for most actions")]
            None,
            [EnumValueDescription("1. Short - Response expected in less than 1 second (test for idle every 0.1 second, max 30 seconds wait)")]
            Short,
            [EnumValueDescription("2. Medium - Between 1 to 5 seconds (test for idle every 0.5 second , max 60 seconds wait)")]
            Medium,
            [EnumValueDescription("3. Long - Between 5 to 30 seconds (test for idle every 1 second, max 2 Minutes wait)")]
            Long,
            [EnumValueDescription("4. Very Long - more than 30 seconds (test for idle every 5 seconds , max 5 minutes wait)")]
            VeryLong,
        }

        public string ElementLocateValue
        {
            get
            {
                return GetOrCreateInputParam(Fields.ElementLocateValue).Value;
            }
            set
            {
                GetOrCreateInputParam(Fields.ElementLocateValue).Value = value;
                OnPropertyChanged(nameof(ElementLocateValue));
            }
        }

        public enum eTableAction
        {
            [EnumValueDescription("Set Value")]
            SetValue,
            [EnumValueDescription("Get Value")]
            GetValue,
            Toggle,
            Click,
            Type,
            [EnumValueDescription("Win Click")]
            WinClick,
            [EnumValueDescription("Async Click")]
            AsyncClick,
            [EnumValueDescription("Get Row Count")]
            GetRowCount,
            [EnumValueDescription("Get Selected Row")]
            GetSelectedRow,
            [EnumValueDescription("Is Cell Enabled")]
            IsCellEnabled,
            [EnumValueDescription("Is Cell Visible")]
            IsVisible,
            [EnumValueDescription("Double Click")]
            DoubleClick,
            [EnumValueDescription("Set Focus")]
            SetFocus,
            [EnumValueDescription("Set Keys")]
            SendKeys,
            [EnumValueDescription("Is Checked")]
            IsChecked,
            [EnumValueDescription("Set Date")]
            SelectDate,
            [EnumValueDescription("Mouse Press & Release")]
            MousePressAndRelease,
            [EnumValueDescription("Activate Row")]
            ActivateRow,
            [EnumValueDescription("Is Visible")]
            isVisible,
            [EnumValueDescription("Select All Rows")]
            SelectAllRows,
            [EnumValueDescription("Right Click")]
            RightClick,
            [EnumValueDescription("Context Menu Click")]
            ContextMenuClick
        }

        // TODO: move Locate Value to here and remove from Act.cs

        // When recording we will collect some more info on the element to use as hint
        public string LocatorHint { get; set; }  // make it list of loc by loc value - which can find the elem or one string - will be used for auto fix

        public override String ActionType
        {
            get
            {
                return "UI Element Action. " + ElementAction;
            }
        }

        public override eImageType Image
        {
            get
            {
                //TODO: replace with below code after Image type onAct.cs will be shifted to eImageType
                //return ElementInfo.GetElementTypeImage(ElementType);
                return ElementType switch
                {
                    eElementType.Button => eImageType.MousePointer,
                    eElementType.TextBox => eImageType.Edit,
                    eElementType.ComboBox => eImageType.ExpandAll,
                    eElementType.List => eImageType.DropList,
                    eElementType.CheckBox => eImageType.CheckBox,
                    eElementType.Image => eImageType.Image,
                    eElementType.Label => eImageType.Paragraph,
                    eElementType.MenuItem => eImageType.Menu,
                    eElementType.MenuBar => eImageType.Window,
                    eElementType.RadioButton => eImageType.RadioButton,
                    eElementType.TreeView => eImageType.MapSigns,
                    eElementType.Window => eImageType.WindowsIcon,
                    eElementType.Table => eImageType.Table,
                    _ => eImageType.Window,// FIXME
                };
            }
        }

        // ------------------------------------------------------

        // For each special Action/Locator which have more than just value we create function to get the data so driver can use it easily, see sample of By X Y in Selenium
        // Locate By XY

        //TODO: use the AIV list of items
        public void GetLocateByXYValuesForDriver(out double X, out double Y)
        {
            // split the Value, do not create new param
            // all locate value need to be combined into string
            if (string.IsNullOrEmpty(ElementLocateValueForDriver))
            {
                X = 0;
                Y = 0;
            }
            else
            {
                string[] xy = ElementLocateValueForDriver.Split(',');
                if ((xy != null) && (xy.Length > 1))
                {
                    if (!double.TryParse(xy[0].Split('=')[1], out X))
                    {
                        X = 0;
                    }

                    if (!double.TryParse(xy[1].Split('=')[1], out Y))
                    {
                        Y = 0;
                    }
                }
                else
                {
                    X = 0;
                    Y = 0;
                }
            }
        }

        //Pack
        public void GetLocateByXYValues(out double X, out double Y, object locateValueParentObject, string locateValueField)
        {
            string locateValue = locateValueParentObject.GetType().GetProperty(locateValueField).GetValue(locateValueParentObject).ToString();//to support diffrent LocateValue fields we have on Act

            // split the Value, do not create new param
            // all locate value need to be combined into string
            if (string.IsNullOrEmpty(locateValue))
            {
                X = 0;
                Y = 0;
            }
            else
            {
                string[] xy = locateValue.Split(',');
                if ((xy != null) && (xy.Length > 1))
                {
                    if (xy[0].Contains('='))
                    {
                        if (!double.TryParse(xy[0].Split('=')[1], out X))
                        {
                            X = 0;
                        }

                        if (!double.TryParse(xy[1].Split('=')[1], out Y))
                        {
                            Y = 0;
                        }
                    }
                    else
                    {
                        if (!double.TryParse(xy[0], out X))
                        {
                            X = 0;
                        }

                        if (!double.TryParse(xy[1], out Y))
                        {
                            Y = 0;
                        }
                    }
                }
                else
                {
                    X = 0;
                    Y = 0;
                }
            }
        }

        //Parse
        public void SetLocateByXYValues(double X, double Y, object locateValueParentObject, string locateValueField)
        {
            locateValueParentObject.GetType().GetProperty(locateValueField).SetValue(locateValueParentObject, "X=" + X + ",Y=" + Y);//to support diffrent LocateValue fields we have on Act
        }

        //TOOD: impl in ActionEditPage to show the output grid or show no output values
        // return per action if to display output values grid
        //Move to Act.cs base
        public bool GetDisplayOutputValue()
        {

            //TODO: fill for all
            return ElementAction switch
            {
                eElementAction.Click or eElementAction.Collapse or eElementAction.Expand => false,
                eElementAction.GetValue => true,
                _ => false,
            };
        }


        public override ActionDetails Details
        {
            //TODO: create sepeare class ActUIElementDetails - so it will do lazy loading/calc
            // for now below will do, just to show the concept in ActionGrid
            get
            {
                // We create a customized user friendly action details for actions grid and report
                ActionDetails d = base.Details;
                if (this.ElementLocateBy == eLocateBy.ByXY)
                {
                    double X;
                    double Y;
                    this.GetLocateByXYValuesForDriver(out X, out Y);
                    d.Info = "Locate By X,Y: X= " + X + ", Y=" + Y;
                }
                else
                {
                    d.Info = "Locate By: " + ElementLocateBy + " - " + ElementLocateValue;
                }

                if (Value != null)
                {
                    d.Info += " Value: " + Value;
                }

                // return params order by priority and improtence
                d.Params.Clear();
                d.Params.Add(new ActionParamInfo() { Param = "Type", Value = this.ElementType.ToString() });   // get the neum desc
                d.Params.Add(new ActionParamInfo() { Param = "Locate By", Value = this.ElementLocateBy.ToString() });   // get the neum desc
                d.Params.Add(new ActionParamInfo() { Param = "Action", Value = this.ElementAction.ToString() });   // get the neum desc
                d.Params.Add(new ActionParamInfo() { Param = "Val", Value = this.Value });   // get the neum desc

                // TODO: push others
                return d;
            }
        }

        public string GetName()
        {
            return "UIElementAction";
        }



        public string ElementLocateValueForDriver
        {
            get
            {
                return this.GetInputParamCalculatedValue(Fields.ElementLocateValue);
            }
        }
        public string TargetLocateValue
        {
            get
            {
                return this.GetInputParamCalculatedValue(Fields.TargetLocateValue);
            }
        }
        public string TargetLocateValueForDriver
        {
            get
            {
                return this.GetInputParamCalculatedValue(Fields.TargetLocateValue);
            }
        }



        // Dup put in centralized location !!!

        public struct Locator
        {
            public string By;
            public string Value;
        }


        public PlatformAction GetAsPlatformAction()
        {
            PlatformAction platformAction = new PlatformAction(this);




            foreach (ActInputValue aiv in this.InputValues)
            {

                string ValueforDriver = aiv.ValueForDriver;
                if (!platformAction.InputParams.ContainsKey(aiv.Param) && !String.IsNullOrEmpty(ValueforDriver))
                {
                    platformAction.InputParams.Add(aiv.Param, ValueforDriver);
                }
            }


            Dictionary<string, string> Locators = [];
            Locators.Add(ElementLocateBy.ToString(), ElementLocateValueForDriver);


            platformAction.InputParams.Add("Locators", Locators);

            return platformAction;
        }


        public override void PostDeserialization()
        {
            //Row selection options Row Number, Any Row, By Selected Row and Where
            //Earlier these were stored in fields differently RowSelectorRadioParam and LocateRowType
            //Below code is for backward compatibility
            //It will move the correct value to "LocateRowType" and remove the other fields
            string currentValue = this.GetInputParamValue("RowSelectorRadioParam");
            if (!string.IsNullOrEmpty(currentValue))
            {

                switch (currentValue)
                {
                    case "RowNum":
                        currentValue = "Row Number";
                        break;

                    case "AnyRow":
                        currentValue = "Any Row";
                        break;

                    case "BySelectedRow":
                        currentValue = "By Selected Row";
                        break;
                }
                this.AddOrUpdateInputParamValue(ActUIElement.Fields.LocateRowType, currentValue);
                this.RemoveInputParam("RowSelectorRadioParam");
            }
        }



    }
}
