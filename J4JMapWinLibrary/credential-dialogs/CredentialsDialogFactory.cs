using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapWinLibrary;

public class CredentialsDialogFactory
{
    private readonly Dictionary<string, Type> _credDialogTypes = new( StringComparer.OrdinalIgnoreCase );
    private readonly ILogger? _logger;

    public CredentialsDialogFactory(
        ILoggerFactory? loggerFactory,
        params Assembly[] assemblies
    )
    {
        _logger = loggerFactory?.CreateLogger<CredentialsDialogFactory>();

        var assemblyList = assemblies.ToList();
        assemblyList.Add( GetType().Assembly );

        var dlgTypes = assemblyList.SelectMany( x => x.GetTypes() )
                                   .Where( t => t.IsAssignableTo( typeof( ContentDialog ) )
                                            && t.GetInterface( nameof( ICredentialsDialog ) ) != null
                                            && t.GetConstructors().Any( c => c.GetParameters().Length == 0 ) )
                                   .Select( t => new
                                    {
                                        DialogType = t,
                                        CredentialsAttribute =
                                            t.GetCustomAttribute<CredentialsDialogAttribute>()
                                    } )
                                   .Where( x => x.CredentialsAttribute != null );

        foreach( var dlgType in dlgTypes )
        {
            // find the projection type's name
            if( Activator.CreateInstance( dlgType.CredentialsAttribute!.CredentialsType )
               is not ICredentials credentials )
            {
                _logger?.LogWarning( "Could not create instance of {credType}, skipping",
                                     dlgType.CredentialsAttribute.CredentialsType );
                continue;
            }

            if( _credDialogTypes.ContainsKey( credentials.ProjectionName ) )
            {
                _logger?.LogWarning( "Duplicate ContentDialog type {dlgType} for {credType}, skipping",
                                     dlgType.DialogType,
                                     dlgType.CredentialsAttribute.CredentialsType );
                continue;
            }

            _credDialogTypes.Add( credentials.ProjectionName, dlgType.DialogType );
        }
    }

    public Type? this[ string projName ] => _credDialogTypes.TryGetValue( projName, out var retVal ) ? retVal : null;

    public ContentDialog? GetCredentialsDialogType( string projectionName )
    {
        if( _credDialogTypes.TryGetValue( projectionName, out var credentialType ) )
            return Activator.CreateInstance( credentialType ) as ContentDialog;

        _logger?.LogError( "{projection} does not have an associated {dialog}",
                           projectionName,
                           typeof( ContentDialog ) );
        return null;
    }
}
