using Everware.Content.Base.World;
using Everware.Content.Meteor.Tiles;
using Everware.Utils;
using System.Threading;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Everware.Content.Meteor;

public class MeteorGeneration
{
    public static readonly int SizeX = 125;
    public static readonly int SizeY = 60;
    public static readonly int CharredSoil = ModContent.TileType<CharredSoilTile>();
    public static readonly int MeteoricGrass = ModContent.TileType<SpiritGrassTile>();
    public static readonly int MeteoricGrassFoliage = TileID.AshPlants;
    public static readonly int CharredStone = ModContent.TileType<CharredSoilTile>();
    public static readonly int MeteoriteOre = TileID.Meteorite;
    public static void GenerateWholeSite(Point pt)
    {
        Thread thread = new Thread(() =>
        {
            GenerateCrater(pt);
            GeneratePointZero(pt);
        })
        {
            IsBackground = true,
        };
        thread.Start();
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

            Point ptT = new(pt.X + i, pt.Y - SizeY - 1);

            if (TileID.Sets.IsATreeTrunk[Main.tile[ptT].TileType])
            {
                ushort t = Main.tile[ptT].TileType;
                while (Main.tile[ptT].TileType == t && Main.tile[ptT].HasTile && ptT.Y > 200)
                {
                    ptT.Y--;
                    Main.tile[ptT + new Point(0, 1)].ClearTile();
                }
            }

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
    public static void GeneratePointZero(Point pt)
    {
        new Shapes.Circle(30).Perform(pt, Actions.Chain(
            new Actions.SwapSolidTile((ushort)CharredStone),
            new Actions.Smooth(true)
        ));

        new Shapes.Slime(20, 0.06, Main.rand.NextFloat(0.2f, 0.7f)).Perform((pt + new Point(15, 0)).Grounded(), Actions.Chain(
            new CustomGenActions.SetMeteorFromGrass(),
            new Actions.Smooth(true)
        ));
        new Shapes.Slime(20, 0.06, Main.rand.NextFloat(0.1f, 0.4f)).Perform((pt + new Point(22, 0)).Grounded(), Actions.Chain(
            new CustomGenActions.SetMeteorFromGrass(),
            new Actions.Smooth(true)
        ));

        new Shapes.Slime(20, 0.06, Main.rand.NextFloat(0.2f, 0.7f)).Perform((pt + new Point(-15, 0)).Grounded(), Actions.Chain(
            new CustomGenActions.SetMeteorFromGrass(),
            new Actions.Smooth(true)
        ));
        new Shapes.Slime(20, 0.06, Main.rand.NextFloat(0.1f, 0.4f)).Perform((pt + new Point(-22, 0)).Grounded(), Actions.Chain(
            new CustomGenActions.SetMeteorFromGrass(),
            new Actions.Smooth(true)
        ));

        GenerateMeteor(pt);
    }
    public static void GenerateMeteor(Point pt)
    {
        pt = pt.Grounded();
        new Shapes.Slime(15, 1, 1.2).Perform(pt, Actions.Chain(
            new CustomGenActions.SetTileFromNone((ushort)MeteoriteOre),
            new Actions.Smooth(true)
        ));
    }
    public static void GenerateStar(Point pt)
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
                if (TileID.Sets.Grass[tt]) tt = MeteoricGrass;
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
