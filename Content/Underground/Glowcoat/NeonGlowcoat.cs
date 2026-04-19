using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;

namespace Everware.Content.Underground.Glowcoat;

public class NeonGlowcoat : BaseGlowcoatItem
{
    public override int MossBlock => TileID.VioletMoss;
    public override int MossItem => ItemID.VioletMoss;
    public override Color Color => new Color(167, 31, 197);
    public override int DustType => DustID.VioletMoss;
    public override Asset<Texture2D> GlowAsset => Assets.Textures.Underground.NeonGlowcoat_Glow.Asset;
    public override string Texture => "Everware/Assets/Textures/Underground/NeonGlowcoat";
}
