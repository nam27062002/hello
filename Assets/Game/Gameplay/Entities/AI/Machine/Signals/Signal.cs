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
		[StateTransitionTrigger] public static string OnPanic 				= "onPanic";
		[StateTransitionTrigger] public static string OnRecoverFromPanic 	= "onRecoverFromPanic";
		[StateTransitionTrigger] public static string OnOutsideArea 		= "onOutsideArea";
		[StateTransitionTrigger] public static string OnBackAtHome			= "onBackAtHome";
		[StateTransitionTrigger] public static string OnCollisionEnter 		= "onCollisionEnter";
		[StateTransitionTrigger] public static string OnBurning 			= "onBurning";
		[StateTransitionTrigger] public static string OnChewing 			= "onChewing";
		[StateTransitionTrigger] public static string OnDestroyed 			= "onDestroyed";
		[StateTransitionTrigger] public static string OnFallDown 			= "onFallDown";
		[StateTransitionTrigger] public static string OnGround				= "OnGround";
	}

	public class Signals {
		//---------------------------------
		public enum Type {
			Leader = 0,
	        Hungry, 	
	        Alert, 	
	        Warning, 
	        Danger, 	
	        Panic, 	
	        BackToHome,
	        Burning, 
	        Chewing, 
			Biting,
	        Destroyed, 
	        CollisionTrigger,
			FallDown,

			Count
		}
		//---------------------------------


		private bool[] 		m_value;
		private string[] 	m_onEnableTrigger;
		private string[]	m_onDisableTrigger;

		private Machine m_machine;


		//---------------------------------
		public Signals(Machine _machine) {
			m_value 			= new bool[(int)Type.Count];
			m_onEnableTrigger 	= new string[(int)Type.Count];
			m_onDisableTrigger 	= new string[(int)Type.Count];

			m_machine = _machine;
		}

		public void Init() {
			for (int i = 0; i < m_value.Length; i++) {
				m_value[i] = false;
			}
		}

		public void SetValue(Type _signal, bool _value) {
			int index = (int)_signal;
			if (m_value[index] != _value) {
				if (_value == true) {
					OnEnable(index);
				} else {
					OnDisable(index);
				}
				m_value[index] = _value;
			}
		}

		public bool GetValue(Type _signal) {
			return m_value[(int)_signal];
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



	/*
	[System.Serializable]
	public abstract class Signal {
		private bool m_value;
		public bool value { get { return m_value; } }

		private Machine m_machine;
		public Machine machine { set { m_machine = value; } }

		public void Init() {
			m_value = false;
		}

		public void Set(bool _value) {
			if (m_value != _value) {
				if (_value) {
					string e = OnEnabled();
					if (e != "") m_machine.OnTrigger(e);
				} else {
					string e = OnDisabled();
					if (e != "") m_machine.OnTrigger(e);
				}

				m_value = _value;
			}
		}

		protected virtual string OnEnabled() { return ""; }
		protected virtual string OnDisabled() { return ""; }
	}

	namespace Signals {
		public class Leader : Signal {
			public static string name = "Leader";

			[StateTransitionTrigger]
			public static string OnLeaderPromoted = "onLeaderPromoted";

			[StateTransitionTrigger]
			public static string OnLeaderDemoted = "onLeaderDemoted";

			protected override string OnEnabled() { return OnLeaderPromoted; }
			protected override string OnDisabled() { return OnLeaderDemoted; }
		}

		// this machine is hungry and it'll search for preys using the Eater machine component
		public class Hungry : Signal {
			public static string name = "Hungry";

			[StateTransitionTrigger]
			public static string OnIsHungry = "onIsHungry";

			[StateTransitionTrigger]
			public static string OnNotHungry = "onNotHungry";

			protected override string OnEnabled() { return OnIsHungry; }
			protected override string OnDisabled() { return OnNotHungry; }
		}

		// this machine will use it's sensor to detect the player. Later we can extend this to detect all king of enemies
		public class Alert : Signal {
			public static string name = "Alert";

			[StateTransitionTrigger]
			public static string OnAlert = "onAlert";

			[StateTransitionTrigger]
			public static string OnIgnoreAll = "onIgnoreAll";

			protected override string OnEnabled() { return OnAlert; }
			protected override string OnDisabled() { return 
				OnIgnoreAll; }
		}

		// enemy detected nearby
		public class Warning : Signal {
			public static string name = "Warning";

			[StateTransitionTrigger]
			public static string OnWarning = "onWarning";

			[StateTransitionTrigger]
			public static string OnCalm = "onCalm";

			protected override string OnEnabled() { return OnWarning; }
			protected override string OnDisabled() { return OnCalm; }
		}

		// enemy is too close
		public class Danger : Signal {
			public static string name = "Danger";

			[StateTransitionTrigger]
			public static string OnDanger = "onDanger";

			[StateTransitionTrigger]
			public static string OnSafe = "onSafe";

			protected override string OnEnabled() { return OnDanger; }
			protected override string OnDisabled() { return OnSafe; }
		}

		// this machine is unable to perform actions
		public class Panic : Signal {
			public static string name = "Panic";

			[StateTransitionTrigger]
			public static string OnPanic = "onPanic";

			[StateTransitionTrigger]
			public static string OnRecoverFromPanic = "onRecoverFromPanic";

			protected override string OnEnabled() { return OnPanic; }
			protected override string OnDisabled() { return OnRecoverFromPanic; }
		}

		// this machine is retreating back to home position
		public class BackToHome : Signal {
			public static string name = "BackToHome";

			[StateTransitionTrigger]
			public static string OnOutsideArea = "onOutsideArea";

			[StateTransitionTrigger]
			public static string OnBackAtHome = "onBackAtHome";

			protected override string OnEnabled() { return OnOutsideArea; }
			protected override string OnDisabled() { return OnBackAtHome; }
		}

		// 
		public class Collided : Signal {
			public static string name = "Collided";

			[StateTransitionTrigger]
			public static string OnCollisionEnter = "onCollisionEnter";

			protected override string OnEnabled() { return OnCollisionEnter; }
		}

		public class CollisionTrigger : Signal {
			public static string name = "CollisionTrigger";
		}
		//

		// a fire is touching this machine
		public class Burning : Signal {
			public static string name = "Burning";

			[StateTransitionTrigger]
			public const string OnBurning = "onBurning";

			protected override string OnEnabled() { return OnBurning; }
		}

		// something is chewing this machine
		public class Chewing : Signal {
			public static string name = "Chewing";

			[StateTransitionTrigger]
			public static string OnChewing = "onChewing";

			protected override string OnEnabled() { return OnChewing; }
		}

		public class Destroyed : Signal {
			public static string name = "Destroyed";

			[StateTransitionTrigger]
			public const string OnDestroyed = "onDestroyed";

			protected override string OnEnabled() { return OnDestroyed; }
		}
	}*/
}