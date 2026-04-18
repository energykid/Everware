using Everware.Common.Systems;
using Everware.Content.Base.NPCs;
using Everware.Utils;
using System;
using Terraria.ID;

namespace Everware.Content.Gallery.Snapdragon;

public partial class Snapdragon : ModNPC
{
    public void IntroAttackState()
    {
        NPC.dontTakeDamage = true;
        HeadFrame = (int)MathHelper.Clamp(3 + (MathHelper.ToRadians((Target.Center.X - NPC.Center.X) * 1.35f)), 0, 6);
        CurveAnimation_Idle();
        NPC.TargetClosest();
        SoundStyle s1 = Assets.Sounds.NPC.Snapdragon_Ambience.Asset;
        s1.MaxInstances = 5;
        if (NPC.ai[0] % 30 == 10 && NumSpineSegmentsActive < SpinePositions.Length) SoundEngine.PlaySound(s1.WithVolumeScale(2f).WithPitchVariance(0.2f), NPC.Center);
        NPC.Center = Vector2.Lerp(NPC.Center, BasePosition + new Vector2(0, -280), 0.1f);
        if (NPC.ai[0] % 8 == 0 && NPC.ai[0] > 30)
        {
            if (NumSpineSegmentsActive < SpinePositions.Length)
            {
                SoundStyle s = Assets.Sounds.NPC.Snapdragon_Assemble.Asset;
                s.MaxInstances = 20;
                SoundEngine.PlaySound(s.WithPitchOffset(-1f + (NPC.ai[0] / 100)).WithVolumeScale(NPC.ai[0] / 200), NPC.Center);
            }
            NumSpineSegmentsActive++;
            if (NumSpineSegmentsActive == SpinePositions.Length + 8)
            {
                SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_Roar.Asset, NPC.Center);

                if (!Main.dedServ)
                {
                    if (ModLoader.TryGetMod("CalamityFables", out Mod calFables))
                    {
                        calFables.Call("vfx.displayBossIntroCard", NPC.TypeName, Mods.Everware.BossIntroText.Snapdragon.GetTextValue(), 100, false, Color.CadetBlue, Color.White, Color.CadetBlue, Color.CadetBlue, Mods.Everware.BossIntroText.MusicCenterpiece.GetTextValue(), Mods.Everware.BossIntroText.MusicianENNWAY.GetTextValue());
                    }
                }
            }
            if (NumSpineSegmentsActive == SpinePositions.Length + 16)
            {
                ChangeAttackState(AttackState.Idle);
            }
        }
        if (NumSpineSegmentsActive >= SpinePositions.Length + 8 && NumSpineSegmentsActive < SpinePositions.Length + 16)
        {
            JawOpening = MathHelper.Lerp(JawOpening, 7f, 0.1f);

            Roaring = true;

            ScreenEffects.AddScreenShake(NPC.Center, 5f, 0.7f);
            ScreenEffects.DimScreen(0.25f);
            ScreenEffects.ZoomScreen(0.5f);

            HeadOffset = new Vector2(Main.rand.NextFloat(-1, 1) * 2f, Main.rand.NextFloat(-1, 1) * 2f);

            if (HeadFrame != 3) NPC.rotation -= MathHelper.ToRadians((HeadFrame - 3) * 2.5f);

            NPC.VelocityMoveTowardsPosition(BasePosition + (BasePosition.DirectionTo(Target.Center) * 300) + new Vector2(0, -200), 0.2f, 0.2f);
        }
        else
        {
            if (NumSpineSegmentsActive >= SpinePositions.Length)
            {
                HeadOpacity = MathHelper.Lerp(HeadOpacity, 1f, 0.2f);
                HeadOffset.Y *= 0.9f;
                HeadOffset.X *= 0.75f;
            }
            else
                HeadOffset = BasePosition - NPC.Center;
        }

        if (NumSpineSegmentsActive < SpinePositions.Length + 16)
            ScreenEffects.PanTo(Vector2.Lerp(BasePosition, BasePosition + new Vector2(0, -50), NPC.ai[0] / 230));

