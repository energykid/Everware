using Terraria.ID;

namespace Everware.Content.Base.NPCs;

public abstract class EverNPC : ModNPC
{
    public virtual Vector2 Size => new Vector2(20, 20);
    public virtual int FrameNumber => 1;
    public int CurrentFrame = 1;
    public virtual int Damage => 0;
    public virtual int TrailLength => 5;
    public virtual bool UsesCustomTrail => false;
    public override void SetDefaults()
    {
        NPC.width = (int)Size.X;
        NPC.height = (int)Size.Y;
        NPC.life = NPC.lifeMax = 100;
        NPC.damage = Damage;
    }
    public override void SetStaticDefaults()
    {
        NPCID.Sets.TrailingMode[Type] = UsesCustomTrail ? -1 : 3;
        NPCID.Sets.TrailCacheLength[Type] = TrailLength;
        Main.npcFrameCount[Type] = FrameNumber;
    }
}
