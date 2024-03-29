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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Layouts.Events;
using ArcGIS.Desktop.Mapping;
using CIMViewer.Helpers;

namespace CIMViewer
{
	internal class CIMDockpaneViewModel : DockPane
	{
		private const string _dockPaneID = "CIMViewerModule_CIMViewerDockpane";
		private CIMService _cimService = null;
		private string _originalCaption = "";
		private static readonly object _lock = new object();

		protected CIMDockpaneViewModel()
		{
			_originalCaption = this.Caption;
			if (Project.Current != null && FrameworkApplication.Panes.ActivePane != null)
			{
				//map or layout?
				if (FrameworkApplication.Panes.ActivePane is ILayoutPane layoutPane) 
				{
					lock (_lock)
					{
						CIMService = new LayoutService(layoutPane.LayoutView.Layout);
					}
				}
				else if (FrameworkApplication.Panes.ActivePane is IMapPane mapPane)
				{
					SetMapMember(mapPane.MapView);
				}
			}
			Initialize();
		}


		private void Initialize()
		{

			ArcGIS.Desktop.Core.Events.ProjectClosingEvent.Subscribe((args) =>
			{
				CIMService = null;//just do it when the project closes
				return Task.FromResult(0);
			});

			#region Layout

			//ArcGIS.Desktop.Layouts.Events.LayoutChangedEvent.Subscribe((args) => {
			//  if (_cimService != null &&
			//      _cimService.ServiceType == CIMServiceType.LayoutElement) {
			//    if (_cimService.URI == args.Layout.URI) {
			//      CIMService = new LayoutService(args.Layout);
			//    }
			//  }
			//});

			//ArcGIS.Desktop.Core.Events.ProjectItemsChangedEvent.Subscribe((args) =>
			//{
			//	if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
			//	{
			//		//its an add action
			//		if (args.ProjectItem is LayoutProjectItem layoutProjectItem)//it's an add layout
			//		{
			//			var on_qt = QueuedTask.OnWorker;
			//			var layout_name = layoutProjectItem.Name;
			//			//var layout = layoutProjectItem.GetLayout();
			//		}
			//	}
			//	else if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
			//	{
			//		//its a delete
			//	}

			//});

			ArcGIS.Desktop.Layouts.Events.LayoutEvent.Subscribe((args) =>
			{
				if (LayoutView.Active == null)
					return;
				if (CIMViewerModule.IgnoreEvents)
					return;

				switch (args.Hint)
				{
					case LayoutEventHint.MapSeriesPageChanged:
						break;
					case LayoutEventHint.MapSeriesRefreshed:
						break;
					case LayoutEventHint.PageChanged:
						if (_cimService != null &&
						_cimService.ServiceType == CIMServiceType.LayoutElement)
						{
							if (LayoutView.Active != null)
							{
								if (LayoutView.Active.Layout.URI == _cimService.URI)
									lock (this)
										CIMService = new LayoutService(LayoutView.Active.Layout);
							}
						}
						break;
					case LayoutEventHint.PropertyChanged:
						break;
				}
			});

			ArcGIS.Desktop.Layouts.Events.LayoutViewEvent.Subscribe((args) =>
			{
				if (LayoutView.Active == null)
					return;
				if (CIMViewerModule.IgnoreEvents)
					return;

				switch (args.Hint)
				{
					case LayoutViewEventHint.Closed:
						if (_cimService != null && (_cimService.ServiceType == CIMServiceType.Layout ||
																		_cimService.ServiceType == CIMServiceType.LayoutElement))
						{
							//assume we are showing the active layout or an element on it
							if (FrameworkApplication.Panes.Count == 0)
								// || FrameworkApplication.Panes.OfType<ILayoutPane>().Count() == 0
								lock (_lock)
								{
									CIMService = null;
								}
						}
						break;
					//replaces LayoutViewEventType.Initialized
					case LayoutViewEventHint.Opened:
						if (LayoutView.Active != null)
						{
							lock (_lock)
							{
								CIMService = LayoutView.Active.Layout != null ?
																		new LayoutService(LayoutView.Active.Layout) :
																		null;
							}
						}
						break;
					case LayoutViewEventHint.Activated:
					case LayoutViewEventHint.Deactivated:
					case LayoutViewEventHint.Closing:
					case LayoutViewEventHint.ExtentChanged:
					case LayoutViewEventHint.DrawingComplete:
					case LayoutViewEventHint.PauseDrawingChanged:
						break;
				}
			});

			ArcGIS.Desktop.Layouts.Events.ElementEvent.Subscribe((args) =>
			{
				if (LayoutView.Active == null)
					return;
				if (CIMViewerModule.IgnoreEvents)
					return;
				IEnumerable<IElement> elems = null;

				if (args.Elements?.Count() > 0 && args.Hint != ElementEventHint.ElementRemoved)
				{
					elems = args.Elements;
				}
				else if (args.Container.GetSelectedElements()?.Count() > 0)
				{
					elems = args.Container.GetSelectedElements();
				}
				if (elems?.Count() > 0)
				{
					if (_cimService == null ||
						_cimService.ServiceType == CIMServiceType.Layout)
					{
						lock (_lock)
							CIMService = new LayoutElementService(elems.First() as Element);
					}
					else
					{
						SetLayoutElement(elems);
					}
				}
				else
				{
					lock (_lock)
						CIMService = new LayoutService(LayoutView.Active.Layout);
				}

				//anything specific here
				switch (args.Hint)
				{
					case ElementEventHint.ElementAdded:
						break;
					case ElementEventHint.ElementRemoved:
						break;
					case ElementEventHint.SelectionChanged:
						break;
					case ElementEventHint.MapFrameActivated:
						break;
					case ElementEventHint.MapFrameDeactivated:
						break;
					case ElementEventHint.MapFrameNavigated:
						break;
					case ElementEventHint.PlacementChanged:
						break;
					case ElementEventHint.PropertyChanged:
						break;
					case ElementEventHint.StyleChanged:
						break;
				}
			});

			#endregion

			#region Reports

			ArcGIS.Desktop.Reports.Events.ReportDataSourceChangedEvent.Subscribe((args) =>
			{
				if (LayoutView.Active == null)
					return;
				if (CIMViewerModule.IgnoreEvents)
					return;

				lock (this)
				{
					if (args.ReportSectionElement != null)
						CIMService = new LayoutElementService(args.ReportSectionElement);
					else if (args.Report != null)
						CIMService = new LayoutService(LayoutView.Active.Layout);
				}
			});

			ArcGIS.Desktop.Reports.Events.ReportPropertyChangedEvent.Subscribe((args) =>
			{
				if (LayoutView.Active == null)
					return;
				if (CIMViewerModule.IgnoreEvents)
					return;

				lock (this)
					CIMService = new LayoutService(LayoutView.Active.Layout);
			});

			ArcGIS.Desktop.Reports.Events.ReportSectionChangedEvent.Subscribe((args) =>
			{
				if (LayoutView.Active == null)
					return;
				if (CIMViewerModule.IgnoreEvents)
					return;

				lock (this)
				{
					if (args.ReportSectionElement != null)
						CIMService = new LayoutElementService(args.ReportSectionElement);
					else if (args.Report != null)
						CIMService = new LayoutService(LayoutView.Active.Layout);
				}
			});
			#endregion

			#region Map
			//The map is deleted from the Project dockpane
			ArcGIS.Desktop.Mapping.Events.MapRemovedEvent.Subscribe((args) =>
			{
				if (CIMViewerModule.IgnoreEvents)
					return;

				lock (this)
				{
					if (CIMService != null && CIMService.ServiceType == CIMServiceType.Map)
					{
						if (args.MapPath == CIMService.URI)
						{
							//this affects our map
							CIMService = null;//just do it when our map is removed
						}
					}
				}
			});
			//The MapPane is closed
			ArcGIS.Desktop.Mapping.Events.MapClosedEvent.Subscribe((args) =>
			{
				if (CIMViewerModule.IgnoreEvents)
					return;

				if (CIMService != null && CIMService.ServiceType == CIMServiceType.Map)
				{
					//if (args.MapPane.MapView != null) {//I think MapView is always valid here
					//  if (args.MapPane.MapView.Map.URI == CIMService.URI) {
					//    //this affects our map
					//    CIMService = null;//just do it when our map is closed
					//  }
					//}
					if (FrameworkApplication.Panes.Count == 0)
					{
						//|| FrameworkApplication.Panes.OfType<IMapPane>().Count() == 0
						lock (this)
							CIMService = null;//There are no more maps
					}
				}
			});
			ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Subscribe((args) =>
			{
				if (CIMViewerModule.IgnoreEvents)
					return;
				//this event also comes through on Layout
				if (MapView.Active == null)
					return;
				//The MapView can be active if there is a mapframe on the layout so
				//check we really do have a MapView as the active pane (and not a layout)
				if (FrameworkApplication.Panes.ActivePane is IMapPane)
					SetMapMember(args.MapView);

			});

			ArcGIS.Desktop.Mapping.Events.ActiveMapViewChangedEvent.Subscribe((args) =>
			{
				if (CIMViewerModule.IgnoreEvents)
					return;

				//if (args.OutgoingView != null && 
				//(_cimService?.ServiceType == CIMServiceType.Map || _cimService?.ServiceType == CIMServiceType.MapMember))
				//  CIMService = null;
				if (FrameworkApplication.Panes.ActivePane != null)
				{
					if (FrameworkApplication.Panes.ActivePane is ILayoutPane)
						return;//the mapview in a mapframe is activating/deactivating
				}

				//This can fire for a map on a layout if the layout is being activated...
				if (args.IncomingView != null)
					SetMapMember(args.IncomingView);
			});

			ArcGIS.Desktop.Mapping.Events.MapPropertyChangedEvent.Subscribe((args) =>
			{

				if (_cimService != null &&
						_cimService.ServiceType == CIMServiceType.Map)
				{
					//we have a map definition loaded
					foreach (var map in args.Maps)
					{
						if (map.URI == _cimService.URI)
						{
							//our map is one of the maps that changed
							//refresh it
							lock (this)
								CIMService = new MapService(map);
							break;
						}
					}
				}

			});

			ArcGIS.Desktop.Mapping.Events.MapMemberPropertiesChangedEvent.Subscribe((args) =>
			{

				var hints = new List<string>();
				foreach (var hint in args.EventHints)
					hints.Add(hint.ToString());
				var hint_string = string.Join(",", hints);

				if (_cimService != null && _cimService.ServiceType == CIMServiceType.MapMember)
				{
					foreach (var mm in args.MapMembers)
					{
						if (mm.URI == _cimService.URI)
						{
							//refresh
							lock (this)
								CIMService = new MapMemberService(mm);
							break;
						}
					}
				}
			});

			ArcGIS.Desktop.Mapping.Events.LayersRemovedEvent.Subscribe((args) =>
			{
				if (CIMViewerModule.IgnoreEvents)
					return;

				if (_cimService != null && _cimService.ServiceType == CIMServiceType.MapMember)
				{
					foreach (var layer in args.Layers)
					{
						if (layer.URI == _cimService.URI)
						{
							lock (this)
								CIMService = null;
						}
					}
				}
			});

			//New events for Voxel
			ArcGIS.Desktop.Mapping.Voxel.Events.VoxelAssetChangedEvent.Subscribe((args) =>
			{
				if (_cimService != null && _cimService.ServiceType != CIMServiceType.MapMember)
					return;

				var vxl_layer = MapView.Active?.GetSelectedLayers().OfType<VoxelLayer>();
				if (vxl_layer == null)
					return;
				if (CIMViewerModule.IgnoreEvents)
					return;

				//there will only be one (or none) selected...
				var surface = MapView.Active.GetSelectedIsosurfaces().FirstOrDefault();
				var slice = MapView.Active.GetSelectedSlices().FirstOrDefault();
				var section = MapView.Active.GetSelectedSections().FirstOrDefault();
				var locked_section = MapView.Active.GetSelectedLockedSections().FirstOrDefault();

				if (surface != null)
					lock (this)
						CIMService = new MapMemberService(surface.Layer);
				else if (slice != null)
					lock (this)
						CIMService = new MapMemberService(slice.Layer);
				else if (section != null)
					lock (this)
						CIMService = new MapMemberService(section.Layer);
				else if (locked_section != null)
					lock (this)
						CIMService = new MapMemberService(locked_section.Layer);
				else
				{
					SetMapMember(MapView.Active);
				}
			});

			#endregion Map

			#region Pane

			//There is no view activation for layout/report
			ArcGIS.Desktop.Framework.Events.ActivePaneChangedEvent.Subscribe((args) =>
			{
				if (CIMViewerModule.IgnoreEvents)
					return;

				if (args.IncomingPane is ILayoutPane)
				{
					var layoutPane = args.IncomingPane as ILayoutPane;
					lock (this)
					{
						//This event can come early, before the view has been created...
						if (layoutPane.LayoutView != null)
						{
							CIMService = new LayoutService(layoutPane.LayoutView.Layout);
						}
						//else
						//{
						//	CIMService = null;
						//}
					}
				}
			});

			#endregion
		}

