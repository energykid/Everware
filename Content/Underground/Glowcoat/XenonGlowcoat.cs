using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;

namespace Everware.Content.Underground.Glowcoat;

public class XenonGlowcoat : BaseGlowcoatItem
{
    public override int MossBlock => TileID.XenonMoss;
    public override int MossItem => ItemID.XenonMoss;
    public override Color Color => new Color(31, 93, 240);
    public override int DustType => DustID.XenonMoss;
    public override Asset<Texture2D> GlowAsset => Assets.Textures.Underground.XenonGlowcoat_Glow.Asset;
    public override string Texture => "Everware/Assets/Textures/Underground/XenonGlowcoat";
}
