using UnityEngine;

public abstract class ISpawnable : MonoBehaviour {
	public abstract void Spawn(ISpawner _spawner);
	public abstract void CustomUpdate();
}
