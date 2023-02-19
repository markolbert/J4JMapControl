using Autofac;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.Hosting;

namespace MapLibTests;

internal partial class DeusEx
{
    private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
    {
        builder.Register( _ => new ProjectionCredentials( hbc.Configuration ) )
               .As<IProjectionCredentials>()
               .SingleInstance();

        builder.RegisterType<MemoryCache>()
               .AsSelf();

        builder.RegisterType<FileSystemCache>()
               .AsSelf();

        builder.RegisterType<NormalizedViewport>()
               .AsSelf();
    }
}
