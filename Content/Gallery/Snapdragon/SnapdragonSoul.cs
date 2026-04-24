namespace Everware.Content.Gallery.Snapdragon;

public class SnapdragonSoul : ModNPC
{
    public override void SetDefaults()
    {
        NPC.defense = 2;
        NPC.life = 100000000;
        NPC.lifeMax = 100000000;
    }
    public override void HitEffect(NPC.HitInfo hit)
    {
        NPC n = Main.npc[(int)NPC.ai[0]];
        if (n != null)
        {
            if (n.active)
            {
                n.StrikeNPC(hit);
            }
        }
    }
}
