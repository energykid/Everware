using Everware.Content.Base;
using Everware.Content.Base.Items;
using Everware.Utils;

namespace Everware.Content.Misc;

[Autoload]
public class ChargeMeters : ILoadable
{
    public static float LocalChargeMeterVisibility = 0f;
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
                var asset = Assets.Textures.Misc.ChargeMeter.Asset;

                var eff = Assets.Effects.Misc.ChargeMeter.CreateEffect();
                eff.Parameters.FillTexture = Assets.Textures.Misc.PerlinNoise.Asset.Value;
                eff.Parameters.Resolution = new Vector2(5f, 4f);
                eff.Parameters.Color1 = weapon.MeterColors();
                eff.Parameters.Color2 = weapon.MeterColors2();
                eff.Parameters.Alpha = Math.Clamp(LocalChargeMeterVisibility, 0f, 1f);
                eff.Parameters.Time = GlobalTimer.Value / 600f;
                eff.Parameters.ColorNumber = 4;
                eff.Apply();

                Main.EntitySpriteDraw(asset.Value,
                    Main.LocalPlayer.Bottom - Main.screenPosition + new Vector2(0f, Main.LocalPlayer.gfxOffY),
                    asset.Frame(),
                    Color.DarkSlateGray.MultiplyRGBA(new(LocalChargeMeterVisibility, LocalChargeMeterVisibility, LocalChargeMeterVisibility, LocalChargeMeterVisibility)),
                    0f, new Vector2(asset.Width() / 2, -6), 1f, SpriteEffects.None);

                Main.spriteBatch.End(out var sb);
                Main.spriteBatch.Begin(sb with { CustomEffect = eff.Shader });

                Main.EntitySpriteDraw(asset.Value,
                    Main.LocalPlayer.Bottom - Main.screenPosition + new Vector2(0f, Main.LocalPlayer.gfxOffY),
                    new Rectangle(0, 0, (int)(asset.Frame().Width * weapon.MeterFill), asset.Frame().Height),
                    Color.White.MultiplyRGBA(new(LocalChargeMeterVisibility, LocalChargeMeterVisibility, LocalChargeMeterVisibility, LocalChargeMeterVisibility)),
                    0f, new Vector2(asset.Width() / 2, -6), 1f, SpriteEffects.None);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(sb);
            }
        }
    }

    private void On_Main_Update(On_Main.orig_Update orig, Main self, GameTime gameTime)
    {
        LocalChargeMeterVisibility *= 0.98f;
        LocalChargeMeterVisibility -= 0.005f;
        LocalChargeMeterVisibility = Math.Max(LocalChargeMeterVisibility, 0f);
        orig(self, gameTime);
    }
}
