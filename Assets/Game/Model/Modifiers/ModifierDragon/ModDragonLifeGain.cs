
public class ModDragonLifeGain : ModifierDragon {
	public const string TARGET_CODE = "life_gain";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModDragonLifeGain(DefinitionNode _def) : base(TARGET_CODE, _def) {
		m_percentage = _def.GetAsFloat("param1");
		BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

    public ModDragonLifeGain(float _percentage) : base(TARGET_CODE) {
        m_percentage = _percentage;
        BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
    }

	public override void Apply() {
		DragonHealthBehaviour healthBehaviour = InstanceManager.player.dragonHealthBehaviour;
		if (healthBehaviour)
			healthBehaviour.AddEatingHpBoost(m_percentage);
	}

    public override void Remove() {
        if (InstanceManager.player != null) {
            DragonHealthBehaviour healthBehaviour = InstanceManager.player.dragonHealthBehaviour;
            if (healthBehaviour)
                healthBehaviour.AddEatingHpBoost(-m_percentage);
        }
    }
}
