using System.IO;

namespace Everware;

public class Everware : Mod
{
    public Everware()
    {
        MusicAutoloadingEnabled = false;
    }
    public Everware Instance => this;
    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        base.HandlePacket(reader, whoAmI);
        EverwarePacketHandler.HandleAllPackets(Instance, reader, whoAmI);
    }
}
