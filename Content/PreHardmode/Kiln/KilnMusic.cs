
using Everware.Content.PreHardmode.Kiln.Tiles;
using System;
using Terraria.ID;

namespace Everware.Content.PreHardmode.Kiln;

public class KilnMusic : ModBiome
{
    public override int Music => MusicLoader.GetMusicSlot("Everware/Sounds/Music/Kiln");
    public override bool IsBiomeActive(Player player)
    {
        return player.GetModPlayer<KilnMusicPlayer>().kilnTiles > 20 && player.GetModPlayer<KilnMusicPlayer>().siltTiles > 10;
    }
}
public class KilnMusicSystem : ModSystem
{
    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {
        Main.LocalPlayer.GetModPlayer<KilnMusicPlayer>().siltTiles += tileCounts[TileID.Silt];
        Main.LocalPlayer.GetModPlayer<KilnMusicPlayer>().kilnTiles += tileCounts[ModContent.TileType<ForgingKiln>()];
        Main.LocalPlayer.GetModPlayer<KilnMusicPlayer>().kilnTiles += tileCounts[ModContent.TileType<KilnBrickPlaced>()];
        Main.LocalPlayer.GetModPlayer<KilnMusicPlayer>().kilnTiles += tileCounts[ModContent.TileType<KilnstonePlaced>()];
    }

    public override void ResetNearbyTileEffects()
    {
        Main.LocalPlayer.GetModPlayer<KilnMusicPlayer>().kilnTiles = 0;
    }
}
public class KilnMusicPlayer : ModPlayer
{
    public int siltTiles = 0;
    public int kilnTiles = 0;
    public int quarryTiles = 0;

    public override void ResetEffects()
    {

    }
}