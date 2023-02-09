﻿using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class OpenMapServer : MapServer<FixedMapTile, string>
{
    private string _userAgent = string.Empty;

    protected OpenMapServer()
    {
        ImageFileExtension = ".png";
    }

    public string RetrievalUrl { get; init; } = string.Empty;
    public override bool Initialized => !string.IsNullOrEmpty(_userAgent);

#pragma warning disable CS1998
    public override async Task<bool> InitializeAsync(string userAgent)
#pragma warning restore CS1998
    {
        _userAgent = userAgent;

        return !string.IsNullOrEmpty(_userAgent);
    }

    public override HttpRequestMessage? CreateMessage(FixedMapTile requestInfo)
    {
        if (!Initialized)
            return null;

        if (string.IsNullOrEmpty(_userAgent))
        {
            Logger.Error("Undefined or empty User-Agent");
            return null;
        }

        var uriText = RetrievalUrl.Replace("ZoomLevel", requestInfo.Scope.Scale.ToString())
            .Replace("XTile", requestInfo.X.ToString())
            .Replace("YTile", requestInfo.Y.ToString());

        var retVal = new HttpRequestMessage(HttpMethod.Get, new Uri(uriText));
        retVal.Headers.Add("User-Agent", _userAgent);

        return retVal;
    }
}