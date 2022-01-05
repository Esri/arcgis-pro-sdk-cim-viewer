using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace CIMViewerAnno.Ribbon
{
  internal class SelectAnno : MapTool
  {
    public SelectAnno()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle;
      SketchOutputMode = SketchOutputMode.Map;
      this.CompleteSketchOnMouseUp = true;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

    protected async override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      await QueuedTask.Run(() => {
        var select = MapView.Active.SelectFeatures(geometry);
        var annoLayer = select.Keys.FirstOrDefault(l => l is AnnotationLayer) as AnnotationLayer;
        if (annoLayer == null)
          return;

        //get the first in the selection only
        var rowcursor = annoLayer.GetSelection().Search(null);
        if (rowcursor.MoveNext()) {
          var af = rowcursor.Current as AnnotationFeature;
          var graphic = af.GetGraphic() as CIMTextGraphic;
          Module1.Current.SetSelectedGraphic(annoLayer, graphic);
        }
        rowcursor.Dispose();
      });
      return true;
    }
  
  }
}
