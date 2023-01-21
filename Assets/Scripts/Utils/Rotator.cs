using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField]
    Transform m_targetRotator;

    [SerializeField]
    float m_XRotationSpeed = 1, m_YRotationSpeed = 1;

    [SerializeField, Min(0)]
    float m_smoothSpeed = 0;

    [SerializeField]
    InputType m_inputType = InputType.Mouse;

    [SerializeField]
    bool m_isDebug = false;

    Vector2 m_turnVector;

    Quaternion m_originalRotation;

    private void Start()
    {
        m_originalRotation = m_targetRotator.transform.rotation;
    }

    private void LateUpdate()
    {
        UpdateVectorAsPerInput(ref m_turnVector);

        if (m_isDebug)
            Debug.LogError(m_turnVector);

        Quaternion newRotation = Quaternion.Euler(-m_turnVector.y, m_turnVector.x, 0);

        Rotate(m_smoothSpeed == 0 ?
                newRotation :
                Quaternion.Lerp(m_targetRotator.transform.rotation, newRotation, Time.deltaTime * m_smoothSpeed));
    }

    private void UpdateVectorAsPerInput(ref Vector2 a_vector)
    {
        switch (m_inputType)
        {
            case InputType.Mouse:
                a_vector.x += Input.GetAxis("Mouse X") * m_XRotationSpeed;
                a_vector.y += Input.GetAxis("Mouse Y") * m_YRotationSpeed;
                break;
            case InputType.WASD:
                a_vector.x += Input.GetAxis("Horizontal") * m_XRotationSpeed;
                a_vector.y += Input.GetAxis("Vertical") * m_YRotationSpeed;
                break;
        }
    }

    public void Rotate(Quaternion a_rotation)
    {
        m_targetRotator.transform.rotation = a_rotation;
    }

    public void Reset()
    {
        m_targetRotator.transform.rotation = m_originalRotation;
    }
}

public enum InputType
{
    Mouse,
    WASD
}