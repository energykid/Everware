using Everware.Common.Systems;
using Everware.Utils;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria.ID;

namespace Everware.Content.Base.Tiles;

public abstract class EverTile : ModTile
{
    #region Paint
    private readonly Dictionary<int, EverTileRenderTargetHolder> paintCache = [];

    internal class EverTileRenderTargetHolder(int paintColor, Asset<Texture2D> asset, int copySettingsFrom = -1) : TilePaintSystemV2.ARenderTargetHolder
    {
        public int PaintColor = paintColor;

        public TreePaintingSettings PaintSettings = TreePaintSystemData.GetTileSettings(copySettingsFrom, 0);

        public Asset<Texture2D> Texture = asset;

        public override void Prepare()
        {
            Texture.Wait();

            PrepareTextureIfNecessary(Texture.Value);
        }

        public override void PrepareShader()
        {
            PrepareShader(PaintColor, PaintSettings);
        }
    }

    public bool TryGetPaintTexture(
        int paintColor,
        Asset<Texture2D> asset,
        [NotNullWhen(true)] out Texture2D? texture
    )
    {
        texture = null;

        if (paintCache.TryGetValue(paintColor, out EverTileRenderTargetHolder? holder) &&
            holder.IsReady)
        {
            texture = holder.Target;

            return true;
        }

        var newHolder = new EverTileRenderTargetHolder(paintColor, asset);

        paintCache[paintColor] = newHolder;

        Main.instance.TilePaintSystem._requests.Add(newHolder);

        return false;
    }
    #endregion

    /// <summary>
    /// For making sure glowcoats don't appear invisible when using dummy tile textures.
    /// Set to a value if you want glowcoats to draw a different texture for the outline.
    /// </summary>
    public virtual string GlowcoatTileTexture => "";
    public static RenderTargetLease ExtraTarget;

    public override void Load()
    {
        if (Main.netMode != NetmodeID.Server)
            Asset = ModContent.Request<Texture2D>(Texture);

        ThreadUtils.RunOnMainThread(() =>
        {
            if (Main.netMode != NetmodeID.Server)
                ExtraTarget = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, (w, h, offW, offH) => (offW, offH));
        });

        On_Main.DoDraw_DrawNPCsOverTiles += DrawExtraTarget;
    }

    public override void Unload()
    {
        ThreadUtils.RunOnMainThread(() =>
        {
            if (Main.netMode != NetmodeID.Server)
                ExtraTarget.Dispose();
        });

        On_Main.DoDraw_DrawNPCsOverTiles -= DrawExtraTarget;
    }

    public Asset<Texture2D> Asset;
    public virtual bool UsesExtraTarget => false;

    public override void SetStaticDefaults()
    {
        Main.tileBlendAll[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
    }

    public static Vector2 MoreDrawOffset => Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);

    /// <summary>
    /// Calls once per tile of this type when UsesExtraDrawing is enabled. 
    /// Draws to ExtraTarget, a RenderTargetLease.
    /// Override this to draw to ExtraTarget differently.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    public virtual void ExtraDrawSingleTile(int i, int j)
    {
        DrawingUtils.DrawSlopedTile(Main.spriteBatch, Asset, i, j, Color.White, new Vector2(16) - ScreenOffset);
    }

    public virtual void ExtraDrawPreEverything()
    {

    }
    /// <summary>
    /// Calls once per type when UsesExtraDrawing is enabled.
    /// Used to draw full-screen shaders on tiles.
    /// Override this to draw ExtraTarget.Target differently.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="spriteBatch"></param>
    public virtual void ExtraDrawEverything()
    {
        Main.spriteBatch.Draw(ExtraTarget.Target, ScreenOffset, ExtraTarget.Target.Bounds, Color.White, 0f, Vector2.Zero, 1f, Main.GameViewMatrix.Effects, 0f);
    }

    public Vector2 RoundedScreenPosition;
    public Vector2 ScreenOffset;

    private void DrawExtraTarget(On_Main.orig_DoDraw_DrawNPCsOverTiles orig, Main self)
    {
        if (UsesExtraTarget)
        {
            RoundedScreenPosition = Main.screenPosition;

            if (RoundedScreenPosition.X % 2 >= 1) RoundedScreenPosition.X -= 1;
            if (RoundedScreenPosition.Y % 2 >= 1) RoundedScreenPosition.Y -= 1;

            ScreenOffset = RoundedScreenPosition - Main.screenPosition;

            using (ExtraTarget.Scope(clearColor: Color.Transparent))
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);
                for (int i = -16; i < (Main.screenWidth / 16) + 16; i++)
                {
                    for (int j = -16; j < (Main.screenHeight / 16) + 16; j++)
                    {
                        Point topLeft = (Main.screenPosition / 16).ToPoint();

                        Point a = new Point(Math.Clamp(topLeft.X + i, 0, Main.maxTilesX), Math.Clamp(topLeft.Y + j, 0, Main.maxTilesY));

                        if (Main.tile[a].TileType == Type)
                        {
                            ExtraDrawSingleTile(a.X, a.Y);
                        }
                    }
                }
                ExtraDrawPreEverything();
                Main.spriteBatch.End();
            }

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            ExtraDrawEverything();
            Main.spriteBatch.End();
        }

        orig(self);
    }
}
