using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;

namespace Everware.Content.Underground.Glowcoat;

public class KryptonGlowcoat : BaseGlowcoatItem
{
    public override int MossBlock => TileID.KryptonMossBlock;
    public override int MossItem => ItemID.KryptonMoss;
    public override Color Color => new Color(0, 255, 80);
    public override int DustType => DustID.KryptonMoss;
    public override Asset<Texture2D> GlowAsset => Assets.Textures.Underground.KryptonGlowcoat_Glow.Asset;
    public override string Texture => "Everware/Assets/Textures/Underground/KryptonGlowcoat";
}
