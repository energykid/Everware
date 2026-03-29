using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Underground.Glowcoat;

public abstract class BaseGlowcoatItem : EverItem
{
    public virtual bool Chromatic => false;
    public virtual int MossBlock => TileID.LavaMossBlock;
    public virtual int DustType => DustID.LavaMoss;
    public virtual Color Color => new Color(255, 90, 0);
    public override void SetDefaults()
    {
        Item.consumable = true;
        Item.useStyle = ItemUseStyleID.Thrust;
        Item.useTime = 10;
        Item.rare = ItemRarityID.LightRed;
        Item.maxStack = Item.CommonMaxStack;
        Item.useAnimation = 10;
        Item.autoReuse = true;
    }
    public override bool? UseItem(Player player)
    {
        if (player.ItemAnimationJustStarted)
        {
            if (Main.tile[(Main.MouseWorld / 16).ToPoint()].HasTile && Main.tileSolid[Main.tile[(Main.MouseWorld / 16).ToPoint()].TileType])
            {
                if (Main.tile[(Main.MouseWorld / 16).ToPoint()].Get<GlowcoatTileData>().color != Color)
                {
                    Point p = (Main.MouseWorld / 16).ToPoint();
                    GlowcoatSystem.Glowcoat(p.X, p.Y, Color, Chromatic);
                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDust(Main.MouseWorld, 0, 0, DustType, Scale: 0.8f);
                    }
                    Item.stack--;
                }
            }
        }

        return base.UseItem(player);
    }
    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(50);
        recipe.AddTile(MossBlock);
        recipe.AddIngredient(ItemID.BottledWater);
        recipe.Register();
    }
}
