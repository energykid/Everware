using Terraria.ID;

namespace Everware.Common.Players;

public sealed class NetworkPlayer : ModPlayer
{
    public static float GlobalTimer;
    public Vector2 TickMousePosition = Vector2.Zero;
    public Vector2 MousePosition = Vector2.Zero;
    public bool MouseDown = false;
    public bool RightClicking = false;
    public int AltFunction = 0;

    public int AnimationTime = 0;

    public override void PostUpdate()
    {
        base.PostUpdate();
    }

    public override void PreUpdate()
    {
        if (Main.myPlayer == Player.whoAmI)
        {
            GlobalTimer++;
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                MousePosition = Main.MouseWorld;
                ModPacket p = Mod.GetPacket();
                p.Write("MouseWorld"); // formerly "5"
                p.Write(Player.whoAmI);
                p.WritePackedVector2(MousePosition);
                p.Send();

                AnimationTime = Player.itemAnimationMax;
                ModPacket p2 = Mod.GetPacket();
                p2.Write("ItemAnimationMax"); // formerly "7"
                p2.Write(Player.whoAmI);
                p2.Write(AnimationTime);
                p2.Send();

                MouseDown = Player.controlUseItem;
                ModPacket p3 = Mod.GetPacket();
                p3.Write("ControlUseItem"); // formerly "8"
                p3.Write(Player.whoAmI);
                p3.Write(MouseDown);
                p3.Send();

                AltFunction = Player.altFunctionUse;
                ModPacket p4 = Mod.GetPacket();
                p4.Write("AltFunctionUse"); // formerly "9"
                p4.Write(Player.whoAmI);
                p4.Write(AltFunction);
                p4.Send();
            }
            if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.MultiplayerClient)
            {
                MousePosition = Main.MouseWorld;
                AltFunction = Player.altFunctionUse;
                MouseDown = Player.controlUseItem;
                AnimationTime = Player.itemAnimationMax;
            }
        }
    }
}
