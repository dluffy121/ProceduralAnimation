using SVGTech.Core.SystemProviderModule;
using SVGTech.Editor.Utils;
using SVGTech.Manager.CurveSystem;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LegConstraintController : MonoBehaviour
{
    [SerializeField]
    MovementController m_movementControllerRef = null;

    [SerializeField]
    Transform m_targetTransform = null;

    [SerializeField]
    Transform m_groundTransform = null;

    [SerializeField]
    float m_stepDistance, m_stepHeight;

    [SerializeField]
    float m_stepSpeed;

    [SerializeField]
    string m_legMoveCurveId, m_legLiftCurveId;

    Curve m_legMoveCurve = null;
    Curve m_legLiftCurve = null;

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

    [SerializeField]
    LegConstraintController[] m_coordinatingControllers;

    [SerializeField]
    float m_stationaryResetDuration = 0.5f;

    [SerializeField]
    float m_stationaryMinVelocity = 0.05f;

    private void Start()
    {
        GetCurve();
        InitializeLastPosition();
    }

    private void GetCurve()
    {
        ICurveManagerRef.GetCurve(ref m_legMoveCurve, m_legMoveCurveId);
        ICurveManagerRef.GetCurve(ref m_legLiftCurve, m_legLiftCurveId);
    }

    [SerializeField, ReadOnly]
    float m_currStepHeight = 0;

    [SerializeField, ReadOnly]
    float m_currStepTime = 0;

    [SerializeField, ReadOnly]
    Vector3 m_lastTargetPosition;

    [SerializeField, ReadOnly]
    bool m_stepping = false;
    public bool Stepping => m_stepping;

    [SerializeField, ReadOnly]
    float m_stationaryTime = 0;
    bool m_resettingLegPos = false;

    private void InitializeLastPosition()
    {
        m_lastTargetPosition = m_targetTransform.position;
    }

    private void Update()
    {
        UpdateStationaryTime();
        UpdateLegPosition();
    }

    private void UpdateLegPosition()
    {
        bool l_canMove = true;
        foreach (var mover in m_coordinatingControllers)
        {
            l_canMove &= !mover.Stepping;
        }

        if (!l_canMove)
        {
            m_targetTransform.position = m_lastTargetPosition;
            return;
        }

        float l_stepDistance = GetStepDistance();

        if (!m_resettingLegPos && m_stationaryTime >= m_stationaryResetDuration)
        {
            m_resettingLegPos = true;
            m_stepping = true;
        }
        if (!m_stepping && Vector3.Distance(m_targetTransform.position, m_groundTransform.position) < l_stepDistance)
        {
            m_targetTransform.position = m_lastTargetPosition;
            return;
        }

        m_stepping = true;

        float l_actualDistance = Vector3.Distance(m_lastTargetPosition, m_groundTransform.position);
        float l_lerpTime;
        m_currStepTime += Time.deltaTime;
        l_lerpTime = m_currStepTime * m_stepSpeed;
        if (l_stepDistance > l_actualDistance)
            l_lerpTime *= l_stepDistance / l_actualDistance;
        else
            l_lerpTime *= l_actualDistance / l_stepDistance;

        Vector3 l_newPosition = m_legMoveCurve.Cerp(m_lastTargetPosition, m_groundTransform.position, l_lerpTime);
        m_currStepHeight = m_legLiftCurve.Cerp(0, GetStepHeight(l_stepDistance, l_actualDistance), l_lerpTime);
        l_newPosition += m_targetTransform.up * m_currStepHeight;

        m_targetTransform.position = l_newPosition;

        if (l_lerpTime >= 1)
        {
            m_stepping = false;
            m_currStepTime = 0;
            m_lastTargetPosition = m_targetTransform.position;
        }
    }

    private void UpdateStationaryTime()
    {
        if (Mathf.Clamp01(m_movementControllerRef.Velocity.magnitude) > m_stationaryMinVelocity
            || Mathf.Clamp01(m_movementControllerRef.AngularVelocity.magnitude) > m_stationaryMinVelocity)
        {
            m_stationaryTime = 0;
            m_resettingLegPos = false;
            return;
        }

        if (m_resettingLegPos)
        {
            m_stationaryTime = 0;
            return;
        }

        m_stationaryTime += Time.deltaTime;
    }

    private float GetStepDistance()
    {
        float l_stepDistance = m_stepDistance;
        if (m_movementControllerRef.Velocity.magnitude > 1)
            l_stepDistance /= m_movementControllerRef.Velocity.magnitude;
        return l_stepDistance;
    }

    private float GetStepHeight(float a_stepDistance, float a_actualDistance)
    {
        if (a_stepDistance > a_actualDistance)
            return m_stepHeight * (a_actualDistance / a_stepDistance);

        return m_stepHeight * (a_stepDistance / a_actualDistance);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(m_targetTransform.position, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_groundTransform.position, m_stepDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(m_lastTargetPosition, 0.1f);
    }
}