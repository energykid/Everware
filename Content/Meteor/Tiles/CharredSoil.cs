using Everware.Content.Base;
using Everware.Content.Base.Items;
using Everware.Content.Base.Tiles;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Meteor.Tiles;

public class CharredSoilTile : EverTile
{
    public override bool UsesExtraTarget => true;
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/CharredSoilTile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = DustID.Silt;
        AddMapEntry(new Color(28, 29, 29));
        Main.tileMergeDirt[Type] = true;
        Main.tileMerge[Type][TileID.Dirt] = true;
    }
    public override void ExtraDrawSingleTile(int i, int j)
    {
        var asset = Assets.Textures.Meteor.Tiles.CharredSoilGlow.Asset;

        int paint = Main.tile[i, j].TileColor;

        Texture2D? texture = null;

        bool useColor = paint > PaintID.None && !TryGetPaintTexture(paint, asset, out texture);

        texture ??= asset.Value;

        DrawingUtils.DrawSlopedTile(Main.spriteBatch, texture, i, j, Color.White, Vector2.Zero);
    }
    public override void ExtraDrawEverything()
    {
        var effect = Assets.Effects.Meteor.MeteorTileStreaks.CreateGlow();

        effect.Parameters.NoiseTexture = Assets.Textures.Misc.PerlinNoise.Asset.Value;
        effect.Parameters.ScreenPosition = new Vector2(Main.screenPosition.X / ExtraTarget.Target.Width, Main.screenPosition.Y / ExtraTarget.Target.Height * Main.LocalPlayer.gravDir);
        effect.Parameters.Progress = Main.LocalPlayer.gravDir == 1 ? GlobalTimer.Value / 10 : GlobalTimer.Value / -10;
        effect.Parameters.NoiseScale = new Vector2(40f, 10f) * 0.6f;
        effect.Parameters.Resolution = ExtraTarget.Target.Size() / 2f;
        effect.Parameters.Resolution2 = Assets.Textures.Misc.PerlinNoise.Asset.Size() / 2f;

        effect.Apply();

        Main.spriteBatch.End(out var sb);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, effect.Shader, Main.GameViewMatrix.TransformationMatrix);

        base.ExtraDrawEverything();

        Main.spriteBatch.Restart(sb);
    }
}
public class CharredSoilItem : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/CharredSoilItem";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }
    public override int PlacementID => ModContent.TileType<CharredSoilTile>();
}
