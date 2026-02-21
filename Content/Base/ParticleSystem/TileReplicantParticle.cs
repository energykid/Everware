using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;

namespace Everware.Content.Base.ParticleSystem;

public class TileReplicantParticle : Particle
{
    int TileType = TileID.Dirt;
    public int HideTimer = 0;
    public float StartingY = 0f;
    Rectangle TileFrame = new Rectangle(0, 0, 16, 16);
    public TileReplicantParticle(int tileType, Rectangle tileFrame, Vector2 pos, Vector2 vel, Vector2 scale, ParticleFunction upd = null, ParticleFunction drw = null) : base(pos, vel, scale, upd, drw)
    {
        TileType = tileType;
        TileFrame = tileFrame;
    }
    public override void Draw()
    {
        Asset<Texture2D> t = TextureAssets.Tile[TileType];

        if (HideTimer <= 0)
            Main.EntitySpriteDraw(t.Value, position - Main.screenPosition, TileFrame, Lighting.GetColor((position / 16).ToPoint()).MultiplyRGBA(new Color(1f, 1f, 1f, Opacity)), Rotation, new Vector2(8, 8), Scale, SpriteEffects.None);
    }
}
