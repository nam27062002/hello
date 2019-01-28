// UITooltipMultidirectional.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialization of the UITooltip with support for multiple directions.
/// </summary>
public class UITooltipMultidirectional : UITooltip {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum ShowDirection {
		LEFT,
		RIGHT,
		UP,
		DOWN
	};

	public enum BestDirectionOptions {
		ALL_DIRECTIONS,
		HORIZONTAL_ONLY,
		VERTICAL_ONLY
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Separator("UITooltipMultidirectional")]
	[SerializeField] protected RectTransform m_arrowLeft = null;
	[SerializeField] protected RectTransform m_arrowRight = null;
	[SerializeField] protected RectTransform m_arrowTop = null;
	[SerializeField] protected RectTransform m_arrowBottom = null;
	
	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Adapts the tooltip to work in a specific direction.
	/// In order to support this feature, the tooltip must leave the proper margins
	/// for the arrow to be placed in any side.
	/// </summary>
	/// <param name="_dir">Target direction.</param>
	public void SetupDirection(ShowDirection _dir) {
		// Setup config vars based on target direction
		Vector2 pivotPoint = GameConstants.Vector2.center;
		ShowHideAnimator.TweenType anim = ShowHideAnimator.TweenType.FADE;
		switch(_dir) {
			case ShowDirection.DOWN: {
					m_arrow = m_arrowTop;
					m_arrowDir = ArrowDirection.HORIZONTAL;
					pivotPoint.y = 1f;
					anim = ShowHideAnimator.TweenType.DOWN;
				}
				break;

			case ShowDirection.UP: {
					m_arrow = m_arrowBottom;
					m_arrowDir = ArrowDirection.HORIZONTAL;
					pivotPoint.y = 0f;
					anim = ShowHideAnimator.TweenType.UP;
				}
				break;

			case ShowDirection.LEFT: {
					m_arrow = m_arrowRight;
					m_arrowDir = ArrowDirection.VERTICAL;
					pivotPoint.x = 1f;
					anim = ShowHideAnimator.TweenType.LEFT;
				}
				break;

			case ShowDirection.RIGHT: {
					m_arrow = m_arrowLeft;
					m_arrowDir = ArrowDirection.VERTICAL;
					pivotPoint.x = 0f;
					anim = ShowHideAnimator.TweenType.RIGHT;
				}
				break;
		}

		// Apply rest of the values
		// Arrow
		RectTransform[] arrows = { m_arrowLeft, m_arrowRight, m_arrowTop, m_arrowBottom };
		for(int i = 0; i < arrows.Length; ++i) {
			if(arrows[i] != null) {
				arrows[i].gameObject.SetActive(m_arrow == arrows[i]);
			}
		}

		// Pivot point
		RectTransform rt = this.transform as RectTransform;
		rt.pivot = pivotPoint;

		// Animation
		animator.tweenType = anim;
		animator.RecreateTween();	// Force instance tween recreation, since the Setup is most likely done right before launching the animation
	}

	/// <summary>
	/// Compute the best direction to launch a tooltip from a specific screen position.
	/// </summary>
	/// <returns>The best direction.</returns>
	/// <param name="_pos">Position to be evaluated, global coords in World units.</param>
	/// <param name="_options">Allowed directions.</param>
	public ShowDirection CalculateBestDirection(Vector3 _pos, BestDirectionOptions _options) {
		// Position will be compared with this tooltip's parent canvas
		Canvas canvas = GetComponentInParent<Canvas>();
		Rect canvasRect = (canvas.transform as RectTransform).rect; // Canvas in local coords
		Vector3 localPos = canvas.transform.InverseTransformPoint(_pos);

		// Compute best direction based on screen pos and options
		switch(_options) {
			case BestDirectionOptions.HORIZONTAL_ONLY: {
				if(localPos.x < canvasRect.center.x) {
					return ShowDirection.RIGHT;
				} else {
					return ShowDirection.LEFT;
				}
			} break;

			case BestDirectionOptions.VERTICAL_ONLY: {
				if(localPos.y < canvasRect.center.y) {
					return ShowDirection.UP;
				} else {
					return ShowDirection.DOWN;
				}
			} break;

			case BestDirectionOptions.ALL_DIRECTIONS: {
				// Find farthest canvas edge and move in that direction
				// Ideally we should consider tooltip size, but let's leave that for another time ^^
				float maxDist = 0f;
				float d = 0f;
				ShowDirection dir = ShowDirection.UP;

				// Left edge
				d = localPos.x - canvasRect.xMin;
				if(d > maxDist) {
					maxDist = d;
					dir = ShowDirection.LEFT;
				}
				//Debug.Log(Colors.fuchsia.Tag("LEFT " + localPos.x + " - " + canvasRect.xMin + " = " + d + " | " + maxDist + " | " + dir));

				// Right edge
				d = canvasRect.xMax - localPos.x;
				if(d > maxDist) {
					maxDist = d;
					dir = ShowDirection.RIGHT;
				}
				//Debug.Log(Colors.fuchsia.Tag("RIGHT " + canvasRect.xMax + " - " + localPos.x + " = " + d + " | " + maxDist + " | " + dir));

				// Bottom edge
				d = localPos.y - canvasRect.yMin;
				if(d > maxDist) {
					maxDist = d;
					dir = ShowDirection.DOWN;
				}
				//Debug.Log(Colors.fuchsia.Tag("DOWN " + localPos.y + " - " + canvasRect.yMin + " = " + d + " | " + maxDist + " | " + dir));

				// Top edge
				d = canvasRect.yMax - localPos.y;
				if(d > maxDist) {
					maxDist = d;
					dir = ShowDirection.UP;
				}
				//Debug.Log(Colors.fuchsia.Tag("UP " + canvasRect.yMax + " - " + localPos.y + " = " + d + " | " + maxDist + " | " + dir));

				return dir;
			} break;
		}

		return ShowDirection.UP;
	}
}