
public class OnDieStatus {
	public bool isInFreeFall = false;
	public bool isPressed_ActionA = false;
	public bool isPressed_ActionB = false;
	public bool isPressed_ActionC = false;

	public IEntity.Type source = IEntity.Type.OTHER; 
	public IEntity.DyingReason reason = IEntity.DyingReason.OTHER;
}
