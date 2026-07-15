using Everware.Utils;
using System.IO;
using Terraria.ID;

namespace Everware.Content.Base.Tiles.TileData;

public struct LastPlayerMinedData : ITileData
{
    public int WhichPlayerAmI;
}

public class LastPlayerMinedPacket : EverPacket
{
    public int X;
    public int Y;
    public int player;
    public override void Read(Mod mod, BinaryReader reader, int playerID)
    {
        X = reader.ReadInt32();
        Y = reader.ReadInt32();
        player = reader.ReadInt32();

        Main.tile[X, Y].Get<LastPlayerMinedData>().WhichPlayerAmI = player;

        if (Main.netMode == NetmodeID.Server) EverwarePacketHandler.SendPacket(this);
    }
    public override void Write(ModPacket packet)
    {
        packet.Write(X);
        packet.Write(Y);
        packet.Write(player);
    }
}
public class LastPlayerMinedItem : GlobalItem
{
    public override void Load()
    {
        On_Player.GetPickaxeDamage += OnDamageTile;
    }

    public override void Unload()
    {
        On_Player.GetPickaxeDamage -= OnDamageTile;
    }

    private int OnDamageTile(On_Player.orig_GetPickaxeDamage orig, Player self, int x, int y, int pickPower, int hitBufferIndex, Tile tileTarget)
    {
        if (pickPower > 0 && tileTarget.HasTile)
        {
            if (self.whoAmI == Main.LocalPlayer.whoAmI)
            {
                if (tileTarget.HasTile)
                {
                    if (self.HeldItem.pick > 0)
                    {
                        tileTarget.Get<LastPlayerMinedData>().WhichPlayerAmI = self.whoAmI;

                        if (Main.netMode != NetmodeID.SinglePlayer)
                        {
                            if (Main.netMode == NetmodeID.Server) EverwarePacketHandler.SendPacket(new LastPlayerMinedPacket()
                            {
                                X = x,
                                Y = y,
                                player = self.whoAmI
                            });
                        }
                    }
                }
            }
        }

        return orig(self, x, y, pickPower, hitBufferIndex, tileTarget);
    }
}