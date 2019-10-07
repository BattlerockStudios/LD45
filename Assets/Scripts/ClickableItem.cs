using UnityEngine;

public class ClickableItem : InteractiveObject
{  
    public override void BeginInteraction(IInteractionSource interactionSource)
    {
        base.BeginInteraction(interactionSource);

        _audioSource.PlayOneShot(_audioSource.clip);
    }
}