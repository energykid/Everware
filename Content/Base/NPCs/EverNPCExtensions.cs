namespace Everware.Content.Base.NPCs;

public static class EverNPCExtensions
{
    public static void VelocityMoveTowardsPosition(this NPC npc, Vector2 position, float speed, float accel, float cap = -1)
    {
        if (cap == -1)
            npc.velocity = Vector2.Lerp(npc.velocity, new Vector2(npc.Distance(position) * speed, 0).RotatedBy(npc.AngleTo(position)), accel);
        else
            npc.velocity = Vector2.Lerp(npc.velocity, new Vector2(MathHelper.Clamp(npc.Distance(position) * speed, 0f, cap), 0).RotatedBy(npc.AngleTo(position)), accel);
    }
    public static void LerpAngleTowardsPosition(this NPC npc, Vector2 position, float amount, float addon = 0f)
    {
        npc.rotation = npc.rotation.AngleLerp(npc.AngleTo(position) + addon, amount);
    }
}
