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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using CIMViewer.Helpers;

namespace CIMViewer {
    internal class CIMDockpaneViewModel : DockPane {
        private const string _dockPaneID = "CIMViewerModule_CIMViewerDockpane";
        private CIMService _cimService = null;
        private string _originalCaption = "";

        protected CIMDockpaneViewModel() {

            Initialize();
            _originalCaption = this.Caption;
            SetMapMember(MapView.Active);
        }


        private void Initialize() {
            
            ArcGIS.Desktop.Core.Events.ProjectClosingEvent.Subscribe((args) => {
                CIMService = null;//just do it when the project closes
                return Task.FromResult(0);
            });
            //The map is deleted from the Project dockpane
            ArcGIS.Desktop.Mapping.Events.MapRemovedEvent.Subscribe((args) => {
                lock (this) {
                    if (CIMService != null && CIMService.ServiceType == CIMServiceType.Map) {
                        if (args.MapPath == CIMService.URI) {
                            //this affects our map
                            CIMService = null;//just do it when our map is removed
                        }
                    }
                }
            });
            //The MapPane is closed
            ArcGIS.Desktop.Mapping.Events.MapClosedEvent.Subscribe((args) => {
                lock (this) {
                    if (CIMService != null && CIMService.ServiceType == CIMServiceType.Map) {
                        if (args.MapPane.MapView != null) {//I think MapView is always valid here
                            if (args.MapPane.MapView.Map.URI == CIMService.URI) {
                                //this affects our map
                                CIMService = null;//just do it when our map is closed
                            }
                        }
                    }
                }
            });
            ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Subscribe((args) => {
                if (CIMViewerModule.IgnoreEvents)
                    return;
                CIMViewerModule.IgnoreEvents = true;
                SetMapMember(args.MapView);
                CIMViewerModule.IgnoreEvents = false;
            });
            ArcGIS.Desktop.Mapping.Events.ActiveMapViewChangedEvent.Subscribe((args) => {
                if (CIMViewerModule.IgnoreEvents)
                    return;
                CIMViewerModule.IgnoreEvents = true;
                if (args.OutgoingView != null)
                    CIMService = null;
                else if (args.IncomingView != null)
                    SetMapMember(args.IncomingView);
                CIMViewerModule.IgnoreEvents = false;
            });
            //Currently changes to an Animation do not raise a map property changed
            //event....the map xml needs to be refreshed "manually"
            ArcGIS.Desktop.Mapping.Events.MapPropertyChangedEvent.Subscribe((args) => {
                if (CIMViewerModule.IgnoreEvents)
                    return;
                CIMViewerModule.IgnoreEvents = true;
                if (_cimService != null && 
                    _cimService.ServiceType == CIMServiceType.Map) {
                    //we have a map definition loaded
                    foreach (var map in args.Maps) {
                        if (map.URI == _cimService.URI) {
                            //our map is one of the maps that changed
                            //refresh it
                            CIMService = new MapService(map);
                            break;
                        }
                    }
                }
                CIMViewerModule.IgnoreEvents = false;
            });
            ArcGIS.Desktop.Mapping.Events.MapMemberPropertiesChangedEvent.Subscribe((args) => {
                if (CIMViewerModule.IgnoreEvents)
                    return;
                CIMViewerModule.IgnoreEvents = true;

                if (_cimService != null && _cimService.ServiceType == CIMServiceType.MapMember) {
                    foreach (var mm in args.MapMembers) {
                        if (mm.URI == _cimService.URI) {
                            //refresh
                            CIMService = new MapMemberService(mm);
                            break;
                        }
                    }
                }
                CIMViewerModule.IgnoreEvents = false;
            });
            ArcGIS.Desktop.Mapping.Events.LayersRemovedEvent.Subscribe((args) => {
                if (CIMViewerModule.IgnoreEvents)
                    return;
                CIMViewerModule.IgnoreEvents = true;

                if (_cimService != null && _cimService.ServiceType == CIMServiceType.MapMember) {
                    foreach (var layer in args.Layers) {
                        if (layer.URI == _cimService.URI) {
                            CIMService = null;
                        }
                    }
                }
                CIMViewerModule.IgnoreEvents = false;
            });
        }

        private void SetMapMember(MapView mv) {
            bool changed = false;
            if (mv != null) {
                //Was a layer selected?
                var layer = mv.GetSelectedLayers().FirstOrDefault();
                if (layer != null) {
                    CIMService = new MapMemberService(layer);
                    changed = true;
                }
                else {
                    //Was a table selected?
                    var table = mv.GetSelectedStandaloneTables().FirstOrDefault();
                    if (table != null) {
                        CIMService = new MapMemberService(table);
                        changed = true;
                    }
                    else {
                        //A Map must have been selected
                        CIMService = new MapService(mv.Map);
                        changed = true;
                    }
                }
            }
            if (!changed && CIMService != null) {
                CIMService = null;
            }
        }

        public CIMService CIMService {
            get {
                return _cimService;
            }
            set
            {
                CIMViewerModule.IgnoreEvents = true;
                SetProperty(ref _cimService, value, () => CIMService);
                if (_cimService == null)
                    this.Caption = _originalCaption;
                else {
                    this.Caption = string.Format("{0} - {1}", _originalCaption, _cimService.URI);
                }
                CIMViewerModule.IgnoreEvents = false;
            }
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show() {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;
            
            pane.Activate();
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class CIMDockpaneView_ShowButton : Button {
        protected override void OnClick() {
            CIMDockpaneViewModel.Show();
        }
    }
}
