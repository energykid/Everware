namespace Everware.Utils;

public static class MathUtils
{
    public static Vector2 CubicBezier2(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        Vector2 a = Vector2.Lerp(p0, p1, t);
        Vector2 b = Vector2.Lerp(p1, p2, t);
        Vector2 c = Vector2.Lerp(p2, p3, t);

        Vector2 d = Vector2.Lerp(a, b, t);
        Vector2 e = Vector2.Lerp(b, c, t);

        return Vector2.Lerp(d, e, t);
    }
}
