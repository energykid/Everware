using System;
using Terraria.WorldBuilding;

namespace Everware.Content.Base.World;

public static class CustomGenShapes
{
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
