using System.Collections.Generic;
using Terraria.ID;

namespace Everware.Content.Reliquary;

public class ChiselablesList : ModSystem
{
    public static List<Chiselable> AllChiselables = [];
    public override void Load()
    {

    }
    public override void Unload()
    {
        AllChiselables.Clear();
    }
}
public class Chiselable
{
    public int BaseStatue;
    public int UpgradedStatue;
    public Chiselable(int Base, int Upgrade)
    {
        BaseStatue = Base;
        UpgradedStatue = Upgrade;
    }
}
