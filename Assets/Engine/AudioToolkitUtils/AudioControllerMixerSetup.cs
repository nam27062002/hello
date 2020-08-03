using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(AudioController))]
public class AudioControllerMixerSetup : MonoBehaviour {

	public AudioController m_controller;
	
	void Awake(){
		if ( m_controller == null )
			GetReference();
		int count = m_controller.AudioCategories.Length;
		for (int i = 0; i < count; i++)
		{
			string name = m_controller.AudioCategories[i].Name;
			switch( name )
			{
				case "SFX":
				{
					m_controller.AudioCategories[i].audioMixerGroup = InstanceManager.masterMixerGroups[ (int)InstanceManager.MIXER_GROUP.SFX];
				}break;
				case "MUSIC": 
				{
					m_controller.AudioCategories[i].audioMixerGroup = InstanceManager.masterMixerGroups[ (int)InstanceManager.MIXER_GROUP.MUSIC];
				}break;
				case "SFX 2D": 
				{
					m_controller.AudioCategories[i].audioMixerGroup = InstanceManager.masterMixerGroups[ (int)InstanceManager.MIXER_GROUP.SFX_2D];
				}break;
			}
		}
	}

	public void GetReference() {
		m_controller = GetComponent<AudioController>();
	}

#if UNITY_EDITOR

	[ContextMenu("Auto Setup")]
    void GetReferenceEditor()
    {
        GetReference();
		int count = m_controller.AudioCategories.Length;
		for (int i = 0; i < count; i++)
		{
			m_controller.AudioCategories[i].audioMixerGroup = null;
		}
		EditorUtility.SetDirty(gameObject);
    }
#endif		

}
