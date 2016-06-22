using AISM;

namespace AI {
	public abstract class Signal {			
		private bool m_value;
		public bool value { get { return m_value; } }

		private Pilot m_pilot;
		public Pilot pilot { set { m_pilot = value; } }

		public void Set(bool _value) {
			if (m_value != _value) {
				if (_value) m_pilot.OnTrigger(OnEnabled());
				else 		m_pilot.OnTrigger(OnDisabled());
						
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
			private static string OnLeaderPromoted = "onLeaderPromoted";

			[StateTransitionTrigger]
			private static string OnLeaderDemoted = "onLeaderDemoted";

			protected override string OnEnabled() { return OnLeaderPromoted; }
			protected override string OnDisabled() { return OnLeaderDemoted; }
		}

		// this machine is hungry and it'll search for preys using the Eater machine component
		public class Hungry : Signal {
			public static string name = "Hungry";

			[StateTransitionTrigger]
			private static string OnIsHungry = "onIsHungry";

			[StateTransitionTrigger]
			private static string OnNotHungry = "onNotHungry";

			protected override string OnEnabled() { return OnIsHungry; }
			protected override string OnDisabled() { return OnNotHungry; }
		}

		// this machine will use it's sensor to detect the player. Later we can extend this to detect all king of enemies
		public class Alert : Signal {
			public static string name = "Alert";

			[StateTransitionTrigger]
			private static string OnAlert = "onAlert";

			[StateTransitionTrigger]
			private static string OnIgnoreAll = "onIgnoreAll";

			protected override string OnEnabled() { return OnAlert; }
			protected override string OnDisabled() { return OnIgnoreAll; }
		}

		// enemy detected nearby
		public class Warning : Signal {
			public static string name = "Warning";

			[StateTransitionTrigger]
			private static string OnWarning = "onWarning";

			[StateTransitionTrigger]
			private static string OnCalm = "onCalm";

			protected override string OnEnabled() { return OnWarning; }
			protected override string OnDisabled() { return OnCalm; }
		}

		// enemy is too close
		public class Danger : Signal {
			public static string name = "Danger";

			[StateTransitionTrigger]
			private static string OnDanger = "onDanger";

			[StateTransitionTrigger]
			private static string OnSafe = "onSafe";

			protected override string OnEnabled() { return OnDanger; }
			protected override string OnDisabled() { return OnSafe; }
		}

		// this machine is unable to perform actions
		public class Panic : Signal {
			public static string name = "Panic";

			[StateTransitionTrigger]
			private static string OnPanic = "onPanic";

			[StateTransitionTrigger]
			private static string OnRecoverFromPanic = "onRecoverFromPanic";

			protected override string OnEnabled() { return OnPanic; }
			protected override string OnDisabled() { return OnRecoverFromPanic; }
		}

		// a fire is touching this machine
		public class Burning : Signal {
			public static string name = "Burning";

			[StateTransitionTrigger]
			private static string OnBurning = "onBurning";

			protected override string OnEnabled() { return OnBurning; }
		}

		// something is chewing this machine
		public class Chewing : Signal {
			public static string name = "Chewing";

			[StateTransitionTrigger]
			private static string OnChewing = "onChewing";

			protected override string OnEnabled() { return OnChewing; }
		}

		public class Destroyed : Signal {
			public static string name = "Destroyed";

			[StateTransitionTrigger]
			private static string OnDestroyed = "onDestroyed";

			protected override string OnEnabled() { return OnDestroyed; }
		}
	}
}