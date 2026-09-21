using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.NPCs;
using Everware.Content.Base.ParticleSystem;
using Everware.Content.Base.Projectiles;
using Everware.Content.Meteor.Tiles;
using Everware.Utils;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using static Daybreak.Common.Features.Hooks.GlobalNPCHooks;

namespace Everware.Content.Meteor.NPCs;

public class SpectralSnail : EverNPC
{
    public override string Texture => "Everware/Assets/Textures/Meteor/NPCs/SpectralSnail";

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 14;
    }

    public override void SetDefaults()
    {

        NPC.width = 156;
        NPC.height = 70;
        NPC.damage = 14;
        NPC.defense = 6;
        NPC.lifeMax = 2000;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.value = Item.buyPrice(gold: 5);
        NPC.aiStyle = -1;
        NPC.knockBackResist = 0f;
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
}
   
