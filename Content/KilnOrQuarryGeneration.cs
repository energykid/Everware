using Everware.Content.Kiln;
using Everware.Content.Quarry;
using Everware.Utils;
using System;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Everware.Content;

public class KilnOrQuarryGeneration : ModSystem
{
    public static ushort[] IrreplaceableTiles = {
        TileID.LivingWood,
        TileID.LeafBlock
    };
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        tasks.Add(new PassLegacy("Generating an abandoned processing site", delegate (GenerationProgress progress, GameConfiguration configuration)
        {
            Point spawn = new(Main.spawnTileX, Main.spawnTileY);
            if (!Main.drunkWorld)
            {
                if (Main.rand.NextBool())
                    KilnGenerator.GenerateKiln(GetPointFrom(spawn));
                else
                    QuarryGenerator.GenerateQuarry(GetPointFrom(spawn));
            }
            else
            {
                KilnGenerator.GenerateKiln(GetPointFrom(spawn, 1));
                QuarryGenerator.GenerateQuarry(GetPointFrom(spawn, -1));
            }
        }));
    }
    public static Point GetPointFrom(Point p, int d = 0, int dist = 0)
    {
        Point refP = p;

        for (int k = 0; k < 10; k++)
        {
            p = refP;
            if (d == 0) d = Main.rand.NextBool() ? 1 : -1;
            int MinDistance = 200;
            int MaxDistance = 300;
            if (dist == 0)
                p.X += d * Main.rand.Next(MinDistance, MaxDistance);
            else
                p.X += d * Main.rand.Next(0, dist);

            if (Main.tileSolid[Main.tile[p].TileType])
            {
                while (WorldGen.SolidOrSlopedTile(Main.tile[p])) p.Y--;

                for (int i = 0; i < 100; i++) if (!Main.tileSolid[Main.tile[p].TileType] || !Main.tile[p].HasTile) p.Y++;
            }

            if (Main.tile[p].LiquidAmount == 0)
            {
                List<Point> points = [];
                for (int a = -20; a <= 20; a += 2)
                {
                    points.Add(new Point(p.X + a, p.Y).Grounded());
                }

                List<int> positions = [];
                for (int a = 0; a < points.Count; a++)
                {
                    positions.Add(points[a].Y);
                }

                positions.Sort();

                if (GetNumSolidTiles(new Rectangle(p.X - 15, p.Y - 10, 30, 10)) < 30)
                {
                    if (Math.Abs(positions[0] - positions[positions.Count - 1]) < 5)
                        break;
                }
            }
        }

        return p;
    }
    public static int GetNumSolidTiles(Rectangle rect)
    {
        int tiles = 0;
        for (int i = rect.X; i < rect.X + rect.Width; i++)
        {
            for (int j = rect.Y; j < rect.Y + rect.Height; j++)
            {
                if (Main.tile[i, j].HasTile && Main.tileSolid[Main.tile[i, j].TileType])
                {
                    tiles++;
                }
            }
        }
        return tiles;
    }
}
