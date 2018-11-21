using UnityEngine;

public class CollectibleRandomRewardEntity : CollectibleEntity {
    //
    [Separator("Rewards")]
    [Comment("Valid rewards: pc, coins, score, xp, health")]
    [SerializeField] private ProbabilitySet m_probability;
    [SerializeField] private string[] m_always;

    //
    private Reward[] m_rewards;


    //
    protected override void Awake() {
        m_rewards = new Reward[m_probability.numElements];
        base.Awake();
    }

    protected override void OnRewardCreated() {
        for (int i = 0; i < m_probability.numElements; ++i) {
            m_rewards[i] = new Reward();

            switch (m_probability.GetLabel(i)) {
                case "pc":      m_rewards[i].pc     = reward.pc;    break;
                case "coins":   m_rewards[i].coins  = reward.coins; break;
                case "score":   m_rewards[i].score  = reward.score; break;
                case "xp":      m_rewards[i].xp     = reward.xp;    break;
                case "health":  m_rewards[i].health = reward.health;break;
            }

            for (int a = 0; a < m_always.Length; ++a) {
                switch (m_always[a]) {
                    case "pc":      m_rewards[i].pc     = reward.pc;    break;
                    case "coins":   m_rewards[i].coins  = reward.coins; break;
                    case "score":   m_rewards[i].score  = reward.score; break;
                    case "xp":      m_rewards[i].xp     = reward.xp;    break;
                    case "health":  m_rewards[i].health = reward.health;break;
                }
            }
        }
    }

    public override Reward GetOnKillReward(DyingReason _reason) {
        return m_rewards[m_probability.GetWeightedRandomElementIdx()];
	}
}
