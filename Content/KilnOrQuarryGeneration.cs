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
    public static ushort[] ReplaceableTiles = {
        TileID.Dirt,
        TileID.Grass,
        TileID.CorruptGrass,
        TileID.CrimsonGrass,
        TileID.Mud,
        TileID.ClayBlock,
        TileID.JungleGrass,
        TileID.Stone,
        TileID.Sand,
        TileID.HardenedSand
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
    public static Point GetPointFrom(Point p, int d = 0)
    {
        for (int k = 0; k < 5; k++)
        {
            if (d == 0) d = Main.rand.NextBool() ? 1 : -1;
            int MinDistance = 200;
            int MaxDistance = 300;
            p.X += d * Main.rand.Next(MinDistance, MaxDistance);

            if (Main.tileSolid[Main.tile[p].TileType])
            {
                while (WorldGen.SolidOrSlopedTile(Main.tile[p])) p.Y--;

                for (int i = 0; i < 100; i++) if (!Main.tileSolid[Main.tile[p].TileType] || !Main.tile[p].HasTile) p.Y++;
            }

            if (Main.tile[p].LiquidAmount == 0)
            {
                Point point1 = new(p.X - 20, p.Y);
                Point point2 = new(p.X + 20, p.Y);

                if (Math.Abs(point1.Grounded().Y - point2.Grounded().Y) < 10)
                    break;
            }
        }

        return p;
    }
}
