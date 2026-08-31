namespace Everware.Content.Meteor;

public class MeteorBiome : ModBiome
{
    public override int Music => Assets.Sounds.Music.Meteor.Slot;
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override bool IsBiomeActive(Player player)
    {
        return Main.LocalPlayer.GetModPlayer<MeteorMusicStats>().meteorTiles > 1500 && (player.Center.Y / 16f) < Main.worldSurface;
    }
    public override void Load()
    {
        On_Main.DoDraw_WallsAndBlacks += DrawStuff;
    }
    private void DrawStuff(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self)
    {
        var asset = Assets.Textures.Misc.SinglePixel.Asset;

        var eff = Assets.Effects.Meteor.MeteorBackground.CreateEffect();
        eff.Apply();

        Main.spriteBatch.End(out var sb);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, eff.Shader);
        Main.EntitySpriteDraw(asset.Value, Vector2.Zero, asset.Frame(), Color.White.MultiplyRGBA(new(MeteorEffectSystem.Intensity, MeteorEffectSystem.Intensity, MeteorEffectSystem.Intensity, MeteorEffectSystem.Intensity)), 0f, Vector2.Zero, new Vector2(Main.screenWidth * 5f, Main.screenHeight), Main.GameViewMatrix.Effects);
        Main.spriteBatch.Restart(sb);

        orig(self);
    }

    public override void Unload()
    {
        On_Main.DoDraw_WallsAndBlacks -= DrawStuff;
    }
}
public class MeteorEffectSystem : ModSystem
{
    // this override just removes vanilla meteor logic completely
    // if WorldGen.spawnMeteor is false, none of the logic in Main.HandleMeteorFall will run
    // i could detour without an orig which would be more "thorough" but  nuh uh
    public override void PreUpdateTime()
    {
        WorldGen.spawnMeteor = false;
    }

    public static float Intensity = 0f;
    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (Main.LocalPlayer.InModBiome<MeteorBiome>() && Main.LocalPlayer.Center.Y < Main.worldSurface * 16f)
        {
            Intensity = MathHelper.Lerp(Intensity, 1f, 0.0075f);
        }
        else
        {
            Intensity = MathHelper.Lerp(Intensity, 0f, 0.0075f);
        }

        tileColor *= (1f - (Intensity * 0.6f));
        backgroundColor = backgroundColor.MultiplyRGBA(new Color((1f - (Intensity * 0.5f)), (1f - (Intensity * 0.68f)), (1f - (Intensity * 0.45f))));

        base.ModifySunLightColor(ref tileColor, ref backgroundColor);
    }

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {
        Main.LocalPlayer.GetModPlayer<MeteorMusicStats>().meteorTiles += tileCounts[MeteorGeneration.CharredSoil];
        Main.LocalPlayer.GetModPlayer<MeteorMusicStats>().meteorTiles += tileCounts[MeteorGeneration.MagicStone];
        Main.LocalPlayer.GetModPlayer<MeteorMusicStats>().meteorTiles += tileCounts[MeteorGeneration.StarCrossedGrass];
        Main.LocalPlayer.GetModPlayer<MeteorMusicStats>().meteorTiles += tileCounts[MeteorGeneration.MeteoriteOre];
    }

    public override void ResetNearbyTileEffects()
    {
        Main.LocalPlayer.GetModPlayer<MeteorMusicStats>().meteorTiles = 0;
    }

    [ModSystemHooks.PostUpdateEverything]
    public void LightingStuff()
    {
        if (Intensity > 0.05f)
        {
            for (int i = 0; i < Main.screenWidth / 16; i++)
                for (int j = 0; j < Main.screenHeight / 16; j++)
                    Lighting.AddLight(Main.screenPosition + (new Vector2(i, j) * 16f), new Vector3(0.01f));
        }
    }
}