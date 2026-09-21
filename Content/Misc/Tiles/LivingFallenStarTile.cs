using Everware.Content.Base.Tiles;
using Terraria.ID;

namespace Everware.Content.Misc.Tiles;

public class LivingFallenStarTile : EverTile
{
    public override string Texture => "Everware/Assets/Textures/Misc/Tiles/LivingFallenStarTile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = DustID.YellowStarDust;
        AddMapEntry(new Color(255, 255, 51));
        Main.tileLighted[Type] = true;
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = g = b = 0.5f;
        base.ModifyLight(i, j, ref r, ref g, ref b);
    }
}