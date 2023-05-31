using System.ComponentModel;

namespace J4JSoftware.J4JMapLibrary;

public interface ICredentials : INotifyPropertyChanged
{
    Type ProjectionType { get; }
    string ProjectionName { get; }
}