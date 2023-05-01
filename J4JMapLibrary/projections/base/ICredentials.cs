using System.ComponentModel;

namespace J4JSoftware.J4JMapLibrary;

public interface ICredentials : INotifyPropertyChanged, IEnumerable<Credentials.CredentialProperty>
{
    Type ProjectionType { get; }
    string ProjectionName { get; }
}