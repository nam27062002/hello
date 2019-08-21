using System.Collections.Generic;
using System;

namespace AI {

	public class SignalTriggers {
		[StateTransitionTrigger] public static int onLeaderPromoted    = UnityEngine.Animator.StringToHash("onLeaderPromoted");
		[StateTransitionTrigger] public static int onLeaderDemoted     = UnityEngine.Animator.StringToHash("onLeaderDemoted");
        [StateTransitionTrigger] public static int onIsHungry 		    = UnityEngine.Animator.StringToHash("onIsHungry");
        [StateTransitionTrigger] public static int onNotHungry 	    = UnityEngine.Animator.StringToHash("onNotHungry");
        [StateTransitionTrigger] public static int onAlert             = UnityEngine.Animator.StringToHash("onAlert");
        [StateTransitionTrigger] public static int onIgnoreAll 	    = UnityEngine.Animator.StringToHash("onIgnoreAll");
        [StateTransitionTrigger] public static int onWarning 		    = UnityEngine.Animator.StringToHash("onWarning");
        [StateTransitionTrigger] public static int onCalm 			    = UnityEngine.Animator.StringToHash("onCalm");
        [StateTransitionTrigger] public static int onDanger 		    = UnityEngine.Animator.StringToHash("onDanger");
        [StateTransitionTrigger] public static int onSafe 			    = UnityEngine.Animator.StringToHash("onSafe");
        [StateTransitionTrigger] public static int onCritical		    = UnityEngine.Animator.StringToHash("onCritical");
        [StateTransitionTrigger] public static int onPanic 		    = UnityEngine.Animator.StringToHash("onPanic");
        [StateTransitionTrigger] public static int onRecoverFromPanic  = UnityEngine.Animator.StringToHash("onRecoverFromPanic");
        [StateTransitionTrigger] public static int onCollisionEnter    = UnityEngine.Animator.StringToHash("onCollisionEnter");
        [StateTransitionTrigger] public static int onTriggerEnter 	    = UnityEngine.Animator.StringToHash("onTriggerEnter");
        [StateTransitionTrigger] public static int onTriggerExit 	    = UnityEngine.Animator.StringToHash("onTriggerExit");
        [StateTransitionTrigger] public static int onBurning 		    = UnityEngine.Animator.StringToHash("onBurning");
        [StateTransitionTrigger] public static int onChewing 		    = UnityEngine.Animator.StringToHash("onChewing");
        [StateTransitionTrigger] public static int onDestroyed 	    = UnityEngine.Animator.StringToHash("onDestroyed");
        [StateTransitionTrigger] public static int onFallDown 		    = UnityEngine.Animator.StringToHash("onFallDown");
        [StateTransitionTrigger] public static int OnGround		    = UnityEngine.Animator.StringToHash("OnGround");
        [StateTransitionTrigger] public static int onWaterEnter        = UnityEngine.Animator.StringToHash("onWaterEnter");
        [StateTransitionTrigger] public static int onWaterExit         = UnityEngine.Animator.StringToHash("onWaterExit");
        [StateTransitionTrigger] public static int onLockedInCage	    = UnityEngine.Animator.StringToHash("onLockedInCage");
        [StateTransitionTrigger] public static int onUnlockedFromCage  = UnityEngine.Animator.StringToHash("onUnlockedFromCage");
        [StateTransitionTrigger] public static int onInvulnerable	    = UnityEngine.Animator.StringToHash("onInvulnerable");
        [StateTransitionTrigger] public static int onVulnerable        = UnityEngine.Animator.StringToHash("onVulnerable");
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
            InLove              = (1 << 25),
		}


		public class TypeComparer : IEqualityComparer<Type>
		{
			public bool Equals(Type b1, Type b2)
		    {
		        return b1 == b2;
		    }

			public int GetHashCode(Type bx)
		    {
		        return (int)bx;
		    }
		}

		//---------------------------------


		private Signals.Type	m_value;
		private Dictionary<Signals.Type, int> 	m_onEnableTrigger;
		private Dictionary<Signals.Type, int> 	m_onDisableTrigger;
		private Dictionary<Signals.Type, object[]>  m_params;

		private IMachine m_machine;


		//---------------------------------
		public Signals(IMachine _machine) {
			m_value 			= Type.None;
			TypeComparer comparer = new TypeComparer();
			m_onEnableTrigger 	= new Dictionary<Signals.Type, int>( comparer );
			m_onDisableTrigger 	= new Dictionary<Signals.Type, int>( comparer );
			m_params			= new Dictionary<Signals.Type, object[]>( comparer );
            
            m_machine = _machine;
		}

		public void Init() {
			m_value = Type.None;
			m_params.Clear();
		}

		public void SetValue(Type _signal, bool _value) {
			bool enabled = (m_value & _signal) != 0;

			if (enabled != _value) {
				if (_value == true) {
					m_value |= _signal;

					OnEnable(_signal);
				} else {
					m_value &= ~_signal;

					m_params[_signal] = null;
					OnDisable(_signal);
				}
			}
		}

		public void SetValue(Type _signal, bool _value, ref object[] _params) {
			m_params[_signal] = _params;
			SetValue(_signal, _value);
		}

		public bool GetValue(Type _signal) {
			return (m_value & _signal) != 0;
		}

		public object[] GetParams(Type _signal) {
			return m_params[_signal];
		}

		public void SetOnEnableTrigger(Type _signal, int _triggerHash) {
			m_onEnableTrigger[_signal] = _triggerHash;
		}

		public void SetOnDisableTrigger(Type _signal, int _triggerHash) {
			m_onDisableTrigger[_signal] = _triggerHash;
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