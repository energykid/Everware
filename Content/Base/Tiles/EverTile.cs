using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Everware.Content.Base.Tiles;

public abstract class EverTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileBlendAll[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
    }
}
