﻿using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SoftMasking.Samples {
    public class SoftMaskSampleChooser : MonoBehaviour {
        public Dropdown dropdown;
        public Text fallbackLabel;

        public void Start() {
            var activeSceneName = SceneManager.GetActiveScene().name;
#if UNITY_EDITOR
            dropdown.options.RemoveAll(x => !IsSceneInBuild(x.text));
            if (dropdown.options.Count == 0) {
                dropdown.gameObject.SetActive(false);
                fallbackLabel.gameObject.SetActive(true);
                fallbackLabel.text = activeSceneName;
            }
#endif
            dropdown.value = dropdown.options.FindIndex(x => x.text == activeSceneName);
            dropdown.onValueChanged.AddListener(Choose);
        }

        public void Choose(int sampleIndex) {
            var sceneName = dropdown.options[sampleIndex].text;
            SceneManager.LoadScene(sceneName);
        }

        
#if UNITY_EDITOR
        bool IsSceneInBuild(string sceneName) {
            return 
                EditorBuildSettings.scenes.Any(
                    s => Path.GetFileNameWithoutExtension(s.path) == sceneName && s.enabled);
        }
#endif
    }
}
