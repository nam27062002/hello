using UnityEngine;

public class ConfigSprite : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer m_spriteRenderer;
    
    public void Setup(string addressablesId)
    {
        AddressablesOp op = ApplicationController.Instance.AddressablesManager.LoadAssetAsync(addressablesId);
        op.OnDone = OnAssetLoaded;
    }		

    private void OnAssetLoaded(AddressablesOp op)
    {
        if (op.Error == null)
        {
            m_spriteRenderer.sprite = op.GetAsset<Sprite>();
        }
    }
}
