using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;

namespace Everware.Content.Underground.Glowcoat;

public class ArgonGlowcoat : BaseGlowcoatItem
{
    public override int MossBlock => TileID.ArgonMoss;
    public override int MossItem => ItemID.ArgonMoss;
    public override Color Color => new Color(255, 31, 89);
    public override int DustType => DustID.ArgonMoss;
    public override Asset<Texture2D> GlowAsset => Assets.Textures.Underground.ArgonGlowcoat_Glow.Asset;
    public override string Texture => "Everware/Assets/Textures/Underground/ArgonGlowcoat";
}
