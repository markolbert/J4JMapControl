using Autofac;
using J4JMapLibrary;
using J4JMapLibrary.MapBuilder;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace MapLibTests;

internal partial class DeusEx
{
    private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
    {
        builder.Register(c =>
                {
                    var retVal = c.TryResolve<IProjectionCredentials>( out var libConfig )
                        ? new ProjectionFactory( libConfig, c.Resolve<IJ4JLogger>() )
                        : new ProjectionFactory( c.Resolve<IJ4JLogger>() );

                    return retVal;
                })
               .AsSelf();

        builder.Register( _ => new ProjectionCredentials( hbc.Configuration ) )
               .As<IProjectionCredentials>()
               .SingleInstance();

        builder.RegisterType<MemoryCache>()
               .AsSelf();

        builder.RegisterType<FileSystemCache>()
               .AsSelf();

        builder.RegisterType<NormalizedViewport>()
               .AsSelf();

        builder.RegisterType<ProjectionFactory>()
               .AsSelf();

        builder.RegisterType<ProjectionBuilder>()
               .AsSelf();
    }
}
