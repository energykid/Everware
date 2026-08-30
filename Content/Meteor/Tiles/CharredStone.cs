using Everware.Content.Base;
using Everware.Content.Base.Items;
using Everware.Content.Base.Tiles;
using Everware.Utils;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace Everware.Content.Meteor.Tiles;

public class CharredStoneTile : EverTile
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/CharredStoneTile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = DustID.Silt;
        AddMapEntry(new Color(28, 29, 29));
    }
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomSolid);
    }
    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var asset = Assets.Textures.Meteor.Tiles.CharredStoneGlow.Asset;

        var effect = Assets.Effects.Meteor.MeteorTileStreaks.CreateGlow();

        effect.Parameters.NoiseTexture = Assets.Textures.Misc.PerlinNoise.Asset.Value;
        effect.Parameters.NoiseScale = new Vector2(10f, 1f);
        effect.Parameters.Resolution = asset.Size() / 2f;
        effect.Parameters.Resolution2 = Assets.Textures.Misc.PerlinNoise.Asset.Size() / 2f;

        effect.Apply();

        spriteBatch.End(out var ss);
        spriteBatch.Begin(ss with { CustomEffect = effect.Shader, SortMode = SpriteSortMode.Deferred });

        DrawingUtils.DrawSlopedTile(spriteBatch, asset, i, j, Lighting.GetColor(i, j).MultiplyRGBA(new(1f, 1f, 1f, (i / 5f) + GlobalTimer.Value / MathHelper.Lerp(1000, 1500, (i % 3f) / 3f))), Vector2.Zero);

        spriteBatch.Restart(ss);
    }
}
public class CharredStoneItem : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/CharredStoneItem";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }
    public override int PlacementID => ModContent.TileType<CharredStoneTile>();
}
