using UnityEngine;

public class GameCamera : MonoBehaviour
{

    [SerializeField]
    private float m_moveSpeed = 1f;

    private void Update()
    {
        var delta = InputUtility.GetDrag() * m_moveSpeed * Time.deltaTime;
        transform.Translate(delta, Space.Self);
    }

}
