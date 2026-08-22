using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Everware.Content.Gallery.Sculptor;

public class GenerateFrozenSculptor : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        tasks.Add(new PassLegacy("Adding the frozen Sculptor", delegate (GenerationProgress progress, GameConfiguration configuration)
        {
            int snowDirection = 0;

            bool success = false;

            Point pos = new Point(Main.spawnTileX, (int)Main.worldSurface + Main.rand.Next(100, 300));

            for (int i = 0; i < Main.maxTilesX / 2; i++)
            {
                Point p1 = pos + new Point(i, 0);
                Point p2 = pos + new Point(-i, 0);

                if (p1.X > 100 && p1.X < Main.maxTilesX - 100)
                {
                    if (Main.tile[p1].TileType == TileID.SnowBlock || Main.tile[p1].TileType == TileID.IceBlock)
                    {
                        pos = p1;
                        snowDirection = 1;
                        success = true;
                        break;
                    }
                }

                if (p2.X > 100 && p2.X < Main.maxTilesX - 100)
                {
                    if (Main.tile[p2].TileType == TileID.SnowBlock || Main.tile[p2].TileType == TileID.IceBlock)
                    {
                        pos = p2;
                        snowDirection = -1;
                        success = true;
                        break;
                    }
                }
            }

            GalleryGeneration.Everware025_SpawnFrozenSculptor(pos + new Point(snowDirection * 160, 0));
        }));
    }
}
