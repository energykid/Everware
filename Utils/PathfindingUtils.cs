using System.Collections.Generic;

namespace Everware.Utils;

public static class PathfindingUtils
{
    const int WorldPadding = 200;
    public struct PathNodeSet(Point start, Rectangle bounds, int divider = 1)
    {
        public int Divider = divider;
        public Point Start = start;
        public Rectangle Bounds = bounds;
        public List<Point> OpenNodes = [];
        public List<Point> ClosedNodes = [];

        /// <summary>
        /// Tries to find the nearest of X tile type within PathNodeSet.Bounds, starting at PathNodeSet.Start.
        /// Uses Djikstra's algorithm.
        /// </summary>
        /// <param name="tileType">The tile ID to look for.</param>
        /// <param name="position">The position of the nearest tile of tileType.</param>
        /// <param name="maxAttempts">The maximum number of search attempts. Cap it lower for performance if needed. Use PathNodeSet.Divider instead if possible.</param>
        /// <returns></returns>
        public bool FindTile(int tileType, out Point position, int maxAttempts = 200)
        {
            OpenNodes.Add(Start);

            for (int gen = 0; gen < maxAttempts; gen++)
            {
                List<Point> nodesToCheck = OpenNodes;

                for (int k = 0; k < nodesToCheck.Count; k++)
                {
                    Point node = nodesToCheck[k];
                    if (node.X > WorldPadding && node.X < Main.maxTilesX - WorldPadding && node.Y > WorldPadding && node.Y < Main.maxTilesY - WorldPadding)
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
    public struct FlatnessCheck(Point position, Point scale, int divider = 1)
    {
        public int Divider = divider;
        public Rectangle Bounds = new Rectangle(position.X, position.Y, scale.X, scale.Y);
        public int OccupiedTilesTop = 0;
        public int NonOccupiedTilesBottom = 0;

        /// <summary>
        /// Determines how "uneven" the terrain in this FlatnessCheck is determined to be, where 0 is completely flat, and higher numbers represent more slope or unevenness.
        /// This is computationally expensive. Should only be used for world generation tasks. 
        /// If it absolutely needs to be used frequently, set FlatnessCheck.Divider to a higher value than 1.
        /// Results in a less accurate, but faster, solution.
        /// </summary>
        /// <returns>0 for a perfectly flat chunk of terrain, higher values for higher rigidity.</returns>
        public int ApproximateTerrainFlatness()
        {
            for (int i = Bounds.X; i < Bounds.X + Bounds.Width; i += Divider)
            {
                for (int j = Bounds.Y; j < Bounds.Y + Bounds.Height; j += Divider)
                {
                    if (i > WorldPadding && i < Main.maxTilesX - WorldPadding && j > WorldPadding && j < Main.maxTilesY - WorldPadding)
                    {
                        bool Top = j < Bounds.Y + (Bounds.Height / 2);
                        if (Main.tile[i, j].HasTile && Main.tileSolid[Main.tile[i, j].TileType])
                        {
                            if (Top) OccupiedTilesTop++;
                        }
                        else
                        {
                            if (!Top) NonOccupiedTilesBottom++;
                        }
                    }
                }
            }

            int slope = 0;

            // For every tile present above the half-way point, add 1.
            // For every tile not present below the half-way point, add 1.
            slope += OccupiedTilesTop + NonOccupiedTilesBottom;

            return slope;
        }
    }
}
