using Everware.Content.Base.Items;
using Everware.Content.Base.ParticleSystem;
using Terraria.ID;

namespace Everware.Content.Underground.DeepCaveLoot;

public class TrickGlove : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Underground/DeepCaveLoot/TrickGlove";

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToAccessory(30, 28);
    }

    private int selOld;
    private int cooldown;
    
    public override void UpdateAccessory(Player player, bool hideVisual)
    {

        if (player.ItemAnimationJustStarted)
            cooldown--;
        if (player.selectedItem != selOld && cooldown <= 0)
        {
            if (player.inventory[player.selectedItem].DamageType == DamageClass.Magic &&
                player.inventory[selOld].DamageType == DamageClass.Magic)
            {
                float dir = Main.rand.NextBool() ? 1f : -1f;
                
                new SleightParticle(player.MountedCenter + new Vector2(0, -50), 0f, player.whoAmI, dir).Spawn();
                new SleightParticle(player.MountedCenter + new Vector2(0, -50), MathHelper.PiOver2, player.whoAmI, dir).Spawn();
                
                cooldown = 3;
                player.statMana = Math.Clamp(player.statMana + 30, 0, player.statManaMax2);
                player.ManaEffect(30);
                
                player.AddBuff(BuffID.MagicPower, 240);
                SoundEngine.PlaySound(Assets.Sounds.Gear.Accessory.TrickGloveSleight.Asset.WithPitchVariance(0.1f), player.Center);
            }
        }
        selOld = player.selectedItem;
    }

    class SleightParticle : Particle
    {
        public override Asset<Texture2D> Texture => Assets.Textures.Misc.LensFlash.Asset;
        private int Owner;
        public SleightParticle(Vector2 pos, float rot, int player, float dir) : base(pos, Vector2.Zero, Vector2.One, null, null)
        {
            AffectedByLight = false;
            Asset = Assets.Textures.Misc.LensFlash.Asset;
            Origin = Asset.Size() / 2f;
            Color = new Color(255, 255, 61, 255);
            Scale = new Vector2(0.3f, 1f);
            Rotation = rot;
            Owner = player;
            ai[1] = dir;
            ai[0] = 0.25f;
        }

        public override void Update()
        {
            ai[0] *= 0.85f;
            
            base.Update();

            Rotation += ai[0] * ai[1];
            
            if (Main.player[Owner] != null)
            {
                position += Main.player[Owner].velocity;
            }

            Color = Color.Lerp(Color, new Color(111, 10, 126, 255), 0.1f);
            
            Scale = Vector2.Lerp(Scale, new Vector2(0.1f, -0.1f), 0.1f);
            if (Scale.Y < 0f) Kill();
        }
    }
}