using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.NPCs;
using Everware.Content.Base.ParticleSystem;
using Everware.Utils;
using System;
using Terraria.ID;

namespace Everware.Content.Gallery.Snapdragon;

[AutoloadBossHead]
public class Snapdragon : ModNPC
{
    public Player Target => Main.player[NPC.target];
    public override string Texture => "Everware/Assets/Textures/Gallery/Snapdragon/Snapdragon_Head";
    public override string BossHeadTexture => "Everware/Assets/Textures/Gallery/Snapdragon/Snapdragon_Head_Boss";
    public static int DefaultContactDamage => 45;
    public Vector2 BasePosition = Vector2.Zero;
    public Vector2[] SpinePositions = Array.Empty<Vector2>();
    public float[] SpineFrames = Array.Empty<float>();
    public Vector2[] SpineCurve = Array.Empty<Vector2>();
    public Vector2 HeadOffset = Vector2.Zero;
    public float HeadFrame = 3;
    public float JawOpening = 0f;
    public float HeadOpacity = 0f;
    public bool SpineVisible = true;
    public bool start = false;
    public Vector2 TargetPosition = Vector2.Zero;
    public bool CenteredForAttacks = true;
    public static int FrostBreathDamage => 110;
    public static int IceSpikeDamage => 35;
    public override void BossHeadSlot(ref int index)
    {
        if (HeadOpacity < 0.5f)
            index = -1;
        base.BossHeadSlot(ref index);
    }
    public enum AttackState
    {
        Intro,

        Idle, // Idle animation, do nothing then decide on an attack
        Roar, // Roar, knocking the player back but otherwise doing nothing

