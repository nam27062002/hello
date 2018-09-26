using UnityEngine;

public class SpartakusAnimationEvents : MonoBehaviour {

	//---------------------------------------------------------------------------------------

	public delegate void OnJumpImpulseDelegate();
    public delegate void OnJumpFallDownDelegate();
	public delegate void OnJumpReceptionDelegate();
	public delegate void OnDizzyRecoverDelegate();

	//---------------------------------------------------------------------------------------

	public event OnJumpImpulseDelegate 		onJumpImpulse;
    public event OnJumpFallDownDelegate     onJumpFallDown;
	public event OnJumpReceptionDelegate 	onJumpReception;
	public event OnDizzyRecoverDelegate 	onDizzyRecover;

	//---------------------------------------------------------------------------------------

	public void OnJumpImpulse() 	{ if (onJumpImpulse   != null) onJumpImpulse();   }
    public void OnJumpFallDown()    { if (onJumpFallDown  != null) onJumpFallDown();  }
	public void OnJumpReception()	{ if (onJumpReception != null) onJumpReception(); }
	public void OnDizzyRecover() 	{ if (onDizzyRecover  != null) onDizzyRecover();  }
}
