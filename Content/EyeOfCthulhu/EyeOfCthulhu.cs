using Everware.Common.Systems;
using Everware.Content.Base.NPCs;
using Everware.Content.Base.ParticleSystem;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Everware.Content.EyeOfCthulhuRework;

public class EyeOfCthulhu : GlobalNPC
{
    public static int BloodProjectileDamage => Main.expertMode ? 9 : 16;
    public static int BashDamage => Main.expertMode ? 38 : 28;
    public static int Phase2BashDamage => Main.expertMode ? 48 : 38;
    public static int TendrilDamage => Main.expertMode ? 32 : 25;
    public static int Phase2TendrilDamage => Main.expertMode ? 43 : 32;
    public static int DesperationThreshold => Main.expertMode ? Main.masterMode ? 700 : 400 : 250;
    public int Phase2Threshold = 0;
    public int Phase = 0;

    // Tendril 0: front left
    // Tendril 1: front right
    // Tendril 2: back left
    // Tendril 3: back right
    public Vector2[] TendrilPositions = new Vector2[4];
    public Vector2[] TendrilJointPositions = new Vector2[4];
    public Vector2[] TendrilClawPositions = new Vector2[4];
    public Vector2 VectorTarget = Vector2.Zero;
    public int TendrilsOut = 0;
    public float TendrilSplay = 0f;
    public float Shake = 0f;
    float Roar = 0f;
    public bool ContactDamage = false;
    public bool TendrilDamageEnabled = false;
    public bool MusicEnabled = false;
    public static float MusicPitch = 1f;
    public static bool ReworkEnabled = true;
    float AI3 = 0f;
    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((float)VectorTarget.X);
        binaryWriter.Write((float)VectorTarget.Y);

        binaryWriter.Write(ContactDamage);
        binaryWriter.Write(TendrilDamageEnabled);

        binaryWriter.Write(Phase);

