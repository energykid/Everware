using System.Collections.Generic;
using System.IO;
using System.Linq;
using Everware.Common;
using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.Items;
using Everware.Content.Misc.Particles;
using Everware.Content.Quarry.Visual;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Meteor.Items;

public class CruiserKeys : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Items/CruiserKeys";
    public override int DuplicationAmount => 1;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.mountType = ModContent.MountType<MeteorCruiser>();
        Item.UseSound = Assets.Sounds.Gear.Mount.MeteorCruiserMount.Asset;
    }
}

public class MeteorCruiserBuff : ModBuff
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Items/MeteorCruiserBuff";

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.buffTime[buffIndex] = 2;
        base.Update(player, ref buffIndex);
    }
}

public class MeteorCruiser : ModMount
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Items/MeteorCruiser";

    public override void SetMount(Player player, ref bool skipDust)
    {
        skipDust = true;
        base.SetMount(player, ref skipDust);
    }

    public override void Dismount(Player player, ref bool skipDust)
    {
        skipDust = true;
        base.Dismount(player, ref skipDust);
    }

    private float vl = 0f;

    public override void SetStaticDefaults()
    {
        MountID.Sets.FacePlayersVelocity[Type] = true;
        MountData.flightTimeMax = 0;
        MountData.textureWidth = 114;
        MountData.textureHeight = 74;
        MountData.backTexture = Assets.Textures.Meteor.Items.MeteorCruiser_Back.Asset;
        MountData.backTextureGlow = Assets.Textures.Meteor.Items.MeteorCruiser_Back_Glow.Asset;
        MountData.frontTexture = Assets.Textures.Meteor.Items.MeteorCruiser_Front.Asset;
        MountData.frontTextureGlow = Assets.Textures.Meteor.Items.MeteorCruiser_Front_Glow.Asset;
        MountData.buff = ModContent.BuffType<MeteorCruiserBuff>();
        MountData.totalFrames = 1;
        MountData.idleFrameCount = 1;

        MountData.spawnDust = -1;

        MountData.heightBoost = 0;

        MountData.fatigueMax = 1;

        MountData.fallDamage = 0f;

        MountData.usesHover = true;

        MountData.runSpeed = 0f;
        MountData.dashSpeed = 0f;
        MountData.acceleration = 0f;
        MountData.jumpHeight = 0;
        MountData.jumpSpeed = 0f;

        MountData.blockExtraJumps = true;

        MountData.playerHeadOffset = 14;
        MountData.playerYOffsets = [14];

        MountData.totalFrames = 1;

        MountData.standingFrameCount = 1;
        MountData.standingFrameDelay = 0;
        MountData.standingFrameStart = 0;

        MountData.runningFrameCount = 1;
        MountData.runningFrameDelay = 0;
        MountData.runningFrameStart = 0;

        MountData.flyingFrameCount = 1;
        MountData.flyingFrameDelay = 0;
        MountData.flyingFrameStart = 0;

        MountData.inAirFrameCount = 1;
        MountData.inAirFrameDelay = 0;
        MountData.inAirFrameStart = 0;

        MountData.idleFrameCount = 0;
        MountData.idleFrameDelay = 0;
        MountData.idleFrameStart = 0;
        MountData.idleFrameLoop = true;

        MountData.swimFrameCount = 0;
        MountData.swimFrameDelay = 0;
        MountData.swimFrameStart = 0;
        
        base.SetStaticDefaults();
    }
    
    public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture,
        ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation,
        ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
    {
        drawOrigin.Y += 19;
        spriteEffects = drawPlayer.GetModPlayer<MeteorCruiserPlayer>().Direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        
        var asset = Assets.Textures.Meteor.Items.MeteorCruiser_TrailMask.Asset;
        var asset2 = Assets.Textures.Meteor.Items.MeteorCruiser_TrailTexture.Asset;
        
        Main.spriteBatch.End(out var sb);
        
        vl += Math.Abs(drawPlayer.GetModPlayer<MeteorCruiserPlayer>().TargetLocationVelocity) / 200f;
        
        var eff = Assets.Effects.Meteor.Items.MeteorCruiserTrail.CreateEffect();
        eff.Parameters.FillTexture = asset2.Value;
        eff.Parameters.Resolution = new Vector2(asset.Width() / asset2.Width(), 1);
        eff.Parameters.Time = -vl;
        eff.Apply();
        
        var pool = RenderTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, asset.Width(), asset.Height());

        using (pool.Scope(clearColor: Color.Transparent))
        {
            Main.spriteBatch.Begin(sb with {CustomEffect = eff.Shader, SamplerState = Main.DefaultSamplerState});

            Main.spriteBatch.Draw(asset.Value, pool.Target.Bounds, Color.White);
            
            Main.spriteBatch.End();
        }
        
        Main.spriteBatch.Begin(sb);
        
        if (drawPlayer.GetModPlayer<MeteorCruiserPlayer>().OldPositions.Count > 2)
        {
            List<Vector2> v2s = PrimitiveBuilder.TrailFrom(drawPlayer.GetModPlayer<MeteorCruiserPlayer>().OldPositions, _ => 1f, out List<Vector2> texcoords, 20f);
            List<Color> colors = new List<Color>();
            for (int i = 0; i < v2s.Count; i++)
            {
                v2s[i] -= Main.screenPosition;
                float tr = drawPlayer.GetModPlayer<MeteorCruiserPlayer>().Transparency;
                colors.Add(new Color(tr, tr, tr, tr));
            }
        
            PrimitiveDrawing.DrawPrimitiveStrip2(v2s, colors, pool.Target, texcoords, true, eff.Shader);
        }
        
        Main.spriteBatch.Restart(sb);
        
        pool.Dispose();
        
        return base.Draw(playerDrawData, drawType, drawPlayer, ref texture, ref glowTexture, ref drawPosition, ref frame, ref drawColor, ref glowColor, ref rotation, ref spriteEffects, ref drawOrigin, ref drawScale, shadow);
    }
}

