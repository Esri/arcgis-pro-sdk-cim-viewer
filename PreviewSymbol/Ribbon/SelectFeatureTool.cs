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
using ArcGIS.Desktop.Mapping;

namespace PreviewSymbol.Ribbon
{
	internal class SelectFeatureTool : MapTool
	{

		private long _featureOID = -1;
		private string _name = "";
		private CIMSymbol _symbol = null;
		private string _msg = "";

		public SelectFeatureTool()
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
			Reset();
			await QueuedTask.Run(() => {
				var result = MapView.Active.SelectFeatures(geometry);

				_msg = "No basic feature layers selected";

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

				//Annotation takes precedence...
				if (annoLayer != null)
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
