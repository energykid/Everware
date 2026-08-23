using Everware.Utils;
using System.Threading;
using Terraria.ID;

namespace Everware.Content.Meteor;

public class MeteorGeneration
{
    public static void GenerateWholeSite(Point pt)
    {
        Thread thread = new Thread(() =>
        {
            GenerateCrater(pt);
        })
        {
            IsBackground = true,
        };
        thread.Start();
    }
    public static void GenerateMeteor(Point pt)
    {

    }
    public static void GenerateCrater(Point pt)
    {

        float impression1 = 0.05f;
        float impression2 = 0.15f;

        pt = pt.Grounded();

        int Border = 5;

        TileDataBuffer[,] tiles = new TileDataBuffer[Main.maxTilesX, Main.maxTilesY];

        for (int i = -SizeX - Border; i <= SizeX + Border; i++)
        {
            for (int j = -SizeY - Border; j <= SizeY + Border; j++)
            {
                Tile t = Main.tile[pt.X + i, pt.Y + j];
                tiles[pt.X + i, pt.Y + j] = new TileDataBuffer(
                    t.HasTile ? t.TileType : -1,
                    t.WallType,
                    t.TileFrameX, t.TileFrameY,
                    t.WallFrameX, t.WallFrameY,
                    t.Slope,
                    t.IsHalfBlock);
            }
        }

        for (int i = -SizeX; i <= SizeX; i++)
        {
            float x = 0;

            x = Easing.KeyFloat(i, -SizeX, -30, 0, 10, Easing.InOutExpo);
            x = Easing.KeyFloat(i, -30, -10, 10, 20, Easing.InExpo, x);
            x = Easing.KeyFloat(i, -10, 10, 20, 20, Easing.InOutExpo, x);
            x = Easing.KeyFloat(i, 10, 30, 20, 10, Easing.OutExpo, x);
            x = Easing.KeyFloat(i, 30, SizeX + 1, 10, 0, Easing.InOutExpo, x);
            for (float j = -SizeY; j <= SizeY + x; j++)
            {
                float l = (float)Math.Sin(i / 20f);

                Point pt1 = new(pt.X + i, pt.Y + (int)j);
                Point pt2 = new(pt.X + i, pt.Y + (int)(j - x));

                if (j + SizeX > x + 6)
                    ReplaceTile(tiles[pt2.X, pt2.Y], pt1, pt);
            }
        }

        for (int j = -SizeY - Border; j <= SizeY + Border + 40; j++)
        {
            for (int i = -SizeX - Border; i <= SizeX + Border; i++)
            {
                WorldGen.TileFrame(pt.X + i, pt.Y + j, true, noBreak: true);
                Tile.SmoothSlope(pt.X + i, pt.Y + j, sync: true);
            }
        }
    }
    public static void GenerateStar(Point pt)
    {

    }
    public static void GeneratePointZero(Point pt)
    {

    }
    public struct TileDataBuffer(int t, int w, int fX, int fY, int fX2, int fY2, SlopeType slope, bool hT = false)
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
    public static readonly int SizeX = 125;
    public static readonly int SizeY = 60;
    public static readonly int CharredSoil = TileID.Ash;
    public static readonly int MeteoricGrass = TileID.AshGrass;
    public static readonly int MeteoricGrassFoliage = TileID.AshPlants;
    public static readonly int CharredStone = TileID.Asphalt;
    public static void ReplaceTile(TileDataBuffer buffer, Point to, Point center)
    {
        int tt = buffer.TileType;

        Vector2 v = to.ToVector2();
        v.Y = MathHelper.Lerp(v.Y, center.ToVector2().Y, -0.75f);
        float k = 1f + (float)(Math.Sin(v.AngleTo(center.ToVector2()) * MathHelper.TwoPi) * 0.1f);
        if (v.Distance(center.ToVector2()) < SizeX * k)
        {
            if (tt != -1)
            {
                if (tt == TileID.Dirt || tt == TileID.ClayBlock) tt = CharredSoil;
                if (TileID.Sets.Grass[tt]) tt = TileID.AshGrass;
                if (tt == TileID.Plants || tt == TileID.Plants2) tt = MeteoricGrassFoliage;
                if (tt == TileID.Stone) tt = CharredStone;
                if (TileID.Sets.IsATreeTrunk[tt] || tt == TileID.LargePiles || tt == TileID.LargePiles2 || tt == TileID.SmallPiles || tt == TileID.Sunflower) tt = -1;
            }
        }

        if (tt != -1)
        {
            Main.tile[to].TileType = (ushort)tt;
        }
        else
        {
            Main.tile[to].ClearEverything();
        }
        Main.tile[to].TileFrameX = (short)buffer.FrameX;
        Main.tile[to].TileFrameY = (short)buffer.FrameY;
        Main.tile[to].WallType = (ushort)buffer.WallType;
        Main.tile[to].wallFrameX((short)buffer.WallFrameX);
        Main.tile[to].wallFrameY((short)buffer.WallFrameY);
        Main.tile[to].halfBrick(buffer.HalfTile);
        Main.tile[to].slope((byte)buffer.Slope);
        Main.tile[to].LiquidAmount = 0;
    }
}
