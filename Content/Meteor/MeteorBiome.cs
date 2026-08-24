namespace Everware.Content.Meteor;

public class MeteorBiome : ModBiome
{
    public override int Music => Assets.Sounds.Music.Meteor.Slot;
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override bool IsBiomeActive(Player player)
    {
        return Main.LocalPlayer.GetModPlayer<MeteorMusicStats>().meteorTiles > 100 && (player.Center.Y / 16f) < Main.worldSurface;
    }
    [ModSystemHooks.PostUpdateEverything]
    public void SpawnParticles()
    {

    }
}
public class MeteorMusicSystem : ModSystem
{
    // this override just removes vanilla meteor logic completely
    // if WorldGen.spawnMeteor is false, none of the logic in Main.HandleMeteorFall will run
    // i could detour without an orig which would be more "thorough" but  nuh uh
    public override void PreUpdateTime()
    {
        WorldGen.spawnMeteor = false;
    }

    float Intensity = 0f;
    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (Main.LocalPlayer.InModBiome<MeteorBiome>())
        {
            Intensity = MathHelper.Lerp(Intensity, 1f, 0.0075f);
        }
        else
        {
            Intensity = MathHelper.Lerp(Intensity, 0f, 0.0075f);
        }

        tileColor *= (1f - (Intensity * 0.6f));
        backgroundColor = backgroundColor.MultiplyRGBA(new Color((1f - (Intensity * 0.6f)), (1f - (Intensity * 0.75f)), (1f - (Intensity * 0.85f))));

        base.ModifySunLightColor(ref tileColor, ref backgroundColor);
    }

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {
        Main.LocalPlayer.GetModPlayer<MeteorMusicStats>().meteorTiles += tileCounts[MeteorGeneration.CharredSoil];
        Main.LocalPlayer.GetModPlayer<MeteorMusicStats>().meteorTiles += tileCounts[MeteorGeneration.CharredStone];
        Main.LocalPlayer.GetModPlayer<MeteorMusicStats>().meteorTiles += tileCounts[MeteorGeneration.MeteoricGrass];
        Main.LocalPlayer.GetModPlayer<MeteorMusicStats>().meteorTiles += tileCounts[MeteorGeneration.MeteoriteOre];
    }

    public override void ResetNearbyTileEffects()
    {
        Main.LocalPlayer.GetModPlayer<MeteorMusicStats>().meteorTiles = 0;
    }
}