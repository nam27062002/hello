using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class Actions {
	[Serializable]
	public class Action {
		[HideInInspector] public Id id;
		[SerializeField] public bool active;

		public Action() {
			id = Actions.Id.Panic;
			active = false;
		}

		public Action(Id _id, bool _active) {
			id = _id;
			active = _active;
		}
	}

	public enum Id {
		Panic = 0,
		Jump,
		GoOn,
		Hide,
		Home,
	}

	[SerializeField] private Action m_panic = new Action(Actions.Id.Panic, false);
	[SerializeField] private Action m_jump 	= new Action(Actions.Id.Jump, false);
	[SerializeField] private Action m_goOn = new Action(Actions.Id.GoOn, true);
	[SerializeField] private Action m_hide = new Action(Actions.Id.Hide, false);
	private Action m_home = new Action(Actions.Id.Home, true);


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

		//----------------------------------------------------
		return availableActions.GetRandomValue();
	}

	public Action GetDefaultAction() {
		if (m_goOn.active && UnityEngine.Random.Range(0f, 1f) < 0.5f) {
			return m_goOn;
		} else {
			return m_home;
		}
	}
}
