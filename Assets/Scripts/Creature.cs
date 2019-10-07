using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Controls the behaviour of our little creatures. Creature is driven by a state machine. To add behaviour to it, 
/// simply make a new state that inherits from AbstractState, and add it in <see cref="Start"/>
/// </summary>
public class Creature : MonoBehaviour
{

    private const string LAST_BELL = "lastBell";
    private const string HUNGRY = "hungry";

    private readonly StateMachine m_stateMachine = new StateMachine();
    private readonly Guid m_id = Guid.NewGuid();

    [SerializeField]
    private GameObject m_eggVisual = null;

    [SerializeField]
    private GameObject m_eggShellVisual = null;

    private GameObject m_creatureVisual = null;

    [SerializeField]
    private GameObject[] m_creatureVisualsArray = null;

    [SerializeField]
    private EmoteIcons m_emoteIcons = new EmoteIcons();

    [SerializeField]
    private Stats m_stats = new Stats();

    private EnvironmentController m_environmentController = null;
    private GameManager m_gameManager = null;

    private IEnumerator Start()
    {
        // ZAS: Yes, this is bad habit... but will work for now
        m_gameManager = Component.FindObjectOfType<GameManager>() ?? throw new NullReferenceException($"{nameof(GameManager)} not found");
        m_environmentController = Component.FindObjectOfType<EnvironmentController>() ?? throw new NullReferenceException($"{nameof(EnvironmentController)} not found!");

        yield return m_gameManager.WaitForStart();
        yield return m_environmentController.WaitForStart();

        m_creatureVisual = m_creatureVisualsArray != null && m_creatureVisualsArray.Length > 0 ? m_creatureVisualsArray[UnityEngine.Random.Range(0, m_creatureVisualsArray.Length)] : throw new NullReferenceException($"{nameof(m_creatureVisualsArray)} is null or empty!");

        m_emoteIcons.FillEmoteIconArray();
        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        m_stateMachine.AddState(new EggState(m_eggVisual, m_eggShellVisual, m_creatureVisual, m_emoteIcons.EmoteVisual, m_environmentController));
        m_stateMachine.AddState(new CreatureIdleState(m_emoteIcons.GetSpecificEmoteIconAfterDisablingAllEmoteIcons(m_emoteIcons.DotEmoteIcon)));
        m_stateMachine.AddState(new CreatureMoveState(transform, m_stats, m_emoteIcons.GetSpecificEmoteIconAfterDisablingAllEmoteIcons(m_emoteIcons.DotEmoteIcon), m_environmentController));
        m_stateMachine.AddState(new CreatureHungryState(m_emoteIcons.GetSpecificEmoteIconAfterDisablingAllEmoteIcons(m_emoteIcons.HungryIcon)));

        m_stateMachine.Start(nameof(EggState));
    }

    private void Update()
    {
        m_stateMachine.Update();

        var newEvents = m_gameManager.CheckEvents(m_id.ToString());
        for (int i = 0; i < newEvents.Length; i++)
        {
            HandleEvent(newEvents[i]);
        }
    }

    private void HandleEvent(GameEvent gameEvent)
    {
        switch (gameEvent.EventType)
        {
            case GameEventType.Bell:
                m_stateMachine.SetBlackboardValue(LAST_BELL, gameEvent.Position);
                break;
            case GameEventType.Food:
                m_stateMachine.SetBlackboardValue(HUNGRY, gameEvent.Position);
                break;
            default:
                Debug.LogError($"Unhandled event {gameEvent.EventType}");
                break;
        }
    }

    private class EggState : AbstractState
    {
        private DateTime m_exitTime = DateTime.MinValue;
        private readonly GameObject m_eggVisual = null;
        private readonly GameObject m_eggShellVisual = null;

        private readonly GameObject m_creatureVisual = null;
        private readonly GameObject m_emoteVisual = null;
        private readonly EnvironmentController m_environmentController = null;

        public EggState(GameObject eggVisual, GameObject eggShellVisual, GameObject creatureVisual, GameObject emoteVisual, EnvironmentController environmentController)
            : base(nameof(EggState))
        {
            m_eggVisual = eggVisual;
            m_eggShellVisual = eggShellVisual;
            m_emoteVisual = emoteVisual;
            m_creatureVisual = GameObject.Instantiate(creatureVisual, eggVisual.transform.parent);
            m_environmentController = environmentController;

            m_eggVisual.SetActive(true);
            m_emoteVisual.SetActive(false);
            m_eggShellVisual.SetActive(false);
            m_creatureVisual.SetActive(false);
        }

        protected override void OnEnter()
        {
            m_exitTime = DateTime.UtcNow.AddSeconds(UnityEngine.Random.Range(5, 10));
            m_environmentController.RevealPoint(m_eggVisual.transform.position);
        }

