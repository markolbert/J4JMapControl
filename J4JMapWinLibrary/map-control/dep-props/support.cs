using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private bool CheckDataSource(
        object? source,
        string sourceName,
        string propName,
        Func<PropertyInfo, bool> propChecker,
        string error
    )
    {
        if( source == null )
            return false;

        var propInfo = source.GetType().GetProperty( propName );

        if( propInfo == null )
        {
            _logger?.LogWarning( "{source} items do not all include a {property} property",
                                 sourceName,
                                 propName );
            return false;
        }

        if( propChecker( propInfo ) )
            return true;

        _logger?.LogWarning( "{property} on {source} items {error}",
                             propName,
                             sourceName,
                             error );

        return false;
    }
}
