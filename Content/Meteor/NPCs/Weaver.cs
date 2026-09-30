using Daybreak.Common.Mathematics;
using Everware.Common.Systems;
using Everware.Content.Base.NPCs;
using Everware.Utils;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;

namespace Everware.Content.Meteor.NPCs;

internal class Weaver : EverNPC
{
    private const int PREFERRED_HANG_LENGTH = 12 * 16;

    private enum BehaviorState : byte
    {
        FindGround,
        WaitInGroundToAmbush,
        HangAndFollowTarget,
        GrappleMeteor
    }

    public override Vector2 Size => new Vector2(70, 120);
    public override int Damage => 50;
    public override string Texture => "Everware/Assets/Textures/Meteor/NPCs/Weaver";

    private ref float Timer => ref ExtraAI[1];

    private Vector2 HangingPosition 
    { 
        get => new Vector2(NPC.ai[0], NPC.ai[1]);
        set
        {
            NPC.ai[0] = value.X;
            NPC.ai[1] = value.Y;
        }
    }

    private bool LeftHanded = false;
    private Vector2 OldMeteorPosition = Vector2.Zero;
    private NPC? GrappledMeteor = null;

    public override void SetDefaults()
    {
        base.SetDefaults();
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 1f;
        NPC.damage = 50;
        State = 0;
        ExtraAI[0] = -Main.rand.NextFloat(100f);
        LeftHanded = Main.rand.NextBool();
        NPC.behindTiles = true;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        base.SendExtraAI(writer);
        writer.WriteVector2(OldMeteorPosition);
        writer.Write(LeftHanded);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        base.ReceiveExtraAI(reader);
        OldMeteorPosition = reader.ReadVector2();
        LeftHanded = reader.ReadBoolean();
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return spawnInfo.Player.InModBiome<MeteorBiome>() ? 0.05f : 0f;
    }

    public override void OnSpawn(IEntitySource source)
    {
        HangingPosition = NPC.Center - 18 * 16 * Vector2.UnitY;
    }

    #region Behaviour
    public override void AI()
    {
        var newState = (BehaviorState)State switch
        {
            BehaviorState.FindGround => FindGround(),
            BehaviorState.WaitInGroundToAmbush => WaitInGroundToAmbush(),
            BehaviorState.HangAndFollowTarget => HangAndFollowTarget(),
            BehaviorState.GrappleMeteor => GrappleMeteor(),
            _ => BehaviorState.FindGround
        };

        State = (int)newState;
        Timer++; // timer
    }

