
public interface IModifierDefinition {
    string GetSku();
    string GetUICategory();
    string GetIconRelativePath();
    string GetName();
    string GetDescription();
    string GetDescriptionShort();

    SimpleJSON.JSONClass ToJson();
}
