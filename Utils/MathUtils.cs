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
    static float sign(Point p1, Point p2, Point p3)
    {
        return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
    }

    static float sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
    }

    public static bool PointInTriangle(Point pt, Point v1, Point v2, Point v3)
    {
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = sign(pt, v1, v2);
        d2 = sign(pt, v2, v3);
        d3 = sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    public static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = sign(pt, v1, v2);
        d2 = sign(pt, v2, v3);
        d3 = sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    public static bool HasNaNs(this Vector3 vec) => float.IsNaN(vec.X) || float.IsNaN(vec.Y) || float.IsNaN(vec.Z);

    public static Vector3 SafeNormalize(this Vector3 v, Vector3 defaultValue)
    {
        if (v == Vector3.Zero || v.HasNaNs())
            return defaultValue;

        return Vector3.Normalize(v);
    }

    public static Vector3 Cross(this Vector3 a, Vector3 b) => new Vector3(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X);

    public static Vector3 Nlerp(this Vector3 start, Vector3 end, float percent) =>
        Vector3.Lerp(start, end, percent).SafeNormalize(Vector3.Zero);
}
