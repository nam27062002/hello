using UnityEngine.SceneManagement;

public interface AddressablesProvider
{
    AddressablesOp LoadDependenciesAsync(AddressablesCatalogEntry entry);
    void UnloadDependencies(AddressablesCatalogEntry entry);

    bool LoadScene(AddressablesCatalogEntry entry, LoadSceneMode mode);
    AddressablesOp LoadSceneAsync(AddressablesCatalogEntry entry, LoadSceneMode mode);
    AddressablesOp UnloadSceneAsync(AddressablesCatalogEntry entry);
}
