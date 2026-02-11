using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.Graphics;
using Terraria.ID;

namespace Everware.Common.Systems;

public class ScreenShakeMultiplier
{
    public Vector2 position = Vector2.Zero;
    public float strength = 0f;
    public float strengthIncrement = 0.95f;
    public float distance = 1920f;
    public ScreenShakeMultiplier(Vector2 pos, float str, float inc = 0.9f, float dist = 1920f)
    {
        position = pos;
        strength = str;
        strengthIncrement = inc;
        distance = dist;
    }
}
public class ScreenEffects : ModSystem
{
    public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
    {
        if (Main.LocalPlayer != null)
        {
            if (Main.LocalPlayer.TryGetModPlayer<ScreenEffectsPlayer>(out ScreenEffectsPlayer result))
            {
                Transform.Zoom *= 1f + result.screenZoom;
            }
        }
    }
    public static List<ScreenShakeMultiplier> screenShakes = [];
    public override void ModifyScreenPosition()
    {

    }
    public override void Load()
    {
        On_Main.DrawNPCs += DimLights;
    }

    public override void Unload()
    {
        On_Main.DrawNPCs -= DimLights;
    }

    private void DimLights(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        float dim = Main.LocalPlayer.GetModPlayer<ScreenEffectsPlayer>().screenDim;
        Main.EntitySpriteDraw(ScreenDarkeningTexture.Value, Vector2.Zero, ScreenDarkeningTexture.Frame(), Color.White.MultiplyRGBA(new(dim, dim, dim, dim)), 0f, Vector2.Zero, 100f, SpriteEffects.None);
        orig(self, behindTiles);
    }
    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        base.ModifySunLightColor(ref tileColor, ref backgroundColor);
    }
    public static void PanTo(Vector2 position)
    {
        Main.LocalPlayer.GetModPlayer<ScreenEffectsPlayer>().screenPosition = position;
        Main.LocalPlayer.GetModPlayer<ScreenEffectsPlayer>().shouldScreenPositionDecrement = false;
        Main.LocalPlayer.GetModPlayer<ScreenEffectsPlayer>().screenPositionIncrement = MathHelper.Lerp(Main.LocalPlayer.GetModPlayer<ScreenEffectsPlayer>().screenPositionIncrement, 1f, 0.1f);
    }
    public static void DimScreen(float dimTarget)
    {
        Main.LocalPlayer.GetModPlayer<ScreenEffectsPlayer>().screenDim = MathHelper.Lerp(Main.LocalPlayer.GetModPlayer<ScreenEffectsPlayer>().screenDim, dimTarget, 0.1f);
    }
    public static void ZoomScreen(float zoomFactor)
    {
        if (!Main.dedServ)
            Main.LocalPlayer.GetModPlayer<ScreenEffectsPlayer>().screenZoom = MathHelper.Lerp(Main.LocalPlayer.GetModPlayer<ScreenEffectsPlayer>().screenZoom, zoomFactor, 0.06f);
    }
    public static void AddScreenShake(Vector2 position, float strength)
    {
        screenShakes.Add(new ScreenShakeMultiplier(position, strength));
    }
    public static void AddScreenShake(Vector2 position, float strength, float multiplier)
    {
        screenShakes.Add(new ScreenShakeMultiplier(position, strength, multiplier));
    }
    public static Asset<Texture2D> ScreenDarkeningTexture => ModContent.Request<Texture2D>("Everware/Textures/ScreenDarkening");
    public override void PostDrawTiles()
    {
    }
}
public class ScreenEffectsPlayer : ModPlayer
{
    public float screenTime = 0f;
    public float screenDim = 0f;
    public float screenShake = 0f;
    public float screenZoom = 0f;
    public Vector2 screenOffset = Vector2.Zero;
    public float screenPositionIncrement = 0f;
    public bool shouldScreenPositionDecrement = false;
    public Vector2 screenPosition = Vector2.Zero;
    public override void ModifyZoom(ref float zoom)
    {
        screenZoom = MathHelper.Lerp(screenZoom, 0f, 0.1f);
    }
    public override void ModifyScreenPosition()
    {
        screenDim = MathHelper.Lerp(screenDim, 0f, 0.1f);
        if (shouldScreenPositionDecrement) screenPositionIncrement = MathHelper.Lerp(screenPositionIncrement, 0f, 0.03f);
        shouldScreenPositionDecrement = true;

        screenTime += 1 + (screenShake);

        for (int i = 0; i < ScreenEffects.screenShakes.Count; i++)
        {
            ScreenShakeMultiplier screenShakeMultiplier = ScreenEffects.screenShakes[i];

            if (Player.Distance(screenShakeMultiplier.position) < screenShakeMultiplier.distance)
            {
                float a = Player.Distance(screenShakeMultiplier.position) / screenShakeMultiplier.distance;
                float l = MathHelper.Lerp(1, 0, a);

                Player.GetModPlayer<ScreenEffectsPlayer>().screenShake = l * screenShakeMultiplier.strength;
            }

            screenShakeMultiplier.strength *= screenShakeMultiplier.strengthIncrement;
            screenShakeMultiplier.strength -= 0.07f;
            if (screenShakeMultiplier.strength <= 0f) ScreenEffects.screenShakes.Remove(screenShakeMultiplier);
        }
        if (Main.netMode == NetmodeID.MultiplayerClient || Main.netMode == NetmodeID.SinglePlayer)
        {
            float shake = screenShake * 0.5f;
            Vector2 off = screenOffset;
            Vector2 pos = screenPosition - (Main.ScreenSize.ToVector2() * 0.5f);
            float posInc = screenPositionIncrement;

            Main.screenPosition = Vector2.Lerp(Main.screenPosition, pos, posInc);
            Main.screenPosition += new Vector2((float)Math.Sin(screenTime) * shake, (float)Math.Sin(screenTime / 1.56f) * shake);
            Main.screenPosition += off;
        }
    }

    public override void ResetEffects()
    {
        screenShake = 0f;
    }
}
