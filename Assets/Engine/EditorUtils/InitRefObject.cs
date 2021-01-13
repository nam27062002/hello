using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitRefObject : ScriptableObject {

	public CaletySettings m_settings;
	public List<TextAsset> m_languages = new List<TextAsset>();
	public List<TextAsset> m_definitions = new List<TextAsset>();
	public List<GameObject> m_objects = new List<GameObject>();
    public List<ScriptableObject> m_scriptableObjects = new List<ScriptableObject>();
	public TMPro.TMP_Settings m_textSettings;
	public List<Material> m_materials = new List<Material>();
    public TextAsset m_assetsLut;
	public UnityEngine.Audio.AudioMixer m_audioMixer;
}
