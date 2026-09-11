using System.Collections.Generic;

namespace Everware.Utils;

public static class PathfindingUtils
{
    public struct PathNodeSet(Point start, Rectangle bounds, int divider = 1)
    {
        public int Divider = divider;
        public Point Start = start;
        public Rectangle Bounds = bounds;
        public List<Point> OpenNodes = [];
        public List<Point> ClosedNodes = [];

        public bool FindTile(int tileType, out Point position, int maxAttempts = 200)
        {
            OpenNodes.Add(Start);

            for (int gen = 0; gen < maxAttempts; gen++)
            {
                List<Point> nodesToCheck = OpenNodes;

                for (int k = 0; k < nodesToCheck.Count; k++)
                {
                    Point node = nodesToCheck[k];
                    if (node.X > 200 && node.X < Main.maxTilesX - 200 && node.Y > 200 && node.Y < Main.maxTilesY - 200)
                    {
                        if (Main.tile[node].TileType == tileType)
                        {
                            position = node;
                            return true;
                        }
                        else
                        {
                            ClosedNodes.Add(node);
                            for (int i = 0; i < 4; i++)
                            {
                                Point p = node + (new Vector2(Divider, 0).RotatedBy((MathHelper.ToRadians(i * 90))).ToPoint());

                                if (!ClosedNodes.Contains(p) && Bounds.Contains(p.X, p.Y)) OpenNodes.Add(p);
                            }
                            OpenNodes.Remove(node);
                        }
                    }
                    else
                    {
                        OpenNodes.Remove(node);
                    }
                }
            }

            position = new Point(0, 0);

            return false;
        }
    }
}
