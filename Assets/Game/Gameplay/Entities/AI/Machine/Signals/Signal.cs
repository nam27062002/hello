using System.Collections.Generic;

namespace AI {

	public class SignalTriggers {
		[StateTransitionTrigger] public static string OnLeaderPromoted 		= "onLeaderPromoted";
		[StateTransitionTrigger] public static string OnLeaderDemoted 		= "onLeaderDemoted";
		[StateTransitionTrigger] public static string OnIsHungry 			= "onIsHungry";
		[StateTransitionTrigger] public static string OnNotHungry 			= "onNotHungry";
		[StateTransitionTrigger] public static string OnAlert 				= "onAlert";
		[StateTransitionTrigger] public static string OnIgnoreAll 			= "onIgnoreAll";
		[StateTransitionTrigger] public static string OnWarning 			= "onWarning";
		[StateTransitionTrigger] public static string OnCalm 				= "onCalm";
		[StateTransitionTrigger] public static string OnDanger 				= "onDanger";
		[StateTransitionTrigger] public static string OnSafe 				= "onSafe";
		[StateTransitionTrigger] public static string OnCritical			= "onCritical";
		[StateTransitionTrigger] public static string OnPanic 				= "onPanic";
		[StateTransitionTrigger] public static string OnRecoverFromPanic 	= "onRecoverFromPanic";
		[StateTransitionTrigger] public static string OnOutsideArea 		= "onOutsideArea";
		[StateTransitionTrigger] public static string OnBackAtHome			= "onBackAtHome";
		[StateTransitionTrigger] public static string OnCollisionEnter 		= "onCollisionEnter";
		[StateTransitionTrigger] public static string OnTriggerEnter 		= "onTriggerEnter";
		[StateTransitionTrigger] public static string OnBurning 			= "onBurning";
		[StateTransitionTrigger] public static string OnChewing 			= "onChewing";
		[StateTransitionTrigger] public static string OnDestroyed 			= "onDestroyed";
		[StateTransitionTrigger] public static string OnFallDown 			= "onFallDown";
		[StateTransitionTrigger] public static string OnGround				= "OnGround";
		[StateTransitionTrigger] public static string OnLockedInCage		= "onLockedInCage";
		[StateTransitionTrigger] public static string OnUnlockedFromCage	= "onUnlockedFromCage";
	}

	public class Signals {
		//---------------------------------
		public enum Type {
			Leader = 0,
	        Hungry, 	
	        Alert, 	
	        Warning, 
	        Danger,
			Critical,
	        Panic, 	
	        BackToHome,
	        Burning, 
	        Chewing, 
			Biting,
			Latching,
	        Destroyed, 
	        Collision,
	        Trigger,
			FallDown,
			LockedInCage,

			Count
		}
		//---------------------------------


		private bool[] 			m_value;
		private string[] 		m_onEnableTrigger;
		private string[]		m_onDisableTrigger;
		private List<object[]> 	m_params;

		private Machine m_machine;


		//---------------------------------
		public Signals(Machine _machine) {
			m_value 			= new bool[(int)Type.Count];
			m_onEnableTrigger 	= new string[(int)Type.Count];
			m_onDisableTrigger 	= new string[(int)Type.Count];
			m_params			= new List<object[]>((int)Type.Count);

			for (int i = 0; i < m_value.Length; i++) {
				m_params.Add(null);
			}

			m_machine = _machine;
		}

		public void Init() {
			for (int i = 0; i < m_value.Length; i++) {
				m_value[i] = false;
				m_params[i] = null;
			}
		}

		public void SetValue(Type _signal, bool _value, object[] _params = null) {
			int index = (int)_signal;
			if (m_value[index] != _value) {
				if (_value == true) {
					m_params[index] = _params;
					OnEnable(index);
				} else {
					m_params[index] = null;
					OnDisable(index);
				}
				m_value[index] = _value;
			}
		}

		public bool GetValue(Type _signal) {
			return m_value[(int)_signal];
		}

		public object[] GetParams(Type _signal) {
			return m_params[(int)_signal];
		}

		public void SetOnEnableTrigger(Type _signal, string _trigger) {
			m_onEnableTrigger[(int)_signal] = _trigger;
		}

		public void SetOnDisableTrigger(Type _signal, string _trigger) {
			m_onDisableTrigger[(int)_signal] = _trigger;
		}

		private void OnEnable(int _index) {
			if (m_onEnableTrigger[_index] != null) {
				m_machine.OnTrigger(m_onEnableTrigger[_index]);
			}
		}

		private void OnDisable(int _index) {
			if (m_onDisableTrigger[_index] != null) {
				m_machine.OnTrigger(m_onDisableTrigger[_index]);
			}
		}
	}
}