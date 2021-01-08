using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace PreviewSymbol.Ribbon
{
	internal class SelectFeatureTool : MapTool
	{

		private long _featureOID = -1;
		private string _name = "";
		private CIMSymbol _symbol = null;
		private string _msg = "";

		private double _bufferDist = 0;
		private CIMSymbolReference _polyOutline = null;

		public SelectFeatureTool()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Point;
			SketchOutputMode = SketchOutputMode.Map;
			this.CompleteSketchOnMouseUp = true;
		}

		protected override Task OnToolActivateAsync(bool active)
		{
			if (MapView.Active.Map.IsScene)
			{
				MessageBox.Show("PreviewSymbol is for 2D maps only", "Warning");
				return Task.FromResult(0);
			}
			var center = MapView.Active.Extent.Center;
			return QueuedTask.Run(() =>
			{
				if (_polyOutline == null)
				{
					_polyOutline = SymbolFactory.Instance.ConstructPolygonSymbol(
													 null, SymbolFactory.Instance.ConstructStroke(
														 ColorFactory.Instance.BlueRGB, 1.5)).MakeSymbolReference();
				}
				var clientPt = MapView.Active.MapToClient(MapView.Active.Extent.Center);
				clientPt.X += SelectionEnvironment.SelectionTolerance;
				var mapPt = MapView.Active.ClientToMap(clientPt);
				_bufferDist = Math.Abs(center.X - mapPt.X);
			});
		}

		protected async override Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			Reset();
			if (MapView.Active.Map.IsScene)
				return false;

			var mv = MapView.Active;
			Geometry sel_geom = null;
			IDisposable sel_graphic = null;
			//Flash the selection geometry so we can see where we clicked
			QueuedTask.Run(() =>
			{
				sel_geom = GeometryEngine.Instance.Buffer(geometry, _bufferDist) as Polygon;
				sel_graphic = mv.AddOverlay(sel_geom, _polyOutline);
			});

			

			await QueuedTask.Run(() => {

				var result = MapView.Active.SelectFeatures(
					sel_geom, SelectionCombinationMethod.New, false, false);
				var result2 = MapView.Active.SelectElements(
					sel_geom, SelectionCombinationMethod.New, false);

				_msg = "No basic feature layers or graphics layers selected";

				//first anno layer (if there is one)
				var annoLayer = result.Keys.OfType<AnnotationLayer>().FirstOrDefault();

				//get the first feature layer that supports symbol lookup
				var symbolLayer = result.Keys.FirstOrDefault(layer =>
				{
					if (layer is FeatureLayer featureLayer)
					{
						//Test whether symbol lookup is supported with CanLookupSymbol()
						return featureLayer.CanLookupSymbol();
					}
					return false;
				}) as FeatureLayer;

				sel_graphic?.Dispose();

				//Graphics Layer takes precedence...
				if (result2.Count > 0)
				{
					var elem = result2.First() as GraphicElement;
					_symbol = elem.GetGraphic().Symbol.Symbol;
					_name = ((GraphicsLayer)elem.GetParent()).Name;
					_featureOID = -1;
					_msg = $"Graphic element {elem.Name}";
				}
				//Then Annotation...
				else if (annoLayer != null)
				{
					_name = annoLayer.Name;
					_featureOID = result[annoLayer][0];
					_msg = $"anno feature {_featureOID}";
					var qf = new QueryFilter()
					{
						ObjectIDs = new List<long> { _featureOID }
					};

					var rowcursor = annoLayer.GetSelection().Search(qf);
					if (rowcursor.MoveNext())
					{
						var af = rowcursor.Current as AnnotationFeature;
						var graphic = af.GetGraphic() as CIMTextGraphic;
						_symbol = graphic.Symbol.Symbol;
					}
					rowcursor.Dispose();
				}
				//Then anything else...
				else if (symbolLayer != null)
				{
					_name = symbolLayer.Name;
					_featureOID = result[symbolLayer][0];
					_msg = $"feature {_featureOID}";
					_symbol = symbolLayer.LookupSymbol(_featureOID, MapView.Active);
				}
			});

			Module1.Current.SetSelectedSymbol(_symbol, _name, _featureOID, _msg);
			return true;
		}

		private void Reset()
		{
			_featureOID = -1;
			_name = "";
			_symbol = null;
			_msg = "";
		}
	}
}
