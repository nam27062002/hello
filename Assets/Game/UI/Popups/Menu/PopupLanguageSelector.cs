using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PopupController))]
public class PopupLanguageSelector : MonoBehaviour {

    public Transform[] m_buttonAnchors;
    public SelectableButtonGroup m_buttonsGroup;
    
    public const string PATH = "UI/Popups/Menu/PF_PopupLanguageSelector";
    private const string LANGUAGE_PILL_PATH = "UI/Metagame/Settings/PF_LanguageButton";

    protected List<DefinitionNode> m_languageDefs;
    
    
    void Awake()
    {
    
        // Get all languages
        if(Application.platform == RuntimePlatform.Android) {
            m_languageDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.LOCALIZATION, "android", "true");
        } else if(Application.platform == RuntimePlatform.IPhonePlayer) {
            m_languageDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.LOCALIZATION, "iOS", "true");
        } else {
            m_languageDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.LOCALIZATION);
        }
        
        // Create all buttons
        // Sort definitions by "order" field, create a pill for each language and init with selected language
        DefinitionsManager.SharedInstance.SortByProperty(ref m_languageDefs, "order", DefinitionsManager.SortType.NUMERIC);
        GameObject prefab = Resources.Load<GameObject>(LANGUAGE_PILL_PATH);
        RectTransform buttonRectTransform = null;
        int i = 0;
        for (i = 0; i < m_languageDefs.Count; i++)
        {
            Transform parent = m_buttonAnchors[i % m_buttonAnchors.Length];
            GameObject languageButton = GameObject.Instantiate<GameObject>(prefab, parent, false);
            m_buttonsGroup.buttons.Add( languageButton.GetComponent<SelectableButton>() );
            Transform tr = languageButton.FindTransformRecursive("Text");
            if ( tr )
            {
                tr.GetComponent<UnityEngine.UI.Text>().text = m_languageDefs[i].Get("tidName");
            }
            
            if (buttonRectTransform == null)
            {
                buttonRectTransform = languageButton.GetComponent<RectTransform>();
            }
        }
        
        while( i % m_buttonAnchors.Length != 0 && buttonRectTransform != null)
        {
            // Add things to make it look nice
            GameObject go =  new GameObject("Fill");
            RectTransform rc = go.AddComponent<RectTransform>();
            // These four properties are to be copied
            rc.anchorMin = buttonRectTransform.anchorMin;
            rc.anchorMax = buttonRectTransform.anchorMax;
            rc.anchoredPosition = buttonRectTransform.anchoredPosition;
            rc.sizeDelta = buttonRectTransform.sizeDelta;
            go.transform.parent = m_buttonAnchors[i % m_buttonAnchors.Length];
            i++;
        }
        
        // Select current language
        string currentLangSku = LocalizationManager.SharedInstance.GetCurrentLanguageSKU();
        for(i = 0; i < m_languageDefs.Count; i++) {
            // Is it the selected one?
            if(m_languageDefs[i].sku == currentLangSku) {
                m_buttonsGroup.selectedIdx = i;
                OnSelectionChanged( i, i );
                break;
            }
        }

        m_buttonsGroup.OnSelectionChanged.AddListener(OnSelectionChanged);
    }
    
    public void OnSelectionChanged(int oldIdx, int m_selectedIdx)
    {
        DefinitionNode newLangDef = m_languageDefs[m_selectedIdx];

        // Change localization!
        if(LocalizationManager.SharedInstance.SetLanguage(newLangDef.sku)) 
        {
            // Store new language
            PlayerPrefs.SetString(PopupSettings.KEY_SETTINGS_LANGUAGE, newLangDef.sku);

            // [AOC] If the setting is enabled, replace missing TIDs for english ones
            if(!Prefs.GetBoolPlayer(DebugSettings.SHOW_MISSING_TIDS, false)) {
                LocalizationManager.SharedInstance.FillEmptyTids("lang_english");
            }
        }

        // Notify the rest of the game!
        Messenger.Broadcast(MessengerEvents.LANGUAGE_CHANGED);
    }
}
