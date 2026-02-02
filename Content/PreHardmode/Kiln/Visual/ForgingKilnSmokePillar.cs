using Everware.Content.Base.ParticleSystem;
using Everware.Content.PreHardmode.Kiln.Tiles;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;

namespace Everware.Content.PreHardmode.Kiln.Visual;

public class ForgingKilnSmokePillar : Particle
{
    public ForgingKilnSmokePillar(string sprite, Vector2 pos, Vector2 vel, Vector2 scale, ParticleFunction upd = null, ParticleFunction drw = null) : base(sprite, pos, vel, scale, upd, drw)
    {
        Sprite = sprite;
        position = pos;
        velocity = vel;
        Scale = scale;
        UpdateFunction = upd;
        DrawFunction = drw;
    }
    public ForgingKilnSmokePillar(Vector2 pos) : base("", pos, Vector2.Zero, Vector2.One, null, null) { }

    float PillarPosition = 0f;
    float Timer = 0f;
    float OutsetMultiplier = 1f;
    float SpawnTimer = 0f;
    bool Destroyed = false;

    public override void Update()
    {
        base.Update();

        if (Main.tile[((position + new Vector2(8, 8)) / 16).ToPoint()].TileType == ModContent.TileType<ForgingKiln>() && !Destroyed)
        {
            SpawnTimer += 0.03f;
        }
        else
        {
            Destroyed = true;
            velocity += new Vector2(Main.windSpeedCurrent / 60, -0.01f);
            SpawnTimer -= 0.008f;
            OutsetMultiplier = MathHelper.Lerp(OutsetMultiplier, 2f, 0.01f);
        }

        SpawnTimer = Math.Clamp(SpawnTimer, -1f, 1f);

        PillarPosition += 1f + (3f * Math.Abs(Main.windSpeedCurrent)) + Math.Abs(velocity.X * 5);

        if (SpawnTimer < 0f) Kill();
    }
    static readonly Asset<Texture2D> Tex = ModContent.Request<Texture2D>("Everware/Content/PreHardmode/Kiln/Visual/ForgingKilnSmokePillar");
    public override void Draw()
    {
        Vector2 origin = position + new Vector2(48, 4);

        origin.X = (float)Math.Floor(origin.X / 2) * 2;

        for (int i = 0; i < 111; i++)
        {
            float p = (int)((i + PillarPosition) % 111);

            float lrp = MathHelper.Lerp(1f, 0f, (float)p / 111);

            float outset = lrp * 100 * OutsetMultiplier;

            float pos = ((float)Math.Sin(((float)p / 10) + (PillarPosition / 80)));

            pos += (Main.windSpeedCurrent * outset) / 30;

            pos *= outset;

            pos = (float)Math.Floor(pos / 2) * 2;

            Vector2 finalPosition = origin + new Vector2(pos, (float)Math.Ceiling(p) * 2f);

            float alph = SpawnTimer / 2f;

            if (SpawnTimer < 1 && Destroyed)
            {
                alph -= (SpawnTimer / 111);
            }

            Main.EntitySpriteDraw(Tex.Value, finalPosition - Main.screenPosition, Tex.Frame(1, 111, 0, (int)Math.Ceiling(p)), Lighting.GetColor(((finalPosition + new Vector2(-0, -222)) / 16).ToPoint()).MultiplyRGBA(new(alph, alph, alph, alph)), 0f, new Vector2(Tex.Width() / 2, Tex.Height()), 1f, SpriteEffects.None);
        }
    }
}