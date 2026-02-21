using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.NPCs;
using Everware.Content.Base.ParticleSystem;
using Everware.Core;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.ID;

namespace Everware.Content.PreHardmode.EyeOfCthulhuRework;

public class EyeOfCthulhu : GlobalNPC
{
    // Tendril 0: front left
    // Tendril 1: front right
    // Tendril 2: back left
    // Tendril 3: back right
    public Vector2[] TendrilPositions = new Vector2[4];
    public Vector2[] TendrilJointPositions = new Vector2[4];
    public Vector2[] TendrilClawPositions = new Vector2[4];
    public Vector2 VelocityTarget = Vector2.Zero;
    public int TendrilsOut = 0;
    public override bool InstancePerEntity => true;
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == NPCID.EyeofCthulhu;
    }
    public enum AttackState
    {
        None,
        Bash,
        Bleed,
        Multiply,
        TendrilsOut,
        Stab,
        Swipe,
        RepeatedBash
    }
    public AttackState CurrentAttack = AttackState.TendrilsOut;
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
        npc.behindTiles = true;
        npc.aiStyle = -1;

        for (int i = 0; i < 4; i++)
        {
            TendrilPositions[i] = TendrilJointPositions[i] = TendrilClawPositions[i] = npc.Center;
        }

        PupilPaletteNum = Main.rand.Next(3);
    }
    public override void FindFrame(NPC npc, int frameHeight)
    {

    }
    public int AttackTimer = 0;
    public int EyeDilationReset = 0;
    public override void AI(NPC npc)
    {
        if (TendrilsOut == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                TendrilPositions[i] = TendrilJointPositions[i] = TendrilClawPositions[i] = npc.Center;
            }
        }
        else
        {
            MoveTendrilsIdle(npc);
        }

        EyeDilationReset--;
        if (EyeDilationReset <= 0)
            EyeDilation = MathHelper.Lerp(EyeDilation, 0.3f + (float)(Math.Sin(GlobalTimer.Value / 10f) * 0.05f), 0.2f);
        else
        {
            EyeDilation = MathHelper.Lerp(EyeDilation, -0.2f, 0.6f);
        }

        npc.TargetClosest();

        Player Target = Main.player[npc.target];

        switch (CurrentAttack)
        {
            case AttackState.None:
                AttackTimer++;
                npc.LerpAngleTowardsPosition(Target.Center, 0.1f, MathHelper.ToRadians(-90f));
                npc.VelocityMoveTowardsPosition(Target.Center + new Vector2((float)Math.Sin(GlobalTimer.Value / 20f) * 50f, -200 + ((float)Math.Sin(GlobalTimer.Value / 17f) * 30f)), 0.03f, 0.03f);

                if (AttackTimer > 50)
                {
                    ChangeState(npc, AttackState.Bash);
                }
                break;
            case AttackState.TendrilsOut:
                AttackTimer++;
                if (AttackTimer > 200)
                {
                    if (TendrilsOut <= 3)
                    {
                        EyeDilation = MathHelper.Lerp(EyeDilation, 1.5f, 0.5f);
                        npc.velocity *= 0.85f;
                        if (AttackTimer % 14 == 0)
                        {
                            UnleashTendril(npc);
                        }
                    }
                    else
                    {
                        ChangeState(npc, AttackState.None);
                    }
                }
                npc.LerpAngleTowardsPosition(Target.Center, 0.1f, MathHelper.ToRadians(-90f));
                npc.VelocityMoveTowardsPosition(Target.Center + new Vector2((float)Math.Sin(GlobalTimer.Value / 20f) * 50f, -200 + ((float)Math.Sin(GlobalTimer.Value / 17f) * 30f)), 0.03f, 0.03f);
                break;
            case AttackState.Bash:
                if (AttackTimer == 0)
                {
                    SoundEngine.PlaySound(Sounds.NPC.EoCWhoosh.Asset, npc.Center);
                    npc.ai[2] = 0f;
                    npc.velocity.Y -= 4f;
                    AttackTimer++;
                }
                else
                {
                    npc.ai[2] = MathHelper.Lerp(npc.ai[2], 1f, 0.1f);
                    if (npc.ai[2] < 0.8f)
                    {
                        EyeDilation = MathHelper.Lerp(EyeDilation, 1f, 0.2f);
                        npc.ai[1] = 0f;
                        npc.LerpAngleTowardsPosition(Target.Center, 0.09f, MathHelper.ToRadians(-90f));
                        npc.VelocityMoveTowardsPosition(Target.Center + new Vector2((float)Math.Sin(GlobalTimer.Value / 20f) * 50f, -700 + ((float)Math.Sin(GlobalTimer.Value / 17f) * 30f)), 0.02f, 0.2f);
                    }
                    else
                    {
                        if (npc.Distance(Target.Center) > 800)
                            ChangeState(npc, AttackState.None);

                        EyeDilation = MathHelper.Lerp(EyeDilation, -0.5f, 0.2f);
                        if (npc.ai[1] < 0.5f)
                        {
                            npc.velocity *= 0.8f;
                        }
                        npc.ai[1] = MathHelper.Lerp(npc.ai[1], 1f, 0.1f);
                        npc.velocity += (npc.rotation + MathHelper.PiOver2).ToRotationVector2() * npc.ai[1] * 1.5f;
                    }
                }
                if (WorldGen.SolidOrSlopedTile(Main.tile[((npc.Center + npc.velocity) / 16).ToPoint()]) && npc.ai[2] > 0.8f)
                {
                    ThrowTileReplicants(npc, (npc.Center / 16).ToPoint());
                    BleedAllOver(npc);
                    npc.ai[2] = 0f;
                    SoundEngine.PlaySound(Sounds.NPC.EoCBash.Asset, npc.Center);
                    ScreenEffects.AddScreenShake(npc.Center, 20f, 0.6f);
                    npc.velocity = Vector2.Zero;
                    npc.velocity.Y = -3f;
                    ChangeState(npc, AttackState.None);
                }
                break;
        }
    }
    public void ChangeState(NPC npc, AttackState state)
    {
        AttackTimer = 0;
        npc.GetGlobalNPC<EyeOfCthulhu>().CurrentAttack = state;
    }
    public float EyeDilation = 0f;

    float FrameNum = 0;
    float PupilPaletteNum = 1;
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.IsABestiaryIconDummy)
        {
            MoveTendrilsIdle(npc);
            drawColor = Color.White;
            EyeDilation = MathHelper.Lerp(EyeDilation, 0.3f + (float)(Math.Sin(GlobalTimer.Value / 10f) * 0.05f), 0.2f);
        }

        FrameNum += 0.2f;
        FrameNum = FrameNum % 5;

        Asset<Texture2D> EyeDraw = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.EyeOfCthulhu.Asset;
        Asset<Texture2D> TendrilDraw = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.EyeOfCthulhuTendril.Asset;
        Vector2 TendrilOrigin = new Vector2(6, 2);
        Asset<Texture2D> ClawDraw = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.EyeOfCthulhuClaw.Asset;
        Vector2 ClawOrigin = new Vector2(12, 2);
        Asset<Texture2D> EyePupilDraw = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.EyeOfCthulhu_PupilMask.Asset;

        if (TendrilsOut > 0)
        {
            for (int i = 0; i < TendrilsOut; i++)
            {
                SpriteEffects eff = SpriteEffects.None;
                if (i == 0 || i == 2)
                    eff = SpriteEffects.FlipHorizontally;

                spriteBatch.Draw(TendrilDraw.Value, TendrilPositions[i] - screenPos, TendrilDraw.Frame(), drawColor, TendrilPositions[i].AngleTo(TendrilJointPositions[i]) - MathHelper.PiOver2, TendrilOrigin, 1f, eff, 0f);
                spriteBatch.Draw(TendrilDraw.Value, TendrilJointPositions[i] - screenPos, TendrilDraw.Frame(), drawColor, TendrilJointPositions[i].AngleTo(TendrilClawPositions[i]) - MathHelper.PiOver2, TendrilOrigin, 1f, eff, 0f);
                spriteBatch.Draw(ClawDraw.Value, TendrilClawPositions[i] - screenPos, ClawDraw.Frame(), drawColor, npc.rotation, ClawOrigin, 1f, eff, 0f);
            }
        }

        spriteBatch.Draw(EyeDraw.Value, npc.Center - screenPos, EyeDraw.Frame(5, 1, (int)Math.Floor(FrameNum), 0), drawColor, npc.rotation, (npc.frame.Size() / 2) + new Vector2(0, 22), 1f, SpriteEffects.None, 0f);

        var PupilEffect = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.EyeOfCthulhuPupil.CreatePupilEffect();
        PupilEffect.Parameters.IrisThreshold = 0.33f + (EyeDilation * 0.22f);
        PupilEffect.Parameters.PupilThreshold = 0.66f + (EyeDilation * 0.22f);
        var PupilPalette = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.PupilPalette1.Asset.Value;
        if (PupilPaletteNum == 1) PupilPalette = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.PupilPalette2.Asset.Value;
        if (PupilPaletteNum == 2) PupilPalette = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.PupilPalette3.Asset.Value;
        PupilEffect.Parameters.PupilGradient = PupilPalette;
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
    public void MoveTendrilsIdle(NPC npc)
    {
        if (TendrilsOut > 0)
            MoveTendril(npc, 0, npc.Center + new Vector2(-110, 20).RotatedBy(npc.rotation));
        if (TendrilsOut > 1)
            MoveTendril(npc, 1, npc.Center + new Vector2(110, 20).RotatedBy(npc.rotation));
        if (TendrilsOut > 2)
            MoveTendril(npc, 2, npc.Center + new Vector2(-100, -80).RotatedBy(npc.rotation));
        if (TendrilsOut > 3)
            MoveTendril(npc, 3, npc.Center + new Vector2(100, -80).RotatedBy(npc.rotation));
    }
    // IK lookalike to move the tendrils
    public void MoveTendril(NPC npc, int i, Vector2 position)
    {
        TendrilPositions[i] = npc.Center + new Vector2(0, -20).RotatedBy(npc.rotation) + (npc.DirectionTo(position) * 20);

        TendrilClawPositions[i] = Vector2.Lerp(TendrilClawPositions[i], position, 0.7f);
        TendrilJointPositions[i] = Vector2.Lerp(TendrilJointPositions[i], Vector2.Lerp(TendrilPositions[i], TendrilClawPositions[i], 0.6f) + new Vector2(0, -40f).RotatedBy(npc.rotation), 0.25f);

        TendrilJointPositions[i] = TendrilPositions[i] + (TendrilPositions[i].DirectionTo(TendrilJointPositions[i]) * 74);
        TendrilClawPositions[i] = TendrilJointPositions[i] + (TendrilJointPositions[i].DirectionTo(TendrilClawPositions[i]) * 74);
    }
    public void UnleashTendril(NPC npc)
    {
        int i = (int)TendrilsOut;
        TendrilPositions[i] = TendrilJointPositions[i] = TendrilClawPositions[i] =
            npc.Center + new Vector2(i == 0 || i == 2 ? -50 : 50, i > 1 ? -10 : 10).RotatedBy(npc.rotation);

        BleedAllOver(npc);

        ScreenEffects.AddScreenShake(npc.Center, 10f, 0.6f);

        SoundEngine.PlaySound(Sounds.NPC.EoCTendrilEmerge.Asset with { MaxInstances = 4 }, TendrilPositions[i]);
        npc.velocity += npc.DirectionFrom(TendrilPositions[i]) * 6f;
        TendrilsOut++;
    }

    public void BleedAllOver(NPC npc)
    {
        for (int j = 0; j < 75; j++)
        {
            Vector2 v = npc.Center + new Vector2(Main.rand.Next(80), 0).RotatedByRandom(MathHelper.TwoPi);
            Dust.NewDustDirect(v, 5, 5, DustID.Blood, v.DirectionFrom(npc.Center).X, v.DirectionFrom(npc.Center).Y, Scale: 1.3f);
        }
    }

    public void ThrowTileReplicants(NPC npc, Point pt)
    {
        for (int i = -4; i <= 6; i++)
        {
            for (int j = -3; j <= 3; j++)
            {
                Vector2 position = new Vector2((pt.X + i) * 16, (pt.Y + j) * 16);
                Tile t = Main.tile[pt.X + i, pt.Y + j];
                if (WorldGen.SolidOrSlopedTile(t))
                {
                    WorldGen.KillTile_MakeTileDust(pt.X + i, pt.Y + j, t);
                    if (!WorldGen.SolidOrSlopedTile(Main.tile[pt.X + i, pt.Y + j - 1]))
                    {
                        Vector2 altVel = npc.velocity;
                        altVel.Normalize();
                        TileReplicantParticle tile = new TileReplicantParticle(t.TileType, new Rectangle(t.TileFrameX, t.TileFrameY, 16, 16), position + new Vector2(8, 8), new Vector2(0, -4f + (Math.Abs((float)i / 3f))), Vector2.One, P =>
                        {
                            (P as TileReplicantParticle).HideTimer--;
                            if ((P as TileReplicantParticle).HideTimer > 0) P.position -= P.velocity;
                            else
                            {
                                P.velocity.Y += 0.4f;
                                if (P.position.Y > (P as TileReplicantParticle).StartingY) P.Kill();
                            }
                        })
                        {
                            HideTimer = Math.Abs(i) * 5,
                            StartingY = position.Y + 8
                        };
                        tile.Spawn();
                    }
                }
            }
        }
    }
}
