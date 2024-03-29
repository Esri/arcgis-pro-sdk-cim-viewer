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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace CIMViewer.Helpers {
    internal static class MapHelper {

        /// <summary>
        /// Return the CIMMap definition as an xml string
        /// </summary>
        /// <param name="map"></param>
        /// <returns>The CIMMap definition as an xml string</returns>
        public static Task<string> GetDefinitionInternalAsync(this Map map) {
            return QueuedTask.Run(() => {
                return map.GetDefinition().ToXml();
            });
        }

        /// <summary>
        /// Set the definition of the map to the new "xmlDefinition"
        /// </summary>
        /// <remarks>Will throw if the xmlDefinition is invalid</remarks>
        /// <param name="map"></param>
        /// <param name="xmlDefinition"></param>
        /// <returns></returns>
        public static Task SetDefinitionInternalAsync(this Map map, string xmlDefinition) {
            CIMUtilities.ValidateXML(xmlDefinition);
            return QueuedTask.Run(() => {
                var cimMap = CIMUtilities.DeserializeXmlDefinition<CIMMap>(xmlDefinition);
                map.SetDefinition(cimMap);
            });
        }

        /// <summary>
        /// Validate the xmlDefinition
        /// </summary>
        /// <remarks>If map is null, the xml is not validated</remarks>
        /// <param name="map"></param>
        /// <param name="xmlDefinition"></param>
        public static void ValidateDefinition(this Map map, string xmlDefinition) {
            if (map != null) {
                CIMUtilities.ValidateXML(xmlDefinition);
            }
        }

    }
}
