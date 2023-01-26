using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using J4JMapLibrary;
using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;

namespace MapLibTests;

public class FactoryTests : TestBase
{
    [Theory]
    [InlineData(typeof(MapProjectionFactory), 3)]
    [InlineData(typeof(string), 0)]
    public void FindsProjectionsByType( Type assemblyType, int numResults )
    {
        var factory = GetFactory(false);
        factory.Should().NotBeNull();

        factory!.Search( assemblyType );

        factory.ProjectionTypes.Count.Should().Be( numResults );
    }

    [ Theory ]
    [InlineData("BingMaps", true)]
    [InlineData("OpenStreetMaps", true)]
    [InlineData("OpenTopoMaps", true)]
    public void CreateProjectionFromName( string projectionName, bool result )
    {
        var factory = GetFactory();
        factory.Should().NotBeNull();

        var projection = factory.CreateMapProjection( projectionName );

        if( result )
            projection.Should().NotBeNull();
        else projection.Should().BeNull();
    }

    [ Theory ]
    [InlineData(typeof(BingMapProjection), true)]
    [InlineData(typeof(OpenStreetMapsProjection), true)]
    [InlineData(typeof(OpenTopoMapsProjection), true)]
    public void CreateProjectionFromType( Type type, bool result )
    {
        var factory = GetFactory();
        factory.Should().NotBeNull();

        var methods = typeof( MapProjectionFactory )
                        .GetMethods()
                        .Where( x => x.Name.Equals( "CreateMapProjection",
                                                    StringComparison.OrdinalIgnoreCase ) 
                                && x.IsGenericMethod )
                        .ToList();

        methods.Count.Should().Be( 1 );

        var method = methods[ 0 ].MakeGenericMethod( type );

        var projection = method.Invoke( factory, new object?[] { null, null } );

        if (result)
            projection.Should().NotBeNull();
        else projection.Should().BeNull();
    }
}
