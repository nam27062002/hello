using System.Collections.Generic;
using System;

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
		[StateTransitionTrigger] public static string OnCollisionEnter 		= "onCollisionEnter";
		[StateTransitionTrigger] public static string OnTriggerEnter 		= "onTriggerEnter";
		[StateTransitionTrigger] public static string OnTriggerExit 		= "onTriggerExit";
		[StateTransitionTrigger] public static string OnBurning 			= "onBurning";
		[StateTransitionTrigger] public static string OnChewing 			= "onChewing";
		[StateTransitionTrigger] public static string OnDestroyed 			= "onDestroyed";
		[StateTransitionTrigger] public static string OnFallDown 			= "onFallDown";
		[StateTransitionTrigger] public static string OnGround				= "OnGround";
		[StateTransitionTrigger] public static string OnLockedInCage		= "onLockedInCage";
		[StateTransitionTrigger] public static string OnUnlockedFromCage	= "onUnlockedFromCage";
		[StateTransitionTrigger] public static string OnInvulnerable		= "onInvulnerable";
		[StateTransitionTrigger] public static string OnVulnerable			= "onVulnerable";
	}

	public class Signals {
		//---------------------------------
		[Flags]
		public enum Type {
			None				= (1 << 0),
			Leader  			= (1 << 1),
			Hungry				= (1 << 2), 	
			Alert				= (1 << 3), 	
			Warning				= (1 << 4), 
			Danger				= (1 << 5),
			Critical			= (1 << 6),
			Panic				= (1 << 7), 	
			BackToHome			= (1 << 8),
			Burning				= (1 << 9), 
			Chewing				= (1 << 10), 
			Latched				= (1 << 11),
			Biting				= (1 << 12),
			Latching			= (1 << 13),
			Destroyed			= (1 << 14), 
			Collision			= (1 << 15),
			Trigger				= (1 << 16),
			FallDown			= (1 << 17),
			InWater				= (1 << 18),
			LockedInCage		= (1 << 19),
			Invulnerable		= (1 << 20),
			InvulnerableBite	= (1 << 21),
			InvulnerableFire	= (1 << 22),
			Ranged				= (1 << 23),
			Melee				= (1 << 24),
		}
		//---------------------------------


		private Signals.Type	m_value;
		private Dictionary<Signals.Type, string> 	m_onEnableTrigger;
		private Dictionary<Signals.Type, string> 	m_onDisableTrigger;
		private Dictionary<Signals.Type, object[]>  m_params;

		private IMachine m_machine;


		//---------------------------------
		public Signals(IMachine _machine) {
			m_value 			= Type.None;
			m_onEnableTrigger 	= new Dictionary<Signals.Type, string>();
			m_onDisableTrigger 	= new Dictionary<Signals.Type, string>();
			m_params			= new Dictionary<Signals.Type, object[]>();

			m_machine = _machine;
		}

		public void Init() {
			m_value = Type.None;
			m_params.Clear();
		}

		public void SetValue(Type _signal, bool _value, object[] _params = null) {
			bool enabled = (m_value & _signal) != 0;

			if (enabled != _value) {
				if (_value == true) {
					m_value |= _signal;

					m_params[_signal] = _params;
					OnEnable(_signal);
				} else {
					m_value &= ~_signal;

					m_params[_signal] = null;
					OnDisable(_signal);
				}
			}
		}

		public bool GetValue(Type _signal) {
			return (m_value & _signal) != 0;
		}

		public object[] GetParams(Type _signal) {
			return m_params[_signal];
		}

		public void SetOnEnableTrigger(Type _signal, string _trigger) {
			m_onEnableTrigger[_signal] = _trigger;
		}

		public void SetOnDisableTrigger(Type _signal, string _trigger) {
			m_onDisableTrigger[_signal] = _trigger;
		}

		private void OnEnable(Type _signal) {
			if (m_onEnableTrigger.ContainsKey(_signal)) {
				m_machine.OnTrigger(m_onEnableTrigger[_signal]);
			}
		}

		private void OnDisable(Type _signal) {
			if (m_onDisableTrigger.ContainsKey(_signal)) {
				m_machine.OnTrigger(m_onDisableTrigger[_signal]);
			}
		}
	}
}