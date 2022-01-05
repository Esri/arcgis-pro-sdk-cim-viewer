using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;

namespace CIMViewerAnno.Events
{

  public class SelectedGraphicChangedEventArgs : EventBase
  {

    public CIMTextGraphic SelectedGraphic { get; private set; }

    public AnnotationLayer AnnotationLayer { get; private set; }

    public SelectedGraphicChangedEventArgs(AnnotationLayer annoLayer, CIMTextGraphic graphic)
    {
      AnnotationLayer = annoLayer;
      SelectedGraphic = graphic;
    }
  }

  /// <summary>
  /// Notify us when an annotation feature has been selected
  /// </summary>
  public class SelectedGraphicChangedEvent : CompositePresentationEvent<SelectedGraphicChangedEventArgs>
  {

    public static SubscriptionToken Subscribe(Action<SelectedGraphicChangedEventArgs> action, bool keepSubscriberReferenceAlive = false)
    {
      return FrameworkApplication.EventAggregator.GetEvent<SelectedGraphicChangedEvent>()
        .Register(action, keepSubscriberReferenceAlive);
    }

    public static void Unsubscribe(Action<SelectedGraphicChangedEventArgs> subscriber)
    {
      FrameworkApplication.EventAggregator.GetEvent<SelectedGraphicChangedEvent>().Unregister(subscriber);
    }

    public static void Unsubscribe(SubscriptionToken token)
    {
      FrameworkApplication.EventAggregator.GetEvent<SelectedGraphicChangedEvent>().Unregister(token);
    }

    internal static void Publish(SelectedGraphicChangedEventArgs payload)
    {
      FrameworkApplication.EventAggregator.GetEvent<SelectedGraphicChangedEvent>().Broadcast(payload);
    }
  }
}
