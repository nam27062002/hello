using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailManager : UbiBCN.SingletonMonoBehaviour<RailManager> {

	private Dictionary<string, BSpline.BezierSpline> m_rails;

	private void Awake() {
		m_rails = new Dictionary<string, BSpline.BezierSpline>();
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
}
