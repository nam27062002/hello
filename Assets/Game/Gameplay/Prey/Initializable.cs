using UnityEngine;

public abstract class Initializable : MonoBehaviour {
		
	protected AreaBounds m_area;

	public abstract void Initialize();
	public virtual void SetAreaBounds(AreaBounds _area) { m_area = _area; }
}
