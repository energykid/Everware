using Everware.Content.Base.Items;
using Everware.Content.Base.ParticleSystem;
using Everware.Content.Misc.Particles;
using Terraria.ID;

namespace Everware.Content.Underground.DeepCaveLoot;

public class TrickGlove : EverEquipmentItem
{
    public override string Texture => "Everware/Assets/Textures/Underground/DeepCaveLoot/TrickGlove";

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToAccessory(30, 28);
    }

    private int selOld;
    private int cooldown;
    private int animationTime = 20;
    
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        animationTime++;

        if (animationTime < 8 && animationTime % 2 == 1)
        {
            Vector2 pos = player.MountedCenter + new Vector2(Main.rand.NextFloat(15f, 25f) * (Main.rand.NextBool() ? 1f : -1f), Main.rand.NextFloat(20f, 30f) * (Main.rand.NextBool() ? 1f : -1f));
            pos.Y += 10;
            
            var particle1 = new SleightParticle(pos, 0f, player.whoAmI, Main.rand.NextBool() ? -1 : 1);
            particle1.Spawn();
            
            for (int i = 0; i < 8; i++)
                new PurpleParticle(pos, new Vector2(Main.rand.NextFloat(0f, 2f), 0).RotatedByRandom(MathHelper.TwoPi), Color.White, Assets.Textures.Underground.DeepCaveLoot.TrickGloveParticle.Asset, 3, particle1, player.whoAmI).Spawn();
        }
        
        if (player.ItemAnimationJustStarted)
            cooldown--;
        if (player.selectedItem != selOld && cooldown <= 0)
        {
            if (player.inventory[player.selectedItem].DamageType == DamageClass.Magic &&
                player.inventory[selOld].DamageType == DamageClass.Magic)
            {
                float dir = Main.rand.NextBool() ? 1f : -1f;

                animationTime = 0;
                
                cooldown = 3;
                player.statMana = Math.Clamp(player.statMana + 30, 0, player.statManaMax2);
                player.ManaEffect(30);
                
                player.AddBuff(BuffID.MagicPower, 240);
                SoundEngine.PlaySound(Assets.Sounds.Gear.Accessory.TrickGloveSleight.Asset.WithPitchVariance(0.1f), player.Center);
            }
        }
        selOld = player.selectedItem;
    }

    class PurpleParticle : AnimationParticle
    {
        private Particle Parent;
        private int Owner;
        public PurpleParticle(Vector2 pos, Vector2 vel, Color color, Asset<Texture2D> asset, int frames,
            Particle parent, int owner) : base(pos, vel, color, asset, frames)
        {
            Parent = parent;
            Owner = owner;
            Pixelated = true;
        }

        public override void Update()
        {
            AffectedByLight = false;
            Center += velocity;
            velocity *= 0.9f;
            ai[0]++;
            if (ai[0] % 6 == 0)
            {
                FrameNum.Y++;
                if (FrameNum.Y >= FrameCount.Y)
                {
                    Kill();
                }
            }
            
            if (Main.player[Owner] != null)
            {
                Center += (Main.player[Owner].position - Main.player[Owner].oldPosition);
            }
            
            Center += Parent.velocity;
        }
    }
    class SleightParticle : Particle
    {
        public override Asset<Texture2D> Texture => Assets.Textures.Underground.DeepCaveLoot.TrickGloveSparkle.Asset;
        private int Owner;
        public SleightParticle(Vector2 pos, float rot, int player, float dir) : base(pos, Vector2.Zero, Vector2.One, null, null)
        {
            AffectedByLight = false;
            Origin = Texture.Size() / 2f;
            Color = Color.White;
            Scale = new Vector2(0f, 0f);
            ai[2] = Main.rand.NextFloat(0.5f, 1.5f);
            Rotation = rot;
            Owner = player;
            ai[1] = dir;
            ai[0] = 1f;
            velocity = new Vector2(0, -Main.rand.NextFloat(2f));
        }

        public override void Update()
        {
            ai[0] += 1f;

            velocity *= 0.85f;
            
            if (ai[0] < 10) Scale = Vector2.Lerp(Scale, new Vector2(ai[2]), 0.3f);
            else Scale = Vector2.Lerp(Scale, new Vector2(ai[2]), -0.4f);
            
            base.Update();

            Rotation += ai[0] * 0.005f * ai[1];
            
            if (Main.player[Owner] != null)
            {
                Center += (Main.player[Owner].position - Main.player[Owner].oldPosition);
            }
            
            if (Scale.Y < 0f) Kill();
        }
    }
}