
public class ModDragonLifeGain : ModifierDragon {
	public const string TARGET_CODE = "life_gain";

	//------------------------------------------------------------------------//
	private float m_value; // percentage

	//------------------------------------------------------------------------//
	public ModDragonLifeGain(DefinitionNode _def) : base(_def) {
		m_value = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		DragonHealthBehaviour healthBehaviour = InstanceManager.player.dragonHealthBehaviour;
		if (healthBehaviour)
			healthBehaviour.AddEatingHpBoost(m_value);
	}

}
