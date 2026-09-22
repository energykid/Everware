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
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        drawData.colorTint = drawData.glowColor = drawData.finalColor = drawData.tileLight = Color.White;

        base.DrawEffects(i, j, spriteBatch, ref drawData);
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.4f;
        g = 0.3f;
        b = 0.1f;
        base.ModifyLight(i, j, ref r, ref g, ref b);
    }
}