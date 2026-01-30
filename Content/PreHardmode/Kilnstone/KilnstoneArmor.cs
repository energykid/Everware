using Everware.Content.Base.Items;
using Everware.Utils;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;

namespace Everware.Content.PreHardmode.Kilnstone;

[AutoloadEquip(EquipType.Head)]
public class KilnstoneHelmet : EverItem
{
    public override int Rarity => ItemRarityID.Green;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToArmor(2);
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(1);
        recipe.AddIngredient(ModContent.ItemType<KilnBrick>(), 25);
        recipe.AddTile(TileID.Furnaces);
        recipe.Register();
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
    {
        return head.type == Type && body.type == ModContent.ItemType<KilnstoneBrickplate>() && legs.type == ModContent.ItemType<KilnstoneChausses>();
    }
    public override void UpdateArmorSet(Player player)
    {
        {
            player.GetModPlayer<KilnstoneSetBonus>().kilnstoneSetBonus = true;
            player.setBonus = "Lingering mining speed boost when breaking soft blocks or stone";
        }
    }
}
[AutoloadEquip(EquipType.Body)]
public class KilnstoneBrickplate : EverItem
{
    public override int Rarity => ItemRarityID.Green;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToArmor(2);
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(1);
        recipe.AddIngredient(ModContent.ItemType<KilnBrick>(), 20);
        recipe.AddIngredient(ItemID.StoneBlock, 15);
        recipe.AddTile(TileID.Furnaces);
        recipe.Register();
    }
}
[AutoloadEquip(EquipType.Legs)]
public class KilnstoneChausses : EverItem
{
    public override int Rarity => ItemRarityID.Green;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToArmor(1);
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(1);
        recipe.AddIngredient(ModContent.ItemType<KilnBrick>(), 10);
        recipe.AddIngredient(ItemID.StoneBlock, 10);
        recipe.AddTile(TileID.Furnaces);
        recipe.Register();
    }

    public override void SetStaticDefaults()
    {
        if (!Main.dedServ)
        {
            var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlot] = true;
        }
    }
}

public class KilnstoneSetBonus : ModPlayer
{
    public bool kilnstoneSetBonus = false;
    public float kilnstoneSetActive = 0f;

    public override void ResetEffects()
    {
        kilnstoneSetBonus = false;
        kilnstoneSetActive -= 0.2f;
        kilnstoneSetActive = Math.Clamp(kilnstoneSetActive, 0, 100);

        if (kilnstoneSetActive > 10)
        {
            float randomChance = UseSpeedMult * 2f;
            float dustSpeed = UseSpeedMult * 2f;
            float dustSize = 1.5f + UseSpeedMult * 1.5f;
            int dustAlpha = 255 - (int)(UseSpeedMult * 200f);

            if (Main.rand.NextFloat() < randomChance)
            {
                Dust d = Dust.NewDustPerfect(Player.Top + new Vector2(0, -4), DustID.Smoke, new Vector2(0, -dustSpeed), dustAlpha, Scale: dustSize);
                d.fadeIn = -3f;
            }
        }
    }
    public override void Load()
    {
        On_Player.GetPickaxeDamage += On_Player_GetPickaxeDamage;
    }
    public override void Unload()
    {
        On_Player.GetPickaxeDamage -= On_Player_GetPickaxeDamage;
    }

    float UseSpeedMult => Player.HeldItem.pick != 0 ? Easing.KeyFloat(kilnstoneSetActive, 0f, 100f, 0f, 0.5f, Easing.Linear) : 0f;

    public override float UseTimeMultiplier(Item item)
    {
        return base.UseSpeedMultiplier(item) * (1 - (UseSpeedMult / 2f));
    }
    public override float UseAnimationMultiplier(Item item)
    {
        return base.UseAnimationMultiplier(item) * (1 - (UseSpeedMult / 2f));
    }
    public override float UseSpeedMultiplier(Item item)
    {
        return base.UseSpeedMultiplier(item) + UseSpeedMult;
    }

    private int On_Player_GetPickaxeDamage(On_Player.orig_GetPickaxeDamage orig, Player self, int x, int y, int pickPower, int hitBufferIndex, Tile tileTarget)
    {
        if (self.GetModPlayer<KilnstoneSetBonus>().kilnstoneSetBonus)
        {
            int type = tileTarget.TileType;
            if (TileID.Sets.Dirt[type] || TileID.Sets.Stone[type] || type == TileID.ClayBlock || type == TileID.Mud || type == TileID.Sand)
                self.GetModPlayer<KilnstoneSetBonus>().kilnstoneSetActive += 5;
        }
        return orig(self, x, y, pickPower, hitBufferIndex, tileTarget);
    }
}