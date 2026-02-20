using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Everware.Common.Systems;

public class InstancedRequestableTarget : INeedRenderTargetContent
{
    private bool isReady;

    private readonly List<InstancedTargetData> targetInstances = [];

    private readonly List<int> requestedInstanceIdentifiers = [];

    public bool IsReady => isReady;

    /// <summary>
    ///     Whether any render target instances are held in this requestable target.
    /// </summary>
    public bool AnyTargetsAllocated => targetInstances.Count >= 1;

    public void PrepareRenderTarget(GraphicsDevice device, SpriteBatch spriteBatch)
    {
        if (requestedInstanceIdentifiers.Count <= 0 || targetInstances.Count <= 0)
            return;

        isReady = false;

        for (int i = 0; i < targetInstances.Count; i++)
        {
            InstancedTargetData instance = targetInstances[i];
            if (!requestedInstanceIdentifiers.Contains(instance.Identifier))
                continue;

            device.SetRenderTarget(instance.Target);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1f, 0);
            instance.DrawAction();
        }

        // Return to the backbuffer.
        device.SetRenderTarget(null);

        // Reset the set of requested instances.
        requestedInstanceIdentifiers.Clear();

        isReady = true;
    }

    public void Request(int width, int height, int identifier, Action drawAction)
    {
        InstancedTargetData existingInstance = targetInstances.FirstOrDefault(n => n.Identifier == identifier);
        if (existingInstance is null)
            targetInstances.Add(new InstancedTargetData(drawAction, width, height, identifier));

        // Ensure that the requested target dimensions are valid.
        // If they aren't, reset them.
        else if (existingInstance.Target.Width != width || existingInstance.Target.Height != height)
        {
            existingInstance.Dispose();
            targetInstances.Remove(existingInstance);
            targetInstances.Add(new InstancedTargetData(drawAction, width, height, identifier));
        }

        if (existingInstance is not null)
            existingInstance.DrawAction = drawAction;

        if (!requestedInstanceIdentifiers.Contains(identifier))
            requestedInstanceIdentifiers.Add(identifier);
    }

    /// <summary>
    ///     Attempts to acquire a given target with a given identifier, returning whether it is ready or not.
    /// </summary>
    /// <param name="identifier">The identifier to search for.</param>
    public bool TryGetTarget(int identifier, [NotNullWhen(true)] out RenderTarget2D target)
    {
        target = null;
        for (int i = 0; i < targetInstances.Count; i++)
        {
            if (targetInstances[i] is not null && targetInstances[i].Identifier == identifier)
            {
                target = targetInstances[i].Target;
                break;
            }
        }

        return target is not null;
    }

    /// <summary>
    ///     Attempts to clear this target, disposing unmanaged data for all of its target sub-instances.
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < targetInstances.Count; i++)
            targetInstances[i].Dispose();
        targetInstances.Clear();

        isReady = false;
        requestedInstanceIdentifiers.Clear();
    }
}
