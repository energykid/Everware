using Everware.Content.Base;
using Everware.Content.Base.NPCs;
using Everware.Core;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.ID;

namespace Everware.Content.PreHardmode.EyeOfCthulhuRework;

public class EyeOfCthulhu : GlobalNPC
{
    public override bool InstancePerEntity => true;
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == NPCID.EyeofCthulhu;
    }
    public enum AttackState
    {
        None,
        SummonEyes1,
        Dash1
    }
    public AttackState CurrentAttack = AttackState.None;
    public enum PupilWidth
    {
        Narrow,
        Normal,
        Wide,
        Manual
    }
    public PupilWidth CurrentPupilWidth = PupilWidth.Normal;
    public override void SetDefaults(NPC npc)
    {
        base.SetDefaults(npc);
        npc.aiStyle = -1;
    }
    public override void FindFrame(NPC npc, int frameHeight)
    {

    }
    public int AttackTimer = 0;
    public int EyeDilationReset = 0;
    public override void AI(NPC npc)
    {
        npc.TargetClosest();

        EyeDilationReset--;
        if (EyeDilationReset <= 0)
            EyeDilation = MathHelper.Lerp(EyeDilation, 0.3f + (float)(Math.Sin(GlobalTimer.Value / 10f) * 0.05f), 0.2f);
        else
        {
            EyeDilation = MathHelper.Lerp(EyeDilation, 0f, 0.6f);
        }

        AttackTimer++;

        if (AttackTimer > 60)
        {
            if (CurrentPupilWidth == PupilWidth.Normal) CurrentPupilWidth = PupilWidth.Narrow;
            else if (CurrentPupilWidth == PupilWidth.Narrow) CurrentPupilWidth = PupilWidth.Wide;
            else if (CurrentPupilWidth == PupilWidth.Wide) CurrentPupilWidth = PupilWidth.Normal;
            AttackTimer = 0;
        }

        Player Target = Main.player[npc.target];

        npc.LerpAngleTowardsPosition(Target.Center, 0.1f, MathHelper.ToRadians(-90f));
        npc.VelocityMoveTowardsPosition(Target.Center + new Vector2((float)Math.Sin(GlobalTimer.Value / 20f) * 50f, -200 + ((float)Math.Sin(GlobalTimer.Value / 17f) * 30f)), 0.03f, 0.03f);
    }
    public void ChangeState(NPC npc, AttackState state)
    {
        npc.GetGlobalNPC<EyeOfCthulhu>().CurrentAttack = state;
    }
    public float EyeDilation = 0f;

    float FrameNum = 0;
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.IsABestiaryIconDummy)
        {
            drawColor = Color.White;
        }

        FrameNum += 0.2f;
        FrameNum = FrameNum % 5;

        Asset<Texture2D> EyeDraw = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.EyeOfCthulhu.Asset;
        Asset<Texture2D> EyePupilDraw = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.EyeOfCthulhu_PupilMask.Asset;

        spriteBatch.Draw(EyeDraw.Value, npc.Center - screenPos, EyeDraw.Frame(5, 1, (int)Math.Floor(FrameNum), 0), drawColor, npc.rotation, (npc.frame.Size() / 2) + new Vector2(0, 22), 1f, SpriteEffects.None, 0f);

        var PupilEffect = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.EyeOfCthulhuPupil.CreatePupilEffect();
        PupilEffect.Parameters.IrisThreshold = 0.33f + (EyeDilation * 0.22f);
        PupilEffect.Parameters.PupilThreshold = 0.66f + (EyeDilation * 0.22f);
        PupilEffect.Parameters.PupilGradient = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.PupilPalette1.Asset.Value;
        PupilEffect.Parameters.LightingColor = drawColor.ToVector4();
        PupilEffect.Apply();

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, PupilEffect.Shader, Main.GameViewMatrix.ZoomMatrix);

        spriteBatch.Draw(EyePupilDraw.Value, npc.Center - screenPos, EyePupilDraw.Frame(), Color.White, npc.rotation, (npc.frame.Size() / 2) + new Vector2(0, 22), 1f, SpriteEffects.None, 0f);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.ZoomMatrix);

        return false;
    }

    public void OnHit(NPC npc, int damage)
    {
        EyeDilationReset = 14;
    }
    // On hit effects
    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        OnHit(npc, damageDone);
        base.OnHitByProjectile(npc, projectile, hit, damageDone);
    }
    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        OnHit(npc, damageDone);
        base.OnHitByItem(npc, player, item, hit, damageDone);
    }
}
