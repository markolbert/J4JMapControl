using FluentAssertions;
using J4JMapLibrary;

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
    [InlineData(typeof(BingMapsProjection), true)]
    [InlineData(typeof(OpenStreetMapsProjection), true)]
    [InlineData(typeof(OpenTopoMapsProjection), true)]
    public async void CreateProjectionFromType( Type type, bool result )
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

        var projTask = method.Invoke(factory, new object?[] { null, null }) as Task;
        projTask.Should().NotBeNull();
        await projTask!;

        var projection = projTask.GetType().GetProperty("Result")!.GetValue(projTask) as ITiledProjection;

        if (result)
            projection.Should().NotBeNull();
        else projection.Should().BeNull();
    }

    [Theory]
    [InlineData(typeof(BingMapsProjection),".jpeg")]
    [InlineData(typeof(OpenStreetMapsProjection), ".png")]
    [InlineData(typeof(OpenTopoMapsProjection), ".png")]
    public async void CheckImageFileExtension(Type type, string fileExtension)
    {
        var factory = GetFactory();
        factory.Should().NotBeNull();

        var methods = typeof(MapProjectionFactory)
                     .GetMethods()
                     .Where(x => x.Name.Equals("CreateMapProjection",
                                               StringComparison.OrdinalIgnoreCase)
                             && x.IsGenericMethod)
                     .ToList();

        methods.Count.Should().Be(1);

        var method = methods[0].MakeGenericMethod(type);

        var projTask = method.Invoke( factory, new object?[] { null, null } ) as Task;
        projTask.Should().NotBeNull();
        await projTask!;

        var projection = projTask.GetType().GetProperty("Result")!.GetValue(projTask) as ITiledProjection;
        projection!.ImageFileExtension.Should().BeEquivalentTo( fileExtension );
    }

}
