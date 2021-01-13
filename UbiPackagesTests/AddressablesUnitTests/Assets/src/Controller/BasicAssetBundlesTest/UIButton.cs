using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    /// <summary>
    /// New values have to be added at the end. Messing around with the order of these values would make constants assigned in the scene to be pointing to the wrong values
    /// </summary>
    public enum EId
    {
        None,
        AddAssetCube,
        UnloadCubeAssetBundle,
        AddSceneCubes,
        RemoveSceneCubes,
        ABInit,
        ABReset,
        MemoryCollect,
        UnloadSceneCubesAB
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
