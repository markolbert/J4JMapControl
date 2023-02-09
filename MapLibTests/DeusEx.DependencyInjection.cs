using Autofac;
using J4JMapLibrary;
using J4JMapLibrary.MapBuilder;
using J4JMapLibrary.Viewport;
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
                    var retVal = c.TryResolve<IProjectionCredentials>( out var libConfig )
                        ? new ProjectionFactory( libConfig, c.Resolve<IJ4JLogger>() )
                        : new ProjectionFactory( c.Resolve<IJ4JLogger>() );

                    return retVal;
                })
               .AsSelf();

        builder.Register( _ =>
                {
                    ProjectionCredentials? config;

                    try
                    {
                        // this will ignore the SourceConfiguration entries because
                        // they're polymorphic, so we go back afterwards and add them
                        config = hbc.Configuration.Get<ProjectionCredentials>();
                    }
                    catch
                    {
                        config = new ProjectionCredentials();
                    }

                    if( config != null )
                        return config;

                    Logger?.Fatal("Failed to initialize ILibraryConfiguration");
                    throw new ApplicationException( "Failed to initialize ILibraryConfiguration" );
                } )
               .As<IProjectionCredentials>()
               .SingleInstance();

        builder.RegisterType<MemoryCache>()
               .AsSelf();

        builder.RegisterType<FileSystemCache>()
               .AsSelf();

        builder.RegisterType<Viewport>()
               .AsSelf();

        builder.RegisterType<ProjectionFactory>()
               .AsSelf();

        builder.RegisterType<ProjectionBuilder>()
               .AsSelf();
    }
}
