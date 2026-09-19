using Everware.Content.Meteor.Tiles;
using Terraria.ID;

namespace Everware.Content.Meteor.Items;

public class MeteoriteChanges : GlobalItem
{
    public override bool InstancePerEntity => true;
    static Asset<Texture2D>? VanillaMeteoriteSprite;
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type == ItemID.Meteorite;
    }
    public override void SetDefaults(Item entity)
    {
        entity.DefaultToPlaceableTile(ModContent.TileType<Meteorite>());
    }
    public override void Load()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            VanillaMeteoriteSprite = TextureAssets.Item[ItemID.Meteorite];
            TextureAssets.Item[ItemID.Meteorite] = Assets.Textures.Meteor.Tiles.MeteoriteItem.Asset;
        }
    }
    public override void Unload()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            if (VanillaMeteoriteSprite != null)
                TextureAssets.Item[ItemID.Meteorite] = VanillaMeteoriteSprite;
        }
    }
}
