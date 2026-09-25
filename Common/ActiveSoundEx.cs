using MonoMod.Cil;
using ReLogic.Utilities;
using GoldMeridian.CodeAnalysis;

namespace Everware.Common;

[ExtensionDataFor<ActiveSound>]
internal sealed class ActiveSoundData
{
    public required float AttenuationDistance { get; set; }
}

file static class ActiveSoundDataBehavior
{
    // PORTING: https://github.com/swirlier-ink/rosemary/blob/main/src/Rosemary/Common/SoundStyleEx.cs
    [OnLoad]
    private static void Load()
    {
        IL_ActiveSound.Update += Update_AttenuationDistance;
    }

    private static void Update_AttenuationDistance(ILContext il)
    {
        var c = new ILCursor(il);

        var selfIndex = -1;

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdarg(out selfIndex)
        );

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdcR4(2500f)
        );

        c.EmitLdarg(selfIndex);
        c.EmitDelegate(
            static (float dist, ActiveSound sound) =>
            {
                if (sound.Data is null)
                {
                    return dist;
                }

                return sound.Data.AttenuationDistance;
            }
        );
    }
}

public static class SoundExtensions
{
    extension(SoundEngine)
    {
        public static SlotId PlaySound(in SoundStyle style, Vector2? position = null, SoundUpdateCallback? updateCallback = null, float attenuationDistance = 2500f)
        {
            if (!Program.IsMainThread)
            {
                var styleCopy = style;
                return Main.RunOnMainThread(() => SoundEngine.PlaySound(in styleCopy, position, updateCallback, attenuationDistance)).GetAwaiter().GetResult();
            }

            var slot = SoundEngine.PlaySound(in style, attenuationDistance >= 10000f ? null : position, updateCallback);

            if (!SoundEngine.TryGetActiveSound(slot, out var activeSound))
            {
                return slot;
            }

            activeSound.Position = position;

            activeSound.Data ??= new ActiveSoundData
            {
                AttenuationDistance = attenuationDistance,
            };

            activeSound.Data?.AttenuationDistance = attenuationDistance;

            return slot;
        }
    }

    extension(SoundStyle style)
    {
        public int FrameDuration => (int)(style.GetSoundEffect().Duration.TotalSeconds * 60f);
    }
}
