using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
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
using ArcGIS.Core.Events;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using ArcGIS.Desktop.Layouts;

namespace PreviewSymbol
{

	internal class SelectedSymbolChangedEventArgs : EventBase
	{

		public CIMSymbol SelectedSymbol { get; set; }

		/// <summary>
		/// If this is present, it means that a graphic element (from
		/// a graphics layer) has been selected
		/// </summary>
		public GraphicElement SelectedElement { get; set; }

		public string SelectedLayerName { get; set; }

		public long SelectedOID { get; set; }

		public string Message { get; set; }

		public SelectedSymbolChangedEventArgs(CIMSymbol symbol, GraphicElement element, string layerName,
																					long oid, string msg)
		{
			SelectedSymbol = symbol;
			SelectedElement = element;
			SelectedLayerName = layerName;
			SelectedOID = oid;
			Message = msg;
		}
	}

	internal class SelectedSymbolChangedEvent : CompositePresentationEvent<SelectedSymbolChangedEventArgs>
	{
		public static SubscriptionToken Subscribe(Action<SelectedSymbolChangedEventArgs> action, bool keepSubscriberReferenceAlive = false)
		{
			return FrameworkApplication.EventAggregator.GetEvent<SelectedSymbolChangedEvent>()
				.Register(action, keepSubscriberReferenceAlive);
		}

		public static void Unsubscribe(Action<SelectedSymbolChangedEventArgs> subscriber)
		{
			FrameworkApplication.EventAggregator.GetEvent<SelectedSymbolChangedEvent>().Unregister(subscriber);
		}

		public static void Unsubscribe(SubscriptionToken token)
		{
			FrameworkApplication.EventAggregator.GetEvent<SelectedSymbolChangedEvent>().Unregister(token);
		}

		internal static void Publish(SelectedSymbolChangedEventArgs payload)
		{
			FrameworkApplication.EventAggregator.GetEvent<SelectedSymbolChangedEvent>().Broadcast(payload);
		}
	}

	internal class Module1 : Module
	{
		private static Module1 _this = null;

		private CIMSymbol _selectedSymbol = null;
		private GraphicElement _selectedElement = null;
		private string _layerName = null;
		private long _oid = -1;
		private string _msg;
		private bool _lockSymbolPreview = false;

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static Module1 Current
		{
			get
			{
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("PreviewSymbol_Module"));
			}
		}

		public CIMSymbol SelectedSymbol => _selectedSymbol;

		public GraphicElement SelectedElement => _selectedElement;

		public string LayerName => _layerName;

		public long SelectedOID => _oid;

		public string Message => _msg;

		public bool LockSymbolPreview
		{
			get
			{
				return _lockSymbolPreview;
			}
			set
			{
				_lockSymbolPreview = value;
			}
		}

		public void SetSelected(CIMSymbol symbol, GraphicElement element, string layerName, 
			                      long oid, string msg)
		{
			_selectedSymbol = symbol;
			_selectedElement = element;
			_layerName = layerName;
			_oid = oid;
			_msg = msg;
			SelectedSymbolChangedEvent.Publish(
				new SelectedSymbolChangedEventArgs(symbol, element, layerName, oid, msg));
		}

		public void NothingSelected(string msg)
		{
			SetSelected(null, null, "", -1, msg);
		}

		public T DeserializeXmlDefinition<T>(string xmlDefinition)
		{
			using (var stringReader = new StringReader(xmlDefinition))
			{
				using (var reader = new XmlTextReader(stringReader))
				{
					reader.WhitespaceHandling = WhitespaceHandling.Significant;
					reader.MoveToContent();
					var typeName = reader.GetAttribute("xsi:type").Replace("typens:", "ArcGIS.Core.CIM.");
					var cimObject = System.Activator.CreateInstance("ArcGIS.Core", typeName).Unwrap() as IXmlSerializable;
					cimObject.ReadXml(reader);
					return (T)cimObject;
				}
			}
		}

		#region Overrides
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

	}
}
