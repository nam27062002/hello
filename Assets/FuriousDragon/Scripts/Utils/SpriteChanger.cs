using UnityEngine;
using System.Collections;

public class SpriteChanger : MonoBehaviour {

	public string spritesheet;

	UnityEngine.Sprite[] fighterSprites;
	SpriteRenderer sprRenderer;
	bool init = false;

	void Load () {
		init = true;
		fighterSprites = Resources.LoadAll<UnityEngine.Sprite>(spritesheet);
		sprRenderer = GetComponent<SpriteRenderer>();
	}
	
	public void SetSprite(int sp){

		if (!init)
			Load ();

		sprRenderer.sprite = fighterSprites[sp];
	}
}
