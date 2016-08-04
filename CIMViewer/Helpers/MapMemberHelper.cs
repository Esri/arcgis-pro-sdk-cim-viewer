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

namespace CIMViewer.Helpers {
    internal static class MapMemberHelper {

        #region API
        public static Task<string> GetDefinitionAsync(this MapMember mm) {
            return QueuedTask.Run(() => {
                if (mm == null)
                    return "";
                if (mm is Layer)
                    return ((Layer) mm).GetDefinition().ToXml();
                return ((StandaloneTable) mm).GetDefinition().ToXml();
            });
        }

        public static Task SetDefinitionAsync(this MapMember mm, string xmlDefinition) {
            CIMUtilities.ValidateXML(xmlDefinition);//This can throw!
            return QueuedTask.Run(() => {
                if (mm is Layer) {
                    var baseLayer = CIMUtilities.DeserializeXmlDefinition<CIMBaseLayer>(xmlDefinition);
                    ((Layer) mm).SetDefinition(baseLayer);
                }
                else {
                    var table = CIMUtilities.DeserializeXmlDefinition<CIMStandaloneTable>(xmlDefinition);
                    ((StandaloneTable)mm).SetDefinition(table);
                }
                
            });
        }

        public static void ValidateDefinition(this MapMember mm, string xmlDefinition) {
            if (mm != null) {
                CIMUtilities.ValidateXML(xmlDefinition);
            }
        }

        #endregion API
        
    }
}
