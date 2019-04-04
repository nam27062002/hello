
public class ModGamePlayPetPowerUp : ModifierGamePlay {
	public const string TARGET_CODE = "pet_power_up";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModGamePlayPetPowerUp(DefinitionNode _def) : base(TARGET_CODE, _def) {
		m_percentage = _def.GetAsFloat("param1");
		BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

	public override void Apply() {
		DragonPowerUp.AddPetPowerUpPercentage(m_percentage);
	}

	public override void Remove() {
		DragonPowerUp.AddPetPowerUpPercentage(-m_percentage);
	}
}