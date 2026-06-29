using Everware.Content.Base.World;
using Everware.Content.Kiln.Tiles;
using Everware.Content.Underground.DeepCaveLoot;
using System.Collections.Generic;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Everware.Content.Underground;

public class UndergroundHouseEdits : ModSystem
{
    public static int DeepCaveLayer => (int)(Main.UnderworldLayer * 0.65f);

    public static List<int> DeepCaveLoot = [];
    public override void OnModLoad()
    {
        DeepCaveLoot = [
        ModContent.ItemType<Groundshakers>(),
        ModContent.ItemType<MasterLockpick>()
        ];
    }

    public override void PostWorldGen()
    {
        for (int i = 0; i < Main.chest.Length; i++)
        {
            if (Main.chest[i] != null)
            {
                Tile chestTile = Main.tile[Main.chest[i].x, Main.chest[i].y];
                if ((int)((float)chestTile.TileFrameX / 36f) == 1) // Gold Chest
                {
                    if (Main.chest[i].y > DeepCaveLayer)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            for (int l = 0; l < 2; l++)
                            {
                                Tile t = Main.tile[Main.chest[i].x + k, Main.chest[i].y + l];
                                t.TileType = (ushort)ModContent.TileType<SteelChestTile>();
                                t.TileFrameX -= 36;
                            }
                        }
                        Chest.Lock(Main.chest[i].x, Main.chest[i].y);
                        Chest chest = Main.chest[i];
                        chest.item[0] = new Item(DeepCaveLoot[Main.rand.Next(DeepCaveLoot.Count)]);
                    }
                    else
                    {
                        Chest chest = Main.chest[i];
                        if (Main.rand.NextBool(2))
                            chest.AddItemToShop(new Item(ItemID.GoldenKey));
                    }
                }
            }
        }
    }

    public override void Load()
    {
        On_HouseBuilder.FillRooms += AddBookshelves;
        On_HouseBuilder.PlaceEmptyRooms += DynamicChangeTileTypes;
    }

    public override void Unload()
    {
        On_HouseBuilder.FillRooms -= AddBookshelves;
        On_HouseBuilder.PlaceEmptyRooms -= DynamicChangeTileTypes;
    }

    // changes the tile type from wood to gray brick if the house is in the cavern layer
    private void DynamicChangeTileTypes(On_HouseBuilder.orig_PlaceEmptyRooms orig, HouseBuilder self)
    {
        bool normal = self.TileType == TileID.WoodBlock;

        if (self.TopRoom.Top >= DeepCaveLayer && normal)
        {
            self.TileType = TileID.GrayBrick;
            self.WallType = WallID.GrayBrick;
        }

        orig(self);

        foreach (Rectangle rect in self.Rooms)
        {
            for (int i = rect.Left; i <= rect.Right; i++)
            {
                for (int j = rect.Top; j <= rect.Bottom; j++)
                {
                    if (Main.tile[i, j].TileType == TileID.WoodBlock && Main.tile[i, j].HasTile)
                        Main.tile[i, j].ResetToType(WorldGen.genRand.NextBool(3) ? (ushort)ModContent.TileType<WornWoodPlaced>() : (ushort)ModContent.TileType<WeatheredWoodPlaced>());

                    if (Main.tile[i, j].HasTile && WorldGen.genRand.NextBool(3))
                    {
                        if (Main.tile[i, j].TileType == TileID.GrayBrick)
                            Main.tile[i, j].ResetToType(TileID.Stone);
                    }
                }
            }
        }
    }

    // adds platform bookshelves to the walls on occasion
    private void AddBookshelves(On_HouseBuilder.orig_FillRooms orig, HouseBuilder self)
    {
        if (self.TileType == TileID.WoodBlock || self.TileType == TileID.GrayBrick)
        {
            foreach (Rectangle room in self.Rooms)
            {
                if (Main.rand.NextBool())
                {
                    int rnd = (int)(room.Width * 0.3f) + Main.rand.Next(-2, 3);
                    Rectangle rect = new Rectangle((room.Width / 2) - (rnd / 2), 3, rnd, 1);

                    new Shapes.Rectangle(rect).Perform(new Point(room.X, room.Y), new Actions.PlaceTile(TileID.Platforms, self.PlatformStyle));
                    new Shapes.Rectangle(rect).Perform(new Point(room.X, room.Y), new Actions.SetFrames(true));
                    new Shapes.Rectangle(new Rectangle(rect.X, rect.Y - 1, rect.Width, rect.Height)).Perform(new Point(room.X, room.Y), new CustomGenActions.PlaceCaveBook());
                }
            }
        }

        orig(self);
    }
}
