using Terraria.ID;

namespace Everware.Content.Underground.Glowcoat;

public class NeonGlowcoat : BaseGlowcoatItem
{
    public override int MossBlock => TileID.VioletMossBlock;
    public override Color Color => new Color(140, 113, 213);
    public override int DustType => DustID.VioletMoss;
    public override string Texture => "Everware/Assets/Textures/Underground/NeonGlowcoat";
}
