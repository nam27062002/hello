
public class ModDragonFuryDuration : ModifierDragon {
	public const string TARGET_CODE = "fury_duration";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModDragonFuryDuration(DefinitionNode _def) : base(_def) {
		m_percentage = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		DragonBreathBehaviour breath = InstanceManager.player.breathBehaviour;
		if (breath) {
			breath.AddDurationBonus(m_percentage);
		}
	}
}
