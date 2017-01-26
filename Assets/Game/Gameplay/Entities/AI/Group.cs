﻿using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class Group {
		public enum Formation {
			SunFlower = 0,
			Triangle
		}

		private static Vector3[] m_offsetsSunflower;
		private static Vector3[] m_offsetsTriangle;
		private static float[] m_triangleRows;

		private List<IMachine> m_members;
		private int m_leader;
        
		private Formation m_formation;
		public Formation formation { get { return m_formation; } }

		private Quaternion m_lastrotation;
		private Quaternion m_rotation;


		public Group() {
			m_leader = -1; // there is no one in charge of this flock
			m_members = new List<IMachine>();     

			if (m_offsetsSunflower == null) {
				CreateOffsets(100);
			}

			m_formation = Formation.SunFlower;

			m_lastrotation = m_rotation = Quaternion.identity;
		}

		public void SetFormation(Formation _formation) {
			m_formation = _formation;
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

			m_offsetsTriangle = new Vector3[100];
			m_triangleRows = new float[100];
			Triangle(100);
        }

        public bool HasOffsets() {
            return m_offsetsSunflower != null;
        }

		public void UpdateRotation(Vector3 _dir) {
			float angle = Vector3.Angle(Vector3.down, _dir);
			if (_dir.x < 0)
				angle *= -1;
			m_lastrotation = m_rotation;
			m_rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}

		public Vector3 GetOffset(IMachine machine, float _radius) {
			int index = m_members.IndexOf(machine);

			if (index > -1) {				
				if (m_formation == Formation.Triangle) {
					if (m_offsetsTriangle != null) {
						return Quaternion.Slerp(m_lastrotation, m_rotation, 1f / m_triangleRows[index]) * (m_offsetsTriangle[index] * _radius);
					}
				} else {
		            if (m_offsetsSunflower != null) {
						return m_rotation * (m_offsetsSunflower[index] * _radius);
		            }
				}
			}

            return Vector3.zero;
        }

		public Vector3 Sunflower(int _i, int _n) {
			float b = 0;
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

		public Vector3 GetSunflowerPosAt(int _i) {
			return m_rotation * m_offsetsSunflower[_i];
		}

		public void Triangle(int _maxEntities) {
			float factor = 1f;

			int row = 0;
			int entitiesInThisRow = 0;

			float y = 0f;

			while (_maxEntities > 0) {				
				entitiesInThisRow++;
				entitiesInThisRow = Mathf.Min(entitiesInThisRow, _maxEntities);

				//
				if (row == 0) {
					m_offsetsTriangle[row] = new Vector3(0, y, 0);
					m_triangleRows[row] = y + 1f;
				} else {
					float w = factor * (entitiesInThisRow - 1);
					for (int i = 0; i < entitiesInThisRow; i++) {
						m_offsetsTriangle[row + i] = new Vector3((-w * 0.5f) + (i * factor), y, 0);
						m_triangleRows[row + i] = y + 1f;
					}
				}
				//

				y += factor;
				row += entitiesInThisRow;

				_maxEntities -= entitiesInThisRow;
			}

			for (int i = 0; i < _maxEntities; i++) {
				m_offsetsTriangle[i].x /= (2f * m_triangleRows[i]);
			}
		}

		public Vector3 GetTrianglePosAt(int _i) {
			return m_rotation * m_offsetsTriangle[_i];
		}
    }
}