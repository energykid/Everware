using Terraria.ID;

namespace Everware.Content.Base.Buffs;

public abstract class EverMinionBuff : ModBuff
{
    public virtual int MinionID => 0;
    public override void SetStaticDefaults()
    {
        BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
    }
    public override void Update(Player player, ref int buffIndex)
    {
        if (player.ownedProjectileCounts[MinionID] == 0)
        {
            player.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}
