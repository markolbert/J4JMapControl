namespace J4JMapLibrary;

public record MapProjectionOptions(
    ISourceConfiguration? SourceConfiguration = null,
    string? Credentials = null,
    ITileCache? Cache = null,
    bool Authenticate = true);