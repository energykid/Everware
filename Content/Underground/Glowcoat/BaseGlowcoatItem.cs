using Everware.Content.Base.Items;
using System;
using Terraria.ID;

namespace Everware.Content.Underground.Glowcoat;

public abstract class BaseGlowcoatItem : EverItem
{
    public virtual bool Chromatic => false;
    public virtual int MossItem => ItemID.LavaMoss;
    public virtual int MossBlock => TileID.LavaMoss;
    public virtual int DustType => DustID.LavaMoss;
    public virtual Asset<Texture2D> GlowAsset => Assets.Textures.Underground.MagmaticGlowcoat_Glow.Asset;
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
        Item.width = GlowAsset.Width();
        Item.height = GlowAsset.Height();
    }
    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        spriteBatch.Draw(GlowAsset.Value, Item.Center - Main.screenPosition, GlowAsset.Frame(), Color.White, rotation, GlowAsset.Frame().Size() / 2, scale, SpriteEffects.None, 0f);
    }
    public override bool? UseItem(Player player)
    {
        if (player.ItemAnimationJustStarted)
        {
            bool inRange = Math.Abs(Player.tileTargetX - (player.Center.X / 16)) < Player.tileRangeX && Math.Abs(Player.tileTargetY - (player.Center.Y / 16)) < Player.tileRangeY;
            Tile t = Main.tile[Player.tileTargetX, Player.tileTargetY];
            if (t.HasTile && inRange)
            {
                if (t.Get<GlowcoatTileData>().color != Color)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath9.WithPitchOffset(0.5f), player.Center);
                    for (int i = 0; i < 6; i++)
                    {
                        Dust d = Dust.NewDustDirect(new Vector2(player.Center.X + (player.direction * 16) - 5, player.Center.Y), 10, 2, DustType, (player.direction * 2) + player.velocity.X, Scale: 0.8f);
                    }
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
        Recipe recipe = CreateRecipe(5);
        recipe.AddTile(MossBlock);
        recipe.AddIngredient(ItemID.BottledWater);
        recipe.Register();

        Recipe recipe2 = CreateRecipe(10);
        recipe2.AddTile(TileID.DyeVat);
        recipe2.AddIngredient(ItemID.BottledWater);
        recipe2.AddIngredient(MossItem);
        recipe2.Register();
    }
}
