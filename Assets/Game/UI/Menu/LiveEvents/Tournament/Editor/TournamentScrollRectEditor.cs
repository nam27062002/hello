//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEditor;
using UnityEditor.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the SnapScrollRect class.
/// </summary>
[CustomEditor(typeof(TournamentScrollRect), true)]
[CanEditMultipleObjects]
public class TournamentScrollRectEditor : OptimizedScrollRectEditor { }