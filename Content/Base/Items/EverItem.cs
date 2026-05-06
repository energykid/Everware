using Terraria.GameContent.Creative;
using Terraria.ID;

namespace Everware.Content.Base.Items;

public abstract class EverItem : ModItem
{
    public virtual int DuplicationAmount => 100;

    public virtual int Rarity => ItemRarityID.White;

    public static Asset<Texture2D> Asset = null;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.rare = Rarity;
        Item.height = Asset.Width();
        Item.width = Asset.Height();
    }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Asset = ModContent.Request<Texture2D>(Texture);
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = DuplicationAmount;
    }
}
