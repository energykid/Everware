using Everware.Content.PreHardmode.Kiln;
using Everware.Content.PreHardmode.Kiln.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Everware.Content.PreHardmode;

public class KilnOrQuarryGeneration : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        /*
        tasks.Add(new PassLegacy("Generating an abandoned processing site", delegate (GenerationProgress progress, GameConfiguration configuration)
        {
            Point spawn = new(Main.spawnTileX, Main.spawnTileY);
            if (Main.rand.NextBool())
                GenerateKiln(GetPointFrom(spawn));
            else
                GenerateQuarry(GetPointFrom(spawn));
        }));
        */
    }
    public Point GetPointFrom(Point p)
    {
        int dir = Main.rand.NextBool() ? 1 : -1;

        for (int i = 0; i < Main.rand.Next(150, 250); i++)
        {
            p.X += dir;
        }

        if (Main.tileSolid[Main.tile[p].TileType])
        {
            while (Main.tileSolid[Main.tile[p].TileType]) p.Y--;
        }
        else
        {
            while (!Main.tileSolid[Main.tile[p].TileType]) p.Y++;
        }

        return p;
    }
}
