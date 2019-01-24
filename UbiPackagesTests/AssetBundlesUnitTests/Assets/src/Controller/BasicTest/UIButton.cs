using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    public enum EId
    {
        None,
        AddAssetCube,
        UnloadCubeAssetBundle,
        AddSceneCubes,
        RemoveSceneCubes
    };

    public EId m_id = EId.None;

    public Button Button { get; set; }

    void Awake()
    {
        Button = GetComponent<Button>();                
    }

    public bool interactable
    {
        get { return Button.interactable; }
        set { Button.interactable = value; }
    }    
}
