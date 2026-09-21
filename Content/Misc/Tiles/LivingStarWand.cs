using Everware.Content.Base.Items;
using Everware.Content.Base.Tiles;
using Terraria.ID;

namespace Everware.Content.Misc.Tiles;

public class LivingStarWand : EverPlaceableItem
{
    public override void SetStaticDefaults() => ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.LivingWoodWand);

        Item.width = 28;
        Item.height = 28;
        Item.tileWand = ItemID.FallenStar;
        Item.createTile = ModContent.TileType<LivingFallenStarTile>();
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.rare = ItemRarityID.Blue;
    }
    public override string Texture => "Everware/Assets/Textures/Misc/Tiles/LivingStarWand";
}