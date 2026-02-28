using Everware.Common.Players;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.ID;

namespace Everware.Core.Projectiles;

public abstract class EverProjectile : ModProjectile
{
    /// <summary>
    ///     Shorthand variable that allows for quickly grabbing of the player that owns this projectile.
    /// </summary>
    public Player Owner => Main.player[Projectile.owner];

    /// <summary>
    ///     Shorthand variable that allows to quickly grab the NetworkPlayer instance of this projectile's owner
    /// </summary>
    public NetworkPlayer NetworkOwner => Owner.GetModPlayer<NetworkPlayer>();

    /// <summary>
    ///     Trail length. This class automatically sets TrailCacheLength to this.
    /// </summary>
    public virtual int TrailLength => 10;

    /// <summary>
    ///     If not null, this class determines the maximum distance between each value in oldRot, in degrees.
    /// </summary>
    public float? TrailRotationSeparation => 10;

    /// <summary>
    ///     If not null, this class determines the maximum distance between each value in oldPos, in total pixels.
    /// </summary>
    public virtual int? TrailSeparation => null;

    public int HitsLeft = 1;
    public bool start = true;
    public int enemyHit = -1;

    #region Networking
    public virtual void NetOnSpawn()
    {

    }
    public virtual void NetOnHitEnemy(NPC npc)
    {

    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        base.SendExtraAI(writer);
        writer.Write(enemyHit);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        base.ReceiveExtraAI(reader);
        enemyHit = reader.ReadInt32();
    }
    public void SendNetHit(NPC target, NPC.HitInfo hit, int damageDone)
    {
        ModPacket p = Mod.GetPacket();
        p.Write("NetOnHitEnemy");
        p.Write(Projectile.whoAmI);
        p.Write(target.whoAmI);
        p.Send();
    }
    #endregion

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        var tex = ModContent.Request<Texture2D>(Texture);

        Projectile.width = tex.Width();
        Projectile.height = tex.Height();
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            SendNetHit(target, hit, damageDone);
        }
        else
        {
            enemyHit = target.whoAmI;
        }

        base.OnHitNPC(target, hit, damageDone);
    }

    public override bool PreAI()
    {
        if (start)
        {
            NetOnSpawn();
            Projectile.netUpdate = true;
            start = false;
        }
        if (enemyHit != -1)
        {
            NetOnHitEnemy(Main.npc[enemyHit]);
            Projectile.netUpdate = true;
            enemyHit = -1;
        }

        if (TrailSeparation != null)
        {
            for (var i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                var oldPos1 = Projectile.oldPos[i];
                var oldPos2 = Projectile.oldPos[i + 1];

                Projectile.oldPos[i + 1] =
                    oldPos1 + new Vector2(Math.Min((float)TrailSeparation, oldPos1.Distance(oldPos2)), 0).RotatedBy(oldPos1.AngleTo(oldPos2));
            }
        }

        if (TrailRotationSeparation != null)
        {
            for (var i = 0; i < Projectile.oldRot.Length - 1; i++)
            {
                var oldRot1 = Projectile.oldRot[i];
                var oldRot2 = Projectile.oldRot[i + 1];

                Projectile.oldRot[i + 1] = oldRot1.AngleTowards(oldRot2, MathHelper.ToRadians((float)TrailRotationSeparation));
            }
        }

        return base.PreAI();
    }

    public override bool PreKill(int timeLeft)
    {
        if (base.PreKill(timeLeft))
        {
            if (enemyHit != -1)
            {
                NetOnHitEnemy(Main.npc[enemyHit]);
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}