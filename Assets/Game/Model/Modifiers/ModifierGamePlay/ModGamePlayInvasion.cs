using System.Collections.Generic;

public class ModGamePlayInvasion : ModifierGamePlay {
	public const string TARGET_CODE = "invasion";

	//------------------------------------------------------------------------//
	private string[] m_prefabNames;
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModGamePlayInvasion(DefinitionNode _def) : base(_def) {
		m_prefabNames = _def.GetAsArray<string>("param1", ";");
		m_percentage = _def.GetAsFloat("param2");
	}

	public override void Apply() {
		for (int i = 0; i < m_prefabNames.Length; ++i) {
			Spawner.AddInvasion(m_prefabNames[i], m_percentage);
		}
	}

	public override void Remove() {
		for (int i = 0; i < m_prefabNames.Length; ++i) {
			Spawner.RemoveInvasion(m_prefabNames[i]);
		}
	}
}
