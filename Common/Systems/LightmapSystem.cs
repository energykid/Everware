using Terraria.ID;

namespace Everware.Common.Systems;

public class LightmapSystem : ModSystem
{
    public static RenderTargetLease RawLightmap;
    public static RenderTargetLease ScreenLightmap;

    public static readonly int Padding = 16;

    public override void Load()
    {
        if (Main.netMode != NetmodeID.Server)
            ThreadUtils.RunOnMainThread(() =>
            {
                RawLightmap = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, (w, h, offW, offH) => ((offW / 16) + Padding, (offH / 16) + Padding));
                ScreenLightmap = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, (w, h, offW, offH) => (offW, offH));
            });
    }

    public override void Unload()
    {
        if (Main.netMode != NetmodeID.Server)
            ThreadUtils.RunOnMainThread(() =>
            {
                RawLightmap.Dispose();
                ScreenLightmap.Dispose();
            });
    }

    public override void PostDrawTiles()
    {
        using (RawLightmap.Scope(preserveContents: true, clearColor: Color.White))
        {
            var pixel = Assets.Textures.Misc.SinglePixel.Asset;
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            for (int i = 0; i < RawLightmap.Target.Width; i++)
            {
                for (int j = 0; j < RawLightmap.Target.Height; j++)
                {
                    Color c = Lighting.GetColor(
                    (int)(Main.screenPosition.X / 16) - (Padding / 2) + i,
                    (int)(Main.screenPosition.Y / 16) - (Padding / 2) + j
                    );

                    Main.EntitySpriteDraw(pixel.Value, new Vector2(i, j), pixel.Frame(), c, 0, Vector2.Zero, 1f, SpriteEffects.None);
                }
            }

            Main.spriteBatch.End();
        }

        using (ScreenLightmap.Scope(preserveContents: true, clearColor: Color.White))
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            Vector2 drawPosition = new Vector2((Padding / 2) * -16);
            drawPosition += new Vector2(-Main.screenPosition.X % 16f, -Main.screenPosition.Y % 16f);

            Main.EntitySpriteDraw(RawLightmap.Target, drawPosition, RawLightmap.Target.Bounds, Color.White, 0f, Vector2.Zero, 16f, SpriteEffects.None);

            Main.spriteBatch.End();
        }
    }
}
