using Everware.Content.Meteor.Tiles;
using Terraria.ID;

namespace Everware.Content.Meteor.Items;

public class MeteoriteChanges : GlobalItem
{
    public override bool InstancePerEntity => true;
    static Asset<Texture2D>? VanillaMeteoriteSprite;
    static Asset<Texture2D>? VanillaMeteoriteBarItemSprite;
    static Asset<Texture2D>? VanillaMeteoriteBarTileSprite;
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type == ItemID.Meteorite || entity.type == ItemID.MeteoriteBar;
    }
    public override void SetDefaults(Item entity)
    {
        if (entity.type == ItemID.Meteorite)
        {
            entity.DefaultToPlaceableTile(ModContent.TileType<Meteorite>());
        }

        if (entity.type == ItemID.MeteoriteBar)
        {
            entity.DefaultToPlaceableTile(ModContent.TileType<MeteoriteBar>());
        }
    }
    public override void Load()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            VanillaMeteoriteSprite = TextureAssets.Item[ItemID.Meteorite];
            TextureAssets.Item[ItemID.Meteorite] = Assets.Textures.Meteor.Tiles.MeteoriteItem.Asset;
            
            VanillaMeteoriteBarItemSprite = TextureAssets.Item[ItemID.MeteoriteBar];
            TextureAssets.Item[ItemID.MeteoriteBar] = Assets.Textures.Meteor.Items.MeteoriteBar.Asset;
        }
    }
    public override void Unload()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            if (VanillaMeteoriteSprite != null)
                TextureAssets.Item[ItemID.Meteorite] = VanillaMeteoriteSprite;
            
            if (VanillaMeteoriteBarItemSprite != null)
                TextureAssets.Item[ItemID.MeteoriteBar] = VanillaMeteoriteBarItemSprite;
        }
    }
}
