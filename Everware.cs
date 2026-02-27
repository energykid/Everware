using System.IO;

namespace Everware;

public class Everware : Mod
{
    public static Everware Instance;
    public Everware()
    {
        MusicAutoloadingEnabled = false;
        Instance = this;
    }
    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        EverwarePacketHandler.HandleAllPackets(Instance, reader, whoAmI);
    }
}
