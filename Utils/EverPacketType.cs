using System.IO;

namespace Everware.Utils;

public abstract class EverPacket : ILoadable
{
    public void Load(Mod mod)
    {
        EverwarePacketHandler.AddPacket(PacketBehavior);
    }

    public virtual void PacketBehavior(Mod mod, BinaryReader reader, int whoAmI, string identifier)
    { }

    public void Unload()
    { }
}
