using Microsoft.Xna.Framework;
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

    public static Vector2 MoreDrawOffset => Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
}
