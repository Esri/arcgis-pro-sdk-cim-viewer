# arcgis-pro-sdk-cim-viewer
Contains the source code for the 'CIM Viewer' ArcGIS Pro Add-in which allows inspection and modification of underlying CIM models.

<!-- TODO: Fill this section below with metadata about this sample-->
```
Language:              C#
Subject:               Framework, Map Authoring
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, http://www.esri.com
Date:                  8/3/2016
ArcGIS Pro:            1.3
Visual Studio:         2015
.NET Target Framework: .NET Framework 4.6.1
```

## Overview
<!-- TODO: Explain how this sample can be used. To use images in this section, create the image file in your sample project's screenshots folder. Use relative url to link to this image using this syntax: ![My sample Image](FacePage/SampleImage.png) -->
The CIMViewer can be used to examine layer, map (2D or 3D), and layout CIM definitions. Select a layer, map, scene, or layout in the TOC with the viewer open and its CIM definition will be loaded into the XML Editor. The XML Editor uses the AvalonEdit control which provides syntax colorization and formatting. Cut, Copy, Paste, and XML Validation have been added in the Add-in so the XML editing experience is reasonably functional though not as rich as a fully fledged commercial editor like XML Spy. However, it is a simple task of copy/pasting the CIM XML into a commercial editor if that level of XML manipulation is desired.

The Save button will save any changes you make back to the layer, map, scene, or layout whose CIM definition you loaded. However, there is not much of a safety net to protect you against making really bad xml or other inadvertant mistakes so use the `Save` functionality with caution. The primary use of the CIM Viewer is to allow you, the developer, to decipher the inner workings or "guts" of the CIM and how it affects the configuration of your Pro project at any given point in time. In other words, it is a <u>learning or educational tool</u> that can be used to help you in your Pro development efforts. It is **not** a customization or configuration tool.

A second add-in, CIMViewerAnno, has been added to the CIMViewerSolution. CIMViewerAnno adds a dockpane that allows you to view the CIMTextGraphic of annotation features. With at least one annotation layer loaded into the current map, select annotation features to load their CIM (similar to the way the CIMViewer works). You will see a preview image of the selected text graphic along with its CIM definition. You can use the standard Pro select tool or the custom select tool provided with the CIMViewerAnno add-in.

## ArcGIS Pro 2.1

 * Support for layouts and layout elements added
 * A CIMViewerAnno project is added to allow you to view and edit anno text graphics CIM xml.

## CIM Viewer tool and CIMViewerAnno resources

