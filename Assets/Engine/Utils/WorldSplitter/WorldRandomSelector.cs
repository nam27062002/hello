using UnityEngine;

public class WorldRandomSelector : MonoBehaviour {
    [SerializeField] private ProbabilitySet m_probabilities = new ProbabilitySet();
    [SerializeField] private GameObject[] m_elements = new GameObject[0];
    public void RefreshElementCount() {
        int count = m_probabilities.numElements;
        if (count != m_elements.Length) {
            System.Array.Resize<GameObject>(ref m_elements, count);
        }
    }

	// Use this for initialization
	void Awake () {
        int index = m_probabilities.GetWeightedRandomElementIdx();
        if (m_elements[index] != null) {
            GameObject.Destroy(m_elements[index]);
        }

        MonoBehaviour.Destroy(this);
	}
}
