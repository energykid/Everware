using Everware.Content.Base.World;
using Everware.Content.Meteor.Tiles;
using Everware.Utils;
using System.Collections.Generic;
using System.Threading;
using Terraria.ID;
using Terraria.WorldBuilding;
using static Everware.Utils.PathfindingUtils;

namespace Everware.Content.Meteor;

public class MeteorGeneration
{
    public static readonly int SizeX = 125;
    public static readonly int SizeY = 60;
    public static readonly int CharredSoilWall = ModContent.WallType<CharredSoilWall>();
    public static readonly int CharredSoil = ModContent.TileType<CharredSoilTile>();
    public static readonly int StarCrossedGrass = ModContent.TileType<StarCrossedGrassTile>();
    public static readonly int StarCrossedGrassFoliage = TileID.AshPlants;
    public static readonly int MagicStone = ModContent.TileType<MagicStoneTile>();
    public static readonly int MeteoriteOre = ModContent.TileType<Meteorite>();
    public static List<int> BlacklistedBlocks => [
        TileID.BlueDungeonBrick,
        TileID.PinkDungeonBrick,
        TileID.GreenDungeonBrick,
        TileID.LivingWood,
        TileID.LeafBlock
    ];
    public static void GenerateWholeSite(out Point outputPosition)
    {
        Point pt = GetMeteorPosition(1000, Main.maxTilesX / 6, Main.maxTilesX / 3, fromLeft: true);
        Thread thread = new Thread(() =>
        {
            GenerateCrater(pt);
            GeneratePointZero(pt);
        })
        {
            IsBackground = true,
        };
        thread.Start();
        outputPosition = pt;
    }
    public static void GenerateCrater(Point pt)
    {
        float impression1 = 0.05f;
        float impression2 = 0.15f;

        pt = pt.Grounded();

        int Border = 5;

        TileUtils.Buffer[,] tiles = new TileUtils.Buffer[Main.maxTilesX, Main.maxTilesY];

        for (int i = -SizeX - Border; i <= SizeX + Border; i++)
        {
            for (int j = -SizeY - Border; j <= SizeY + Border; j++)
            {
                Tile t = Main.tile[pt.X + i, pt.Y + j];
                tiles[pt.X + i, pt.Y + j] = new TileUtils.Buffer(
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

            x = Easing.KeyFloat(i, -30, -13, 0, 12, Easing.InExpo, x);
            x = Easing.KeyFloat(i, -13, 13, 12, 12, Easing.InOutExpo, x);
            x = Easing.KeyFloat(i, 13, 30, 12, 0, Easing.OutExpo, x);

            for (float j = -SizeY; j <= SizeY + x; j++)
            {
                Point ptT = new(pt.X + i, pt.Y + (int)j);

                if (TileID.Sets.IsATreeTrunk[Main.tile[ptT].TileType])
                {
                    ushort t = Main.tile[ptT].TileType;
                    while (Main.tile[ptT].TileType == t && Main.tile[ptT].HasTile && ptT.Y > 200)
                    {
                        ptT.Y--;
                        Point pp = ptT + new Point(0, 1);
                        Tile tt = Main.tile[pp];
                        tt.HasTile = false;
                    }
                }

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
        new Shapes.Circle(35).Perform(pt, Actions.Chain(
            new CustomGenActions.SetCharredSoil(),
            new Actions.Smooth(true)
        ));

        new Shapes.Slime(20, 0.06, Main.rand.NextFloat(0.2f, 0.7f)).Perform((pt + new Point(15, 0)).Grounded() + new Point(0, 3), Actions.Chain(
            new CustomGenActions.SetMeteorFromGrass(),
            new Actions.Smooth(true)
        ));

        new Shapes.Slime(20, 0.06, Main.rand.NextFloat(0.2f, 0.7f)).Perform((pt + new Point(-15, 0)).Grounded() + new Point(0, 3), Actions.Chain(
            new CustomGenActions.SetMeteorFromGrass(),
            new Actions.Smooth(true)
        ));

        for (int i = -7; i <= 7; i++)
        {
            if (Math.Abs(i) >= 4)
            {
                Point center = pt + new Point(i * 10, 0) + new Point(Main.rand.Next(-3, 3), 0);
                new Shapes.Slime(20, Main.rand.NextFloat(0.1f, 0.2f), Main.rand.NextFloat(0.12f, 0.4f)).Perform(center.Grounded(), Actions.Chain(
                    new Actions.SetTileKeepWall((ushort)MagicStone, true),
                    new Actions.Smooth(true)
                ));
            }
        }

        GenerateMeteor(pt);
    }
    public static void GenerateMeteor(Point pt)
    {
        pt = pt.Grounded();
        new Shapes.Slime(20, 1, 1.2).Perform(pt, Actions.Chain(
            new CustomGenActions.SetTileFromNone((ushort)MeteoriteOre),
            new Actions.Smooth(true)
        ));
    }
    public static void GenerateStar(Point pt)
    {

    }
    public static void ReplaceTile(TileUtils.Buffer buffer, Point to, Point center)
    {
        int tt1 = buffer.TileType;
        int tt = buffer.TileType;
        int ww = buffer.WallType;

        bool bb = true;

        Vector2 v = to.ToVector2();
        v.Y = MathHelper.Lerp(v.Y, center.ToVector2().Y, -0.75f);
        float k = 1f + (float)(Math.Sin(v.AngleTo(center.ToVector2()) * MathHelper.TwoPi) * 0.1f);
        if (v.Distance(center.ToVector2()) < SizeX * k)
        {
            if (v.Distance(center.ToVector2()) > ((SizeX * k) - 6)) bb = Main.rand.NextBool((int)(1 + (v.Distance(center.ToVector2()) - ((SizeX * k) - 6))));

            if (bb)
            {
                if (tt != -1)
                {
                    if (!BlacklistedBlocks.Contains(tt1)) tt = CharredSoil;
                    if (TileID.Sets.Grass[tt1] || tt1 == TileID.JungleGrass) tt = StarCrossedGrass;
                    if (TileID.Sets.Stone[tt1]) tt = CharredSoil;

                    if (!Main.tileSolid[tt1])
                        tt = -1;
                    else
                        if (TileID.Sets.IsATreeTrunk[tt1] || tt1 == TileID.LargePiles || tt1 == TileID.LargePiles2 || tt1 == TileID.SmallPiles || tt1 == TileID.Sunflower) tt = -1;
                }

                if (ww != WallID.None) ww = CharredSoilWall;
            }
        }

        Tile t = Main.tile[to];
        if (tt != -1)
        {
            t.TileType = (ushort)tt;
            t.TileFrameX = (short)buffer.FrameX;
            t.TileFrameY = (short)buffer.FrameY;
            t.halfBrick(buffer.HalfTile);
            t.slope((byte)buffer.Slope);
        }
        else
        {
            t.HasTile = false;
        }
        t.WallType = (ushort)ww;
        t.wallFrameX((short)buffer.WallFrameX);
        t.wallFrameY((short)buffer.WallFrameY);
        t.LiquidAmount = 0;
    }
    public static Point GetMeteorPosition(int numChecks = 10, int minDist = 50, int maxDist = 250, bool fromLeft = true)
    {
        Point p = new Point(Main.maxTilesX / 2, (int)Main.worldSurface - 200).Grounded();
        Point refP = new Point(p.X, p.Y);

        for (int j = 0; j < 10; j++)
        {
            for (int i = 0; i <= numChecks; i++)
            {
                if (i < numChecks)
                {
                    refP = new Point(p.X, p.Y)
                    {
                        X = (int)MathHelper.Lerp(0, Main.maxTilesX, (float)i / numChecks)
                    };
                    if (!fromLeft)
                    {
                        refP.X = (int)MathHelper.Lerp(Main.maxTilesX, 0, (float)i / numChecks);
                    }
                    refP.X += Main.rand.Next(-200, 200);
                    if (Math.Abs(refP.X - p.X) > minDist && Math.Abs(refP.X - p.X) < maxDist)
                    {
                        refP = refP.Grounded();

                        int slope = new FlatnessCheck(refP, new Point(160, 20), 2).ApproximateTerrainFlatness();

                        if (slope < (100 - (j * 5)))
                        {
                            bool skip = false;
                            {
                                for (int k = 0; k < BlacklistedBlocks.Count; k++)
                                {
                                    if (new TileCheck(new Rectangle(refP.X - 160, refP.Y - 20, 320, 40)).IsTileInside(BlacklistedBlocks[k]))
                                    {
                                        skip = true;
                                        break;
                                    }
                                }
                            }
                            if (!skip)
                            {
                                p = refP;
                                break;
                            }
                        }
                    }
                }
            }
            if (p == refP) break;
        }

        return p;
    }
}
