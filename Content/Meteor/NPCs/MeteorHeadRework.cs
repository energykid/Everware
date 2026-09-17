using Terraria.ID;

namespace Everware.Content.Meteor.NPCs;

public class MeteorHeadRework : GlobalNPC
{
    int Personality = 0;
    public override bool InstancePerEntity => true;
    public override void SetDefaults(NPC entity)
    {
        Personality = Main.rand.Next(5);
        base.SetDefaults(entity);
    }
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == NPCID.MeteorHead;
    }
    public override bool PreAI(NPC npc)
    {
        npc.rotation = Vector2.Zero.AngleFrom(npc.velocity);
        npc.ai[1]++;
        npc.TargetClosest(false);
        npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(Main.player[npc.target].Center) * 0.5f, 0.07f);
        return false;
    }
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.IsABestiaryIconDummy) npc.ai[1]++;

        Vector2 origin = new(16, 16);

        var HeadAsset = Assets.Textures.Meteor.NPCs.MeteorHead.Asset;
        var GlowAsset = Assets.Textures.Meteor.NPCs.MeteorHead_Glow.Asset;
        var HeadFrame = HeadAsset.Frame(5, 1, Personality);

        var FlameAsset = Assets.Textures.Meteor.NPCs.MeteorHead_FireMask.Asset;

        var Effects = npc.velocity.X < 0 || npc.IsABestiaryIconDummy ? SpriteEffects.None : SpriteEffects.FlipVertically;

        Main.spriteBatch.End(out var sb);

        float p1 = MathHelper.Lerp(1f, 0f, npc.ai[1] / 50f % 1f);
        float p2 = MathHelper.Lerp(1f, 0f, ((npc.ai[1] / 50f) + 0.5f) % 1f);

        var eff1 = Assets.Effects.Misc.GradientClip.CreateEffect();
        eff1.Parameters.LightingColor = Color.White.ToVector4();
        eff1.Parameters.ColorClip = p1;
        eff1.Parameters.ColorClipUpper = pwid(p1);
        eff1.Parameters.Gradient = Assets.Textures.Meteor.NPCs.MeteorFlameGradient.Asset.Value;
        eff1.Apply();

        Main.spriteBatch.Begin(sb with { CustomEffect = eff1.Shader });
        Main.EntitySpriteDraw(FlameAsset.Value, npc.Center - screenPos, FlameAsset.Frame(), Color.White, npc.rotation, origin, 1.15f, Effects);

        Main.spriteBatch.End();
        var eff2 = Assets.Effects.Misc.GradientClip.CreateEffect();
        eff2.Parameters.LightingColor = Color.White.ToVector4();
        eff2.Parameters.ColorClip = p2;
        eff2.Parameters.ColorClipUpper = pwid(p2);
        eff2.Parameters.Gradient = Assets.Textures.Meteor.NPCs.MeteorFlameGradient.Asset.Value;
        eff2.Apply();
        Main.spriteBatch.Begin(sb with { CustomEffect = eff2.Shader });

        Main.EntitySpriteDraw(FlameAsset.Value, npc.Center - screenPos, FlameAsset.Frame(), Color.White, npc.rotation, origin, 1.15f, Effects);
        Main.spriteBatch.Restart(sb);

        Main.EntitySpriteDraw(HeadAsset.Value, npc.Center - screenPos, HeadFrame, Color.Lerp(drawColor, Color.White, 0.75f), npc.rotation, origin, 1f, Effects);
        Main.EntitySpriteDraw(GlowAsset.Value, npc.Center - screenPos, HeadFrame, Color.White, npc.rotation, origin, 1f, Effects);

        float pwid(float i) { return (i * 2f); }
        return false;
    }
    public override void Load()
    {
        On_NPC.FindFrame += CancelMeteorHeadAI;
    }
    public override void Unload()
    {
        On_NPC.FindFrame -= CancelMeteorHeadAI;
    }

    private void CancelMeteorHeadAI(On_NPC.orig_FindFrame orig, NPC self)
    {
        if (self.type != NPCID.MeteorHead)
        {
            orig(self);
        }
    }
}
