using UnityEngine;
using UnityEngine.UI;

public class TournamentScrollRect : OptimizedScrollRect<TournamentLeaderboardPill, HDTournamentData.LeaderboardLine> {

	private int m_index;
	private TournamentLeaderboardPill m_playerPill;
	private Vector2 m_playerPillSize;

	public void SetupPlayerPill(GameObject _pillPrefab, int _index, HDTournamentData.LeaderboardLine _data) {
		if (m_playerPill != null) {
			GameObject.Destroy(m_playerPill.gameObject);
		}

		m_playerPill = GameObject.Instantiate<GameObject>(_pillPrefab, content, false).GetComponent<TournamentLeaderboardPill>();
		m_playerPill.InitWithData(_data);
		m_playerPill.GetComponent<Button>().onClick.AddListener(OnPlayerPillClick);

		m_index = _index;

		RectTransform rt = m_playerPill.gameObject.GetComponent<RectTransform>();
		m_playerPillSize = rt.sizeDelta;
	}

	protected override void LateUpdate() {
		base.LateUpdate();
		OnScrollMoved();
	}

	protected override void OnScrollMoved() {
		if (m_playerPill != null) {
			Vector2 pos = GetPillPosition(m_index);
			Vector2 relativePos = (m_containerSize * 0.5f) - pos - content.anchoredPosition;

			if (relativePos.y < m_playerPillSize.y * 0.5f) {
				pos.y = (m_containerSize.y * 0.5f) - content.anchoredPosition.y - m_playerPillSize.y * 0.5f;
			} else if (relativePos.y > m_visibleAreaSize.y - m_playerPillSize.y * 0.5f) {
				pos.y = (m_containerSize.y * 0.5f) - content.anchoredPosition.y - (m_visibleAreaSize.y - m_playerPillSize.y * 0.5f);
			}
			m_playerPill.SetPosition(pos);
		}
	}

	protected override void OnPillCreated() {
		// Player pill should be the last one in the hierarchy (so it is drawed on top)
		m_playerPill.transform.SetAsLastSibling();
	}

	private void OnPlayerPillClick() {
		FocusOn(m_index);
	}
}
