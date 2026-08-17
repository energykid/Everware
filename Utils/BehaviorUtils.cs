using Everware.Common.Players;
using Everware.Content.Base.Items;
using Everware.Content.Base.ParticleSystem;
using Terraria.ID;

namespace Everware.Utils;

public static class BehaviorUtils
{
    public static void ThrowTileReplicants(Vector2 velocity, Point pt, int num)
    {
        for (int i = -num; i <= num; i++)
        {
            for (int j = -3; j <= 5; j++)
            {
                Vector2 position = new Vector2((pt.X + i) * 16, (pt.Y + j) * 16);
                Tile t = Main.tile[pt.X + i, pt.Y + j];

                bool t1 = WorldGen.SolidOrSlopedTile(t) && !t.IsActuated;
                if (t1)
                {
                    bool NotSalt = true;

                    if (ModLoader.TryGetMod("SpiritReforged", out Mod spiritReforged))
                    {
                        var a = spiritReforged.Find<ModTile>("SaltBlockReflective");
                        if (t.TileType == a.Type)
                        {
                            NotSalt = false;
                        }
                    }
                    if (NotSalt)
                    {
                        StillTileReplicantParticle tile0 = new StillTileReplicantParticle(t.TileType, new Rectangle(t.TileFrameX, t.TileFrameY, 16, 16), position + new Vector2(8, 8), Vector2.Zero, Vector2.One)
                        {
                            HideTimer = 60
                        };
                        tile0.Spawn();
                        WorldGen.KillTile_MakeTileDust(pt.X + i, pt.Y + j, t);
                        if (!WorldGen.SolidOrSlopedTile(Main.tile[pt.X + i, pt.Y + j - 2]))
                        {
                            Vector2 altVel = velocity;
                            altVel.Normalize();
                            TileReplicantParticle tile = new TileReplicantParticle(t.TileType, new Rectangle(t.TileFrameX, t.TileFrameY, 16, 16), position + new Vector2(8, 8), new Vector2(0, -4f + (Math.Abs((float)i / 3f))), Vector2.One, P =>
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
                                HideTimer = Math.Abs(i) * 5,
                                StartingY = position.Y + 8
                            };
                            tile.Spawn();
                        }
                    }
                }
            }
        }
    }

    public static EverWeaponItem GetEverWeaponItem(this Player player)
    {
        if (player.HeldItem.ModItem is EverWeaponItem it) return it;
        return null;
    }
    public static NetworkPlayer NetworkHandler(this Player player)
    {
        return player.GetModPlayer<NetworkPlayer>();
    }
    public static bool IsHostile(this NPC npc)
    {
        return (!npc.friendly && !npc.CountsAsACritter);
    }
    public static Vector2 Grounded(this Vector2 baseVec)
    {
        if (SolidTileOrPlatform(Main.tile[(baseVec / 16).ToPoint()]))
        {
            for (int i = 0; i < 250; i++)
            {
                baseVec.Y -= 2;
                if (!SolidTileOrPlatform(Main.tile[(baseVec / 16).ToPoint()])) break;
            }
            return baseVec;
        }
        for (int i = 0; i < 250; i++)
        {
            baseVec.Y += 2;
            if (SolidTileOrPlatform(Main.tile[(baseVec / 16).ToPoint()])) break;
        }
        return baseVec;
    }
    public static Point Grounded(this Point baseVec)
    {
        if (SolidTileOrPlatform(Main.tile[baseVec]))
        {
            for (int i = 0; i < 250; i++)
            {
                baseVec.Y -= 1;
                if (!SolidTileOrPlatform(Main.tile[baseVec])) break;
            }
            return baseVec;
        }
        for (int i = 0; i < 250; i++)
        {
            baseVec.Y += 1;
            if (SolidTileOrPlatform(Main.tile[baseVec])) break;
        }
        return baseVec;
    }

    public static bool SolidTileOrPlatform(Tile tile)
    {
        return WorldGen.SolidOrSlopedTile(tile) || Main.tileSolidTop[tile.type];
    }

    public static bool SolidTilePlatformOrLiquid(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        return WorldGen.SolidOrSlopedTile(tile) || Main.tileSolidTop[tile.type] || Main.tile[i, j].LiquidAmount > 0;
    }

    public static bool ClosestNPC(ref NPC target, float maxDistance, Vector2 position, bool ignoreTiles = false, int overrideTarget = -1, int forcedNPCType = -1, bool hostilesOnly = false)
    {
        bool foundTarget = false;
        if (overrideTarget != -1)
        {
            if ((Main.npc[overrideTarget].Center - position).Length() < maxDistance)
            {
                target = Main.npc[overrideTarget];
                return true;
            }
        }
        for (int k = 0; k < 200; k++)
        {
            NPC possibleTarget = Main.npc[k];
            float distance = (possibleTarget.Center - position).Length();
            bool found = distance < maxDistance && possibleTarget.active && (Collision.CanHit(position, 0, 0, possibleTarget.Center, 0, 0) || ignoreTiles);
            if (hostilesOnly)
            {
                if (possibleTarget.friendly || possibleTarget.townNPC || possibleTarget.dontTakeDamage || possibleTarget.CountsAsACritter || possibleTarget.type == NPCID.TargetDummy)
                    found = false;
            }
            if (found)
            {
                if (forcedNPCType == -1 || forcedNPCType == Main.npc[k].type)
                {
                    target = Main.npc[k];
                    foundTarget = true;

                    maxDistance = (target.Center - position).Length();
                }
            }
        }
        return foundTarget;
    }
}
