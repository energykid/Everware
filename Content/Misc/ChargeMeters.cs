using Everware.Content.Base;
using Everware.Content.Base.Items;
using Everware.Utils;

namespace Everware.Content.Misc;

[Autoload]
public class ChargeMeters : ILoadable
{
    public static float LocalChargeMeterVisibility = 0f;
    public static float LocalChargeMeterFill = 0f;
    public static float LocalChargeMeterAnim = 0f;
    public void Load(Mod mod)
    {
        On_Main.DrawNPCs += On_Main_DrawNPCs;
        On_Main.Update += On_Main_Update;
    }

    public void Unload()
    {
        On_Main.DrawNPCs -= On_Main_DrawNPCs;
        On_Main.Update -= On_Main_Update;
    }

    private void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        orig(self, behindTiles);

        if (!behindTiles && !Main.dedServ)
        {
            if (Main.LocalPlayer.GetEverWeaponItem() is EverWeaponItem weapon)
            {
                float rot = 0f;
                rot = (float)Math.Sin((GlobalTimer.Value / 2f) + (LocalChargeMeterAnim * 20f)) * LocalChargeMeterAnim * 0.2f;
                float sc = 1f + (Easing.OutBack(LocalChargeMeterAnim) * 0.4f);

                var asset = Assets.Textures.Misc.ChargeMeter.Asset;

                var eff = Assets.Effects.Misc.ChargeMeter.CreateEffect();
                eff.Parameters.FillTexture = Assets.Textures.Misc.ChargeMeter_Shine.Asset.Value;
                eff.Parameters.Color1 = weapon.MeterColors();
                eff.Parameters.Color2 = weapon.MeterColors2();
                eff.Parameters.Alpha = Math.Clamp(LocalChargeMeterVisibility, 0f, 1f);
                eff.Parameters.Time = GlobalTimer.Value / -200f;
                eff.Parameters.ColorNumber = 3;
                eff.Apply();

                Main.EntitySpriteDraw(asset.Value,
                    Main.LocalPlayer.Bottom - Main.screenPosition + new Vector2(0f, Main.LocalPlayer.gfxOffY),
                    asset.Frame(),
                    Color.Black.MultiplyRGBA(new(LocalChargeMeterVisibility, LocalChargeMeterVisibility, LocalChargeMeterVisibility, LocalChargeMeterVisibility * 0.5f * LocalChargeMeterAnim)),
                    rot, new Vector2(asset.Width() / 2, -6), sc * 1.05f, SpriteEffects.None);

                Main.EntitySpriteDraw(asset.Value,
                    Main.LocalPlayer.Bottom - Main.screenPosition + new Vector2(0f, Main.LocalPlayer.gfxOffY),
                    asset.Frame(),
                    Color.DarkSlateGray.MultiplyRGBA(new(LocalChargeMeterVisibility, LocalChargeMeterVisibility, LocalChargeMeterVisibility, LocalChargeMeterVisibility)),
                    rot, new Vector2(asset.Width() / 2, -6), sc, SpriteEffects.None);

                Main.spriteBatch.End(out var sb);
                Main.spriteBatch.Begin(sb with { CustomEffect = eff.Shader });

                Main.EntitySpriteDraw(asset.Value,
                    Main.LocalPlayer.Bottom - Main.screenPosition + new Vector2(0f, Main.LocalPlayer.gfxOffY),
                    new Rectangle(0, 0, (int)(asset.Frame().Width * LocalChargeMeterFill), asset.Frame().Height),
                    Color.White.MultiplyRGBA(new(LocalChargeMeterVisibility, LocalChargeMeterVisibility, LocalChargeMeterVisibility, LocalChargeMeterVisibility)),
                    rot, new Vector2(asset.Width() / 2, -6), sc, SpriteEffects.None);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(sb);
            }
        }
    }

    private void On_Main_Update(On_Main.orig_Update orig, Main self, GameTime gameTime)
    {
        LocalChargeMeterAnim *= 0.8f;

        if (Main.LocalPlayer.GetEverWeaponItem() != null)
            LocalChargeMeterFill = MathHelper.Lerp(LocalChargeMeterFill, Main.LocalPlayer.GetEverWeaponItem().MeterFill, 0.4f);

        LocalChargeMeterVisibility *= 0.98f;
        LocalChargeMeterVisibility -= 0.005f;
        LocalChargeMeterVisibility = Math.Max(LocalChargeMeterVisibility, 0f);
        orig(self, gameTime);
    }
}
