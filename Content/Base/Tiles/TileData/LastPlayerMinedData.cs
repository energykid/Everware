using Terraria.ID;

namespace Everware.Content.Base.Tiles.TileData;

public struct LastPlayerMinedData : ITileData
{
    public int WhichPlayerAmI;
}

public class LastPlayerMinedItem : GlobalItem
{
    public override void Load()
    {
        EverwarePacketHandler.AddPacket(
            (mod, reader, whoAmI, identifier) =>
            {
                if (identifier == "SetLastPlayerMined")
                {
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    int playerName = reader.ReadInt32();

                    Main.tile[x, y].Get<LastPlayerMinedData>().WhichPlayerAmI = playerName;

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = Everware.Instance.GetPacket();
                        packet.Write("SetLastPlayerMined");
                        packet.Write(x);
                        packet.Write(y);
                        packet.Write(playerName);
                        packet.Send();
                    }
                }
            }
        );

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
                            ModPacket packet = Everware.Instance.GetPacket();
                            packet.Write("SetLastPlayerMined");
                            packet.Write(x);
                            packet.Write(y);
                            packet.Write(self.whoAmI);
                            packet.Send();
                        }
                    }
                }
            }
        }

        return orig(self, x, y, pickPower, hitBufferIndex, tileTarget);
    }
}