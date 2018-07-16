using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RailManager {
	private static Dictionary<string, BSpline.BezierSpline> m_rails = new Dictionary<string, BSpline.BezierSpline>();

	public static void RegisterRail(BSpline.BezierSpline _rail) {
        if (m_rails != null && _rail != null && !m_rails.ContainsKey(_rail.name)) {
            m_rails.Add(_rail.name, _rail);
        }
	}

	public static void UnRegisterRail(BSpline.BezierSpline _rail) {
        if (m_rails != null && _rail != null) {
            m_rails.Remove(_rail.name);
        }
	}

	public static BSpline.BezierSpline GetRailByName(string _name) {
        if (m_rails != null &&m_rails.ContainsKey(_name)) {
			return m_rails[_name];
		}
		return null;
	}
}