		private void SetMapMember(MapView mv)
		{
			bool changed = false;
			if (mv != null)
			{
				//Was a layer selected?
				var layer = mv.GetSelectedLayers().FirstOrDefault();
				var surface_layer = mv.GetSelectedElevationSurfaceLayers().FirstOrDefault();
				var source_layer = mv.GetSelectedElevationSourceLayers().FirstOrDefault();

				//Was a layer selected?
				if (layer != null)
				{
					lock (this)
						CIMService = new MapMemberService(layer);
					changed = true;
				}
				//Was an elevation surface selected?
				else if (surface_layer != null)
        {
					lock (this)
						CIMService = new MapMemberService(surface_layer);
					changed = true;
				}
				//Was an elevation source selected?
				else if (source_layer != null)
				{
					lock (this)
						CIMService = new MapMemberService(source_layer);
					changed = true;
				}
				else
				{
					//Was a table selected?
					var table = mv.GetSelectedStandaloneTables().FirstOrDefault();
					if (table != null)
					{
						lock (this)
							CIMService = new MapMemberService(table);
						changed = true;
					}
					else
					{
						//A Map must have been selected
						lock (this)
							CIMService = new MapService(mv.Map);
						changed = true;
					}
				}
			}
			if (!changed && CIMService != null)
			{
				lock (this)
					CIMService = null;
			}
		}

