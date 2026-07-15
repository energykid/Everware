using System.IO;
using Terraria.ID;

namespace Everware.Common;

public abstract class EverPacket : ILoadable
{
    public EverPacket() { }

    public virtual void Write(ModPacket packet) { }
    public virtual void Read(Mod mod, BinaryReader reader, int playerID) { }

    public void Load(Mod mod)
    {
        EverwarePacketHandler.CustomPacketReaders.Add(Read);
        EverwarePacketHandler.CustomPacketNames.Add(GetType().Name);
    }

    public void Send()
    {
        EverwarePacketHandler.SendPacket(this);
    }

    public void Unload()
    { }

    public void RelayFromServer()
    {
        if (Main.netMode == NetmodeID.Server) EverwarePacketHandler.SendPacket(this);
    }
}
