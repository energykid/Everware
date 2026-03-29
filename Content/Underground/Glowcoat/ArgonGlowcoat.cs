using Terraria.ID;

namespace Everware.Content.Underground.Glowcoat;

public class ArgonGlowcoat : BaseGlowcoatItem
{
    public override int MossBlock => TileID.ArgonMossBlock;
    public override Color Color => new Color(255, 97, 143);
    public override int DustType => DustID.ArgonMoss;
    public override string Texture => "Everware/Assets/Textures/Underground/ArgonGlowcoat";
}
