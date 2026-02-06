using Everware.Content.PreHardmode.Kiln;
using Everware.Content.PreHardmode.Quarry;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Everware.Content.PreHardmode;

public class KilnOrQuarryGeneration : ModSystem
{
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
        if (d == 0) d = Main.rand.NextBool() ? 1 : -1;
        int MinDistance = 200;
        int MaxDistance = 300;
        p.X += d * Main.rand.Next(MinDistance, MaxDistance);

        if (Main.tileSolid[Main.tile[p].TileType])
        {
            while (WorldGen.SolidOrSlopedTile(Main.tile[p])) p.Y--;

            for (int i = 0; i < 100; i++) if (!Main.tileSolid[Main.tile[p].TileType] || !Main.tile[p].HasTile) p.Y++;
        }

        return p;
    }
}
