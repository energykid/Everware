using Everware.Content.EyeOfCthulhuRework;
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

        if (!Main.gameMenu && EyeOfCthulhu.ReworkEnabled)
        {
            if (NPC.CountNPCS(NPCID.EyeofCthulhu) > 0)
            {
                if (Main.npc[NPC.FindFirstNPC(NPCID.EyeofCthulhu)].GetGlobalNPC<EyeOfCthulhu>().MusicEnabled)
                {
                    Main.newMusic = Assets.Sounds.Music.EyeOfCthulhu.Slot;
                    Main.musicFade[Main.newMusic] = 1;
                }
                else
                {
                    Main.newMusic = Assets.Sounds.Music.Silence.Slot;
                }
            }
        }
    }
}
