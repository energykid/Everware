namespace Everware.Content.Meteor;

public class MeteorBiome : ModBiome
{
    public override int Music => Assets.Sounds.Music.Meteor.Slot;
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override bool IsBiomeActive(Player player)
    {
        return Main.LocalPlayer.GetModPlayer<MeteorMusicStats>().meteorTiles > 100 && (player.Center.Y / 16f) < Main.worldSurface;
    }
}
public class MeteorMusicSystem : ModSystem
{
    float Intensity = 0f;
    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (Main.LocalPlayer.InModBiome<MeteorBiome>())
        {
            Intensity = MathHelper.Lerp(Intensity, 1f, 0.025f);
        }
        else
        {
            Intensity = MathHelper.Lerp(Intensity, 0f, 0.05f);
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