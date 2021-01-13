using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(AudioSource))]
public class AudioSourceMixerSetup : MonoBehaviour {

	public AudioSource m_audioSource;
	public InstanceManager.MIXER_GROUP outputMixerGroup = InstanceManager.MIXER_GROUP.MASTER;
	// Use this for initialization
	void Awake () {
		if ( m_audioSource == null )
			GetReference();
		m_audioSource.outputAudioMixerGroup = InstanceManager.masterMixerGroups[ (int)outputMixerGroup ];
	}

	// TODO: in editor mode
	public void GetReference() {
		m_audioSource = GetComponent<AudioSource>();
	}
#if UNITY_EDITOR

	[ContextMenu("Auto Setup")]
    void GetReferenceEditor()
    {
        GetReference();
		if (m_audioSource.outputAudioMixerGroup != null)
		{
			switch(m_audioSource.outputAudioMixerGroup.name)
			{
				case "Master": outputMixerGroup = InstanceManager.MIXER_GROUP.MASTER;break;
				case "Music": outputMixerGroup = InstanceManager.MIXER_GROUP.MUSIC;break;
				case "Sfx": outputMixerGroup = InstanceManager.MIXER_GROUP.SFX;break;
				case "Sfx 2D": outputMixerGroup = InstanceManager.MIXER_GROUP.SFX_2D;break;
			}
		}
		m_audioSource.outputAudioMixerGroup = null;
		EditorUtility.SetDirty(gameObject);
    }
#endif		



}
