using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailManager : UbiBCN.SingletonMonoBehaviour<RailManager> {

	private Dictionary<string, BSpline.BezierSpline> m_rails = new Dictionary<string, BSpline.BezierSpline>();

	private void OnEnable() {
		Messenger.AddListener(GameEvents.GAME_AREA_EXIT, Clear);
		Messenger.AddListener(GameEvents.GAME_ENDED, Clear);
	}

	private void OnDisable() {
		Messenger.RemoveListener(GameEvents.GAME_AREA_EXIT, Clear);
		Messenger.RemoveListener(GameEvents.GAME_ENDED, Clear);
	}        


	public static void RegisterRail(BSpline.BezierSpline _rail) {
		instance.m_rails.Add(_rail.name, _rail);
	}

	public static BSpline.BezierSpline GetRailByName(string _name) {
		if (instance.m_rails.ContainsKey(_name)) {
			return instance.m_rails[_name];
		}
		return null;
	}

	private void Clear() {
		m_rails.Clear();
	}
}
