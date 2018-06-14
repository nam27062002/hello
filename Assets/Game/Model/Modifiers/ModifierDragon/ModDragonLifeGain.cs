
public class ModDragonLifeGain : ModifierDragon {
	public const string TARGET_CODE = "life_gain";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModDragonLifeGain(DefinitionNode _def) : base(_def) {
		m_percentage = _def.GetAsFloat("param1");
		m_textParam = m_percentage + "%";
	}

	public override void Apply() {
		DragonHealthBehaviour healthBehaviour = InstanceManager.player.dragonHealthBehaviour;
		if (healthBehaviour)
			healthBehaviour.AddEatingHpBoost(m_percentage);
	}

}
