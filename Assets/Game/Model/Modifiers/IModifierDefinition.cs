
public interface IModifierDefinition {
	DefinitionNode def { get; }
	string GetIconRelativePath();
	string GetDescription();
	string GetDescriptionShort();
}
