using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace Everware.Utils;

[Autoload]
public static class DrawingUtils
{
    public static void DrawSlopedTile(SpriteBatch sb, Asset<Texture2D> asset, int i, int j, Color color, Vector2 offset, float sc = 1f)
    {
        var hasTile = Main.tile[i, j].HasTile;
        var slope = Main.tile[i, j].Slope;
        var halfTile = Main.tile[i, j].IsHalfBlock;
        var invisible = Main.tile[i, j].IsTileInvisible;
        var fullBright = Main.tile[i, j].IsTileFullbright;

        var tileRenderer = Main.instance.TilesRenderer;

        var drawData = new TileDrawInfo
        {
            tileCache = Main.tile[i, j]
        };
        drawData.typeCache = drawData.tileCache.type;
        drawData.tileFrameX = drawData.tileCache.frameX;
        drawData.tileFrameY = drawData.tileCache.frameY;
        drawData.tileLight = Color.White;
        drawData.colorTint = Color.White;
        drawData.finalColor = TileDrawing.GetFinalLight(drawData.tileCache, drawData.typeCache, drawData.tileLight, drawData.colorTint);
        tileRenderer.GetTileDrawData(
            i,
            j,
            drawData.tileCache,
            drawData.typeCache,
            ref drawData.tileFrameX,
            ref drawData.tileFrameY,
            out drawData.tileWidth,
            out drawData.tileHeight,
            out drawData.tileTop,
            out drawData.halfBrickHeight,
            out drawData.addFrX,
            out drawData.addFrY,
            out drawData.tileSpriteEffect,
            out drawData.glowTexture,
            out drawData.glowSourceRect,
            out drawData.glowColor
        );
        drawData.drawTexture = tileRenderer.GetTileDrawTexture(drawData.tileCache, i, j);

        if (!hasTile || invisible)
        {
            return;
        }

        var position = new Vector2(i * 16f, j * 16f + drawData.tileTop) + offset - Main.screenPosition + (new Vector2(-8, -8) * sc);

        var source = new Rectangle(drawData.tileFrameX + drawData.addFrX, drawData.tileFrameY + drawData.addFrY, drawData.tileWidth, drawData.tileHeight);

        drawData.tileLight = Color.White;
        drawData.colorTint = Color.White;
        drawData.finalColor = TileDrawing.GetFinalLight(drawData.tileCache, drawData.typeCache, drawData.tileLight, drawData.colorTint);

        if (asset is null)
        {
            return;
        }

        if (slope == SlopeType.Solid && !halfTile)
        {
            sb.Draw(asset.Value, position, source, color, 0f, Vector2.Zero, sc, drawData.tileSpriteEffect, 0f);
        }
        else if (halfTile)
        {
            position = new Vector2(i * 16f, j * 16f + drawData.tileTop) + offset - Main.screenPosition + (new Vector2(-8, -4));
            sb.Draw(asset.Value, new Vector2(position.X, position.Y + 4), new Rectangle(source.X, source.Y, 16, 8), color);
        }
        else
        {
            if (slope is SlopeType.SlopeDownLeft or SlopeType.SlopeDownRight)
            {
                for (var a = 0; a < 16; a += 2)
                {
                    int length;
                    int height;

                    if (slope == SlopeType.SlopeDownRight)
                    {
                        length = 16 - a - 2;
                        height = 16 - a;
                    }
                    else
                    {
                        length = a;
                        height = 16 - length;
                    }

                    sb.Draw(asset.Value, position + new Vector2(length * sc, a), new Rectangle(source.X + length, source.Y, 2, height), color);
                }
            }
            else
            {
                for (var a = 0; a < 16; a += 2)
                {
                    int length;
                    int height;

                    if (slope == SlopeType.SlopeUpLeft)
                    {
                        length = a;
                        height = 16 - length;
                    }
                    else
                    {
                        length = 16 - a - 2;
                        height = 16 - a;
                    }

                    sb.Draw(asset.Value, position + new Vector2(length * sc, 0), new Rectangle(source.X + length, source.Y + 16 - height, 2, height), color);
                }
            }
        }
    }

