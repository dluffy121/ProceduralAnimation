using SVGTech.Core.SystemProviderModule;
using SVGTech.Editor.Utils;
using SVGTech.Utils.Extensions;
using UnityEngine;

namespace ProceduralAnimation.RobotSphere
{
    public class RaycastGrounding : MonoBehaviour
    {
        [SerializeField]
        Transform m_targetTransform;
        public Transform TargetTransform { get => m_targetTransform; set => m_targetTransform = value; }

        [SerializeField]
        Transform m_raycastOriginTransform;
        public Transform RaycastOriginTransform { get => m_raycastOriginTransform; set => m_raycastOriginTransform = value; }

        [SerializeField]
        Vector3 m_raycastOriginOffset;
        public Vector3 RaycastOriginOffset { get => m_raycastOriginOffset; set => m_raycastOriginOffset = value; }

        [SerializeField]
        Vector3 m_raycastDirection;
        public Vector3 RaycastDirection { get => m_raycastDirection; set => m_raycastDirection = value; }

        [SerializeField]
        float m_raycastMaxDistance = Mathf.Infinity;
        public float RaycastMaxDistance { get => m_raycastMaxDistance; set => m_raycastMaxDistance = value; }

        [SerializeField]
        LayerMask m_layerMask;
        public LayerMask LayerMask { get => m_layerMask; set => m_layerMask = value; }

        #region Debug

        [Header("Debug")]
        [SerializeField, ReadOnly]
        Vector3 m_direction;

        #endregion

        void FixedUpdate()
        {
            RaycastGround();
        }

        RaycastHit m_hit;
        void RaycastGround()
        {
            m_direction = m_raycastOriginTransform.rotation * m_raycastDirection;
            if (Physics.Raycast(m_raycastOriginTransform.position + m_raycastOriginOffset, m_direction, out m_hit, m_raycastMaxDistance, m_layerMask))
            {
                Vector3 l_tmpVector = (m_targetTransform.up * (m_targetTransform.lossyScale.y / 2));
                m_targetTransform.position = m_hit.point;// + l_tmpVector;

                Vector3 xAxisAngle = m_targetTransform.forward.ProjectOnContactPlane(m_hit.normal).normalized;
                Vector3 zAxisAngle = m_targetTransform.right.ProjectOnContactPlane(m_hit.normal).normalized;

                l_tmpVector = m_targetTransform.localEulerAngles;
                l_tmpVector.x += Vector3.SignedAngle(m_targetTransform.forward, xAxisAngle, m_targetTransform.right);
                l_tmpVector.z += Vector3.SignedAngle(m_targetTransform.right, zAxisAngle, m_targetTransform.forward);

                m_targetTransform.localEulerAngles = l_tmpVector;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(m_raycastOriginTransform.position + m_raycastOriginOffset, m_raycastOriginTransform.rotation * m_raycastDirection);
            // Gizmos.DrawLine(m_raycastOriginTransform.position + m_raycastOriginOffset, m_hit.point);
        }
#endif
    }
}