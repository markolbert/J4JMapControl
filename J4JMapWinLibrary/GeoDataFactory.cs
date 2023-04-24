using System.Collections;
using System.Collections.Generic;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapWinLibrary;

public class GeoDataFactory
{
    private readonly string _locProp;
    private readonly ILogger? _logger;

    public GeoDataFactory(
        string locationProperty,
        ILoggerFactory? loggerFactory = null

    )
    {
        _locProp = locationProperty;
        _logger = loggerFactory?.CreateLogger<GeoDataFactory>();
    }

    public IEnumerable<GeoData> Create( object source )
    {
        if( source is not IEnumerable items )
            yield break;

        foreach( var item in items )
        {
            var retVal = new GeoData( item );

            SetLocation(retVal);

            yield return retVal;
        }
    }

    private void SetLocation( GeoData item )
    {
        var locPropInfo = item.Entity.GetType().GetProperty(_locProp);
        if (locPropInfo == null)
        {
            _logger?.LogWarning("{entityType} does not define a {locProp} property", item.Entity.GetType(), _locProp);
            return;
        }

        if (locPropInfo.PropertyType != typeof(string))
        {
            _logger?.LogWarning("{locProp} property on {entityType} is not a string",
                                _locProp,
                                item.Entity.GetType());
            return;
        }

        var toParse = locPropInfo.GetValue(item.Entity) as string;

        if (!MapExtensions.TryParseToLatLong(toParse, out var latitude, out var longitude))
        {
            _logger?.LogWarning("Could not parse '{text}' to latitude/longitude", toParse);
            return;
        }

        item.Latitude = latitude;
        item.Longitude = longitude;
        item.LocationIsValid = true;
    }
}
