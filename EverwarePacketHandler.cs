using Everware.Common.Players;
using Everware.Core.Projectiles;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;

namespace Everware;

public static class EverwarePacketHandler
{
    public delegate void PacketBehavior(Mod mod, BinaryReader reader, int whoAmI, string identifier);
    public static List<PacketBehavior> CustomPackets = [];
    public static void AddPacket(PacketBehavior behavior)
    {
        CustomPackets.Add(behavior);
    }
    public static void HandleAllPackets(Mod mod, BinaryReader reader, int whoAmI)
    {
        string str = reader.ReadString();

        bool shouldRunNewPacketBehavior = true;

        switch (str)
        {
            case "MouseWorld": // Mouse world sending

                if (Main.netMode == NetmodeID.Server)
                {
                    int playerID1 = reader.ReadInt32();
                    Vector2 post1 = reader.ReadPackedVector2();

                    ModPacket p = mod.GetPacket();
                    p.Write("MouseWorld");
                    p.Write(playerID1);
                    p.WritePackedVector2(post1);

                    Main.player[playerID1].GetModPlayer<NetworkPlayer>().MousePosition = post1;

                    p.Send();
                }
                else
                {
                    int playerID2 = reader.ReadInt32();
                    Vector2 post2 = reader.ReadPackedVector2();

                    Main.player[playerID2].GetModPlayer<NetworkPlayer>().MousePosition = post2;
                }

                shouldRunNewPacketBehavior = false;
                break;
            case "ItemAnimationMax": // Item animation time sending/recieving
                if (Main.netMode == NetmodeID.Server)
                {
                    int playerID = reader.ReadInt32();
                    int time = reader.ReadInt32();

                    Main.player[playerID].GetModPlayer<NetworkPlayer>().AnimationTime = time;

                    ModPacket animPacket = mod.GetPacket();
                    animPacket.Write("ItemAnimationMax");
                    animPacket.Write(playerID);
                    animPacket.Write(time);
                    animPacket.Send();
                }
                else
                {
                    int playerID = reader.ReadInt32();
                    int time = reader.ReadInt32();

                    Main.player[playerID].GetModPlayer<NetworkPlayer>().AnimationTime = time;
                }

                shouldRunNewPacketBehavior = false;
                break;

            case "ControlUseItem": // Player mouse down sending/recieving
                if (Main.netMode == NetmodeID.Server)
                {
                    int playerID = reader.ReadInt32();
                    bool down = reader.ReadBoolean();

                    Main.player[playerID].GetModPlayer<NetworkPlayer>().MouseDown = down;

                    ModPacket animPacket = mod.GetPacket();
                    animPacket.Write("ControlUseItem");
                    animPacket.Write(playerID);
                    animPacket.Write(down);
                    animPacket.Send();
                }
                else
                {
                    int playerID = reader.ReadInt32();
                    bool down = reader.ReadBoolean();

                    Main.player[playerID].GetModPlayer<NetworkPlayer>().MouseDown = down;
                }

                shouldRunNewPacketBehavior = false;
                break;

            case "AltFunctionUse": // Player alt use down sending/recieving
                if (Main.netMode == NetmodeID.Server)
                {
                    int playerID = reader.ReadInt32();
                    int altFunc = reader.ReadInt32();

                    Main.player[playerID].GetModPlayer<NetworkPlayer>().AltFunction = altFunc;

                    ModPacket animPacket = mod.GetPacket();
                    animPacket.Write("AltFunctionUse");
                    animPacket.Write(playerID);
                    animPacket.Write(altFunc);
                    animPacket.Send();
                }
                else
                {
                    int playerID = reader.ReadInt32();
                    int altFunc = reader.ReadInt32();

                    Main.player[playerID].GetModPlayer<NetworkPlayer>().AltFunction = altFunc;
                }

                shouldRunNewPacketBehavior = false;
                break;

            case "NetOnHitEnemy": // NetOnHitNPC
                if (Main.netMode == NetmodeID.Server)
                {
                    int proj = reader.ReadInt32();
                    int npc = reader.ReadInt32();

                    ModPacket animPacket = mod.GetPacket();
                    animPacket.Write("NetOnHitEnemy");
                    animPacket.Write(proj);
                    animPacket.Write(npc);
                    animPacket.Send();

                    Projectile pr = Main.projectile.FirstOrDefault(x => x.identity == proj);

                    (pr.ModProjectile as EverProjectile).NetOnHitEnemy(Main.npc[npc]);
                }
                else
                {
                    int proj = reader.ReadInt32();
                    int npc = reader.ReadInt32();

                    Projectile pr = Main.projectile.FirstOrDefault(x => x.identity == proj);

                    (pr.ModProjectile as EverProjectile).NetOnHitEnemy(Main.npc[npc]);
                }

                shouldRunNewPacketBehavior = false;
                break;

            default: break;
        }

        if (shouldRunNewPacketBehavior)
        {
            foreach (PacketBehavior behavior in CustomPackets)
            {
                behavior(mod, reader, whoAmI, str);
            }
        }
    }
}
