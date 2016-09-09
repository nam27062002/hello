#if UNITY_EDITOR
using UnityEditor;

public static class EditorBuildSettingsSceneExtensions
{
	public static string SceneName(this EditorBuildSettingsScene instance)
	{
		var sceneName = instance.path.Substring(instance.path.LastIndexOf('/') + 1);
		sceneName = sceneName.Remove(sceneName.IndexOf(".unity"));
		return sceneName;
	}
}
#endif