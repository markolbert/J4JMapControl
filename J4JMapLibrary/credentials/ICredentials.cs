using System.ComponentModel;
using Microsoft.AspNetCore.DataProtection;

namespace J4JSoftware.J4JMapLibrary;

public interface ICredentials : INotifyPropertyChanged
{
    Type ProjectionType { get; }
    string ProjectionName { get; }
    IEnumerable<Credentials.CredentialProperty> CredentialProperties { get; }

    ICredentials Encrypt( IDataProtector protector);
    ICredentials Decrypt( IDataProtector protector );
}