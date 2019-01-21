using UnityEngine;
using UnityEngine.UI;

public class LeaguesScrollRect : OptimizedScrollRect<LeaguesLeaderboardPill, LeaguesLeaderboardPillData> {
	// Internal References
	private LeaguesPlayerInfoTooltip m_tooltip = null;

	// Internal Logic
	private int m_playerIndex;
	private LeaguesLeaderboardPill m_playerPill;
	private Vector2 m_playerPillSize;

	public void SetupTooltip(LeaguesPlayerInfoTooltip _tooltip) {
		m_tooltip = _tooltip;
	}

	public void SetupPlayerPill(GameObject _pillPrefab, int _pillIndex, LeaguesLeaderboardPillData _data) {
		if (m_playerPill != null) {
			GameObject.Destroy(m_playerPill.gameObject);
		}

		if(_pillIndex < 0) return;
		if(_data == null) return;
		if(_pillPrefab == null) return;

		m_playerPill = GameObject.Instantiate<GameObject>(_pillPrefab, content, false).GetComponent<LeaguesLeaderboardPill>();
		m_playerPill.InitWithData(_data);
		m_playerPill.GetComponent<Button>().onClick.AddListener(OnPlayerPillClick);

		m_playerIndex = _pillIndex;

		RectTransform rt = m_playerPill.gameObject.GetComponent<RectTransform>();
		m_playerPillSize = rt.sizeDelta;
	}

	public void FocusPlayerPill(bool _animate) {
		FocusOn(m_playerIndex, _animate);
	}

	protected override void LateUpdate() {
		base.LateUpdate();
		OnScrollMoved();
	}

	protected override void OnScrollMoved() {
		if (m_playerPill != null) {
			Vector2 pos = GetPillPosition(m_playerIndex);
			Vector2 relativePos = (m_containerSize * 0.5f) - pos - content.anchoredPosition;

			if (relativePos.y < m_playerPillSize.y * 0.5f) {
				pos.y = (m_containerSize.y * 0.5f) - content.anchoredPosition.y - m_playerPillSize.y * 0.5f;
			} else if (relativePos.y > m_visibleAreaSize.y - m_playerPillSize.y * 0.5f) {
				pos.y = (m_containerSize.y * 0.5f) - content.anchoredPosition.y - (m_visibleAreaSize.y - m_playerPillSize.y * 0.5f);
			}
			m_playerPill.SetPosition(pos);
		}
	}

	protected override void OnPillCreated(LeaguesLeaderboardPill _pill) {
		// Player pill should be the last one in the hierarchy (so it is drawed on top)
		if(m_playerPill != null) m_playerPill.transform.SetAsLastSibling();

		// Setup tooltip trigger
		if(m_tooltip != null) _pill.SetupTooltip(m_tooltip);
	}

	private void OnPlayerPillClick() {
		FocusPlayerPill(true);
	}
}
