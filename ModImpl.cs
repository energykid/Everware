using System.IO;

namespace Everware;

public class ModImpl : Mod
{
    public static ModImpl Instance;
    public ModImpl()
    {
        MusicAutoloadingEnabled = false;
        Instance = this;
    }
    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        EverwarePacketHandler.HandleAllPackets(Instance, reader, whoAmI);
    }
}
