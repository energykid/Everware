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
    public string Sprite = "";
    public Vector2 FrameCount = Vector2.One;
    public Vector2 FrameNum = Vector2.Zero;
    public Color Color = Color.White;
    public float Rotation = 0f;
    public Vector2 Scale = Vector2.One;
    public SpriteEffects Effects = SpriteEffects.None;
    public bool DrawBelowEntities = false;

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

    public Particle(string sprite, Vector2 pos, Vector2 vel, Vector2 scale, ParticleFunction upd = null, ParticleFunction drw = null)
    {
        Sprite = sprite;
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
        if (Sprite != "")
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Sprite);
            Rectangle frame = tex.Frame((int)FrameCount.X, (int)FrameCount.Y, (int)FrameNum.X, (int)FrameNum.Y);
            Main.EntitySpriteDraw(tex.Value, position - Main.screenPosition, frame, Color, Rotation, frame.Size() / 2f, Scale, Effects);
        }
    }
}