using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Everware.Content.Base.Items;

public static class EverItemExtensions
{
    public static void DefaultToArmor(this Item item, int defense)
    {
        item.wornArmor = true;
        item.defense = defense;
    }
}
