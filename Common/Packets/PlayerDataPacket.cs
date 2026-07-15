using Everware.Common.Players;
using System.IO;
using Terraria.ID;

namespace Everware.Common.Packets;

public class PlayerDataPacket : EverPacket
{
    public Player? plr = null;
    public override void Read(Mod mod, BinaryReader reader, int playerID)
    {
        int whoAmI = reader.ReadInt32();
        Vector2 mousePos = reader.ReadVector2();
        bool mouseDown = reader.ReadBoolean();
        int animTime = reader.ReadInt32();
        int altFunc = reader.ReadInt32();
        bool rightClicking = reader.ReadBoolean();

        plr = Main.player[whoAmI];

        NetworkPlayer nPlr = plr.GetModPlayer<NetworkPlayer>();

        nPlr.MousePosition = mousePos;
        nPlr.MouseDown = mouseDown;
        nPlr.AnimationTime = animTime;
        nPlr.AltFunction = altFunc;
        nPlr.RightClicking = rightClicking;

        if (Main.netMode == NetmodeID.Server)
        {
            EverwarePacketHandler.SendPacket(this);
        }
    }
    public override void Write(ModPacket packet)
    {
        if (plr != null)
        {
            NetworkPlayer nPlr = plr.GetModPlayer<NetworkPlayer>();

            packet.Write(plr.whoAmI);

            packet.WriteVector2(nPlr.MousePosition);
            packet.Write(nPlr.MouseDown);
            packet.Write(nPlr.AnimationTime);
            packet.Write(nPlr.AltFunction);
            packet.Write(nPlr.RightClicking);
        }
    }
}