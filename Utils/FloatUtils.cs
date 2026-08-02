namespace Everware.Utils;

public static class FloatUtils
{
    public static bool ValueAt(this float me, float destination, float speed)
    {
        return Math.Abs(me - destination) < speed && me >= destination;
    }
}
