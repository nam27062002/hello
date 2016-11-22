using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class Group {
		private static Vector3[] m_offsetsSunflower;

		private List<IMachine> m_members;
		private int m_leader;
        

		public Group() {
			m_leader = -1; // there is no one in charge of this flock
			m_members = new List<IMachine>();     

			if (m_offsetsSunflower == null) {
				CreateOffsets(100);
			}
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

		private void CreateOffsets(int _maxEntities) {
            m_offsetsSunflower = new Vector3[_maxEntities];            
            for (int i = 0; i < _maxEntities; i++) {
				m_offsetsSunflower[i] = Sunflower(i + 1, _maxEntities);
            }
        }

        public bool HasOffsets() {
            return m_offsetsSunflower != null;
        }

		public Vector3 GetOffset(IMachine machine, float _radius)
        {
            if (m_offsetsSunflower != null) {               
                int index = m_members.IndexOf(machine);
                if (index > -1)
                    return m_offsetsSunflower[index] * _radius;
            }

            return Vector3.zero;
        }

		public Vector3 Sunflower(int _i, int _n) {
			float b = 0; //round(alpha*sqrt(n));
			float phi = (Mathf.Sqrt(5f) + 1) / 2f;

			float r = SunflowerRadius(_i, _n, b);
			float theta = (2 * Mathf.PI * _i) / (phi * phi);

			return new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), 0f) * 6f; // x6 so the separation is around 1 unit between points
		}

		public float SunflowerRadius(int _i, int _n, float _b) {
			if (_i > _n - _b) {
				return 1f;
			} else {
				return Mathf.Sqrt(_i - 0.5f) / Mathf.Sqrt(_n - ((_b + 1) / 2f));
			}
		}
    }
}