using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class Actions {
	[Serializable]
	public class Action {
		[HideInInspector] public Id id;
		[SerializeField] public bool active;
		[SerializeField] public float probability;

		public Action() {
			id = Actions.Id.Panic;
			active = false;
			probability = 1;
		}

		public Action(Id _id, bool _active, float _probability) {
			id = _id;
			active = _active;
			probability = _probability;
		}
	}

	public enum Id {
		Panic = 0,
		Jump,
		GoOn,
		Hide,
		Home,
	}

	[SerializeField] private Action m_panic = new Action(Actions.Id.Panic, false, 1);
	[SerializeField] private Action m_jump 	= new Action(Actions.Id.Jump, false, 1);
	[SerializeField] private Action m_goOn = new Action(Actions.Id.GoOn, true, 1);
	[SerializeField] private Action m_hide = new Action(Actions.Id.Hide, false, 1);
	private Action m_home = new Action(Actions.Id.Home, true, 1);

	//

	public Action GetAction(ref Actions _entityActions) {
		List<Action> availableActions = new List<Action>();

		if (m_panic.active && _entityActions.m_panic.active) {
			availableActions.Add(m_panic);
		}

		if (m_jump.active && _entityActions.m_jump.active) {
			availableActions.Add(m_jump);
		}

		if (m_goOn.active && _entityActions.m_goOn.active) {
			availableActions.Add(m_goOn);
		}

		if (m_hide.active && _entityActions.m_hide.active) {
			availableActions.Add(m_hide);
		}

		if ( availableActions.Count > 0 ) {
			float totalProbability = 0;
			for( int i = 0; i<availableActions.Count; i++ )
				totalProbability += availableActions[i].probability;
			float rand = UnityEngine.Random.Range(0.0f, totalProbability - Mathf.Epsilon);
			for( int i = 0; i<availableActions.Count; i++ )
			{
				Action act = availableActions[i];
				if ( rand < act.probability){
					return availableActions[i];
				}
				rand -= act.probability;
			}
		}
		return null;

	}

	public Action GetDefaultAction() {
		if (m_goOn.active && UnityEngine.Random.Range(0f, 1f) < 0.5f) {
			return m_goOn;
		} else {
			return m_home;
		}
	}
}
