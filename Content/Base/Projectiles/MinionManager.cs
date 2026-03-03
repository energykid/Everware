using System.Collections.Generic;

namespace Everware.Content.Base.Projectiles;

public class MinionManager : ModPlayer
{
    public static List<MinionBuffLink> AllMinionLinks = [];

    public static void LinkMinionToBuff(int minion, int buff)
    {
        AllMinionLinks.Add(new MinionBuffLink(minion, buff));
    }

    public class MinionBuffLink
    {
        public int MinionID;
        public int BuffID;

        public MinionBuffLink(int minionID, int buffID)
        {
            MinionID = minionID;
            BuffID = buffID;
        }
    }

    public override void PostUpdateBuffs()
    {
        foreach (MinionBuffLink link in AllMinionLinks)
        {
            if (!Player.HasBuff(link.BuffID))
            {
                if (Player.ownedProjectileCounts[link.MinionID] > 0)
                {
                    for (int i = 0; i < Main.projectile.Length; i++)
                    {
                        if (Main.projectile[i].type == link.MinionID)
                        {
                            Main.projectile[i].Kill();
                        }
                    }
                }
            }
        }
    }
}