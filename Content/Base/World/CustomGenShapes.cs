using Everware.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.WorldBuilding;

namespace Everware.Content.Base.World;

public static class CustomGenShapes
{
    public class Strip : GenShape
    {
        List<Vector2> _points;

        public Strip(List<Vector2> points)
        {
            _points = points;
        }

        public override bool Perform(Point origin, GenAction action)
        {
            if (!Apply(origin, action))
            {
                return false;
            }
            return true;
        }

        public bool Apply(Point origin, GenAction action)
        {
            Point TopLeft = origin;
            Point BottomRight = origin;

            var dummyPoints = _points;
            
            dummyPoints.Sort((A, B) => A.X < B.X ? -1 : 1);
            TopLeft.X = (int)dummyPoints.First().X;
            BottomRight.X = (int)dummyPoints.Last().X;
            dummyPoints.Sort((A, B) => A.Y < B.Y ? -1 : 1);
            TopLeft.Y = (int)dummyPoints.First().Y;
            BottomRight.Y = (int)dummyPoints.Last().Y;
            
            Rectangle rect = new Rectangle(TopLeft.X, TopLeft.Y, BottomRight.X - TopLeft.X, BottomRight.Y - TopLeft.Y);

            for (int tri = 0; tri < _points.Count - 4; tri++)
            {
                for (int i = rect.X; i <= rect.X + rect.Width; i++)
                {
                    for (int j = rect.Y; j <= rect.Y + rect.Height; j++)
                    {
                        if (MathUtils.PointInTriangle(new Point(i, j), (_points[tri]).ToPoint(), (_points[tri + 1]).ToPoint(), (_points[tri + 2]).ToPoint()))
                        {
                            if (i > 100 && i < Main.maxTilesX - 100)
                            {
                                if (j > 100 && j < Main.maxTilesY - 100)
                                {
                                    if (!UnitApply(action, Point.Zero, i, j))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
    public class Triangle : GenShape
    {
        Point vert1 = Point.Zero;
        Point vert2 = Point.Zero;
        Point vert3 = Point.Zero;
        Rectangle rect = Rectangle.Empty;
        public Triangle(Point V1, Point V2, Point V3)
        {
            vert1 = V1;
            vert2 = V2;
            vert3 = V3;
            float negativeX = Math.Min(vert1.X, Math.Min(vert2.X, vert3.X));
            float negativeY = Math.Min(vert1.Y, Math.Min(vert2.Y, vert3.Y));
            float positiveX = Math.Max(vert1.X, Math.Max(vert2.X, vert3.X));
            float positiveY = Math.Max(vert1.Y, Math.Max(vert2.Y, vert3.Y));
            rect = new Rectangle((int)negativeX, (int)negativeY, (int)positiveX, (int)positiveY);
        }

        public override bool Perform(Point origin, GenAction action)
        {
            if (!Apply(origin, action))
            {
                return false;
            }
            return true;
        }

        public bool Apply(Point origin, GenAction action)
        {
            for (int i = origin.X + rect.X; i <= origin.X + rect.Width; i++)
            {
                for (int j = origin.Y + rect.Y; j <= origin.Y + rect.Height; j++)
                {
                    if (MathUtils.PointInTriangle(new Point(i - origin.X, j - origin.Y), vert1, vert2, vert3))
                    {
                        if (!UnitApply(action, origin, i, j))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
    public class Hole : GenShape
    {
        public int _passes;
        private int _radius;

        public Hole(int radius, int passes)
        {
            _radius = radius;
            _passes = passes;
        }

        public void SetRadius(int radius)
        {
            _radius = radius;
        }

        public override bool Perform(Point origin, GenAction action)
        {
            for (int i = 0; i < _passes; i++)
            {
                if (!ApplyCircle(origin.X, origin.Y + (i * 2), origin, action))
                {
                    return false;
                }
            }
            return true;
        }

        public bool ApplyCircle(int xx, int yy, Point origin, GenAction action)
        {
            int num = (_radius + 1) * (_radius + 1);
            for (int i = yy - _radius; i <= yy + _radius; i++)
            {
                double num2 = (double)_radius / (double)_radius * (double)(i - yy);
                int num3 = Math.Min(_radius, (int)Math.Sqrt((double)num - num2 * num2));
                for (int j = xx - num3; j <= xx + num3; j++)
                {
                    if (!UnitApply(action, origin, j, i) && _quitOnFail)
                        return false;
                }
            }

            return true;
        }
    }
}
