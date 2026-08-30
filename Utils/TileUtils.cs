using Terraria.ID;

namespace Everware.Utils;

public class TileUtils
{
    public struct Buffer(int t, int w, int fX, int fY, int fX2, int fY2, SlopeType slope, bool hT = false)
    {
        public int TileType = t;
        public int WallType = w;
        public int FrameX = fX;
        public int FrameY = fY;
        public int WallFrameX = fX2;
        public int WallFrameY = fY2;
        public SlopeType Slope = slope;
        public bool HalfTile = hT;
    }
}