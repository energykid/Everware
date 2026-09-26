using Everware.Content.Kiln;
using Everware.Content.Quarry;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using static Everware.Utils.PathfindingUtils;

namespace Everware.Content;

public class KilnOrQuarryGeneration : ModSystem
{
    public static ushort[] IrreplaceableTiles = {
        TileID.LivingWood,
        TileID.LeafBlock
    };
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        tasks.Add(new PassLegacy(Mods.Everware.KilnQuarryGen.GetTextValue(), delegate (GenerationProgress progress, GameConfiguration configuration)
        {
            Point spawn = new(Main.spawnTileX, Main.spawnTileY);
            if (!Main.drunkWorld)
            {
                if (Main.rand.NextBool())
                    KilnGenerator.GenerateKiln(GetFlatPointFromSpawn(spawn));
                else
                    QuarryGenerator.GenerateQuarry(GetFlatPointFromSpawn(spawn));
            }
            else
            {
                KilnGenerator.GenerateKiln(GetFlatPointFromSpawn(spawn, 1));
                QuarryGenerator.GenerateQuarry(GetFlatPointFromSpawn(spawn, -1));
            }
        }));
    }
}
