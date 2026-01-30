using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Everware.Content.Base.Items;

public abstract class EverItem : ModItem
{
    public virtual int DuplicationAmount => 100;

    public virtual int Rarity => ItemRarityID.White;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.rare = Rarity;
    }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = DuplicationAmount;
    }
}
