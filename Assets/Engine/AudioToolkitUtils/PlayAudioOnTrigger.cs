using UnityEngine;
using System.Collections;

public class PlayAudioOnTrigger : AudioTriggerBase {


	public enum PlayPosition
    {
        Global,
        ChildObject,
        ObjectPosition,
    }

	public string audioID;
	public PlayPosition position = PlayPosition.Global; // has no meaning for Music

	private void _Play()
    {
        switch ( position )
        {
	        case PlayPosition.Global:
	            AudioController.Play( audioID ); break;
	        case PlayPosition.ChildObject:
	            AudioController.Play( audioID, transform ); break;
	        case PlayPosition.ObjectPosition:
	            AudioController.Play( audioID, transform.position ); break;
        }
    }

	protected override void _OnEventTriggered()
    {
        if ( string.IsNullOrEmpty( audioID ) ) return;

        _Play();
    }

}
