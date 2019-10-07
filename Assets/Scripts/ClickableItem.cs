using UnityEngine;

public class ClickableItem : InteractiveObject
{  
    public override void BeginInteraction(IInteractionSource interactionSource)
    {
        base.BeginInteraction(interactionSource);
        // TODO: Add Bell SFX, add game event to change the blob state to walk towards the bell

    }
}