using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Everware.Utils;

public static class DrawingUtils
{
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
