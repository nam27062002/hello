using AISM;

namespace AI {
	public abstract class Signal {			
		private bool m_value;
		public bool value { get { return m_value; } }

		private Machine m_machine;
		public Machine machine { set { m_machine = value; } }

		public void Set(bool _value) {
			if (m_value != _value) {
				if (_value) m_machine.OnTrigger(OnEnabled());
				else 		m_machine.OnTrigger(OnDisabled());
						
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
			protected override string OnDisabled() { return OnIgnoreAll; }
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

		// a fire is touching this machine
		public class Burning : Signal {
			public static string name = "Burning";

			[StateTransitionTrigger]
			public static string OnBurning = "onBurning";

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
			public static string OnDestroyed = "onDestroyed";

			protected override string OnEnabled() { return OnDestroyed; }
		}
	}
}