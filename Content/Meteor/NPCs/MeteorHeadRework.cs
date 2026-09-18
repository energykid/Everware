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
    public override void DrawBehind(NPC npc, int index)
    {
        Main.instance.DrawCacheNPCsMoonMoon.Add(index);
    }
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == NPCID.MeteorHead;
    }
    public override bool PreAI(NPC npc)
    {
        npc.rotation = Vector2.Zero.AngleFrom(npc.velocity);
        npc.ai[1] += 0.5f;
        npc.TargetClosest(false);
        npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(Main.player[npc.target].Center) * 0.5f, 0.07f);

        Vector2 pos = npc.Center + new Vector2(14, 0).RotatedBy(npc.rotation).RotatedByRandom(MathHelper.PiOver2);
        Dust d = Dust.NewDustPerfect(pos, DustID.FlameBurst, Vector2.Zero);
        d.noGravity = true;
        if (Main.rand.NextBool(3))
        {
            Dust d2 = Dust.NewDustPerfect(pos, DustID.Smoke, -npc.velocity * 2f);
            d2.scale = 0.5f;
            d2.noGravity = true;
        }

        return false;
    }
    public static void Draw(NPC npc, Vector2 screenPos, Color drawColor)
    {
        Lighting.AddLight(npc.Center, Color.Red.ToVector3() * 0.2f);

        if (npc.IsABestiaryIconDummy) npc.ai[1] += 0.5f;

        Vector2 origin = new(16, 16);

        int HeadFrameY = (int)(npc.ai[1] / 5f % 3);
        if (npc.ai[2] > 0) HeadFrameY = 3;

        var HeadAsset = Assets.Textures.Meteor.NPCs.MeteorHead.Asset;
        var GlowAsset = Assets.Textures.Meteor.NPCs.MeteorHead_Glow.Asset;
        var HeadFrame = HeadAsset.Frame(5, 4, npc.GetGlobalNPC<MeteorHeadRework>().Personality, HeadFrameY);

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
    }
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.ai[2] == 0) Draw(npc, screenPos, drawColor);
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
