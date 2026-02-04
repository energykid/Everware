using System.Collections.Generic;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Everware.Content.Base.World;

public class CustomGenActions
{
    public class GrowGrass : GenAction
    {
        public GrowGrass() { }

        public override bool Apply(Point origin, int x, int y, params object[] args)
        {
            ushort type = TileID.Dirt;

            if (Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
            {
                type = Main.tile[x, y].TileType;

                if (type == TileID.Dirt)
                {
                    if (!Main.tile[x + 1, y].HasTile ||
                        !Main.tile[x - 1, y].HasTile ||
                        !Main.tile[x, y + 1].HasTile ||
                        !Main.tile[x, y - 1].HasTile ||
                        !Main.tile[x + 1, y + 1].HasTile ||
                        !Main.tile[x - 1, y + 1].HasTile ||
                        !Main.tile[x - 1, y - 1].HasTile ||
                        !Main.tile[x + 1, y - 1].HasTile)
                    {
                        Main.tile[x, y].ResetToType(TileID.Grass);
                        WorldUtils.TileFrame(x, y, true);
                    }
                }
            }

            return UnitApply(origin, x, y, args);
        }
    }

    public class SetSilt : GenAction
    {
        public SetSilt() { }

        public override bool Apply(Point origin, int x, int y, params object[] args)
        {
            ushort type = TileID.Dirt;

            if (Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
            {
                type = Main.tile[x, y].TileType;
                Main.tile[x, y].ResetToType(TileID.Silt);
            }

            if (!Main.tile[x, y + 1].HasTile && Main.tile[x, y].TileType == TileID.Silt)
            {
                Main.tile[x - 1, y + 1].ResetToType(TileID.Stone);
                Main.tile[x + 1, y + 1].ResetToType(TileID.Stone);
                Main.tile[x, y + 1].ResetToType(TileID.Stone);
                Main.tile[x, y + 2].ResetToType(TileID.Stone);
            }

            WorldUtils.TileFrame(x, y, true);
            return UnitApply(origin, x, y, args);
        }
    }
    public class PoundTile : GenAction
    {
        public PoundTile() { }

        public override bool Apply(Point origin, int x, int y, params object[] args)
        {
            WorldGen.PoundTile(x, y);
            return UnitApply(origin, x, y, args);
        }
    }

    public class ClearTileExcept : GenAction
    {
        private ushort id;
        public ClearTileExcept(ushort ID)
        {
            id = ID;
        }

        public override bool Apply(Point origin, int x, int y, params object[] args)
        {
            if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType != id)
            {
                WorldGen.KillTile(x, y, false, false, true);
            }

            WorldUtils.TileFrame(x, y, true);
            return UnitApply(origin, x, y, args);
        }
    }

    public class SetTileFromOther : GenAction
    {
        private ushort id;
        public SetTileFromOther(ushort ID)
        {
            id = ID;
        }

        public override bool Apply(Point origin, int x, int y, params object[] args)
        {
            if (Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
            {
                WorldGen.KillTile(x, y, false, false, true);
                WorldGen.PlaceTile(x, y, id, true);
            }

            WorldUtils.TileFrame(x, y, true);
            return UnitApply(origin, x, y, args);
        }
    }

    public class ClearTileForRoom : GenAction
    {
        private ushort id;
        private ushort wallid;
        public ClearTileForRoom(ushort tileID, ushort wallID)
        {
            id = tileID;
            wallid = wallID;
        }

        public override bool Apply(Point origin, int x, int y, params object[] args)
        {
            List<Point> ps =
            [
                new Point(-1, 0),
                new Point(-1, -1),
                new Point(1, 0),
                new Point(1, -1),
                new Point(0, 1),
                new Point(0, -1),
                new Point(1, 1),
                new Point(-1, 1),
            ];

            foreach (Point p in ps)
            {
                Point p2 = new Point(x + p.X, y + p.Y);
                if (Main.tile[p2].HasTile && Main.tileSolid[Main.tile[p2].TileType])
                {
                    WorldGen.KillWall(p2.X, p2.Y);
                    WorldGen.PlaceWall(p2.X, p2.Y, wallid);
                    Main.tile[p2].ResetToType(id);
                    WorldUtils.TileFrame(p2.X, p2.Y, true);
                }
            }

            WorldGen.KillWall(x, y);
            WorldGen.PlaceWall(x, y, wallid);
            WorldGen.KillTile(x, y, false, false, true);

            WorldUtils.TileFrame(x, y, true);
            return UnitApply(origin, x, y, args);
        }
    }

    public class SetWall : GenAction
    {
        private ushort wallid;
        public SetWall(ushort wallID)
        {
            wallid = wallID;
        }

        public override bool Apply(Point origin, int x, int y, params object[] args)
        {
            WorldUtils.ClearWall(x, y);
            Main.tile[x, y].ClearWallPaintAndCoating();
            WorldGen.PlaceWall(x, y, wallid);

            WorldUtils.TileFrame(x, y, true);

            return UnitApply(origin, x, y, args);
        }
    }

    public class SetWallBetweenTwo : GenAction
    {
        private ushort wallid;
        private ushort wallid2;
        public SetWallBetweenTwo(ushort wallID, ushort wallID2)
        {
            wallid = wallID;
            wallid2 = wallID2;
        }

        public override bool Apply(Point origin, int x, int y, params object[] args)
        {
            WorldUtils.ClearWall(x, y);
            Main.tile[x, y].ClearWallPaintAndCoating();
            WorldGen.PlaceWall(x, y, Main.rand.NextBool() ? wallid : wallid2);

            WorldUtils.TileFrame(x, y, true);

            return UnitApply(origin, x, y, args);
        }
    }

    public class SetTileBetweenTwo : GenAction
    {
        private ushort id1;
        private ushort id2;
        public SetTileBetweenTwo(ushort ID1, ushort ID2)
        {
            id1 = ID1;
            id2 = ID2;
        }

        public override bool Apply(Point origin, int x, int y, params object[] args)
        {
            WorldUtils.ClearTile(x, y);

            Main.tile[x, y].ResetToType(Main.rand.NextBool() ? id1 : id2);

            WorldUtils.TileFrame(x, y, true);

            return UnitApply(origin, x, y, args);
        }
    }
}