        binaryWriter.Write(AI3);
    }
    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        VectorTarget.X = binaryReader.ReadSingle();
        VectorTarget.Y = binaryReader.ReadSingle();

        ContactDamage = binaryReader.ReadBoolean();
        TendrilDamageEnabled = binaryReader.ReadBoolean();

        Phase = binaryReader.ReadInt32();

        AI3 = binaryReader.ReadSingle();
    }
    public override bool InstancePerEntity => true;
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return ReworkEnabled && entity.type == NPCID.EyeofCthulhu;
    }
    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
    {
        return base.CanHitPlayer(npc, target, ref cooldownSlot) && ContactDamage;
    }
    public enum AttackState
    {
        TendrilsOut,

        Flee,
        Phase2Transition,
        None,
        Bash,
        BashCooldown,
        Bleed,
        Multiply,
        Stab,
        Swipe,
        Pinch
    }
    public AttackState CurrentAttack = AttackState.TendrilsOut;
    public enum PupilWidth
    {
        Narrow,
        Normal,
        Wide,
        Manual
    }
    public PupilWidth CurrentPupilWidth = PupilWidth.Normal;
    public override void SetDefaults(NPC npc)
    {
        base.SetDefaults(npc);
        npc.behindTiles = true;
        npc.aiStyle = -1;

        npc.ai[2] = (int)AttackState.TendrilsOut;

        for (int i = 0; i < 4; i++)
        {
            TendrilPositions[i] = TendrilJointPositions[i] = TendrilClawPositions[i] = npc.Center;
        }

        PupilPaletteNum = Main.rand.Next(3);

        Phase2Threshold = (int)(npc.lifeMax * 0.6f);
        if (Main.expertMode) Phase2Threshold = (int)(npc.lifeMax * 0.75f);
    }
    public int EyeDilationReset = 0;
    int TargetedPlayer = 0;
    public float Timer = 0f;
    public override void AI(NPC npc)
    {
        Timer++;

        if (Main.netMode != NetmodeID.Server)
        {
            if (npc.life < DesperationThreshold)
            {
                MusicPitch = MathHelper.Lerp(MusicPitch, MathHelper.Lerp(0.3f, 0f, npc.life / (float)DesperationThreshold), 0.1f);
            }
            else
            {
                MusicPitch = 0f;
            }
            MusicLoader.GetMusic(Everware.Instance, "Assets/Sounds/Music/EyeOfCthulhu").SetVariable("Pitch", MusicPitch);
        }

        ContactDamage = false;

        int Plr = -1;
        int Distance = 2500;
        foreach (Player player in Main.player)
        {
            if (player.Distance(npc.Center) < Distance)
            {
                Plr = player.whoAmI;
                Distance = (int)player.Distance(npc.Center);
            }
        }

        if (Plr != -1)
            npc.target = Plr;

        Player Target = Main.player[npc.target];
        bool ShouldFlee = Main.dayTime || Target.dead;

        if (TendrilsOut == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                TendrilPositions[i] = TendrilJointPositions[i] = TendrilClawPositions[i] = npc.Center;
            }
        }
        else
        {
            MoveTendrilsIdle(npc);
        }

        TendrilSplay = MathHelper.Lerp(TendrilSplay, 0f, 0.1f);

        EyeDilationReset--;
        if (EyeDilationReset <= 0)
            EyeDilation = MathHelper.Lerp(EyeDilation, 0.3f + (float)(Math.Sin(Timer / 10f) * 0.05f), 0.2f);
        else
        {
            EyeDilation = MathHelper.Lerp(EyeDilation, -0.2f, 0.6f);
        }


        switch ((AttackState)npc.ai[2])
        {
            case AttackState.Flee:
                npc.velocity.Y -= 0.5f;
                npc.velocity.X *= 1.02f;

                if (npc.Distance(Target.Center) > 2500)
                {
                    npc.active = false;
                }

                if (!ShouldFlee)
                {
                    ChangeState(npc, AttackState.None);
                }
                break;
            case AttackState.None:
                npc.ai[0]++;
                npc.LerpAngleTowardsPosition(Target.Center, 0.1f, MathHelper.ToRadians(-90f));
                npc.VelocityMoveTowardsPosition(GroundedTargetPosition(npc) + new Vector2((float)Math.Sin(Timer / 20f) * 50f, -200 + ((float)Math.Sin(Timer / 17f) * 30f)), 0.03f, 0.03f);

                if (ShouldFlee)
                {
                    ChangeState(npc, AttackState.Flee);
                }

                if (npc.life < Phase2Threshold && Phase == 0)
                {
                    ChangeState(npc, AttackState.Phase2Transition);
                }

                if (npc.ai[0] > 80)
                {
                    AttackState[] states = { AttackState.Bash, AttackState.Stab };
                    ChangeState(npc, states[Main.rand.Next(states.Length)]);
                }
                break;
            case AttackState.TendrilsOut:
                npc.ai[0]++;
                if (npc.ai[0] > 200)
                {
                    ScreenEffects.ZoomScreen(0.3f);
                    ScreenEffects.PanTo(npc.Center);

                    int Time = 14;

                    if (ModLoader.TryGetMod("CalamityFables", out Mod calFables))
                    {
                        calFables.Call("vfx.displayBossIntroCard", "Eye of Cthulhu", "Night Stalker", 100, false, Color.Red, Color.White, Color.DarkBlue, Color.DarkGreen, "ENNWAY", "");
                        Time = 20;
                    }

                    if (TendrilsOut <= 3)
                    {
                        EyeDilation = MathHelper.Lerp(EyeDilation, 1.5f, 0.5f);
                        npc.velocity *= 0.85f;
                        if (npc.ai[0] % Time == 1)
                        {
                            MusicEnabled = true;

                            UnleashTendril(npc);
                        }
                    }
                    else
                    {
                        ChangeState(npc, AttackState.None);
                    }
                }
                npc.LerpAngleTowardsPosition(Target.Center, 0.1f, MathHelper.ToRadians(-90f));
                npc.VelocityMoveTowardsPosition(GroundedTargetPosition(npc) + new Vector2((float)Math.Sin(Timer / 20f) * 50f, -200 + ((float)Math.Sin(Timer / 17f) * 30f)), 0.03f, 0.03f, 15);
                break;
            case AttackState.Bash:
                npc.damage = BashDamage;
                if (Phase > 0)
                    npc.damage = Phase2BashDamage;
                if (npc.ai[0] == 0)
                {
                    AI3 = 0f;
                    npc.velocity.Y -= 4f;
                    npc.ai[0]++;
                }
                else
                {
                    AI3 = MathHelper.Lerp(AI3, 1f, 0.075f);
                    if (AI3 < 0.8f)
                    {
                        npc.velocity *= 0.9f;
                        EyeDilation = MathHelper.Lerp(EyeDilation, 1f, 0.2f);
                        npc.ai[1] = 0f;
                        npc.LerpAngleTowardsPosition(Target.Center, 0.04f, MathHelper.ToRadians(-90f));
                        npc.VelocityMoveTowardsPosition(Target.Center + new Vector2((float)Math.Sin(AI3 / 20f) * 50f, -700 + ((float)Math.Sin(Timer / 17f) * 30f)), 0.02f, 0.2f);
                    }
                    else
                    {
                        if ((npc.rotation + MathHelper.PiOver2).AngleTowards(npc.AngleTo(npc.Center + new Vector2(0, 20)), MathHelper.ToRadians(50)) - npc.rotation >= MathHelper.ToRadians(40))
                            npc.LerpAngleTowardsPosition(npc.Center + new Vector2(0f, 1f), 0.01f, -MathHelper.PiOver2);

                        if (npc.ai[1] == 0)
                        {
                            SoundEngine.PlaySound(Assets.Sounds.NPC.EoCWhoosh.Asset, npc.Center);

                            if (Phase > 0)
                                EmitRoar(npc);
                        }

                        ContactDamage = true;

                        if (npc.Distance(Target.Center) > 800)
                            ChangeState(npc, AttackState.None);

                        EyeDilation = MathHelper.Lerp(EyeDilation, -0.5f, 0.2f);
                        if (npc.ai[1] < 0.5f)
                        {
                            npc.velocity *= 0.8f;
                        }
                        npc.ai[1] += 0.025f;
                        npc.ai[1] = MathHelper.Clamp(npc.ai[1], 0f, 1f);
                        npc.velocity += (npc.rotation + MathHelper.PiOver2).ToRotationVector2() * Easing.OutBounce(npc.ai[1]) * 1.75f;
                    }
                }
                bool b = false;
                for (int i = 0; i < 4; i++)
                {
                    if (SolidTileOrPlatformBeneath(npc, Main.tile[((npc.Center + new Vector2(0, i * 16).RotatedBy(npc.rotation)) / 16).ToPoint()]))
                    {
                        b = true;
                        break;
                    }
                }
                if (b && AI3 > 0.8f)
                {
                    ThrowTileReplicants(npc, (npc.Center / 16).ToPoint());
                    BleedAllOver(npc);
                    AI3 = 0f;
                    SoundEngine.PlaySound(Assets.Sounds.NPC.EoCBash.Asset, npc.Center);
                    ScreenEffects.AddScreenShake(npc.Center, 20f, 0.6f);
                    npc.velocity = Vector2.Zero;
                    npc.velocity.Y = -3f;
                    AI3 = -2f;
                    npc.ai[0]++;
                    if (Phase > 0)
                    {
                        if (npc.life < DesperationThreshold)
                        {
                            ChangeState(npc, AttackState.Bash);
                            npc.ai[0] = -40;
                            if (npc.life < DesperationThreshold / 2)
                            {
                                npc.ai[0] = -20;
                            }
                            if (npc.life < DesperationThreshold / 3)
                            {
                                npc.ai[0] = -10;
                            }
                        }
                        else
                        {
                            if (npc.ai[0] > Easing.KeyFloat(npc.life, DesperationThreshold, npc.lifeMax, 4, 1, Easing.Linear))
                            {
                                Shake = 7;
                                npc.velocity *= 0.4f;
                                ChangeState(npc, AttackState.BashCooldown);
                                npc.ai[0] = -30;
                            }
                        }
                    }
                    else
                    {
                        npc.velocity *= 0.6f;
                        ChangeState(npc, AttackState.BashCooldown);
                        Shake = 8;
                        npc.ai[0] = -40;
                    }
                }
                break;
            case AttackState.BashCooldown:
                ContactDamage = false;
                npc.ai[0]++;
                if (npc.ai[0] == 0)
                {
                    Shake = (MathHelper.Lerp(1f, 0f, (float)-npc.ai[0] * 0.2f));
                    SoundEngine.PlaySound(Assets.Sounds.NPC.EoCBash.Asset with { Pitch = 1f }, npc.Center);
                    npc.velocity.Y -= 13f;
                }
                if (npc.ai[0] >= 0)
                {
                    npc.LerpAngleTowardsPosition(Target.Center, 0.1f, MathHelper.ToRadians(-90f));

                    npc.velocity *= 0.88f;
                    npc.velocity.Y += 0.05f;
                    if (npc.ai[0] > 30)
                        ChangeState(npc, Main.rand.NextBool() ? AttackState.Bleed : AttackState.Multiply);
                }
                else
                {
                    npc.velocity *= 0.9f;
                    npc.velocity.Y = Math.Abs(npc.velocity.Y);
                }
                break;
            case AttackState.Bleed:
                npc.VelocityMoveTowardsPosition(GroundedTargetPosition(npc) + new Vector2((float)Math.Sin(Timer / 20f) * 50f, -250 + ((float)Math.Sin(Timer / 17f) * 30f)), 0.1f, 0.05f);
                npc.LerpAngleTowardsPosition(Target.Center, 0.1f, MathHelper.ToRadians(-90f));
                npc.ai[0]++;
                if (npc.ai[0] > 50)
                {
                    npc.VelocityMoveTowardsPosition(Target.Center + new Vector2((float)Math.Sin(Timer / 20f) * 50f, -200 + ((float)Math.Sin(Timer / 17f) * 30f)), 0.03f, 0.03f, 10);

                    npc.velocity *= 0.95f;
                    if (npc.ai[0] % 25 == 18)
                    {
                        for (int i = 0; i < ((Phase > 0) ? 2 : 1); i++)
                        {
                            BleedInDirection(npc, npc.Center + new Vector2(200, 0).RotatedBy(npc.rotation + MathHelper.ToRadians(90f)));
                            npc.velocity += new Vector2(-5, 0).RotatedBy(npc.rotation + MathHelper.ToRadians(90f));

                            SoundEngine.PlaySound(Assets.Sounds.NPC.EoCTendrilEmerge.Asset, npc.Center);
                            ScreenEffects.AddScreenShake(npc.Center, 10, 0.6f);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectileDirect(new EntitySource_Parent(npc, "EoC blood projectile"), PupilPosition(npc), npc.DirectionTo(PupilPosition(npc) + new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-10, 10))) * 25, ModContent.ProjectileType<CthulhuBloodProjectile>(), BloodProjectileDamage, 2f);
                        }
                    }
                    if (npc.ai[0] % 25 >= 18)
                    {
                        EyeDilation = MathHelper.Lerp(EyeDilation, 3, 0.4f);
                    }
                    if (npc.ai[0] > 150)
                    {
                        ChangeState(npc, AttackState.None);
                    }
                }
                else
                {
                    Shake += 0.2f;
                    if (npc.ai[0] >= 20)
                    {
                        EyeDilation = MathHelper.Lerp(EyeDilation, 1f + ((float)npc.ai[0] / 50f) * 2f, 0.4f);
                        if (npc.ai[0] == 20)
                            SoundEngine.PlaySound(Assets.Sounds.NPC.EoCMuffledRoar.Asset, npc.Center);
                    }
                    npc.velocity *= 0.95f;
                }
                break;
            case AttackState.Multiply:
                npc.VelocityMoveTowardsPosition(GroundedTargetPosition(npc) + new Vector2((float)Math.Sin(Timer / 20f) * 50f, -300 + ((float)Math.Sin(Timer / 17f) * 30f)), 0.02f, 0.2f);
                npc.LerpAngleTowardsPosition(Target.Center, 0.1f, MathHelper.ToRadians(-90f));
                npc.ai[0]++;
                if (npc.ai[0] > 50)
                {
                    npc.VelocityMoveTowardsPosition(Target.Center + new Vector2((float)Math.Sin(Timer / 20f) * 50f, -200 + ((float)Math.Sin(Timer / 17f) * 30f)), 0.03f, 0.03f, 10);

                    npc.velocity *= 0.95f;
                    if (npc.ai[0] % 10 == 5)
                    {
                        BleedInDirection(npc, npc.Center + new Vector2(200, 0).RotatedBy(npc.rotation + MathHelper.ToRadians(90f)));
                        npc.velocity += new Vector2(-5, 0).RotatedBy(npc.rotation + MathHelper.ToRadians(90f));

                        SoundEngine.PlaySound(Assets.Sounds.NPC.EoCTendrilEmerge.Asset, npc.Center);
                        ScreenEffects.AddScreenShake(npc.Center, 10, 0.6f);

                        int newNpc = NPC.NewNPC(new EntitySource_Parent(npc, "EoC minion"), (int)PupilPosition(npc).X, (int)PupilPosition(npc).Y, NPCID.ServantofCthulhu);
                        Main.npc[newNpc].velocity = npc.DirectionTo(PupilPosition(npc)) * 3f;
                    }
                    if (npc.ai[0] % 10 >= 5)
                    {
                        EyeDilation = MathHelper.Lerp(EyeDilation, 3, 0.4f);
                    }
                    if (npc.ai[0] > 80)
                    {
                        ChangeState(npc, AttackState.None);
                    }
                }
                else
                {
                    Shake += 0.2f;
                    if (npc.ai[0] >= 20)
                    {
                        EyeDilation = MathHelper.Lerp(EyeDilation, 1f + ((float)npc.ai[0] / 50f) * 2f, 0.4f);
                        if (npc.ai[0] == 20)
                            SoundEngine.PlaySound(Assets.Sounds.NPC.EoCMuffledRoar.Asset, npc.Center);
                    }
                    npc.velocity *= 0.95f;
                }
                break;
            case AttackState.Phase2Transition:
                npc.velocity *= 0.9f;
                if (npc.ai[0] == 0)
                {
                    VectorTarget = npc.Center;
                    SoundEngine.PlaySound(Assets.Sounds.NPC.EoCMuffledRoar.Asset with { Pitch = -0.5f }, npc.Center);
                }
                npc.VelocityMoveTowardsPosition(VectorTarget, 0.2f, 0.1f, 8);
                npc.ai[0]++;
                if (npc.ai[0] < 180)
                {
                    if (npc.ai[0] < 160 && npc.ai[0] > 15 && npc.ai[0] % 15 == 5)
                    {
                        TendrilSplay += 0.4f;
                        SoundEngine.PlaySound(Assets.Sounds.NPC.EoCTendrilEmerge.Asset with { Pitch = Easing.KeyFloat(npc.ai[0], 0, 100, 0.2f, 0.6f, Easing.Linear) }, npc.Center);
                        Vector2 rand = new Vector2(Main.rand.NextFloat(2f, 5f), 0f).RotatedByRandom(MathHelper.TwoPi);
                        BleedAllOver(npc);
                        BleedInDirection(npc, npc.Center + rand);
                        npc.velocity -= rand;
                    }

                    Shake += 0.5f;
                    npc.LerpAngleTowardsPosition(npc.Bottom, 0.005f, -MathHelper.PiOver2);
                }
                else
                {
                    if (npc.ai[0] >= 180 && npc.ai[0] < 230)
                    {
                        ScreenEffects.PanTo(npc.Center);
                        ScreenEffects.ZoomScreen(0.5f);
                        ScreenEffects.AddScreenShake(npc.Center, 2f);
                    }
                    if (npc.ai[0] > 260)
                    {
                        ChangeState(npc, AttackState.None);
                    }
                    if (npc.ai[0] == 180)
                    {
                        SoundEngine.PlaySound(Assets.Sounds.NPC.EoCTendrilEmerge.Asset with { Pitch = -0.2f }, npc.Center);

                        EmitRoar(npc);

                        BleedAllOver(npc);

                        for (int i = 0; i < 7; i++)
                        {
                            BleedInDirection(npc, npc.Center + new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-10, 10)));
                        }

                        Phase = 1;

                        npc.netUpdate = true;
                    }
                    npc.LerpAngleTowardsPosition(Target.Center, 0.4f, -MathHelper.PiOver2);
                }
                break;
            case AttackState.Stab:
                if (npc.ai[0] == 0)
                {
                    int dir = Main.rand.NextBool() ? -1 : 1;
                    if (Target.velocity.X > 0) dir = 1;
                    if (Target.velocity.X < 0) dir = -1;
                    VectorTarget = new Vector2(dir * 220, Main.rand.NextFloat(-90, -20));

                    npc.netUpdate = true;

                    if (Phase == 0)
                        SoundEngine.PlaySound(Assets.Sounds.NPC.EoCMuffledRoar.Asset, npc.Center);
                }
                npc.ai[0]++;

                npc.LerpAngleTowardsPosition(Target.Center, 0.1f, MathHelper.ToRadians(-90f));

                int Delay = 64;
                if (npc.ai[0] < Delay)
                {
                    npc.VelocityMoveTowardsPosition(GroundedTargetPosition(npc) + new Vector2((float)Math.Sin(Timer / 20f) * 50f, -50 + ((float)Math.Sin(Timer / 17f) * 30f)) + VectorTarget, 0.1f, 0.1f);
                }
                else
                {
                    if (npc.ai[0] == Delay && Phase > 0)
                    {
                        EmitRoar(npc);
                    }
                    if (npc.ai[0] < Delay + 32)
                    {
                        TendrilDamageEnabled = true;
                        if (npc.ai[0] % 8 >= 2 && npc.ai[0] % 15 < 9)
                        {
                            int i = 0;
                            if (npc.ai[0] >= Delay + 8) i = 1;
                            if (npc.ai[0] >= Delay + 16) i = 2;
                            if (npc.ai[0] >= Delay + 24) i = 3;

                            MoveTendril(npc, i, Target.Center + new Vector2(Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5)), 1f);

                            float DashSpeed = Phase > 0 ? 13 : 9;

                            if (npc.ai[0] % 8 == 2)
                            {
                                SoundEngine.PlaySound(Assets.Sounds.NPC.EoCStab.Asset, TendrilClawPositions[i]);
                                npc.velocity += (npc.DirectionTo(Target.Center) * DashSpeed).RotatedBy(Main.rand.NextFloat(MathHelper.ToRadians(-10), MathHelper.ToRadians(10)));
                            }
                        }
                        npc.velocity *= 0.8f;
                    }
                    else
                    {
                        TendrilDamageEnabled = false;
                        npc.velocity *= 0.94f;
                    }
                    if (npc.ai[0] >= Delay + 48)
                    {
                        ChangeState(npc, Main.rand.NextBool() ? AttackState.Bleed : AttackState.Multiply);
                    }
                }

                break;
            case AttackState.Swipe:
                break;
            case AttackState.Pinch:
                break;
        }
        Roar *= 0.95f;
        Shake *= 0.9f;

        if (TendrilDamageEnabled)
        {
            for (int i = 0; i < 4; i++)
            {
                DamageWithTendril(npc, i);
            }
        }
    }
    public override void OnKill(NPC npc)
    {
    }
    public Vector2 PupilPosition(NPC npc)
    {
        return npc.Center + new Vector2(60, 0).RotatedBy(npc.rotation + MathHelper.PiOver2);
    }
    public void ChangeState(NPC npc, AttackState state)
    {
        npc.ai[0] = 0;
        npc.ai[2] = (int)state;
        if (npc.life < DesperationThreshold)
            npc.ai[2] = (int)AttackState.Bash;
        npc.netUpdate = true;
    }
    public float EyeDilation = 0f;

    float FrameNum = 0;
    float PupilPaletteNum = 1;
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Vector2 offset = new Vector2(Main.rand.NextFloat(-Shake, Shake), 0).RotatedByRandom(MathHelper.TwoPi);

        if (npc.IsABestiaryIconDummy)
        {
            MoveTendrilsIdle(npc);
            drawColor = Color.White;
            EyeDilation = MathHelper.Lerp(EyeDilation, 0.3f + (float)(Math.Sin(Timer / 10f) * 0.05f), 0.2f);
        }
        else
            drawColor = Lighting.GetColor((npc.Top / 16).ToPoint());

        FrameNum += 0.2f;
        FrameNum = FrameNum % 5;

        Asset<Texture2D> EyeDraw = Assets.Textures.EyeOfCthulhu.EyeOfCthulhu_Main.Asset;
        if (Phase > 0)
        {
            EyeDraw = Assets.Textures.EyeOfCthulhu.EyeOfCthulhuPhase2.Asset;
            if (Roar > 0.1f) EyeDraw = Assets.Textures.EyeOfCthulhu.EyeOfCthulhuRoaring.Asset;
        }
        Asset<Texture2D> TendrilDraw = Assets.Textures.EyeOfCthulhu.EyeOfCthulhuTendril.Asset;
        Vector2 TendrilOrigin = new Vector2(6, 2);
        Asset<Texture2D> ClawDraw = Assets.Textures.EyeOfCthulhu.EyeOfCthulhuClaw.Asset;
        Vector2 ClawOrigin = new Vector2(12, 2);
        Asset<Texture2D> EyePupilDraw = Assets.Textures.EyeOfCthulhu.EyeOfCthulhu_PupilMask.Asset;

        if (TendrilsOut > 0)
        {
            for (int i = 0; i < TendrilsOut; i++)
            {
                SpriteEffects eff = SpriteEffects.None;
                if (i == 0 || i == 2)
                    eff = SpriteEffects.FlipHorizontally;

                spriteBatch.Draw(TendrilDraw.Value, TendrilPositions[i] - screenPos + offset, TendrilDraw.Frame(), drawColor, TendrilPositions[i].AngleTo(TendrilJointPositions[i]) - MathHelper.PiOver2, TendrilOrigin, 1f, eff, 0f);
                spriteBatch.Draw(TendrilDraw.Value, TendrilJointPositions[i] - screenPos + offset, TendrilDraw.Frame(), drawColor, TendrilJointPositions[i].AngleTo(TendrilClawPositions[i]) - MathHelper.PiOver2, TendrilOrigin, 1f, eff, 0f);
                spriteBatch.Draw(ClawDraw.Value, TendrilClawPositions[i] - screenPos + offset, ClawDraw.Frame(), drawColor, npc.rotation, ClawOrigin, 1f, eff, 0f);
            }
        }

        Rectangle Frame = EyeDraw.Frame(5, 1, (int)Math.Floor(FrameNum), 0);
        if (Phase > 0 && Roar > 0.1f)
        {
            Frame = EyeDraw.Frame(2, 1, (int)Math.Floor(FrameNum * 3f) % 2);
        }

        spriteBatch.Draw(EyeDraw.Value, npc.Center - screenPos + offset, Frame, drawColor, npc.rotation, (npc.frame.Size() / 2) + new Vector2(0, 22), 1f, SpriteEffects.None, 0f);

        if (Phase == 0)
        {
            var PupilEffect = Assets.Effects.EyeOfCthulhu.EyeOfCthulhuPupil.CreatePupilEffect();
            PupilEffect.Parameters.IrisThreshold = 0.33f + (EyeDilation * 0.22f);
            PupilEffect.Parameters.PupilThreshold = 0.66f + (EyeDilation * 0.22f);
            var PupilPalette = Assets.Textures.EyeOfCthulhu.PupilPalette1.Asset.Value;
            if (PupilPaletteNum == 1) PupilPalette = Assets.Textures.EyeOfCthulhu.PupilPalette2.Asset.Value;
            if (PupilPaletteNum == 2) PupilPalette = Assets.Textures.EyeOfCthulhu.PupilPalette3.Asset.Value;
            PupilEffect.Parameters.PupilGradient = PupilPalette;
            PupilEffect.Parameters.LightingColor = drawColor.ToVector4();
            PupilEffect.Apply();

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, PupilEffect.Shader, Main.GameViewMatrix.ZoomMatrix);

            spriteBatch.Draw(EyePupilDraw.Value, npc.Center - screenPos + offset, EyePupilDraw.Frame(), Color.White, npc.rotation, (npc.frame.Size() / 2) + new Vector2(0, 22), 1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.ZoomMatrix);

        }
        return false;
    }
    public void EmitRoar(NPC npc)
    {
        Roar = 1f;
        SoundEngine.PlaySound(Assets.Sounds.NPC.EnhancedEoCRoar.Asset, npc.Center);
    }
    public void OnHit(NPC npc, int damage)
    {

    }
    // On hit effects
    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        OnHit(npc, damageDone);
        base.OnHitByProjectile(npc, projectile, hit, damageDone);
    }
    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        OnHit(npc, damageDone);
        base.OnHitByItem(npc, player, item, hit, damageDone);
    }

    public bool SolidTileOrPlatformBeneath(NPC npc, Tile tile)
    {
        bool b1 = WorldGen.SolidOrSlopedTile(tile) || Main.tileSolidTop[tile.type];
        return b1 && npc.Bottom.Y > Main.player[npc.target].Top.Y;
    }
    public void MoveTendrilsIdle(NPC npc)
    {
        if (TendrilsOut > 0)
            MoveTendril(npc, 0, npc.Center + new Vector2(-110, 20).RotatedBy(npc.rotation + TendrilSplay));
        if (TendrilsOut > 1)
            MoveTendril(npc, 1, npc.Center + new Vector2(110, 20).RotatedBy(npc.rotation - TendrilSplay));
        if (TendrilsOut > 2)
            MoveTendril(npc, 2, npc.Center + new Vector2(-100, -80).RotatedBy(npc.rotation + TendrilSplay));
        if (TendrilsOut > 3)
            MoveTendril(npc, 3, npc.Center + new Vector2(100, -80).RotatedBy(npc.rotation - TendrilSplay));
    }
    // IK lookalike to move the tendrils
    public void MoveTendril(NPC npc, int i, Vector2 position, float lerp = 0.2f)
    {
        TendrilPositions[i] = npc.Center + new Vector2(0, -20).RotatedBy(npc.rotation) + (npc.DirectionTo(position) * 20);

        TendrilClawPositions[i] = Vector2.Lerp(TendrilClawPositions[i], position, lerp * 0.7f);
        TendrilJointPositions[i] = Vector2.Lerp(TendrilJointPositions[i], Vector2.Lerp(TendrilPositions[i], TendrilClawPositions[i], 0.7f) + new Vector2(0, -80f).RotatedBy(npc.rotation), lerp);

        TendrilJointPositions[i] = TendrilPositions[i] + (TendrilPositions[i].DirectionTo(TendrilJointPositions[i]) * 74);
        TendrilClawPositions[i] = TendrilJointPositions[i] + (TendrilJointPositions[i].DirectionTo(TendrilClawPositions[i]) * 74);
    }
    public void BleedInDirection(NPC npc, Vector2 targetPos)
    {
        for (float j = -6f; j <= 6f; j += 2)
        {
            CthulhuBloodParticle part = new CthulhuBloodParticle(npc.Center + new Vector2(55, 0).RotatedBy(npc.AngleTo(targetPos)), new Vector2(2, 0).RotatedBy(npc.AngleTo(targetPos) + MathHelper.ToRadians(j * Main.rand.NextFloat(15f, 20f))))
            {
                Scale = new Vector2(MathHelper.Lerp(0.7f, 0.2f, Math.Abs(j) / 5f), MathHelper.Lerp(2f, 1f, Math.Abs(j) / 5f))
            };
            part.Spawn();

        }

        CthulhuBloodRingParticle part2 = new CthulhuBloodRingParticle(npc.Center + new Vector2(55, 0).RotatedBy(npc.AngleTo(targetPos)), new Vector2(2, 0).RotatedBy(npc.AngleTo(targetPos) + MathHelper.Pi))
        {
            Scale = new Vector2(1.4f, 1.2f)
        };
        part2.Spawn();
    }

    public void UnleashTendril(NPC npc)
    {
        int i = (int)TendrilsOut;
        TendrilPositions[i] = TendrilJointPositions[i] = TendrilClawPositions[i] =
            npc.Center + new Vector2(i == 0 || i == 2 ? -50 : 50, i > 1 ? -30 : 10).RotatedBy(npc.rotation);

        BleedInDirection(npc, TendrilPositions[i]);

        BleedAllOver(npc);

        ScreenEffects.AddScreenShake(npc.Center, 10f, 0.6f);

        SoundEngine.PlaySound(Assets.Sounds.NPC.EoCTendrilEmerge.Asset with { MaxInstances = 4 }, TendrilPositions[i]);
        npc.velocity += npc.DirectionFrom(TendrilPositions[i]) * 6f;
        TendrilsOut++;
    }

    public void DamageWithTendril(NPC npc, int tendrilNumber)
    {
        foreach (Player player in Main.ActivePlayers)
        {
            if (player.Distance(TendrilClawPositions[tendrilNumber]) < 40)
            {
                if (!player.immune)
                {
                    int damage = Phase > 0 ? Phase2TendrilDamage : TendrilDamage;

                    player.Hurt(PlayerDeathReason.ByNPC(npc.whoAmI), damage, Math.Sign(player.Center.X - npc.Center.X), knockback: 3.5f);
                }
            }
        }
    }

    public void BleedAllOver(NPC npc)
    {
        for (int j = 0; j < 75; j++)
        {
            Vector2 v = npc.Center + new Vector2(Main.rand.Next(80), 0).RotatedByRandom(MathHelper.TwoPi);
            Dust.NewDustDirect(v, 5, 5, DustID.Blood, v.DirectionFrom(npc.Center).X, v.DirectionFrom(npc.Center).Y, Scale: 1.3f);
        }
    }

    public Vector2 GroundedTargetPosition(NPC npc)
    {
        return Main.player[npc.target].Center.Grounded();
    }

    public void ThrowTileReplicants(NPC npc, Point pt)
    {
        for (int i = -4; i <= 6; i++)
        {
            for (int j = -3; j <= 3; j++)
            {
                Vector2 position = new Vector2((pt.X + i) * 16, (pt.Y + j) * 16);
                Tile t = Main.tile[pt.X + i, pt.Y + j];
                if (WorldGen.SolidOrSlopedTile(t))
                {
                    WorldGen.KillTile_MakeTileDust(pt.X + i, pt.Y + j, t);
                    if (!WorldGen.SolidOrSlopedTile(Main.tile[pt.X + i, pt.Y + j - 1]))
                    {
                        Vector2 altVel = npc.velocity;
                        altVel.Normalize();
                        TileReplicantParticle tile = new TileReplicantParticle(t.TileType, new Rectangle(t.TileFrameX, t.TileFrameY, 16, 16), position + new Vector2(8, 8), new Vector2(0, -4f + (Math.Abs((float)i / 3f))), Vector2.One, P =>
                        {
                            (P as TileReplicantParticle).HideTimer--;
                            if ((P as TileReplicantParticle).HideTimer > 0) P.position -= P.velocity;
                            else
                            {
                                P.velocity.Y += 0.4f;
                                if (P.position.Y > (P as TileReplicantParticle).StartingY) P.Kill();
                            }
                        })
                        {
                            HideTimer = Math.Abs(i) * 5,
                            StartingY = position.Y + 8
                        };
                        tile.Spawn();
                    }
                }
            }
        }
    }
}

public class EyeOfCthulhuTendrilHitbox : ModProjectile
{
    public override string Texture => "Everware/Textures/SkewedRadialBlast";

    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.width = Projectile.height = 35;
        Projectile.hostile = true;
    }

    public override void AI()
    {
        if (Main.npc[(int)Projectile.ai[0]].active)
        {
            Projectile.timeLeft = 3;

            NPC npc = Main.npc[(int)Projectile.ai[0]];

            if (npc.TryGetGlobalNPC<EyeOfCthulhu>(out EyeOfCthulhu EoC))
            {
                Projectile.Center = EoC.TendrilClawPositions[(int)Projectile.ai[1]];

                Projectile.damage = EoC.TendrilDamageEnabled ? (EoC.Phase > 0 ? EyeOfCthulhu.TendrilDamage : EyeOfCthulhu.TendrilDamage) : 0;

            }
        }
        base.AI();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}