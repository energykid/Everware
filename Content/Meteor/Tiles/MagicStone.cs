using Everware.Content.Base;
using Everware.Content.Base.Items;
using Everware.Content.Base.Tiles;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Meteor.Tiles;

public class MagicStoneTile : EverTile
{
    public override bool UsesExtraTarget => true;
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/MagicStoneTile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = DustID.Silt;
        AddMapEntry(new Color(28, 29, 29));
    }
    public override void ExtraDrawSingleTile(int i, int j)
    {
        DrawingUtils.DrawSlopedTile(Main.spriteBatch, Assets.Textures.Meteor.Tiles.MagicStoneGlow.Asset, i, j, Color.White, Vector2.Zero);
    }
    public override void ExtraDrawEverything()
    {
        var effect = Assets.Effects.Meteor.MeteorTileStreaks.CreateGlow();

        effect.Parameters.NoiseTexture = Assets.Textures.Misc.PerlinNoise.Asset.Value;
        effect.Parameters.ScreenPosition = new Vector2(Main.screenPosition.X / ExtraTarget.Target.Width, Main.screenPosition.Y / ExtraTarget.Target.Height);
        effect.Parameters.Progress = GlobalTimer.Value / 10;
        effect.Parameters.NoiseScale = new Vector2(20f, 1f) * 0.6f;
        effect.Parameters.Resolution = ExtraTarget.Target.Size() / 2f;
        effect.Parameters.Resolution2 = Assets.Textures.Misc.PerlinNoise.Asset.Size() / 2f;

        effect.Apply();

        Main.spriteBatch.End(out var sb);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, effect.Shader, Main.GameViewMatrix.ZoomMatrix);

        base.ExtraDrawEverything();

        Main.spriteBatch.Restart(sb);
    }
}
public class MagicStoneItem : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/MagicStoneItem";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }
    public override int PlacementID => ModContent.TileType<MagicStoneTile>();
}
