using Everware.Content.Base;
using Everware.Content.Base.Tiles;
using Everware.Utils;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace Everware.Content.Meteor.Tiles;

public class SpiritGrassTile : EverTile
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/SpiritGrassTile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = DustID.CrystalSerpent_Pink;
        AddMapEntry(new Color(250, 200, 219));
        Main.tileLighted[Type] = true;
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<CharredSoilTile>();
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.8f;
        g = 0.4f;
        b = 0.85f;

        var amt = MathHelper.Lerp(1f, 0.5f, (float)Math.Sin((GlobalTimer.Value / 40f) + (i / 30f) + (j / 80f)));
        var amt2 = MathHelper.Lerp(1f, 0.5f, (float)Math.Sin(((GlobalTimer.Value + 25f) / 40f) + (i / 30f) + (j / 80f)));

        r *= amt;
        g *= amt2;
        b *= amt2;

        base.ModifyLight(i, j, ref r, ref g, ref b);
    }
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomSolid);
    }
    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var asset = Assets.Textures.Meteor.Tiles.SpiritGrassGlow.Asset;
        DrawingUtils.DrawSlopedTile(spriteBatch, asset, i, j, Color.White, Vector2.Zero);
    }
}