#if UNITY_EDITOR
using UnityEditor;

public class AddressablesLoadFromEditorOp : AddressablesResultOp
{
    private string m_path;
    
    public void Setup(string path)
    {
        m_path = path;
        base.Setup(null);
    }

    public override T GetAsset<T>()
    {
        return AssetDatabase.LoadAssetAtPath<T>(m_path);
    }
}
#endif
