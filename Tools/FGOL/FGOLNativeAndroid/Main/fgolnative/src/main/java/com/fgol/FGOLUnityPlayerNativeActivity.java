package com.fgol;

import android.content.pm.PackageManager;
import android.content.res.Configuration;
import android.annotation.TargetApi;
import android.content.Context;
import android.os.Build;
import android.os.Bundle;
import android.support.annotation.RequiresApi;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;

import com.unity3d.player.UnityPlayer;

public class FGOLUnityPlayerNativeActivity extends com.unity3d.player.UnityPlayerNativeActivity {

	@Override
	protected void attachBaseContext(Context base)
	{
		super.attachBaseContext(base);
	}
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
	}

	// Quit Unity
	@Override
	protected void onDestroy() {
		super.onDestroy();
	}

	// Pause Unity
	@Override
	public void onPause() {
		super.onPause();
	}

	// Resume Unity
	@Override
	public void onResume() {
		super.onResume();
		EnableImmersiveMode();

	}

	// This ensures the layout will be correct.
	@Override
	public void onConfigurationChanged(Configuration newConfig) {
		super.onConfigurationChanged(newConfig);
	}

	// Notify Unity of the focus change.
	@Override
	public void onWindowFocusChanged(boolean hasFocus) {
		super.onWindowFocusChanged(hasFocus);
	}

	// For some reason the multiple keyevent type is not supported by the ndk.
	// Force event injection by overriding dispatchKeyEvent().
	@Override
	public boolean dispatchKeyEvent(KeyEvent event) {
		return super.dispatchKeyEvent(event);
	}

	// Pass any events not handled by (unfocused) views straight to UnityPlayer
	@Override
	public boolean onKeyUp(int keyCode, KeyEvent event) {
		return super.onKeyUp(keyCode, event);
	}

	@Override
	public boolean onKeyDown(int keyCode, KeyEvent event) {
		return super.onKeyDown(keyCode, event);
	}

	@Override
	public boolean onTouchEvent(MotionEvent event) {
		return super.onTouchEvent(event);
	}

	@RequiresApi(23)
	@Override
	public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) 
	{

		for (int i = 0; i < permissions.length; i++) 
		{
			if (grantResults[i] == PackageManager.PERMISSION_GRANTED)
			{
				UnityPlayer.UnitySendMessage("FGOLNativeReceiver","PermissionReceivedSuccess", "" + permissions[i]);
			} 
			else
			{
				if (shouldShowRequestPermissionRationale(permissions[i]))
                {
                    // Can retry to ask for permission
                     UnityPlayer.UnitySendMessage("FGOLNativeReceiver","PermissionReceivedFailed", "" + permissions[i]);
                }
                else
                {
                	 // Can't ask for permissions anymore
                     UnityPlayer.UnitySendMessage("FGOLNativeReceiver","PermissionReceivedFailedDontAskAnymore", "" + permissions[i]);
                }
			}
		}
	}

	@TargetApi(Build.VERSION_CODES.JELLY_BEAN)
	public static void EnableImmersiveMode() 
	{
		if (Build.VERSION.SDK_INT >= 19) // KITKAT
		{
			Runnable runnable = new Runnable() 
			{
				public void run() 
				{
					int flagImmersiveSticky = 4096; // View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY
					int flags = flagImmersiveSticky	| View.SYSTEM_UI_FLAG_FULLSCREEN | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION;
					UnityPlayer.currentActivity.findViewById(android.R.id.content).setSystemUiVisibility(flags);
				};
			};
			UnityPlayer.currentActivity.runOnUiThread(runnable);
		}
	}

}