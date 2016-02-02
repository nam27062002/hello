package com.ubisoft.utils;

import android.app.Activity;
import android.widget.Toast;

public class ToastInterface {
	public static void makeToast(final Activity activity, final String text, final boolean isLong)
	{
		activity.runOnUiThread(new Runnable(){
			@Override
			public void run()
			{
				int duration = isLong? Toast.LENGTH_LONG:Toast.LENGTH_SHORT;
				Toast t = Toast.makeText(activity, text, duration);
				t.show();
			}
		});
	}
}
