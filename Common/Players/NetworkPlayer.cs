using Everware.Common.Packets;
using Everware.Core.Projectiles;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Common.Players;

public class NetworkItem : GlobalItem
{
    public override bool? UseItem(Item item, Player Player)
    {
        Player.GetModPlayer<NetworkPlayer>().AltFunction = Player.altFunctionUse;
        if (Main.netMode != NetmodeID.SinglePlayer && Player.whoAmI == Main.myPlayer)
        {
            EverwarePacketHandler.SendPacket(new PlayerDataPacket() { plr = Player });
        }

        return base.UseItem(item, Player);
    }
}

public class NetworkPlayer : ModPlayer
{
    public static float GlobalTimer;
    public Vector2 TickMousePosition = Vector2.Zero;
    public Vector2 MousePosition = Vector2.Zero;
    public bool MouseDown = false;
    public bool RightClicking = false;
    public int AltFunction = 0;
    public float Meter = 0f;

    public int AnimationTime = 0;

    public bool RightClicked = false;

    public override void PreUpdate()
    {

    }
    public override void PostUpdate()
    {
        if (Main.myPlayer == Player.whoAmI)
        {
            if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.MultiplayerClient)
            {
                MousePosition = Main.MouseWorld;
                MouseDown = Player.controlUseItem;
                AnimationTime = Player.itemAnimationMax;
                RightClicking = Player.controlUseTile;
                if (Player.GetEverWeaponItem() != null)
                    Meter = Player.GetEverWeaponItem().MeterFill;
            }
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                EverwarePacketHandler.SendPacket(new PlayerDataPacket() { plr = Player });
            }
        }
    }
    public override bool CanUseItem(Item item)
    {
        foreach (Projectile projectile in Main.projectile)
        {
            if (projectile.ModProjectile is EverHoldoutProjectile && projectile.owner == Player.whoAmI && projectile.active) return false;
        }

        return base.CanUseItem(item);
    }
}
