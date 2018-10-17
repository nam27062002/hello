using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabDragonBarLockedElement : LabDragonBarElement {

    [Separator("Lock")]
    [SerializeField] private GameObject m_lock;

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
