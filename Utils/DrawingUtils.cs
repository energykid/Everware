namespace Everware.Utils;

[Autoload]
public static class DrawingUtils
{
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
