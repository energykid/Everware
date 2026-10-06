using Terraria.GameContent.Creative;
using Terraria.ID;

namespace Everware.Content.Base.Items;

public abstract class EverItem : ModItem
{
    /// <summary>
    /// Represents the ID of the vanilla item that this item will replace. 
    /// </summary>
    public virtual int VanillaID => ItemID.None;
    /// <summary>
    /// Represents the config option that determines whether this item will replace its VanillaID.
    /// </summary>
    public virtual bool ReplacementCondition => false;
    public virtual int DuplicationAmount => 100;

    public virtual int Rarity => ItemRarityID.White;

    public Asset<Texture2D> Asset;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.rare = Rarity;
        if (Asset != null)
        {
            Item.height = Asset.Width();
            Item.width = Asset.Height();
        }
    }

    [ModSystemHooks.PostAddRecipes]
    public void Setup(ModSystem self)
    {
        for (int i = 0; i < Recipe.numRecipes; i++)
        {
            Recipe recipe = Main.recipe[i];

            if (recipe.TryGetIngredient(VanillaID, out Item ingredient))
            {
                recipe.RemoveIngredient(VanillaID);
                recipe.AddIngredient(Type, ingredient.stack);
            }
            if (recipe.TryGetResult(VanillaID, out Item result))
            {
                recipe.ReplaceResult(Type, result.stack);
            }
        }
    }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Asset = ModContent.Request<Texture2D>(Texture);
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = DuplicationAmount;
    }
}