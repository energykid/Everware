using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;

namespace Everware.Content.Underground.Glowcoat;

public class AuroralGlowcoat : BaseGlowcoatItem
{
    public override int MossBlock => TileID.IceBlock;
    public override int MossItem => ItemID.IceTorch;
    public override Color Color => new Color(31, 221, 213);
    public override int DustType => DustID.IceTorch;
    public override Asset<Texture2D> GlowAsset => Assets.Textures.Underground.AuroralGlowcoat_Glow.Asset;
    public override string Texture => "Everware/Assets/Textures/Underground/AuroralGlowcoat";
}
