using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.Items;
using Everware.Utils;
using ReLogic.Content;
using System;
using Terraria.DataStructures;
using Terraria.ID;

namespace Everware.Content.Underground.DeepCaveLoot;

[AutoloadEquip(EquipType.Shoes)]
public class Groundshakers : EverItem
{
    public static int Damage => 20;
    public override string Texture => "Everware/Assets/Textures/Underground/DeepCaveLoot/Groundshakers";
    public override void SetDefaults()
    {
        Item.DefaultToAccessory(26, 28);
        Item.rare = ItemRarityID.Orange;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<GroundshakersPlayer>().Enabled = true;
        base.UpdateAccessory(player, hideVisual);
    }
}

public class GroundshakersPlayer : ModPlayer
{
    public bool Enabled = false;
    public int ParryCooldown = 0;
    float SpinCooldown = -1;
    float Spin = 0f;
    float Spin2 = 0f;
    public override void ResetEffects()
    {
        ParryCooldown -= 1;
        if (Enabled && Player.velocity.Y != 0 && ParryCooldown < 0 && (Player == Main.LocalPlayer || Main.netMode == NetmodeID.Server))
        {
            if (Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[0] < 15 && Player.doubleTapCardinalTimer[1] == 0)
            {
                SoundEngine.PlaySound(Assets.Sounds.Gear.Accessory.GroundshakersLunge.Asset.WithPitchVariance(0.2f), Player.Center);
                ParryCooldown = 60;
            }
        }

        Enabled = false;
    }
    public override void TransformDrawData(ref PlayerDrawSet drawInfo)
    {
        // run this if in game & not inanimate
        if (!Main.gameMenu)
        {
            int pC = Player.GetModPlayer<GroundshakersPlayer>().ParryCooldown;
            // only run if parrying is currently possible
            if (pC > 50)
            {
                Color c = Color.Lerp(Color.Transparent, Color.White, (pC - 50f) / 10f);
                var StreakEffect = Assets.Effects.Underground.GroundshakerStreaks.CreateEffect();
                StreakEffect.Parameters.Color = c.MultiplyRGBA(Lighting.GetColor((Player.Center / 16).ToPoint())).ToVector4();
                StreakEffect.Parameters.Clip = MathHelper.Lerp(0.8f, 0.5f, (pC - 50f) / 10f);
                StreakEffect.Parameters.Time = GlobalTimer.Value / 7f;
                StreakEffect.Parameters.Coords = new Vector2(0.5f, 0.1f);
                StreakEffect.Apply();

                var GlowEffect = Assets.Effects.Underground.GlowcoatColoration.CreateEffect();
                GlowEffect.Parameters.Color = c.MultiplyRGBA(Lighting.GetColor((Player.Center / 16).ToPoint())).ToVector4();
                GlowEffect.Apply();

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, StreakEffect.Shader, Main.GameViewMatrix.ZoomMatrix);

                Asset<Texture2D> tex = Assets.Textures.Misc.PerlinNoise.Asset;
                Main.EntitySpriteDraw(tex.Value, Player.Center + new Vector2(0, -30) - Main.screenPosition, tex.Frame(), Color.White, 0f, tex.Size() / 2f, new Vector2(0.3f, 0.6f), SpriteEffects.None);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, GlowEffect.Shader, Main.GameViewMatrix.ZoomMatrix);

                for (int i = 0; i < drawInfo.DrawDataCache.Count; i++)
                {
                    DrawData data = drawInfo.DrawDataCache[i];
                    for (int j = 0; j < 4; j++)
                    {
                        Vector2 pos = data.position;
                        data.position += new Vector2(2, 0).RotatedBy(MathHelper.ToRadians(90 * j));
                        data.Draw(Main.spriteBatch);
                        data.position = pos;
                    }
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, Main._multiplyBlendState, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.ZoomMatrix);
            }
        }
    }
    public float Cooldown => Player.GetModPlayer<GroundshakersPlayer>().ParryCooldown;
    public override void PreUpdateMovement()
    {
        SpinCooldown--;
        if (SpinCooldown == 0)
        {
            SoundEngine.PlaySound(Assets.Sounds.Gear.Accessory.GroundshakersImpact.Asset.WithPitchVariance(0.2f), Player.Center);

            Player.velocity.Y = -8;
            ScreenEffects.AddScreenShake(Player.Center, 5f);
            ScreenEffects.ZoomScreen(-0.3f);
        }
        if (SpinCooldown > 0)
        {
            Player.velocity = Vector2.Zero;
            ScreenEffects.ZoomScreen(0.2f);
        }
        if (SpinCooldown < 0)
            Spin *= 0.9f;
        if (Math.Abs(Spin) >= 1)
        {
            Player.fullRotationOrigin = Player.Size / 2f;
        }
        Player.fullRotation += MathHelper.ToRadians(Spin - Spin2);
        Spin2 = Spin;
        float v = MathHelper.Lerp(10f, 25f, (Cooldown - 50f) / 10f);
        if (Cooldown > 50)
        {
            Player.velocity.Y = v;
            Player.velocity.X = 0f;

            for (int i = 0; i < Main.ActiveNPCs.span.Length; i++)
            {
                if (Main.ActiveNPCs.span[i].IsHostile() && Main.ActiveNPCs.span[i].immune[Player.whoAmI] <= 0 && Main.ActiveNPCs.span[i].active)
                {
                    if (Main.ActiveNPCs.span[i].Distance(Player.Bottom) < 80 && Player.Bottom.Y < Main.ActiveNPCs.span[i].Center.Y)
                    {
                        if (Cooldown > 51)
                            Player.velocity.X = (MathHelper.Lerp(Player.Center.X, Main.ActiveNPCs.span[i].Center.X, 0.35f) - Player.Center.X);
                    }
                }
            }
        }
    }
    public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
    {
        if (Cooldown > 50)
        {
            Player.AddImmuneTime(cooldownSlot, 1);
            for (int i = 0; i < Main.ActiveNPCs.span.Length; i++)
            {
                if (Main.ActiveNPCs.span[i].IsHostile() && Main.ActiveNPCs.span[i].immune[Player.whoAmI] <= 0 && Main.ActiveNPCs.span[i].active)
                {
                    if (Main.ActiveNPCs.span[i].Distance(Player.Bottom) < 20)
                    {
                        Spin = 360f * Player.direction;
                        SoundEngine.PlaySound(Assets.Sounds.Gear.Accessory.GroundshakersSweetener.Asset.WithPitchVariance(0.2f), Player.Center);
                        SpinCooldown = 7;
                        Player.GetModPlayer<GroundshakersPlayer>().ParryCooldown = 35;
                        Main.ActiveNPCs.span[i].SimpleStrikeNPC(Groundshakers.Damage, Player.direction, Main.rand.NextBool(10), 4f, DamageClass.MeleeNoSpeed, true);
                    }
                }
            }
            return false;
        }
        return base.CanBeHitByNPC(npc, ref cooldownSlot);
    }
}