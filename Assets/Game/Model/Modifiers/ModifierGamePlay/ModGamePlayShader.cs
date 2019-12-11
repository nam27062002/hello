
using UnityEngine;

public class ModGamePlayShader : ModifierGamePlay {
	public const string TARGET_CODE = "shader";

	//------------------------------------------------------------------------//
	string m_subType;
	string m_value;

	//------------------------------------------------------------------------//
	public ModGamePlayShader(DefinitionNode _def) : base(TARGET_CODE, _def) {
		m_subType = _def.GetAsString("param1");
		m_value = _def.GetAsString("param2");
	}

	public override bool isLateModifier(){ 
		return true;
	}

	public override void Apply() {
		switch( m_subType )
		{
			case "keyword":
			{
				Shader.EnableKeyword(m_value);
			}break;
		}
	}

	public override void Remove () {
		switch( m_subType )
		{
			case "keyword":
			{
				Shader.DisableKeyword(m_value);
			}break;
		}
	}
}
