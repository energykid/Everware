using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.Tiles;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Meteor.Tiles;

public class Meteorite : EverTile
{
    public override bool UsesExtraTarget => true;
    public override string Texture => "Everware/Assets/Textures/Misc/Tiles/TransparentDummyTileTexture";
    public override string GlowcoatTileTexture => "Everware/Assets/Textures/Meteor/Tiles/MeteoriteTile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = DustID.Silt;
        HitSound = SoundID.Tink;
        AddMapEntry(new Color(116, 55, 55));
        TileID.Sets.ChecksForMerge[Type] = true;
        Main.tileMerge[ModContent.TileType<CharredSoilTile>()][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<CharredSoilTile>()] = true;
        MinPick = 50;
        RegisterItemDrop(ItemID.Meteorite);
    }
    public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
    {
        WorldGen.TileMergeAttempt(-2, ModContent.TileType<CharredSoilTile>(), ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
    }

    public override void ExtraDrawSingleTile(int i, int j)
    {
        var asset = Assets.Textures.Meteor.Tiles.MeteoriteTile.Asset;

        int paint = Main.tile[i, j].TileColor;

        Texture2D? texture = null;

        bool useColor = paint > PaintID.None && !TryGetPaintTexture(paint, asset, out texture);

        texture ??= asset.Value;

        DrawingUtils.DrawSlopedTile(Main.spriteBatch, texture, i, j, Color.White, new Vector2(8f, 8f) - ScreenOffset);
    }
    public override void ExtraDrawEverything()
    {
        var effect = Assets.Effects.Meteor.MeteoriteTileCracks.CreateGlow();

        effect.Parameters.NoiseTexture = Assets.Textures.Misc.PerlinNoise.Asset.Value;
        effect.Parameters.Lightmap = LightmapSystem.ScreenLightmap.Target;
        effect.Parameters.ScreenPosition = new Vector2(RoundedScreenPosition.X / ExtraTarget.Target.Width, RoundedScreenPosition.Y / ExtraTarget.Target.Height * Main.LocalPlayer.gravDir);
        effect.Parameters.Progress = Main.LocalPlayer.gravDir == 1 ? GlobalTimer.Value / 10 : GlobalTimer.Value / -10;
        effect.Parameters.NoiseScale = new Vector2(2f, 1f) * 0.6f;
        effect.Parameters.ScreenResolution = ExtraTarget.Target.Size() / 2f;
        effect.Parameters.NoiseResolution = Assets.Textures.Misc.PerlinNoise.Asset.Size() / 2f;
        effect.Parameters.CrackTexture = Assets.Textures.Meteor.Tiles.MeteoriteTileOverlay.Asset.Value;
        effect.Parameters.CrackResolution = Assets.Textures.Meteor.Tiles.MeteoriteTileOverlay.Asset.Size() / 2f;
        effect.Parameters.OutlineColor1 = new Color(58, 43, 58).ToVector4();
        effect.Parameters.OutlineColor2 = new Color(143, 43, 58).ToVector4();

        effect.Apply();

        Main.spriteBatch.End(out var sb);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, effect.Shader, Main.GameViewMatrix.TransformationMatrix);

        Main.spriteBatch.Draw(ExtraTarget.Target, ScreenOffset, ExtraTarget.Target.Bounds, Color.Yellow, 0f, Vector2.Zero, 1f, Main.GameViewMatrix.Effects, 0f);

        Main.spriteBatch.Restart(sb);
    }
}