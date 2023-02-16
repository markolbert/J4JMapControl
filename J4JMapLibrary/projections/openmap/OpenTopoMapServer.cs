﻿namespace J4JMapLibrary;

[ MapServer("OpenTopoMaps") ]
public sealed class OpenTopoMapServer : OpenMapServer
{
    public OpenTopoMapServer()
    {
        MinScale = 0;
        MaxScale = 15;
        MaxRequestLatency = 5000;
        RetrievalUrl = "https://tile.opentopomap.org/{zoom}/{x}/{y}.png";
        Copyright = "© OpenTopoMap-Mitwirkende, SRTM | Kartendarstellung\n© OpenTopoMap\nCC-BY-SA";
        CopyrightUri = new Uri( "http://opentopomap.org/" );
    }
}
