using UnityEngine;

public class DestroyInSeconds : SelfDestroy {
	protected override void Awake() {
		m_mode = Mode.SECONDS;
		base.Awake();
    }
}
