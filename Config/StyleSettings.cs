using ReLogic.Content;
using System;
using Terraria.ID;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Everware.Config;

public class StyleSettings : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;
    public override string LocalizationCategory => "ConfigKeys";

    // Boss Style
    [BackgroundColor(98, 155, 255)]
    [CustomModConfigItem(typeof(StyleElement))]
    [ReloadRequired]
    public bool eocEnabled = true;
    public static bool EoCEnabled = true;

    public StyleSettings()
    {
        eocEnabled = false;
        EoCEnabled = false;
    }

    public override void OnChanged()
    {
        EoCEnabled = eocEnabled;
    }

    public override void OnLoaded()
    {
        EoCEnabled = eocEnabled = true;
    }
}

class StyleElement : ConfigElement<bool>
{
    float UITimer = 0f;
    float Sq = 0f;

    float BlueberryUIThing = 0f;

    public override void OnBind()
    {
        base.OnBind();
        OnLeftClick += delegate
        {
            BlueberryUIThing = 1f;
            SoundEngine.PlaySound(SoundID.MenuTick.WithPitchOffset(-0.3f));
            Value = !Value;
        };
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        UITimer++;
        BlueberryUIThing = MathHelper.Lerp(BlueberryUIThing, 0f, 0.1f);

        base.Draw(spriteBatch);

        Asset<Texture2D> tex = Value ? Assets.Textures.Misc.EverwareIcon.Asset : Assets.Textures.Misc.VanillaIcon.Asset;

        float x = 60f;

        CalculatedStyle dimensions = GetDimensions();
        float b = MathHelper.Lerp(1f, 0f, BlueberryUIThing);

        string EVWString = Everware.Instance.DisplayNameClean;

        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, Value ? EVWString : Mods.Everware.ConfigKeys.StyleSettings.Unchanged.GetTextValue(), new Vector2(dimensions.X + dimensions.Width - 60f - x + (BlueberryUIThing * 20f), dimensions.Y + 8f), Color.White.MultiplyRGBA(new(b, b, b, b)), 0f, Vector2.Zero, new Vector2(0.8f), spread: 2f - (BlueberryUIThing * 2f));
        Rectangle sourceRectangle = tex.Frame();
        Vector2 drawPosition = new Vector2(dimensions.X + dimensions.Width - 76f - x, dimensions.Y + 12f);

        Main.spriteBatch.End();

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.UIScaleMatrix);

        float rot = (float)Math.Sin(UITimer / 30f) * MathHelper.ToRadians(10f);
        float up = (float)Math.Sin(UITimer / 42.5f) * 4f;
        float sc = 1f - (up * 0.005f);

        Sq = MathHelper.Lerp(Sq, BlueberryUIThing, 0.3f);

        float a = (float)Math.Sin(Sq * 20f) * (Sq * 0.3f);
        a = a * MathHelper.Lerp(2f, 1f, a);

        Vector2 sqsh = new Vector2(1f + a, 1f - a);

        Main.EntitySpriteDraw(tex.Value, drawPosition + new Vector2(0, MathHelper.Lerp(up, 6f, 0.3f) + 3f), sourceRectangle, new Color(0f, 0f, 0f, 0.1f), rot, sourceRectangle.Size() / 2f, sqsh * sc * 1.1f, SpriteEffects.None);
        Main.EntitySpriteDraw(tex.Value, drawPosition + new Vector2(0, up), sourceRectangle, Color.White, rot, sourceRectangle.Size() / 2f, sqsh * sc, SpriteEffects.None);
    }
}
