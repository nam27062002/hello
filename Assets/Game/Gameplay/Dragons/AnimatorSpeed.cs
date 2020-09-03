using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorSpeed : MonoBehaviour
{
    [SerializeField] Animator m_animator;
    [SerializeField] float speed = 1.0f;

    readonly int m_animParamSpeedId = Animator.StringToHash("speed");

    void Start()
    {
        m_animator.SetFloat(m_animParamSpeedId, speed);
    }
}
