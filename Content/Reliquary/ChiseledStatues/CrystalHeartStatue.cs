using Everware.Content.Base;
using Everware.Content.Base.Items;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Reliquary.ChiseledStatues;

public class CrystalHeartStatueItem : EverStatueItem
{
    public override string Texture => "Everware/Assets/Textures/Reliquary/ChiseledStatues/CrystalHeartStatueItem";
    public override int PlacementID => ModContent.TileType<CrystalHeartStatue>();
    public override int BaseStatue => ItemID.HeartStatue;
}
public class CrystalHeartStatue : EverStatueTile
{
    public override int MaxCooldown => 60 * 10;
    public override string Texture => "Everware/Assets/Textures/Reliquary/ChiseledStatues/CrystalHeartStatue";
    public override void OnPulse(Point pos)
    {
        SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, pos.ToVector2() * 16);
        Item.NewItem(new EntitySource_Misc("Crystal Heart Statue"), GetRect(pos), ModContent.ItemType<PowerHeart>());
    }
}
public class PowerHeart : EverPickupItem
{
    public override string Texture => "Everware/Assets/Textures/Reliquary/ChiseledStatues/PowerHeart";
    public override bool OnPickup(Player player)
    {
        player.AddBuff(ModContent.BuffType<PowerHealingBuff>(), 300);
        SoundEngine.PlaySound(SoundID.Grab, player.Center);
        SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse.WithPitchOffset(1f), player.Center);
        return base.OnPickup(player);
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.scale = 0f;
    }
    public override void Update(ref float gravity, ref float maxFallSpeed)
    {

    }
    float sc = 0f;
    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Lighting.AddLight(Item.Center, 0.4f, 0, 0.4f);
        float rot = Item.velocity.X * 0.3f;
        Item.scale = MathHelper.Lerp(Item.scale, 1f, 0.2f);
        float sc = (float)Math.Pow((double)Easing.OutBack(MathHelper.Clamp(Item.scale, 0f, 1f)), 5);
        Vector2 s = new Vector2(sc, 2 - sc);
        Asset<Texture2D> Glow = Assets.Textures.Misc.SmallGlow.Asset;
        Asset<Texture2D> MainTex = Assets.Textures.Reliquary.ChiseledStatues.PowerHeart.Asset;
        spriteBatch.Draw(Glow.Value, Item.Center - Main.screenPosition, Glow.Frame(), new Color(55, 0, 55, 0), 0f, Glow.Size() / 2f, s * 0.5f, SpriteEffects.None, 0f);
        spriteBatch.Draw(MainTex.Value, Item.Center - Main.screenPosition, MainTex.Frame(), Color.White, rot, MainTex.Size() / 2f, s, SpriteEffects.None, 0f);
        return false;
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
}

public class PowerHealingBuff : ModBuff
{
    public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
    {
        Asset<Texture2D> MainTex = Assets.Textures.Reliquary.ChiseledStatues.PowerHeart.Asset;
        spriteBatch.Draw(MainTex.Value, drawParams.Position + new Vector2(16) + new Vector2(0f, 4f), MainTex.Frame(), new(0f, 0f, 0f, 0.3f), (float)Math.Sin(GlobalTimer.Value / 90f) * 0.1f, MainTex.Size() / 2f, 0.9f + ((float)Math.Sin(GlobalTimer.Value / 60f) * 0.1f), SpriteEffects.None, 0f);
        spriteBatch.Draw(MainTex.Value, drawParams.Position + new Vector2(16) + new Vector2(0f, -2f), MainTex.Frame(), Color.White, (float)Math.Sin(GlobalTimer.Value / 90f) * 0.1f, MainTex.Size() / 2f, 1f + ((float)Math.Sin(GlobalTimer.Value / 60f) * 0.15f), SpriteEffects.None, 0f);
    }
    public override string Texture => "Everware/Assets/Textures/Reliquary/ChiseledStatues/PowerHealingBuff";
    public override bool ReApply(Player player, int time, int buffIndex)
    {
        player.buffTime[buffIndex] += time;
        return false;
    }
    public override void Update(Player player, ref int buffIndex)
    {
        if (player.buffTime[buffIndex] % 5 == 0)
        {
            player.statLife += 1;
            player.statLife = (int)MathHelper.Clamp(player.statLife, 0, player.statLifeMax);
        }
    }
}