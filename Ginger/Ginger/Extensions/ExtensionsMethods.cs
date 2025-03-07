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
using Amdocs.Ginger.UserControls;
using Amdocs.Ginger.ValidationRules;
using Ginger.Actions;
using Ginger.Agents;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ginger
{
    //TODO: Rename to Controls Extension Methods
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };

        // all Controls of type UIElement
        public static void Refresh(this UIElement uiElement)
        {
            try
            {
                uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Refresh uiElement failed - " + uiElement.GetType().Name, ex);
            }

        }

        // ------------------------------------------------------------
        // Combo Box
        // ------------------------------------------------------------

        // Bind ComboBox to enum type field, list of valid values are all enum of the field selected
        public static void BindControl(this ComboBox ComboBox, Object obj, string Field)
        {
            // Get the current value so we can make it selected
            PropertyInfo PI = obj.GetType().GetProperty(Field);
            object CurrentFieldEnumValue = PI.GetValue(obj);
            if (CurrentFieldEnumValue == null || CurrentFieldEnumValue.GetType() == typeof(string))
            {
                // if it's string like in Excel Sheet name combo then do binding to the text
                // or we can ask for function call to return list of values - TODO: later if we need Val/text
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, ComboBox.TextProperty, obj, Field, BindingMode.TwoWay);
            }
            else
            {
                GingerCore.General.FillComboFromEnumObj(ComboBox, CurrentFieldEnumValue);
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, obj, Field, BindingMode.TwoWay);
            }
        }

        // Bind Combo for enum type, but provide the subset list of enums/valid values to show
        public static void BindControl(this ComboBox ComboBox, Object obj, string Field, dynamic enumslist, bool isSorted = true)
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, obj, Field, BindingMode.TwoWay);
            List<object> l = [.. enumslist];

            // Get yhe current value so it will be sleected in the combo after the list created
            PropertyInfo PI = obj.GetType().GetProperty(Field);
            object CurrentFieldEnumValue = PI.GetValue(obj);

            GingerCore.General.FillComboFromEnumObj(ComboBox, CurrentFieldEnumValue, l, isSorted);
        }

        // Bind Combo for enum type, but provide the subset list of enums/valid values to show
        // also using grouping on results, according to 
        public static void BindControlWithGrouping(this ComboBox ComboBox, Object obj, string Field, dynamic enumslist)
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, obj, Field, BindingMode.TwoWay);
            List<ComboGroupedEnumItem> l = [];
            foreach (var v in enumslist)
            {
                ComboGroupedEnumItem item = new ComboGroupedEnumItem
                {
                    text = GingerCore.General.GetEnumValueDescription(v.GetType(), v),
                    Category = GingerCore.General.GetEnumDescription(v.GetType(), v),
                    Value = v
                };

                l.Add(item);
            }

            // Get yhe current value so it will be sleected in the combo after the list created
            PropertyInfo PI = obj.GetType().GetProperty(Field);
            object CurrentFieldEnumValue = PI.GetValue(obj);

            ListCollectionView lcv = new ListCollectionView(l);
            lcv.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            lcv.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));

            GingerCore.General.FillComboFromEnumObj(ComboBox, CurrentFieldEnumValue, null, true, lcv);
        }

        /// <summary>
        /// Bind the combo box to ObservableList 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ComboBox"></param>
        /// <param name="obj">Object to bind to</param>
        /// <param name="Field">Object field to bind</param>
        /// <param name="list">List of Observable items to display in the combo box</param>
        /// <param name="DisplayMemberPath">list item field to display</param>
        /// <param name="SelectedValuePath">list item value to return when selected</param>
        public static void BindControl<T>(this ComboBox ComboBox, Object obj, string Field, ObservableList<T> list, string DisplayMemberPath, string SelectedValuePath, BindingMode bindingMode = BindingMode.TwoWay)
        {
            ComboBox.ItemsSource = list;
            ComboBox.DisplayMemberPath = DisplayMemberPath;
            ComboBox.SelectedValuePath = SelectedValuePath;

            //ControlsBinding.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, obj, Field, bindingMode);   
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, obj, Field, bindingMode);
        }

        // ------------------------------------------------------------
        // Validation rules
        // ------------------------------------------------------------

        public enum eValidationRule
        {
            CannotBeEmpty,
            FileExist
        }

        // ------------------------------------------------------------
        // Combo Box
        // ------------------------------------------------------------
        public static void AddValidationRule(this ComboBox comboBox, ValidationRule validationRule)
        {
            BindingExpression bd = null;
            //Check if Selected value is binded
            bd = comboBox.GetBindingExpression(ComboBox.SelectedValueProperty);
            if (bd != null)
            {
                AddValidation(comboBox, ComboBox.SelectedValueProperty, validationRule);
                return;
            }

            //Check if text is binded
            bd = comboBox.GetBindingExpression(ComboBox.TextProperty);
            if (bd != null)
            {
                AddValidation(comboBox, ComboBox.TextProperty, validationRule);
                return;
            }

            throw new Exception("trying to add rule to control which is not binded - " + comboBox.Name);
        }

        public static void AddValidationRule(this ComboBox comboBox, eValidationRule validationRule)
        {
            if (validationRule == eValidationRule.CannotBeEmpty)
            {
                comboBox.AddValidationRule(new EmptyValidationRule());
            }
        }


        // ------------------------------------------------------------
        // Text Box
        // ------------------------------------------------------------
        public static void BindControl(this ComboBox ComboBox, dynamic enumslist)
        {
            List<object> l = [.. enumslist];
            // Get yhe current value so it will be sleected in the combo after the list created
            GingerCore.General.FillComboFromEnumObj(ComboBox, l[0], l);
        }
        public static void BindControl(this TextBox TextBox, Object obj, string Field, BindingMode bm = BindingMode.TwoWay)
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TextBox, TextBox.TextProperty, obj, Field, bm);
        }

        public static void BindControl(this TreeView TreeView, Object obj, string Field, BindingMode bm = BindingMode.TwoWay)
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TreeView, TreeView.SelectedValuePathProperty, obj, Field, bm);
        }

        public static void BindControl(this TextBox TextBox, ActInputValue AIV)
        {
            TextBox.BindControl(AIV, nameof(ActInputValue.Value));
        }


        public static void AddValidationRule(this TextBox textBox, ValidationRule validationRule)
        {
            AddValidation(textBox, TextBox.TextProperty, validationRule);
        }

        public static void AddValidationRule(this TreeView TreeView, ValidationRule validationRule)
        {
            AddValidation(TreeView, TreeView.SelectedValuePathProperty, validationRule);
        }

        public static void AddValidationRule(this TextBox textBox, eValidationRule validationRule)
        {
            if (validationRule == eValidationRule.CannotBeEmpty)
            {
                AddValidation(textBox, TextBox.TextProperty, new EmptyValidationRule());
            }
        }

        // ------------------------------------------------------------
        // ucAgentControl
        // ------------------------------------------------------------
        public static void AddValidationRule(this ucAgentControl agentControl, ValidationRule validationRule)
        {
            BindingExpression bd = null;

            bd = agentControl.GetBindingExpression(ucAgentControl.SelectedAgentProperty);
            if (bd != null)
            {
                AddValidation(agentControl, ucAgentControl.SelectedAgentProperty, validationRule);
                return;
            }

            throw new Exception("trying to add rule to AgentControl user control which is not binded - " + agentControl.Name);
        }

        // ------------------------------------------------------------
        // ucAgentControl
        // ------------------------------------------------------------

        public static void AddValidationRule(this Frame frame, ValidationRule validationRule)
        {
            AddValidation(frame, Frame.ContentProperty, validationRule);
        }

        // ------------------------------------------------------------
        // UCValue Expression
        // ------------------------------------------------------------

        public static void BindControl(this UCValueExpression UCValueExpression, Context context, Object obj, string Field)
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(UCValueExpression, TextBox.TextProperty, obj, "Value");
            UCValueExpression.Init(context, obj, Field);
        }

        // ------------------------------------------------------------
        // check box
        // ------------------------------------------------------------
        public static void BindControl(this CheckBox checkBox, Object obj, string field)
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(checkBox, CheckBox.IsCheckedProperty, obj, field);

        }


        // ------------------------------------------------------------
        // Label
        // ------------------------------------------------------------

        public static void BindControl(this Label Label, Object obj, string Field)
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(Label, Label.ContentProperty, obj, Field, BindingMode.OneWay);
        }



        // ------------------------------------------------------------
        //Image Maker
        // ------------------------------------------------------------
        public static void BindControl(this ImageMakerControl Label, Object obj, string Field)
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(Label, ImageMakerControl.ImageTypeProperty, obj, Field, BindingMode.OneWay);
        }



        // ------------------------------------------------------------
        // Frame
        // ------------------------------------------------------------
        public static void SetContent(this Frame Frame, UIElement uiElemnt)
        {
            // Clear history first
            if (!Frame.CanGoBack && !Frame.CanGoForward)
            {
                // do nothing    
            }
            else
            {
                // clear frame history
                var entry = Frame.RemoveBackEntry();
                while (entry != null)
                {
                    entry = Frame.RemoveBackEntry();
                }
            }

            // Set the frame content
            Frame.Content = uiElemnt;
        }

        // ------------------------------------------------------------
        // ucGrid
        // ------------------------------------------------------------

        //public static void AddValidationRule(this ucGrid ucgrid , ValidationRule validationRule)
        //{
        //    AddValidation(ucgrid, ucGrid.RowsCountProperty, validationRule);
        //}

        //public static void AddValidationRule(this ucGrid ucgrid, eValidationRule validationRule)
        //{
        //    if (validationRule == eValidationRule.CannotBeEmpty)
        //    {
        //        AddValidation(ucgrid, ucGrid.RowsCountProperty, new GridValidationRule());
        //    }
        //    //TODO: throw...
        //}

        // ------------------------------------------------------------
        // Validations
        // ------------------------------------------------------------

        public static void RemoveValidations(this FrameworkElement frameworkElement, DependencyProperty SelectedProperty)
        {
            BindingExpression bd = frameworkElement.GetBindingExpression(SelectedProperty);

            if (bd != null)
            {
                bd.ParentBinding.ValidationRules.Clear();
            }
        }

        private static void AddValidation(this FrameworkElement frameworkElement, DependencyProperty dependencyProperty, ValidationRule validationRule)
        {
            BindingExpression bd = frameworkElement.GetBindingExpression(dependencyProperty);
            bd.ParentBinding.ValidationRules.Add(validationRule);

            if (bd.ParentBinding.ValidationRules.Count > 1)
            {
                // no need to recreate the controlTemplate
                return;
            }

            bd.ParentBinding.NotifyOnValidationError = true;


            // This Xaml is being created in code
            //
            //  <TextBox x:Name="AgentNameTextBox"  TextWrapping="Wrap" Text="" Style="{StaticResource @TextBoxStyle}" FontWeight="Bold" Margin="10">
            //    <Validation.ErrorTemplate>
            //        <ControlTemplate>
            //            <StackPanel>
            //                <!--Placeholder for the TextBox itself-->
            //                <AdornedElementPlaceholder x:Name = "textBox" />
            //                <TextBlock Text = "{Binding [0].ErrorContent}" Foreground = "Red" />
            //            </StackPanel>
            //        </ControlTemplate>
            //    </Validation.ErrorTemplate>
            //  </TextBox >

            // Generate the above xaml programmatically

            ControlTemplate controlTemplate = new ControlTemplate();
            var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
            var stackPanelPlaceHolder = new FrameworkElementFactory(typeof(AdornedElementPlaceholder));
            stackPanel.AppendChild(stackPanelPlaceHolder);
            var textBlock = new FrameworkElementFactory(typeof(TextBlock));

            //Bind the textblock to validation error
            Binding b2 = new Binding
            {
                Source = frameworkElement,
                //b2.Path = new PropertyPath("(Validation.Errors)[0].ErrorContent"); // OK

                // '/' = CurrentItem which is better than [0] as the list might be empty
                Path = new PropertyPath("(Validation.Errors)/ErrorContent"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            textBlock.SetBinding(TextBlock.TextProperty, b2);

            textBlock.SetValue(TextBlock.ForegroundProperty, Brushes.Red);
            stackPanel.AppendChild(textBlock);
            controlTemplate.VisualTree = stackPanel;

            // attach the control template to the text box
            Validation.SetErrorTemplate(frameworkElement, controlTemplate);
        }

        public static void ClearValidations(this FrameworkElement frameworkElement, DependencyProperty dependencyProperty)
        {
            //try
            //{
            BindingExpression bd = frameworkElement.GetBindingExpression(dependencyProperty);
            bd.ParentBinding.ValidationRules.Clear();
            //}
            //catch (Exception ex)
            //{
            //    Reporter.ToLog(eAppReporterLogLevel.WARN, "Failed to clear control validations", ex, true, true)
            // }
        }

        public static ValidationRule GetValidationRule(this FrameworkElement frameworkElement, DependencyProperty dependencyProperty, Type type)
        {
            BindingExpression bd = frameworkElement.GetBindingExpression(dependencyProperty);
            foreach (ValidationRule vr in bd.ParentBinding.ValidationRules)
            {
                if (vr.GetType() == type)
                {
                    return vr;
                }
            }
            return null;
        }

        public static void ClearControlsBindings(this DependencyObject dependencyObject)
        {
            foreach (DependencyObject element in dependencyObject.EnumerateVisualDescendents())
            {
                BindingOperations.ClearAllBindings(element);
            }
        }
        public static IEnumerable<DependencyObject> EnumerateVisualDescendents(this DependencyObject dependencyObject)
        {
            yield return dependencyObject;

            foreach (DependencyObject child in dependencyObject.EnumerateVisualChildren())
            {
                foreach (DependencyObject descendent in child.EnumerateVisualDescendents())
                {
                    yield return descendent;
                }
            }
        }
        public static IEnumerable<DependencyObject> EnumerateVisualChildren(this DependencyObject dependencyObject)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                yield return VisualTreeHelper.GetChild(dependencyObject, i);
            }
        }
    }
}