using UnityEngine;
public class AddressablesLoadFromResourcesOp : AddressablesResultOp
{
    private string m_path;
    
    public void Setup(string path)
    {
        m_path = path;
        base.Setup(null);
    }

    public override T GetAsset<T>()
    {
        return Resources.Load<T>(m_path);
    }
}
