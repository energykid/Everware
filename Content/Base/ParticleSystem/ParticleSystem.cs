using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;

namespace Everware.Content.Base.ParticleSystem;

public class ParticleSystem : ModSystem
{
    public static List<Particle> AllParticles = [];

    public override void PostDrawTiles()
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);
        for (int i = 0; i < AllParticles.Count; i++)
        {
            AllParticles[i].Draw();
        }
        Main.spriteBatch.End();
    }

    public override void PostUpdateEverything()
    {
        for (int i = 0; i < AllParticles.Count; i++)
        {
            AllParticles[i].Update();
        }
    }
}

public abstract class Particle : Entity
{
    public Vector2 FrameCount = Vector2.One;
    public Vector2 FrameNum = Vector2.Zero;
    public Color Color = Color.White;
    public float Rotation = 0f;
    public Vector2 Scale = Vector2.One;
    public SpriteEffects Effects = SpriteEffects.None;
    public bool DrawBelowEntities = false;
    public bool AffectedByLight = true;
    public float Opacity = 1f;

    public virtual Asset<Texture2D> Texture => null;

    public delegate void ParticleFunction(Particle p);

    public ParticleFunction UpdateFunction;
    public ParticleFunction DrawFunction;

    public void Spawn()
    {
        ParticleSystem.AllParticles.Add(this);
    }

    public void Kill()
    {
        ParticleSystem.AllParticles.Remove(this);
    }

    public Particle(Vector2 pos, Vector2 vel, Vector2 scale, ParticleFunction upd = null, ParticleFunction drw = null)
    {
        position = pos;
        velocity = vel;
        Scale = scale;
        UpdateFunction = upd;
        DrawFunction = drw;
    }

    public virtual void Update()
    {
        if (UpdateFunction != null)
            UpdateFunction(this);
        position += velocity;
    }
    public virtual void Draw()
    {
        if (DrawFunction != null)
            DrawFunction(this);
        if (Texture != null)
        {
            Color c = !AffectedByLight ? Color : Color.MultiplyRGBA(Lighting.GetColor((position / 16f).ToPoint()));
            Rectangle frame = Texture.Frame((int)FrameCount.X, (int)FrameCount.Y, (int)FrameNum.X, (int)FrameNum.Y);
            Main.EntitySpriteDraw(Texture.Value, position - Main.screenPosition, frame, Color.MultiplyRGBA(new(1f, 1f, 1f, Opacity)), Rotation, frame.Size() / 2f, Scale, Effects);
        }
    }
}