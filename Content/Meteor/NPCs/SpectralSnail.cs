using Everware.Content.Base.NPCs;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace Everware.Content.Meteor.NPCs;

public class SpectralSnail : EverNPC
{
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.AddTags(
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Meteor,
            new FlavorTextBestiaryInfoElement("Mods.Everware.Bestiary.SpectralSnail"));
    }
    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<MeteorBiome>() ? 0.15f : 0f;
    }

    public override string Texture => "Everware/Assets/Textures/Meteor/NPCs/SpectralSnail";

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 14;
    }

    public override int Health => 400;
    public override void SetDefaults()
    {
        base.SetDefaults();
        NPC.width = 156;
        NPC.height = 70;
        NPC.damage = 14;
        NPC.defense = 6;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.value = CoinValue.Silver(50);
        NPC.aiStyle = -1;
        NPC.knockBackResist = 0f;
    }
    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        base.ApplyDifficultyAndPlayerScaling(1, balance, bossAdjustment);
    }
    public override void FindFrame(int frameHeight)
    {
        int startFrame = 0;
        int finalFrame = 13;

        int frameSpeed = 2;
        NPC.frameCounter += 0.5f;

        if (NPC.frameCounter > frameSpeed)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y += frameHeight;

            if (NPC.frame.Y > finalFrame * frameHeight)
            {
                NPC.frame.Y = startFrame * frameHeight;
            }
        }
    }

    //Flips every 3000 ticks, probably will need to be adjusted depending on the desired behavior.

    int fliptimer = 0;

    public override void AI()
    {
        fliptimer++;

        NPC.velocity.X = 0.5f;
        NPC.velocity.Y = 0f;

        NPC.direction = MathF.Sign(NPC.velocity.X);
        NPC.spriteDirection = NPC.direction;


        if (fliptimer > 3000)
        {
            NPC.velocity.X = -0.5f;
            NPC.direction = MathF.Sign(NPC.velocity.X);
            NPC.spriteDirection = NPC.direction;
        }

        if (fliptimer > 6000)
        {
            fliptimer = 0;
        }

    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var asset = Assets.Textures.Meteor.NPCs.SpectralSnail.Asset;
        var asset2 = Assets.Textures.Meteor.NPCs.SpectralSnail_Glow.Asset;

        var effects = NPC.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        var shader = Assets.Effects.Meteor.NPCs.SpectralSnailDither.CreateEffect();
        shader.Parameters.Resolution = asset.Size() / 2;
        shader.Parameters.Frames = 14;
        shader.Parameters.FrameNum = NPC.frame.Y / NPC.height;
        shader.Parameters.Progress = MathHelper.Lerp(-1f, 1f, NPC.ai[1]);
        shader.Apply();

        Main.spriteBatch.End(out var sb);
        Main.spriteBatch.Begin(sb with { CustomEffect = shader.Shader });
        Main.EntitySpriteDraw(asset.Value, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects);
        Main.spriteBatch.End();

        var shader2 = Assets.Effects.Meteor.NPCs.SpectralSnailDither.CreateEffect();
        shader2.Parameters.Resolution = asset.Size() / 2;
        shader2.Parameters.Frames = 14;
        shader2.Parameters.FrameNum = NPC.frame.Y / NPC.height;
        shader2.Parameters.Progress = MathHelper.Lerp(-1f, 1f, NPC.ai[2]);
        shader2.Apply();
        Main.spriteBatch.Begin(sb with { CustomEffect = shader2.Shader });

        Main.EntitySpriteDraw(asset2.Value, NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects);
        return false;
    }
}

