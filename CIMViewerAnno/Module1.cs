using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using System.Xml;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Mapping;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CIMViewerAnno.Events;

namespace CIMViewerAnno
{
  internal class Module1 : Module
  {
    private static Module1 _this = null;
    private CIMTextGraphic _selectedGraphic = null;
    private AnnotationLayer _annoLayer = null;
    private static int count = 1;
    private static string _lasterror = "";

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CIMViewerAnno_Module"));
      }
    }

    #region Overrides

    protected override bool Initialize()
    {
      ArcGIS.Desktop.Mapping.Events.MapSelectionChangedEvent.Subscribe(args => {
        if (args.Selection?.Count > 0) {
          var match = args.Selection.Where(kvp => {
            var anno = kvp.Key as AnnotationLayer;
            return anno != null;
          }).FirstOrDefault();
          if (match.Key != null) {
            GetFirstSelectedAnnoFeatureAsync(match.Key as AnnotationLayer);
          }
          else {
            //clear the selection
            this.SetSelectedGraphic(null, null);
          }
        }
        else {
          //clear the selection
          this.SetSelectedGraphic(null, null);
        }
      });
      if (MapView.Active != null) {
        var mv = MapView.Active;
        if (mv.Map.SelectionCount > 0) {
          QueuedTask.Run(() => {
            var match = mv.Map.GetSelection().Where(kvp => {
              var anno = kvp.Key as AnnotationLayer;
              return anno != null;
            }).FirstOrDefault();
            if (match.Key != null) {
              GetFirstSelectedAnnoFeatureAsync(match.Key as AnnotationLayer);
            }
          });
        }
      }
      return true;
    }

    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    #endregion Overrides

    private Task GetFirstSelectedAnnoFeatureAsync(AnnotationLayer annoLayer)
    {
      return QueuedTask.Run(() => {
        var annoSelect = annoLayer.GetSelection();
        var rowcursor = annoLayer.GetSelection().Search(null);
        if (rowcursor.MoveNext()) {
          var af = rowcursor.Current as AnnotationFeature;
          var graphic = af.GetGraphic() as CIMTextGraphic;
          Module1.Current.SetSelectedGraphic(annoLayer, graphic);
        }
      });
    }

    public void SetSelectedGraphic(AnnotationLayer annoLayer, CIMTextGraphic graphic)
    {
      _annoLayer = annoLayer;
      _selectedGraphic = graphic;
      SelectedGraphicChangedEvent.Publish(new SelectedGraphicChangedEventArgs(_annoLayer, _selectedGraphic));
    }

    public CIMTextGraphic SelectedGraphic => _selectedGraphic;

    public AnnotationLayer AnnotationLayer => _annoLayer;

    public CIMTextGraphic GraphicFromXml(string xmlGraphic)
    {
      //Will throw if not called on a QueuedTask!
      CIMTextGraphic textGraphic = new CIMTextGraphic();
      StringReader sr = new StringReader(xmlGraphic);
      if (!string.IsNullOrEmpty(xmlGraphic)) {
        using (XmlReader reader = new XmlTextReader(sr)) {
          textGraphic.ReadXml(reader);
        }
      }
      return textGraphic;
    }

    public string LastError => _lasterror;

    public Task<bool> ChangeAnnotationTextGraphicAsync(string xmlGraphic)
    {
      _lasterror = "";//reset
      if (string.IsNullOrEmpty(xmlGraphic))
        return Task.FromResult(false);
      if (this.AnnotationLayer == null)
        return Task.FromResult(false);

      return QueuedTask.Run(() =>
      {
        if (!((AnnotationFeatureClass) AnnotationLayer.GetFeatureClass()).GetDefinition().AreSymbolOverridesAllowed())
        {
          _lasterror = $"Overrides are not allowed on '{AnnotationLayer.GetFeatureClass().GetName()}'";
          return false;//overrides are not allowed
        }
          
        EditOperation op = new EditOperation();
        op.Name = $"Change annotation graphic [{count++}]";
        op.SelectModifiedFeatures = true;

        var oid = this.AnnotationLayer.GetSelection().GetObjectIDs().First();

        //At 2.1 we must use an edit operation Callback...
        op.Callback(context => {
          QueryFilter qf = new QueryFilter() {
            WhereClause = $"OBJECTID = {oid}"
          };
          //Cursor must be non-recycling. Use the table ~not~ the layer..i.e. "GetTable().Search()"
          //annoLayer is ~your~ Annotation layer
          var rowCursor = this.AnnotationLayer.GetTable().Search(qf, false);
          rowCursor.MoveNext();
          var annoFeature = rowCursor.Current as ArcGIS.Core.Data.Mapping.AnnotationFeature;
          
          // update the graphic
          CIMTextGraphic textGraphic = GraphicFromXml(xmlGraphic);
          annoFeature.SetGraphic(textGraphic);
          // store is required
          annoFeature.Store();
          //refresh layer cache
          context.Invalidate(annoFeature);

        }, this.AnnotationLayer.GetTable());
        var ok = op.Execute();
        if (!ok)
          _lasterror = op.ErrorMessage;
        return ok;
      });
    }

  }
}
