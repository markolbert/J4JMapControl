using System.Collections;
using System.Reflection;
using J4JMapLibrary;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;

namespace AutofacSupport;

public class SourceConfigurations : ISourceConfigurations
{
    public record SourceConfigurationInfo(string Name, bool HasConfigurationConstructor, Type SourceConfigurationType);

    private readonly Dictionary<string, SourceConfigurationInfo> _sources =
        new Dictionary<string, SourceConfigurationInfo>(StringComparer.OrdinalIgnoreCase);

    private readonly IJ4JLogger _logger;

    public SourceConfigurations(
        IJ4JLogger logger,
        bool includeThis = true, 
        params Assembly[] assemblies
    )
    {
        _logger = logger;
        _logger.SetLoggedType(GetType());

        var toScan = assemblies.ToList();

        if (includeThis)
            toScan.Add(GetType().Assembly);

        var typeTester =
            new ConstructorParameterTester<IMapProjection>(typeof(IJ4JLogger), typeof(ISourceConfiguration));

        foreach (var assembly in toScan.Distinct())
        {
            foreach (var projectionType in assembly.GetTypes()
                         .Where(t => t.IsAssignableTo(typeof(IMapProjection))))
            {
                var attributes = projectionType.GetCustomAttributes<MapProjectionAttribute>();
                var projName = attributes.FirstOrDefault()?.Name;
                if( string.IsNullOrEmpty(projName))
                    continue;

                var projName = attributes.First().Name;

                if (_sources.ContainsKey(projName))
                {
                    _logger.Warning<string>("Duplicate IMapProjection class '{0}', skipping",
                        projectionType.FullName ?? string.Empty);
                    
                    continue;
                }

                if (typeTester.MeetsRequirements(projectionType))
                    _sources.Add(attributes.First().Name,new SourceConfigurationInfo(attributes.First().Name, true, projectionType));
            }
        }
    }

    public SourceConfigurations(
        bool includeThis = true,
        params Type[] types
    )
        : this(includeThis, types.Select(x => x.Assembly).ToArray())
    {
    }

    public SourceConfigurations()
        : this(false, typeof(SourceConfigurations).Assembly)
    {
    }

    public IEnumerator<ISourceConfiguration> GetEnumerator()
    {
        foreach (var srcConfig in _sources)
        {
            yield return srcConfig.Value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}