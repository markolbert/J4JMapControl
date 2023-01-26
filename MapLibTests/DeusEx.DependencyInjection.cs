using Autofac;
using J4JMapLibrary;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MapLibTests;

internal partial class DeusEx
{
    private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
    {
        builder.Register(c =>
                {
                    var retVal = c.TryResolve<ILibraryConfiguration>( out var libConfig )
                        ? new MapProjectionFactory( libConfig, c.Resolve<IJ4JLogger>() )
                        : new MapProjectionFactory( c.Resolve<IJ4JLogger>() );

                    return retVal;
                })
               .AsSelf();

        builder.Register( _ =>
                {
                    LibraryConfiguration? config;

                    try
                    {
                        // this will ignore the SourceConfiguration entries because
                        // they're polymorphic, so we go back afterwards and add them
                        config = hbc.Configuration.Get<LibraryConfiguration>();
                    }
                    catch
                    {
                        config = new LibraryConfiguration();
                    }

                    var sourceIdx = 0;
                    var keyValuePairs = hbc.Configuration.AsEnumerable().ToList();

                    // ReSharper disable once AccessToModifiedClosure
                    while( keyValuePairs.Any( x => x.Key.Equals( $"SourceConfigurations:{sourceIdx}" ) ) )
                    {
                        if( keyValuePairs.Any(
                               // ReSharper disable once AccessToModifiedClosure
                               x => x.Key.Equals( $"SourceConfigurations:{sourceIdx}:MetadataRetrievalUrl" ) ) )
                        {
                            var dynamicConfig = new DynamicConfiguration();
                            hbc.Configuration.GetSection( $"SourceConfigurations:{sourceIdx}" ).Bind( dynamicConfig );
                            config!.SourceConfigurations.Add( dynamicConfig );
                        }
                        else
                        {
                            var staticConfig = new StaticConfiguration();
                            hbc.Configuration.GetSection( $"SourceConfigurations:{sourceIdx}" ).Bind( staticConfig );
                            config!.SourceConfigurations.Add( staticConfig );
                        }

                        sourceIdx++;
                    }

                    return config!;
                } )
               .As<ILibraryConfiguration>()
               .SingleInstance();
    }
}
