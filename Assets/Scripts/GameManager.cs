using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CircularList<T>
{

    private readonly T[] m_items = null;
    private int m_addIndex = 0;

    public int Length => m_items.Length;

    public T this[int index]
    {
        get { return m_items[index]; }
    }

    public CircularList(int size)
    {
        m_items = new T[size];
    }

    public void Add(T item)
    {
        m_items[m_addIndex] = item;
        m_addIndex++;

        if (m_addIndex > m_items.Length - 1)
        {
            m_addIndex = 0;
        }
    }

}

public struct GameEvent
{
    public static long EventID = 0;

    public GameEvent(GameEventType type)
    {
        EventType = type;
        Position = Vector3.zero;
        ID = Interlocked.Increment(ref EventID);
    }

    public GameEventType EventType;
    public Vector3 Position;
    public long ID;
}

public enum GameEventType
{
    None,
    Bell,
    Food
}

public class GameManager : MonoBehaviour, IInteractionSource
{
    // TJS: (SHOULD FIND THE REAL MATH FOR THIS) For making sure the blob stays level with the floor when going towards the bell or other interactives.
    private const float HEIGHT_OFFSET = 0.6f;

    // TJS: Interactive layer that raycasts can hit
    private const int m_layerMask = 1 << 8;
    private readonly RaycastHit[] m_raycastHits = new RaycastHit[1];
    private readonly Plane m_plane = new Plane(Vector3.up, new Vector3(0f, 0.6f, 0f));
    private bool m_isStarted = false;
    private CircularList<GameEvent> m_events = new CircularList<GameEvent>(50);
    private readonly Dictionary<string, long> m_eventClientToIndexMap = new Dictionary<string, long>();

    private InteractiveObject m_interactiveObject = null;

    private void Start()
    {
        m_isStarted = true;
    }

    public IEnumerator WaitForStart()
    {
        while (!m_isStarted)
        {
            yield return null;
        }
    }

    public GameEvent[] CheckEvents(string id)
    {
        if (!m_eventClientToIndexMap.TryGetValue(id, out long readID))
        {
            m_eventClientToIndexMap[id] = 0;
            readID = 0;
        }

        var newEvents = new List<GameEvent>();
        for (int i = 0; i < m_events.Length; i++)
        {
            var current = m_events[i];
            if (current.ID <= readID)
            {
                continue;
            }

            newEvents.Add(current);
            readID = current.ID;
        }

        m_eventClientToIndexMap[id] = readID;

        return newEvents.ToArray();
    }

    private void RegisterEvent(GameEventType gameEventType, Vector3 position)
    {
        m_events.Add(
                   new GameEvent(gameEventType)
                   {
                       Position = position
                   }
               );
    }

    private void Update()
    {
        if (InputUtility.DidTouchBegin())
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            m_plane.Raycast(ray, out float enter);

            var intersect = ray.GetPoint(enter);
            Debug.DrawLine(ray.origin, intersect, Color.green, 15f);

            if (Physics.RaycastNonAlloc(ray, m_raycastHits, Mathf.Infinity, m_layerMask) > 0)
            {
                m_interactiveObject = m_raycastHits[0].collider.GetComponent<InteractiveObject>();
                var raycastHitLocation = m_raycastHits[0].collider.bounds.min;
                Vector3 position = new Vector3(raycastHitLocation.x, HEIGHT_OFFSET, raycastHitLocation.z);
                RegisterEvent(m_interactiveObject.gameEventType, position);
            }

            m_interactiveObject?.BeginInteraction(this);
        }
    }


    #region Interface Methods

    public void OnInteractionBegin(InteractiveObject interactive)
    {
        m_interactiveObject = interactive;
    }

    public void OnInteractionEnd(InteractiveObject interactive)
    {
        if (m_interactiveObject == interactive)
        {
            m_interactiveObject = null;
        }
    }

    #endregion
}
