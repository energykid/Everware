namespace Everware.Content.Base.Items;

public abstract class EverTomeWeapon : EverWeaponItem
{
    public virtual Asset<Texture2D> TomeAsset => Assets.Textures.Misc.TestItem.Asset;
    public override bool UseCustomDraw => true;
    public override void CustomDraw(Player player, float direction)
    {
        float a = ((float)player.itemAnimation / (float)player.itemAnimationMax * 7f);
        Rectangle frame = TomeAsset.Frame(1, 7, frameY: (int)(MathHelper.Clamp((float)a, 0f, 6f)));
        Vector2 pos = player.Center + new Vector2(player.direction > 0 ? 15 : -15, player.gfxOffY);
        Main.EntitySpriteDraw(TomeAsset.Value, pos - Main.screenPosition, frame, Lighting.GetColor((pos / 16).ToPoint()), 0f, frame.Size() / 2f, 1f, player.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
    }
    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(player.direction * -65f));

        float a = ((float)player.itemAnimation / (float)player.itemAnimationMax * 7f);
        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(player.direction * (-120f + (a * 4))));
    }
}
