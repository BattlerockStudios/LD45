using UnityEngine;

public class ClickableItem : MonoBehaviour
{

    [SerializeField]
    private float m_rotateSpeed = 100f;

    [SerializeField]
    private float m_oscillateIntensity = 0.2f;

    [SerializeField]
    private Transform m_itemContainer = null;

    private Vector3 m_containerPositionAtStart = Vector3.zero;

    private void Start()
    {
        m_containerPositionAtStart = m_itemContainer.localPosition;
    }

    private void Update()
    {
        m_itemContainer.Rotate(0f, Time.deltaTime * m_rotateSpeed, 0f, Space.Self);
        m_itemContainer.localPosition = m_containerPositionAtStart + new Vector3(0f, Mathf.Sin(Time.time) * m_oscillateIntensity, 0f);
    }

}
