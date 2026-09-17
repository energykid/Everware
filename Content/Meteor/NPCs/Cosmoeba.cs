using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.NPCs;
using Everware.Content.Meteor.Tiles;
using Everware.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Everware.Content.Meteor.NPCs;

public class Cosmoeba : EverNPC
{
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
                    var pos = BehaviorUtils.FindNearbyTilePosition(NPC.Center, 300, ModContent.TileType<MagicStoneTile>(), 8);

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
                NPC.ai[1] += 2.5f;
                ExtraAI[0] = MathHelper.Lerp(ExtraAI[0], 3f, 0.2f);
                NPC.velocity = Vector2.Lerp(NPC.velocity, (Target.DirectionTo(NPC.Center) * ExtraAI[0]) + new Vector2(0, -0.25f * ExtraAI[0]), 0.2f);
                NPC.rotation = Vector2.Zero.AngleTo(NPC.velocity) + MathHelper.Pi;
                NPC.TargetClosest(false);

                NPC.velocity = NPC.velocity.RotatedBy(MathHelper.ToRadians((float)Math.Sin(NPC.ai[1] / 20f) * 7f));

                /// TODO: Make amoebas find meteors and program in that behavior
                if ((NPC.life < NPC.lifeMax / 2f && NPC.Distance(Target.Center) > 150) || NPC.ai[2] > (15 * 12))
                    ChangeState(BehaviorState.FindingMeteor);

                NPC.ai[2]++;
                if (NPC.ai[2] > 25 && NPC.ai[2] % 15 < 1)
                    SoundEngine.PlaySound(Assets.Sounds.NPC.CosmoebaFleeLoop.Asset with { MaxInstances = 3, Pitch = Personality }, NPC.Center);

                if (NPC.Distance(Target.Center) > 200 && NPC.ai[2] > 50)
                    ChangeState(BehaviorState.Wandering);
                break;
            case (int)BehaviorState.FindingMeteor:
                break;
            case (int)BehaviorState.Charging:
                break;
        }

        NPC.ai[0]++;
        NPC.ai[1] += NPC.velocity.Length();
        float spd = 1f + ((float)Math.Sin(NPC.ai[0] / 30f) * 0.7f);
        base.AI();

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
    public void StartFleeing()
    {
        ExtraAI[0] = 10;
        if (State != (int)BehaviorState.Fleeing)
            SoundEngine.PlaySound(Assets.Sounds.NPC.CosmoebaFlee.Asset with { MaxInstances = 3, Pitch = Personality }, NPC.Center);
        ChangeState(BehaviorState.Fleeing);
    }
    public void ChangeState(BehaviorState state)
    {
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
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var Body = Assets.Textures.Meteor.NPCs.CosmoebaBody.Asset;
        var BodyInternal = Assets.Textures.Meteor.NPCs.CosmoebaBodyInternal.Asset;
        var Feelers = Assets.Textures.Meteor.NPCs.CosmoebaFeelers.Asset;
        var Stars = Assets.Textures.Meteor.NPCs.CosmoebaStars.Asset;

        Rectangle bodyFrame = Body.Frame(1, 3, 0, 0);

        Rectangle feelerFrame = Feelers.Frame(1, 4, 0, (int)(((NPC.ai[1] / 20) + (GlobalTimer.Value / 20)) % 4));

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

        using (target.Scope(clearColor: Color.Transparent))
        {
            PrimitiveDrawing.DrawPrimitiveTrail(new Vector2(200, 200), p, 20, a => { return MathHelper.Lerp(1.2f, 0.7f, a); }, colors: (a, b) =>
            {
                return new Color(126, 187, 237);
            });
            PrimitiveDrawing.DrawPrimitiveTrail(new Vector2(200, 200), p, 20, a => { return MathHelper.Lerp(1, 0.5f, a); });
        }

        Vector2 off = new Vector2(20, 0).RotatedBy(-NPC.rotation);

        Main.spriteBatch.Restart(sb with { SamplerState = Main.DefaultSamplerState });

        Main.EntitySpriteDraw(target.Target, NPC.Center - Main.screenPosition, target.Target.Bounds, new Color(126, 187, 237), 0f, target.Target.Bounds.Size() / 2f, 2f, SpriteEffects.None);

        Main.EntitySpriteDraw(Body.Value, NPC.Center - Main.screenPosition, bodyFrame, Color.White, NPC.rotation, bodyFrame.Size() / 2f, 1f, SpriteEffects.None);

        Main.EntitySpriteDraw(Feelers.Value, NPC.Center - Main.screenPosition, feelerFrame, Color.White, NPC.rotation, new Vector2(feelerFrame.Width + 6, feelerFrame.Height / 2f), 1f, SpriteEffects.None);

        Main.spriteBatch.Restart(sb with { CustomEffect = StarShader.Shader });

        Main.EntitySpriteDraw(BodyInternal.Value, NPC.Center - Main.screenPosition, bodyFrame, Color.White, NPC.rotation, bodyFrame.Size() / 2f, 1f, SpriteEffects.None);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(sb);

        target.Dispose();

        return false;
    }
    #endregion
}