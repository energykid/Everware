using Daybreak.Common.Rendering;
using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.ParticleSystem;
using Everware.Utils;
using System;
using System.Collections.Generic;
using Terraria.Localization;

namespace Everware.Content.Gallery.Snapdragon;

public class SnapdragonIceSpikeSystem : ModSystem
{
    public class IceTriangle(Vector2 cen, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        public Vector2 Center { get; set; } = cen;
        public Vector2 P1 { get; set; } = p1;
        public Vector2 P2 { get; set; } = p2;
        public Vector2 P3 { get; set; } = p3;
        public int Timer = 0;
        float v = Main.rand.NextFloat(-0.02f, 0.02f);
        public float Scale = 0f;
        float targetScale = Main.rand.NextFloat(1f, 1.5f);
        public void Update()
        {
            Timer++;
            if (Timer < 50)
            {
                Scale = MathHelper.Lerp(Scale, targetScale, 0.2f);
                v *= 0.9f;
                P1 = P1.RotatedBy(v);
            }
            if (Timer > 340)
            {
                P1 *= 0.9f;
                P2 *= 0.9f;
                P3 *= 0.9f;
            }
        }
    }

    public static List<IceTriangle> AllTriangles = [];
    public static ParticleLayer IceLayer = new ParticleLayer(0.5f);

