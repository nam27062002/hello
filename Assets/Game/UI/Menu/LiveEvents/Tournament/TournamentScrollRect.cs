using UnityEngine;

public class TournamentScrollRect : OptimizedScrollRect<TournamentLeaderboardPill, HDTournamentData.LeaderboardLine> {

	private TournamentLeaderboardPill m_playerPill;
	private HDTournamentData.LeaderboardLine m_data;
	private Vector2 m_playerPillSize;

	public void SetupPlayerPill(GameObject _pillPrefab, HDTournamentData.LeaderboardLine _data) {
		if (m_playerPill != null) {
			GameObject.Destroy(m_playerPill);
		}

		m_playerPill = GameObject.Instantiate<GameObject>(_pillPrefab, content, false).GetComponent<TournamentLeaderboardPill>();
		m_playerPill.InitWithData(_data);

		RectTransform rt = m_playerPill.gameObject.GetComponent<RectTransform>();
		m_playerPillSize = rt.sizeDelta;

		m_data = _data;
	}

	protected override void OnScrollMoved() {
		if (m_playerPill != null) {
			Vector2 pillPos = GameConstants.Vector2.zero;
			pillPos.y = GetPillPositionY(m_data.m_rank);

			float relativePos = (m_containerSize.y * 0.5f) - pillPos.y - content.anchoredPosition.y;

			if (relativePos < m_pillSize.y * 0.5f) {
				pillPos.y = (m_containerSize.y * 0.5f) - content.anchoredPosition.y - m_playerPillSize.y * 0.5f;
			} else if (relativePos > m_visibleAreaSize.y - m_pillSize.y * 0.5f) {
				pillPos.y = (m_containerSize.y * 0.5f) - content.anchoredPosition.y - (m_visibleAreaSize.y - m_playerPillSize.y * 0.5f);
			}

			m_playerPill.SetPosition(pillPos);
		}
	}
}
