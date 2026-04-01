using Everware.Content.Base.World;
using Everware.Content.Underground.DeepCaveLoot;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Everware.Content.Underground;

public class UndergroundHouseEdits : ModSystem
{
    public static int DeepCaveLayer => (int)(Main.UnderworldLayer * 0.65f);

    public override void PostWorldGen()
    {
        for (int i = 0; i < Main.chest.Length; i++)
        {
            if (Main.chest[i] != null)
            {
                if (Main.chest[i].y > DeepCaveLayer)
                {
                    Tile chestTile = Main.tile[Main.chest[i].x, Main.chest[i].y];
                    if ((int)((float)chestTile.TileFrameX / 36f) == 1) // Gold Chest
                    {
                        Chest chest = Main.chest[i];
                        if (Main.rand.NextBool(6))
                            chest.item[0] = new Item(ModContent.ItemType<Groundshakers>());
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
