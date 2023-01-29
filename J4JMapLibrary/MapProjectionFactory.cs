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
        ServerConfigurationStyle ServerConfigurationStyle,
        Type MapProjectionType
    );

    private readonly Dictionary<string, SourceConfigurationInfo> _sources = new(StringComparer.OrdinalIgnoreCase);

    private readonly IJ4JLogger _logger;
    private readonly ILibraryConfiguration? _libConfig;

    public MapProjectionFactory(
        ILibraryConfiguration libConfig,
        IJ4JLogger logger
    )
        : this(logger)
    {
        _libConfig = libConfig;
    }

    public MapProjectionFactory(
        IJ4JLogger logger
    )
    {
        _logger = logger;
        _logger.SetLoggedType(GetType());
    }

    public ReadOnlyCollection<Type> ProjectionTypes =>
        _sources.Select(x => x.Value.MapProjectionType)
            .ToList()
            .AsReadOnly();

    public ReadOnlyCollection<string> ProjectionNames =>
        _sources.Select(x => x.Key)
            .ToList()
            .AsReadOnly();

    public void Search(params Type[] types) => Search(types.Select(x => x.Assembly).ToArray());

    public void Search(params Assembly[] assemblies)
    {
        var toScan = assemblies.ToList();

        var publicCtors = new HasPublicConstructors<IMapProjection>();

        var libConfigTester = new ConstructorParameterTester<IMapProjection>(
            typeof(ILibraryConfiguration),
            typeof(IJ4JLogger),
            typeof(ITileCache));

        foreach (var assembly in toScan.Distinct())
        {
            foreach (var projectionType in assembly.GetTypes()
                         .Where(t => t.GetInterface(nameof(IMapProjection)) != null
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

                    var srcConfigTester = curAttr.ServerConfigurationStyle switch
                    {
                        ServerConfigurationStyle.Dynamic => new ConstructorParameterTester<IMapProjection>(
                            typeof(IDynamicConfiguration),
                            typeof(IJ4JLogger),
                            typeof(ITileCache)),
                        ServerConfigurationStyle.Static => new ConstructorParameterTester<IMapProjection>(
                            typeof(IStaticConfiguration),
                            typeof(IJ4JLogger),
                            typeof(ITileCache)),
                        _ => throw new InvalidEnumArgumentException(
                            $"Unsupported ServerConfigurationStyle '{curAttr.ServerConfigurationStyle}'")
                    };

                    var hasSrcCtor = srcConfigTester.MeetsRequirements(projectionType);
                    var hasLibCtor = libConfigTester.MeetsRequirements(projectionType);

                    if (!(hasLibCtor || hasSrcCtor))
                        continue;

                    if (srcConfigTester.MeetsRequirements(projectionType))
                        _sources.Add(curAttr.Name,
                            new SourceConfigurationInfo(curAttr.Name,
                                hasLibCtor,
                                curAttr.ServerConfigurationStyle,
                                projectionType));
                }
            }
        }
    }

    public async Task<TProj?> CreateMapProjection<TProj>(
        MapProjectionOptions? options = null
    )
        where TProj : class, ITiledProjection
    {
        var ctorInfo = _sources.Values
            .FirstOrDefault(x => x.MapProjectionType == typeof(TProj));

        if (ctorInfo != null)
            return (TProj?)await CreateMapProjectionInternal(ctorInfo, options);

        _logger.Error("{0} is not a known map projection type", typeof(TProj));
        return null;
    }

    public async Task<ITiledProjection?> CreateMapProjection(
        Type projectionType,
        MapProjectionOptions? options = null
    )
    {
        if (!projectionType.IsAssignableTo(typeof(TiledProjection)))
        {
            _logger.Error("{0} is not derived from {1}", projectionType, typeof(TiledProjection));
            return null;
        }

        var ctorInfo = _sources.Values
            .FirstOrDefault(x => x.MapProjectionType == projectionType);

        if (ctorInfo != null)
            return (ITiledProjection?)await CreateMapProjectionInternal(ctorInfo, options);

        _logger.Error("{0} is not a known map projection type", projectionType);
        return null;
    }

    public async Task<IMapProjection?> CreateMapProjection(
        string name,
        MapProjectionOptions? options = null
    )
    {
        var ctorInfo = GetSourceConfigInfo(name);
        if (ctorInfo == null)
            return null;

        return await CreateMapProjectionInternal(ctorInfo, options);
    }

    private async Task<IMapProjection?> CreateMapProjectionInternal(
        SourceConfigurationInfo ctorInfo,
        MapProjectionOptions? options = null
    )
    {
        options ??= new MapProjectionOptions();

        var retVal = options.SourceConfiguration switch
        {
            IDynamicConfiguration => CreateDynamicStatic(ctorInfo,
                options,
                ServerConfigurationStyle.Dynamic),

            IStaticConfiguration => CreateDynamicStatic(ctorInfo, options,
                ServerConfigurationStyle.Static),

            _ => CreateLibConfig(ctorInfo, options)
        };

        if (retVal == null)
            return retVal;

        var credentials = options.Credentials;

        if (string.IsNullOrEmpty(credentials))
        {
            if (_libConfig != null && !_libConfig.TryGetCredential(ctorInfo.Name, out credentials))
            {
                _logger.Information<string>(
                    "No credentials specified or found for map projection '{0}', could not authenticate",
                    ctorInfo.Name);

                return null;
            }
        }

        if (!options.Authenticate || await retVal.Authenticate(credentials))
            return retVal;

        _logger.Warning<string>("Authentication failed for map projection '{0}'", ctorInfo.Name);
        return retVal;
    }

    private IMapProjection? CreateDynamicStatic(SourceConfigurationInfo ctorInfo, MapProjectionOptions options,
        ServerConfigurationStyle serverConfig)
    {
        if (ctorInfo.ServerConfigurationStyle != serverConfig)
        {
            _logger.Error("'{0}' requires an I{1}Configuration argument", ctorInfo.MapProjectionType, serverConfig);
            return null;
        }

        try
        {
            if (Activator.CreateInstance(ctorInfo.MapProjectionType,
                    BindingFlags.CreateInstance | BindingFlags.Public,
                    null,
                    new object?[] { options.SourceConfiguration, _logger, options.Cache }) is IMapProjection retVal)
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

    private SourceConfigurationInfo? GetSourceConfigInfo(string name)
    {
        var retVal = _sources.ContainsKey(name) ? _sources[name] : null;

        if (retVal != null)
            return retVal;

        _logger.Error<string>("No '{0}' map projection class was found", name);
        return null;
    }

    private IMapProjection? CreateLibConfig(SourceConfigurationInfo ctorInfo, MapProjectionOptions options)
    {
        if (!ctorInfo.HasLibraryConfigurationConstructor)
        {
            _logger.Error("{0} has no public constructor accepting an ILibraryConfiguration parameter",
                ctorInfo.MapProjectionType);
            return null;
        }

        if (_libConfig == null)
        {
            _logger.Error("No ILibraryConfiguration was provided when the factory was created");
            return null;
        }

        if (!_libConfig.SourceConfigurations.Any(
                x => x.Name.Equals(ctorInfo.Name, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.Error<string>("No ISourceConfiguration was found for the name '{0}'", ctorInfo.Name);
            return null;
        }

        IMapProjection? retVal = null;

        try
        {
            retVal = Activator.CreateInstance(ctorInfo.MapProjectionType,
                new object?[] { _libConfig, _logger, options.Cache }) as IMapProjection;

            if (retVal == null)
                _logger.Error("Failed to create instance of {0}", ctorInfo.MapProjectionType);
        }
        catch (Exception ex)
        {
            _logger.Error<Type, string>("Failed to create instance of {0}, message was '{1}'",
                ctorInfo.MapProjectionType,
                ex.Message);
        }

        return retVal;
    }
}