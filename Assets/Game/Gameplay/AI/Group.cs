using System.Collections.Generic;

namespace AI {
	public class Group {
		private List<IMachine> m_members;
		private int m_leader;

		public Group() {
			m_leader = -1; // there is no one in charge of this flock
			m_members = new List<IMachine>();
		}

		public void Enter(IMachine _member) {
			m_members.Add(_member);

			if (m_leader < 0) {
				m_leader = 0;
				m_members[0].SetSignal(Signals.Leader.name, true);
			}
		}

		public void Leave(IMachine _member) {
			int index = m_members.IndexOf(_member);

			if (index >= 0) { // found
				m_members.Remove(_member);
				_member.SetSignal(Signals.Leader.name, false);

				if (count == 1) {
					m_leader = -1;
				} else if (m_leader == index) {					
					m_leader = count - 1;
					m_members[m_leader].SetSignal(Signals.Leader.name, true);
				}
			}
		}

		public void ChangeLeader() {
			m_members[m_leader].SetSignal(Signals.Leader.name, false);
			m_leader = (m_leader + 1) % count;
			m_members[m_leader].SetSignal(Signals.Leader.name, true);
		}

		public IMachine leader {
			get { 
				if (m_leader >= 0) {
					return m_members[m_leader];
				}
				return null;
			}
		}

		public int count {
			get {
				return m_members.Count;
			}
		}

		public IMachine this[int i] {
			get {
				return m_members[i];
			}
		}
	}
}