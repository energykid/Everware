using System.Collections.Generic;
using System.IO;
using Everware.Content.Base.Items;
using Everware.Content.Base.Tiles;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Everware.Content.Gallery.Sculptor;

#region Drop Rule
public class ExcessiveForceDropRule : IItemDropRule
{
    public int chanceDenominator;
    public int itemId;
    
    public bool CanDrop(DropAttemptInfo info)
    {
        return info.npc.type == ModContent.NPCType<SculptorNPC>();
    }

    public ExcessiveForceDropRule(
        int chDenominator)
    {
        itemId = ModContent.ItemType<ExcessiveForce>();
        chanceDenominator = chDenominator;
        ChainedRules = new List<IItemDropRuleChainAttempt>();
    }
    public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
    {
        float personalDropRate = 1f / (float)chanceDenominator;
        float dropRate = personalDropRate * ratesInfo.parentDroprateChance;
        drops.Add(new DropRateInfo(this.itemId, 1, 1, dropRate, ratesInfo.conditions));
        Chains.ReportDroprates(this.ChainedRules, personalDropRate, drops, ratesInfo);
    }

    public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
    {
        if (Main.rand.NextBool(chanceDenominator))
        {
            if (info.npc.ModNPC is SculptorNPC sculptor)
            {
                int item = CommonCode.DropItem(info.npc.getRect(), new EntitySource_Loot(info.npc, "Sculptor Drop"), itemId, 1, true);
                if (Main.item[item].ModItem is ExcessiveForce exForce)
                {
                    exForce.SculptorName = sculptor.NameIndex;
                    if (Main.dedServ)
                    {
                        NetMessage.SendData(MessageID.SyncItem, number: item);
                    }
                }
            }
            return new ItemDropAttemptResult
            {
                State = ItemDropAttemptResultState.Success
            };
        }
        return new ItemDropAttemptResult
        {
            State = ItemDropAttemptResultState.FailedRandomRoll
        };
    }

    public List<IItemDropRuleChainAttempt> ChainedRules { get; set; }
}
#endregion

#region Tile Data

public struct SculptorNameData : ITileData
{
    public int SculptorName;
}

#endregion

public class ExcessiveForce : EverPlaceableItem
{
    public override int Rarity => 2;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.maxStack = 1;
    }

    public override void OnConsumeItem(Player player)
    {
        Point p = (Main.MouseWorld / 16).ToPoint();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                Main.tile[new Point(p.X + i, p.Y + j)].Get<SculptorNameData>().SculptorName = SculptorName;
            }
        }
        base.OnConsumeItem(player);
    }

    public int SculptorName = 0;
    
    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(SculptorName);
    }

    public override void NetReceive(BinaryReader reader)
    {
        SculptorName = reader.ReadInt32();
    }

    public override void SaveData(TagCompound tag)
    {
        tag.Set("SculptorName", SculptorName);
    }

    public override void LoadData(TagCompound tag)
    {
        SculptorName = tag.GetInt("SculptorName");
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        int ind = tooltips.FindIndex(T => T.Name == "Placeable");
        if (ind != -1)
            tooltips.Insert(ind + 1, new TooltipLine("SculptorName", "'" + SculptorNPC.FirstNameList[SculptorName] + " " + SculptorNPC.LastNameList[SculptorName] + "'"));
        base.ModifyTooltips(tooltips);
    }

    public override string Texture => "Everware/Assets/Textures/Gallery/Sculptor/ExcessiveForce";
    public override int PlacementID => ModContent.TileType<ExcessiveForcePlaced>();
}

public class ExcessiveForcePlaced : EverMultitile
{
    public override string Texture => "Everware/Assets/Textures/Gallery/Sculptor/ExcessiveForce_Placed";
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        return true;
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        Item it = new Item(ModContent.ItemType<ExcessiveForce>());
        (it.ModItem as ExcessiveForce).SculptorName = Main.tile[i,j].Get<SculptorNameData>().SculptorName;
        return [it];
    }
}