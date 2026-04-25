using Everware.Common.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameInput;
using Terraria.ID;

namespace Everware;

public static class EverExtensions
{
    public static bool RightClicking(this Player plr)
    {
        return plr.GetModPlayer<NetworkPlayer>().RightClicking;
    }
    public static void SendRightClick(this Player plr, bool? result = null)
    {
        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            if (plr == Main.LocalPlayer)
            {
                ModPacket p = Everware.Instance.GetPacket();
                p.Write("RightClickNotifier");
                p.Write(plr.whoAmI);
                p.Write(result == null ? Main.mouseRight : result.Value);
                p.Send();
            }
        }
        plr.GetModPlayer<NetworkPlayer>().RightClicking = result == null ? Main.mouseRight : result.Value;
    }
}

public static class Sell
{
    /// <summary>
    /// Converts a value in silver coins to copper coins, accounting for sell price.
    /// </summary>
    /// <param name="silver">The number of silver coins this value should be equal to.</param>
    /// <returns>The equivalent number of copper coins times 5.</returns>
    public static int Silver(int silver)
    {
        return silver * 100 * 5;
    }
    /// <summary>
    /// Converts a value in gold coins to copper coins, accounting for sell price.
    /// </summary>
    /// <param name="silver">The number of gold coins this value should be equal to.</param>
    /// <returns>The equivalent number of copper coins times 5.</returns>
    public static int Gold(int gold)
    {
        return gold * 100 * 100 * 5;
    }
    /// <summary>
    /// Converts a value in platinum coins to copper coins, accounting for sell price.
    /// </summary>
    /// <param name="silver">The number of platinum coins this value should be equal to.</param>
    /// <returns>The equivalent number of copper coins times 5.</returns>
    public static int Platinum(int plat)
    {
        return plat * 100 * 100 * 100 * 5;
    }
    /// <summary>
    /// Converts a desired sell price to raw value, accounting for the 5x decrease when calculating sell value.
    /// </summary>
    /// <param name="copper">The integer to return.</param>
    /// <returns>The integer fed in times 5. Don't use this for anything except sell prices.</returns>
    public static int Copper(int copper)
    {
        return copper * 5;
    }
}