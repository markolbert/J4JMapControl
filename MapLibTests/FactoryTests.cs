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
        var factory = J4JDeusEx.ServiceProvider.GetService<MapProjectionFactory>();
        factory.Should().NotBeNull();

        factory!.Search( assemblyType );

        factory.ProjectionTypes.Count.Should().Be( numResults );
    }

    [ Theory ]
    [InlineData("BingMaps", true)]
    [InlineData("BingMap", false)]
    [InlineData("OpenStreetMaps", true)]
    [InlineData("OpenTopoMaps", true)]
    public void CreateProjection( string projectionName, bool result )
    {
        var factory = J4JDeusEx.ServiceProvider.GetService<MapProjectionFactory>();
        factory.Should().NotBeNull();

        factory!.Search( typeof( MapProjectionFactory ) );

        var projection = factory.CreateMapProjection( projectionName );

        if( result )
            projection.Should().NotBeNull();
        else projection.Should().BeNull();
    }
}
