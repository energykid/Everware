namespace Everware.Content.Reliquary.ChiseledStatues;

public class GreatGargoyleTagNPC : GlobalNPC
{
    public override void Load()
    {
        On_Main.DrawNPCs += On_Main_DrawNPCs;
    }
    public override void Unload()
    {
        On_Main.DrawNPCs -= On_Main_DrawNPCs;
    }

    private void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        if (behindTiles)
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                GreatGargoyleTagNPC taggedNPC = npc.GetGlobalNPC<GreatGargoyleTagNPC>();
                if (taggedNPC.Tagged)
                {
                    float v = npc.width + npc.height / 2;

                    var asset = Assets.Textures.Reliquary.ChiseledStatues.GreatGargoyle_Vortex.Asset;
                    var asset2 = Assets.Textures.Reliquary.ChiseledStatues.GreatGargoyle_VortexLoopingInternal.Asset;

                    var eff = Assets.Effects.Reliquary.ChiseledStatues.GargoyleVortex.CreateEffect();
                    eff.Parameters.Amount = 10f + (taggedNPC.TimerAdd * 2.5f);
                    eff.Parameters.Time = taggedNPC.Timer / 150f;
                    eff.Parameters.FillTexture = asset2.Value;
                    eff.Parameters.Resolution = asset.Size() / 2f;
                    eff.Parameters.ExtraRotation = taggedNPC.Timer / 20f;
                    eff.Parameters.Color = [
                            Color.Black.ToVector4() * 0.2f,
                    Color.Black.ToVector4() * 0.2f,
                    Color.Black.ToVector4() * 0.2f,
                    Color.Black.ToVector4() * 0.2f
                        ];
                    eff.Apply();

                    Main.spriteBatch.End(out var sb);
                    Main.spriteBatch.Begin(sb with { CustomEffect = eff.Shader, BlendState = BlendState.AlphaBlend, SamplerState = SamplerState.PointClamp });

                    Main.EntitySpriteDraw(asset.Value, npc.Center - Main.screenPosition, asset.Frame(), Color.White, 0f, asset.Size() / 2f, taggedNPC.Scale * (new Vector2(v, v) / new Vector2(60, 60)) * 1.15f, SpriteEffects.None);

                    Main.spriteBatch.End();

                    eff.Parameters.Color = [
                            Color.Black.ToVector4(),
                    Color.DarkSlateBlue.ToVector4() * new Vector4(0.75f, 0.75f, 0.5f, 1f),
                    GreatGargoyle.GreenYellow.ToVector4() * new Vector4(0.2f, 0.5f, 0.6f, 1f),
                    GreatGargoyle.GreenYellow.ToVector4()
                        ];
                    eff.Apply();

                    Main.spriteBatch.Begin(sb with { CustomEffect = eff.Shader, BlendState = BlendState.AlphaBlend, SamplerState = SamplerState.PointClamp });

                    Main.EntitySpriteDraw(asset.Value, npc.Center - Main.screenPosition, asset.Frame(), Color.White, 0f, asset.Size() / 2f, taggedNPC.Scale * (new Vector2(v, v) / new Vector2(60, 60)), SpriteEffects.None);

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(sb);
                }
            }
        }

        orig(self, behindTiles);
    }

    public override bool InstancePerEntity => true;

    int TagTime = 0;
    float Timer = 0f;
    float TimerAdd = 6f;
    Vector2 Scale = Vector2.Zero;
    public bool Tagged = false;
    public int SoundTimer = 0;

    public void Tag(NPC npc, int seconds)
    {
        Tagged = true;
        TagTime = 60 * seconds;
        TimerAdd = 6f;
        Scale = Vector2.Zero;
    }

    public bool ProjectileCountsForTagDamage(Projectile projectile)
    {
        if (!Tagged) return false;
        return projectile.DamageType == DamageClass.Melee ||
            projectile.DamageType == DamageClass.MeleeNoSpeed ||
            projectile.DamageType == DamageClass.Summon;
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        if (ProjectileCountsForTagDamage(projectile)) modifiers.FinalDamage.Multiplicative *= 1.1f;
    }
    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        base.HitEffect(npc, hit);
    }

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        base.ModifyHitByItem(npc, player, item, ref modifiers);
    }

    public override bool PreAI(NPC npc)
    {
        if (TagTime == 590)
        {
            SoundEngine.PlaySound(Assets.Sounds.Gear.Weapon.GreatGargoyleRift_Open.Asset, npc.Center);
        }

        if (TagTime > 0)
            TagTime--;

        if (TagTime == 1)
        {
            SoundEngine.PlaySound(Assets.Sounds.Gear.Weapon.GreatGargoyleRift_Close.Asset, npc.Center);
        }
        if (Tagged)
        {
            SoundTimer++;
            if (SoundTimer % 25 == 0)
            {
                SoundEngine.PlaySound(Assets.Sounds.Gear.Weapon.GreatGargoyleRift_Idle.Asset with { MaxInstances = 20, PitchRange = (0.5f, 1.5f) }, npc.Center, a =>
                {
                    if (TagTime < 10)
                    {
                        a.Pitch *= 0.8f;
                        a.Volume *= 0.9f;
                    }
                    if (TagTime <= 0)
                        return false;
                    return true;
                });
            }

            Timer -= 2f + TimerAdd;
            TimerAdd = MathHelper.Lerp(TimerAdd, -0.1f, 0.2f);

            if (TagTime <= 1)
            {
                Scale = Vector2.Lerp(Scale, new Vector2(1.25f, -0.1f), 0.3f);
                if (Scale.Y < 0f)
                    Tagged = false;
            }
            else
            {
                Scale = Vector2.Lerp(Scale, new Vector2(0.6f), 0.3f);
            }
        }

        return base.PreAI(npc);
    }

    public override void AI(NPC npc)
    {
    }
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {

        return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
    }
}