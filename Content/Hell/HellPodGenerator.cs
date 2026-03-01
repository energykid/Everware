

using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Everware.Content.Hell;

public class HellPodGenerator : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        tasks.Add(new PassLegacy("Generating hell pods", delegate (GenerationProgress progress, GameConfiguration configuration)
        {
            int amountOfPods = 100;

            for (int i = 0; i < amountOfPods; i++)
            {
                int X = Main.rand.Next(300, Main.maxTilesX - 300);

                Point spawn = new Point(X, Main.rand.Next(Main.UnderworldLayer + 25, Main.maxTilesY - 130));

                bool shouldPlace = true;

                for (int x = 0; x < 3; x++)
                {
                    for (int y = -2; y <= 5; y++)
                    {
                        if (Main.tile[spawn.X + x, spawn.Y + y].HasTile)
                        {
                            shouldPlace = false;
                            break;
                        }
                    }
                }
                if (shouldPlace)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            Main.tile[spawn.X + x, spawn.Y + y].ResetToType((ushort)ModContent.TileType<HellPod>());
                            Main.tile[spawn.X + x, spawn.Y + y].TileFrameX = (short)(x * 18);
                            Main.tile[spawn.X + x, spawn.Y + y].TileFrameY = (short)(y * 18);
                        }
                    }

                    ModContent.GetInstance<HellPodTileEntity>().Generic_HookPostPlaceMyPlayer.hook(spawn.X, spawn.Y, ModContent.TileType<HellPod>(), 0, 1, 0);
                }
            }

        }));
    }
}

