using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class Group {
		private List<IMachine> m_members;
		private int m_leader;
        private Vector3[] m_offsets;     

		public Group() {
			m_leader = -1; // there is no one in charge of this flock
			m_members = new List<IMachine>();            
		}        

		public void Enter(IMachine _member) {            
			m_members.Add(_member);

			if (m_leader < 0) {
				m_leader = 0;
				m_members[0].SetSignal(Signals.Type.Leader, true);
			}
		}

		public void Leave(IMachine _member) {
			int index = m_members.IndexOf(_member);

			if (index >= 0) { // found
				m_members.Remove(_member);
				_member.SetSignal(Signals.Type.Leader, false);

				if (count == 0) {
					m_leader = -1;
				} else if (m_leader == index) {
					m_leader = count - 1;
					m_members[m_leader].SetSignal(Signals.Type.Leader, true);
				} else if (m_leader > index) {
					m_leader--;
				}                              
			}
		}

		public void ChangeLeader() {
			m_members[m_leader].SetSignal(Signals.Type.Leader, false);
			m_leader = (m_leader + 1) % count;
			m_members[m_leader].SetSignal(Signals.Type.Leader, true);
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

        public void CreateOffsets(float _radius, int _maxEntities) {
            m_offsets = new Vector3[_maxEntities];            
            Vector2 pos;
            for (int i = 0; i < _maxEntities; i++) {
                pos = UnityEngine.Random.insideUnitCircle * _radius;
                m_offsets[i] = new Vector3(pos.x, pos.y, 0f);
            }
        }

        public bool HasOffsets() {
            return m_offsets != null;
        }

        public Vector3 GetOffset(IMachine machine)
        {
            if (m_offsets != null) {               
                int index = m_members.IndexOf(machine);
                if (index > -1)
                    return m_offsets[index];
            }

            return Vector3.zero;
        }
    }
}