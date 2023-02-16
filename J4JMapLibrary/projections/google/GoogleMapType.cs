using System.ComponentModel;

namespace J4JMapLibrary;

public enum GoogleMapType
{
    [ Description( "Google (Roadmap)" ) ]
    RoadMap,

    [ Description( "Google (Satellite)" ) ]
    Satellite,

    [ Description( "Google (Terrain)" ) ]
    Terrain,

    [ Description( "Google (Hybrid)" ) ]
    Hybrid
}
