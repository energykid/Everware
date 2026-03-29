using Terraria.ID;

namespace Everware.Content.Underground.Glowcoat;

public class XenonGlowcoat : BaseGlowcoatItem
{
    public override int MossBlock => TileID.XenonMossBlock;
    public override Color Color => new Color(31, 93, 240);
    public override int DustType => DustID.XenonMoss;
    public override string Texture => "Everware/Assets/Textures/Underground/XenonGlowcoat";
}