    public override void PostUpdateNPCs()
    {
        IceLayer.Update();
        if (AllTriangles.Count > 1)
        {
            for (int i = 0; i < AllTriangles.Count; i++)
            {
                if (i < AllTriangles.Count && AllTriangles[i] != null)
                {
                    AllTriangles[i].Update();

                    foreach (Player plr in Main.ActivePlayers)
                    {
                        if (MathUtils.PointInTriangle(plr.Center, AllTriangles[i].Center + (AllTriangles[i].P1 * AllTriangles[i].Scale), AllTriangles[i].Center + (AllTriangles[i].P2 * AllTriangles[i].Scale), AllTriangles[i].Center + (AllTriangles[i].P3 * AllTriangles[i].Scale)))
                        {
                            if (plr.immuneTime <= 0)
                            {
                                plr.Hurt(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(plr.name + " " + Mods.Everware.DeathReason.SnapdragonIce.GetText())), Snapdragon.IceSpikeDamage, Math.Sign(plr.Center.X - AllTriangles[i].Center.X), false, false, false, 20);

                                plr.immune = true;
                                plr.immuneTime = 20;
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < AllTriangles.Count; i++)
            {
                if (i < AllTriangles.Count && AllTriangles[i] != null)
                {
                    if (AllTriangles[i].Timer > 360)
                    {
                        AllTriangles.Remove(AllTriangles[i]);
                        if (AllTriangles.Count < 1) break;
                    }
                }
            }

        }
    }

    public override void Load()
    {
        On_Main.DoDraw_Tiles_NonSolid += DrawIceSpikes;
    }

    public Vector2 CameraPos()
    {
        return new Vector2((float)Math.Floor(Main.screenPosition.X / 2) * 2, (float)Math.Floor(Main.screenPosition.X / 2) * 2);
    }

    private void DrawIceSpikes(On_Main.orig_DoDraw_Tiles_NonSolid orig, Main self)
    {
        int w = 2000;
        int h = 2000;

        Vector2 scrP = (Main.screenLastPosition - Main.screenPosition);
        scrP = new Vector2((float)Math.Floor(scrP.X / 2) * 2, (float)Math.Floor(scrP.Y / 2) * 2);

        var spikeTarget = ScreenspaceTargetPool.Shared.Rent(
            Main.instance.GraphicsDevice,
            (width, height) => (2000, 2000)
        );

        Main.spriteBatch.End(out var sb);

        using (spikeTarget.Scope(clearColor: Color.Transparent))
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, Main._multiplyBlendState, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            if (AllTriangles.Count > 1)
            {
                List<Vector2> v2s = [];
                List<Color> cols = [];

                Vector2 basePos = GallerySystem.GalleryPosition.ToVector2() * 16f;

                for (int i = 0; i < AllTriangles.Count; i++)
                {
                    Vector2 pC = new Vector2((float)Math.Ceiling(AllTriangles[i].Center.X / 2) * 2, (float)Math.Ceiling(AllTriangles[i].Center.Y / 2) * 2);

                    pC = ((pC - (GallerySystem.GalleryPosition.ToVector2() * 16f))) + new Vector2(w, h);

                    Vector2 p1 = (pC + (AllTriangles[i].P1 * AllTriangles[i].Scale));
                    Vector2 p2 = (pC + (AllTriangles[i].P2 * AllTriangles[i].Scale));
                    Vector2 p3 = (pC + (AllTriangles[i].P3 * AllTriangles[i].Scale));
                    Vector2 p4 = (pC - ((AllTriangles[i].P1 * AllTriangles[i].Scale / 10f)));

                    v2s.Add(pC);
                    v2s.Add(pC);
                    v2s.Add(p1);
                    v2s.Add(pC);
                    v2s.Add(p2);
                    v2s.Add(pC);
                    v2s.Add(p4);
                    v2s.Add(pC);
                    v2s.Add(p3);
                    v2s.Add(pC);
                    v2s.Add(pC);

                    Color c = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    Color c2 = new Color(0.15f, 0.15f, 0.15f, 0.15f);

                    cols.Add(Color.Transparent);
                    cols.Add(Color.Transparent);
                    cols.Add(c);
                    cols.Add(Color.Transparent);
                    cols.Add(c2);
                    cols.Add(Color.Transparent);
                    cols.Add(Color.Transparent);
                    cols.Add(Color.Transparent);
                    cols.Add(c2);
                    cols.Add(Color.Transparent);
                    cols.Add(Color.Transparent);
                }

                for (int i = 0; i < v2s.Count; i++) v2s[i] /= 2f;

                PrimitiveDrawing.DrawPrimitiveStrip2(v2s, cols, new Vector2(w, h));
            }

            IceLayer.Draw();

            Main.spriteBatch.End();
        }

        Main.spriteBatch.Begin(sb);

        var IceSpikeEffect = Assets.Effects.Gallery.Snapdragon.SnapdragonIceSpikeColoration.CreateEffect();
        IceSpikeEffect.Parameters.Resolution = spikeTarget.Target.Size();
        IceSpikeEffect.Parameters.Parallax = -Main.screenPosition / 4000f;
        IceSpikeEffect.Parameters.Timer = GlobalTimer.Value / 60f;
        IceSpikeEffect.Parameters.FillTexture = Assets.Textures.Gallery.IceSpikeSampler.Asset.Value;
        IceSpikeEffect.Apply();

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, Main._multiplyBlendState, Main.DefaultSamplerState, null, null, IceSpikeEffect.Shader, Main.GameViewMatrix.ZoomMatrix);

        Main.spriteBatch.Draw(spikeTarget.Target, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, spikeTarget.Target.Frame(), Color.White, 0f, spikeTarget.Target.Size() / 2f, 2f, SpriteEffects.None, 0f);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, Main._multiplyBlendState, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.ZoomMatrix);

        spikeTarget.Dispose();

        orig(self);
    }

    public override void Unload()
    {
        On_Main.DoDraw_Tiles_NonSolid -= DrawIceSpikes;
    }
}


public class IceSpikeTextureFlashParticle : Particle
{
    public Asset<Texture2D> MyTexture = null;
    public override Asset<Texture2D> Texture => MyTexture;
    public IceSpikeTextureFlashParticle(Vector2 pos, Vector2 vel, Vector2 scale, Asset<Texture2D> asset) : base(pos, vel, scale, null, null)
    {
        MyTexture = asset;
        Scale = scale;
    }
    public Vector2 GetPosition()
    {
        Vector2 pC = new Vector2((float)Math.Ceiling(position.X / 2) * 2, (float)Math.Ceiling(position.Y / 2) * 2);

        pC = ((pC - (GallerySystem.GalleryPosition.ToVector2() * 16f))) + new Vector2(2000, 2000);

        pC /= 2f;

        return pC;
    }
    public override Vector2 VisualPosition => GetPosition();
}
