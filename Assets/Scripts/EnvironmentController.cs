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

    private readonly List<Transform> m_animatingList = new List<Transform>();
    private bool m_isStarted = false;

    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var childTransform = transform.GetChild(i);
            childTransform.position = new Vector3(childTransform.position.x, transform.position.y - NEGATIVE_DISTANCE, childTransform.position.z);
        }

        m_isStarted = true;
    }

    public IEnumerator WaitForStart()
    {
        while(!m_isStarted)
        {
            yield return null;
        }
    }

    public void RevealPoint(Vector3 point)
    {
        point.y -= NEGATIVE_DISTANCE;

        var hitTransforms = Physics.OverlapSphere(point, 1f).Select(h => h.transform).Distinct();
        foreach (var hitTransform in hitTransforms)
        {
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

        var startPos = trans.position;
        var endPos = startPos + (Vector3.up * NEGATIVE_DISTANCE);
        await AnimationUtility.AnimateOverTime(500, p => trans.position = Vector3.Lerp(startPos, endPos, m_curve.Evaluate(p)));

        m_animatingList.Remove(trans);
    }

}
