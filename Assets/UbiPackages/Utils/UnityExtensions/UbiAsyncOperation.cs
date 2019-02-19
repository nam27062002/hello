public interface UbiAsyncOperation
{
    bool allowSceneActivation { get; set; }
    bool isDone { get; }
    float progress { get; }
}
