using System.Collections.Generic;

namespace Everware.Common.Systems
{
    public struct DeferredSprite
    {
        public Texture2D sprite;
        public Vector2 position;
        public Rectangle sourceRectangle;
        public Color color;
        public Vector2 origin;
        public float rotation;
        public Vector2 scale;
        public SpriteEffects spriteEffects;
        public Effect shaderEffect;

        /// <summary>
        /// This exists so I can defer shader parameters per-draw, since these are all drawn at once but might be instantiated at different places.
        /// </summary>
        public Action setParams;

        public bool additive;
        public bool postAll;
    }
    public struct DeferredPrim
    {
        public Texture2D sprite;
        public List<Vector2> vertices;
        public List<Vector2> texcoords;
        public List<Color> colors;
        public Effect shaderEffect;

        /// <summary>
        /// This exists so I can defer shader parameters per-draw, since these are all drawn at once but might be instantiated at different places.
        /// </summary>
        public Action setParams;

        public PrimitiveType primitiveType;

        public bool additive;
        public bool postAll;

        public void Draw(bool shouldBlowUp = false)
        {
            if (primitiveType == PrimitiveType.TriangleStrip)
            {
                if (!shouldBlowUp)
                {
                    List<Vector2> verts = vertices;

                    for (int i = 0; i < verts.Count; i++)
                    {
                        verts[i] /= 2;
                    }

                    PrimitiveDrawing.DrawPrimitiveStrip(vertices, colors, sprite, texcoords, additive, shaderEffect);
                }
                else PrimitiveDrawing.DrawPrimitiveStrip(vertices, colors, sprite, texcoords, additive, shaderEffect);
            }
            /*
            else if (primitiveType == PrimitiveType.LineStrip)
            {
                if (!shouldBlowUp)
                {
                    List<Vector2> verts = vertices;

                    for (int i = 0; i < verts.Count; i++)
                    {
                        verts[i] /= 2;
                    }

                    PrimitiveDrawing.DrawPrimitiveLine(vertices, colors, sprite, texcoords, additive, shaderEffect);
                }
                else PrimitiveDrawing.DrawPrimitiveLine(vertices, colors, sprite, texcoords, additive, shaderEffect);
            }
            */
        }
    }

