#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MapTests.cs
//
// This file is part of JumpForJoy Software's MapLibTests.
// 
// MapLibTests is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// MapLibTests is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with MapLibTests. If not, see <https://www.gnu.org/licenses/>.
#endregion

using FluentAssertions;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;

namespace MapLibTests;

public class MapTests : TestBase
{
    [ Fact ]
    public void ValidApiKey()
    {
        foreach( var projectionName in ProjectionNames )
        {
            var projection = CreateAndAuthenticateProjection( projectionName );
            projection.Should().NotBeNull();
            projection!.Initialized.Should().BeTrue();

            if( projection is not BingMapsProjection bingMaps )
                continue;

            bingMaps.Metadata!.PrimaryResource.Should().NotBeNull();
            bingMaps.Metadata.PrimaryResource!.ZoomMax.Should().Be( 21 );
            bingMaps.Metadata.PrimaryResource.ZoomMin.Should().Be( 1 );
            bingMaps.Metadata.PrimaryResource.ImageHeight.Should().Be( 256 );
            bingMaps.Metadata.PrimaryResource.ImageWidth.Should().Be( 256 );
        }
    }

    [ Theory ]
    [ InlineData( 500, true ) ]
    [ InlineData( 0, false ) ]
    [ InlineData( 1, false ) ]
    public async Task BingApiKeyLatency( int maxLatency, bool testResult )
    {
        var credentials = CredentialsFactory["BingMaps"] as BingCredentials;
        credentials.Should().NotBeNull();
        
        var projection = ProjectionFactory.CreateProjection("BingMaps") as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.MaxRequestLatency = maxLatency;

        projection.SetCredentials( credentials! ).Should().Be( true );

        if ( maxLatency is > 0 and < 10 )
        {
            await Assert.ThrowsAsync<TimeoutException>( async () => await projection.AuthenticateAsync() );
        }
        else
        {
            var initialized = await projection.AuthenticateAsync();
            initialized.Should().Be( testResult );
        }
    }

    [ Theory ]
    [InlineData(0, 0, 0, "0")] // scale can't be set to zero, so it gets overridden to 1
    [InlineData(1, 0, 0, "0")]
    [InlineData(1, 1, 0, "1")]
    [InlineData(1, 0, 1, "2")]
    [InlineData(1, 1, 1, "3")]
    [InlineData(2, 0, 0, "00")]
    [InlineData( 2, 1, 0, "01" )]
    [InlineData( 2, 0, 1, "02" )]
    [InlineData( 2, 1, 1, "03" )]
    [InlineData( 3, 0, 0, "000" )]
    [InlineData( 3, 1, 0, "001" )]
    [InlineData( 3, 2, 0, "010" )]
    [InlineData( 3, 3, 0, "011" )]
    [InlineData( 3, 0, 1, "002" )]
    [InlineData( 3, 1, 1, "003" )]
    [InlineData( 3, 2, 1, "012" )]
    [InlineData( 3, 3, 1, "013" )]
    [InlineData( 3, 0, 2, "020" )]
    [InlineData( 3, 1, 2, "021" )]
    [InlineData( 3, 2, 2, "030" )]
    [InlineData( 3, 3, 2, "031" )]
    [InlineData( 3, 0, 3, "022" )]
    [InlineData( 3, 1, 3, "023" )]
    [InlineData( 3, 2, 3, "032" )]
    [InlineData( 3, 3, 3, "033" )]
    public void BingMapsQuadKeys( int scale, int xTile, int yTile, string quadKey )
    {
        var projection = CreateAndAuthenticateProjection("BingMaps") as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var region = new MapRegion( projection, LoggerFactory )
                    .Scale( scale )
                    .Update();

        var mapTile = new MapTile(region, yTile).SetXAbsolute(xTile);
        mapTile.QuadKey.Should().Be( quadKey );
    }

    [Theory]
    [InlineData(0, 0, 0, "0")]
    [InlineData(1, 0, 0, "00")]
    [InlineData(1, 1, 0, "10")]
    [InlineData(1, 0, 1, "20")]
    [InlineData(1, 1, 1, "30")]
    [InlineData(2, 0, 0, "000")]
    [InlineData(2, 1, 0, "010")]
    [InlineData(2, 0, 1, "020")]
    [InlineData(2, 1, 1, "030")]
    [InlineData(3, 0, 0, "0000")]
    [InlineData(3, 1, 0, "0010")]
    [InlineData(3, 2, 0, "0100")]
    [InlineData(3, 3, 0, "0110")]
    [InlineData(3, 0, 1, "0020")]
    [InlineData(3, 1, 1, "0030")]
    [InlineData(3, 2, 1, "0120")]
    [InlineData(3, 3, 1, "0130")]
    [InlineData(3, 0, 2, "0200")]
    [InlineData(3, 1, 2, "0210")]
    [InlineData(3, 2, 2, "0300")]
    [InlineData(3, 3, 2, "0310")]
    [InlineData(3, 0, 3, "0220")]
    [InlineData(3, 1, 3, "0230")]
    [InlineData(3, 2, 3, "0320")]
    [InlineData(3, 3, 3, "0330")]
    public void OpenStreetMapsQuadKeys(int scale, int xTile, int yTile, string quadKey)
    {
        var projection = CreateAndAuthenticateProjection("OpenStreetMaps") as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var region = new MapRegion(projection, LoggerFactory)
                    .Scale(scale)
                    .Update();

        var mapTile = new MapTile(region, yTile).SetXAbsolute(xTile);
        mapTile.QuadKey.Should().Be(quadKey);
    }

    [Theory]
    [InlineData(0, 0, 0, "0")]
    [InlineData(1, 0, 0, "00")]
    [InlineData(1, 1, 0, "10")]
    [InlineData(1, 0, 1, "20")]
    [InlineData(1, 1, 1, "30")]
    [InlineData(2, 0, 0, "000")]
    [InlineData(2, 1, 0, "010")]
    [InlineData(2, 0, 1, "020")]
    [InlineData(2, 1, 1, "030")]
    [InlineData(3, 0, 0, "0000")]
    [InlineData(3, 1, 0, "0010")]
    [InlineData(3, 2, 0, "0100")]
    [InlineData(3, 3, 0, "0110")]
    [InlineData(3, 0, 1, "0020")]
    [InlineData(3, 1, 1, "0030")]
    [InlineData(3, 2, 1, "0120")]
    [InlineData(3, 3, 1, "0130")]
    [InlineData(3, 0, 2, "0200")]
    [InlineData(3, 1, 2, "0210")]
    [InlineData(3, 2, 2, "0300")]
    [InlineData(3, 3, 2, "0310")]
    [InlineData(3, 0, 3, "0220")]
    [InlineData(3, 1, 3, "0230")]
    [InlineData(3, 2, 3, "0320")]
    [InlineData(3, 3, 3, "0330")]
    public void OpenTopoMapsQuadKeys(int scale, int xTile, int yTile, string quadKey)
    {
        var projection = CreateAndAuthenticateProjection("OpenTopoMaps") as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var region = new MapRegion(projection, LoggerFactory)
                    .Scale(scale)
                    .Update();

        var mapTile = new MapTile(region, yTile).SetXAbsolute(xTile);
        mapTile.QuadKey.Should().Be(quadKey);
    }
}