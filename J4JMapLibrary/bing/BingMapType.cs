using System.ComponentModel;

namespace J4JMapLibrary;

public enum BingMapType
{
    [Description("Bing (Aerial)")]
    Aerial,

    [Description("Bing (Aerial w/Labels)")]
    AerialWithLabels,

    [Description("Bing (Roads)")]
    RoadOnDemand
}
