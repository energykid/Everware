using Terraria.ID;

namespace Everware.Content.Underground.Glowcoat;

public class ChromaticGlowcoat : BaseGlowcoatItem
{
    public override int MossBlock => TileID.RainbowMoss;
    public override int MossItem => ItemID.RainbowMoss;
    public override Color Color => new Color(255, 255, 255);
    public override bool Chromatic => true;
    public override int DustType => DustID.RainbowRod;
    public override Asset<Texture2D> GlowAsset => Assets.Textures.Underground.ChromaticGlowcoat_Glow.Asset;
    public override string Texture => "Everware/Assets/Textures/Underground/ChromaticGlowcoat";
}
