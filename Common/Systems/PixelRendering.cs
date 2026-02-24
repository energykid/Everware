using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using Terraria.Graphics.Light;

namespace Everware.Common.Systems
{
    public struct PixelatedDraw
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
    public struct PixelatedPrim
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
        }
    }

    public class PixelRendering : ModSystem
    {
        public static RenderTarget2D PixelatedRenderTarget;
        public static RenderTarget2D AdditivePixelatedRenderTarget;
        public static List<PixelatedDraw> Draws = [];
        public static List<PixelatedPrim> Prims = [];
        public override void Load()
        {
            IL_Main.DoDraw += il =>
            {
                var c = new ILCursor(il);

                if (!c.TryGotoNext(MoveType.After, i => i.MatchCall(typeof(TimeLogger), "DetailedDrawReset"), i => i.MatchLdsfld(typeof(Main), "gameMenu"), i => i.MatchBrtrue(out _)))
                {
                    return;
                }

                c.Emit(OpCodes.Call, typeof(PixelRendering).GetMethod("DrawAllPixelatedSprites"));
            };
        }

        public void DrawPixelationTargets()
        {
            Vector2 targetPosition = new Vector2(2 - (float)Math.Floor((Main.LocalPlayer.Center.X % 2) / 2), 2 - (float)Math.Floor((Main.LocalPlayer.Center.Y % 2) / 2));

            if (!Main.gameMenu)
            {
                if (PixelatedRenderTarget != null)
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);
                    Main.EntitySpriteDraw(PixelatedRenderTarget, targetPosition, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight / 2), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0);
                    Main.spriteBatch.End();
                    PixelatedRenderTarget.Dispose();
                }
                if (AdditivePixelatedRenderTarget != null)
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);
                    Main.EntitySpriteDraw(AdditivePixelatedRenderTarget, targetPosition, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight / 2), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0);
                    Main.spriteBatch.End();
                    AdditivePixelatedRenderTarget.Dispose();
                }
            }
            if (PixelatedRenderTarget != null)
            {
                PixelatedRenderTarget = null;
            }
            if (AdditivePixelatedRenderTarget != null)
            {
                AdditivePixelatedRenderTarget = null;
            }
        }

        public override void PostDrawTiles()
        {
            DrawPixelationTargets();
        }

        public static void DrawAllPixelatedSprites()
        {
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

            foreach (PixelatedDraw draw in Draws)
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

            foreach (PixelatedPrim prim in Prims)
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

            foreach (PixelatedDraw draw in Draws)
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

            foreach (PixelatedPrim prim in Prims)
            {
                if (prim.additive)
                {
                    if (prim.setParams != null)
                        prim.setParams();

                    prim.Draw(b);
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

            if (b)
            {
                Main.graphics.GraphicsDevice.SetRenderTarget(Main.gameInactive ? null : Main.screenTarget);
            }

            Main.spriteBatch.End();

            Draws.Clear();
            Prims.Clear();
            //Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, null, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
        }

        public static void DrawPixelatedSprite(Texture2D sprite, Vector2 position, Rectangle sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffect = SpriteEffects.None, bool additive = false, Effect effect = null, Action setparams = null)
        {
            PixelatedDraw draw = new PixelatedDraw
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

        public static void DrawPixelatedSprite(PixelatedDraw draw)
        {
            Draws.Add(draw);
        }

        public static PixelatedDraw PixelatedSprite(Texture2D sprite, Vector2 position, Rectangle sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffect = SpriteEffects.None, bool additive = false, Effect effect = null, Action setparams = null)
        {
            PixelatedDraw draw = new PixelatedDraw
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

        public static void DrawPixelatedPrims(List<Vector2> vertices, List<Color> colors, List<Vector2> texcoords, Texture2D sprite = null, bool additive = false, Effect effect = null, bool line = false, Action setparams = null)
        {
            PixelatedPrim draw = new PixelatedPrim
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

        public static void DrawPixelatedPrims(PixelatedPrim prim)
        {
            Prims.Add(prim);
        }

        public static PixelatedPrim PixelatedPrim(List<Vector2> vertices, List<Color> colors, List<Vector2> texcoords, Texture2D sprite = null, bool additive = false, Effect effect = null, bool line = false, Action setparams = null)
        {
            PixelatedPrim draw = new PixelatedPrim
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
