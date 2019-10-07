using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class InteractiveObject : MonoBehaviour
{
    public GameEventType gameEventType = GameEventType.None;

    protected Guid m_uniqueID = Guid.NewGuid();

    protected AudioSource _audioSource;

    public bool IsBeingInteractedWith
    {
        get { return m_interactionSource != null; }
    }

    protected IInteractionSource m_interactionSource = null;

    public virtual void BeginInteraction(IInteractionSource interactionSource)
    {
        m_interactionSource = interactionSource;
        m_interactionSource.OnInteractionBegin(this);
    }

    public void OnDeselect()
    {  
        Debug.Log($"{name} is deselected!");
    }

    public void OnSelect()
    {     
        Debug.Log($"{name} is selected!");
    }

    public void ReleaseInteraction()
    {
        m_interactionSource?.OnInteractionEnd(this);
        m_interactionSource = null;
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
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