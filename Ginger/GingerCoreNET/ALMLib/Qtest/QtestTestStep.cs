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


namespace GingerCore.ALM.Qtest
{
    public class QtestTestStep
    {
        public string StepID { get; set; }
        public string StepName { get; set; }
        public string Description { get; set; }
        public string Expected { get; set; }
        public QtestTestParameter Params { get; set; }
        public string CalledTestCaseId { get; set; }

        public QtestTestStep(string stepID, string description, string expected, string calledTestCaseId = null, string stepName = null)
        {
            StepID = stepID;
            Description = description;
            Expected = expected;
            CalledTestCaseId = calledTestCaseId;
            StepName = stepName;
        }

        public QtestTestStep()
        {

        }
    }
}
