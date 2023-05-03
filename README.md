# J4JMapLibrary and WinUI 3 Controls

## Significant Non-breaking Changes

The WinUI 3 demo app, **WinAppTest**, is now decoupled from my hosting/dependency injection libraries.

## NuGet

You can find the latest NuGet packages here:

- [J4JMapLibrary](https://www.nuget.org/packages/J4JSoftware.J4JMapLibrary/)
- [J4JMapWinLibrary](https://www.nuget.org/packages/J4JSoftware.J4JMapWinLibrary/)

## Components

This repository contains assemblies for interacting with various online map services and displaying imagery obtained from them:

- **J4JMapLibrary** is the core library. You can find its [documentation here](J4JMapLibrary/docs/readme.md).
- **J4JMapWinLibrary** contains the actual map control. You can find its [documentation here](J4JMapWinLibrary/docs/readme.md).
- **MapLibTests** contains XUnit tests for **J4JMapLibrary**
- **WinAppTest** contains a demo WinUI 3 app (which is also used for testing)

## Mapping Services

All the mapping services currently supported by **J4JMapControl** require authentication. That isn't hard or expensive to obtain, but without credentials no map imagery will be displayed. You can, however, "obtain" credentials for **Open Street Maps** and **Open Topo Maps** by supplying a user agent string identifying your app (or the **WinAppTest**, when you run it on your system).

|Projection|Obtaining Credentials|Links|
|----------|--------------------|-----|
|Bing Maps|You have to create an account with the service|[https://www.microsoft.com/en-us/maps/create-a-bing-maps-key](https://www.microsoft.com/en-us/maps/create-a-bing-maps-key)|
|Google Maps|You have to create an account with the service|[https://developers.google.com/maps/documentation/maps-static/get-api-key](https://developers.google.com/maps/documentation/maps-static/get-api-key)|
|Open Street Maps|You must identify your app with a user agent string|[https://operations.osmfoundation.org/policies/tiles/](https://operations.osmfoundation.org/policies/tiles/)|
|Open Topo Maps|You must identify your app with a user agent string|[https://operations.osmfoundation.org/policies/tiles/](https://operations.osmfoundation.org/policies/tiles/)|

Enjoy!
