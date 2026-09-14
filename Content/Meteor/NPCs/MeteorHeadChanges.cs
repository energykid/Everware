using System.Collections.Generic;
using Terraria.ID;

namespace Everware.Content.Meteor.NPCs;

public class MeteorHeadChanges : GlobalNPC
{
    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        if (spawnInfo.Player.InModBiome<MeteorBiome>())
        {
            pool[NPCID.MeteorHead] = 0.2f;
            pool[NPCID.EnchantedNightcrawler] = 0.1f;
        }
        base.EditSpawnPool(pool, spawnInfo);
    }
}
