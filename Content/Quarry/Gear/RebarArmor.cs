using Everware.Content.Base.Items;
using Everware.Content.Base.Tiles.TileData;
using Everware.Content.Quarry.Tiles;
using Everware.Content.Quarry.Visual;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace Everware.Content.Quarry.Gear;

#region Armor Pieces
[AutoloadEquip(EquipType.Head)]
public class RebarCrest : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Quarry/RebarCrest";
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
        recipe.AddIngredient(ModContent.ItemType<RebarRod>(), 25);
        recipe.AddTile(ModContent.TileType<WeldingStation>());
        recipe.Register();
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
    {
        return head.type == Type && body.type == ModContent.ItemType<RebarGridmail>() && legs.type == ModContent.ItemType<RebarSandals>();
    }
    public override void UpdateArmorSet(Player player)
    {
        {
            player.GetModPlayer<RebarSetBonus>().rebarSetBonus = true;
            player.setBonus = "Chance to duplicate most ores";
        }
    }

    public override void SetStaticDefaults()
    {
        if (!Main.dedServ)
        {
            var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            ArmorIDs.Head.Sets.DrawFullHair[equipSlot] = true;
        }
    }
}
[AutoloadEquip(EquipType.Body)]
public class RebarGridmail : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Quarry/RebarGridmail";
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
        recipe.AddIngredient(ModContent.ItemType<RebarRod>(), 20);
        recipe.AddTile(ModContent.TileType<WeldingStation>());
        recipe.Register();
    }

    public override void SetStaticDefaults()
    {
        if (!Main.dedServ)
        {
            var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
        }
    }
}
[AutoloadEquip(EquipType.Legs)]
public class RebarSandals : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Quarry/RebarSandals";
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
        recipe.AddIngredient(ModContent.ItemType<RebarRod>(), 10);
        recipe.AddTile(ModContent.TileType<WeldingStation>());
        recipe.Register();
    }

    public override void SetStaticDefaults()
    {
        if (!Main.dedServ)
        {
            var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlot] = false;
        }
    }
}
#endregion

public class RebarSetBonus : ModPlayer
{
    public static readonly SoundStyle ScavengeSound = Assets.Sounds.Gear.Armor.RebarScavengeOre.Asset with { PitchRange = (-0.2f, 0.3f) };

    public bool rebarSetBonus = false;

    public override void ResetEffects()
    {
        rebarSetBonus = false;
    }
}

public class RebarGlobalTile : GlobalTile
{
    public override void Load()
    {
        EverwarePacketHandler.AddPacket(
            (mod, reader, whoAmI, identifier) =>
            {
                if (identifier == "SendRebarParticles")
                {
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();

                    Vector2 position = new Vector2(x, y);

                    SoundEngine.PlaySound(RebarSetBonus.ScavengeSound, position);
                    for (int k = 0; k < 5; k++)
                        Dust.NewDustPerfect(position, ModContent.DustType<RebarOreSparkle>());
                }
            }
        );
    }
    public void DropStuff(int i, int j, int type)
    {
        if (Main.tile[i, j].Get<LastPlayerMinedData>().WhichPlayerAmI != -1)
        {
            if (Main.player[Main.tile[i, j].Get<LastPlayerMinedData>().WhichPlayerAmI].GetModPlayer<RebarSetBonus>().rebarSetBonus && TileID.Sets.Ore[type])
            {
                if (Main.rand.NextBool(3))
                {
                    Vector2 position = new Vector2((i * 16) + 8, (j * 16) + 8);

                    int t = Main.tile[i, j].TileType;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            int ii = Item.NewItem(new EntitySource_TileBreak(i, j), new Rectangle(i * 16, j * 16, 16, 16), new Item(TileLoader.GetItemDropFromTypeAndStyle(type)), true);
                            Main.item[ii].GetGlobalItem<RebarGlobalItem>().StackabilityTimer = 1f;
                            Main.item[ii].GetGlobalItem<RebarGlobalItem>().CanBeStacked = false;

                            if (Main.netMode == NetmodeID.Server)
                            {
                                NetMessage.SendData(MessageID.SyncItem, number: ii);

                                ModPacket p = Everware.Instance.GetPacket();
                                p.Write("SendRebarParticles");
                                p.Write((int)position.X);
                                p.Write((int)position.Y);
                                p.Send();
                            }
                        }
                    }

                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        SoundEngine.PlaySound(RebarSetBonus.ScavengeSound, position);
                        for (int k = 0; k < 5; k++)
                            Dust.NewDustPerfect(position, ModContent.DustType<RebarOreSparkle>());
                    }
                }
                else
                {
                    int ii = Item.NewItem(new EntitySource_TileBreak(i, j), new Rectangle(i * 16, j * 16, 16, 16), new Item(TileLoader.GetItemDropFromTypeAndStyle(type)));
                }
            }
        }
    }

    public override bool CanDrop(int i, int j, int type)
    {
        if (Main.player[Main.tile[i, j].Get<LastPlayerMinedData>().WhichPlayerAmI].GetModPlayer<RebarSetBonus>().rebarSetBonus && TileID.Sets.Ore[type])
        {
            DropStuff(i, j, type);
            return false;
        }
        return base.CanDrop(i, j, type);
    }
}

public class RebarGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;
    public float StackabilityTimer = 0;
    public bool CanBeStacked = true;
    public override void NetSend(Item item, BinaryWriter writer)
    {
        writer.Write(CanBeStacked);
        writer.Write(StackabilityTimer);
    }
    public override void NetReceive(Item item, BinaryReader reader)
    {
        CanBeStacked = reader.ReadBoolean();
        StackabilityTimer = reader.ReadSingle();
    }
    public override bool CanStackInWorld(Item destination, Item source)
    {
        if (!CanBeStacked) return false;
        return base.CanStackInWorld(destination, source) && StackabilityTimer == 0f;
    }
    public override bool CanPickup(Item item, Player player)
    {
        return base.CanPickup(item, player) && StackabilityTimer <= 0.1f;
    }
    public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        Asset<Texture2D> fr = TextureAssets.Item[item.type];

        float sc = Easing.KeyFloat(StackabilityTimer, 0f, 1f, 1.5f, 1f, Easing.Linear, 1f);
        float a = Easing.KeyFloat(StackabilityTimer, 0f, 1f, 0f, 1f, Easing.Linear, 1f);

        for (int i = 0; i < 5; i++)
        {
            Main.EntitySpriteDraw(fr.Value,
                item.Center - Main.screenPosition,
                fr.Frame(), Color.White.MultiplyRGBA(new Color(a, a, a, 0f)), rotation, fr.Size() / 2f, scale * sc, SpriteEffects.None);
        }
    }
    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        StackabilityTimer *= 0.85f;
        base.Update(item, ref gravity, ref maxFallSpeed);
    }
}