    public static void DrawTile(SpriteBatch spriteBatch, Asset<Texture2D> asset, int i, int j)
    {
        if (Main.tile[i, j].Slope == Terraria.ID.SlopeType.Solid)
            spriteBatch.Draw(asset.Value, new Vector2(i * 16, j * 16) - Main.screenPosition + ((Main.tile[i, j].IsHalfBlock ? new Vector2(0, 10) : Vector2.Zero) + new Vector2(0, -2)),
            new Rectangle(Main.tile[i, j].TileFrameX, Main.tile[i, j].TileFrameY, 16, Main.tile[i, j].IsHalfBlock ? 8 : 16), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        else
        {
            Vector2 bounds = new Vector2(0, 16);

            for (int k = 0; k < 8; k++)
            {
                switch (Main.tile[i, j].Slope)
                {
                    case Terraria.ID.SlopeType.SlopeDownRight:
                        if (k == 0) bounds = new Vector2(14, 16);
                        else bounds.X -= 2;
                        break;
                    case Terraria.ID.SlopeType.SlopeUpRight:
                        if (k == 0) bounds = new Vector2(0, 2);
                        else bounds.Y += 2;
                        break;
                    case Terraria.ID.SlopeType.SlopeDownLeft:
                        if (k != 0) bounds.X += 2;
                        break;
                    case Terraria.ID.SlopeType.SlopeUpLeft:
                        if (k != 0) bounds.Y -= 2;
                        break;
                }

                spriteBatch.Draw(asset.Value, new Vector2(i * 16, j * 16) - Main.screenPosition + new Vector2(k * 2, bounds.X - 2),
                new Rectangle(Main.tile[i, j].TileFrameX + (k * 2), Main.tile[i, j].TileFrameY, 2, (int)bounds.Y), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
        }
    }
    public static void DrawTile(SpriteBatch spriteBatch, Asset<Texture2D> asset, int i, int j, Color c)
    {
        if (Main.tile[i, j].Slope == Terraria.ID.SlopeType.Solid)
            spriteBatch.Draw(asset.Value, new Vector2(i * 16, j * 16) - Main.screenPosition + (Main.tile[i, j].IsHalfBlock ? new Vector2(0, 8) : Vector2.Zero),
            new Rectangle(Main.tile[i, j].TileFrameX, Main.tile[i, j].TileFrameY, 16, Main.tile[i, j].IsHalfBlock ? 8 : 16), c, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        else
        {
            Vector2 bounds = new Vector2(0, 16);

            for (int k = 0; k < 8; k++)
            {
                switch (Main.tile[i, j].Slope)
                {
                    case Terraria.ID.SlopeType.SlopeDownRight:
                        if (k == 0) bounds = new Vector2(14, 16);
                        else bounds.X -= 2;
                        break;
                    case Terraria.ID.SlopeType.SlopeUpRight:
                        if (k == 0) bounds = new Vector2(0, 2);
                        else bounds.Y += 2;
                        break;
                    case Terraria.ID.SlopeType.SlopeDownLeft:
                        if (k != 0) bounds.X += 2;
                        break;
                    case Terraria.ID.SlopeType.SlopeUpLeft:
                        if (k != 0) bounds.Y -= 2;
                        break;
                }

                spriteBatch.Draw(asset.Value, new Vector2(i * 16, j * 16) - Main.screenPosition + new Vector2(k * 2, bounds.X),
                new Rectangle(Main.tile[i, j].TileFrameX + (k * 2), Main.tile[i, j].TileFrameY, 2, (int)bounds.Y), c, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
        }
    }
    public static void EverEntitySpriteDraw(Asset<Texture2D> texture, Vector2 position, Vector2 origin, float rotation = 0f, int framesV = 1, int frameNumV = 0)
    {
        Main.EntitySpriteDraw(texture.Value, position, texture.Frame(1, framesV, 0, frameNumV), Color.White, rotation, origin, Vector2.One, 0);
    }
    public static void EverEntitySpriteDraw(Asset<Texture2D> texture, Vector2 position, Vector2 origin, Vector2 scale, float rotation = 0f, int framesV = 1, int frameNumV = 0)
    {
        Main.EntitySpriteDraw(texture.Value, position, texture.Frame(1, framesV, 0, frameNumV), Color.White, rotation, origin, scale, 0);
    }
    public static void EverEntitySpriteDraw(Asset<Texture2D> texture, Vector2 position, Vector2 origin, Color color, float rotation = 0f, int framesV = 1, int frameNumV = 0)
    {
        Main.EntitySpriteDraw(texture.Value, position, texture.Frame(1, framesV, 0, frameNumV), color, rotation, origin, Vector2.One, 0);
    }
    public static void EverEntitySpriteDraw(Asset<Texture2D> texture, Vector2 position, Vector2 origin, Vector2 scale, Color color, float rotation = 0f, int framesV = 1, int frameNumV = 0)
    {
        Main.EntitySpriteDraw(texture.Value, position, texture.Frame(1, framesV, 0, frameNumV), color, rotation, origin, Vector2.One, 0);
    }
    public static void DrawGlowWithPadding(Texture2D texture, Vector2 position, Rectangle frame, Color c, float rotation, Vector2 origin, Vector2 scale, SpriteEffects eff = SpriteEffects.None, float radius = 0.15f)
    {
        int padding = 60;

        Rectangle fr = frame;
        fr.Width += padding;
        fr.Height += padding;

        var glowTarget = ScreenspaceTargetPool.Shared.Rent(
            Main.instance.GraphicsDevice,
            (Width, Height) => (fr.Width, fr.Height)
        );

        Main.spriteBatch.End(out var sb);
        using (glowTarget.Scope(clearColor: Color.Transparent))
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.EffectMatrix);
            Main.spriteBatch.Draw(texture, new Vector2(padding / 2, padding / 2), frame, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
        }
        Main.spriteBatch.Begin(sb);

        var GlowEffect = Assets.Effects.Misc.GlowBlur.CreateEffect();
        GlowEffect.Parameters.Color = c.ToVector4();
        GlowEffect.Parameters.Radius = radius;
        GlowEffect.Apply();

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, null, null, GlowEffect.Shader, Main.GameViewMatrix.TransformationMatrix);

        Main.EntitySpriteDraw(glowTarget.Target, position, glowTarget.Target.Bounds, Color.White, rotation, origin + new Vector2(padding / 2, padding / 2), scale, eff);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(sb);

        glowTarget.Dispose();
    }
    public static Vector2 TileOffset()
    {
        return Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
    }
    public static Vector2 PlayerOffset(Player player)
    {
        return new Vector2(0f, player.gfxOffY);
    }
    public static void DrawTrailBehind(Projectile projectile, Color color1, Color color2, Vector2 offset, bool scaleDown = false, Asset<Texture2D> customTexture = null, float customScale = 1f)
    {
        Asset<Texture2D> tex;

        if (projectile.ModProjectile != null)
        {
            tex = ModContent.Request<Texture2D>(projectile.ModProjectile.Texture);
        }
        else
        {
            tex = TextureAssets.Projectile[projectile.type];
        }

        if (customTexture != null)
        {
            tex = customTexture;
        }

        for (var i = 0; i < projectile.oldPos.Length; i++)
        {
            var sc = 1f;

            if (scaleDown)
            {
                sc = MathHelper.Lerp(sc, 0f, (float)i / projectile.oldPos.Length);
            }

            Main.EntitySpriteDraw(
                tex.Value,
                projectile.oldPos[i] + projectile.Size / 2 - Main.screenPosition + offset,
                tex.Frame(),
                Color.Lerp(color1, color2, i / (float)projectile.oldPos.Length),
                projectile.oldRot[i] != 0 ? projectile.oldRot[i] : projectile.rotation,
                tex.Size() / 2,
                sc * customScale,
                SpriteEffects.None
            );
        }
    }
    public static void DrawTrailBehind(Projectile projectile, Color color1, Color color2, bool scaleDown = false)
    {
        Asset<Texture2D> tex;

        if (projectile.ModProjectile != null)
        {
            tex = ModContent.Request<Texture2D>(projectile.ModProjectile.Texture);
        }
        else
        {
            tex = TextureAssets.Projectile[projectile.type];
        }

        for (var i = 0; i < projectile.oldPos.Length; i++)
        {
            var sc = 1f;

            if (scaleDown)
            {
                sc = MathHelper.Lerp(sc, 0f, (float)i / projectile.oldPos.Length);
            }

            Main.EntitySpriteDraw(
                tex.Value,
                projectile.oldPos[i] + projectile.Size / 2 - Main.screenPosition,
                tex.Frame(),
                Color.Lerp(color1, color2, i / (float)projectile.oldPos.Length),
                projectile.oldRot[i] != 0 ? projectile.oldRot[i] : projectile.rotation,
                projectile.Size / 2,
                sc,
                SpriteEffects.None
            );
        }
    }

    public static void DrawGlowBehind(Projectile projectile, Color color, Vector2 offset, float width = 2)
    {
        Asset<Texture2D> tex;

        if (projectile.ModProjectile != null)
        {
            tex = ModContent.Request<Texture2D>(projectile.ModProjectile.Texture);
        }
        else
        {
            tex = TextureAssets.Projectile[projectile.type];
        }

        for (var i = 0; i < 360; i += 90)
        {
            Main.EntitySpriteDraw(
                tex.Value,
                projectile.Center + new Vector2(width, 0).RotatedBy(MathHelper.ToRadians(i)) - Main.screenPosition + offset,
                tex.Frame(),
                color,
                projectile.rotation,
                projectile.Size / 2,
                projectile.scale,
                SpriteEffects.None
            );
        }
    }

    public static void DrawGlowBehind(Projectile projectile, Color color, Vector2 offset, SpriteEffects eff, float width = 2)
    {
        Asset<Texture2D> tex;

        if (projectile.ModProjectile != null)
        {
            tex = ModContent.Request<Texture2D>(projectile.ModProjectile.Texture);
        }
        else
        {
            tex = TextureAssets.Projectile[projectile.type];
        }

        for (var i = 0; i < 360; i += 90)
        {
            Main.EntitySpriteDraw(
                tex.Value,
                projectile.Center + new Vector2(width, 0).RotatedBy(MathHelper.ToRadians(i)) - Main.screenPosition + offset,
                tex.Frame(),
                color,
                projectile.rotation,
                projectile.Size / 2,
                projectile.scale,
                eff
            );
        }
    }

    public static void DrawGlowBehind(Projectile projectile, Color color, Vector2 offset, SpriteEffects eff, float width = 2, Rectangle? frame = null, Asset<Texture2D> overrideTex = null)
    {
        Asset<Texture2D> tex;

        if (projectile.ModProjectile != null)
        {
            tex = ModContent.Request<Texture2D>(projectile.ModProjectile.Texture);
        }
        else
        {
            tex = TextureAssets.Projectile[projectile.type];
        }

        if (overrideTex != null)
            tex = overrideTex;

        for (var i = 0; i < 360; i += 90)
        {
            Main.EntitySpriteDraw(
                tex.Value,
                projectile.Center + new Vector2(width, 0).RotatedBy(MathHelper.ToRadians(i)) - Main.screenPosition + offset,
                frame != null ? frame.Value : tex.Frame(),
                color,
                projectile.rotation,
                projectile.Size / 2,
                projectile.scale,
                eff
            );
        }
    }
}
