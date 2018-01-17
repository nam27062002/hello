
public interface IViewControl {
	PreyAnimationEvents animationEvents { get; }
	int vertexCount { get; }
	int rendererCount { get; }
	void PreDisable();

	void Spawn(ISpawner _spawner);
}
