using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Controls the behaviour of our little creatures. Creature is driven by a state machine. To add behaviour to it, 
/// simply make a new state that inherits from AbstractState, and add it in <see cref="Start"/>
/// </summary>
public class Creature : MonoBehaviour
{

    private readonly StateMachine m_stateMachine = new StateMachine();

    [SerializeField]
    private GameObject m_eggVisual = null;

    [SerializeField]
    private GameObject m_creatureVisual = null;

    [SerializeField]
    private float m_moveSpeed = 1f;

    private void Start()
    {
        m_stateMachine.AddState(new EggState(m_eggVisual, m_creatureVisual));
        m_stateMachine.AddState(new CreatureIdleState());
        m_stateMachine.AddState(new CreatureMoveState(transform, m_moveSpeed));

        m_stateMachine.Start(nameof(EggState));
    }

    private void Update()
    {
        m_stateMachine.Update();
    }

    private class EggState : AbstractState
    {
        private DateTime m_exitTime = DateTime.MinValue;
        private readonly GameObject m_eggVisual = null;
        private readonly GameObject m_creatureVisual = null;

        public EggState(GameObject eggVisual, GameObject creatureVisual)
            : base(nameof(EggState))
        {
            m_eggVisual = eggVisual;
            m_creatureVisual = creatureVisual;

            m_eggVisual.SetActive(true);
            m_creatureVisual.SetActive(false);
        }

        protected override void OnEnter()
        {
            m_exitTime = DateTime.UtcNow.AddSeconds(UnityEngine.Random.Range(5, 10));
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

                ExitToState(nameof(CreatureIdleState));
            }
        }
    }

    private class CreatureIdleState : AbstractState
    {

        private DateTime m_exitTime = DateTime.MinValue;

        public CreatureIdleState()
            : base(nameof(CreatureIdleState))
        {
        }

        protected override void OnEnter()
        {
            m_exitTime = DateTime.UtcNow.AddSeconds(UnityEngine.Random.Range(1, 10));
        }

        protected override void OnExit()
        {
            m_exitTime = DateTime.MinValue;
        }

        protected override void OnUpdate()
        {
            if (DateTime.UtcNow > m_exitTime)
            {
                ExitToState(nameof(CreatureMoveState));
            }
        }

    }

    private class CreatureMoveState : AbstractState
    {

        private readonly Transform m_creatureTransform = null;
        private readonly float m_moveSpeed = 0f;

        public CreatureMoveState(Transform transform, float moveSpeed)
          : base(nameof(CreatureMoveState))
        {
            m_creatureTransform = transform;
            m_moveSpeed = moveSpeed;
        }

        protected override void OnEnter()
        {
            var randomInCircle = UnityEngine.Random.insideUnitCircle;
            var targetPosition = m_creatureTransform.position + (new Vector3(randomInCircle.x, 0f, randomInCircle.y) * 2f);

            MoveToTargetAsync(targetPosition);
        }

        protected override void OnExit()
        {
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

        private async Task TranslateToTargetAsync(Vector3 target)
        {
            var start = m_creatureTransform.position;
            var vectorToTarget = (target - m_creatureTransform.position);
            var maxMagnitude = vectorToTarget.magnitude;
            var segments = Mathf.FloorToInt(maxMagnitude / m_moveSpeed);
            for (int i = 0; i < segments; i++)
            {
                var atStart = m_creatureTransform.position;
                var atEnd = atStart + (vectorToTarget.normalized * m_moveSpeed);
                await AnimationUtility.AnimateOverTime(
                    1000,
                    x =>
                    {
                        var t = -(4f * Mathf.Pow(x - .5f, 2)) + 1;

                        var poss = Vector3.Lerp(atStart, atEnd, x);
                        poss.y += t;

                        m_creatureTransform.position = poss;
                    }
                );

                m_creatureTransform.position = atEnd;

                await Task.Delay(500);
            }

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
                }
            );

            m_creatureTransform.position = atEnd2;
        }

    }

}
