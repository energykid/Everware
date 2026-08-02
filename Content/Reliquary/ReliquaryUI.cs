using Everware.Content.Gallery.Sculptor;
using System.Collections.Generic;
using Terraria.GameInput;
using Terraria.UI;

namespace Everware.Content.Reliquary;

public class ReliquaryUISystem : ModSystem
{
    internal static UserInterface Interface;
    internal static ReliquaryUIState State;
    internal static SculptorTradeUIState TradeState;
    internal static NPC? Sculptor = null;

    public static bool ReliquaryOpenedFromInventory = false;

    public override void Load()
    {
        if (!Main.dedServ)
        {
            Interface = new UserInterface();
            State = new ReliquaryUIState();
            TradeState = new SculptorTradeUIState();
            State.Activate();
            TradeState.Activate();
        }
    }
    public override void Unload()
    {
        State = null;
        TradeState = null;
    }
    public override void PostUpdateInput()
    {
        if (Interface?.CurrentState == State)
        {
            PlayerInput.LockVanillaMouseScroll("Everware: Sculptor and Reliquary");
        }
    }
    public override void UpdateUI(GameTime gameTime)
    {
        if (Interface?.CurrentState != null)
        {
            Interface.Update(gameTime);

            if (!SculptorNearby(Main.LocalPlayer, out NPC guide, 350) && !ReliquaryOpenedFromInventory)
            {
                CloseUI();
            }
            else if (!Main.LocalPlayer.releaseInventory)
            {
                CloseUI();
            }
            Main.LocalPlayer.mouseInterface = true;
        }
    }
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "Everware: Sculptor and Reliquary",
                delegate
                {
                    if (Interface?.CurrentState != null)
                    {
                        Interface.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
                    }
                    return true;
                },
                InterfaceScaleType.UI));
        }
    }
    public override void PostUpdateEverything()
    {
        if (Sculptor != null && Interface.CurrentState == TradeState)
        {
            TradeState.Position = Sculptor.Center + new Vector2(0, -120);
            if (Main.LocalPlayer.talkNPC != -1)
                Main.CloseNPCChatOrSign();
        }
    }

    public static void OpenUI()
    {
        Main.CloseNPCChatOrSign();
        Interface.SetState(State);
    }
    public static void OpenTrade(NPC? sculptor = null)
    {
        if (sculptor == null)
        {
            State.Position = Main.ScreenSize.ToVector2() / 2f;
        }
        else
        {
            Sculptor = sculptor;
        }
        Main.playerInventory = true;
        Interface.SetState(TradeState);

        List<string> key = ["One", "Two", "Three"];
        TradeState.SetDialogue(Mods.Everware.NPCs.SculptorNPC.Dialogue.TradeGreeting.GetChildText(key[Main.rand.Next(key.Count)]).Value);
    }
    public static void CloseUI()
    {
        if (Sculptor != null)
        {
            if (Sculptor.ModNPC is SculptorNPC sculptor)
            {
                sculptor.Focused = false;
                Sculptor.netUpdate = true;
            }
        }
        Interface.SetState(null);
        ReliquaryOpenedFromInventory = false;
    }

    public bool SculptorNearby(Player player, out NPC sculptor, float dist)
    {
        sculptor = null;

        bool b = false;
        float distance = dist;
        foreach (NPC npc in Main.npc)
        {
            if (npc.type == ModContent.NPCType<SculptorNPC>() && npc.Distance(player.Center) < distance)
            {
                sculptor = npc;
                distance = npc.Distance(player.Center);
                b = true;
            }
        }

        return b;
    }
}