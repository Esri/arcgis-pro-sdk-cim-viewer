//   Copyright 2016 Esri
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Layouts;

namespace CIMViewer.Helpers
{
  internal static class LayoutElementHelper
  {

    #region API
    public static Task<string> GetDefinitionAsync(this Element element)
    {
      return QueuedTask.Run(() => {
        if (element == null)
          return "";
        return element.GetDefinition().ToXml();
      });
    }

    public static Task SetDefinitionAsync(this Element element, string xmlDefinition)
    {
      CIMUtilities.ValidateXML(xmlDefinition);//This can throw!
      return QueuedTask.Run(() => {
        var cimElement = CIMUtilities.DeserializeXmlDefinition<CIMElement>(xmlDefinition);
        element.SetDefinition(cimElement);
      });
    }

    public static void ValidateDefinition(this Element element, string xmlDefinition)
    {
      if (element != null) {
        CIMUtilities.ValidateXML(xmlDefinition);
      }
    }

    #endregion API

  }
}
