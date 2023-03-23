using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.J4JMapLibrary.region;
internal class Offset
{
    private Vector3 GetStaticOffset()

    {
        if (_curConfig == null || !_fragments.Any())
            return new Vector3();

        var boundingBox = _curConfig.GetBoundingBox(_projection);

        return new Vector3(-(boundingBox.Width - _curConfig.RequestedWidth) / 2,
                           -(boundingBox.Height - _curConfig.RequestedHeight) / 2,
                           0);
    }

    private Vector3 GetTiledOffset()
    {
        if (_curConfig == null || !_fragments.Any())
            return new Vector3();

        var tileHeightWidth = ((ITiledProjection)_projection).TileHeightWidth;

        var upperLeftTile = _fragments[0];

        var tiledPoint = new TiledPoint((ITiledProjection)_projection) { Scale = _curConfig.Scale };
        tiledPoint.SetLatLong(_curConfig.Latitude, _curConfig.Longitude);

        var xOffset = tiledPoint.X - _curConfig.RequestedWidth / 2 - upperLeftTile.XTile * tileHeightWidth;
        var yOffset = tiledPoint.Y - _curConfig.RequestedHeight / 2 - upperLeftTile.YTile * tileHeightWidth;

        return new Vector3(-xOffset, -yOffset, 0);
    }
}
