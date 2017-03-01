using UnityEngine;

public class SpartakusAnimationEvents : MonoBehaviour {

	//---------------------------------------------------------------------------------------

	public delegate void OnJumpImpulseDelegate();
	public delegate void OnJumpReceptionDelegate();
	public delegate void OnDizzyRecoverDelegate();

	//---------------------------------------------------------------------------------------

	public event OnJumpImpulseDelegate 		onJumpImpulse;
	public event OnJumpReceptionDelegate 	onJumpReception;
	public event OnDizzyRecoverDelegate 	onDizzyRecover;

	//---------------------------------------------------------------------------------------

	public void OnJumpImpulse() 	{ if (onJumpImpulse   != null) onJumpImpulse();   }
	public void OnJumpReception()	{ if (onJumpReception != null) onJumpReception(); }
	public void OnDizzyRecover() 	{ if (onDizzyRecover  != null) onDizzyRecover();  }
}
