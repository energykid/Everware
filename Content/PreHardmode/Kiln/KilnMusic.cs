
using Everware.Content.PreHardmode.Kiln.Tiles;
using System;
using Terraria.ID;

namespace Everware.Content.PreHardmode.Kiln;

public class KilnMusic : ModBiome
{
    public override int Music => MusicLoader.GetMusicSlot("Everware/Sounds/Music/Kiln");
    public override bool IsBiomeActive(Player player)
    {
        return player.GetModPlayer<KilnQuarryMusicStats>().kilnTiles > 20 && player.GetModPlayer<KilnQuarryMusicStats>().siltTiles > 10;
    }
}
public class KilnMusicSystem : ModSystem
{
    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {
        Main.LocalPlayer.GetModPlayer<KilnQuarryMusicStats>().siltTiles += tileCounts[TileID.Silt];
        Main.LocalPlayer.GetModPlayer<KilnQuarryMusicStats>().kilnTiles += tileCounts[ModContent.TileType<ForgingKiln>()];
        Main.LocalPlayer.GetModPlayer<KilnQuarryMusicStats>().kilnTiles += tileCounts[ModContent.TileType<KilnBrickPlaced>()];
        Main.LocalPlayer.GetModPlayer<KilnQuarryMusicStats>().kilnTiles += tileCounts[ModContent.TileType<KilnstonePlaced>()];
    }

    public override void ResetNearbyTileEffects()
    {
        Main.LocalPlayer.GetModPlayer<KilnQuarryMusicStats>().kilnTiles = 0;
    }
}