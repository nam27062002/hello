package com.ubisoft.utils;

import android.Manifest;
import android.app.Activity;
import android.content.pm.PackageManager;
import android.support.v4.app.ActivityCompat;

import com.google.android.gms.ads.identifier.AdvertisingIdClient;

public class PlatformUtils implements ActivityCompat.OnRequestPermissionsResultCallback {
	public static String getTrackingId(final Activity activity){
		try {
			return AdvertisingIdClient.getAdvertisingIdInfo(activity).getId().toString();
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			return "";
		}
	}
	
	public static void requestPermissions(final Activity activity){
		if(!arePermissionsGranted(activity)){
			activity.runOnUiThread(new Runnable(){
				public void run(){
					System.out.println("Requesting permissions for " + activity.getPackageName() + " from " + activity.getCallingPackage());
					ActivityCompat.requestPermissions(activity, new String[]{Manifest.permission.WRITE_EXTERNAL_STORAGE, Manifest.permission.GET_ACCOUNTS}, 1);
				}
			});
		}
	}
	
	public static boolean arePermissionsGranted(final Activity activity){
		return	activity.getPackageManager().checkPermission(Manifest.permission.WRITE_EXTERNAL_STORAGE, activity.getPackageName()) == PackageManager.PERMISSION_GRANTED
		&&		activity.getPackageManager().checkPermission(Manifest.permission.GET_ACCOUNTS, activity.getPackageName()) == PackageManager.PERMISSION_GRANTED;
	}

	@Override
	public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] results) {
		if(requestCode == 1){
			for(int i = 0; i < permissions.length; ++i){
				System.out.println("Permission " + permissions[i] + " : " + (results[i] == PackageManager.PERMISSION_GRANTED));
			}
		}
	}
	
	public interface OnPermissionsResultListener{
		public void onPermissionsResult();
	}
}
