namespace Everware.Content.Reliquary.ChiseledStatues;

public class PowerBuffPlayer : ModPlayer
{
    public override float UseSpeedMultiplier(Item item)
    {
        if (Player.HasBuff<PowerManaBuff>() && Player.HeldItem.DamageType == DamageClass.Magic)
        {
            return base.UseSpeedMultiplier(item) * 1.5f;
        }

        return base.UseSpeedMultiplier(item);
    }

    public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
    {
        if (Player.HasBuff<PowerManaBuff>() && Player.HeldItem.DamageType == DamageClass.Magic)
        {
            damage.Multiplicative *= 0.65f;
        }

        base.ModifyWeaponDamage(item, ref damage);
    }
}
