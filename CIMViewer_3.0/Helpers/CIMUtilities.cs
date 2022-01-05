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
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CIMViewer.Helpers {
    internal static class CIMUtilities {

        public static void ValidateXML(string xml) {
            var doc = new XmlDocument();
            doc.LoadXml(xml);//This will throw XmlException
            doc = null;
        }

        public static T DeserializeXmlDefinition<T>(string xmlDefinition) {
            using (var stringReader = new StringReader(xmlDefinition)) {
                using (var reader = new XmlTextReader(stringReader)) {
                    reader.WhitespaceHandling = WhitespaceHandling.Significant;
                    reader.MoveToContent();
                    var typeName = reader.GetAttribute("xsi:type").Replace("typens:", "ArcGIS.Core.CIM.");
                    var cimObject = System.Activator.CreateInstance("ArcGIS.Core", typeName).Unwrap() as IXmlSerializable;
                    cimObject.ReadXml(reader);
                    return (T)cimObject;
                }
            }
        }
    }
}
