
public class ModDragonPetPowerUp : ModifierDragon {
	public const string TARGET_CODE = "pet_power_up";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModDragonPetPowerUp(DefinitionNode _def) : base(_def) {
		m_percentage = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		InstanceManager.player.GetComponent<DragonPowerUp>().AddPetPowerUpPercentage(m_percentage);
	}

	public override void Remove() {
		InstanceManager.player.GetComponent<DragonPowerUp>().AddPetPowerUpPercentage(-m_percentage);
	}
}