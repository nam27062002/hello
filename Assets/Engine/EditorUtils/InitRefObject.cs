using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitRefObject : ScriptableObject {

	public CaletySettings m_settings;
	public List<TextAsset> m_languages = new List<TextAsset>();
	public List<TextAsset> m_definitions = new List<TextAsset>();
	public List<GameObject> m_objects = new List<GameObject>();
}
