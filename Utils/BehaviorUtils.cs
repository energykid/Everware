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

    public static bool SolidTileOrPlatform(Tile tile)
    {
        return WorldGen.SolidOrSlopedTile(tile) || Main.tileSolidTop[tile.type];
    }
}
