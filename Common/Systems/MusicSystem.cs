using Terraria.ID;

namespace Everware.Common.Systems;

public class EverMusicSystem : ModSystem
{
    public override void Load()
    {
        On_Main.UpdateAudio_DecideOnNewMusic += DecideBossMusic;
    }

    private void DecideBossMusic(On_Main.orig_UpdateAudio_DecideOnNewMusic orig, Main self)
    {
        orig(self);

        if (NPC.CountNPCS(NPCID.EyeofCthulhu) > 0)
        {
            Main.newMusic = Sounds.Music.EyeOfCthulhu.Slot;
        }
    }
}
