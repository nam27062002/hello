using System.Collections.Generic;

public class ModGamePlaySceneSelector : ModifierGamePlay {
	public const string TARGET_CODE = "scene_selector";

	//------------------------------------------------------------------------//
	private Dictionary<string, string[]> m_scenesToInclude;
    private Dictionary<string, string[]> m_scenesToExclude;


    //------------------------------------------------------------------------//
    public ModGamePlaySceneSelector(DefinitionNode _def) : base(TARGET_CODE, _def) {
        m_scenesToInclude = new Dictionary<string, string[]>();
        ParseParams(_def.Get("param1"), m_scenesToInclude);

        m_scenesToExclude = new Dictionary<string, string[]>();
        ParseParams(_def.Get("param2"), m_scenesToExclude);
    }

	public override void Apply() {
        ApplyDictionary(m_scenesToInclude, true);
        ApplyDictionary(m_scenesToExclude, false);
    }

	public override void Remove() {
        RemoveDictionary(m_scenesToInclude, true);
        RemoveDictionary(m_scenesToExclude, false);
    }

    //Format
    //  area1:scene1,scene2;area2:scene3,scene4;
    private void ParseParams(string _param, Dictionary<string, string[]> _dict) {
        string[] areas = _param.Split(';');
        foreach (string area in areas) {
            string[] data = area.Split(':');
            _dict.Add(data[0], data[1].Split(','));
        }
    }

    private void ApplyDictionary(Dictionary<string, string[]> _dict, bool _include) {
        foreach (KeyValuePair<string, string[]> pair in _dict) {
            foreach (string scene in pair.Value) {
                if (_include)   LevelManager.AddSceneToInclude(pair.Key, scene);
                else            LevelManager.AddSceneToExclude(pair.Key, scene);
            }
        }
    }

    private void RemoveDictionary(Dictionary<string, string[]> _dict, bool _include) {
        foreach (KeyValuePair<string, string[]> pair in _dict) {
            foreach (string scene in pair.Value) {
                if (_include)   LevelManager.RemoveSceneToInclude(pair.Key, scene);
                else            LevelManager.RemoveSceneToExclude(pair.Key, scene);
            }
        }
    }
}
