using UnityEngine;
using System.Collections.Generic;

public class DragonPetEatBehaviour : EatBehaviour {

	[SerializeField] private DragonTier m_petTier;
	[SerializeField] private float m_bite;

	void Start() {
		m_mouth = transform.FindTransformRecursive("Fire_Dummy");
		m_tongueDirection = m_mouth.position - transform.FindTransformRecursive("Dragon_Head").position;
		m_tongueDirection.Normalize();

		m_motion = GetComponent<PreyMotion>();

		m_tier = m_petTier;
		m_biteSkill = m_bite;
	}

	protected override void SlowDown(bool _enable) {}
}
