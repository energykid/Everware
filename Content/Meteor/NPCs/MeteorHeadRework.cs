using Everware.Content.Misc.Particles;
using Terraria.ID;

namespace Everware.Content.Meteor.NPCs;

public class MeteorHeadRework : GlobalNPC
{
    public static readonly int ExplosionDamage = 30;
    int Personality = 0;
    public bool Exploding = false;
    public override bool InstancePerEntity => true;
    public override void SetDefaults(NPC entity)
    {
        Personality = Main.rand.Next(5);
        base.SetDefaults(entity);
    }
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == NPCID.MeteorHead;
    }
    public override bool PreAI(NPC npc)
    {
        if (Exploding)
        {
            new SmallSmoke(npc.Center, npc.velocity / 2f, Color.Black with {A = 150}).Spawn();
            npc.ai[0] += 2.5f;
            npc.velocity.Y += 0.2f;
            Tile t = Main.tile[npc.Center.ToTileCoordinates()];
            if (t.HasTile && Main.tileSolid[t.TileType])
            {
                Explode(npc);
                npc.active = false;
                npc.StrikeInstantKill();
            }

            SpawnDust(npc, 5);
        }
        else
        {
            if (npc.ai[3] == 1)
            {
                npc.ai[0] += 0.5f;
                SpawnDust(npc);
                return false;
            }

            if (npc.ai[1] != 0)
            {
                if (!Main.npc[(int)npc.ai[1]].active)
                {
                    if (npc.ai[2] < 2)
                        npc.ai[2] = 0;
                    npc.ai[1] = 0;
                    npc.netUpdate = true;
                }
                npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionFrom(Main.npc[(int)npc.ai[1]].Center) * 0.5f, 0.07f);
            }
            else
            {
                npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(Main.player[npc.target].Center) * 0.5f, 0.07f);
            }

            npc.rotation = npc.rotation.AngleLerp(Main.player[npc.target].AngleTo(npc.Center), 0.1f);
            npc.ai[0] += 0.5f;
            npc.TargetClosest(false);
            SpawnDust(npc);
        }

        return false;
    }

    private void SpawnDust(NPC npc, float num = 1)
    {
        for (float i = 0; i < num; i++)
        {
            Vector2 pos = Vector2.Lerp(npc.Center, npc.oldPosition, i / num) + new Vector2(14, 0).RotatedBy(npc.rotation).RotatedByRandom(MathHelper.PiOver2);
            Dust d = Dust.NewDustPerfect(pos, DustID.FlameBurst, Vector2.Zero);
            d.noGravity = true;
            if (Main.rand.NextBool(3))
            {
                Dust d2 = Dust.NewDustPerfect(pos, DustID.Smoke, -npc.velocity * 2f);
                d2.scale = 0.5f;
                d2.noGravity = true;
            }
        }
    }

    public static void Explode(NPC npc)
    {
        SoundEngine.PlaySound(SoundID.NPCDeath14.WithPitchVariance(0.2f), npc.Center);
        
        if (!Main.dedServ)
            Projectile.NewProjectile(new EntitySource_Parent(npc), npc.Center, Vector2.Zero,
                ModContent.ProjectileType<MeteorHeadExplosion>(), ExplosionDamage, 2f);
    }
    
    public static void Draw(NPC npc, Vector2 screenPos, Color drawColor)
    {
        Lighting.AddLight(npc.Center, Color.Red.ToVector3() * 0.2f);

        if (npc.IsABestiaryIconDummy) npc.ai[0] += 0.5f;

        Vector2 origin = new(16, 16);

        int HeadFrameY = (int)(npc.ai[0] / 5f % 3);
        if (npc.ai[2] > 0) HeadFrameY = 3;

        var HeadAsset = Assets.Textures.Meteor.NPCs.MeteorHead.Asset;
        var GlowAsset = Assets.Textures.Meteor.NPCs.MeteorHead_Glow.Asset;
        var HeadFrame = HeadAsset.Frame(5, 4, npc.GetGlobalNPC<MeteorHeadRework>().Personality, HeadFrameY);

        var FlameAsset = Assets.Textures.Meteor.NPCs.MeteorHead_FireMask.Asset;

        var Effects = npc.velocity.X < 0 || npc.IsABestiaryIconDummy ? SpriteEffects.None : SpriteEffects.FlipVertically;

        if (npc.ai[2] < 1 || npc.ai[3] == 1)
        {
            Vector2 sc = new Vector2(1.15f);
            if (npc.GetGlobalNPC<MeteorHeadRework>().Exploding)
            {
                npc.rotation = npc.velocity.ToRotation() + MathHelper.Pi;
                sc.X *= 2f;
                origin.X = 0;
            }
            
            Main.spriteBatch.End(out var sb);

            float p1 = MathHelper.Lerp(1f, 0f, npc.ai[0] / 50f % 1f);
            float p2 = MathHelper.Lerp(1f, 0f, ((npc.ai[0] / 50f) + 0.5f) % 1f);

            var eff1 = Assets.Effects.Misc.GradientClip.CreateEffect();
            eff1.Parameters.LightingColor = Color.White.ToVector4();
            eff1.Parameters.ColorClip = p1;
            eff1.Parameters.ColorClipUpper = pwid(p1);
            eff1.Parameters.Gradient = Assets.Textures.Meteor.NPCs.MeteorFlameGradient.Asset.Value;
            eff1.Apply();

            Main.spriteBatch.Begin(sb with { CustomEffect = eff1.Shader });
            Main.EntitySpriteDraw(FlameAsset.Value, npc.Center - screenPos, FlameAsset.Frame(), Color.White, npc.rotation, origin, sc * npc.scale, Effects);

            Main.spriteBatch.End();
            var eff2 = Assets.Effects.Misc.GradientClip.CreateEffect();
            eff2.Parameters.LightingColor = Color.White.ToVector4();
            eff2.Parameters.ColorClip = p2;
            eff2.Parameters.ColorClipUpper = pwid(p2);
            eff2.Parameters.Gradient = Assets.Textures.Meteor.NPCs.MeteorFlameGradient.Asset.Value;
            eff2.Apply();
            Main.spriteBatch.Begin(sb with { CustomEffect = eff2.Shader });

            Main.EntitySpriteDraw(FlameAsset.Value, npc.Center - screenPos, FlameAsset.Frame(), Color.White, npc.rotation, origin, sc * npc.scale, Effects);

            Main.spriteBatch.Restart(sb);
        }

        var eff = Assets.Effects.Meteor.NPCs.MeteorHeadDissipate.CreateEffect();
        eff.Parameters.NoiseTexture = Assets.Textures.Misc.PerlinNoise.Asset.Value;
        eff.Parameters.Clip = -0.2f + (npc.ai[2] / 40f);
        eff.Apply();

        Main.spriteBatch.End(out var sb1);
        Main.spriteBatch.Begin(sb1 with { CustomEffect = eff.Shader });

        Main.EntitySpriteDraw(HeadAsset.Value, npc.Center - screenPos, HeadFrame, Color.Lerp(drawColor, Color.White, 0.75f), npc.rotation, origin, npc.scale, Effects);
        Main.EntitySpriteDraw(GlowAsset.Value, npc.Center - screenPos, HeadFrame, Color.White, npc.rotation, origin, npc.scale, Effects);

        Main.spriteBatch.Restart(sb1);
        float pwid(float i) { return (i * 2f); }
    }
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.ai[2] != 1) Draw(npc, screenPos, drawColor);
        return false;
    }
    public override void Load()
    {
        if (Main.netMode != NetmodeID.Server)
            On_NPC.FindFrame += CancelMeteorHeadDust;
    }
    public override void Unload()
    {
        if (Main.netMode != NetmodeID.Server)
            On_NPC.FindFrame -= CancelMeteorHeadDust;
    }

    private void CancelMeteorHeadDust(On_NPC.orig_FindFrame orig, NPC self)
    {
        if (self.type != NPCID.MeteorHead)
        {
            orig(self);
        }
    }
}
