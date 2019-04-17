
public interface IViewControl {
	PreyAnimationEvents animationEvents { get; }
	int vertexCount { get; }
	int rendererCount { get; }
    float freezeParticleScale { get; }

    void PreDisable();

	void Spawn(ISpawner _spawner);

	void ForceGolden();
}