    private BehaviorState FindGround()
    {
        var tilePosition = NPC.Center.ToTileCoordinates();

        for (int i = tilePosition.Y; i < Main.worldSurface; i++)
        {
            var tile = Main.tile[tilePosition.X, i];

            if (!tile.HasTile)
                continue;

            if (TileID.Sets.Platforms[tile.TileType])
                continue;

            if (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
            {
                AIPosition = new Point(tilePosition.X, i).ToWorldCoordinates();
                NPC.netUpdate = true;
                return BehaviorState.WaitInGroundToAmbush;
            }
        }

        // it shoudl NOT fail to find a spot to appear on
        NPC.active = false;
        return BehaviorState.FindGround;
    }

    private BehaviorState WaitInGroundToAmbush()
    {
        NPC.velocity = Vector2.Zero;
        NPC.Center = Vector2.Lerp(NPC.Center, AIPosition, 0.5f);
        NPC.TargetClosest();

        float aggroRange = 5 * 16;

        if (NPC.HasValidTarget && Main.player[NPC.target].Center.DistanceSQ(NPC.Center) <= aggroRange * aggroRange)
        {
            HangingPosition = NPC.Center - PREFERRED_HANG_LENGTH * Vector2.UnitY;
            NPC.ai[2] = 150f;
            NPC.netUpdate = true;
            NPC.behindTiles = false;

            return BehaviorState.HangAndFollowTarget; 
        }

        return BehaviorState.WaitInGroundToAmbush;
    }

    private BehaviorState HangAndFollowTarget()
    {
        if (NPC.HasValidTarget)
        {
            var targetPos = Main.player[NPC.target].Center - 25 * 16 * Vector2.UnitY;

            HangingPosition = Vector2.Lerp(HangingPosition, targetPos, 0.03f);
        }
        else
            return BehaviorState.FindGround;

        HangOnString(1);
        NPC.ai[2]--;

        if (NPC.ai[2] > 0)
            return BehaviorState.HangAndFollowTarget;

        NPC? MeteorHead = PathfindingUtils.GetClosestNPC(NPC.position, 800, NPCID.MeteorHead, NPC2 => { return NPC2.ai[1] == 0; });

        if (MeteorHead is null)
            return BehaviorState.HangAndFollowTarget;

        // this should be a method i think
        // if found meteor then prepare variables and grab the meteor head
        GrappledMeteor = MeteorHead;
        MeteorHead.ai[1] = NPC.whoAmI;
        MeteorHead.ai[3] = 1;
        MeteorHead.netUpdate = true;
        ExtraAI[0] = MeteorHead.whoAmI;
        NPC.ai[2] = NPC.Center.Distance(MeteorHead.Center);
        NPC.netUpdate = true;
        OldMeteorPosition = GrappledMeteor.Center - GrappledMeteor.velocity;
        return BehaviorState.GrappleMeteor;
    }

    private BehaviorState GrappleMeteor()
    {
        if (GrappledMeteor is null || !GrappledMeteor.active || GrappledMeteor.type != NPCID.MeteorHead)
            return BehaviorState.HangAndFollowTarget;

        var playerPosition = Main.player[NPC.target].Center;

        GrappledMeteor.velocity = Vector2.Zero;

        var acceleration = (GrappledMeteor.Center - NPC.Center).SafeNormalize(Vector2.Zero);

        // meteor head verlet integration
        var oldPosition = GrappledMeteor.Center;
        var velocity = GrappledMeteor.Center - OldMeteorPosition;
        velocity *= 0.95f;

        if (LeftHanded)
            acceleration = new Vector2(acceleration.Y, -acceleration.X) * 5f;
        else
            acceleration = new Vector2(-acceleration.Y, acceleration.X) * 5f;

        GrappledMeteor.Center += velocity + acceleration;
        OldMeteorPosition = oldPosition;

        // move away from player
        var toTarget = (playerPosition - GrappledMeteor.Center).SafeNormalize(Vector2.Zero);

        HangingPosition -= toTarget * 3f;

        HangOnString(2);

        // MAKE IT SPIN
        var hangPosition = NPC.Center;
        var preferredLength = 180;

        NPC.ai[2] = MathHelper.Lerp(NPC.ai[2], preferredLength, 0.02f);

        var midPoint = Vector2.Lerp(GrappledMeteor.Center, NPC.Center, 0.5f);
        var snapPos = (midPoint - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.ai[2];

        var vectorFrom = GrappledMeteor.Center - NPC.Center;
        float distance = Vector2.Distance(GrappledMeteor.Center, NPC.Center);
        float signedDistance = 0;

        if (distance > 0)
            signedDistance = (NPC.ai[2] / distance) - 1f;

        Vector2 translation = vectorFrom * (signedDistance * 0.5f);

        GrappledMeteor.Center += translation * 0.95f;
        NPC.Center -= translation * 0.05f;
        GrappledMeteor.rotation = vectorFrom.ToRotation() - MathHelper.PiOver2;

        // throw the meteor if possible
        if (velocity.LengthSquared() >= 35 * 35 &&
            Vector2.Dot(toTarget, velocity.SafeNormalize(Vector2.Zero)) > 0.975f &&
            Main.player[NPC.target].Center.DistanceSQ(NPC.Center) >= NPC.ai[2] * NPC.ai[2] * 4f)
        {
            // ensure that it doesnt miss thje player if they stand still
            var meteorVelocity = GrappledMeteor.Center.DirectionTo(playerPosition) * velocity.Length();

            GrappledMeteor.velocity = meteorVelocity;
            GrappledMeteor.damage += NPC.damage;
            NPC.ai[2] = 180;
            NPC.netUpdate = true;
            return BehaviorState.HangAndFollowTarget;
        }

        return BehaviorState.GrappleMeteor;
    }

    private void HangOnString(int steps = 1)
    {
        var tilePosForWind = NPC.Center.ToTileCoordinates();

        var windStrength = Main.instance.TilesRenderer.GetWindCycle(tilePosForWind.X, tilePosForWind.Y, Main.instance.TilesRenderer._grassWindCounter);

        // verlet integration for spider on web movement
        var oldPosition = NPC.Center;
        var velocity = NPC.Center - AIPosition;
        velocity += Vector2.UnitY * 1.5f;
        velocity += Vector2.UnitX * windStrength * 0.2f;
        velocity *= 0.99f;

        NPC.Center += velocity + NPC.velocity;
        AIPosition = oldPosition;

        // constrain to web position 
        for (int i = 0; i < steps; i++)
        {
            var vectorFrom = (NPC.Center - HangingPosition).SafeNormalize(Vector2.Zero);

            NPC.Center = HangingPosition + vectorFrom * PREFERRED_HANG_LENGTH;
            NPC.rotation = vectorFrom.ToRotation() - MathHelper.PiOver2;
        }

        NPC.velocity = Vector2.Zero;
    }
    #endregion

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Microsoft.Xna.Framework.Color drawColor)
    {
        if (State != 2 && State != 3)
            return true;

        var stringBegin = Vector2.UnitY * -42;
        stringBegin = NPC.Center + stringBegin.RotatedBy(NPC.rotation);

        var begin = stringBegin - Main.screenPosition;
        var middle = HangingPosition - Main.screenPosition;
        var end = HangingPosition with { Y = 0 } - Main.screenPosition;

        int segments = 30;

        var points = new List<Vector2>();

        for (int i = 0; i < segments; i++)
            points.Add(Bezier.VectorQuadratic(begin, middle, end, Vector2.One * i / (float)segments));

        var stringColor = new Color(177, 134, 101);

        PrimitiveDrawing.DrawPrimitiveTrail(new Vector2(Main.screenWidth, Main.screenHeight), points, 2, a => { return MathHelper.Lerp(1.2f, 0.7f, a); }, colors: (a, b) =>
        {
            return stringColor;
        });

        if (State != 3 || GrappledMeteor is null || !GrappledMeteor.active || GrappledMeteor.type != NPCID.MeteorHead)
            return true;

        points = new List<Vector2>();

        var velocity = GrappledMeteor.Center - OldMeteorPosition;

        begin = NPC.Center - screenPos;
        end = GrappledMeteor.Center - screenPos;
        middle = (end - begin).SafeNormalize(Vector2.Zero);
        middle = Vector2.Lerp(begin, end, 0.67f) + velocity * 2f;

        for (int i = 0; i < segments; i++)
            points.Add(Bezier.VectorQuadratic(begin, middle, end, Vector2.One * i / (float)segments));

        PrimitiveDrawing.DrawPrimitiveTrail(new Vector2(Main.screenWidth, Main.screenHeight), points, 2, a => { return MathHelper.Lerp(1.2f, 0.7f, a); }, colors: (a, b) =>
        {
            return stringColor;
        });

        return true;
    }
}