		private void SetLayoutElement(IEnumerable<IElement> elements)
		{
			if (elements == null)
				return;
			SetLayoutElement(elements.Cast<Element>());
		}

		private void SetLayoutElement(IEnumerable<Element> elements)
		{
			if (elements == null)
				return;
			if (_cimService != null &&
					_cimService.ServiceType == CIMServiceType.LayoutElement)
			{
				//we have a layout element loaded
				//refresh our item
				var layout = LayoutView.Active?.Layout;
				var elem = elements.FirstOrDefault(e => e.Name == _cimService.URI);
				if (elem == null)
					elem = elements.FirstOrDefault();
				lock (this)
				{
					CIMService = elem != null ? new LayoutElementService(elem) : null;
				}
			}
		}

		public CIMService CIMService
		{
			get
			{
				return _cimService;
			}
			set
			{
				SetProperty(ref _cimService, value, () => CIMService);
				if (_cimService == null)
					this.Caption = _originalCaption;
				else
				{
					this.Caption = string.Format("{0} - {1}", _originalCaption, _cimService.URI);
				}
			}
		}

		/// <summary>
		/// Show the DockPane.
		/// </summary>
		internal static void Show()
		{
			DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
			if (pane == null)
				return;

			pane.Activate();
		}
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class CIMDockpaneView_ShowButton : Button
	{
		protected override void OnClick()
		{
			CIMDockpaneViewModel.Show();
		}
	}
}
