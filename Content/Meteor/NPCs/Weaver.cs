using Daybreak.Common.Mathematics;
using Everware.Common.Systems;
using Everware.Content.Base.NPCs;
using Everware.Utils;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;
using static Everware.Core.AssetReferences.Assets.Textures.Meteor.NPCs;

namespace Everware.Content.Meteor.NPCs;

internal class Weaver : EverNPC
{
    private const int PREFERRED_DISTANCE_GRAB = 800;
    private const int PREFERRED_METEOR_DIST = 180;
    private const int PREFERRED_HANG_LENGTH = 12 * 16;
    private const float HANG_ANIM_LENGTH = 25f;
    private const float THROW_DELAY = 12f;

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

    private ref float MiscAITimer => ref NPC.ai[2];
    private ref float Timer => ref ExtraAI[0];
    private ref float MeteorWhoAmI => ref ExtraAI[1];

    private Vector3 Center3D
    {
        get => new Vector3(NPC.Center, NPC.ai[3]);
        set
        {
            NPC.Center = new Vector2(value.X, value.Y);
            NPC.ai[3] = value.Z;
        }
    }


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
    private float MaxDistance = 0f;
    private Vector3 OldMeteorPosition = Vector3.Zero;
    private Vector3 MeteorPosition = Vector3.Zero;
    private NPC? GrappledMeteor = null;

    public override void SetDefaults()
    {
        base.SetDefaults();
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 1f;
        NPC.damage = 50;
        State = 0;
        LeftHanded = Main.rand.NextBool();
        NPC.behindTiles = true;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        base.SendExtraAI(writer);
        writer.WriteVector3(OldMeteorPosition);
        writer.WriteVector3(MeteorPosition);
        writer.Write(LeftHanded);
        writer.Write(MaxDistance);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        base.ReceiveExtraAI(reader);
        OldMeteorPosition = reader.ReadVector3();
        MeteorPosition = reader.ReadVector3();
        LeftHanded = reader.ReadBoolean();
        MaxDistance = reader.ReadSingle();
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
            Timer = 0;
            AIPosition = NPC.Center - PREFERRED_HANG_LENGTH * Vector2.UnitY;
            HangingPosition = NPC.Center;
            NPC.netUpdate = true;
            return BehaviorState.HangAndFollowTarget; 
        }

