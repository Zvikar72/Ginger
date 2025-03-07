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
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Amdocs.Ginger.Repository
{
    public class XMLTemplateParser : APIConfigurationsDocumentParserBase
    {
        private Dictionary<string, int> AllPlaceHolders = new Dictionary<string, int>();

        public override ObservableList<ApplicationAPIModel> ParseDocument(string FileName, ObservableList<ApplicationAPIModel> AAMSList, bool avoidDuplicatesNodes = false)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(FileName);
            ObservableList<ApplicationAPIModel> parameters = GetParameters(doc, AAMSList, avoidDuplicatesNodes);
            return parameters;
        }

        public ObservableList<ApplicationAPIModel> ParseDocumentWithXMLContent(string fileContent, ObservableList<ApplicationAPIModel> AAMSList, bool avoidDuplicatesNodes = false)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fileContent);
            return GetParameters(doc, AAMSList, avoidDuplicatesNodes);
        }

        private ObservableList<ApplicationAPIModel> GetParameters(XmlDocument doc, ObservableList<ApplicationAPIModel> AAMSList, bool avoidDuplicatesNodes)
        {
            ApplicationAPIModel AAM = new ApplicationAPIModel
            {
                Name = Path.GetFileNameWithoutExtension(doc.BaseURI)
            };
            XMLDocExtended XDE = new XMLDocExtended(doc);

            if (avoidDuplicatesNodes)
            {
                XDE.RemoveDuplicatesNodes();
            }

            IEnumerable<XMLDocExtended> NodeList = XDE.GetEndingNodes(false);
            ObservableList<AppModelParameter> AMPList = GetParamList(NodeList);

            AAM.RequestBody = XDE.XMLString;
            AAM.AppModelParameters = AMPList;
            AAMSList.Add(AAM);
            AllPlaceHolders.Clear();
            return AAMSList;
        }

        private ObservableList<AppModelParameter> GetParamList(IEnumerable<XMLDocExtended> NodeList)
        {
            ObservableList<AppModelParameter> AMPList = new ObservableList<AppModelParameter>();
            foreach (XMLDocExtended XDN in NodeList)
            {

                string UniqPlaceHolder = "{" + GetPlaceHolderName(XDN.LocalName.ToUpper()) + "}";
                AMPList.Add(new AppModelParameter(UniqPlaceHolder, string.Empty, XDN.LocalName, XDN.XPath, new ObservableList<OptionalValue> { new OptionalValue { Value = XDN.Value, IsDefault = true } }));
                XDN.Value = UniqPlaceHolder;

                if (XDN.Attributes != null && XDN.Attributes.Count > 0)
                {
                    foreach (XmlAttribute XmlAttribute in XDN.Attributes)
                    {
                        if (!string.IsNullOrEmpty(XmlAttribute.Prefix))
                        {
                            continue;
                        }

                        string UniqAttributePlaceHolder = "{" + GetPlaceHolderName(XmlAttribute.LocalName.ToUpper()) + "}";
                        AMPList.Add(new AppModelParameter(UniqAttributePlaceHolder, string.Empty, XmlAttribute.LocalName, XDN.XPath, new ObservableList<OptionalValue> { new OptionalValue { Value = XmlAttribute.Value, IsDefault = true } }));
                        XmlAttribute.Value = UniqAttributePlaceHolder;
                    }
                }
            }
            return AMPList;
        }

        public static ObservableList<ActReturnValue> ParseXMLResponseSampleIntoReturnValues(string fileContent)
        {
            ObservableList<ActReturnValue> ReturnValues = new ObservableList<ActReturnValue>();
            if (!string.IsNullOrEmpty(fileContent))
            {
                XmlDocument docResponseBody = new XmlDocument();
                docResponseBody.LoadXml(fileContent);
                XMLDocExtended XDEResponseBody = new XMLDocExtended(docResponseBody);
                IEnumerable<XMLDocExtended> NodeListResponseBody = XDEResponseBody.GetEndingNodes(false);

                foreach (XMLDocExtended XDN in NodeListResponseBody)
                {
                    ReturnValues.Add(new ActReturnValue() { Param = XDN.LocalName, Path = XDN.XPathWithoutNamspaces, Active = true, DoNotConsiderAsTemp = true });
                }
            }

            return ReturnValues;
        }

        private void GetAllOptionalValuesFromExamples(string placeHolderName, XmlNode xmlNode, ref ObservableList<OptionalValue> optionalValuesList)
        {
            foreach (XmlNode Node in xmlNode.ChildNodes)
            {
                if (placeHolderName.Contains(Node.Name))//TODO:add check if Index is the same
                {
                    optionalValuesList.Add(new OptionalValue() { Value = Node.InnerText });
                }
                GetAllOptionalValuesFromExamples(placeHolderName, Node, ref optionalValuesList);
            }
        }

        public string GetPlaceHolderName(string ElementName)
        {
            string PlaceHolderName = ElementName.ToUpper();
            if (AllPlaceHolders.ContainsKey(PlaceHolderName))
            {
                AllPlaceHolders[PlaceHolderName]++;
                return ElementName.ToUpper() + AllPlaceHolders[PlaceHolderName];
            }
            else
            {
                AllPlaceHolders.Add(ElementName.ToUpper(), 0);
                return ElementName.ToUpper();
            }


        }
    }
}
