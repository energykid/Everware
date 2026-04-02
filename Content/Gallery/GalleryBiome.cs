using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Everware.Content.Gallery;

public class GallerySystem : ModSystem
{
    public static Point GalleryPosition = Point.Zero;
    public override void SaveWorldData(TagCompound tag)
    {
        tag.Set("GalleryPosition", GalleryPosition);
    }
    public override void LoadWorldData(TagCompound tag)
    {
        GalleryPosition = tag.Get<Point>("GalleryPosition");
    }
}
public class GalleryBiome : ModBiome
{
    public override string Name => "TheGallery";
    public override int Music => MusicID.OtherworldlyIce;
    public override bool IsBiomeActive(Player player)
    {
        return player.Distance(GallerySystem.GalleryPosition.ToVector2() * 16) < 2500;
    }
}