    public class PixelRendering : ModSystem
    {
        public override void PostSetupContent()
        {
            ThreadUtils.RunOnMainThread(() =>
            {
                PixelTarget = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice,
                    Main.screenWidth / 2, Main.screenHeight / 2);
                AdditivePixelTarget = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice,
                Main.screenWidth / 2, Main.screenHeight / 2);
            });
        }
        public override void Unload()
        {
            PixelTarget.Dispose();
            AdditivePixelTarget.Dispose();
        }
        public static RenderTargetLease PixelTarget;
        public static RenderTargetLease AdditivePixelTarget;

        public static List<DeferredSprite> Draws = [];
        public static List<DeferredPrim> Prims = [];

        public override void Load()
        {
            On_Main.DoDraw_Tiles_Solid += DrawPixelation;
        }

        private void DrawPixelation(On_Main.orig_DoDraw_Tiles_Solid orig, Main self)
        {
            orig(self);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            using (PixelTarget.Scope(clearColor: Color.Transparent))
            {
                foreach (DeferredSprite draw in Draws)
                {
                    Vector2 drawPosition = new Vector2((float)Math.Floor(draw.position.X / 2), (float)Math.Floor(draw.position.Y / 2));

                    if (!draw.additive)
                    {
                        Main.spriteBatch.End(out var sb);

                        Main.spriteBatch.Begin(sb with { CustomEffect = draw.shaderEffect });

                        draw.setParams?.Invoke();

                        Main.spriteBatch.Draw(draw.sprite, drawPosition, draw.sourceRectangle, draw.color, draw.rotation, draw.origin, draw.scale / 2, draw.spriteEffects, 0);
                    }
                }
            }

            using (AdditivePixelTarget.Scope(clearColor: Color.Transparent))
            {
                foreach (DeferredSprite draw in Draws)
                {
                    Vector2 drawPosition = new Vector2((float)Math.Floor(draw.position.X / 2), (float)Math.Floor(draw.position.Y / 2));

                    if (draw.additive)
                    {
                        Main.spriteBatch.End(out var sb);

                        Main.spriteBatch.Begin(sb with { CustomEffect = draw.shaderEffect });

                        draw.setParams?.Invoke();

                        Main.spriteBatch.Draw(draw.sprite, drawPosition, draw.sourceRectangle, draw.color, draw.rotation, draw.origin, draw.scale / 2, draw.spriteEffects, 0);
                    }
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(PixelTarget.Target, Vector2.Zero, PixelTarget.Target.Bounds, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0);

            Main.spriteBatch.End(out var ss);
            Main.spriteBatch.Begin(ss with { BlendState = BlendState.Additive });

            Main.spriteBatch.Draw(AdditivePixelTarget.Target, Vector2.Zero, AdditivePixelTarget.Target.Bounds, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0);

            Main.spriteBatch.End();

            Draws.Clear();
            Prims.Clear();
        }

        public static void DrawAllPixelatedSprites()
        {
            /*
                bool b = true; // this bool should be made to represent whether the lighting mode is Color/White or Retro/Trippy

                if (Lighting.Mode == LightMode.Retro || Lighting.Mode == LightMode.Trippy) b = false;

                Vector2 pos = Main.screenLastPosition - Main.screenPosition;

                if (b)
                {
                    if (PixelatedRenderTarget == null)
                        PixelatedRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);

                    if (PixelatedRenderTarget.Width != Main.screenWidth || PixelatedRenderTarget.Height != Main.screenHeight)
                    {
                        PixelatedRenderTarget.Dispose();
                        PixelatedRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
                    }

                    if (AdditivePixelatedRenderTarget == null)
                        AdditivePixelatedRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);

                    if (AdditivePixelatedRenderTarget.Width != Main.screenWidth || AdditivePixelatedRenderTarget.Height != Main.screenHeight)
                    {
                        AdditivePixelatedRenderTarget.Dispose();
                        AdditivePixelatedRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
                    }

                    Main.graphics.GraphicsDevice.SetRenderTarget(PixelatedRenderTarget);

                    Main.graphics.GraphicsDevice.Clear(Color.Transparent);
                }


                //Main.spriteBatch.End();
                Effect eff = null;
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (DeferredSprite draw in Draws)
                {
                    if (eff != draw.shaderEffect)
                    {
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, draw.shaderEffect, Main.GameViewMatrix.TransformationMatrix);
                        eff = draw.shaderEffect;
                    }

                    Vector2 drawPosition = new Vector2((float)Math.Floor(draw.position.X / 2), (float)Math.Floor(draw.position.Y / 2)) + (pos / 2);
                    if (!b) drawPosition = new Vector2((float)Math.Floor(draw.position.X), (float)Math.Floor(draw.position.Y)) + (pos);

                    if (!draw.additive)
                    {
                        if (draw.setParams != null)
                            draw.setParams();

                        Main.spriteBatch.Draw(draw.sprite, drawPosition, draw.sourceRectangle, draw.color, draw.rotation, draw.origin, b ? draw.scale / 2 : draw.scale, draw.spriteEffects, 0);
                    }
                }

                foreach (DeferredPrim prim in Prims)
                {
                    if (!prim.additive)
                    {
                        if (prim.setParams != null)
                            prim.setParams();

                        prim.Draw(b);
                    }
                }

                if (b)
                {
                    Main.graphics.GraphicsDevice.SetRenderTarget(AdditivePixelatedRenderTarget);
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                eff = null;

                foreach (DeferredSprite draw in Draws)
                {
                    if (eff != draw.shaderEffect)
                    {
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, null, Main.Rasterizer, draw.shaderEffect, Main.GameViewMatrix.TransformationMatrix);
                        eff = draw.shaderEffect;
                    }

                    Vector2 drawPosition = new Vector2((float)Math.Floor(draw.position.X / 2), (float)Math.Floor(draw.position.Y / 2)) + (pos / 2);
                    if (!b) drawPosition = new Vector2((float)Math.Floor(draw.position.X), (float)Math.Floor(draw.position.Y)) + (pos);

                    if (draw.additive)
                    {
                        if (draw.setParams != null)
                            draw.setParams();

                        Main.spriteBatch.Draw(draw.sprite, drawPosition, draw.sourceRectangle, draw.color, draw.rotation, draw.origin, b ? draw.scale / 2 : draw.scale, draw.spriteEffects, 0);
                    }
                }

                foreach (DeferredPrim prim in Prims)
                {
                    if (prim.additive)
                    {
                        if (prim.setParams != null)
                            prim.setParams();

                        prim.Draw(b);
                    }
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                if (b)
                {
                    Main.graphics.GraphicsDevice.SetRenderTarget(Main.gameInactive ? null : Main.screenTarget);
                }

                Main.spriteBatch.End();

                Draws.Clear();
                Prims.Clear();
                //Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, null, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                */
        }

        public static void DrawPixelatedSprite(Texture2D sprite, Vector2 position, Rectangle sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffect = SpriteEffects.None, bool additive = false, Effect effect = null, Action setparams = null)
        {
            DeferredSprite draw = new DeferredSprite
            {
                sprite = sprite,
                position = position,
                sourceRectangle = sourceRectangle,
                color = color,
                origin = origin,
                rotation = rotation,
                scale = scale,
                spriteEffects = spriteEffect,
                shaderEffect = effect,
                additive = additive,
                setParams = setparams
            };

            Draws.Add(draw);
        }

        public static void DrawPixelatedSprite(DeferredSprite draw)
        {
            Draws.Add(draw);
        }

        public static DeferredSprite PixelatedSprite(Texture2D sprite, Vector2 position, Rectangle sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffect = SpriteEffects.None, bool additive = false, Effect effect = null, Action setparams = null)
        {
            DeferredSprite draw = new DeferredSprite
            {
                sprite = sprite,
                position = position,
                sourceRectangle = sourceRectangle,
                color = color,
                origin = origin,
                rotation = rotation,
                scale = scale,
                spriteEffects = spriteEffect,
                shaderEffect = effect,
                additive = additive,
                setParams = setparams
            };

            return draw;
        }

        public static void DrawDeferredPrims(List<Vector2> vertices, List<Color> colors, List<Vector2> texcoords, Texture2D sprite = null, bool additive = false, Effect effect = null, bool line = false, Action setparams = null)
        {
            DeferredPrim draw = new DeferredPrim
            {
                sprite = sprite,
                vertices = vertices,
                colors = colors,
                texcoords = texcoords,
                additive = additive,
                shaderEffect = effect,
                setParams = setparams
            };
            if (line) draw.primitiveType = PrimitiveType.LineStrip;
            else draw.primitiveType = PrimitiveType.TriangleStrip;

            Prims.Add(draw);
        }

        public static void DrawDeferredPrims(DeferredPrim prim)
        {
            Prims.Add(prim);
        }

        public static DeferredPrim DeferredPrim(List<Vector2> vertices, List<Color> colors, List<Vector2> texcoords, Texture2D sprite = null, bool additive = false, Effect effect = null, bool line = false, Action setparams = null)
        {
            DeferredPrim draw = new DeferredPrim
            {
                sprite = sprite,
                vertices = vertices,
                colors = colors,
                texcoords = texcoords,
                additive = additive,
                shaderEffect = effect,
                setParams = setparams
            };
            if (line) draw.primitiveType = PrimitiveType.LineStrip;
            else draw.primitiveType = PrimitiveType.TriangleStrip;

            return draw;
        }
    }

    public static class SpriteEffectRendering
    {
        public static void DrawAdditiveSprite(Texture2D sprite, Vector2 position, Rectangle sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffect = SpriteEffects.None, bool hasAlreadyBegun = true, Effect effect = null)
        {
            if (hasAlreadyBegun) Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, null, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(sprite, position, sourceRectangle, color, rotation, origin, scale, spriteEffect);

            Main.spriteBatch.End();

            if (hasAlreadyBegun) Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void DrawSprite(Texture2D sprite, Vector2 position, Rectangle sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffect = SpriteEffects.None, bool hasAlreadyBegun = true, Effect effect = null)
        {
            if (hasAlreadyBegun) Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(sprite, position, sourceRectangle, color, rotation, origin, scale, spriteEffect);

            Main.spriteBatch.End();

            if (hasAlreadyBegun) Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
