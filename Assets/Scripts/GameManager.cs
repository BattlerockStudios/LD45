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
    Bell
}

public class GameManager : MonoBehaviour
{

    private readonly Plane m_plane = new Plane(Vector3.up, Vector3.zero);
    private bool m_isStarted = false;
    private CircularList<GameEvent> m_events = new CircularList<GameEvent>(50);
    private readonly Dictionary<string, long> m_eventClientToIndexMap = new Dictionary<string, long>();

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

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                var ray = Camera.main.ScreenPointToRay(touch.position);

                m_plane.Raycast(ray, out float enter);

                var intersect = ray.GetPoint(enter);
                Debug.DrawLine(ray.origin, intersect, Color.green, .5f);

                m_events.Add(
                    new GameEvent(GameEventType.Bell)
                    {
                        Position = intersect
                    }
                );
            }
        }
    }

}
