using Everware.Core.Projectiles;
using Everware.Utils;
using System.IO;
using System.Linq;
using Terraria.ID;

namespace Everware.Common.Packets;

public class ProjectileHitEnemyPacket : EverPacket
{
    public int proj = -1;
    public int npc = -1;
    public override void Read(Mod mod, BinaryReader reader, int playerID)
    {
        int identity = reader.ReadInt32();
        int target = reader.ReadInt32();

        proj = identity;
        npc = target;

        Projectile p = Main.projectile.First(A =>
        {
            return A.identity == identity;
        });

        if (p.ModProjectile is EverProjectile p2)
        {
            p2.NetOnHitEnemy(Main.npc[npc]);
        }

        if (Main.netMode == NetmodeID.Server)
        {
            EverwarePacketHandler.SendPacket(this);
        }
    }
    public override void Write(ModPacket packet)
    {
        if (proj != -1 && npc != -1)
        {
            packet.Write(proj);
            packet.Write(npc);
        }
    }
}