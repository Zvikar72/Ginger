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

using Amdocs.Ginger.Plugin.Core;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Xml;

namespace Ginger.UserControlsLib.TextEditor.Common
{
    public class PlugInTextEditorWrapper : TextEditorBase, ITextEditor
    {
        ITextEditor mPlugInTextFileEditor;

        public PlugInTextEditorWrapper(ITextEditor PugInTextFileEditor)
        {
            mPlugInTextFileEditor = PugInTextFileEditor;
        }

        public string GetEditorID()
        {
            return "mPlugInTextFileEditor.EditorID";   //!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        public override List<string> Extensions
        {
            get { return mPlugInTextFileEditor.Extensions; }
        }

        public override string Descritpion { get { return null; } }

        public override Image Icon { get { return null; } }

        public override IHighlightingDefinition HighlightingDefinition
        {
            get
            {
                byte[] xml = mPlugInTextFileEditor.HighlightingDefinition;
                MemoryStream ms = new MemoryStream(xml);
                using (XmlReader reader = XmlReader.Create(ms))
                {
                    IHighlightingDefinition HighlightingDefinition = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    return HighlightingDefinition;
                }
            }
        }

        byte[] ITextEditor.HighlightingDefinition => throw new System.NotImplementedException();

        Amdocs.Ginger.Plugin.Core.IFoldingStrategy ITextEditor.FoldingStrategy => throw new System.NotImplementedException();


        public override List<ITextEditorToolBarItem> Tools
        {
            get
            {
                return mPlugInTextFileEditor.Tools;
            }
        }


        public override IFoldingStrategy FoldingStrategy
        {
            get
            {
                // return new PlugInFoldingStrategy(mPlugInTextFileEditor.FoldingStrategy );
                return null;
            }
        }

        public string Name => throw new System.NotImplementedException();

        public ITextHandler TextHandler { get { return mPlugInTextFileEditor.TextHandler; } set { mPlugInTextFileEditor.TextHandler = value; } }

        //public override List<ICompletionData> GetCompletionData(string txt, SelectedContentArgs SelectedContentArgs)
        //{
        //    List<ICompletionData> list = new List<ICompletionData>();

        //    string CurrentLine = SelectedContentArgs.CaretLineText().TrimStart();
        //    bool IsAtStartOfLine = CurrentLine.Length == 1;
        //    List<string> KeyWords = mPlugInTextFileEditor.CompletionDataKeyWords;

        //    if (SelectedContentArgs.IsAtStartOfLine() || IsAtStartOfLine)
        //    {
        //        foreach (string KeyWord in KeyWords)
        //        {
        //            if (txt.ToUpper() == KeyWord.Substring(0,1).ToUpper())
        //                list.Add(new PlugInTextCompletionData(KeyWord,mPlugInTextFileEditor.CompletionDataKeyWords));
        //        }
        //    }
        //    return list;
        //}

        //public override Page GetSelectedContentPageEditor(SelectedContentArgs SelectedContentArgs)
        //{
        //    ReadOnlyCollection<FoldingSection> list = SelectedContentArgs.GetFoldingsAtCaretPosition();
        //    if (list == null) return null;
        //    if (list.Count > 0)
        //    {
        //        string txt = list[0].TextContent;
        //        string StartKeyWord = mPlugInTextFileEditor.TableEditorPageDict["StartKeyWord"];
        //        string EndKeyWord = mPlugInTextFileEditor.TableEditorPageDict["EndKeyWord"];
        //        string KeyWordForTableLocationIndication = mPlugInTextFileEditor.TableEditorPageDict["KeyWordForTableLocationIndication"];
        //        int CaretPosition = SelectedContentArgs.CaretPosition - SelectedContentArgs.GetFoldingOffSet();

        //        int StartKeyWordPosition = txt.IndexOf(StartKeyWord);
        //        int EndKeyWordPosition = txt.IndexOf(EndKeyWord);
        //        int KeyWordForTableLocationIndicationPosition = txt.IndexOf(KeyWordForTableLocationIndication);

        //        string TextAfterKeyWordForTableLocationIndication = string.Empty;
        //        if (KeyWordForTableLocationIndicationPosition != -1)
        //            TextAfterKeyWordForTableLocationIndication = txt.Substring(KeyWordForTableLocationIndicationPosition);

        //        int AfterKeyWordStartKeyWordPosition = KeyWordForTableLocationIndicationPosition + TextAfterKeyWordForTableLocationIndication.IndexOf(StartKeyWord);
        //        int AfterKeyWordEndKeyWordPosition = KeyWordForTableLocationIndicationPosition + TextAfterKeyWordForTableLocationIndication.IndexOf(EndKeyWord);

        //        if ((CaretPosition > StartKeyWordPosition && CaretPosition < EndKeyWordPosition)|| (CaretPosition > AfterKeyWordStartKeyWordPosition && CaretPosition < AfterKeyWordEndKeyWordPosition))
        //        {
        //            TableEditorPage p = new TableEditorPage(SelectedContentArgs, StartKeyWord, EndKeyWord, KeyWordForTableLocationIndication);
        //            return p;
        //        }
        //    }
        //    return null;
        //}

        public override void UpdateSelectedContent()
        {
        }

        public override Page GetSelectedContentPageEditor(SelectedContentArgs SelectedContentArgs)
        {
            // throw new System.NotImplementedException();
            return null;
        }

        public override List<ICompletionData> GetCompletionData(string txt, SelectedContentArgs SelectedContentArgs)
        {
            // throw new System.NotImplementedException();
            return null;
        }

        // public override string Title()
        // {
        //     return mPlugInTextFileEditor.Title();
        // }

        //public override List<TextEditorToolBarItem> Tools { get { return mPlugInTextFileEditor.Tools; } }
    }
}
