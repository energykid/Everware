using Everware.Content.Kiln.Tiles;
using Everware.Content.Quarry.Tiles;
using System;
using Terraria.ID;

namespace Everware.Content.Quarry;

public class QuarryMusic : ModBiome
{
    public override int Music => Assets.Sounds.Music.Quarry.Slot;
    public override bool IsBiomeActive(Player player)
    {
        return player.GetModPlayer<KilnQuarryMusicStats>().quarryTiles > 20 && player.GetModPlayer<KilnQuarryMusicStats>().siltTiles > 10;
    }
}
public class QuarryMusicSystem : ModSystem
{
    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {
        Main.LocalPlayer.GetModPlayer<KilnQuarryMusicStats>().siltTiles += tileCounts[TileID.Silt];
        Main.LocalPlayer.GetModPlayer<KilnQuarryMusicStats>().quarryTiles += tileCounts[ModContent.TileType<SturdyBricksPlaced>()];
        Main.LocalPlayer.GetModPlayer<KilnQuarryMusicStats>().quarryTiles += tileCounts[ModContent.TileType<WornWoodPlaced>()];
        Main.LocalPlayer.GetModPlayer<KilnQuarryMusicStats>().quarryTiles += tileCounts[ModContent.TileType<WeldingStation>()];
    }

    public override void ResetNearbyTileEffects()
    {
        Main.LocalPlayer.GetModPlayer<KilnQuarryMusicStats>().quarryTiles = 0;
    }
}