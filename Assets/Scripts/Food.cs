using UnityEngine;

public class Food : InteractiveObject
{
    [SerializeField]
    private int decreaseHunger = 10;

    public int DecreaseHunger { get => decreaseHunger; }

    public override void BeginInteraction(IInteractionSource interactionSource)
    {
        base.BeginInteraction(interactionSource);
        // TODO: Add Bell SFX, add game event to change the blob state to walk towards the bell

    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            var creature = collision.gameObject.GetComponentInParent<Creature>();
            var hungerLevel = creature.Stats.HungerLevel;
            creature.Stats.SetHungerLevel(hungerLevel - decreaseHunger);
            Destroy(gameObject);
        }
    }
}