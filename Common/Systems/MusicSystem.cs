using Everware.Content.EyeOfCthulhuRework;
using Everware.Content.Gallery.Snapdragon;
using Terraria.ID;
using Terraria.Localization;

namespace Everware.Common.Systems;

public class EverMusicSystem : ModSystem
{
    public override void PostAddRecipes()
    {
        if (!ModLoader.TryGetMod("MusicDisplay", out Mod display))
            return;

        LocalizedText modName = Mods.Everware.MusicDisplay.ModName.GetText();

        void AddMusic(int slot, string name)
        {
            display.Call("AddMusic", (short)slot,
                Language.GetText("Mods.Everware.MusicDisplay." + name + ".Name"),
                Language.GetText("Mods.Everware.MusicDisplay." + name + ".Author"),
                Language.GetText("Mods.Everware.MusicDisplay.ModName"));
        }

        AddMusic(Assets.Sounds.Music.SomewhereElse.Slot, "SomewhereElse");
        AddMusic(Assets.Sounds.Music.Kiln.Slot, "Kiln");
        AddMusic(Assets.Sounds.Music.Quarry.Slot, "Quarry");
        AddMusic(Assets.Sounds.Music.EyeOfCthulhu.Slot, "EyeOfCthulhu");
    }

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
            else
            {
                MusicLoader.GetMusic(Everware.Instance, "Assets/Sounds/Music/EyeOfCthulhu").SetVariable("Pitch", 0f);
            }
            if (NPC.CountNPCS(ModContent.NPCType<Snapdragon>()) > 0)
            {
                if ((Main.npc[NPC.FindFirstNPC(ModContent.NPCType<Snapdragon>())].ModNPC as Snapdragon).NumSpineSegmentsActive >= 22)
                {
                    Main.newMusic = Assets.Sounds.Music.Snapdragon.Slot;
                    Main.musicFade[Main.newMusic] = 1;
                }
                else
                {
                    Main.newMusic = Assets.Sounds.Music.Silence.Slot;
                    Main.musicFade[Main.newMusic] = 1;
                }
            }
        }
    }
}
