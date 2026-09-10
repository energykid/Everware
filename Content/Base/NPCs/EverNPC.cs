namespace Everware.Content.Base.NPCs;

public abstract class EverNPC : ModNPC
{
    public virtual Vector2 Size => new Vector2(20, 20);
    public virtual int FrameNumber => 1;
    public override void SetDefaults()
    {
        NPC.width = (int)Size.X;
        NPC.height = (int)Size.Y;
    }
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = FrameNumber;
    }
}
