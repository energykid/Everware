using Everware.Content.Base.World;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Everware.Content.Underground;

[Autoload]
public class UndergroundHouseEdits : ILoadable
{
    public void Load(Mod mod)
    {
        On_HouseBuilder.FillRooms += On_HouseBuilder_FillRooms;
    }

    private void On_HouseBuilder_FillRooms(On_HouseBuilder.orig_FillRooms orig, HouseBuilder self)
    {
        foreach (Rectangle room in self.Rooms)
        {
            if (Main.rand.NextBool())
            {
                int rnd = (int)(room.Width * 0.3f) + Main.rand.Next(-2, 3);
                Rectangle rect = new Rectangle((room.Width / 2) - (rnd / 2), 3, rnd, 1);

                new Shapes.Rectangle(rect).Perform(new Point(room.X, room.Y), Actions.Chain(new Actions.PlaceTile(TileID.Platforms), new Actions.SetFrames(true)));
                new Shapes.Rectangle(new Rectangle(rect.X, rect.Y - 1, rect.Width, rect.Height)).Perform(new Point(room.X, room.Y), new CustomGenActions.PlaceCaveBook());
            }
        }

        orig(self);
    }

    public void Unload()
    {
        On_HouseBuilder.FillRooms -= On_HouseBuilder_FillRooms;
    }
}
