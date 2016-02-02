package com.ubisoft.audio;

import android.content.Context;
import android.media.AudioManager;
import android.util.Log;

public class AudioUtils implements AudioManager.OnAudioFocusChangeListener {

	public static boolean requestPlayMusic(Context context, boolean force) {
		if(!isExternalMusicPlaying(context) || force)
		{
			AudioManager am = ((AudioManager) context.getSystemService(Context.AUDIO_SERVICE));
			int result = am.requestAudioFocus(new AudioUtils(), AudioManager.STREAM_MUSIC, AudioManager.AUDIOFOCUS_GAIN);
			return result == AudioManager.AUDIOFOCUS_REQUEST_GRANTED;
		}
		return false;
	}

	public static boolean isExternalMusicPlaying(Context context) {
		boolean result = ((AudioManager) context.getSystemService(Context.AUDIO_SERVICE)).isMusicActive();
		Log.d("AudioUtils", "[AudioUtils] isExternalMusicPlaying() : " + result);
		return result;
	}

	@Override
	public void onAudioFocusChange(int focusChange) {
		// TODO Auto-generated method stub
		Log.d("AudioUtils", "[AudioUtils] Audio Focus Changed to " + focusChange);
	}
	
	public interface NativeAudioListener
	{
		void OnAudioGranted();
		void OnAudioLost();
	}
}
