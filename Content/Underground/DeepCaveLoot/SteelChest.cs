using Everware.Content.Base.Items;
using Everware.Content.Base.Tiles;
using Terraria.ID;

namespace Everware.Content.Underground.DeepCaveLoot;

public class SteelChestTile : ChestTemplate
{
    protected override bool CanBeLocked => true;
    public override string Texture => "Everware/Assets/Textures/Underground/SteelChestTile";
    public override string HighlightTexture => "Everware/Assets/Textures/Underground/SteelChestTile_Outline";
    public override bool CanBeUnlockedNormally => true;
    protected override int ChestKeyItemId => ItemID.GoldenKey;
    public override int DropItem => ModContent.ItemType<SteelChestItem>();
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }
}

public class SteelChestItem : EverPlaceableItem
{
    public override int Rarity => 3;
    public override int PlacementID => ModContent.TileType<SteelChestTile>();
    public override string Texture => "Everware/Assets/Textures/Underground/SteelChestItem";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = Sell.Silver(45);
    }
}