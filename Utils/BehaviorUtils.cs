namespace Everware.Utils;

public static class BehaviorUtils
{
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
                if (possibleTarget.friendly || possibleTarget.townNPC || possibleTarget.dontTakeDamage || possibleTarget.CountsAsACritter)
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
