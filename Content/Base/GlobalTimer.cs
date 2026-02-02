namespace Everware.Content.Base;

public class GlobalTimer : ModSystem
{
    public static float Value = 0f;
    public override void PostUpdateDusts()
    {
        Value++;
    }
}
