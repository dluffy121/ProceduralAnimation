using System;
using SVGTech.Core.SystemProviderModule;
using SVGTech.Manager.CurveSystem;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RoboSphereCharacterController : MonoBehaviour
{
    [SerializeField]
    Animator m_animator;

    [SerializeField]
    RigBuilder m_rigBuilder;

    [SerializeField]
    MovementController m_movementController;

    [SerializeField]
    KeyCode m_activateKey;
    private const string ActivateParameter = "Open";

    bool m_isActive = false;

    private const string RunParameter = "Roll";

    [SerializeField]
    string m_walkToRunWeightTransitionCurveId = "WalkToRunWeight";
    [SerializeField]
    string m_runToWalkWeightTransitionCurveId = "RunToWalkWeight";
    Curve m_walkToRunWeightTransitionCurve = null;
    Curve m_runToWalkWeightTransitionCurve = null;
    float m_walkToRunTransitionNormalizedTime = 0;
    float m_runToWalkTransitionNormalizedTime = 0;

    CharacterState m_currentState;
    AnimatorStateInfo m_currentAnimStateInfo;

    private ICurveManager m_ICurveManager = null;
    private ICurveManager ICurveManagerRef
    {
        get
        {
            if (m_ICurveManager == null)
                m_ICurveManager = SystemProvider.Instance.GetSystem<CurveManager>();
            return m_ICurveManager;
        }
    }

    private void Start()
    {
        GetCurve();
        UpdateActiveState();

        m_movementController.OnStartRunning += UpdateRunState;
        m_movementController.OnEndRunning += UpdateRunState;
    }

    private void GetCurve()
    {
        ICurveManagerRef.GetCurve(ref m_walkToRunWeightTransitionCurve, m_walkToRunWeightTransitionCurveId);
        ICurveManagerRef.GetCurve(ref m_runToWalkWeightTransitionCurve, m_runToWalkWeightTransitionCurveId);
    }

    private void Update()
    {
        if (Input.GetKeyDown(m_activateKey))
        {
            m_isActive = !m_isActive;
            UpdateActiveState();
        }

        GetCurrentState();

        UpdateCurrentState();
    }

    private void UpdateActiveState()
    {
        m_animator.SetBool(ActivateParameter, m_isActive);
        EnableMovementController(m_isActive);
    }

    private void EnableMovementController(bool a_value)
    {
        m_movementController.enabled = a_value;
    }

    private void GetCurrentState()
    {
        m_currentAnimStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
        if (m_currentAnimStateInfo.IsName("Open"))
            m_currentState = CharacterState.Opening;
        if (m_currentAnimStateInfo.IsName("Close"))
            m_currentState = CharacterState.Closing;
        if (m_currentAnimStateInfo.IsName("Go_To_Roll"))
            m_currentState = CharacterState.WalkToRun;
        if (m_currentAnimStateInfo.IsName("Closed_StopRoll"))
            m_currentState = CharacterState.RunToWalk;
        if (m_currentAnimStateInfo.IsName("Idle_Loop"))
            m_currentState = m_movementController.IsRunning ? CharacterState.Running : CharacterState.Walking;
    }

    private void UpdateCurrentState()
    {
        float l_animStateNormalizedTime = Mathf.Clamp01(m_currentAnimStateInfo.normalizedTime);
        switch (m_currentState)
        {
            case CharacterState.Opening:
                UpdateRigWeight(l_animStateNormalizedTime);
                break;
            case CharacterState.Closing:
                UpdateRigWeight(1 - l_animStateNormalizedTime);
                break;
            case CharacterState.Walking:
                break;
            case CharacterState.WalkToRun:
                float cerpValue = m_walkToRunWeightTransitionCurve.Cerp(0, 1, l_animStateNormalizedTime);
                UpdateRigWeight(cerpValue);
                UpdateRunMultiplierStrength(cerpValue);
                break;
            case CharacterState.Running:
                break;
            case CharacterState.RunToWalk:
                cerpValue = m_runToWalkWeightTransitionCurve.Cerp(0, 1, l_animStateNormalizedTime);
                UpdateRigWeight(cerpValue);
                UpdateRunMultiplierStrength(1 - cerpValue);
                break;
        }
    }

    // NOTE:
    // Since weight update can only happen in Update loop this is seems to be the best solution
    // Otherwise shifting this method and its calling to StateMachineBehaviour's OnStateUpdate method can help optimize, since it will happen on Animation thread
    private void UpdateRigWeight(float a_weight)
    {
        m_rigBuilder.layers[0].rig.weight = a_weight;
    }

    private void UpdateRunMultiplierStrength(float cerpValue)
    {
        m_movementController.RunMultiplierStrength = 1 - cerpValue;
    }

    private void UpdateRunState()
    {
        if (m_animator.GetBool(RunParameter) == m_movementController.IsRunning) return;

        m_animator.SetBool(RunParameter, m_movementController.IsRunning);
    }
}

public enum CharacterState
{
    Closed,
    Opening,
    Closing,
    Walking,
    WalkToRun,
    Running,
    RunToWalk
}