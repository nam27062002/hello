
public abstract class IViewControl : ISpawnable {
	public abstract PreyAnimationEvents animationEvents { get; }
	public abstract int vertexCount { get; }
	public abstract int rendererCount { get; }
    public abstract float freezeParticleScale { get; }

    public abstract void PreDisable();

	public abstract void ForceGolden();

    public abstract void Freezing(float freezeLevel);
}
