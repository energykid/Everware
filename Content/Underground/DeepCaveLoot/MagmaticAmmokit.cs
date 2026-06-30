using Everware.Content.Base.Items;
using Everware.Core.Projectiles;

namespace Everware.Content.Underground.DeepCaveLoot;

public class MagmaticAmmokit : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Underground/DeepCaveLoot/MagmaticAmmokit";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToAccessory(30, 24);
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);
    }
}

public class MagmaticAmmokitPlayer : ModPlayer
{

}

public class MagmaticExplosion : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Underground/DeepCaveLoot/MagmaticAmmokit";
    Vector2 DrawPosition = Vector2.Zero;
    bool Start = false;
    public override void SetDefaults()
    {
        base.SetDefaults();
    }
    public override void AI()
    {
        if (DrawPosition == Vector2.Zero)
        {
            DrawPosition = Projectile.Center;
        }
        base.AI();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}
