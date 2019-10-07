using System;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    protected Guid m_uniqueID = Guid.NewGuid();

    public bool IsBeingInteractedWith
    {
        get { return m_interactionSource != null; }
    }

    public Material SelectedMaterial = null;
    public Material DeselectedMaterial = null;
    public Renderer Renderer = null;

    protected IInteractionSource m_interactionSource = null;

    public virtual void BeginInteraction(IInteractionSource interactionSource)
    {
        m_interactionSource = interactionSource;
        m_interactionSource.OnInteractionBegin(this);
    }

    public void OnDeselect()
    {
        Renderer.material = DeselectedMaterial;

        Debug.Log($"{name} is deselected!");
    }

    public void OnSelect()
    {
        DeselectedMaterial = Renderer.material;
        Renderer.material = SelectedMaterial;

        Debug.Log($"{name} is selected!");
    }

    public void ReleaseInteraction()
    {
        m_interactionSource?.OnInteractionEnd(this);
        m_interactionSource = null;
    }

    private void Update()
    {
        InteractionUpdate(IsBeingInteractedWith);
    }

    protected virtual void InteractionUpdate(bool isBeingInteractedWith)
    {

    }

}

public interface IInteractionSource
{
    void OnInteractionBegin(InteractiveObject interactive);
    void OnInteractionEnd(InteractiveObject interactive);
}