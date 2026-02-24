using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Everware.Common.Systems
{
    public static class VertexBufferExtensions
    {
        public static int GetPrimitiveCount(this VertexBuffer buffer, PrimitiveType type)
        {
            return type switch
            {
                PrimitiveType.TriangleList => buffer.VertexCount / 2,
                PrimitiveType.TriangleStrip => buffer.VertexCount - 2,
                PrimitiveType.LineList => buffer.VertexCount / 2,
                PrimitiveType.LineStrip => buffer.VertexCount - 1,
                PrimitiveType.PointListEXT => buffer.VertexCount / 3,
                _ => 0 // throw new ArgumentException($"Unsupported primitive type: {type}", nameof(type))
            };
        }
    }

    public static class ThreadUtils
    {
        public static bool IsMainThread => AssetRepository.IsMainThread;

        public static void RunOnMainThread(Action action, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsMainThread)
            {
                action();
                return;
            }

            ManualResetEventSlim manualResetEvent = new(false);
            Exception error = null;

            Main.QueueMainThreadAction(
                () =>
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            action();
                    }
                    catch (Exception exception)
                    {
                        error = exception;
                    }
                    finally
                    {
                        manualResetEvent.Set();
                    }
                }
            );

            manualResetEvent.Wait(cancellationToken);

            if (error != null)
                throw new AggregateException(error);
        }
    }


    [Autoload(Side = ModSide.Client)]
    public sealed class PrimitiveDrawing : ILoadable
    {
        public static BasicEffect effect;

        private static GraphicsDevice Device => Main.graphics.GraphicsDevice;

        private static DynamicIndexBuffer indexBuffer;
        private static DynamicVertexBuffer vertexBuffer;

        void ILoadable.Load(Mod mod)
        {
        }

        void ILoadable.Unload()
        {
            ThreadUtils.RunOnMainThread(
                () =>
                {
                    indexBuffer?.Dispose();
                    indexBuffer = null;

                    vertexBuffer?.Dispose();
                    vertexBuffer = null;
                }
            );
        }

        public static void DrawPrimitiveStrip(List<Vector2> vertices, List<Color> colors, Texture2D sprite = null, List<Vector2> texcoords = null, bool add = false, Effect eff = null)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, eff, Main.GameViewMatrix.TransformationMatrix);

            List<VertexPositionColorTexture> vv = [];
            short[] ii = new short[vertices.Count + 4];

            for (int i = 0; i <= vertices.Count + 2; i++)
            {
                Vector2 t = Vector2.Zero;
                if (i < vertices.Count)
                {
                    if (texcoords != null)
                    {
                        t = texcoords[i];
                    }

                    vv.Add(new VertexPositionColorTexture(new Vector3(vertices[i], 0), colors[i], t));
                    ii[i] = (short)(i);
                }
                else
                {
                    vv.Add(new VertexPositionColorTexture(new Vector3(vertices[vertices.Count - 1], 0), Color.Transparent, t));
                    ii[i] = (short)(i);
                }
            }

            if (effect == null)
                effect = new BasicEffect(Main.graphics.GraphicsDevice);

            if (eff != null)
            {
                foreach (EffectPass pass in eff.CurrentTechnique.Passes)
                {
                    //effect.CurrentTechnique.Passes.Append(pass);
                }
            }

            effect.World = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f);
            effect.View = Main.GameViewMatrix.TransformationMatrix;
            effect.Projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            if (sprite != null)
            {
                effect.Texture = sprite;
                effect.TextureEnabled = true;
            }
            effect.VertexColorEnabled = true;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, add ? BlendState.Additive : BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, eff, Main.GameViewMatrix.TransformationMatrix);

            DrawPrimitive(PrimitiveType.TriangleStrip, vv.ToArray(), ii, effect);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);

            //effect.Dispose();
        }

        public static void DrawPrimitiveLine(List<Vector2> vertices, List<Color> colors, Texture2D sprite = null, List<Vector2> texcoords = null, bool add = false, Effect eff = null)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, eff, Main.GameViewMatrix.TransformationMatrix);

            List<VertexPositionColorTexture> vv = [];
            short[] ii = new short[vertices.Count + 4];

            for (int i = 0; i <= vertices.Count + 2; i++)
            {
                Vector2 t = Vector2.Zero;
                if (i < vertices.Count)
                {
                    if (texcoords != null)
                    {
                        t = texcoords[i];
                    }

                    vv.Add(new VertexPositionColorTexture(new Vector3(vertices[i], 0), colors[i], t));
                    ii[i] = (short)(i);
                }
                else
                {
                    vv.Add(new VertexPositionColorTexture(new Vector3(vertices[vertices.Count - 1], 0), Color.Transparent, t));
                    ii[i] = (short)(i);
                }
            }

            if (effect == null)
                effect = new BasicEffect(Main.graphics.GraphicsDevice);

            if (eff != null)
            {
                foreach (EffectPass pass in eff.CurrentTechnique.Passes)
                {
                    //effect.CurrentTechnique.Passes.Append(pass);
                }
            }

            effect.World = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f);
            effect.View = Main.GameViewMatrix.TransformationMatrix;
            effect.Projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            if (sprite != null)
            {
                effect.Texture = sprite;
                effect.TextureEnabled = true;
            }
            effect.VertexColorEnabled = true;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, add ? BlendState.Additive : BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, eff, Main.GameViewMatrix.TransformationMatrix);

            DrawPrimitive(PrimitiveType.LineStrip, vv.ToArray(), ii, effect);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);

            //effect.Dispose();
        }

        public static void DrawPrimitive(PrimitiveType type, VertexPositionColorTexture[] vertices, short[] indices, Effect effect)
        {
            if (vertices.Length <= 0 || indices.Length <= 0 || effect == null)
                return;

            /*

            if (vertexBuffer == null || vertexBuffer.VertexCount < vertices.Length)
            {
                vertexBuffer?.Dispose();
                vertexBuffer = new DynamicVertexBuffer(Device, VertexPositionColorTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            }

            if (indexBuffer == null || indexBuffer.IndexCount < indices.Length)
            {
                indexBuffer?.Dispose();
                indexBuffer = new DynamicIndexBuffer(Device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            }

            vertexBuffer.SetData(vertices, SetDataOptions.Discard);
            indexBuffer.SetData(indices, 0, indices.Length, SetDataOptions.Discard);

            Device.SetVertexBuffer(vertexBuffer);
            Device.Indices = indexBuffer;*/

            Device.RasterizerState = RasterizerState.CullNone;

            int primitiveCount = indices.Length;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.DrawUserIndexedPrimitives(type, vertices, 0, vertices.Length, indices, 0, primitiveCount);
            }
        }
    }
}