        protected override void OnExit()
        {
            m_exitTime = DateTime.MinValue;
        }

        protected override void OnUpdate()
        {
            if (DateTime.UtcNow > m_exitTime)
            {
                m_eggVisual.SetActive(false);
                m_creatureVisual.SetActive(true);
                m_eggShellVisual.SetActive(true);
                m_emoteVisual.SetActive(true);

                m_eggShellVisual.transform.parent = null;

                ExitToState(nameof(CreatureIdleState));
            }
        }
    }

    private class CreatureIdleState : AbstractState
    {
        private DateTime m_exitTime = DateTime.MinValue;
        private readonly GameObject m_emoteIcon = null;

        public CreatureIdleState(GameObject emoteIcon)
            : base(nameof(CreatureIdleState))
        {
            m_emoteIcon = emoteIcon;
        }

        protected override void OnEnter()
        {
            m_emoteIcon.SetActive(true);
            m_exitTime = DateTime.UtcNow.AddSeconds(UnityEngine.Random.Range(1, 2));
        }

        protected override void OnExit()
        {
            m_exitTime = DateTime.MinValue;
        }

        protected override void OnUpdate()
        {
            if (m_blackboardValues.ContainsKey(HUNGRY))
            {
                ExitToState(nameof(CreatureHungryState));
                return;
            }

            // ZAS: If there is a bell, then we want to get moving!
            if (m_blackboardValues.ContainsKey(LAST_BELL))
            {
                ExitToState(nameof(CreatureMoveState));
                return;
            }

            // ZAS: If our random timer is expired, do something else
            if (DateTime.UtcNow > m_exitTime)
            {
                ExitToState(nameof(CreatureMoveState));
                return;
            }
        }

    }

    private class CreatureMoveState : AbstractState
    {
        private readonly Transform m_creatureTransform = null;
        private readonly GameObject m_emoteIcon = null;
        private readonly Stats m_stats;
        private int m_hungerLevel = 0;
        private readonly EnvironmentController m_environmentController = null;

        public CreatureMoveState(Transform transform, Stats stats, GameObject emoteIcon, EnvironmentController environmentController)
          : base(nameof(CreatureMoveState))
        {
            m_creatureTransform = transform;
            m_emoteIcon = emoteIcon;
            m_stats = stats;
            m_hungerLevel = m_stats.HungerLevel;
            m_environmentController = environmentController;
        }

        protected override void OnEnter()
        {
            m_emoteIcon.SetActive(true);

            var targetPosition = Vector3.zero;
            if (m_blackboardValues.ContainsKey(LAST_BELL))
            {
                targetPosition = (Vector3)m_blackboardValues[LAST_BELL];

                // ZAS: We are consuming this bell event
                m_blackboardValues.Remove(LAST_BELL);
            }
            else
            {
                var randomInCircle = UnityEngine.Random.insideUnitCircle;
                targetPosition = m_creatureTransform.position + (new Vector3(randomInCircle.x, 0f, randomInCircle.y) * m_stats.MovementSpeed);
            }

            MoveToTargetAsync(targetPosition);
        }

        protected override void OnExit()
        {
            m_stats.SetHungerLevel(m_hungerLevel++);
        }

        protected override void OnUpdate()
        {
        }

        private async Task MoveToTargetAsync(Vector3 target)
        {
            await RotateToTargetAsync(target);
            await TranslateToTargetAsync(target);

            ExitToState(nameof(CreatureIdleState));
        }

        private async Task RotateToTargetAsync(Vector3 target)
        {
            var start = m_creatureTransform.rotation;
            var end = Quaternion.LookRotation(target - m_creatureTransform.position, Vector3.up);
            await AnimationUtility.AnimateOverTime(200, p => m_creatureTransform.rotation = Quaternion.Slerp(start, end, p));
        }

        private bool ShouldBreakMovement()
        {
            return m_blackboardValues.ContainsKey(LAST_BELL);
        }

