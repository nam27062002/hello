using System.Collections.Generic;

public class ModGamePlaySpawnFrequency : ModifierGamePlay {
	public const string TARGET_CODE = "spawn_frequency";

	//------------------------------------------------------------------------//
	private string[] m_prefabNames;
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModGamePlaySpawnFrequency(DefinitionNode _def) : base(TARGET_CODE, _def) {
		m_prefabNames = _def.GetAsArray<string>("param1", ";");
		m_percentage = _def.GetAsFloat("param2");
		BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

	public override void Apply() {
		for (int i = 0; i < m_prefabNames.Length; ++i) {
			Spawner.AddSpawnFrequency(m_prefabNames[i], m_percentage);
		}
	}

	public override void Remove() {
		for (int i = 0; i < m_prefabNames.Length; ++i) {
			Spawner.RemoveSpawnFrequency(m_prefabNames[i]);
		}
	}
}
