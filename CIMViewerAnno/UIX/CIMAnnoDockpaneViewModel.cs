using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using CIMViewerAnno.Events;

namespace CIMViewerAnno.UIX
{
  internal class CIMAnnoDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "CIMViewerAnno_UI_CIMAnnoDockpane";
    private CIMTextGraphic _cimTextGraphic = null;
    private static bool _oneShot = false;

    protected CIMAnnoDockpaneViewModel() { }

    protected override Task InitializeAsync()
    {
      SelectedGraphicChangedEvent.Subscribe((args) => {
        this.TextGraphic = args.SelectedGraphic;
      });
      this.TextGraphic = Module1.Current.SelectedGraphic;
      return Task.FromResult(0);
    }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      //The very first time, if the pane is not visible, set the
      //selection tool active
      if (!pane.IsVisible && !_oneShot)
      {
        _oneShot = true;
        FrameworkApplication.SetCurrentToolAsync("esri_mapping_selectByRectangleTool");
      }
      pane.Activate();

    }

    #region Properties

    public CIMTextGraphic TextGraphic
    {
      get
      {
        return _cimTextGraphic;
      }
      set
      {
        SetProperty(ref _cimTextGraphic, value, () => TextGraphic);
      }
    }

    private ICommand _selectCommand;

    public ICommand SelectCommand
    {
      get
      {
        return _selectCommand ?? (_selectCommand = new RelayCommand(() => {
          FrameworkApplication.SetCurrentToolAsync("CIMViewerAnno_Ribbon_SelectAnno");
        }));
      }
    }

    #endregion
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class CIMAnnoDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      CIMAnnoDockpaneViewModel.Show();
    }
  }
}