        NPC.rotation = MathHelper.Lerp(NPC.rotation, (NPC.Center.X - Target.Center.X) * 0.001f, 0.4f);

        float maxDist1 = NPC.Center.Distance(BasePosition);
        maxDist1 = MathHelper.Clamp(maxDist1, 200, 250);

        NPC.VelocityMoveTowardsPosition(Target.Center + new Vector2(-20f + (float)(Math.Sin(NPC.ai[0] / 60) * 100f), -220 + (float)(Math.Sin(NPC.ai[0] / 63) * 20f)), 0.1f, 0.05f, 20);

        Vector2 outPos1 = new Vector2(maxDist1, 0).RotatedBy(BasePosition.AngleTo(NPC.Center));

        NPC.Center = Vector2.Lerp(NPC.Center, BasePosition + (outPos1 * new Vector2(0.7f, 1.0f)), 0.3f);

        for (float i = 14; i >= 0; i--)
        {
            if (i >= Easing.KeyFloat(NumSpineSegmentsActive, 0, 15, 15, 0, Easing.Linear))
            {
                float skewX = (NPC.Center.X - BasePosition.X) * MathHelper.Lerp(0.2f, 0.1f, (i / 15f));
                float skew = Math.Abs(NPC.Center.X - BasePosition.X) * 0.2f;
                skew -= 10;
                if (skew < 0) skew = 0;
                float v = Easing.KeyFloat(i, 0, 7, 1f, 0.2f, Easing.OutExpo, 0f);
                v = 0.1f;

                Vector2 off = (MathUtils.CubicBezier2(NPC.Center + new Vector2(0, -10).RotatedBy(NPC.rotation), SpineCurve[1], SpineCurve[0], BasePosition, i / 15f));

                SpinePositions[(int)i] = Vector2.Lerp(SpinePositions[(int)i], off, v);
            }
        }
    }
    public void IdleAttackState()
    {
        NPC.dontTakeDamage = false;
        HeadFrame = (int)MathHelper.Clamp(3 + (MathHelper.ToRadians((Target.Center.X - NPC.Center.X) * 1.35f)), 0, 6);
        CurveAnimation_Idle();

        NPC.TargetClosest();

        NPC.rotation = MathHelper.Lerp(NPC.rotation, (NPC.Center.X - Target.Center.X) * 0.001f, 0.02f);

        float maxDist = NPC.Center.Distance(BasePosition);
        maxDist = MathHelper.Clamp(maxDist, 200, 250);

        NPC.VelocityMoveTowardsPosition(Target.Center + new Vector2(-20f + (float)(Math.Sin(NPC.ai[0] / 60) * 100f), -220 + (float)(Math.Sin(NPC.ai[0] / 63) * 20f)), 0.1f, 0.05f, 20);

        Vector2 outPos = new Vector2(maxDist, 0).RotatedBy(BasePosition.AngleTo(NPC.Center));

        NPC.Center = Vector2.Lerp(NPC.Center, BasePosition + (outPos * new Vector2(0.7f, 1.0f)), 0.3f);

        for (float i = 0; i < 15; i++)
        {
            float skewX = (NPC.Center.X - BasePosition.X) * MathHelper.Lerp(0.2f, 0.1f, (i / 15f));
            float skew = Math.Abs(NPC.Center.X - BasePosition.X) * 0.2f;
            skew -= 10;
            if (skew < 0) skew = 0;
            float v = Easing.KeyFloat(i, 0, 7, 1f, 0.2f, Easing.OutExpo, 0f);
            v = Easing.KeyFloat(i, 7, 15, 0.2f, 1f, Easing.InExpo, v);

            Vector2 off = (MathUtils.CubicBezier2(NPC.Center + new Vector2(0, -10).RotatedBy(NPC.rotation), SpineCurve[1], SpineCurve[0], BasePosition, i / 15f));

            SpinePositions[(int)i] = Vector2.Lerp(SpinePositions[(int)i], off, v);
        }

        int amtOfTime = 60;
        if (Main.expertMode) amtOfTime = 50;
        if (Main.masterMode) amtOfTime = 40;

        if (NPC.ai[0] > amtOfTime) ChangeAttackState(Main.rand.NextBool() ? AttackState.FrostBreath : AttackState.SnapFreeze);
    }
    public void FrostBreathAttackState()
    {
        ClampPositioning();
        float aa = Easing.KeyFloat(NPC.ai[0], 60, 150, NPC.direction * -40, NPC.direction * 90, Easing.InCubic, NPC.direction * -20);
        aa = Easing.KeyFloat(NPC.ai[0], 150, 182, NPC.direction * 90, NPC.direction * 110, Easing.OutCubic, aa);
        float rotOffset = Easing.KeyFloat(NPC.ai[0], 60, 182, 0f, NPC.direction * 40, Easing.OutInCirc, NPC.direction * -20);
        rotOffset += aa;
        NPC.rotation = MathHelper.Lerp(NPC.rotation, ((NPC.Center.X - Target.Center.X) * 0.001f) + MathHelper.ToRadians(-rotOffset), 0.4f);
        if (NPC.ai[0] < 65)
        {
            CurveAnimation_Idle();
            Roaring = true;

            JawOpening = MathHelper.Lerp(JawOpening, 4f, 0.05f);

            FrostBreathLength = 0f;
            NPC.direction = Target.Center.X > NPC.Center.X ? 1 : -1;
            TargetPosition = Target.Center.Grounded();
            TargetPosition.X += (NPC.direction * 200);
            HeadFrame = (int)MathHelper.Clamp(3 + (MathHelper.ToRadians((TargetPosition.X - NPC.Center.X) * 1.35f)), 0, 6);
            NPC.VelocityMoveTowardsPosition(BasePosition + new Vector2(0, -200), 0.1f, 0.05f, 2);
        }
        else
        {
            CurveAnimation_Active();
            Point p = (NPC.Center / 16f).ToPoint();
            float length = 0f;
            for (int i = 0; i < 50; i++)
            {
                length += 16f * 5f;
                p += (FrostBreathAngle.ToRotationVector2() * 5).ToPoint();
                if (p.X < 100 || p.X > Main.maxTilesX - 100) break;
                if (p.Y < 100 || p.Y > Main.maxTilesY - 100) break;
                if (Main.tile[p].HasTile)
                {
                    if (Main.tileSolid[Main.tile[p].TileType]) break;
                }
            }
            if (NPC.ai[0] == 70)
            {
                SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_FrostBreath.Asset, NPC.Center);
                SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_Emerge.Asset, NPC.Center);
            }
            HeadOffset = new Vector2(Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5));

            JawOpening = MathHelper.Lerp(JawOpening, 7f, 0.1f);

            Roaring = true;

            float rot = NPC.AngleTo(TargetPosition) + (-NPC.direction * MathHelper.ToRadians(NPC.direction * rotOffset)) + MathHelper.ToRadians(NPC.direction * 35f);
            BreatheFrost(rot);

            FrostBreathAngle = rot - MathHelper.ToRadians(90f);
            FrostBreathWidth = MathHelper.Lerp(FrostBreathWidth, 300, 0.05f);
            FrostBreathLength = MathHelper.Lerp(FrostBreathLength, length * 3f, 0.2f);

            ScreenEffects.DimScreen(MathHelper.Lerp(0.65f, 0f, NPC.ai[0] / 180f));
            ScreenEffects.AddScreenShake(NPC.Center, 4, 0.7f);

            NPC.VelocityMoveTowardsPosition(BasePosition - (BasePosition.DirectionTo(Target.Center) * 70) + new Vector2(0, -300), 0.2f, 0.2f);
        }
        if (NPC.ai[0] > 180)
        {
            TryBurrow();
        }
    }
    public void RoarAttackState()
    {
        CurveAnimation_Idle();
        ClampPositioning();
        NPC.rotation = MathHelper.Lerp(NPC.rotation, (NPC.Center.X - Target.Center.X) * 0.001f, 0.4f);
        if (NPC.ai[0] < 25)
        {
            if (NPC.ai[0] == 2)
                SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_Idle.Asset.WithPitchVariance(0.2f), NPC.Center);

            if (NPC.ai[0] % 5 == 4)
                SoundEngine.PlaySound((Assets.Sounds.NPC.Snapdragon_Ambience.Asset with { MaxInstances = 20 }).WithPitchOffset(NPC.ai[0] * 0.06f), NPC.Center);

            NPC.direction = Target.Center.X > NPC.Center.X ? 1 : -1;
            HeadFrame = (int)MathHelper.Clamp(3 + (MathHelper.ToRadians((Target.Center.X - NPC.Center.X) * 1.35f)), 0, 6);
            NPC.VelocityMoveTowardsPosition(BasePosition + new Vector2(0, -200), 0.1f, 0.05f, 2);
        }
        else
        {
            Roaring = true;

            if (NPC.ai[0] % 5 == 4)
                SoundEngine.PlaySound((Assets.Sounds.NPC.Snapdragon_Ambience.Asset with { MaxInstances = 20 }).WithPitchVariance(0.2f), NPC.Center);

            if (NPC.ai[0] == 30) SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_Roar.Asset, NPC.Center);

            HeadOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));

            if (HeadFrame != 3) NPC.rotation -= MathHelper.ToRadians((HeadFrame - 3) * (NPC.ai[0] / 20));

            if (NPC.ai[0] < 40)
            {
                JawOpening = MathHelper.Lerp(JawOpening, 12f, 0.1f);
                ScreenEffects.DimScreen(0.25f);
                ScreenEffects.AddScreenShake(NPC.Center, 7, 0.7f);
            }
            else
            {
                JawOpening = MathHelper.Lerp(JawOpening, 10f, 0.05f);
            }

            NPC.VelocityMoveTowardsPosition(BasePosition + (BasePosition.DirectionTo(Target.Center) * 300) + new Vector2(0, -200), 0.2f, 0.2f);
        }
        if (NPC.ai[0] > 70)
        {
            ChangeAttackState(AttackState.Idle);
        }
    }
    public void BurrowAttackState()
    {
        NPC.direction = HeadFrame > 3 ? 1 : -1;
        float a = 0f;
        float BurrowWindupTime = 35;
        float BurrowWindupSpeed = 15;
        float DigTime = 50;
        if (NPC.ai[0] == 1) SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_BurrowWindup.Asset, NPC.Center);
        if (NPC.ai[0] < BurrowWindupTime)
        {
            HeadFrame = NPC.direction > 0 ? 6 : 0;

            Vector2 v = new Vector2(0, -400);
            v = Easing.KeyVector2(NPC.ai[0], 0f, BurrowWindupSpeed / 3f * 2f, new Vector2(0, -300), new Vector2(NPC.direction * 100, -250), Easing.InOutBack);
            v = Easing.KeyVector2(NPC.ai[0], BurrowWindupSpeed / 3f * 2f, BurrowWindupSpeed, new Vector2(NPC.direction * 50, -250), new Vector2(NPC.direction * 100, 50), Easing.InExpo);
            if (NPC.ai[0] > 18) v = new Vector2(NPC.direction * 300, 50);

            a = Math.Clamp(Easing.InCirc(NPC.ai[0] / BurrowWindupTime * 1.2f), 0f, 1f);

            NPC.rotation = MathHelper.Lerp(NPC.rotation, MathHelper.PiOver4 * NPC.direction * (a * 2f), 0.2f);

            NPC.VelocityMoveTowardsPosition(BasePosition + v, 0.15f, a);

            if (NPC.ai[0] == 12)
            {
                SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_BurrowDig.Asset, NPC.Center);
            }

            if (NPC.ai[0] > 12)
            {
                NPC.velocity *= 1.2f;
                if (NPC.Center.Y > BasePosition.Y - 100) NPC.ai[0] = BurrowWindupTime - 1;
            }

            SpineCurve[0] = Vector2.Lerp(SpineCurve[0], Vector2.Lerp(NPC.Center, BasePosition, 0.6f) + (NPC.DirectionTo(BasePosition) * (-50 * Math.Abs(NPC.rotation) * 0.1f)) + new Vector2(0, 40), 0.2f);
            SpineCurve[1] = Vector2.Lerp(SpineCurve[1], NPC.Center + (NPC.DirectionTo(BasePosition) * 200) + new Vector2(MathHelper.ToDegrees(NPC.rotation * 0.1f), -100) + new Vector2(MathHelper.ToDegrees(NPC.rotation), 0), 0.35f);

            SpineCurve[0] = Vector2.Lerp(SpineCurve[0], Vector2.Lerp(NPC.Center, BasePosition, 0.6f) + new Vector2(-NPC.direction * 300, 0).RotatedBy(NPC.rotation), a);
            SpineCurve[1] = Vector2.Lerp(SpineCurve[1], Vector2.Lerp(NPC.Center, BasePosition, 0.3f) + new Vector2(-NPC.direction * 100, 0).RotatedBy(NPC.rotation), a);
        }
        else
        {
            NPC.velocity = Vector2.Lerp(NPC.Center, BasePosition, 0.08f) - NPC.Center;
            a = 0.5f;
            if (NPC.ai[0] % 10 == 2)
            {
                SoundEngine.PlaySound(SoundID.WormDig, NPC.Center);
            }
            if (NPC.ai[0] == BurrowWindupTime)
            {
                for (float i = 0; i < 40; i++)
                {
                    Vector2 p = Vector2.Lerp(NPC.Center, BasePosition, i / 40f);
                    ThrowShardParticle((p + new Vector2(Main.rand.NextFloat(-30f, 30f), -50f)).Grounded(), new Vector2(MathHelper.Lerp(NPC.direction * 15f, 0f, i / 40f), Main.rand.NextFloat(-12f, -6f)), 1.25f);
                }
                ThrowTileReplicants(NPC, (NPC.Center / 16).ToPoint(), 10, 5);

                a = 1f;

                SpineVisible = false;

                HeadOpacity = 0f;

                CenteredForAttacks = !CenteredForAttacks;
                if (!CenteredForAttacks) BasePosition.X += Main.rand.NextFloat(-750, -400) * (Main.rand.NextBool() ? 1 : -1);
                else BasePosition = GallerySystem.GalleryPosition.ToVector2() * 16f;
            }
            if (NPC.ai[0] == BurrowWindupTime + DigTime)
            {
                SpineCurve[0] = BasePosition;
                SpineCurve[1] = BasePosition;
                MoveSpineDefault(1f);

                SpineVisible = true;

                NPC.Center = BasePosition;

                NPC.rotation = 0f;

                SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, NPC.Center);
                SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_Ambience.Asset, NPC.Center);
                ResetSpine();
            }
            if (NPC.ai[0] < BurrowWindupTime + DigTime - 10)
            {
                ThrowShardParticle((NPC.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), -50f)).Grounded(), new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-8f, -5f)));
            }
            if (NPC.ai[0] > BurrowWindupTime + DigTime)
            {
                if (NPC.ai[0] < BurrowWindupTime + DigTime + 2)
                {
                    SpineCurve[0] = BasePosition;
                    SpineCurve[1] = BasePosition;
                    MoveSpineDefault(1f);
                }

                if (NPC.ai[0] == BurrowWindupTime + DigTime + 6)
                {
                    for (int i = 0; i < 10; i++)
                        ThrowShardParticle((NPC.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), -50f)).Grounded(), new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-8f, -5f)), 1.7f);

                    ThrowTileReplicants(NPC, ((NPC.Center + new Vector2(0, -50)).Grounded() / 16).ToPoint(), 10, 5);

                    SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_BurrowEmerge.Asset, NPC.Center);
                }

                HeadOpacity = MathHelper.Lerp(HeadOpacity, 1f, 0.35f);

                NPC.VelocityMoveTowardsPosition(BasePosition + new Vector2(0, -400), 0.1f, 0.2f);

                if (NPC.ai[0] > BurrowWindupTime + DigTime + 6)
                {
                    PickAttack();
                }
            }
        }
        MoveSpineDefault(a);
    }
    public void SnapFreezeAttackState()
    {
        RotateTowardsPlayer(out var rotOffset2);

        CurveAnimation_Idle();

        NPC.VelocityMoveTowardsPosition(BasePosition + new Vector2(0, -50), 0.05f, 0.1f);

        JawOpening = MathHelper.Lerp(JawOpening, 5f, 0.1f);
        if (NPC.ai[0] > 90) TryBurrow();

        if (NPC.ai[0] > 52)
        {
            if (NPC.ai[0] % 6 == 3)
            {
                NPC.velocity += Target.DirectionTo(NPC.Center) * 7f;
                JawOpening = 7.5f;
                SoundEngine.PlaySound(SoundID.DD2_OgreSpit, NPC.Center);
                PuffFrost(NPC.rotation + MathHelper.ToRadians(90f) - rotOffset2, new Vector2(7.5f, 1.5f), 2f);
                Projectile p = Projectile.NewProjectileDirect(new EntitySource_Parent(NPC, "Snapdragon Snap Freeze Attack"), Vector2.Lerp(MouthPosition(), NPC.Center, 0.6f) + new Vector2(Main.rand.NextFloat(-30, 30), Main.rand.NextFloat(0, 30)), new Vector2(30, 0).RotatedBy(NPC.rotation + MathHelper.ToRadians(90f) - rotOffset2 + Main.rand.NextFloat(-0.1f, 0.1f)), ModContent.ProjectileType<SnapFreezeProjectile>(), 40, 4, ai2: NPC.ai[0] - 10);
            }
        }

        ClampPositioning();
    }
    public void BiteAttackState()
    {
        RotateTowardsPlayer(out var rot2);

        CurveAnimation_Idle();

        if (NPC.ai[0] == 30)
        {
            SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_Windup.Asset.WithPitchVariance(0.2f), NPC.Center);
            SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_Ambience.Asset.WithPitchOffset(1f).WithVolumeScale(1.5f), NPC.Center);
        }

        NPC.VelocityMoveTowardsPosition(BasePosition + new Vector2(0, -175) + (Target.DirectionTo(NPC.Center) * 70), 0.05f, 0.1f);

        if (NPC.ai[0] < 70)
        {
            JawOpening = MathHelper.Lerp(JawOpening, 8f, 0.25f);

            if (NPC.ai[0] > 50)
            {
                NPC.VelocityMoveTowardsPosition(BasePosition + new Vector2(0, -175) + (Target.DirectionTo(NPC.Center) * 85), 0.1f, 0.1f);
            }
        }
        else
        {
            if (NPC.ai[0] < 75)
                NPC.damage = BiteDamage;
            Vector2 bPos = BasePosition + new Vector2(0, -200);
            Vector2 pos = Target.Center;
            if (pos.Distance(bPos) > 350) pos = bPos + (bPos.DirectionTo(pos) * 350);
            if (NPC.ai[0] == 70)
            {
                SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_Bite.Asset.WithPitchVariance(0.2f), NPC.Center);
                NPC.Center = Vector2.Lerp(NPC.Center, pos, 0.6f);
            }
            if (NPC.ai[0] < 80)
                NPC.VelocityMoveTowardsPosition(pos, 0.2f, 0.6f, 10);
            JawOpening = 0f;
        }
        if (NPC.ai[0] > 90) ChangeAttackState(AttackState.Burrow);
    }
    public void BeakBashAttackState()
    {

    }
    public void RotateTowardsPlayer(out float rot2)
    {
        NPC.direction = Target.Center.X > NPC.Center.X ? 1 : -1;

        HeadFrame = (int)MathHelper.Clamp(3 + (MathHelper.ToRadians((Target.Center.X - NPC.Center.X) * 1.35f)), 0, 6);

        float rotOffset2 = (NPC.direction * MathHelper.ToRadians(15f));

        NPC.rotation = NPC.rotation.AngleLerp(NPC.AngleTo(Target.Center) - MathHelper.PiOver2 + rotOffset2, 0.1f);

        rot2 = rotOffset2;
    }
}