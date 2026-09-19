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

namespace Everware.Content.Meteor.NPCs;

public class Cosmoeba : EverNPC
{
    public ParticleLayer FizzLayer = new();
    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<MeteorBiome>() ? 0.25f : 0f;
    }
    public override string Texture => "Everware/Assets/Textures/Meteor/NPCs/CosmoebaBody";
    public override Vector2 Size => new Vector2(42, 42);
    public override int FrameNumber => 3;
    public override int TrailLength => 10;
    public override int Damage => 10;

    float Personality = Main.rand.NextFloat(-0.2f, 0.2f);

    public override void SetDefaults()
    {
        base.SetDefaults();
        Personality = Main.rand.NextFloat(-0.2f, 0.2f);
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.damage = 0;
        State = (int)BehaviorState.Wandering;
        ExtraAI[0] = -Main.rand.NextFloat(100f);
        NPC.netUpdate = true;
        NPC.HitSound = Assets.Sounds.NPC.CosmoebaHurt.Asset.WithPitchVariance(0.2f) with { MaxInstances = 5 };
        NPC.DeathSound = Assets.Sounds.NPC.CosmoebaKill.Asset.WithPitchVariance(0.1f) with { MaxInstances = 2 };
    }
    public enum BehaviorState
    {
        Wandering,
        CirclingMagicStone,
        Fleeing,
        FindingMeteor,
        Charging
    }

    #region Behavior
    public override void AI()
    {
        if (Main.netMode != NetmodeID.Server)
            FizzLayer.Update();
        NPC.scale = MathHelper.Lerp(NPC.scale, 1f, 0.025f);
        switch (State)
        {
            case (int)BehaviorState.Wandering:
                NPC.TargetClosest(false);

                FleeIfTooClose();

                ExtraAI[0]++;

                if (ExtraAI[0] > 150 && NPC.ai[0] % 5 == 0)
                {
                    var pos = BehaviorUtils.FindNearbyTilePosition(NPC.Center, 30, ModContent.TileType<MagicStoneTile>(), 2);

                    if (pos != null)
                    {
                        AIPosition = pos.Value;
                        SoundEngine.PlaySound(Assets.Sounds.NPC.CosmoebaLocateStone.Asset.WithPitchVariance(0.2f) with { MaxInstances = 5 }, NPC.Center);
                        ChangeState(BehaviorState.CirclingMagicStone);
                        break;
                    }
                }
                if (ExtraAI[1] > 0)
                {
                    AIPosition = NPC.Center + new Vector2(Main.rand.NextFloat(80, 300), 0).RotatedByRandom(MathHelper.TwoPi);
                    ExtraAI[1] = -Main.rand.NextFloat(120, 480);
                    NPC.netUpdate = true;
                }
                ExtraAI[1]++;
                NPC.ai[2]++;
                AIPosition += new Vector2((float)Math.Sin(NPC.ai[2] / 14f) * 6f, (float)Math.Sin(NPC.ai[2] / 12.5f) * 6f);

                if (NPC.ai[0] % 50 == 0)
                {
                    var pos = BehaviorUtils.FindNearbyTilePosition(NPC.Center, 300, ModContent.TileType<MagicStoneTile>(), 12);

                    if (pos != null)
                    {
                        AIPosition = Vector2.Lerp(AIPosition, pos.Value, 0.2f);
                        break;
                    }
                }

                NPC.rotation = Vector2.Zero.AngleTo(NPC.velocity) + MathHelper.Pi;

                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Lerp(NPC.Center, AIPosition, 0.005f) - NPC.Center, 0.05f);

                break;
            case (int)BehaviorState.CirclingMagicStone:
                NPC.TargetClosest(false);

                FleeIfTooClose();

                NPC.ai[2]++;

                NPC.ai[1] += 2f;

                NPC.rotation = NPC.Center.AngleTo(AIPosition) + MathHelper.Pi;

                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Lerp(NPC.Center, AIPosition + new Vector2(8, 0).RotatedBy(NPC.rotation) + new Vector2((float)Math.Sin(NPC.ai[2] / 14f) * 7f, (float)Math.Sin(NPC.ai[2] / 12.5f) * 7f), 0.015f) - NPC.Center, 0.1f);
                break;
            case (int)BehaviorState.Fleeing:
                ExtraAI[0] = MathHelper.Lerp(ExtraAI[0], 3f, 0.2f);
                NPC.velocity = Vector2.Lerp(NPC.velocity, (Target.DirectionTo(NPC.Center) * ExtraAI[0]) + new Vector2(0, -0.25f * ExtraAI[0]), 0.2f);
                NPC.TargetClosest(false);

                NPC.ai[1] += 2.5f;
                NPC.velocity = NPC.velocity.RotatedBy(MathHelper.ToRadians((float)Math.Sin(NPC.ai[1] / 20f) * 7f));
                NPC.rotation = Vector2.Zero.AngleTo(NPC.velocity) + MathHelper.Pi;

                if (NPC.ai[2] > (15 * 12) || NPC.life < NPC.lifeMax / 2)
                {
                    FindMeteor();
                }

                NPC.ai[2]++;
                if (NPC.ai[2] > 25 && NPC.ai[2] % 15 < 1)
                {
                    Vector2 v = new Vector2(20, 0).RotatedByRandom(MathHelper.TwoPi);

                    new PanicParticle(NPC.Center + v, v / 16f, NPC.whoAmI).Spawn();

                    SoundEngine.PlaySound(Assets.Sounds.NPC.CosmoebaFleeLoop.Asset with { MaxInstances = 3, Pitch = Personality }, NPC.Center);
                }

                if (NPC.Distance(Target.Center) > 200 && NPC.ai[2] > 50)
                    ChangeState(BehaviorState.Wandering);
                break;
            case (int)BehaviorState.FindingMeteor:
                NPC foundMeteorHead = Main.npc[(int)ExtraAI[1]];
                if (!foundMeteorHead.active || foundMeteorHead.life <= 0)
                {
                    ChangeState(BehaviorState.Wandering);
                }
                else
                {
                    if (foundMeteorHead.Distance(NPC.Center) < 30)
                    {
                        float ScaleLerp = Easing.KeyFloat(FireAmount, 0f, 0.5f, 0f, 1f, Easing.InSine);
                        ScaleLerp = Easing.KeyFloat(FireAmount, 0.5f, 1f, 1f, 0f, Easing.OutSine, ScaleLerp);

                        if (foundMeteorHead.ai[2] == 0)
                        {
                            foundMeteorHead.ai[2]++;
                        }

                        if (NPC.ai[2] % 3 < 1)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_LightningBugZap.WithPitchOffset(-1f + (NPC.ai[2] / 50f)).WithPitchVariance(0.3f).WithVolumeScale(0.2f), NPC.Center);
                        }

                        FireAmount += 0.02f;

                        new FizzParticle(NPC.Center + new Vector2(Main.rand.NextFloat(-20, 20), 0).RotatedByRandom(MathHelper.TwoPi), new Vector2(0, -ScaleLerp * 2f), NPC.whoAmI) { Scale = new Vector2(ScaleLerp, ScaleLerp) }.Spawn(FizzLayer);

                        var d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f);
                        d.scale = ScaleLerp * 2f;
                        d.noGravity = true;

                        NPC.velocity *= 0.8f;
                        NPC.scale = MathHelper.Lerp(NPC.scale, 1.4f, 0.05f);
                        foundMeteorHead.Center = Vector2.Lerp(foundMeteorHead.Center, NPC.Center, 0.3f);
                        BodyFrame = MathHelper.Lerp(BodyFrame, 2f, 0.1f);
                        foundMeteorHead.velocity *= 0.8f;
                        if (NPC.ai[2] <= 2)
                        {
                            SoundEngine.PlaySound(Assets.Sounds.NPC.CosmoebaEatMeteorHead.Asset.WithVolumeScale(1.3f), NPC.Center);
                        }
                        foundMeteorHead.ai[2]++;
                        NPC.ai[2]++;
                        if (NPC.ai[2] > 50)
                        {
                            foundMeteorHead.active = false;
                            NPC.TargetClosest(false);
                            SoundEngine.PlaySound(Assets.Sounds.NPC.CosmoebaCharge.Asset.WithPitchVariance(0.2f).WithVolumeScale(2f), NPC.Center);
                            NPC.velocity = NPC.DirectionTo(Target.Center) * 2f;
                            ChangeState(BehaviorState.Charging);
                        }
                    }
                    else
                    {
                        NPC.ai[2] = MathHelper.Lerp(NPC.ai[2], 10f, 0.05f);
                        NPC.ai[1] += 1.5f;
                        NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(foundMeteorHead.Center) * NPC.ai[2], 0.3f);
                        NPC.velocity = NPC.velocity.RotatedBy(MathHelper.ToRadians((float)Math.Sin(NPC.ai[1] / 20f) * 7f));
                        NPC.rotation = Vector2.Zero.AngleTo(NPC.velocity) + MathHelper.Pi;
                    }
                }
                break;
            case (int)BehaviorState.Charging:

                Vector2 pos2 = NPC.Center + new Vector2(14, 0).RotatedBy(NPC.rotation).RotatedByRandom(MathHelper.PiOver4);
                Dust d2 = Dust.NewDustPerfect(pos2, DustID.FlameBurst, Vector2.Zero);
                d2.noGravity = true;
                Dust d3 = Dust.NewDustPerfect(pos2, DustID.Smoke, NPC.velocity * 0.5f);
                d3.noGravity = true;

                NPC.rotation = Vector2.Zero.AngleTo(NPC.velocity) + MathHelper.Pi;
                if (NPC.Distance(Target.Center) < 60 || NPC.ai[2] > 0)
                {
                    if (NPC.ai[2] % 3 < 1)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_LightningBugZap.WithPitchOffset(-1f + (NPC.ai[2] / 15f)).WithPitchVariance(0.3f).WithVolumeScale(0.2f), NPC.Center);
                    }

                    NPC.ai[1] += 2;
                    NPC.velocity *= 0.9f;
                    NPC.ai[2]++;
                    BodyFrame = MathHelper.Lerp(BodyFrame, 3f, 0.1f);

                    new FizzParticle(NPC.Center + new Vector2(Main.rand.NextFloat(-20, 20), 0).RotatedByRandom(MathHelper.TwoPi), new Vector2(Main.rand.Next(2), 0).RotatedByRandom(MathHelper.TwoPi), NPC.whoAmI) { Scale = new Vector2(0.5f) }.Spawn(FizzLayer);

                    if (NPC.ai[2] > 30)
                    {
                        Explode();
                    }
                }
                else
                {
                    BodyFrame = MathHelper.Lerp(BodyFrame, 0f, 0.2f);
                    NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Target.Center) * 10f, 0.05f);
                }
                break;
        }

        NPC.ai[0]++;
        NPC.ai[1] += NPC.velocity.Length();
        float spd = 1f + ((float)Math.Sin(NPC.ai[0] / 30f) * 0.7f);
        base.AI();
        WaveTail();
    }
    public void Explode()
    {
        Projectile.NewProjectile(new EntitySource_Parent(NPC, "Cosmoeba Explosion"),
        NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmoebaExplosion>(), 40, 5f);

        NPC.active = false;
        SoundEngine.PlaySound(Assets.Sounds.NPC.CosmoebaExplode.Asset.WithPitchVariance(0.2f), NPC.Center);
    }
    public void WaveTail()
    {
        for (int i = 0; i < NPC.oldPos.Length; i++)
        {
            NPC.oldPos[i] += NPC.rotation.ToRotationVector2() * 3f;
        }

        for (int i = 1; i < NPC.oldPos.Length; i++)
        {
            if (i > 0)
            {
                float ang = NPC.oldPos[i - 1].AngleTo(NPC.oldPos[i]);
                NPC.oldPos[i] = NPC.oldPos[i - 1] + new Vector2(MathHelper.Clamp(NPC.oldPos[i - 1].Distance(NPC.oldPos[i]), 2, 4), 0).RotatedBy(ang);
                NPC.oldPos[i] += new Vector2(0, (float)Math.Sin(((NPC.ai[0] / 6f) + (NPC.ai[1] / 10f) / 3f) + ((float)-i)) * MathHelper.Clamp(NPC.velocity.Length() / 3f, 1f, 3f)).RotatedBy(ang);
            }
        }
    }
    public void FleeIfTooClose()
    {
        if (NPC.Distance(Target.Center) < 100)
        {
            StartFleeing();
        }
    }
    public void FindMeteor()
    {
        if (State != (int)BehaviorState.FindingMeteor)
        {
            NPC? MeteorHead = PathfindingUtils.GetClosestNPC(NPC.position, 800, NPCID.MeteorHead);

            if (MeteorHead != null)
            {
                AbsorbedMeteorHead = MeteorHead;
                MeteorHead.ai[2] = 1;
                MeteorHead.ai[1] = NPC.whoAmI;
                MeteorHead.netUpdate = true;
                ExtraAI[1] = MeteorHead.whoAmI;
                ChangeState(BehaviorState.FindingMeteor);
            }
        }
    }
    public void StartFleeing()
    {
        if (State != (int)BehaviorState.Charging && State != (int)BehaviorState.FindingMeteor && State != (int)BehaviorState.Fleeing)
        {
            Vector2 v = new Vector2(20, 0).RotatedByRandom(MathHelper.TwoPi);
            new PanicParticle(NPC.Center + v, v / 16f, NPC.whoAmI).Spawn();
            ExtraAI[0] = 10;
            if (State != (int)BehaviorState.Fleeing)
                SoundEngine.PlaySound(Assets.Sounds.NPC.CosmoebaFlee.Asset with { MaxInstances = 3, Pitch = Personality }, NPC.Center);
            ChangeState(BehaviorState.Fleeing);
        }
    }
    public void ChangeState(BehaviorState state)
    {
        NPC.netUpdate = true;
        State = (int)state;
        NPC.ai[2] = 0;
    }
    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitByProjectile(projectile, hit, damageDone);
        StartFleeing();
    }
    #endregion

    #region Drawing
    float BodyFrame = 0;
    NPC? AbsorbedMeteorHead = null;
    float FireAmount = 0f;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
        {
            NPC.velocity = new Vector2(-2, 0);
            NPC.ai[0]++;
            NPC.ai[1] += NPC.velocity.Length() / 2f;
            for (float i = 0; i < NPC.oldPos.Length; i++)
            {
                NPC.oldPos[(int)i] = NPC.position + new Vector2(i * 3f, MathHelper.Lerp(0f, (float)Math.Sin((i / 2f) + (-NPC.ai[0] / 4f)) * 4f, i / (float)NPC.oldPos.Length));
            }
        }

        if (AbsorbedMeteorHead != null && AbsorbedMeteorHead.active)
        {
            MeteorHeadRework.Draw(AbsorbedMeteorHead, screenPos, drawColor);
        }

        var Body = Assets.Textures.Meteor.NPCs.CosmoebaBody.Asset;
        var BodyInternal = Assets.Textures.Meteor.NPCs.CosmoebaBodyInternal.Asset;
        var Feelers = Assets.Textures.Meteor.NPCs.CosmoebaFeelers.Asset;
        var Stars = Assets.Textures.Meteor.NPCs.CosmoebaStars.Asset;

        int fr = FireAmount > 0.5f ? 1 : 0;

        Rectangle bodyFrame = Body.Frame(2, 3, fr, (int)BodyFrame);

        Rectangle feelerFrame = Feelers.Frame(2, 4, fr, (int)(((NPC.ai[1] / 20) + (GlobalTimer.Value / 20)) % 4));

        List<Vector2> p = NPC.oldPos.ToList();

        for (int i = 0; i < p.Count; i++)
        {
            p[i] -= NPC.position;
            p[i] /= 2f;

            p[i] += new Vector2(100, 100);
        }

        var StarShader = Assets.Effects.Meteor.NPCs.CosmoebaStars.CreateEffect();
        StarShader.Parameters.StarTexture = Assets.Textures.Meteor.NPCs.CosmoebaStars.Asset.Value;
        StarShader.Parameters.Progress = -NPC.ai[1] / 200f;
        StarShader.Apply();

        Main.spriteBatch.End(out var sb);
        Main.spriteBatch.Begin(sb with { SamplerState = Main.DefaultSamplerState });

        var target = RenderTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, 200, 200);

        Color c1 = new Color(43, 101, 180);
        Color c2 = new Color(136, 224, 255);

        if (FireAmount > 0.5f)
        {
            c1 = new Color(255, 124, 0);
            c2 = new Color(255, 255, 0);
        }

        using (target.Scope(clearColor: Color.Transparent))
        {
            PrimitiveDrawing.DrawPrimitiveTrail(new Vector2(200, 200), p, 20, a => { return MathHelper.Lerp(1.2f, 0.7f, a); }, colors: (a, b) =>
            {
                return c1;
            });
            PrimitiveDrawing.DrawPrimitiveTrail(new Vector2(200, 200), p, 20, a => { return MathHelper.Lerp(1, 0.5f, a); }, colors: (a, b) =>
            {
                return c2;
            });
        }

        Vector2 off = new Vector2(20, 0).RotatedBy(-NPC.rotation);

        Main.spriteBatch.Restart(sb with { SamplerState = Main.DefaultSamplerState });

        Main.EntitySpriteDraw(target.Target, NPC.Center - screenPos + new Vector2(10, 0).RotatedBy(NPC.rotation), target.Target.Bounds, Color.White, 0f, target.Target.Bounds.Size() / 2f, 2f * NPC.scale, SpriteEffects.None);

        Main.EntitySpriteDraw(Body.Value, NPC.Center - screenPos, bodyFrame, Color.White, NPC.rotation, bodyFrame.Size() / 2f, NPC.scale, SpriteEffects.None);

        Main.EntitySpriteDraw(Feelers.Value, NPC.Center - screenPos, feelerFrame, Color.White, NPC.rotation, new Vector2(feelerFrame.Width + 6, feelerFrame.Height / 2f), NPC.scale, SpriteEffects.None);

        Main.spriteBatch.Restart(sb with { CustomEffect = StarShader.Shader });

        Main.EntitySpriteDraw(BodyInternal.Value, NPC.Center - screenPos, bodyFrame, Color.White.MultiplyRGBA(new(0.5f, 0.5f, 0.5f, 0.5f)), NPC.rotation, bodyFrame.Size() / 2f, NPC.scale, SpriteEffects.None);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(sb);

        FizzLayer.Draw();

        target.Dispose();

        return false;
    }
    #endregion

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.AddTags(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Meteor);
    }

    public class PanicParticle : Particle
    {
        int npc = 0;
        public override Asset<Texture2D> Texture => Assets.Textures.Meteor.NPCs.CosmoebaPanic.Asset;
        public PanicParticle(Vector2 pos, Vector2 vel, int whoAmI) : base(pos, vel, Vector2.One, null, null)
        {
            Center += new Vector2(0, -5);
            FrameCount = new Vector2(1, 4);
            FrameNum = new Vector2(0, 0);
            Rotation = vel.ToRotation() + MathHelper.ToRadians(-90f);
            AffectedByLight = false;
            npc = whoAmI;
            Effects = velocity.X > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }
        public override void Update()
        {
            Center += Main.npc[npc].velocity;
            base.Update();
            velocity *= 0.9f;
            velocity.Y += 0.2f;
            FrameNum.Y += 0.3f;
            if (FrameNum.Y >= 4) Kill();
        }
    }
    public class FizzParticle : Particle
    {
        int npc = 0;
        public override Asset<Texture2D> Texture => Assets.Textures.Meteor.NPCs.CosmoebaFizz.Asset;
        public FizzParticle(Vector2 pos, Vector2 vel, int whoAmI) : base(pos, vel, Vector2.One, null, null)
        {
            Center += new Vector2(Main.rand.NextFloat(-10, 10)).RotatedByRandom(MathHelper.TwoPi);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            FrameCount = new Vector2(1, 5);
            FrameNum = new Vector2(0, 0);
            AffectedByLight = false;
            npc = whoAmI;
            Pixelated = true;
        }
        public override void Update()
        {
            if (npc != -1)
                Center += Main.npc[npc].velocity;
            base.Update();
            velocity *= 0.95f;
            FrameNum.Y += 0.3f;
            if (FrameNum.Y >= 5) Kill();
        }
    }
}
