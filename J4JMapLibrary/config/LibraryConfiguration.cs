using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace J4JMapLibrary;

public class LibraryConfiguration : ILibraryConfiguration
{
    private readonly IJ4JLogger? _logger;

    public LibraryConfiguration()
    {
        if( !J4JDeusEx.IsInitialized )
            return;

        _logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();
        _logger?.SetLoggedType( GetType() );
    }

    public bool IsInitialized { get; private set; }

    public bool Initialize( IConfiguration config )
    {
        var sourceIdx = 0;
        var kvPairs = config.AsEnumerable().ToList();
        var retVal = true;

        // ReSharper disable once AccessToModifiedClosure
        while( kvPairs.Any( x => x.Key.Equals( $"{nameof( SourceConfigurations )}:{sourceIdx}" ) ) )
        {
            var identifiedType = false;

            if( kvPairs.Any(

                   // ReSharper disable once AccessToModifiedClosure
                   x => x.Key.Equals(
                       $"{nameof( SourceConfigurations )}:{sourceIdx}:{nameof( DynamicConfiguration.MetadataRetrievalUrl )}" ) ) )
            {
                var dynamicConfig = new DynamicConfiguration();
                config.GetSection( $"{nameof( SourceConfigurations )}:{sourceIdx}" ).Bind( dynamicConfig );

                SourceConfigurations.Add( dynamicConfig );

                identifiedType = true;
            }

            if( kvPairs.Any(

                   // ReSharper disable once AccessToModifiedClosure
                   x => x.Key.Equals(
                       $"{nameof( SourceConfigurations )}:{sourceIdx}:{nameof( StaticConfiguration.RetrievalUrl )}" ) ) )
            {
                var staticConfig = new StaticConfiguration();

                config.GetSection( $"{nameof( SourceConfigurations )}:{sourceIdx}" ).Bind( staticConfig );
                SourceConfigurations.Add( staticConfig );

                identifiedType = true;
            }

            sourceIdx++;

            if( !identifiedType )
                _logger?.Error( "Failed to identify correct source configuration type derived from {0}",
                                typeof( SourceConfiguration ) );

            retVal &= identifiedType;
        }

        IsInitialized = retVal;

        return retVal;
    }

    public List<SourceConfiguration> SourceConfigurations { get; set; } = new();
    public List<Credential> Credentials { get; set; } = new();

    public bool TryGetConfiguration( string name, out SourceConfiguration? result )
    {
        result = SourceConfigurations
           .FirstOrDefault( x => x.Name.Equals( name, StringComparison.OrdinalIgnoreCase ) );

        return result != null;
    }

    public bool TryGetCredential(string name, out string? result)
    {
        result = Credentials
           .FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Key;

        return result != null;
    }

    public bool ValidateConfiguration()
    {
        if( SourceConfigurations.Any( x => string.IsNullOrEmpty( x.Name ) ) )
        {
            _logger?.Warning( "Eliminating map source configurations lacking a name" );
            SourceConfigurations = SourceConfigurations.Where( x => !string.IsNullOrEmpty( x.Name ) ).ToList();
        }

        var temp = SourceConfigurations.Distinct(SourceConfiguration.DefaultComparer).ToList();

        if( temp.Count != SourceConfigurations.Count )
        {
            _logger?.Warning("Eliminating duplicate map source configurations");
            SourceConfigurations = temp;
        }

        temp.Clear();

        foreach( var srcConfig in SourceConfigurations )
        {
            var isValid = srcConfig switch
            {
                StaticConfiguration staticConfig => ValidateConfiguration( staticConfig ),
                DynamicConfiguration dynamicConfig => ValidateConfiguration( dynamicConfig ),
                _ => throw new InvalidOperationException(
                    $"Unsupported SourceConfiguration type '{srcConfig.GetType()}'" )
            };

            if( isValid )
                temp.Add( srcConfig );
        }

        SourceConfigurations = temp;

        if( temp.Any() )
            return true;

        _logger?.Fatal("No valid map source configurations defined");
        return false;
    }

    private bool ValidateConfiguration( StaticConfiguration config )
    {
        if( config.MinScale < 0 )
        {
            _logger?.Error<string>("Minimum scale for {0} is < 0", config.Name  );
            return false;
        }

        if (config.MaxScale < 0)
        {
            _logger?.Error<string>("Maximum scale for {0} is < 0", config.Name);
            return false;
        }

        if( config.MinScale > config.MaxScale )
        {
            _logger?.Error( "Minimum scale ({0}) for {1} exceeds maximum scale ({2})",
                            config.MinScale,
                            config.Name,
                            config.MaxScale );
            return false;
        }

        if( config.TileHeightWidth <= 0 )
        {
            _logger?.Error<int, string>( "Tile width/height ({0}) for {1} <= 0",
                                         config.TileHeightWidth,
                                         config.Name );
            return false;
        }

        if( !ValidateCredentials( config ) )
            return false;

        if ( !string.IsNullOrEmpty( config.RetrievalUrl ) )
            return true;

        _logger?.Error<string>("Retrieval Url for {0} is undefined", config.Name);
        return false;
    }

    private bool ValidateConfiguration(DynamicConfiguration config)
    {
        if( !string.IsNullOrEmpty( config.MetadataRetrievalUrl ) )
            return ValidateCredentials( config );

        _logger?.Error<string>( "Metadata retrieval Url for {0} is undefined", config.Name );
        return false;
    }

    // assumes config.Name is not empty
    private bool ValidateCredentials( SourceConfiguration config )
    {
        if( !config.CredentialsRequired )
            return true;

        var credential =
            Credentials.FirstOrDefault( x => x.Name.Equals( config.Name, StringComparison.OrdinalIgnoreCase ) );

        if( credential == null )
        {
            _logger?.Error<string>( "No credentials defined for {0}", config.Name );
            return false;
        }

        if( !string.IsNullOrEmpty( credential.Key ) )
            return true;

        _logger?.Error<string>( "Undefined credential for {0}", config.Name );
        return false;
    }
}
