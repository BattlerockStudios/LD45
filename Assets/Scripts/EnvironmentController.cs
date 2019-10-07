using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{

    private const float NEGATIVE_DISTANCE = 50f;

    [SerializeField]
    private AnimationCurve m_curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [SerializeField]
    private Vector2 m_minBoundary = Vector2.zero;

    [SerializeField]
    private Vector2 m_maxBoundary = Vector2.zero;

    private readonly List<Transform> m_animatingList = new List<Transform>();
    private bool m_isStarted = false;
    private Bounds m_bounds = new Bounds();

    private void OnDrawGizmos()
    {
        var center = (m_minBoundary + m_maxBoundary) / 2f;

        Gizmos.DrawSphere(new Vector3(m_minBoundary.x, transform.position.y, m_minBoundary.y), .2f);
        Gizmos.DrawSphere(new Vector3(m_maxBoundary.x, transform.position.y, m_maxBoundary.y), .2f);
        Gizmos.DrawWireCube(new Vector3(center.x, transform.position.y, center.y), new Vector3(m_maxBoundary.x - m_minBoundary.x, 0f, m_maxBoundary.y - m_minBoundary.y));
    }

    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var childTransform = transform.GetChild(i);
            childTransform.position = new Vector3(childTransform.position.x, transform.position.y - NEGATIVE_DISTANCE, childTransform.position.z);
        }

        m_bounds.Encapsulate(new Vector3(m_minBoundary.x, transform.position.y, m_minBoundary.y));
        m_bounds.Encapsulate(new Vector3(m_maxBoundary.x, transform.position.y, m_maxBoundary.y));

        m_isStarted = true;
    }

    public IEnumerator WaitForStart()
    {
        while (!m_isStarted)
        {
            yield return null;
        }
    }

    public Vector3 GetClosesValidPoint(Vector3 point)
    {
        return m_bounds.ClosestPoint(point);
    }

    public void RevealPoint(Vector3 point)
    {
        point.y -= NEGATIVE_DISTANCE;

        var hitTransforms = Physics.OverlapSphere(point, 1f).Select(h => h.transform).Distinct();
        foreach (var hitTransform in hitTransforms)
        {
            // ZAS: We only want to consider direct children
            if (hitTransform.parent != transform)
            {
                continue;
            }

            if (m_animatingList.Contains(hitTransform))
            {
                continue;
            }

            AnimateUpAsync(hitTransform);
        }
    }

    public void RevealPath(Vector3 start, Vector3 end)
    {
        start.y -= NEGATIVE_DISTANCE;
        end.y -= NEGATIVE_DISTANCE;

        var hitTransforms = Physics.SphereCastAll(start, 1f, (end - start).normalized, (end - start).magnitude).Select(h => h.transform).Distinct();
        foreach (var hitTransform in hitTransforms)
        {
            // ZAS: We only want to consider direct children
            if (hitTransform.parent != transform)
            {
                continue;
            }

            if (m_animatingList.Contains(hitTransform))
            {
                continue;
            }

            AnimateUpAsync(hitTransform);
        }
    }

    private async Task AnimateUpAsync(Transform trans)
    {
        m_animatingList.Add(trans);

        var tiles = trans.GetComponentsInChildren<Tile>(includeInactive: true);
        if(tiles != null)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].Show();
            }
        }

        var startPos = trans.position;
        var endPos = startPos + (Vector3.up * NEGATIVE_DISTANCE);
        await AnimationUtility.AnimateOverTime(500, p => trans.position = Vector3.Lerp(startPos, endPos, m_curve.Evaluate(p)));

        m_animatingList.Remove(trans);
    }

}
