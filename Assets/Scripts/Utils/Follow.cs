using System;
using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField]
    Transform m_sourceTransform;

    [SerializeField]
    Transform m_targetTransform;

    [SerializeField]
    float m_lerpSpeed = float.NaN;

    [Header("Toggle, can help reduce unwanted calculations")]
    [SerializeField]
    bool m_followPosition = true;
    public bool IsFollowPosition { get => m_followPosition; set => m_followPosition = value; }
    [SerializeField]
    bool m_followRotation = true;
    public bool IsFollowRotation { get => m_followRotation; set => m_followRotation = value; }

    [Header("Multiplier")]
    [SerializeField]
    Vector3 m_followPositionMultiplier = Vector3.one;
    [SerializeField]
    Vector3 m_followRotationMultiplier = Vector3.one;

    [Header("Offset")]
    [SerializeField]
    Vector3 m_followPositionOffset;
    [SerializeField]
    Vector3 m_followRotationOffset;

    [Header("Debug")]
    [SerializeField]
    bool m_isDebug = false;

    private void LateUpdate()
    {
        FollowPosition();
        FollowRotation();
    }

    public void FollowPosition()
    {
        if (m_followPosition)
        {
            Vector3 l_lerpPosition = Vector3.Scale(m_targetTransform.position, m_followPositionMultiplier) + m_followPositionOffset;
            if (float.IsNaN(m_lerpSpeed) || m_lerpSpeed == 0)
            {
                m_sourceTransform.position = l_lerpPosition;
                return;
            }
            m_sourceTransform.position = Vector3.Lerp(m_sourceTransform.position, l_lerpPosition, m_lerpSpeed);
        }
    }

    public void FollowRotation()
    {
        if (m_followRotation)
        {
            Vector3 l_lerpRotation = Vector3.Scale(m_targetTransform.eulerAngles, m_followRotationMultiplier) + m_followRotationOffset;
            if (float.IsNaN(m_lerpSpeed) || m_lerpSpeed == 0)
            {
                m_sourceTransform.eulerAngles = l_lerpRotation;
                return;
            }
            m_sourceTransform.eulerAngles = Vector3.Lerp(m_sourceTransform.eulerAngles, l_lerpRotation, m_lerpSpeed);
        }
    }
}