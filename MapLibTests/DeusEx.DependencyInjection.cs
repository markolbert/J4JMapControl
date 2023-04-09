using Autofac;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MapLibTests;

internal partial class DeusEx
{
    private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
    {
        builder.Register( c =>
                {
                    var retVal = new ProjectionFactory( c.Resolve<IConfiguration>(),
                                                        c.Resolve<ILoggerFactory>() );

                    retVal.InitializeFactory();

                    return retVal;
                } )
               .AsSelf()
               .SingleInstance();

        builder.Register( c => new MemoryCache( "In Memory", c.Resolve<ILoggerFactory>() ) )
               .AsSelf();

        builder.Register( c => new FileSystemCache( "File System", c.Resolve<ILoggerFactory>() ) )
               .AsSelf();
    }
}
