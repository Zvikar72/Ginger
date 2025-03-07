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


namespace Ginger.Plugin.Platform.Web.Elements
{
    /// <summary>
    /// Exposes the Functionality for Canvas elements
    /// </summary>
    public interface ICanvas : IGingerWebElement
    {


        /// <summary>
        /// Draws an object on Canvas. 
        /// </summary>
        void DrawObject();

        /// <summary>
        /// Clicks on a relative position from Top Left Corner of the Canvas
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void ClickXY(int x, int y);

    }
}