        return BehaviorState.WaitInGroundToAmbush;
    }

    private BehaviorState HangAndFollowTarget()
    {
        int extraAnimLength = 30;

        if (Timer <= HANG_ANIM_LENGTH + extraAnimLength + 7)
        {
            var interpolant = MathHelper.Clamp((Timer - HANG_ANIM_LENGTH) / extraAnimLength, 0f, 1f);

            NPC.Center = Vector2.Lerp(HangingPosition, AIPosition, Easing.InOutSine(interpolant));

            MiscAITimer = 150f;

            if (Timer == HANG_ANIM_LENGTH + extraAnimLength + 7)
            {
                AIPosition = NPC.Center;
                HangingPosition = NPC.Center - PREFERRED_HANG_LENGTH * Vector2.UnitY;
                NPC.behindTiles = false;
            }
            return BehaviorState.HangAndFollowTarget;
        }

        if (NPC.HasValidTarget)
        {
            var targetPos = Main.player[NPC.target].Center - 25 * 16 * Vector2.UnitY;
            targetPos += (LeftHanded ? 1f : -1f) * Vector2.UnitX * 16 * 50;

            HangingPosition = Vector2.Lerp(HangingPosition, targetPos, 0.02f);
        }
        else
            return BehaviorState.FindGround;

        HangOnString();
        MiscAITimer--;

        if (MiscAITimer > 0)
            return BehaviorState.HangAndFollowTarget;

        NPC? MeteorHead = PathfindingUtils.GetClosestNPC(Main.player[NPC.target].Center, PREFERRED_DISTANCE_GRAB, NPCID.MeteorHead, NPC2 => { return NPC2.ai[1] == 0; });

        if (MeteorHead is null)
            return BehaviorState.HangAndFollowTarget;

        // this should be a method i think
        // if found meteor then prepare variables and grab the meteor head
        GrappledMeteor = MeteorHead;
        MeteorHead.ai[1] = NPC.whoAmI;
        MeteorHead.netUpdate = true;
        MeteorWhoAmI = MeteorHead.whoAmI;
        MiscAITimer = NPC.Center.Distance(MeteorHead.Center);
        NPC.netUpdate = true;
        Timer = 0;

        return BehaviorState.GrappleMeteor;
    }

    private BehaviorState GrappleMeteor()
    {
        GrappledMeteor = Main.npc[(int)MeteorWhoAmI];

        if (GrappledMeteor is null || !GrappledMeteor.active || GrappledMeteor.type != NPCID.MeteorHead)
            return BehaviorState.HangAndFollowTarget;

        if (Timer <= THROW_DELAY)
        {

            HangOnString();
            OldMeteorPosition = new Vector3(GrappledMeteor.Center - GrappledMeteor.velocity, 0f);
            MeteorPosition = new Vector3(GrappledMeteor.Center, 0f);
        }

        var throwDelay2 = THROW_DELAY + 10;

        if (Timer <= throwDelay2)
        {
            MaxDistance = (GrappledMeteor.Center - NPC.Center).Length();
            return BehaviorState.GrappleMeteor;
        }

        // Update general meteor variables
        GrappledMeteor.velocity = Vector2.Zero;
        GrappledMeteor.ai[3] = 1; // lobotomize meteor head
        GrappledMeteor.damage = 0;
        
        var playerPosition = Main.player[NPC.target].Center;
        var toTarget = (playerPosition - GrappledMeteor.Center).SafeNormalize(Vector2.Zero);

        // make the meteor get closer until it reaches its desired distance
        var distInterp = Easing.InBack(MathHelper.Clamp((Timer - throwDelay2) / 40, 0, 1));
        MiscAITimer = MathHelper.Lerp(MaxDistance, PREFERRED_METEOR_DIST, distInterp);

        // move further away if too close
        float dist = Main.player[NPC.target].Center.DistanceSQ(NPC.Center);
        if (dist <= PREFERRED_METEOR_DIST * PREFERRED_METEOR_DIST * 4f)
            HangingPosition -= toTarget * 2;
        if (dist > PREFERRED_METEOR_DIST * PREFERRED_METEOR_DIST * 8f)
            HangingPosition += toTarget * 2;

        HangOnString();

        // get normal vector to cross product with
        var duration = 2 * 40f;
        var multiplier = MathHelper.Clamp((Timer - throwDelay2) / duration, 0, 1);

        var x = Vector2.Dot((GrappledMeteor.Center - NPC.Center).SafeNormalize(Vector2.Zero), Vector2.UnitY);
        var y = Vector2.Dot((GrappledMeteor.Center - NPC.Center).SafeNormalize(Vector2.Zero), Vector2.UnitX);

        var normal = MathF.Abs(x) > MathF.Abs(y) ? Vector3.UnitX : Vector3.UnitY;
        normal = normal.Nlerp(-Vector3.UnitZ, Easing.InExpo(multiplier));

        // meteor head verlet integration in 3d space
        var oldPosition = MeteorPosition;
        var velocity = MeteorPosition - OldMeteorPosition - new Vector3(GrappledMeteor.velocity, 0f);

        var axis2 = new Vector2((GrappledMeteor.Center - NPC.Center).X, (GrappledMeteor.Center - NPC.Center).Y);
        velocity += new Vector3(axis2.X, axis2.Y, 0f).SafeNormalize(Vector3.Zero).Cross(normal) * 2.75f;

        MeteorPosition += velocity;
        OldMeteorPosition = oldPosition;

        // constrain npc center and meteor position in 3d space
        var vectorFrom = Center3D - MeteorPosition;
        float distance = Vector3.Distance(Center3D, MeteorPosition);
        float signedDistance = 0;

        if (distance > 0)
            signedDistance = (MiscAITimer / distance) - 1f;

        Vector3 translation = vectorFrom * (signedDistance * 0.5f);

        Center3D += translation * 0.05f;
        MeteorPosition -= translation * 0.95f;

        // Give the illusion of the meteor moving in 3D space
        var velocity2D = new Vector2(velocity.X, velocity.Y);

        GrappledMeteor.Center = new Vector2(MeteorPosition.X, MeteorPosition.Y);
        GrappledMeteor.scale = 1f / MathHelper.Clamp((1 - MeteorPosition.Z / (2f * MiscAITimer)), 0.01f, 50);
        GrappledMeteor.rotation = Vector2.Zero.AngleFrom(velocity2D);
        GrappledMeteor.velocity = velocity2D * 0.01f;

        // Update meteor draw order to ensure it draws behind the spider if behind the spider
        GrappledMeteor.behindTiles = MeteorPosition.Z < 0f;
        NPC.behindTiles = MeteorPosition.Z >= 0f;

        // Throw the meteor if possible
        var velocityNormal = velocity.SafeNormalize(Vector3.Zero);
        var toTargetNormal = new Vector3(toTarget, 0f);
        var toPlayerNormal = (new Vector3(playerPosition, 0f) - MeteorPosition).SafeNormalize(Vector3.Zero);

        if (multiplier == 1f &&
            velocity2D.LengthSquared() >= 26 * 26 &&
            Vector3.Dot(toTargetNormal, toPlayerNormal) >= 0.975f &&
            Vector3.Dot(toTargetNormal, velocityNormal) >= 0.975f &&
            Main.player[NPC.target].Center.DistanceSQ(NPC.Center) >= MiscAITimer * MiscAITimer * 4f)
        {
            // ensure that it doesnt miss thje player if they stand still
            var meteorVelocity = GrappledMeteor.Center.DirectionTo(playerPosition) * velocity.Length();

            GrappledMeteor.velocity = meteorVelocity;
            GrappledMeteor.damage = NPC.damage; // make it injherit the weaver's damage number
            GrappledMeteor.scale = 1f;
            MiscAITimer = 150;
            NPC.netUpdate = true;
            NPC.ai[3] = 0f; // set z coordinate to 0
            return BehaviorState.HangAndFollowTarget;
        }

        return BehaviorState.GrappleMeteor;
    }

    private void HangOnString()
    {
        var tilePosForWind = NPC.Center.ToTileCoordinates();

        var windStrength = Main.instance.TilesRenderer.GetWindCycle(tilePosForWind.X, tilePosForWind.Y, Main.instance.TilesRenderer._grassWindCounter);

        // verlet integration for spider on web movement
        var oldPosition = NPC.Center;
        var velocity = NPC.Center - AIPosition;
        velocity += Vector2.UnitY * 1.5f;
        velocity += Vector2.UnitX * windStrength * 0.2f;
        velocity *= 0.975f;

        NPC.Center += velocity + NPC.velocity;
        AIPosition = oldPosition;

        // constrain to web position
        var vectorFrom = (NPC.Center - HangingPosition).SafeNormalize(Vector2.Zero);

        NPC.Center = HangingPosition + vectorFrom * PREFERRED_HANG_LENGTH;
        NPC.rotation = vectorFrom.ToRotation() - MathHelper.PiOver2;
        NPC.velocity = Vector2.Zero;
    }
    #endregion

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (State != 2 && State != 3)
            return true;

        var interpolant = State == 2 ? Easing.InOutSine(MathHelper.Clamp(Timer / HANG_ANIM_LENGTH, 0, 1)) : 1f;
        var stringBegin = Vector2.UnitY * -42;
        stringBegin = NPC.Center + stringBegin.RotatedBy(NPC.rotation);

        var begin = stringBegin - Main.screenPosition;
        var end = Vector2.Lerp(stringBegin, HangingPosition, interpolant) - Main.screenPosition;
        var middle = (begin + end) * 0.5f - NPC.velocity;

        int segments = 40;

        var points = new List<Vector2>();

        for (int i = 0; i < segments; i++)
            points.Add(Bezier.VectorQuadratic(begin, middle, end, Vector2.One * i / (float)segments));

        if (interpolant < 1f)
        {
            for (int i = 0; i < segments - 1; i++)
            {
                var vector = points[i + 1] - points[i];
                vector = vector.SafeNormalize(Vector2.Zero);

                points[i] += MathF.Sin(i / MathHelper.TwoPi) * new Vector2(vector.Y, -vector.X) * (1f - interpolant) * 72f * (1f - (i / (float)segments));
            }
        }

        var stringColor = new Color(177, 134, 101);

        PrimitiveDrawing.DrawPrimitiveTrail(new Vector2(Main.screenWidth, Main.screenHeight), points, 2, a => { return MathHelper.Lerp(1.2f, 0.7f, a); }, colors: (a, b) =>
        {
            float length = 16 * 8f;
            return Color.Lerp(stringColor, Color.Transparent, Easing.OutCirc(1f - a));
        });

        if (State != 3 || GrappledMeteor is null || !GrappledMeteor.active || GrappledMeteor.type != NPCID.MeteorHead)
            return true;

        points = new List<Vector2>();

        var velocity = MeteorPosition - OldMeteorPosition;
        interpolant = Easing.InOutSine(MathHelper.Clamp((Timer) / (THROW_DELAY), 0, 1));

        begin = NPC.Center - screenPos;
        end = Vector2.Lerp(NPC.Center, GrappledMeteor.Center, interpolant) - screenPos;
        middle = (end - begin).SafeNormalize(Vector2.Zero);
        middle = Vector2.Lerp(begin, end, 0.67f) + new Vector2(velocity.X, velocity.Y) * 2f;

        for (int i = 0; i < segments; i++)
            points.Add(Bezier.VectorQuadratic(begin, middle, end, Vector2.One * i / (float)segments));

        if (interpolant < 1)
        {
            for (int i = 0; i < segments - 1; i++)
            {
                var vector = points[i + 1] - points[i];
                vector = vector.SafeNormalize(Vector2.Zero);

                points[i] += MathF.Sin(i * 0.4f) * new Vector2(vector.Y, -vector.X) * (1f - interpolant) * 72f * (1f - (i / (float)segments));
            }
        }

        PrimitiveDrawing.DrawPrimitiveTrail(new Vector2(Main.screenWidth, Main.screenHeight), points, 2, a => { return MathHelper.Lerp(1.2f, 0.7f, a); }, colors: (a, b) =>
        {
            return stringColor;
        });

        return true;
    }
}
