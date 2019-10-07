using UnityEngine;

public class Tile : MonoBehaviour
{

    [SerializeField]
    private Transform m_disabledObjectParent = null;

    private void Awake()
    {
        m_disabledObjectParent?.gameObject.SetActive(false);
    }

    public void Show()
    {
        m_disabledObjectParent?.gameObject.SetActive(true);
    }

}
