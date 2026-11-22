using System.Collections.Generic;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Common.Systems;

public class PrimitiveBuilder
{
    public static List<Vector2> TrailFrom(List<Vector2> points, Easing.AnimationCurve widthCurve, out List<Vector2> texcoords, float baseWidth = 10)
    {
        List<Vector2> pts2 = new();
        List<Vector2> texcs = new();

        float angle = 0;
        for (int i = 0; i < points.Count; i++)
        {
            float progress = (float)i / (float)points.Count;
            float width = widthCurve(progress) * baseWidth;
            if (i < points.Count - 1) angle = points[i].AngleTo(points[i + 1]);
            pts2.Add(points[i] + new Vector2(width, 0).RotatedBy(angle + MathHelper.PiOver2));
            texcs.Add(new(progress, 0));
            pts2.Add(points[i] + new Vector2(width, 0).RotatedBy(angle - MathHelper.PiOver2));
            texcs.Add(new(progress, 1));
        }
        
        texcoords = texcs;
        
        return pts2;
    }
}