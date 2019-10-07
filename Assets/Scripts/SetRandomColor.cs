using UnityEngine;
using System.Collections;
using System;

public class SetRandomColor : MonoBehaviour 
{
    [SerializeField]
    private float m_hueMin = 0f;
    [SerializeField]
    private float m_hueMax = 1f;
    [SerializeField]
    private float m_saturationMin = 1f;
    [SerializeField]
    private float m_saturationMax = 1f;
    [SerializeField]
    private float m_valueMin = 0.5f;
    [SerializeField]
    private float m_valueMax = 1f;


    // Initializes things
    void Start () 
	{
        var renderer = GetComponent<Renderer>() ?? throw new NullReferenceException($"{nameof(SetRandomColor)}: {nameof(Renderer)} not found!");
        renderer.material.color = UnityEngine.Random.ColorHSV(m_hueMin, m_hueMax, m_saturationMin, m_saturationMax, m_valueMin, m_valueMax);
    }
}
