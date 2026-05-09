using Everware.Content.Reliquary;
using System.Collections.Generic;
using System.IO;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Everware.Content.Gallery.Sculptor;

public class SculptorNPC : ModNPC
{
    private static Profiles.StackedNPCProfile Profile;
    public bool Focused = false;
    public int FocusedPlayer = -1;
    public override bool CanTownNPCSpawn(int numTownNPCs)
    {
        return numTownNPCs >= 2 && SculptorTownNPCArrivalSystem.SculptorAvailable;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        base.SendExtraAI(writer);
        writer.Write(Focused);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        base.ReceiveExtraAI(reader);
        Focused = reader.ReadBoolean();
    }
    public override void SetDefaults()
    {
        NPC.friendly = true;
        NPC.width = 18;
        NPC.height = 40;
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;

        AnimationType = NPCID.Guide;
    }
    public override string GetChat()
    {
        List<string> key = ["One", "Two", "Three", "Four", "Five", "Six"];
        return Mods.Everware.NPCs.SculptorNPC.Dialogue.Greeting.GetChildText(key[Main.rand.Next(key.Count)]).Value;
    }
    public override void SetChatButtons(ref string button, ref string button2)
    {
        button = Mods.Everware.NPCs.SculptorNPC.Chisel.GetTextValue();
    }
    public override void OnChatButtonClicked(bool firstButton, ref string shopName)
    {
        if (firstButton)
        {
            ReliquaryUISystem.OpenTrade(NPC);
            Focused = true;
            FocusedPlayer = Main.myPlayer;
            NPC.netUpdate = true;
        }
    }
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 25; // The amount of frames the NPC has

        NPCID.Sets.ExtraFramesCount[Type] = 9; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs.
        NPCID.Sets.AttackFrameCount[Type] = 4;
        NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the npc that it tries to attack enemies.
        NPCID.Sets.PrettySafe[Type] = 300;
        NPCID.Sets.AttackType[Type] = 3; // Swings a weapon.
        NPCID.Sets.AttackTime[Type] = 60; // The amount of time it takes for the NPC's attack animation to be over once it starts.
        NPCID.Sets.AttackAverageChance[Type] = 30;
        NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset.
        NPCID.Sets.ShimmerTownTransform[Type] = true; // This set says that the Town NPC has a Shimmered form. Otherwise, the Town NPC will become transparent when touching Shimmer like other enemies.

        // This sets entry is the most important part of this NPC. Since it is true, it tells the game that we want this NPC to act like a town NPC without ACTUALLY being one.
        // What that means is: the NPC will have the AI of a town NPC, will attack like a town NPC, and have a shop (or any other additional functionality if you wish) like a town NPC.
        // However, the NPC will not have their head displayed on the map, will de-spawn when no players are nearby or the world is closed, and will spawn like any other NPC.
        NPCID.Sets.ActsLikeTownNPC[Type] = true;

        // To reiterate, since this NPC isn't technically a town NPC, we need to tell the game that we still want this NPC to have a custom/randomized name when they spawn.
        // In order to do this, we simply make this hook return true, which will make the game call the TownNPCName method when spawning the NPC to determine the NPC's name.
        NPCID.Sets.SpawnsWithCustomName[Type] = true;

        // Connects this NPC with a custom emote.
        // This makes it when the NPC is in the world, other NPCs will "talk about him".
        // NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExampleBoneMerchantEmote>();

        // The vanilla Bone Merchant cannot interact with doors (open or close them, specifically), but if you want your NPC to be able to interact with them despite this,
        // uncomment this line below.
        NPCID.Sets.AllowDoorInteraction[Type] = true;

        // Influences how the NPC looks in the Bestiary
        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            Velocity = 1f, // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
            Direction = 1 // -1 is left and 1 is right. NPCs are drawn facing the left by default but ExamplePerson will be drawn facing the right
        };

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

        Profile = new Profiles.StackedNPCProfile(
                new Profiles.DefaultNPCProfile(Texture, -1),
                new Profiles.DefaultNPCProfile(Texture/* + "_Shimmer"*/, -1)
            );
    }
    public override string Texture => "Everware/Assets/Textures/Gallery/Sculptor/SculptorNPC";
    public override ITownNPCProfile TownNPCProfile()
    {
        return Profile;
    }
    public override bool CanChat()
    {
        return true;
    }
    public override List<string> SetNPCNameList()
    {
        return [
            "Gardner",
            "Toryn",
            "Ennsten",
            "Arnstron",
            "Thien",
            "Nell"
            ];
    }
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        // We can use AddRange instead of calling Add multiple times in order to add multiple items at once
        bestiaryEntry.Info.AddRange([
            // Sets the preferred biomes of this town NPC listed in the bestiary.
            // With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundSnow,

				// Sets your NPC's flavor text in the bestiary. (use localization keys)
				new FlavorTextBestiaryInfoElement("Mods.Everware.Bestiary.Sculptor")
        ]);
    }
    public override void HitEffect(NPC.HitInfo hit)
    {
        int num = NPC.life > 0 ? 1 : 5;

        for (int k = 0; k < num; k++)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.SnowflakeIce);
        }

        /*
        // Create gore when the NPC is killed.
        if (Main.netMode != NetmodeID.Server && NPC.life <= 0)
        {
            // Retrieve the gore types. This NPC only has shimmer variants. (6 total gores)
            string variant = "";
            if (NPC.IsShimmerVariant)
                variant += "_Shimmer";
            int headGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Head").Type;
            int armGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Arm").Type;
            int legGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Leg").Type;

            // Spawn the gores. The positions of the arms and legs are lowered for a more natural look.
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, headGore, 1f);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
        }
        */
    }
    public override void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight)
    {
        itemWidth = 60;
        itemHeight = 60;
    }
    public override void AI()
    {
        if (Focused)
        {
            NPC.velocity.X = 0f;
            if (Main.player.IndexInRange(FocusedPlayer))
            {
                NPC.direction = Math.Sign(Main.player[FocusedPlayer].Center.X - NPC.Center.X);
            }
        }
    }
}
public class SculptorTownNPCArrivalSystem : ModSystem
{
    public static bool SculptorAvailable = false;
    public override void SaveWorldData(TagCompound tag)
    {
        if (SculptorAvailable)
            tag.Add("Everware Sculptor Available", true);
    }
    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.TryGet("Everware Sculptor Available", out bool available))
        {
            SculptorAvailable = available;
        }
    }
}