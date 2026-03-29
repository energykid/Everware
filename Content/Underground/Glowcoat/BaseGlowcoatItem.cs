using Everware.Content.Base.Items;
using System;
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
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(new Vector2(player.Center.X + (player.direction * 16) - 5, player.Center.Y), 10, 2, DustType, (player.direction * 2) + player.velocity.X, Scale: 0.8f);

            }
            bool inRange = Math.Abs(Player.tileTargetX - (player.Center.X / 16)) < Player.tileRangeX && Math.Abs(Player.tileTargetY - (player.Center.Y / 16)) < Player.tileRangeY;
            Tile t = Main.tile[Player.tileTargetX, Player.tileTargetY];
            if (t.HasTile && Main.tileSolid[t.TileType] && inRange)
            {
                if (t.Get<GlowcoatTileData>().color != Color)
                {
                    Point p = (Main.MouseWorld / 16).ToPoint();
                    GlowcoatSystem.Glowcoat(Player.tileTargetX, Player.tileTargetY, Color, Chromatic);
                    for (int i = 0; i < 15; i++)
                    {
                        Dust d = Dust.NewDustDirect(new Vector2((Player.tileTargetX * 16) + 8, (Player.tileTargetY * 16) + 8), 0, 0, DustType, Scale: 1.5f);
                        d.noGravity = true;
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
