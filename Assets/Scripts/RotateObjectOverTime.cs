using UnityEngine;

public class RotateObjectOverTime : MonoBehaviour
{
    [SerializeField]
    private float m_rotateSpeed = 100f;

    [SerializeField]
    private float m_oscillateIntensity = 0.2f;

    private Vector3 m_startPosition = Vector3.zero;

    private void Update()
    {
        transform.Rotate(0f, Time.deltaTime * m_rotateSpeed, 0f, Space.Self);
        transform.localPosition = m_startPosition + new Vector3(0f, Mathf.Sin(Time.time) * m_oscillateIntensity, 0f);
    }
}
