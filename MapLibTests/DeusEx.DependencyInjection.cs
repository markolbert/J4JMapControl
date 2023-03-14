using Autofac;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MapLibTests;

internal partial class DeusEx
{
    private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
    {
        builder.Register( c =>
                {
                    var retVal = new ProjectionFactory( c.Resolve<IConfiguration>(),
                                                  c.Resolve<ILogger>() );

                    retVal.ScanAssemblies();

                    return retVal;
                } )
               .AsSelf()
               .SingleInstance();

        builder.RegisterType<MemoryCache>()
               .AsSelf();

        builder.RegisterType<FileSystemCache>()
               .AsSelf();

        builder.RegisterType<NormalizedViewport>()
               .AsSelf();
    }
}
