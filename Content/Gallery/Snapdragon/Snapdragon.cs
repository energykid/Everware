using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.NPCs;
using Everware.Content.Base.ParticleSystem;
using Everware.Utils;
using System;

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
    public float HeadOpacity = 0f;
    public bool start = false;
    public Vector2 TargetPosition = Vector2.Zero;
    public static int FrostBreathDamage => 40;
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
        NPC.ai[0] = NPC.ai[1] = 0; CurrentAttack = stateTo;
    }
    public AttackState CurrentAttack = AttackState.Intro;
    public override void SetDefaults()
    {
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
                    ScreenEffects.AddScreenShake(NPC.Center, 5f, 0.7f);
                    ScreenEffects.DimScreen(0.25f);
                    ScreenEffects.ZoomScreen(0.5f);

                    HeadOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));

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

                if (NPC.ai[0] > 140) ChangeAttackState(AttackState.FrostBreath);
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
                    if (NPC.ai[0] == 30) SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_Roar.Asset, NPC.Center);

                    HeadOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));

                    if (HeadFrame != 3) NPC.rotation -= MathHelper.ToRadians((HeadFrame - 3) * (NPC.ai[0] / 20));

                    ScreenEffects.DimScreen(0.25f);
                    ScreenEffects.AddScreenShake(NPC.Center, 4, 0.7f);

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
                    NPC.direction = Target.Center.X > NPC.Center.X ? 1 : -1;
                    HeadFrame = (int)MathHelper.Clamp(3 + (MathHelper.ToRadians((Target.Center.X - NPC.Center.X) * 1.35f)), 0, 6);
                    TargetPosition = Target.Center.Grounded();
                    TargetPosition.X += (NPC.direction * 200);
                    NPC.VelocityMoveTowardsPosition(BasePosition + new Vector2(0, -200), 0.1f, 0.05f, 2);
                }
                else
                {
                    if (NPC.ai[0] == 70)
                    {
                        SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_FrostBreath.Asset, NPC.Center);
                        SoundEngine.PlaySound(Assets.Sounds.NPC.Snapdragon_Emerge.Asset, NPC.Center);
                    }
                    HeadOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));

                    BreatheFrost(NPC.AngleTo(TargetPosition) + (-NPC.direction * MathHelper.ToRadians(NPC.direction * rotOffset)) + MathHelper.ToRadians(NPC.direction * 25));

                    ScreenEffects.DimScreen(MathHelper.Lerp(0.65f, 0f, NPC.ai[0] / 180f));
                    ScreenEffects.AddScreenShake(NPC.Center, 4, 0.7f);

                    NPC.VelocityMoveTowardsPosition(BasePosition + (BasePosition.DirectionTo(Target.Center) * 50) + new Vector2(0, -300), 0.2f, 0.2f);
                }
                if (NPC.ai[0] > 180)
                {
                    ChangeAttackState(AttackState.Idle);
                }
                break;
            #endregion
        }

        if (CurrentAttack != AttackState.Intro)
        {
            HeadOffset *= 0.9f;
        }

        // Check if current attack isn't intro/idle since those two states handle this logic in the switch statement
        if (CurrentAttack != AttackState.Intro && CurrentAttack != AttackState.Idle)
        {
            #region Move the spine positions to their corresponding spots on the curve
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
            #endregion
        }
        else
        {

            NPC.rotation = MathHelper.Lerp(NPC.rotation, MathHelper.Clamp(NPC.rotation, -0.5f, 0.5f), 0.1f);
            NPC.direction = Target.Center.X > NPC.Center.X ? 1 : -1;
        }
    }
    public void BreatheFrost(float rotation)
    {
        Asset<Texture2D>[] assets = {
        Assets.Textures.Misc.Smoke1.Asset,
        Assets.Textures.Misc.Smoke2.Asset,
        Assets.Textures.Misc.Smoke3.Asset};

        Vector2 spawnPos = NPC.Center + ((HeadFrame > 2 ? new Vector2(60, 110) : (HeadFrame < 2 ? new Vector2(-60, 110) : new Vector2(0, 95))).RotatedBy(NPC.rotation));

        new TextureFlashParticle(spawnPos + (rotation.ToRotationVector2() * 20f), (rotation.ToRotationVector2() * 220f).RotatedByRandom(0.01f), Vector2.One, assets[Main.rand.Next(assets.Length)])
        {
            Opacity = 1f,
            Pixelated = true,
            AffectedByLight = true,
            Color = Color.White,
            Scale = new Vector2(1f, 0.1f),
            UpdateFunction = FrostBreathUpdateFunc
        }.Spawn();

        Projectile.NewProjectile(new EntitySource_Parent(NPC, "Snapdragon Frost Breath"), NPC.Center, rotation.ToRotationVector2() * 220, ModContent.ProjectileType<SnapdragonFrostBreathHitbox>(), FrostBreathDamage, 4f);
    }
    public static Particle.ParticleFunction FrostBreathSparkleFunc = P =>
    {
        if (P.velocity.Length() > 2)
            P.Rotation = P.velocity.AngleFrom(Vector2.Zero);
        P.ai[0]++;
        if (P.ai[0] < P.ai[1]) P.Scale = Vector2.Lerp(P.Scale, new Vector2(0.3f, P.ai[2]), 0.4f);
        else P.Scale = Vector2.Lerp(P.Scale, new Vector2(-0.1f, 0.1f), 0.5f);
        if (P.Scale.X < -0.05f) P.Kill();
    };
    public static Particle.ParticleFunction FrostBreathUpdateFunc = P =>
    {
        if (Main.rand.NextBool(5) && P.velocity.Length() > 50)
        {
            new TextureFlashParticle(P.Center + new Vector2(Main.rand.NextFloat(-200, 200), Main.rand.NextFloat(-40, 40)).RotatedBy(P.Rotation), P.velocity * 0.5f, Vector2.One, Assets.Textures.Misc.LensFlash.Asset)
            {
                Opacity = 0.5f,
                Pixelated = true,
                AffectedByLight = true,
                Color = Color.AliceBlue.MultiplyRGBA(new(1f, 1f, 1f, 0f)),
                Scale = new Vector2(0.1f, 0f),
                ai = new float[]{
                0f,
                Main.rand.Next(5, 10),
                Main.rand.NextFloat(0.15f, 0.25f)
            },
                UpdateFunction = FrostBreathSparkleFunc
            }.Spawn();
        }

        P.ai[0]++;
        if (P.ai[0] > 10)
            P.velocity = P.velocity.RotatedBy(-Math.Sign(P.velocity.X) * 0.04f);
        P.velocity *= 0.8f;
        P.Color = Color.Lerp(Color.Transparent, Color.GhostWhite, P.Opacity);
        P.Color = Color.Lerp(P.Color, Color.Transparent, P.Opacity * 0.5f);
        P.Color = Color.Lerp(P.Color, Color.Transparent, 0.7f);
        P.Rotation = Vector2.Zero.AngleTo(P.velocity);
        P.Scale = Vector2.Lerp(P.Scale, new Vector2(Math.Max(1f / 256f * P.velocity.Length(), 0.4f), 0.5f), 0.3f);
        P.Opacity *= 0.9f;
        if (P.Opacity < 0.05f) P.Kill();
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
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Asset<Texture2D> HeadTex = Assets.Textures.Gallery.Snapdragon.Snapdragon_Head.Asset;
        Asset<Texture2D> CollarTex = Assets.Textures.Gallery.Snapdragon.Snapdragon_Collar.Asset;

        DrawSpine();
        DrawSpine(true);

        for (int i = -4; i <= 4; i++)
        {
            for (int j = 0; j <= 4; j++)
            {
                Point p = ((BasePosition / 16) + new Vector2(i, j)).ToPoint();
                if (p.X > 100 && p.X < Main.maxTilesX - 100)
                    if (p.Y > 100 && p.Y < Main.maxTilesY - 100 && Main.tile[p].HasUnactuatedTile)
                        Main.instance.TilesRenderer.DrawSingleTile(new TileDrawInfo(), true, 0, screenPos, Vector2.Zero, p.X, p.Y);
            }
        }

        if (HeadOpacity > 0f)
        {
            Rectangle collarFrame = CollarTex.Frame(5, frameX: (int)Math.Round(HeadFrame / 7 * 5));
            Rectangle headFrame = HeadTex.Frame(7, frameX: (int)Math.Round(HeadFrame));
            Main.EntitySpriteDraw(CollarTex.Value, HeadOffset + NPC.Center + new Vector2(0, 10) - screenPos + (NPC.DirectionTo(BasePosition) * 10), collarFrame, drawColor.MultiplyRGBA(new(HeadOpacity, HeadOpacity, HeadOpacity, HeadOpacity)), 0f, collarFrame.Size() / 2f, 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(HeadTex.Value, HeadOffset + NPC.Center - screenPos + new Vector2(0, 25), headFrame, drawColor.MultiplyRGBA(new(HeadOpacity, HeadOpacity, HeadOpacity, HeadOpacity)), NPC.rotation, headFrame.Size() / 2f, 1f, SpriteEffects.None);
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
}
