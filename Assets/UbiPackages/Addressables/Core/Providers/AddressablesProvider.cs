using UnityEngine.SceneManagement;

public abstract class AddressablesProvider
{
    public virtual AddressablesOp LoadDependenciesAsync(AddressablesCatalogEntry entry)
    {
        // By default there's no dependencies
        AddressablesOpResult returnValue = new AddressablesOpResult();
        returnValue.Setup(null, null);

        return returnValue;
    }

    public virtual void UnloadDependencies(AddressablesCatalogEntry entry) {}

    public abstract bool LoadScene(AddressablesCatalogEntry entry, LoadSceneMode mode);
    public abstract AddressablesOp LoadSceneAsync(AddressablesCatalogEntry entry, LoadSceneMode mode);
    public abstract AddressablesOp UnloadSceneAsync(AddressablesCatalogEntry entry);
}
