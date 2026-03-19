//   Copyright 2026 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace CIMViewer.Helpers
{
  internal static class LayoutHelper
  {
    /// <summary>
    /// Return the CIMLayout definition as an xml string
    /// </summary>
    /// <param name="layout"></param>
    /// <returns>The CIMLayout definition as an xml string</returns>
    public static Task<string> GetDefinitionInternalAsync(this Layout layout)
    {
      return QueuedTask.Run(() => {
        return layout.GetDefinition().ToXml();
      });
    }

    /// <summary>
    /// Set the definition of the layout to the new "xmlDefinition"
    /// </summary>
    /// <remarks>Will throw if the xmlDefinition is invalid</remarks>
    /// <param name="layout"></param>
    /// <param name="xmlDefinition"></param>
    /// <returns></returns>
    public static Task SetDefinitionInternalAsync(this Layout layout, string xmlDefinition)
    {
      CIMUtilities.ValidateXML(xmlDefinition);
      return QueuedTask.Run(() => {
        var cimLayout = CIMUtilities.DeserializeXmlDefinition<CIMLayout>(xmlDefinition);
        layout.SetDefinition(cimLayout);
      });
    }

    /// <summary>
    /// Validate the xmlDefinition
    /// </summary>
    /// <remarks>If layout is null, the xml is not validated</remarks>
    /// <param name="layout"></param>
    /// <param name="xmlDefinition"></param>
    public static void ValidateDefinition(this Layout layout, string xmlDefinition)
    {
      if (layout != null) {
        CIMUtilities.ValidateXML(xmlDefinition);
      }
    }
  }
}
