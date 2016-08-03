# arcgis-pro-sdk-cim-viewer
Contains the source code for the 'CIM Viewer' ArcGIS Pro Add-in which allows inspection and modification of underlying CIM models.

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Framework, Map Authoring
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, http://www.esri.com
Date:                  4/4/2016
ArcGIS Pro:            1.3
Visual Studio:         2013, 2015
.NET Target Framework: .NET Framework 4.6.1
```

##Resources

This add-in requires a thrid party nuget called AvalonEdit, when you first rebuild your solution that nuget should be updated (or installed) auomatically, however, if that is not the case you can install the control for here:

* [Avalon Edit nuget](https://www.nuget.org/packages/AvalonEdit)

##Overview
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
The CIMViewer can be used to examine layer and map (2D or 3D) CIM definitions. Select a layer, map, or scene in the TOC with the viewer open and its CIM definition will be loaded into the XML Editor. The XML Editor uses the AvalonEdit control which provides syntax colorization and formatting. Cut, Copy, Paste, and XML Validation have been added in the Add-in so the XML editing experience is reasonably functional though not as rich as a fully fledged commercial editor like XML Spy. However, it is a simple task of copy/pasting the CIM XML into a commercial editor if that level of XML manipulation is desired.

The Save button will save any changes you make back to the layer, map, or scene whose CIM definition you loaded. However, there is not much of a safety net to protect you against making really bad xml or other inadvertant mistakes so use the `Save` functionality with caution. The primary use of the CIM Viewer is to allow you, the developer, to decipher the inner workings or "guts" of the CIM and how it affects the configuration of your Pro project at any given point in time. In other words, it is a learning or educational tool, not a customization or configuration tool, that can be used to help you in your Pro development efforts.

##Future:

* Add CIM "Viewing" for Layouts
* Add CIM "Viewing" for Project

##How to use the CIM Viewer tool

1. Make sure you have re-built the CIM Viewer tool and the add-in is available in ArcGIS Pro.  
1. Start ArcGIS Pro.  
1. Open any project file with a map or scene. Click on the "CIM Viewer" tab on the ribbon and then on the "Show CIMViewer" button.  
![UI](Screenshots/Screen1.png)  
1. Open the "Contents" dockpane and select any layer in the TOC of the map or scene.  
![UI](Screenshots/Screen2.png)  
1. View the "Cartographic Information Model Viewer" dockpane to see and/or manipulate the CIM for the selected layer.  
![UI](Screenshots/Screen3.png)  
1. To "post" (or save) any changes you made in the "Cartographic Information Model Viewer" dockpane back to ArcGIS Pro you can click the 'Save' button.
1. Select the map or scene node in the TOC to view its CIM
![UI](Screenshots/Screen4.png)
