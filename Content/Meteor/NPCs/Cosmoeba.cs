using Everware.Content.Base.NPCs;

namespace Everware.Content.Meteor.NPCs;

public class Cosmoeba : EverNPC
{
    public override Vector2 Size => new Vector2(42, 42);
    public override int FrameNumber => 3;

    public override void SetDefaults()
    {
        base.SetDefaults();
    }
    public override void AI()
    {
        base.AI();
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        return false;
    }
}