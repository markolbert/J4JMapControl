using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

public abstract partial class Projection
{
    protected object? Credentials { get; private set; }
    protected abstract bool ValidateCredentials(object credentials);

    public bool SetCredentials(object credentials)
    {
        if (ValidateCredentials(credentials))
            Credentials = credentials;

        return Credentials != null;
    }

    public bool Authenticate() => Task.Run(async () => await AuthenticateAsync()).Result;

#pragma warning disable CS1998
    public virtual async Task<bool> AuthenticateAsync(CancellationToken ctx = default)
#pragma warning restore CS1998
    {
        if (Credentials != null)
            return true;

        Logger?.LogError("Attempting to authenticate before setting credentials");
        return false;
    }

}
