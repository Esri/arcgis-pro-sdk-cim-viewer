using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
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

				#region Do the Symbol Lookup
				if (symbolLayer == null)
				{
					_msg = "No feature layers selected";
					return;
				}
				//get the first selected feature's oid
				_name = symbolLayer.Name;
				_featureOID = result[symbolLayer][0];
				#endregion

				_symbol = symbolLayer.LookupSymbol(_featureOID, MapView.Active);

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
