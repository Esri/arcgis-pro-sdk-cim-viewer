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
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Layouts;

namespace CIMViewer.Helpers {

    public enum CIMServiceType {
        MapMember,
        Map,
        Layout,
        LayoutElement
    };

    /// <summary>
    /// Base class for interacting with the underlying CIM definitions
    /// </summary>
    public abstract class CIMService {

        protected CIMServiceType _serviceType;
        public abstract Task<string> GetDefinitionAsync();

        public abstract Task SetDefinitionAsync(string xmlDefinition);

        public abstract void ValidateDefinition(string xmlDefinition);

        public abstract string URI { get; }

        public CIMServiceType ServiceType => _serviceType;
    }

    /// <summary>
    /// Provides read/write access to the MapMember's CIM definition
    /// </summary>
    public class MapMemberService : CIMService {

        private MapMember _mm;

        public MapMemberService(MapMember mm) {
            _mm = mm;
            _serviceType = CIMServiceType.MapMember;
        }

        public MapMember MapMember => _mm;

        public override string URI => _mm.URI;
        

        /// <summary>
        /// Get the underlying CIM Definition from the map member
        /// </summary>
        /// <returns></returns>
        public override Task<string> GetDefinitionAsync() {
            return MapMember.GetDefinitionAsync();
        }

        /// <summary>
        /// Set the underlying MapMember's CIMDefinition to the new "xmlDefinition"
        /// </summary>
        /// <param name="xmlDefinition"></param>
        /// <returns></returns>
        public override Task SetDefinitionAsync(string xmlDefinition) {
            return MapMember.SetDefinitionAsync(xmlDefinition); //Can throw
        }

        /// <summary>
        /// Validate the MapMember xmlDefinition
        /// </summary>
        /// <param name="xmlDefinition"></param>
        /// <remarks>This will throw a System.Xml.XmlException if the xml is invalid</remarks>
        /// <exception cref="System.Xml.XmlException">The Xml is Invalid or Incorrectly Formatted</exception>
        public override void ValidateDefinition(string xmlDefinition) {
            MapMember.ValidateDefinition(xmlDefinition); //Can throw
        }
    }

    /// <summary>
    /// Provides read/write access to the Map's CIM definition (CIMMap)
    /// </summary>
    public class MapService : CIMService {

        private Map _map;

        public MapService(Map map) {
            _map = map;
            _serviceType = CIMServiceType.Map;
        }

        public Map Map => _map;

        public override string URI => _map.URI;

        public override Task<string> GetDefinitionAsync() {
            return this.Map.GetDefinitionInternalAsync();
        }

        public override Task SetDefinitionAsync(string xmlDefinition) {
            return this.Map.SetDefinitionInternalAsync(xmlDefinition);
        }

        public override void ValidateDefinition( string xmlDefinition) {
            this.Map.ValidateDefinition(xmlDefinition);//Can throw
        }
    }

  /// <summary>
  /// Provides read/write access to the Map's CIM definition (CIMMap)
  /// </summary>
  public class LayoutService : CIMService
  {

    private Layout _layout;

    public LayoutService(Layout layout)
    {
      _layout = layout;
      _serviceType = CIMServiceType.Layout;
    }

    public Layout Layout => _layout;

    public override string URI => _layout.URI;

    public override Task<string> GetDefinitionAsync()
    {
      return this.Layout.GetDefinitionInternalAsync();
    }

    public override Task SetDefinitionAsync(string xmlDefinition)
    {
      return this.Layout.SetDefinitionInternalAsync(xmlDefinition);
    }

    public override void ValidateDefinition(string xmlDefinition)
    {
      this.Layout.ValidateDefinition(xmlDefinition);//Can throw
    }
  }

  /// <summary>
  /// Provides read/write access to the MapMember's CIM definition
  /// </summary>
  public class LayoutElementService : CIMService
  {

    private Element _elem;

    public LayoutElementService(Element element)
    {
      _elem = element;
      _serviceType = CIMServiceType.LayoutElement;
    }

    public LayoutElementService(IElement element)
    {
      _elem = element as Element;
      _serviceType = CIMServiceType.LayoutElement;
    }

    /// <summary>
    /// Get the layout element
    /// </summary>
    public Element Element => _elem;

    /// <summary>
    /// Gets the element URI (for layout elements return its name)
    /// </summary>
    public override string URI => _elem.Name;

    /// <summary>
    /// Get the underlying CIM Definition from the layout element
    /// </summary>
    /// <returns></returns>
    public override Task<string> GetDefinitionAsync()
    {
      return this.Element.GetDefinitionAsync();
    }

    /// <summary>
    /// Set the underlying element's CIMDefinition to the new "xmlDefinition"
    /// </summary>
    /// <param name="xmlDefinition"></param>
    /// <returns></returns>
    public override Task SetDefinitionAsync(string xmlDefinition)
    {
      return Element.SetDefinitionAsync(xmlDefinition); //Can throw
    }

    /// <summary>
    /// Validate the layout element's xmlDefinition
    /// </summary>
    /// <param name="xmlDefinition"></param>
    /// <remarks>This will throw a System.Xml.XmlException if the xml is invalid</remarks>
    /// <exception cref="System.Xml.XmlException">The Xml is Invalid or Incorrectly Formatted</exception>
    public override void ValidateDefinition(string xmlDefinition)
    {
      Element.ValidateDefinition(xmlDefinition); //Can throw
    }
  }
}