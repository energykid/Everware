namespace Everware.Content.Gallery.Snapdragon;

[AutoloadBossHead]
public class Snapdragon : ModNPC
{
    public override string Texture => "Everware/Assets/Textures/Gallery/Snapdragon/Snapdragon_Head";
    public override string BossHeadTexture => "Everware/Assets/Textures/Gallery/Snapdragon/Snapdragon_Head_Boss";
    public static int DefaultContactDamage => 45;
    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.life = NPC.lifeMax = 25000;
        NPC.defense = 35;
        NPC.knockBackResist = 0f;
        NPC.damage = 0;
        NPC.width = 120; NPC.height = 190;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.boss = true;
    }
    public override void SetStaticDefaults()
    {

    }
}
