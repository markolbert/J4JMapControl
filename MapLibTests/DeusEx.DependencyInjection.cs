using Autofac;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MapLibTests;

internal partial class DeusEx
{
    private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
    {
        builder.Register( c =>
                {
                    var retVal = new ProjectionFactory( c.Resolve<IConfiguration>(),
                                                  c.Resolve<IJ4JLogger>() );

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

        //builder.Register( c =>
        //        {
        //            var config = c.Resolve<IConfiguration>();

        //            var retVal = new BingCredentials( string.Empty );
        //            var section = config.GetSection( "BingCredentials" ); 
        //            section.Bind( retVal );

        //            return retVal;
        //        } )
        //       .AsSelf()
        //       .SingleInstance();

        //builder.Register(c =>
        //        {
        //            var config = c.Resolve<IConfiguration>();

        //            var retVal = new GoogleCredentials(string.Empty, string.Empty);
        //            var section = config.GetSection("GoogleCredentials");
        //            section.Bind(retVal);

        //            return retVal;
        //        })
        //       .AsSelf()
        //       .SingleInstance();

        //builder.Register(c =>
        //        {
        //            var config = c.Resolve<IConfiguration>();

        //            var retVal = new OpenStreetCredentials();
        //            var section = config.GetSection("OpenStreetCredentials");
        //            section.Bind(retVal);

        //            return retVal;
        //        })
        //       .AsSelf()
        //       .SingleInstance();

        //builder.Register(c =>
        //        {
        //            var config = c.Resolve<IConfiguration>();

        //            var retVal = new OpenTopoCredentials();
        //            var section = config.GetSection("OpenTopoCredentials");
        //            section.Bind(retVal);

        //            return retVal;
        //        })
        //       .AsSelf()
        //       .SingleInstance();
    }
}
