using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabDragonBarLockedElement : LabDragonBarElement {

    [Separator("Lock")]
    [SerializeField] private GameObject m_lock;

	protected int m_unlockLevel;
	protected DragonTier m_requiredTier;

	public void SetUnlockInfo(int _unlockLevel, DragonTier _requiredTier) {
		m_unlockLevel = _unlockLevel;
		m_requiredTier = _requiredTier;
	}

    protected override void OnLocked() {
        m_lock.SetActive(true);
    }

    protected override void OnAvailable() {
        m_lock.SetActive(false);
    }

    protected override void OnOwned() {
        m_lock.SetActive(false);
    }
}
