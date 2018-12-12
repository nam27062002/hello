using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CPGameplayCheats : MonoBehaviour {
    [SerializeField] private TMP_InputField m_textInput;
    [SerializeField] private TMP_InputField m_numberInput;


    //--------------------------------------------------------------------------
    public void OnAddTime()     { InstanceManager.gameSceneControllerBase.elapsedSeconds += long.Parse(m_numberInput.text); }
    public void OnRemoveTime()  { InstanceManager.gameSceneControllerBase.elapsedSeconds -= long.Parse(m_numberInput.text); }
    public void OnSetTime()     { InstanceManager.gameSceneControllerBase.elapsedSeconds  = long.Parse(m_numberInput.text); }
    //--------------------------------------------------------------------------

    //--------------------------------------------------------------------------
    public void OnAddXP() {
        Reward reward = new Reward();
        reward.xp = long.Parse(m_numberInput.text);
        RewardManager.instance.OnApplyCheatsReward(reward);
    }

    public void OnRemoveXP() {
        Reward reward = new Reward();
        reward.xp = -long.Parse(m_numberInput.text);
        RewardManager.instance.OnApplyCheatsReward(reward);
    }

    public void OnSetXP() {
        Reward reward = new Reward();
        reward.xp = long.Parse(m_numberInput.text) - RewardManager.xp;
        RewardManager.instance.OnApplyCheatsReward(reward);
    }
    //--------------------------------------------------------------------------

    //--------------------------------------------------------------------------
    public void OnSetCategoryKill() {
        RewardManager.instance.SetCategoryKill(m_textInput.text, int.Parse(m_numberInput.text));
    }
    public void OnSetNPCKill() {
        RewardManager.instance.SetNPCKill(m_textInput.text, int.Parse(m_numberInput.text));
    }
    //--------------------------------------------------------------------------
}