public class MeteorCruiserPacket : EverPacket
{
    public Player? plr = null;

    public override void Read(Mod mod, BinaryReader reader, int playerID)
    {
        int whoAmI = reader.ReadInt32();
        float vel = reader.ReadSingle();
        Vector2 location = reader.ReadVector2();

        plr = Main.player[whoAmI];
        
        MeteorCruiserPlayer mPlr = plr.GetModPlayer<MeteorCruiserPlayer>();

        mPlr.TargetLocationVelocity = vel;
        mPlr.TargetLocation = location;

        if (Main.netMode == NetmodeID.Server)
        {
            EverwarePacketHandler.SendPacket(this);
        }
    }

    public override void Write(ModPacket packet)
    {
        if (plr != null)
        {
            packet.Write(plr.whoAmI);
            packet.Write(plr.GetModPlayer<MeteorCruiserPlayer>().TargetLocationVelocity);
            packet.WriteVector2(plr.GetModPlayer<MeteorCruiserPlayer>().TargetLocation);
        }
    }
}
public class MeteorCruiserPlayer : ModPlayer
{
    public Vector2 TargetLocation = Vector2.Zero;
    public int Direction = 0;
    public float TargetLocationVelocity = 0f;
    public float TargetLocationVelocityMultiplier = 1f;
    public float BumpCooldown = 0f;
    public float HopOnAnimation = 0f;

    public List<Vector2> OldPositions = new();
    
    public float T = 0f;
    public float Transparency = 0f;
    
    public Vector2 PositionPrediction = Vector2.Zero;

    public override void PostUpdate()
    {
        base.PostUpdate();
    }

    private ActiveSound? Sound = null;

