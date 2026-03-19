using ArcGIS.Core.CIM;
using ArcGIS.Core.Internal.CIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewSymbol.Helpers
{
  internal static class CIMUtilities
  {
    public static string ToXml(this CIMObject cimObject)
    {
      return XmlUtil.ToXml(cimObject);
    }

    public static T FromXml<T>(string xml) where T : CIMObject, new()
    {
      return XmlUtil.FromXml<T>(xml);
    }
  }

}
