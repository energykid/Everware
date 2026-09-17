using Terraria.ID;

namespace Everware.Content.Meteor.NPCs;

public class MeteorHeadRework : GlobalNPC
{
    public override bool InstancePerEntity => true;
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == NPCID.MeteorHead;
    }
    public override bool PreAI(NPC npc)
    {
        npc.TargetClosest(false);
        npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(Main.player[npc.target].Center) * 0.5f, 0.07f);
        return false;
    }
}
