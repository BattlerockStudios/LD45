using System;
using UnityEngine;

public class FruitTree : InteractiveObject
{
    [SerializeField]
    private GameObject m_fruitPrefab;

    private readonly StateMachine m_stateMachine = new StateMachine();

    [SerializeField]
    private GameObject[] m_availableFruits = null;

    private Vector3[] m_initialFruitPositions = null;
    private Quaternion[] m_initialFruitRotations = null;

    private bool m_alreadyInitialized = false;

    private RespawnFruitState m_respawnFruitState = null;

    public bool AreFruitsAvailable
    {
        get
        {
            var areFruitsAvailable = m_availableFruits != null && m_availableFruits.Length > 0;

            if (areFruitsAvailable == false)
            {
                Debug.LogWarning("WARNING! NO FRUITS ARE AVAILABLE!");
            }

            return areFruitsAvailable;
        }
    }

    private void Initialize()
    {
        KeepTrackOfFruitInitialPositions();
        m_respawnFruitState = new RespawnFruitState(m_availableFruits.Length, m_fruitPrefab, m_initialFruitPositions, m_initialFruitRotations, transform);
        m_stateMachine.AddState(m_respawnFruitState);
        m_alreadyInitialized = true;
    }

    private void Update()
    {
        m_stateMachine.Update();

        if (m_availableFruits == null)
        {
            m_availableFruits = m_respawnFruitState.newlyAvailableFruits;
        }
    }

    public override void BeginInteraction(IInteractionSource interactionSource)
    {
        base.BeginInteraction(interactionSource);
        // TODO: Add SFX, make fruit fall off trees

        ShakeTree();
    }

    private void ShakeTree()
    {   
        if (AreFruitsAvailable == true)
        {
            if (m_alreadyInitialized == false)
            {
                Initialize();
            }

            for (int i = 0; i < m_availableFruits.Length; i++)
            {
                var rigidbody = m_availableFruits[i].GetComponent<Rigidbody>();
                rigidbody.isKinematic = false;
                m_availableFruits[i].layer = LayerMask.NameToLayer("Interactable");
                m_availableFruits[i].transform.parent = null;
            }

            m_availableFruits = null;
            m_stateMachine.Start(nameof(RespawnFruitState));
        }
    }

    private void KeepTrackOfFruitInitialPositions()
    {
        if (AreFruitsAvailable)
        {
            m_initialFruitPositions = new Vector3[m_availableFruits.Length];
            m_initialFruitRotations = new Quaternion[m_availableFruits.Length];

            for (int i = 0; i < m_initialFruitPositions.Length; i++)
            {
                m_initialFruitPositions[i] = m_availableFruits[i].transform.position;
                m_initialFruitRotations[i] = m_availableFruits[i].transform.rotation;
            }
        }
    }

    private class RespawnFruitState : AbstractState
    {
        private DateTime m_exitTime = DateTime.MinValue;

        private readonly int m_fruitCount = 0;
        private readonly GameObject m_fruitPrefab = null;
        private readonly Vector3[] m_positions = null;
        private readonly Quaternion[] m_rotations = null;

        private readonly Transform m_parent = null;

        public GameObject[] newlyAvailableFruits;

        public RespawnFruitState(int fruitCount, GameObject fruitPrefab,
            Vector3[] positions, Quaternion[] rotations, Transform parent)
            : base(nameof(RespawnFruitState))
        {
            m_fruitCount = fruitCount;
            m_fruitPrefab = fruitPrefab;
            m_positions = positions;
            m_rotations = rotations;
            m_parent = parent;
            newlyAvailableFruits = new GameObject[fruitCount];
        }

        private void FruitRespawn()
        {
            for (int i = 0; i < m_fruitCount; i++)
            {
                newlyAvailableFruits[i] = Instantiate(m_fruitPrefab, m_positions[i], m_rotations[i], m_parent);
            }
        }

        protected override void OnEnter()
        {
            m_exitTime = DateTime.UtcNow.AddSeconds(UnityEngine.Random.Range(30, 60));
        }

        protected override void OnExit()
        {
            // Set it to max to stop 
            m_exitTime = DateTime.MaxValue;
        }

        protected override void OnUpdate()
        {
            // ZAS: If our random timer is expired, do something else
            if (DateTime.UtcNow > m_exitTime)
            {
                FruitRespawn();
                OnExit();
                return;
            }
        }
    }
}