        private async Task TranslateToTargetAsync(Vector3 target)
        {
            var start = m_creatureTransform.position;
            var vectorToTarget = (target - m_creatureTransform.position);
            var maxMagnitude = vectorToTarget.magnitude;
            var segments = Mathf.FloorToInt(maxMagnitude / m_stats.MovementSpeed);
            for (int i = 0; i < segments; i++)
            {
                if (ShouldBreakMovement())
                {
                    break;
                }

                var atStart = m_creatureTransform.position;
                var atEnd = atStart + (vectorToTarget.normalized * m_stats.MovementSpeed);
                await AnimationUtility.AnimateOverTime(
                    1000,
                    x =>
                    {
                        var t = -(4f * Mathf.Pow(x - .5f, 2)) + 1;

                        var poss = Vector3.Lerp(atStart, atEnd, x);
                        poss.y += t;

                        m_creatureTransform.position = poss;
                        m_environmentController.RevealPoint(poss);
                    }
                );

                m_creatureTransform.position = atEnd;

                await Task.Delay(500);
            }

            if (!ShouldBreakMovement())
            {
                var atStart2 = m_creatureTransform.position;
                var atEnd2 = target;
                await AnimationUtility.AnimateOverTime(
                    1000,
                    x =>
                    {
                        var t = -(4f * Mathf.Pow(x - .5f, 2)) + 1;

                        var poss = Vector3.Lerp(atStart2, atEnd2, x);
                        poss.y += t;

                        m_creatureTransform.position = poss;
                        m_environmentController.RevealPoint(poss);
                    }
                );

                m_creatureTransform.position = atEnd2;
            }
        }

    }

    private class CreatureHungryState : AbstractState
    {
        private readonly GameObject m_hungerIcon = null;
        private DateTime m_exitTime = DateTime.MinValue;

        public CreatureHungryState(GameObject hungerIcon)
            : base(nameof(CreatureHungryState))
        {
            m_hungerIcon = hungerIcon;
        }

        protected override void OnEnter()
        {
            m_hungerIcon.SetActive(true);
            m_exitTime = DateTime.UtcNow.AddSeconds(UnityEngine.Random.Range(1, 2));
        }

        protected override void OnExit()
        {
            m_hungerIcon.SetActive(false);
            m_exitTime = DateTime.MinValue;
        }

        protected override void OnUpdate()
        {
            // ZAS: If there is a bell, then we want to get moving!
            if (m_blackboardValues.ContainsKey(LAST_BELL))
            {
                ExitToState(nameof(CreatureMoveState));
                return;
            }

            // ZAS: If our random timer is expired, do something else
            if (DateTime.UtcNow > m_exitTime)
            {
                ExitToState(nameof(CreatureMoveState));
                return;
            }
        }

    }
}

[Serializable]
public class Stats
{
    #region Private Variables

    [Range(0, 100)]
    [SerializeField]
    private int m_hungerLevel = 0;

    [Range(0, 100)]
    [SerializeField]
    private int m_sleepinessLevel = 0;
       
    [SerializeField]
    private float m_movementSpeed = 3;

    #endregion
    
    #region Public Properties

    public int HungerLevel
    {
        get { return m_hungerLevel; }
    }

    public int SleepinessLevel
    {
        get { return m_sleepinessLevel; }
    }

    public float MovementSpeed
    {
        get { return m_movementSpeed; }
    }

    #endregion

    #region Public Methods

    public void SetHungerLevel(int value)
    {
        m_hungerLevel = value;
    }

    #endregion
}

[Serializable]
public class EmoteIcons
{
    // TJS: This is populated by adding the transform children of the m_emoteVisual object
    private GameObject[] m_emoteIconsArray = null;

    [SerializeField]
    private GameObject m_dotEmoteIcon = null;

    [SerializeField]
    private GameObject m_exclamationIcon = null;

    [SerializeField]
    private GameObject m_questionMarkIcon = null;

    [SerializeField]
    private GameObject m_hungryIcon = null;

    [SerializeField]
    private GameObject m_sleepyIcon = null;

    [SerializeField]
    private GameObject m_emoteVisual = null;

    public GameObject DotEmoteIcon { get => m_dotEmoteIcon; }
    public GameObject ExclamationIcon { get => m_exclamationIcon; }
    public GameObject QuestionMarkIcon { get => m_questionMarkIcon; }
    public GameObject HungryIcon { get => m_hungryIcon; }
    public GameObject SleepyIcon { get => m_sleepyIcon; }
    public GameObject EmoteVisual { get => m_emoteVisual; }

    public void FillEmoteIconArray()
    {
        int iconCount = m_emoteVisual.transform.childCount;

        m_emoteIconsArray = new GameObject[iconCount];

        for (int i = 0; i < iconCount; i++)
        {
            var childTransform = EmoteVisual.transform.GetChild(i);
            m_emoteIconsArray[i] = childTransform.gameObject;
        }
    }

    private void DisableAllEmoteIcons()
    {
        for (int i = 0; i < m_emoteIconsArray.Length; i++)
        {
            m_emoteIconsArray[i].SetActive(false);
        }
    }

    // TJS: Probably could have done all this differently... but running out of time!!
    public GameObject GetSpecificEmoteIconAfterDisablingAllEmoteIcons(GameObject emoteIcon)
    {
        DisableAllEmoteIcons();
        return emoteIcon;
    }
}