using System.Reflection;

namespace J4JSoftware.J4JMapLibrary;

public interface ICredentialsFactory
{
    CredentialsFactoryBase ScanAssemblies( params Type[] types );
    CredentialsFactoryBase ScanAssemblies( params Assembly[] assemblies );
    bool InitializeFactory();
    ICredentials? this[ string projName ] { get; }
    ICredentials? this[ Type projType ] { get; }
}
