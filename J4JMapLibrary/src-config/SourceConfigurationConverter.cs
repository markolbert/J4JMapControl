using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace J4JMapLibrary;

internal class SourceConfigurationConverter : JsonConverter<SourceConfiguration>
{
    private static readonly decimal DefaultMaxLatitude = (decimal) ( Math.Atan( Math.Sinh( Math.PI ) ) * 180 / Math.PI );

    private readonly Dictionary<string, object?> _properties = new( StringComparer.OrdinalIgnoreCase );

    private string? _propName;

    public override SourceConfiguration? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if( reader.TokenType != JsonTokenType.StartObject )
            throw new JsonException();

        _properties.Clear();

        var reading = true;

        while( reader.Read() && reading )
        {
            switch( reader.TokenType )
            {
                case JsonTokenType.EndObject:
                    reading = false;
                    break;

                case JsonTokenType.PropertyName:
                    if( !string.IsNullOrEmpty( _propName ) )
                        throw new JsonException("Consecutive property names encountered without intervening value");

                    _propName = reader.GetString();

                    break;

                case JsonTokenType.False:
                    AddPropertyValue(reader.GetBoolean());
                    break;

                case JsonTokenType.Number:
                    AddPropertyValue(reader.GetDecimal());
                    break;

                case JsonTokenType.String:
                    AddPropertyValue(reader.GetString());
                    break;

                case JsonTokenType.True:
                    AddPropertyValue(reader.GetBoolean());
                    break;
            }
        }

        return CreateSourceConfiguration();
    }

    private void AddPropertyValue(object? value)
    {
        if (string.IsNullOrEmpty(_propName))
            throw new JsonException("No property name defined when attempting to store value");

        if( _properties.ContainsKey(_propName))
            throw new JsonException("Duplicate property name encountered when attempting to store value");

        _properties.Add(_propName, value);
    }

    private SourceConfiguration? CreateSourceConfiguration()
    {
        if( !_properties.ContainsKey( "ConfigurationStyle" )
           || _properties["ConfigurationStyle"] is not string configStyle )
            return null;

        switch( configStyle.ToLower() )
        {
            case "dynamic":
                return CreateDynamicConfiguration();

            case "static":
                return CreateStaticConfiguration();
                break;

            default:
                throw new InvalidEnumArgumentException( $"Unsupported ConfigurationStyle '{configStyle}'" );
        }
    }

    private DynamicConfiguration? CreateDynamicConfiguration()
    {
        var copyrightUri = RetrieveProperty( "CopyrightUri", string.Empty );
        var maxLatitude = RetrieveProperty( "MaxLatitude", DefaultMaxLatitude );
        var minLatitude = RetrieveProperty("MinLatitude", -DefaultMaxLatitude);
        var maxLongitude = RetrieveProperty("MaxLongitude", 180d);
        var minLongitude = RetrieveProperty("MinLongitude", -180d);

        return new DynamicConfiguration
        {
            Name = RetrieveProperty( "Name", string.Empty ),
            ConfigurationStyle = ServerConfiguration.Dynamic,
            CredentialsRequired = RetrieveProperty( "CredentialsRequired", true ),
            Copyright = RetrieveProperty( "Copyright", string.Empty ),
            CopyrightUri = string.IsNullOrEmpty( copyrightUri ) ? null : new Uri( copyrightUri ),
            MaxLatitude = Convert.ToDouble( maxLatitude ),
            MinLatitude = Convert.ToDouble( minLatitude ),
            MaxLongitude = Convert.ToDouble( maxLongitude ),
            MinLongitude = Convert.ToDouble( minLongitude ),
            MetadataRetrievalUrl = RetrieveProperty( "MetadataRetrievalUrl", string.Empty )
        };
    }

    private StaticConfiguration? CreateStaticConfiguration()
    {
        var copyrightUri = RetrieveProperty( "CopyrightUri", string.Empty );
        var maxLatitude = RetrieveProperty( "MaxLatitude", DefaultMaxLatitude );
        var minLatitude = RetrieveProperty( "MinLatitude", -DefaultMaxLatitude );
        var maxLongitude = RetrieveProperty( "MaxLongitude", 180d );
        var minLongitude = RetrieveProperty( "MinLongitude", -180d );
        var minScale = RetrieveProperty( "MinScale", 0m );
        var maxScale = RetrieveProperty( "MaxScale", 0m );
        var widthHeight = RetrieveProperty( "TileHeightWidth", 0m );

        return new StaticConfiguration
        {
            Name = RetrieveProperty( "Name", string.Empty ),
            ConfigurationStyle = ServerConfiguration.Static,
            CredentialsRequired = RetrieveProperty( "CredentialsRequired", true ),
            Copyright = RetrieveProperty( "Copyright", string.Empty ),
            CopyrightUri = string.IsNullOrEmpty( copyrightUri ) ? null : new Uri( copyrightUri ),
            MaxLatitude = Convert.ToDouble( maxLatitude ),
            MinLatitude = Convert.ToDouble( minLatitude ),
            MaxLongitude = Convert.ToDouble( maxLongitude ),
            MinLongitude = Convert.ToDouble( minLongitude ),
            RetrievalUrl = RetrieveProperty( "RetrievalUrl", string.Empty ),
            MaxScale = Convert.ToInt32( maxScale ),
            MinScale = Convert.ToInt32( minScale ),
            TileHeightWidth = Convert.ToInt32( widthHeight )
        };
    }

    private T RetrieveProperty<T>( string key, T defaultValue )
        where T : notnull
    {
        if( !_properties.ContainsKey( key ) )
            return defaultValue;

        if(_properties[key] is T propValue )
            return propValue;

        return defaultValue;
    }

    public override void Write( Utf8JsonWriter writer, SourceConfiguration value, JsonSerializerOptions options )
    {
        if( value.ConfigurationStyle == null )
            throw new JsonException( "Undefined ConfigurationStyle" );

        writer.WriteStartObject();

        foreach (var propInfo in value.GetType().GetProperties())
        {
            var propValue = propInfo.GetValue(value);

            if (propValue != null)
                WriteProperty(writer, propInfo, propValue);
        }

        writer.WriteEndObject();
    }

    private void WriteProperty( Utf8JsonWriter writer, PropertyInfo propInfo, object value )
    {
        switch (Type.GetTypeCode(propInfo.PropertyType))
        {
            case TypeCode.Boolean:
                writer.WriteBoolean(propInfo.Name, (bool) value);
                break;

            case TypeCode.String:
                writer.WriteString( propInfo.Name, (string) value );
                break;

            case TypeCode.Int32:
                if( propInfo.PropertyType == typeof( ServerConfiguration ) )
                    writer.WriteString( propInfo.Name, value.ToString() );
                else
                    writer.WriteNumber( propInfo.Name, (int) value );

                break;

            case TypeCode.Double:
                writer.WriteNumber( propInfo.Name, (double) value );
                break;

            case TypeCode.Object:
                writer.WriteString(propInfo.Name, value.ToString());
                break;
        }
    }
}
