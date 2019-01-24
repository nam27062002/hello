//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEditor;
using UnityEditor.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the LeaguesScrollRect class.
/// </summary>
[CustomEditor(typeof(LeaguesScrollRect), true)]
[CanEditMultipleObjects]
public class LeaguesScrollRectEditor : OptimizedScrollRectEditor { }