        Burrow, // Relocate to a different part of the arena, causing icicles to fall while doing so
        BeakBash, // Slam beak-first into the ground while facing the player, causing debris to come from the ground
        FrostBreath, // Breathe frost at the player then rotate upwards, filling an entire side of the arena with solid, spiky ice and
                     // forcing them to swerve behind the boss
        SnapFreeze, // Fire bursts of cold air that instantly freeze into icicles
    }
    public void ChangeAttackState(AttackState stateTo)
    {
        NPC.ai[0] = 0; NPC.ai[1] = 0; CurrentAttack = stateTo;
    }
    public AttackState CurrentAttack = AttackState.Intro;
    public override void SetDefaults()
    {
        if (!Main.dedServ)
            FrostBreathLayer.AllParticles.Clear();
        CurrentAttack = AttackState.Intro;
        NPC.aiStyle = -1;
        NPC.life = NPC.lifeMax = 25000;
        NPC.defense = 35;
        NPC.knockBackResist = 0f;
        NPC.damage = 0;
        NPC.width = 120; NPC.height = 190;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.boss = true;
        BasePosition = (GallerySystem.GalleryPosition.ToVector2() * 16).Grounded();
        NPC.Center = BasePosition + new Vector2(0f, 800f);
        SpinePositions = new Vector2[15];
        SpineFrames = new float[15];
        for (float i = 0; i < 15; i++)
        {
            SpinePositions[(int)i] = BasePosition + new Vector2(0, 100);
            SpineFrames[(int)i] = 2f;
        }
        SpineCurve = new Vector2[2];
        for (float i = 0; i < 2; i++)
        {
            SpineCurve[(int)i] = Vector2.Lerp(NPC.Center, BasePosition, i / 2f);
        }
        HeadOpacity = 0f;
        HeadOffset = new Vector2(0, 500);
        Music = Assets.Sounds.Music.Silence.Slot;
    }
    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        if (!SpineVisible) return false;
        scale *= 1.5f;
        return base.DrawHealthBar(hbPosition, ref scale, ref position);
    }
    bool Roaring = false;
    public override void AI()
    {
        if (HeadOpacity > 0.9f) HeadOpacity = 1f;

        if (!start)
        {
            for (float i = 0; i < 15; i++)
            {
                SpinePositions[(int)i] = BasePosition + new Vector2(0, 100);
                SpineFrames[(int)i] = 2;
            }
            for (float i = 0; i < 2; i++)
            {
                SpineCurve[(int)i] = Vector2.Lerp(NPC.Center, BasePosition, i / 15f);
            }
            start = true;
        }

        Roaring = false;

        NPC.ai[0]++;

        switch (CurrentAttack)
        {
            #region Intro
            case AttackState.Intro:
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
                    JawOpening = MathHelper.Lerp(JawOpening, 12f, 0.1f);

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
                break;
            #endregion
            #region Idle
            case AttackState.Idle:

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

                int amtOfTime = 140;
                if (Main.expertMode) amtOfTime = 100;
                if (Main.masterMode) amtOfTime = 60;

                if (NPC.ai[0] > amtOfTime) ChangeAttackState(AttackState.FrostBreath);
                break;
            #endregion
            #region Roar
            case AttackState.Roar:
                CurveAnimation_Idle();
                ClampPositioning();
                NPC.rotation = MathHelper.Lerp(NPC.rotation, (NPC.Center.X - Target.Center.X) * 0.001f, 0.4f);
                if (NPC.ai[0] < 25)
                {
                    NPC.direction = Target.Center.X > NPC.Center.X ? 1 : -1;
                    HeadFrame = (int)MathHelper.Clamp(3 + (MathHelper.ToRadians((Target.Center.X - NPC.Center.X) * 1.35f)), 0, 6);
                    NPC.VelocityMoveTowardsPosition(BasePosition + new Vector2(0, -200), 0.1f, 0.05f, 2);
                }
                else
                {
                    Roaring = true;

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
                break;
            #endregion
            #region Frost Breath
            case AttackState.FrostBreath:
                CurveAnimation_Active();
                ClampPositioning();
                float rotOffset = Easing.KeyFloat(NPC.ai[0], 60, 182, NPC.direction * -20, NPC.direction * 110, Easing.Linear, NPC.direction * -20);
                NPC.rotation = MathHelper.Lerp(NPC.rotation, ((NPC.Center.X - Target.Center.X) * 0.001f) + MathHelper.ToRadians(-rotOffset), 0.4f);
                if (NPC.ai[0] < 65)
                {
                    Roaring = true;

                    JawOpening = MathHelper.Lerp(JawOpening, 8f, 0.05f);

                    FrostBreathLength = 0f;
                    NPC.direction = Target.Center.X > NPC.Center.X ? 1 : -1;
                    TargetPosition = Target.Center.Grounded();
                    TargetPosition.X += (NPC.direction * 200);
                    HeadFrame = (int)MathHelper.Clamp(3 + (MathHelper.ToRadians((TargetPosition.X - NPC.Center.X) * 1.35f)), 0, 6);
                    NPC.VelocityMoveTowardsPosition(BasePosition + new Vector2(0, -200), 0.1f, 0.05f, 2);
                }
                else
                {
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
                    HeadOffset = new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2));

                    JawOpening = MathHelper.Lerp(JawOpening, 12f, 0.1f);

                    Roaring = true;

                    float rot = NPC.AngleTo(TargetPosition) + (-NPC.direction * MathHelper.ToRadians(NPC.direction * rotOffset)) + MathHelper.ToRadians(NPC.direction * 25);
                    BreatheFrost(rot);

                    FrostBreathAngle = rot - MathHelper.ToRadians(90f);
                    FrostBreathWidth = MathHelper.Lerp(FrostBreathWidth, 300, 0.05f);
                    FrostBreathLength = MathHelper.Lerp(FrostBreathLength, length * 3f, 0.2f);

                    ScreenEffects.DimScreen(MathHelper.Lerp(0.65f, 0f, NPC.ai[0] / 180f));
                    ScreenEffects.AddScreenShake(NPC.Center, 4, 0.7f);

                    NPC.VelocityMoveTowardsPosition(BasePosition + (BasePosition.DirectionTo(Target.Center) * 50) + new Vector2(0, -300), 0.2f, 0.2f);
                }
                if (NPC.ai[0] > 180)
                {
                    ChangeAttackState(AttackState.Burrow);
                }
                break;
            #endregion
            #region Burrow
            case AttackState.Burrow:
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
                            ChangeAttackState(AttackState.Idle);
                        }
                    }
                }
                MoveSpineDefault(a);
                break;
            #endregion
            #region Snap Freeze
            case AttackState.SnapFreeze:

                break;
            #endregion
        }

        if (!Roaring)
            JawOpening = MathHelper.Lerp(JawOpening, 6f, 0.2f);

        if (CurrentAttack != AttackState.Intro)
        {
            HeadOffset *= 0.96f;
        }
        if (CurrentAttack != AttackState.FrostBreath)
        {
            FrostBreathWidth *= 0.7f;
        }

        // Check if current attack isn't intro/idle since those two states handle this logic in the switch statement
        if (CurrentAttack != AttackState.Intro && CurrentAttack != AttackState.Idle && CurrentAttack != AttackState.Burrow)
        {
            MoveSpineDefault(0.3f);
        }
        else
        {
            NPC.rotation = MathHelper.Lerp(NPC.rotation, MathHelper.Clamp(NPC.rotation, -0.5f, 0.5f), 0.1f);
            NPC.direction = Target.Center.X > NPC.Center.X ? 1 : -1;
        }

        NPC.dontTakeDamage = !SpineVisible;
    }
    public void ResetSpine()
    {
        SpinePositions = new Vector2[15];
        for (int i = 0; i < 15; i++) SpinePositions[i] = BasePosition;
        MoveSpineDefault(1f);
    }
    public void MoveSpineDefault(float amount)
    {
        for (float i = 0; i < 15; i++)
        {
            float skewX = (NPC.Center.X - BasePosition.X) * MathHelper.Lerp(0.2f, 0.1f, (i / 15f));
            float skew = Math.Abs(NPC.Center.X - BasePosition.X) * 0.2f;
            skew -= 10;
            if (skew < 0) skew = 0;
            float v = Easing.KeyFloat(i, 0, 7, 1f, 0.2f, Easing.OutExpo, 0f);
            v = Easing.KeyFloat(i, 7, 15, 0.2f, 1f, Easing.InExpo, v);
            v = MathHelper.Lerp(v, 1f, amount);

            Vector2 off = (MathUtils.CubicBezier2(NPC.Center + new Vector2(0, -10).RotatedBy(NPC.rotation), SpineCurve[1], SpineCurve[0], BasePosition, i / 15f));

            SpinePositions[(int)i] = Vector2.Lerp(SpinePositions[(int)i], off, v);
        }
    }
    public Vector2 MouthPosition()
    {
        return NPC.Center + ((HeadFrame > 2 ? new Vector2(50, 130) : (HeadFrame < 2 ? new Vector2(-50, 130) : new Vector2(0, 95))).RotatedBy(NPC.rotation));
    }
    public void BreatheFrost(float rotation)
    {
        Asset<Texture2D>[] assets = {
        Assets.Textures.Gallery.Snapdragon.FrostBreathParticle1.Asset,
        Assets.Textures.Gallery.Snapdragon.FrostBreathParticle2.Asset,
        Assets.Textures.Gallery.Snapdragon.FrostBreathParticle3.Asset};

        Vector2 spawnPos = MouthPosition();

        for (int i = 0; i < 20; i++)
            new TextureFlashParticle(spawnPos - ((rotation.ToRotationVector2() * 40f).RotatedByRandom(2f)), (rotation.ToRotationVector2() * 220f).RotatedByRandom(0.4f), Vector2.One, assets[Main.rand.Next(assets.Length)])
            {
                Opacity = 0.2f,
                AffectedByLight = false,
                Pixelated = true,
                Color = Color.White.MultiplyRGBA(new(1f, 1f, 1f, 0f)),
                Scale = new Vector2(3f, 0f),
                UpdateFunction = FrostBreathUpdateFunc,
                ai = [
                0f, 0f, Main.rand.NextFloat(-0.3f, 0.3f)
                ]
            }.Spawn();

        Projectile.NewProjectile(new EntitySource_Parent(NPC, "Snapdragon Frost Breath"), NPC.Center, rotation.ToRotationVector2() * 220, ModContent.ProjectileType<SnapdragonFrostBreathHitbox>(), FrostBreathDamage, 4f);
    }
    public static Particle.ParticleFunction FrostBreathSparkleFunc = P =>
    {
        if (P.velocity.Length() > 2)
            P.Rotation = P.velocity.AngleFrom(Vector2.Zero);
        P.ai[0]++;
        if (P.ai[0] > 0)
        {
            P.velocity = P.velocity.RotatedBy(MathHelper.ToRadians(P.ai[2]) / 4f);
            P.ai[2] *= 0.8f;
        }
        if (P.ai[0] < P.ai[1] && P.ai[0] > 0) P.Scale = Vector2.Lerp(P.Scale, new Vector2(0.7f, 0.05f), 0.4f);
        if (P.ai[0] >= P.ai[1]) P.Scale = Vector2.Lerp(P.Scale, new Vector2(-0.1f, 0.1f), 0.5f);
        if (P.Scale.X < -0.05f) P.Kill();
    };
    public static Particle.ParticleFunction FrostBreathUpdateFunc = P =>
    {
        if (Main.rand.NextBool(20) && P.velocity.Length() > 50)
        {
            float aa = Main.rand.NextFloat(-45f, 45f);
            new TextureFlashParticle(P.Center + new Vector2(Main.rand.NextFloat(-100, 100), Main.rand.NextFloat(-100, 100)).RotatedBy(P.Rotation + aa), P.velocity * 0.5f, Vector2.One, Assets.Textures.Misc.LensFlash.Asset)
            {
                Opacity = 0.5f,
                Pixelated = true,
                AffectedByLight = false,
                Color = Color.AliceBlue.MultiplyRGBA(new(1f, 1f, 1f, 1f)),
                Scale = new Vector2(0.03f, 0f),
                ai = new float[]{
                -Main.rand.NextFloat(2, 4),
                Main.rand.Next(0, 8),
                -aa
            },
                UpdateFunction = FrostBreathSparkleFunc
            }.Spawn();
        }

        P.velocity = P.velocity.RotatedBy(MathHelper.ToRadians(P.ai[2]) / 4f);
        P.ai[2] *= 0.9f;
        P.ai[0]++;
        if (P.ai[0] > 10 && WorldGen.SolidOrSlopedTile(Main.tile[(P.Center / 16).ToPoint()]))
        {
            P.velocity = P.velocity.RotatedBy(-Math.Sign(P.velocity.X) * 0.04f);
            P.velocity *= 0.75f;
            P.Opacity *= 0.75f;
        }
        P.Color = Color.Lerp(Color.Transparent, Color.GhostWhite, P.Opacity);
        P.Color = Color.Lerp(P.Color, Color.Transparent, P.Opacity * 0.5f);
        P.Color = Color.Lerp(P.Color, Color.Transparent, 0.7f);
        P.Rotation = Vector2.Zero.AngleTo(P.velocity);
        P.Scale = Vector2.Lerp(P.Scale, new Vector2(2f, 2f), 0.3f);
        if (P.ai[0] > 10)
        {
        }
        if (P.Opacity < 0.01f) P.Kill();
    };
    public void ClampPositioning()
    {
        float maxDist = NPC.Center.Distance(BasePosition);
        maxDist = MathHelper.Clamp(maxDist, 200, 250);

        NPC.VelocityMoveTowardsPosition(Target.Center + new Vector2(-20f + (float)(Math.Sin(NPC.ai[0] / 60) * 100f), -220 + (float)(Math.Sin(NPC.ai[0] / 63) * 20f)), 0.1f, 0.05f, 20);

        Vector2 outPos = new Vector2(maxDist, 0).RotatedBy(BasePosition.AngleTo(NPC.Center));

        NPC.Center = Vector2.Lerp(NPC.Center, BasePosition + (outPos * new Vector2(0.7f, 1.0f)), 0.3f);
    }
    public override void HitEffect(NPC.HitInfo hit)
    {
        if (!Main.dedServ) SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_Hit.Asset.WithPitchVariance(0.1f).WithVolumeScale(0.5f), NPC.Center);
    }
    public Particle.ParticleFunction IceShardFunction = P =>
    {
        P.Rotation += P.velocity.X / 8f;
        P.velocity.Y += 0.2f;
        P.ai[0]++;
        if (P.ai[0] > 10)
            P.Scale = Vector2.Lerp(P.Scale, new Vector2(-0.2f, -0.2f), 0.05f);
        if (P.Scale.X < 0f) P.Kill();
    };
    public void ThrowShardParticle(Vector2 pos, Vector2 vel, float scale = 1f)
    {
        new IceSpikeTextureFlashParticle(pos, vel, new Vector2(Main.rand.NextFloat(0.1f, 0.6f), Main.rand.NextFloat(0.1f, 0.6f)) * scale, Assets.Textures.Gallery.Snapdragon.SnapdragonIceShard.Asset)
        {
            UpdateFunction = IceShardFunction
        }.Spawn(SnapdragonIceSpikeSystem.IceLayer);
    }
    public void ThrowTileReplicants(NPC npc, Point pt, int amt, int delay = 6)
    {
        for (int i = -amt; i <= amt; i++)
        {
            for (int j = -3; j <= 5; j++)
            {
                Vector2 position = new Vector2((pt.X + i) * 16, (pt.Y + j) * 16);

                if (WorldGen.InWorld(pt.X + i, pt.Y + j, 100))
                {
                    Tile t = Main.tile[pt.X + i, pt.Y + j];
                    if (WorldGen.SolidOrSlopedTile(t) && !t.IsActuated)
                    {
                        StillTileReplicantParticle tile0 = new StillTileReplicantParticle(t.TileType, new Rectangle(t.TileFrameX, t.TileFrameY, 16, 16), position + new Vector2(8, 8), Vector2.Zero, Vector2.One)
                        {
                            HideTimer = 60
                        };
                        tile0.Spawn();
                        WorldGen.KillTile_MakeTileDust(pt.X + i, pt.Y + j, t);
                        if (!WorldGen.SolidOrSlopedTile(Main.tile[pt.X + i, pt.Y + j - 2]))
                        {
                            Vector2 altVel = npc.velocity;
                            altVel.Normalize();
                            TileReplicantParticle tile = new TileReplicantParticle(t.TileType, new Rectangle(t.TileFrameX, t.TileFrameY, 16, 16), position + new Vector2(8, 8), new Vector2(0, Easing.KeyFloat(Math.Abs(i), 0f, amt, -5f, 0, Easing.Linear, 0)), Vector2.One, P =>
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
                                HideTimer = Math.Abs(i) * delay,
                                StartingY = position.Y + 8
                            };
                            tile.Spawn();
                        }
                    }
                }
            }
        }
    }

    public static ParticleLayer FrostBreathLayer = new();

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Asset<Texture2D> HeadTex = Assets.Textures.Gallery.Snapdragon.Snapdragon_Head.Asset;
        Asset<Texture2D> JawTex = Assets.Textures.Gallery.Snapdragon.Snapdragon_Jaw.Asset;
        Asset<Texture2D> CollarTex = Assets.Textures.Gallery.Snapdragon.Snapdragon_Collar.Asset;

        if (SpineVisible)
        {
            DrawSpine();
            DrawSpine(true);
        }

        if (HeadOpacity > 0f)
        {
            float dir = HeadFrame < 3 ? -1 : 1;

            Rectangle collarFrame = CollarTex.Frame(5, frameX: (int)Math.Round(HeadFrame / 7 * 5));
            Rectangle headFrame = HeadTex.Frame(7, 13, (int)Math.Round(HeadFrame), (int)Math.Round(HeadFrame == 0 || HeadFrame == 6 ? JawOpening : 0));
            float jawRot = 0f;
            if (HeadFrame != 0 && HeadFrame != 6) jawRot += (dir * 0.06f * (JawOpening - 6));
            else jawRot += (dir * 0.02f * (JawOpening - 6));
            Main.EntitySpriteDraw(CollarTex.Value, HeadOffset + NPC.Center + new Vector2(0, 10) - screenPos + (NPC.DirectionTo(BasePosition) * 10), collarFrame, drawColor.MultiplyRGBA(new(HeadOpacity, HeadOpacity, HeadOpacity, HeadOpacity)), 0f, collarFrame.Size() / 2f, 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(JawTex.Value, HeadOffset + NPC.Center - screenPos + new Vector2(0, 25), headFrame, drawColor.MultiplyRGBA(new(HeadOpacity, HeadOpacity, HeadOpacity, HeadOpacity)), NPC.rotation + jawRot, headFrame.Size() / 2f, 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(HeadTex.Value, HeadOffset + NPC.Center - screenPos + new Vector2(0, 25), headFrame, drawColor.MultiplyRGBA(new(HeadOpacity, HeadOpacity, HeadOpacity, HeadOpacity)), NPC.rotation, headFrame.Size() / 2f, 1f, SpriteEffects.None);
        }

        int x1 = (int)(NPC.Center.X / 16) - 5;
        int x2 = (int)(NPC.Center.X / 16) + 5;

        for (int i = x1 - ((int)BasePosition.X / 16); i <= x2 - ((int)BasePosition.X / 16); i++)
        {
            for (int j = 0; j <= 4; j++)
            {
                Point p = ((BasePosition / 16) + new Vector2(i, j)).ToPoint();
                if (p.X > 100 && p.X < Main.maxTilesX - 100)
                    if (p.Y > 100 && p.Y < Main.maxTilesY - 100 && Main.tile[p].HasUnactuatedTile)
                        Main.instance.TilesRenderer.DrawSingleTile(new TileDrawInfo(), true, 0, screenPos, Vector2.Zero, p.X, p.Y);
            }
        }

        x1 = (int)(BasePosition.X / 16) - 5;
        x2 = (int)(BasePosition.X / 16) + 5;

        for (int i = x1 - ((int)BasePosition.X / 16); i <= x2 - ((int)BasePosition.X / 16); i++)
        {
            for (int j = 0; j <= 4; j++)
            {
                Point p = ((BasePosition / 16) + new Vector2(i, j)).ToPoint();
                if (p.X > 100 && p.X < Main.maxTilesX - 100)
                    if (p.Y > 100 && p.Y < Main.maxTilesY - 100 && Main.tile[p].HasUnactuatedTile)
                        Main.instance.TilesRenderer.DrawSingleTile(new TileDrawInfo(), true, 0, screenPos, Vector2.Zero, p.X, p.Y);
            }
        }

        return false;
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        boundingBox = new Rectangle((int)NPC.Center.X - 60, (int)NPC.Center.Y - 80, 120, 160);
        Rectangle bb = new Rectangle((int)BasePosition.X - 30, (int)BasePosition.Y - 30, 60, 60);
        boundingBox = Rectangle.Union(boundingBox, bb);
        base.ModifyHoverBoundingBox(ref boundingBox);
    }

    public int NumSpineSegmentsActive = 0;

    public void DrawSpine(bool topLayer = false)
    {
        Asset<Texture2D> NeckTex = Assets.Textures.Gallery.Snapdragon.Snapdragon_Neck.Asset;
        Asset<Texture2D> SpineTex1 = Assets.Textures.Gallery.Snapdragon.Snapdragon_Spine1.Asset;
        Asset<Texture2D> SpineTex2 = Assets.Textures.Gallery.Snapdragon.Snapdragon_Spine2.Asset;
        Asset<Texture2D> CollarSpineTex = Assets.Textures.Gallery.Snapdragon.Snapdragon_SpineCollared.Asset;
        Asset<Texture2D> CollarSpineTex_Top = Assets.Textures.Gallery.Snapdragon.Snapdragon_SpineCollared_Top.Asset;

        for (int i = 0; i < SpinePositions.Length; i++)
        {
            var t = i % 2 == 0 ? SpineTex1 : SpineTex2;
            if ((i + 1) % 4 == 3 && i < SpinePositions.Length - 2) t = topLayer ? CollarSpineTex_Top : CollarSpineTex;
            if (i == 1) t = NeckTex;

            Rectangle f = t.Frame(5, frameX: 2);

            Vector2 lastPos = NPC.Center;
            if (i > 0) lastPos = SpinePositions[i - 1];
            Color c = Lighting.GetColor((SpinePositions[i] / 16).ToPoint());
            if (!topLayer || t == CollarSpineTex_Top)
            {
                float rot = SpinePositions[i].AngleTo(lastPos) + MathHelper.ToRadians(90f);

                float lerpFactor = Easing.KeyFloat((float)i, 0, SpinePositions.Length, 0.2f, 0.005f, Easing.OutCirc);
                if (!topLayer && !Main.gameInactive)
                    SpineFrames[i] = MathHelper.Lerp(SpineFrames[i], (float)MathHelper.Clamp(2 + (MathHelper.ToRadians((NPC.Center.X - BasePosition.X) * 1.2f)), 0, 4), lerpFactor);

                f = t.Frame(5, frameX: (int)Math.Round(SpineFrames[i]));
                Vector2 p = SpinePositions[i] + new Vector2(0, 20) - Main.screenPosition + (NPC.DirectionTo(BasePosition) * 10);
                if ((p + Main.screenPosition).Y < BasePosition.Y + 20)
                    Main.EntitySpriteDraw(t.Value, p, f, c, SpinePositions[i].AngleTo(lastPos) + MathHelper.ToRadians(90f), f.Size() / 2f, 1f, SpriteEffects.None);
            }
        }
    }

    public void CurveAnimation_Idle()
    {
        float rot = -NPC.rotation * 3f;

        rot = MathHelper.Clamp(rot, -0.5f, 0.5f);

        SpineCurve[0] = Vector2.Lerp(SpineCurve[0], Vector2.Lerp(NPC.Center, BasePosition, 0.6f) + (NPC.DirectionTo(BasePosition) * (-50 * Math.Abs(NPC.rotation) * 0.1f)).RotatedBy(-rot) + new Vector2(0, 40), 0.2f);
        SpineCurve[1] = Vector2.Lerp(SpineCurve[1], NPC.Center + (NPC.DirectionTo(BasePosition) * 200).RotatedBy(rot) + new Vector2(MathHelper.ToDegrees(NPC.rotation * 0.1f), -100) + new Vector2(MathHelper.ToDegrees(NPC.rotation), 0), 0.35f);

        SpineCurve[0] += new Vector2((float)Math.Sin(GlobalTimer.Value / 25f) * 2f, (float)Math.Sin(GlobalTimer.Value / 24f) * 2f);
        SpineCurve[1] -= new Vector2((float)Math.Sin(GlobalTimer.Value / 15f) * 2f, (float)Math.Sin(GlobalTimer.Value / 14f) * 2f);
    }

    public void CurveAnimation_Active()
    {
        float rot = -NPC.rotation * 3f;

        rot = MathHelper.Clamp(rot, -0.5f, 0.5f);

        SpineCurve[0] = Vector2.Lerp(SpineCurve[0], Vector2.Lerp(NPC.Center, BasePosition, 0.6f) + (NPC.DirectionTo(BasePosition) * (-50 * Math.Abs(NPC.rotation) * 0.1f)).RotatedBy(-rot) + new Vector2(0, 40), 0.2f);
        SpineCurve[1] = Vector2.Lerp(SpineCurve[1], NPC.Center + (NPC.DirectionTo(BasePosition) * 200).RotatedBy(rot) + new Vector2(MathHelper.ToDegrees(NPC.rotation * 0.1f), -100) + new Vector2(MathHelper.ToDegrees(NPC.rotation), 0), 0.35f);

        SpineCurve[0] += new Vector2((float)Math.Sin(GlobalTimer.Value / 25f) * 2f, (float)Math.Sin(GlobalTimer.Value / 24f) * 2f);
        SpineCurve[1] -= new Vector2((float)Math.Sin(GlobalTimer.Value / 15f) * 2f, (float)Math.Sin(GlobalTimer.Value / 14f) * 2f);
    }

    public float FrostBreathTimer = 0f;
    public float FrostBreathWidth = 0f;
    public float FrostBreathLength = 0f;
    public float FrostBreathAngle = 0f;
}
