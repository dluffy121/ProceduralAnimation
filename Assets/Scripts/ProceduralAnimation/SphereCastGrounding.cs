using SVGTech.Core.SystemProviderModule;
using SVGTech.Editor.Utils;
using SVGTech.Utils.Extensions;
using UnityEngine;

namespace ProceduralAnimation.RobotSphere
{
    public class SphereCastGrounding : MonoBehaviour
    {
        [SerializeField]
        Transform m_targetTransform;
        public Transform TargetTransform { get => m_targetTransform; set => m_targetTransform = value; }

        [SerializeField]
        Transform m_spherecastOriginTransform;
        public Transform SpherecastOriginTransform { get => m_spherecastOriginTransform; set => m_spherecastOriginTransform = value; }

        [SerializeField]
        Vector3 m_spherecastOriginOffset;
        public Vector3 SpherecastOriginOffset { get => m_spherecastOriginOffset; set => m_spherecastOriginOffset = value; }

        [SerializeField]
        Vector3 m_spherecastDirection;
        public Vector3 SpherecastDirection { get => m_spherecastDirection; set => m_spherecastDirection = value; }

        [SerializeField]
        float m_spherecastRadius;
        public float SpherecastRadius { get => m_spherecastRadius; set => m_spherecastRadius = value; }

        [SerializeField]
        float m_spherecastMaxDistance = Mathf.Infinity;
        public float SpherecastMaxDistance { get => m_spherecastMaxDistance; set => m_spherecastMaxDistance = value; }

        [SerializeField]
        int m_layerMask;
        public int LayerMask { get => m_layerMask; set => m_layerMask = value; }

#if UNITY_EDITOR
        [SerializeField]
        string[] m_strLayerMasks;

        #region Debug

        [Header("Debug")]

        [SerializeField, ReadOnly]
        Vector3 m_direction;

        #endregion

        void OnValidate()
        {
            if (m_strLayerMasks == null || m_strLayerMasks.Length == 0) return;
            m_layerMask = UnityEngine.LayerMask.GetMask(m_strLayerMasks);
        }
#endif

        void FixedUpdate()
        {
            SpherecastGround();
        }

        RaycastHit m_hit;
        void SpherecastGround()
        {
            m_direction = m_spherecastOriginTransform.rotation * m_spherecastDirection;
            if (Physics.SphereCast(m_spherecastOriginTransform.position + m_spherecastOriginOffset, m_spherecastRadius, m_direction, out m_hit, m_spherecastMaxDistance, m_layerMask))
            {
                Vector3 l_tmpVector = (m_targetTransform.up * (m_targetTransform.lossyScale.y / 2));
                m_targetTransform.position = m_hit.point + l_tmpVector;

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
            Gizmos.DrawLine(m_spherecastOriginTransform.position + m_spherecastOriginOffset, m_hit.point);
            Gizmos.DrawWireSphere(m_spherecastOriginTransform.position + m_spherecastOriginOffset + m_direction * m_hit.distance, m_spherecastRadius);
        }
#endif
    }
}