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
    public override string Texture => "Everware/Assets/Textures/Meteor/NPCs/CosmoebaBody";
    public override Vector2 Size => new Vector2(42, 42);
    public override int FrameNumber => 3;
    public override int TrailLength => 10;
    public override int Damage => 10;

    public override void SetDefaults()
    {
        base.SetDefaults();
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        State = (int)BehaviorState.Wandering;
        NPC.netUpdate = true;
    }

    Vector2 TargetPosition = Vector2.Zero;
    public enum BehaviorState
    {
        Wandering,
        CirclingMagicStone,
        Fleeing,
        FindingMeteor,
        Charging
    }

    public override void AI()
    {
        switch (State)
        {
            case (int)BehaviorState.Wandering:

                if (NPC.ai[0] % 5 == 0)
                {
                    var pos = BehaviorUtils.FindNearbyTilePosition(NPC.Center, 30, ModContent.TileType<MagicStoneTile>(), 2);

                    if (pos != null)
                    {
                        TargetPosition = pos.Value;
                        State = (int)BehaviorState.CirclingMagicStone;
                        NPC.ai[2] = 0;
                        break;
                    }
                }
                if (NPC.ai[2] >= 0)
                {
                    TargetPosition = NPC.Center + new Vector2(Main.rand.NextFloat(80, 300), 0).RotatedByRandom(MathHelper.TwoPi);
                    NPC.ai[2] = -Main.rand.NextFloat(120, 480);
                    NPC.netUpdate = true;
                }
                NPC.ai[2]++;
                TargetPosition += new Vector2((float)Math.Sin(NPC.ai[2] / 14f) * 6f, (float)Math.Sin(NPC.ai[2] / 12.5f) * 6f);

                if (NPC.ai[0] % 50 == 0)
                {
                    var pos = BehaviorUtils.FindNearbyTilePosition(NPC.Center, 300, ModContent.TileType<MagicStoneTile>(), 8);

                    if (pos != null)
                    {
                        TargetPosition = Vector2.Lerp(TargetPosition, pos.Value, 0.2f);
                        break;
                    }
                }

                NPC.rotation = Vector2.Zero.AngleTo(NPC.velocity) + MathHelper.Pi;

                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Lerp(NPC.Center, TargetPosition, 0.005f) - NPC.Center, 0.05f);

                break;
            case (int)BehaviorState.CirclingMagicStone:
                NPC.ai[2]++;

                NPC.ai[1] += 2f;

                NPC.rotation = NPC.Center.AngleTo(TargetPosition) + MathHelper.Pi;

                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Lerp(NPC.Center, TargetPosition + new Vector2(8, 0).RotatedBy(NPC.rotation) + new Vector2((float)Math.Sin(NPC.ai[2] / 14f) * 7f, (float)Math.Sin(NPC.ai[2] / 12.5f) * 7f), 0.015f) - NPC.Center, 0.1f);
                break;
            case (int)BehaviorState.Fleeing:
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

        var target = RenderTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, 200, 200);

        using (target.Scope(clearColor: Color.Transparent))
        {
            PrimitiveDrawing.DrawPrimitiveTrail(new Vector2(200, 200), p, 20, a => { return MathHelper.Lerp(1.2f, 0.7f, a); }, colors: (a, b) =>
            {
                return new Color(126, 187, 237);
            });
            PrimitiveDrawing.DrawPrimitiveTrail(new Vector2(200, 200), p, 20, a => { return MathHelper.Lerp(1, 0.5f, a); });
        }

        var StarShader = Assets.Effects.Meteor.NPCs.CosmoebaStars.CreateEffect();
        StarShader.Parameters.StarTexture = Assets.Textures.Meteor.NPCs.CosmoebaStars.Asset.Value;
        StarShader.Parameters.Progress = -NPC.ai[1] / 200f;
        StarShader.Apply();

        Main.spriteBatch.End(out var sb);
        Main.spriteBatch.Begin(sb with { SamplerState = SamplerState.PointWrap });

        Vector2 off = new Vector2(20, 0).RotatedBy(-NPC.rotation);

        Main.EntitySpriteDraw(target.Target, NPC.Center - Main.screenPosition, target.Target.Bounds, new Color(126, 187, 237), 0f, target.Target.Bounds.Size() / 2f, 2f, SpriteEffects.None);

        Main.EntitySpriteDraw(Body.Value, NPC.Center - Main.screenPosition, bodyFrame, Color.White, NPC.rotation, bodyFrame.Size() / 2f, 1f, SpriteEffects.None);

        Main.EntitySpriteDraw(Feelers.Value, NPC.Center - Main.screenPosition, feelerFrame, Color.White, NPC.rotation, new Vector2(feelerFrame.Width + 6, feelerFrame.Height / 2f), 1f, SpriteEffects.None);

        Main.spriteBatch.Restart(sb with { CustomEffect = StarShader.Shader });

        Main.EntitySpriteDraw(BodyInternal.Value, NPC.Center - Main.screenPosition, bodyFrame, Color.White, NPC.rotation, bodyFrame.Size() / 2f, 1f, SpriteEffects.None);

        Main.spriteBatch.Restart(sb);

        target.Dispose();

        return false;
    }
}