    public override void PreUpdateMovement()
    {
        if (Player.mount.Type == ModContent.MountType<MeteorCruiser>())
        {
            if (Sound == null || !Sound.IsPlaying)
            {
                Sound = new ActiveSound(Assets.Sounds.Gear.Mount.MeteorCruiserLoop.Asset with { IsLooped = true }, Player.Center, snd =>
                {
                    snd.Pitch = MathHelper.Lerp(snd.Pitch, -0.5f + Math.Abs(TargetLocationVelocity / 7f), 0.01f);
                    snd.Volume = MathHelper.Lerp(snd.Volume, 1f, 0.01f);
                    snd.Position = Player.Center;
                    return Main.instance.IsActive;
                });
            }
            
            Sound.Update();
            
            BumpCooldown--;

            if (BumpCooldown > -240)
            {
                float intensity = (BumpCooldown + 240) / 240 / 6;
                if (Main.rand.NextBool(5))
                    new WeldingSpark(Player.MountedCenter + new Vector2(Main.rand.NextFloat(-60, 60), Main.rand.NextFloat(-20, 20)), new Vector2(Main.rand.NextFloat(1f, 6f)).RotatedByRandom(MathHelper.TwoPi)).Spawn();
                new SmallSmoke(Player.MountedCenter + new Vector2(Main.rand.NextFloat(-60, 60), Main.rand.NextFloat(-20, 20)), new Vector2(Main.rand.NextFloat(1f, 6f)).RotatedByRandom(MathHelper.TwoPi), Color.Black.MultiplyRGBA(new(0f, 0f, 0f, intensity))) { UpdateFunction =
                    A => { A.velocity.Y -= 0.2f; }
                }.Spawn();
                new SmallMistFade(Player.MountedCenter + new Vector2(Direction * -30 + Main.rand.NextFloat(-7, 7), -10 + Main.rand.NextFloat(-3, 3)).RotatedBy(Player.fullRotation), new Vector2(0, -2), Color.Black.MultiplyRGBA(new(0f, 0f, 0f, intensity)), new(0.7f)) {Rotation = Main.rand.NextFloat(360f)}.Spawn();
            }
            
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                T++;
                if (T % 10 == 0)
                    new MeteorCruiserPacket { plr = Player }.Send();
            }

            if (TargetLocation == Vector2.Zero)
            {
                BumpCooldown = -240;
                HopOnAnimation = 0;
                TargetLocation = Player.Center;
                TargetLocationVelocity = 0f;
                OldPositions.Clear();
            }
            
            if (Transparency <= 0f)
                OldPositions.Clear();
            
            OldPositions.Insert(0, Player.Center + new Vector2(Direction * -30, -10));
            if (OldPositions.Count > 20)
            {
                OldPositions.RemoveAt(20);
            }

            HopOnAnimation++;

            if (BumpCooldown < 0)
            {
                if (Player.controlLeft)
                {
                    if (TargetLocationVelocity < 0)
                        Transparency = MathHelper.Lerp(Transparency, 1f, 0.075f);
                    else
                        Transparency = MathHelper.Lerp(Transparency, -0.1f, 0.1f);

                    OldPositions.ForEach(A => { A.X += 2; });
                    
                    TargetLocationVelocity -= 0.2f;

                    Direction = Math.Sign(TargetLocationVelocity);
                }
                else if (Player.controlRight)
                {
                    if (TargetLocationVelocity > 0)
                        Transparency = MathHelper.Lerp(Transparency, 1f, 0.075f);
                    else
                        Transparency = MathHelper.Lerp(Transparency, -0.1f, 0.1f);
                    
                    OldPositions.ForEach(A => { A.X -= 2; });
                    
                    TargetLocationVelocity += 0.2f;

                    Direction = Math.Sign(TargetLocationVelocity);
                }
                else
                {
                    Transparency = MathHelper.Lerp(Transparency, -0.1f, 0.1f);
                    TargetLocationVelocity = (float)(Math.Max((Math.Abs(TargetLocationVelocity) - 0.2), 0.1) * Math.Sign(TargetLocationVelocity));
                }
            }
            else
            {
                TargetLocationVelocity *= 0.98f;
            }

            if (Player.controlDown)
            {
                if (Math.Abs(TargetLocationVelocity) > 10)
                    TargetLocationVelocity *= 0.9f;
            }
            
            TargetLocationVelocity = Math.Clamp(TargetLocationVelocity, -15, 15);

            int checkWidth = Math.Abs((int)(TargetLocationVelocity / 15 * 7));
            checkWidth = Math.Max(checkWidth, 3);    
            
            if (Math.Abs(TargetLocationVelocity) > 0.2)
                TargetLocation.X += TargetLocationVelocity * TargetLocationVelocityMultiplier;

            List<Vector2> targets = new List<Vector2>();
            for (int i = -checkWidth; i <= checkWidth; i++)
            {
                Vector2 pos = (TargetLocation + new Vector2(20 * i, -350));
                if (pos.X > 100 && pos.Y > 100 && pos.X < (Main.maxTilesX * 16) - 100 &&
                    pos.Y < (Main.maxTilesY * 16) - 100)
                {
                    Vector2 gr = pos.GroundedWithWater();
                    targets.Add(gr);
                }
            }
            List<Vector2> targets2 = new List<Vector2>();
            for (int i = -checkWidth / 2; i <= checkWidth / 2; i++)
            {
                Vector2 pos = (TargetLocation + new Vector2(20 * i, -30));
                if (pos.X > 100 && pos.Y > 100 && pos.X < (Main.maxTilesX * 16) - 100 &&
                    pos.Y < (Main.maxTilesY * 16) - 100)
                {
                    Vector2 gr = pos.GroundedWithWater();
                    targets2.Add(gr);
                }
            }
            
            // i am effectively checking two "ground areas;" one very high up and one just above the player
            targets.Sort((a, b) => a.Y > b.Y ? 1 : -1);
            targets2.Sort((a, b) => a.Y > b.Y ? 1 : -1);
            
            Vector2 target1 = new Vector2(TargetLocation.X, targets.FirstOrDefault(TargetLocation).Y);
            Vector2 target2 = new Vector2(TargetLocation.X, targets2.FirstOrDefault(TargetLocation).Y);
            
            Vector2 locTo1 = target1 + new Vector2(0, -60f + (float)(Math.Sin(GlobalTimer.Value / 20f) * 5f));
            Vector2 locTo2 = target2 + new Vector2(0, -60f + (float)(Math.Sin(GlobalTimer.Value / 20f) * 5f));

            Vector2 oldLocation = TargetLocation;
            Vector2 newLocation = Vector2.Lerp(TargetLocation, locTo1, 0.05f);
            newLocation.Y = MathHelper.Lerp(newLocation.Y, locTo1.Y, Math.Abs(TargetLocationVelocity / 45));
            Vector2 newLocation2 = Vector2.Lerp(TargetLocation, locTo2, 0.05f);
            newLocation2.Y = MathHelper.Lerp(newLocation2.Y, locTo2.Y, Math.Abs(TargetLocationVelocity / 45));
            Player.gravity = 0;
            Player.shimmering = true;
            Player.shimmerTransparency = 0f;
            Player.sitting.isSitting = true;
            
            TargetLocation.X = newLocation.X;
            float Y = TargetLocation.MoveTowards(new Vector2(TargetLocation.X, newLocation.Y), 40).Y;
            if (Math.Abs(Y - TargetLocation.Y) < 40 || Y > TargetLocation.Y)
                TargetLocation.Y = Y;
            else
            {
                Y = TargetLocation.MoveTowards(new Vector2(TargetLocation.X, newLocation2.Y), 40).Y;
                if (Math.Abs(Y - TargetLocation.Y) < 40 || Y > TargetLocation.Y)
                    TargetLocation.Y = Y;
                else if (BumpCooldown < 0)
                {
                    Vector2 posToCheck = Player.Center + new Vector2(0, -40);
                    if (!Collision.CanHit(posToCheck, 1, 60,
                            posToCheck + new Vector2(TargetLocationVelocity, 0), 1,
                            60))
                    {
                        TargetLocationVelocity = -TargetLocationVelocity * 0.75f;
                        TargetLocation = Player.Center;
                        BumpCooldown = 120;
                        ScreenEffects.AddScreenShake(Player.Center, 12, 0.85f);
                        Sound.Volume = 0f;
                        Sound.Pitch = -1f;
                        Transparency = 0f;
                        SoundEngine.PlaySound(Assets.Sounds.Gear.Mount.MeteorCruiserCrash.Asset.WithPitchVariance(0.1f), Player.Center);
                    }
                }
            }
            Player.velocity = Vector2.Lerp(Player.Center, TargetLocation, 0.1f) - Player.Center;
            Player.direction = Direction;
            Player.fullRotation = MathHelper.Lerp(Player.fullRotation, MathHelper.Lerp(Player.fullRotation, MathHelper.ToRadians(TargetLocationVelocity * 0.1f) + (MathHelper.ToRadians(TargetLocation.Y - oldLocation.Y) * Math.Sign(TargetLocationVelocity) * 2), 0.05f), 0.5f);
        }
        else
        {
            if (Sound != null)
            {
                Player.velocity = Vector2.Zero;
                Sound.Stop();
                Sound = null;
            }
            TargetLocation = Vector2.Zero;
        }
    }

    public override void PreUpdate()
    {
        base.PreUpdate();
    }
}