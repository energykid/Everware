namespace Everware.Utils;

public static class BehaviorUtils
{
    public static Vector2 Grounded(this Vector2 baseVec)
    {
        if (WorldGen.SolidOrSlopedTile(Main.tile[(baseVec / 16).ToPoint()]))
        {
            for (int i = 0; i < 250; i++)
            {
                baseVec.Y -= 2;
                if (!WorldGen.SolidOrSlopedTile(Main.tile[(baseVec / 16).ToPoint()])) break;
            }
            return baseVec;
        }
        for (int i = 0; i < 250; i++)
        {
            baseVec.Y += 2;
            if (WorldGen.SolidOrSlopedTile(Main.tile[(baseVec / 16).ToPoint()])) break;
        }
        return baseVec;
    }
}
