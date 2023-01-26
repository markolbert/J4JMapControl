using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class MapProjectionFactory
{
    private record SourceConfigurationInfo(
        string Name,
        bool HasLibraryConfigurationConstructor,
        ServerConfiguration ServerConfiguration,
        Type MapProjectionType
    );

    private readonly Dictionary<string, SourceConfigurationInfo> _sources = new( StringComparer.OrdinalIgnoreCase );

    private readonly IJ4JLogger _logger;
    private readonly ILibraryConfiguration? _libraryConfiguration;

    public MapProjectionFactory(
        ILibraryConfiguration libraryConfiguration,
        IJ4JLogger logger
    )
        : this( logger )
    {
        _libraryConfiguration = libraryConfiguration;
    }

    public MapProjectionFactory(
        IJ4JLogger logger
    )
    {
        _logger = logger;
        _logger.SetLoggedType( GetType() );
    }

    public void Search( params Type[] types ) => Search( types.Select( x => x.Assembly ).ToArray() );

    public void Search( params Assembly[] assemblies )
    {
        var toScan = assemblies.ToList();

        var publicCtors = new HasPublicConstructors<IMapProjection>();

        var libConfigTester =
            new ConstructorParameterTester<IMapProjection>(typeof(ILibraryConfiguration), typeof(IJ4JLogger));

        foreach (var assembly in toScan.Distinct())
        {
            foreach (var projectionType in assembly.GetTypes()
                                                   .Where(t => t.IsAssignableTo(typeof(IMapProjection))
                                                            && publicCtors.MeetsRequirements(t)))
            {
                foreach (var curAttr in projectionType.GetCustomAttributes<MapProjectionAttribute>())
                {
                    if (string.IsNullOrEmpty(curAttr.Name))
                        continue;

                    if (_sources.ContainsKey(curAttr.Name))
                    {
                        _logger.Warning<string>("Duplicate IMapProjection class '{0}', skipping",
                                                 projectionType.FullName ?? string.Empty);

                        continue;
                    }

                    var srcConfigTester = curAttr.ServerConfiguration switch
                    {
                        ServerConfiguration.Dynamic => new ConstructorParameterTester<IMapProjection>(
                            typeof( IDynamicConfiguration ),
                            typeof( IJ4JLogger ) ),
                        ServerConfiguration.Static => new ConstructorParameterTester<IMapProjection>(
                            typeof( IStaticConfiguration ),
                            typeof( IJ4JLogger ) ),
                        _ => throw new InvalidEnumArgumentException(
                            $"Unsupported ServerConfiguration '{curAttr.ServerConfiguration}'" )
                    };

                    var hasSrcCtor = srcConfigTester.MeetsRequirements(projectionType);
                    var hasLibCtor = libConfigTester.MeetsRequirements(projectionType);

                    if (!(hasLibCtor || hasSrcCtor))
                        continue;

                    if (srcConfigTester.MeetsRequirements(projectionType))
                        _sources.Add(curAttr.Name,
                                      new SourceConfigurationInfo(curAttr.Name,
                                                                   hasLibCtor,
                                                                   curAttr.ServerConfiguration,
                                                                   projectionType));
                }
            }
        }
    }

    public IMapProjection? CreateMapProjection( string name, ISourceConfiguration? srcConfig = null )
    {
        return srcConfig switch
        {
            IDynamicConfiguration dynamicConfig => CreateDynamicStatic( name, srcConfig, ServerConfiguration.Dynamic ),
            IStaticConfiguration staticConfig => CreateDynamicStatic( name, srcConfig, ServerConfiguration.Static ),
            _ => CreateLibConfig( name )
        };
    }

    private IMapProjection? CreateDynamicStatic( string name, ISourceConfiguration config, ServerConfiguration serverConfig )
    {
        var ctorInfo = _sources.ContainsKey(name) ? _sources[name] : null;
        if( ctorInfo == null )
        {
            _logger.Error<string>("No '{0}' map projection class was found", name);
            return null;
        }

        if( ctorInfo.ServerConfiguration != serverConfig )
        {
            _logger.Error("'{0}' requires an I{1}Configuration argument", ctorInfo.MapProjectionType, serverConfig);
            return null;
        }

        try
        {
            if( Activator.CreateInstance( ctorInfo.MapProjectionType,
                                          BindingFlags.CreateInstance | BindingFlags.Public,
                                          null,
                                          new object[] { config, _logger } ) is IMapProjection retVal )
                return retVal;

            _logger.Error( "Failed to create instance of {0}", ctorInfo.MapProjectionType );
        }
        catch( Exception ex )
        {
            _logger.Error<Type, string>( "Failed to create instance of {0}, message was '{1}'",
                                   ctorInfo.MapProjectionType,
                                   ex.Message );
        }

        return null;
    }

    private IMapProjection? CreateLibConfig( string name )
    {
        var ctorInfo = _sources.ContainsKey(name) ? _sources[name] : null;
        if (ctorInfo == null)
        {
            _logger.Error<string>("No '{0}' map projection class was found", name);
            return null;
        }

        if( !ctorInfo.HasLibraryConfigurationConstructor )
        {
            _logger.Error("{0} has no public constructor accepting an ILibraryConfiguration parameter", ctorInfo.MapProjectionType);
            return null;
        }

        if ( _libraryConfiguration == null )
        {
            _logger.Error("No ILibraryConfiguration was provided when the factory was created");
            return null;
        }

        if( !_libraryConfiguration.SourceConfigurations.Any(
               x => x.Name.Equals( name, StringComparison.OrdinalIgnoreCase ) ) )
        {
            _logger.Error<string>( "No ISourceConfiguration was found for the name '{0}'", name );
            return null;
        }

        try
        {
            if (Activator.CreateInstance(ctorInfo.MapProjectionType,
                                         new object[] { _libraryConfiguration, _logger }) is IMapProjection retVal)
                return retVal;

            _logger.Error("Failed to create instance of {0}", ctorInfo.MapProjectionType);
        }
        catch (Exception ex)
        {
            _logger.Error<Type, string>("Failed to create instance of {0}, message was '{1}'",
                                        ctorInfo.MapProjectionType,
                                        ex.Message);
        }

        return null;
    }

    public bool TryGetCredentials( string name, out string? result )
    {
        result = null;

        if (_libraryConfiguration == null)
        {
            _logger.Error("No ILibraryConfiguration was provided when the factory was created");
            return false;
        }

        result = _libraryConfiguration.Credentials
                                      .FirstOrDefault( x => x.Name.Equals( name, StringComparison.OrdinalIgnoreCase ) )
                                     ?.Key;

        if( result == null )
            _logger.Error<string>("No credentials for '{0}' were found", name);

        return result != null;
    }

    public ReadOnlyCollection<Type> ProjectionTypes =>
        _sources.Select( x => x.Value.MapProjectionType )
                .ToList()
                .AsReadOnly();
}