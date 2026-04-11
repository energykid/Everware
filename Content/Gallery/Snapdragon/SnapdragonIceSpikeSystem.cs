using Everware.Common.Systems;
using Everware.Content.Base;
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

    internal static readonly InstancedRequestableTarget IceSpikeTarget = new InstancedRequestableTarget();

    public override void PostUpdateNPCs()
    {
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
                                plr.Hurt(PlayerDeathReason.ByCustomReason(NetworkText.FromKey(Mods.Everware.DeathReason.SnapdragonIce.KEY)), Snapdragon.FrostBreathDamage, Math.Sign(plr.Center.X - AllTriangles[i].Center.X), false, false, false, 20);
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

    public override void SetStaticDefaults()
    {
        Main.ContentThatNeedsRenderTargets.Add(IceSpikeTarget);
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
        if (AllTriangles.Count > 1)
        {
            int width = 2000;
            int height = 2000;

            Vector2 scrP = (Main.screenLastPosition - Main.screenPosition);
            scrP = new Vector2((float)Math.Floor(scrP.X / 2) * 2, (float)Math.Floor(scrP.Y / 2) * 2);

            InstancedRequestableTarget target = IceSpikeTarget;

            target.Request(width, height, 0, () =>
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, Main._multiplyBlendState, Main.DefaultSamplerState, null, null);

                List<Vector2> v2s = [];
                List<Color> cols = [];

                Vector2 basePos = GallerySystem.GalleryPosition.ToVector2() * 16f;

                for (int i = 0; i < AllTriangles.Count; i++)
                {

                    Vector2 pC = new Vector2((float)Math.Ceiling(AllTriangles[i].Center.X / 2) * 2, (float)Math.Ceiling(AllTriangles[i].Center.Y / 2) * 2);

                    pC = ((pC - (GallerySystem.GalleryPosition.ToVector2() * 16f)) / 2f) + new Vector2(width / 2f, height / 2f);

                    Vector2 p1 = (pC + (AllTriangles[i].P1 * AllTriangles[i].Scale / 2f));
                    Vector2 p2 = (pC + (AllTriangles[i].P2 * AllTriangles[i].Scale / 2f));
                    Vector2 p3 = (pC + (AllTriangles[i].P3 * AllTriangles[i].Scale / 2f));
                    Vector2 p4 = (pC - ((AllTriangles[i].P1 * AllTriangles[i].Scale / 2f) / 5f));

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

                    cols.Add(Color.Transparent);
                    cols.Add(Color.Transparent);
                    cols.Add(c);
                    cols.Add(Color.Transparent);
                    cols.Add(c);
                    cols.Add(Color.Transparent);
                    cols.Add(Color.Transparent);
                    cols.Add(Color.Transparent);
                    cols.Add(c);
                    cols.Add(Color.Transparent);
                    cols.Add(Color.Transparent);
                }

                PrimitiveDrawing.DrawPrimitiveStrip2(v2s, cols, new Vector2(width, height));

                Main.spriteBatch.End();
            });

            if (target.TryGetTarget(0, out RenderTarget2D spikeTarget))
            {
                var IceSpikeEffect = Assets.Effects.Gallery.SnapdragonIceSpikeColoration.CreateEffect();
                IceSpikeEffect.Parameters.Resolution = spikeTarget.Size();
                IceSpikeEffect.Parameters.Parallax = -Main.screenPosition / 4000f;
                IceSpikeEffect.Parameters.Timer = GlobalTimer.Value / 60f;
                IceSpikeEffect.Parameters.FillTexture = Assets.Textures.Gallery.IceSpikeSampler.Asset.Value;
                IceSpikeEffect.Apply();

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, Main._multiplyBlendState, Main.DefaultSamplerState, null, null, IceSpikeEffect.Shader, Main.GameViewMatrix.ZoomMatrix);

                Main.spriteBatch.Draw(spikeTarget, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, spikeTarget.Frame(), Color.White, 0f, spikeTarget.Size() / 2f, 2f, SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, Main._multiplyBlendState, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.ZoomMatrix);
            }
        }

        orig(self);
    }

    public override void Unload()
    {
        On_Main.DoDraw_Tiles_NonSolid -= DrawIceSpikes;
    }
}