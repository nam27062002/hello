using System;
using UnityEngine;


[Serializable]
public class ColorRange {
    [SerializeField] private Color a;
    [SerializeField] private Color b;

    public Color GetA() {
        return a;
    }

    public Color GetB() {
        return b;
    }

    public Color GetRandom() {
        float t = UnityEngine.Random.Range(0f, 1f);
        return Color.Lerp(a, b, t);
    }
}