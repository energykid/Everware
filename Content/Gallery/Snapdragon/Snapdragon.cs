using Everware.Content.Base;
using Everware.Content.Base.NPCs;
using Everware.Content.Base.ParticleSystem;
using Everware.Utils;
using System;
using System.IO;

namespace Everware.Content.Gallery.Snapdragon;

[AutoloadBossHead]
public partial class Snapdragon : ModNPC
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
    bool Roaring = false;
    public float HeadOpacity = 0f;
    public bool SpineVisible = true;
    public bool start = false;
    public Vector2 TargetPosition = Vector2.Zero;
    public bool CenteredForAttacks = true;
    public static int FrostBreathDamage => 80;
    public static int IceSpikeDamage => 35;
    public static int BiteDamage => 95;
    public enum AttackState
    {
        Intro,

        Idle, // Idle animation, do nothing then decide on an attack
        Roar, // Roar, knocking the player back but otherwise doing nothing

        Bite, // Try to bite the player
        Burrow, // Relocate to a different part of the arena, causing icicles to fall while doing so
        BeakBash, // Slam beak-first into the ground while facing the player, causing debris to come from the ground
        FrostBreath, // Breathe frost at the player then rotate upwards, filling an entire side of the arena with solid, spiky ice and
                     // forcing them to swerve behind the boss
        SnapFreeze, // Fire bursts of cold air that instantly freeze into icicles
    }
    public void ChangeAttackState(AttackState stateTo)
    {
        NPC.ai[0] = 0; NPC.ai[1] = 0;

        CurrentAttack = stateTo;
    }
    public void PickAttack()
    {
        AttackState[] states = {
            AttackState.FrostBreath,
            AttackState.SnapFreeze
        };

        ChangeAttackState(states[Main.rand.Next(states.Length)]);

        NPC.netUpdate = true;
    }
    public void TryBurrow()
    {
        if (NPC.Distance(Target.Center) < 400) ChangeAttackState(AttackState.Bite);
        else ChangeAttackState(AttackState.Burrow);
        NPC.netUpdate = true;
    }
    public AttackState CurrentAttack = AttackState.Intro;
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write((int)CurrentAttack);
        writer.WriteVector2(TargetPosition);
        writer.WriteVector2(BasePosition);
        writer.Write(CenteredForAttacks);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        CurrentAttack = (AttackState)reader.ReadInt32();
        TargetPosition = reader.ReadVector2();
        BasePosition = reader.ReadVector2();
        CenteredForAttacks = reader.ReadBoolean();
    }
    #region Boss Properties
    public override void BossHeadSlot(ref int index)
    {
        if (HeadOpacity < 0.5f)
            index = -1;
        base.BossHeadSlot(ref index);
    }
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
    #endregion
    public override void AI()
    {
        NPC.damage = 0;
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
            case AttackState.Intro:
                IntroAttackState();
                break;
            case AttackState.Idle:
                IdleAttackState();
                break;
            case AttackState.Roar:
                RoarAttackState();
                break;
            case AttackState.FrostBreath:
                FrostBreathAttackState();
                break;
            case AttackState.Burrow:
                BurrowAttackState();
                break;
            case AttackState.SnapFreeze:
                SnapFreezeAttackState();
                break;
            case AttackState.Bite:
                BiteAttackState();
                break;
            case AttackState.BeakBash:
                BeakBashAttackState();
                break;
        }

        if (!Roaring)
            JawOpening = MathHelper.Lerp(JawOpening, 1f, 0.2f);

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
    public void PuffFrost(float rotation, Vector2 scaleOfPuff, float scaleOfParticles)
    {
        Asset<Texture2D>[] assets = {
        Assets.Textures.Gallery.Snapdragon.FrostBreathParticle1.Asset,
        Assets.Textures.Gallery.Snapdragon.FrostBreathParticle2.Asset,
        Assets.Textures.Gallery.Snapdragon.FrostBreathParticle3.Asset};

        Vector2 spawnPos = Vector2.Lerp(MouthPosition(), NPC.Center, 0.3f);

        for (int i = 0; i < 20; i++)
            new TextureFlashParticle(spawnPos, (new Vector2(Main.rand.NextFloat(0f, 1f), Main.rand.NextFloat(-1f, 1f)) * scaleOfPuff).RotatedBy(rotation).RotatedByRandom(0.4f), Vector2.One, assets[Main.rand.Next(assets.Length)])
            {
                Opacity = 0.05f,
                AffectedByLight = false,
                Pixelated = true,
                Color = Color.CadetBlue.MultiplyRGBA(new(0.4f, 0.45f, 0.5f, 0.45f)),
                Scale = new Vector2(0.1f, 0.1f) * scaleOfParticles,
                UpdateFunction = FrostPuffUpdateFunc,
                ai = [
                0f, 0f, Main.rand.NextFloat(-0.2f, 0.2f)
                ]
            }.Spawn();
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
    public static Particle.ParticleFunction FrostPuffUpdateFunc = P =>
    {
        P.velocity *= 0.95f;
        P.Opacity *= 0.98f;
        P.Color = Color.Lerp(P.Color, Color.Transparent, 0.05f);
        P.Rotation += P.velocity.X * 0.1f;
        P.Scale = Vector2.Lerp(P.Scale, new Vector2(0.5f, 0.5f), 0.1f);
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
        Asset<Texture2D> HeadGlow = Assets.Textures.Gallery.Snapdragon.Snapdragon_Head_Glow.Asset;
        Asset<Texture2D> JawGlow = Assets.Textures.Gallery.Snapdragon.Snapdragon_Jaw_Glow.Asset;
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
            Rectangle headFrame = HeadTex.Frame(7, 8, (int)Math.Round(HeadFrame), (int)Math.Round(HeadFrame == 0 || HeadFrame == 6 ? JawOpening : 0));
            float jawRot = 0f;
            if (HeadFrame != 0 && HeadFrame != 6) jawRot += (dir * 0.06f * (JawOpening));
            else jawRot += (dir * 0.02f * (JawOpening));
            Vector2 headOff = new Vector2(NPC.direction * 12, 0).RotatedBy(NPC.rotation);
            Main.EntitySpriteDraw(CollarTex.Value, HeadOffset + NPC.Center + new Vector2(0, 10) - screenPos + (NPC.DirectionTo(BasePosition) * 10), collarFrame, drawColor.MultiplyRGBA(new(HeadOpacity, HeadOpacity, HeadOpacity, HeadOpacity)), 0f, collarFrame.Size() / 2f, 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(JawTex.Value, HeadOffset + NPC.Center - screenPos + new Vector2(0, 25) + headOff, headFrame, drawColor.MultiplyRGBA(new(HeadOpacity, HeadOpacity, HeadOpacity, HeadOpacity)), NPC.rotation + jawRot, headFrame.Size() / 2f, 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(JawGlow.Value, HeadOffset + NPC.Center - screenPos + new Vector2(0, 25) + headOff, headFrame, Color.White.MultiplyRGBA(new(HeadOpacity, HeadOpacity, HeadOpacity, HeadOpacity)), NPC.rotation + jawRot, headFrame.Size() / 2f, 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(HeadTex.Value, HeadOffset + NPC.Center - screenPos + new Vector2(0, 25) + headOff, headFrame, drawColor.MultiplyRGBA(new(HeadOpacity, HeadOpacity, HeadOpacity, HeadOpacity)), NPC.rotation, headFrame.Size() / 2f, 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(HeadGlow.Value, HeadOffset + NPC.Center - screenPos + new Vector2(0, 25) + headOff, headFrame, Color.White.MultiplyRGBA(new(HeadOpacity, HeadOpacity, HeadOpacity, HeadOpacity)), NPC.rotation, headFrame.Size() / 2f, 1f, SpriteEffects.None);
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

        rot = Easing.KeyFloat(Math.Abs(NPC.rotation), MathHelper.PiOver2, MathHelper.Pi, rot, 0f, Easing.Linear, rot);

        rot = Math.Abs(rot) * NPC.direction;

        SpineCurve[0] = Vector2.Lerp(SpineCurve[0],
            Vector2.Lerp(NPC.Center, BasePosition, 0.6f) + (NPC.DirectionTo(BasePosition) * (-50 * Math.Abs(NPC.rotation) * 0.1f)).RotatedBy(-rot) + new Vector2(0, 40), 0.2f);
        SpineCurve[1] = Vector2.Lerp(SpineCurve[1],
            NPC.Center + (NPC.DirectionTo(BasePosition) * 200).RotatedBy(rot) + new Vector2(MathHelper.ToDegrees(Math.Abs(NPC.rotation) * NPC.direction * 0.1f), -100) + new Vector2(MathHelper.ToDegrees(0f), 0), 0.35f);

        SpineCurve[0] += new Vector2((float)Math.Sin(GlobalTimer.Value / 25f) * 2f, (float)Math.Sin(GlobalTimer.Value / 24f) * 2f);
        SpineCurve[1] -= new Vector2((float)Math.Sin(GlobalTimer.Value / 15f) * 2f, (float)Math.Sin(GlobalTimer.Value / 14f) * 2f);
    }

    public void CurveAnimation_Active()
    {
        float rot = -NPC.rotation * 3f;

        rot = MathHelper.Clamp(rot, -0.5f, 0.5f);

        rot = Easing.KeyFloat(Math.Abs(NPC.rotation), MathHelper.PiOver2, MathHelper.Pi, rot, 0f, Easing.Linear, rot);

        rot = Math.Abs(rot) * NPC.direction;

        SpineCurve[0] = Vector2.Lerp(SpineCurve[0],
            Vector2.Lerp(NPC.Center, BasePosition, 0.6f) + (NPC.DirectionTo(BasePosition) * (-50 * NPC.direction)).RotatedBy(-rot) + new Vector2(0, 40), 0.2f);
        SpineCurve[1] = Vector2.Lerp(SpineCurve[1],
            NPC.Center + (NPC.DirectionTo(BasePosition) * 200).RotatedBy(rot) + new Vector2(-NPC.direction * 100, -100) + new Vector2(MathHelper.ToDegrees(0f), 0), 0.35f);

        SpineCurve[0] += new Vector2((float)Math.Sin(GlobalTimer.Value / 1.5f) * 1f, (float)Math.Sin(GlobalTimer.Value / 0.75f) * 1f);
        SpineCurve[1] -= new Vector2((float)Math.Sin(GlobalTimer.Value / 2f) * 1f, (float)Math.Sin(GlobalTimer.Value / 0.5f) * 1f);
    }

    public float FrostBreathTimer = 0f;
    public float FrostBreathWidth = 0f;
    public float FrostBreathLength = 0f;
    public float FrostBreathAngle = 0f;
}