Both add-ins require the [ArcGIS Pro SDK](https://github.com/esri/arcgis-pro-sdk#installing-arcgis-pro-sdk-for-net)

Both add-ins require a third party nuget called AvalonEdit and a third party nuget called Extended.Wpf.Toolkit. 

* [Avalon Edit nuget](https://www.nuget.org/packages/AvalonEdit)
* [Extended.Wpf.Toolkit](http://wpftoolkit.codeplex.com/)

When you first rebuild your solution those nuget are usually updated (or installed) auomatically. However, if that is not the case, you can use the NuGet Package manager inside Visual Studio (Tools->NuGet Package Manager->Manage NuGet Packages for Solution...).

![NuGet](Screenshots/vs1.png)

![NuGet](Screenshots/vs2.png)

You don't have the NuGet Package Manager installed? Find it [here](https://visualstudiogallery.msdn.microsoft.com/5d345edc-2e2d-4a9c-b73b-d53956dc458d) or go to Tools->Extensions and Updates and search for "NuGet Package Manager"
You are not familiar with NuGets? Watch [this tutorial](https://www.youtube.com/watch?v=F8sx49NdCNk)

**Note:**  
If your References to the ArcGIS Pro Assemblies in the CIMViewer project are broken (because your ArcGIS Pro is installed to a different location than the CIMViewer references) don't forget to use the [Pro Fix References](https://github.com/esri/arcgis-pro-sdk#arcgis-pro-sdk-for-net-utilities) utility that comes with the Pro SDK to fix them

![ProSDK](Screenshots/pro_fix1.png)

## How to use the CIM Viewer tool

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

## CIMViewerAnno tool

1. Make sure you have the add-in built and available
1. Start ArcGIS Pro
1. Open any project that has an annotation layer
1. Open the CIMAnnoDockPane
1. Select any annotation feature to load its text graphic CIM definition
![UI](Screenshots/Screen5.png)
1. If you make changes to the CIM definition you can use "Preview" to change the preview image on the dockpane without changing the underlying annotation feature.
1. Select "Save" to apply your changes to the selected annotation feature (assuming you have the appropriate write permissions on the annotation feature class).

## ArcGIS Pro SDK Resources

* [API Reference online](http://pro.arcgis.com/en/pro-app/sdk/api-reference)
* <a href="http://pro.arcgis.com/en/pro-app/sdk/" target="_blank">ArcGIS Pro SDK for .NET (pro.arcgis.com)</a>
* [arcgis-pro-sdk-community-samples](http://github.com/Esri/arcgis-pro-sdk-community-samples)
* [ArcGIS Pro DAML ID Reference](http://github.com/Esri/arcgis-pro-sdk/wiki/ArcGIS Pro DAML ID Reference)
* [FAQ](http://github.com/Esri/arcgis-pro-sdk/wiki/FAQ)
* [ArcGIS Pro SDK icons](https://github.com/Esri/arcgis-pro-sdk/releases/tag/1.2.0.5023)  
![ArcGIS Pro SDK for .NET Icons](https://esri.github.io/arcgis-pro-sdk/images/Home/Image-of-icons.png "ArcGIS Pro SDK Icons")
* [ProConcepts: ArcGIS Pro Add in Samples](https://github.com/Esri/arcgis-pro-sdk-community-samples/wiki/ProConcepts-ArcGIS-Pro-Add-in-Samples)

### Samples Data

* Sample data for ArcGIS Pro SDK Community Samples can be downloaded from the [repo releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) page. 

## Requirements
The requirements for the machine on which you develop your ArcGIS Pro add-ins are listed here.

#### ArcGIS Pro

* ArcGIS Pro 1.3  
* Interested in ArcGIS Pro versions older than 1.3? Read this [ProConcept](https://github.com/Esri/arcgis-pro-sdk-community-samples/wiki/ProConcepts-Working-With-Previous-Versions-of-ArcGIS-Pro)  

#### Supported platforms

* Windows 10 (Home, Pro, Enterprise, Education) (64 bit [EM64T])  
* Windows 8.1 Basic, Professional, and Enterprise (64 bit [EM64T]) 
* Windows 8 Basic, Professional, and Enterprise (64 bit [EM64T]) 
* Windows 7 SP1 Ultimate, Enterprise, Professional, and Home Premium (64 bit [EM64T]) 

#### Supported .NET framework

* Microsoft .NET Framework 4.6.1 Developer Pack 

#### Supported IDEs

* Visual Studio 2015 (Professional, Enterprise, and Community Editions)

Note: [ArcGIS Pro system requirements](http://pro.arcgis.com/en/pro-app/get-started/arcgis-pro-system-requirements.htm) 

## Contributing

Esri welcomes contributions from anyone and everyone. Please see our [guidelines for contributing](https://github.com/esri/contributing).

Refer to this [wiki](https://github.com/Esri/arcgis-pro-sdk-community-samples/wiki/ProGuide-Contribute-Samples) for detailed instructions on the ArcGIS Pro SDK Samples submission process.

## Licensing
Copyright 2016 Esri

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at:

   http://www.apache.org/licenses/LICENSE-2.0.

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's [license.txt](./License.txt) file.

[](Esri Tags: ArcGIS-Pro-SDK)
[](Esri Language: C-Sharp)​

<p align = center><img src="http://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png"  alt="pre-req" align = "top" height = "20" width = "20" >
<b> ArcGIS Pro 1.3 SDK for Microsoft .NET Framework</b>
</p>
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="http://pro.arcgis.com/en/pro-app/sdk" target="_blank">ArcGIS Pro SDK</a> | <a href="http://pro.arcgis.com/en/pro-app/sdk/api-reference/index.html" target="_blank">API Reference</a> | [Requirements](#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | [Getting Started](https://github.com/Esri/arcgis-pro-sdk/wiki#getting-started) | <a href="http://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>


