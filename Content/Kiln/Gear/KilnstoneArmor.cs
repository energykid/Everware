using Everware.Content.Base.Items;
using Everware.Content.Kiln.Tiles;
using Everware.Content.Kiln.Visual;
using Everware.Utils;
using System;
using Terraria.ID;

namespace Everware.Content.Kiln.Gear;

#region Armor Pieces
[AutoloadEquip(EquipType.Head)]
public class KilnstoneHelmet : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Kiln/KilnstoneHelmet";
    public override int DuplicationAmount => 1;
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
        recipe.AddTile(ModContent.TileType<ForgingKiln>());
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
    public override string Texture => "Everware/Assets/Textures/Kiln/KilnstoneBrickplate";
    public override int DuplicationAmount => 1;
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
        recipe.AddTile(ModContent.TileType<ForgingKiln>());
        recipe.Register();
    }
}
[AutoloadEquip(EquipType.Legs)]
public class KilnstoneChausses : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Kiln/KilnstoneChausses";
    public override int DuplicationAmount => 1;
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
        recipe.AddTile(ModContent.TileType<ForgingKiln>());
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
#endregion

public class KilnstoneSetBonus : ModPlayer
{
    public bool kilnstoneSetBonus = false;
    public float kilnstoneSetActive = 0f;
    public float kilnstoneTimer = 0f;

    bool soundIsPlaying;

    public override void ResetEffects()
    {
        kilnstoneSetBonus = false;
    }
    public override void PostUpdate()
    {
        if (!soundIsPlaying && kilnstoneSetBonus)
        {
            SoundStyle snd = Assets.Sounds.Gear.Armor.KilnstoneSmoke.Asset with
            {
                IsLooped = true
            };
            SoundEngine.PlaySound(snd, Player.Center, Sound =>
            {
                Sound.Position = Player.Center;
                Sound.Pitch = UseSpeedMult * 4f;
                Sound.Volume = UseSpeedMult * 0.5f;
                bool b = kilnstoneSetBonus && !Main.gameInactive;
                if (!b) soundIsPlaying = false;
                return b;
            });
            soundIsPlaying = true;
        }
        if (!kilnstoneSetBonus)
        {
            soundIsPlaying = false;
            kilnstoneSetActive = 0;
        }
        kilnstoneSetActive -= 0.2f;
        kilnstoneSetActive = Math.Clamp(kilnstoneSetActive, 0, 100);
        kilnstoneTimer++;

        if (kilnstoneSetActive > 10 && kilnstoneSetBonus)
        {
            int dustAlpha = 255 - (int)(UseSpeedMult * 255f * 2f);

            if (kilnstoneTimer % 6 == 0)
            {
                Dust d = Dust.NewDustPerfect(Player.Top + new Vector2(-3 + Main.rand.NextFloat(4), -5), ModContent.DustType<KilnstoneSmoke>(), new Vector2(0, -0.5f), dustAlpha, Scale: 0f);
                d.fadeIn = -3f;
            }
        }
    }

    float UseSpeedMult => Easing.KeyFloat(kilnstoneSetActive, 0f, 100f, 0f, 0.5f, Easing.Linear);
    bool PickaxeHeld => Player.HeldItem.pick != 0;

    public override float UseTimeMultiplier(Item item)
    {
        return base.UseSpeedMultiplier(item) * (PickaxeHeld ? (1 - (UseSpeedMult * 0.25f)) : 1f);
    }
    public override float UseAnimationMultiplier(Item item)
    {
        return base.UseAnimationMultiplier(item) * (1 - ((Player.HeldItem.pick != 0 ? UseSpeedMult : 0f) * 0.25f));
    }
    public override float UseSpeedMultiplier(Item item)
    {
        return base.UseSpeedMultiplier(item) + UseSpeedMult * 0.5f;
    }
    public override void Load()
    {
        EverwarePacketHandler.AddPacket(
            (mod, reader, whoAmI, identifier) =>
            {
                if (identifier == "IncreaseKilnstoneSetBonus")
                {
                    int plr = reader.ReadInt32();
                    float amt = reader.ReadSingle();

                    Main.player[plr].GetModPlayer<KilnstoneSetBonus>().kilnstoneSetActive += amt;

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket packet = Everware.Instance.GetPacket();
                        packet.Write("IncreaseKilnstoneSetBonus");
                        packet.Write(plr);
                        packet.Write(amt);
                        packet.Send(ignoreClient: plr);
                    }
                }
            }
        );

        On_Player.GetPickaxeDamage += On_Player_GetPickaxeDamage;
    }
    public override void Unload()
    {
        On_Player.GetPickaxeDamage -= On_Player_GetPickaxeDamage;
    }

    private int On_Player_GetPickaxeDamage(On_Player.orig_GetPickaxeDamage orig, Player self, int x, int y, int pickPower, int hitBufferIndex, Tile tileTarget)
    {
        if (self.GetModPlayer<KilnstoneSetBonus>().kilnstoneSetBonus)
        {
            float power = 3 + ((100f / (float)pickPower));

            int type = tileTarget.TileType;
            if (TileID.Sets.Dirt[type] || TileID.Sets.Stone[type] || TileID.Sets.Grass[type] || type == TileID.ClayBlock || type == TileID.Mud || type == TileID.Sand)
            {
                self.GetModPlayer<KilnstoneSetBonus>().kilnstoneSetActive += power;

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = Everware.Instance.GetPacket();
                    packet.Write("IncreaseKilnstoneSetBonus");
                    packet.Write(self.whoAmI);
                    packet.Write((float)power);
                    packet.Send();
                }
            }
        }
        return orig(self, x, y, pickPower, hitBufferIndex, tileTarget);
    }
}