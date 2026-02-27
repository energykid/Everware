using Everware.Content.Base;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;

namespace Everware.Content.Hell;

public class HellPodGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;
    public bool ShouldHover = false;
    public float SizeTimer = 0;
    public bool Rare = false;
    float maxFall = 0f;
    float grav = 0f;
    bool start = false;

    public static Vector2 GroundedOrLavad(Vector2 baseVec)
    {
        if (BehaviorUtils.SolidTilePlatformOrLiquid((int)baseVec.X / 16, (int)baseVec.Y / 16))
        {
            for (int i = 0; i < 250; i++)
            {
                baseVec.Y -= 2;
                if (!BehaviorUtils.SolidTilePlatformOrLiquid((int)baseVec.X / 16, (int)baseVec.Y / 16)) break;
            }
            return baseVec;
        }
        for (int i = 0; i < 250; i++)
        {
            baseVec.Y += 2;
            if (BehaviorUtils.SolidTilePlatformOrLiquid((int)baseVec.X / 16, (int)baseVec.Y / 16)) break;
        }
        return baseVec;
    }
    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        if (ShouldHover)
        {
            SizeTimer -= 0.1f;
            SizeTimer *= 0.9f;
        }

        if (ShouldHover && SizeTimer > 0)
        {
            gravity = 0f;
            maxFallSpeed = 0f;
        }
        else if (ShouldHover && GroundedOrLavad(item.position).Y < (item.position + new Vector2(0, 120)).Y)
        {
            if (!start)
            {
                grav = gravity;
                maxFall = maxFallSpeed;
                start = true;
            }
            else
            {
                grav *= 0.6f;
                maxFall *= 0.6f;
                maxFall = Math.Min(maxFall, 5);
            }

            gravity = grav;
            maxFallSpeed = maxFall;

            item.position.Y += (float)Math.Sin((GlobalTimer.Value + (item.whoAmI * 7)) / 20f) * 0.1f;
        }

        base.Update(item, ref gravity, ref maxFallSpeed);
    }

    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Asset<Texture2D> fr = TextureAssets.Item[item.type];

        if (ShouldHover)
        {
            if (SizeTimer > 0)
                rotation = (float)Math.Sin(SizeTimer / 3f) * (SizeTimer / 60f);

            scale = Easing.KeyFloat(SizeTimer, 0, 30, 1f, 2f, Easing.InCubic, 1f);
            scale = Easing.KeyFloat(SizeTimer, 30, 60, 2f, 0f, Easing.OutCubic, scale);

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(fr.Value,
                    item.Center + new Vector2(Rare ? 4 : 2, 0).RotatedBy(i * MathHelper.PiOver2) + new Vector2(0, -1) - Main.screenPosition,
                    fr.Frame(), Color.White.MultiplyRGBA(new Color(1f, 0.75f, 0.2f, 0f)), rotation, fr.Size() / 2f, scale, SpriteEffects.None);
            }
        }

        if (Rare)
        {
            lightColor = Color.White;
        }

        return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
    }
}