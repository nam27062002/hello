using UnityEngine.SceneManagement;

public interface AddressablesProvider
{
    bool LoadScene(AddressablesCatalogEntry entry, LoadSceneMode mode);
    AddressablesOp UnloadSceneAsync(AddressablesCatalogEntry entry);
}
