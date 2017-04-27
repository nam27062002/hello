package com.fgol;

import java.util.List;
import java.util.Locale;
import java.util.ArrayList;
import java.io.File;
import java.io.IOException;
import java.security.MessageDigest;

import android.app.Activity;
import android.app.ActivityManager;
import android.app.ActivityManager.AppTask;
import android.app.ActivityManager.MemoryInfo;
import android.app.UiModeManager;
import android.app.AppOpsManager;
import android.annotation.SuppressLint;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.pm.ActivityInfo;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.content.pm.Signature;
import android.content.res.Configuration;
import android.graphics.BitmapFactory;
import android.graphics.BitmapFactory.Options;
import android.support.annotation.RequiresApi;
import android.telephony.TelephonyManager;
import android.provider.Settings.Secure;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.InputDevice;
import android.view.ViewGroup;
import android.view.ViewGroup.LayoutParams;
import android.view.animation.Animation;
import android.view.animation.LinearInterpolator;
import android.view.animation.RotateAnimation;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.net.Uri;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.os.Build;
import android.os.Environment;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.support.v4.content.ContextCompat;
import android.support.v4.app.ActivityCompat;
import android.support.v4.app.NotificationManagerCompat;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatDialog;
import android.os.StatFs;

import com.google.android.gms.ads.identifier.AdvertisingIdClient;
import com.google.android.gms.ads.identifier.AdvertisingIdClient.Info;
import com.unity3d.player.UnityPlayer;

import java.lang.reflect.Method;
import java.lang.reflect.Field;

public class FGOLNative {

	public static int PermissionRequestID = 20102;

	public static FGOLNative init(Activity a) {
		return new FGOLNative(a);
	}

	public static Activity activity;

	//fgn is used externally, so we hide the warnings
	@SuppressWarnings("unused")
	private FGN fgn;

	public FGOLNative(Activity a) {
		activity = a;
		System.out.println("FGOLNative initialised with activity " + a);
	}

	public int getNumGamepads()
	{
		int gamePads = 0;

		int[] deviceIds = InputDevice.getDeviceIds();
		for (int i = 0; i < deviceIds.length; i++)
		{
			InputDevice device = getInputDeviceSafe(deviceIds[i]);
			if (device != null && isGamepad(device))
			{
				gamePads++;
			}
		}

		return gamePads;
	}

	private boolean isGamepad(InputDevice inputDevice)
	{
		if (inputDevice != null)
		{
			int hasFlags = InputDevice.SOURCE_GAMEPAD | InputDevice.SOURCE_JOYSTICK;
			return (inputDevice.getSources() & hasFlags) == hasFlags;
		}

		return false;
	}

	private static InputDevice getInputDeviceSafe(int deviceId)
	{
		InputDevice device = null;
		try
		{
			device = InputDevice.getDevice(deviceId);
		}
		catch(Exception e)
		{
			//in rare cases during the connect/disconnect process, this error can be thrown:
			//"java.lang.RuntimeException: Could not get input device information"
			device = null;
		}

		return device;
	}

	public void initFGN(final String filename) {

		try {

			Runnable runnable = new Runnable() {
	            public void run() {
	            	try {
	            		fgn = new FGN(filename);
	        		} catch (Exception e) {
	        			System.out.println("initFGN.run: " + e.toString());
	        		}
	            }
			};

			activity.runOnUiThread(runnable);

		} catch (Exception e) {
			System.out.println("initFGN: " + e.toString());
		}
	}


	public String GetAPKPath(String packageName)
	{
		PackageManager pm = activity.getPackageManager();
		try
		{
			ApplicationInfo app = pm.getApplicationInfo(packageName, 0);
			if(app != null)
			{
				Log.d("FGOLNative", "APK Path of package " + packageName + " is " + app.sourceDir);
				return app.sourceDir;
			}
		}
		catch(NameNotFoundException nnfe)
		{
			System.out.println("GetAPKPath() ERROR:" + nnfe.getMessage());
		}
		return null;
	}

	public String GetCurrentAPKPath()
	{
		Log.d("FGOLNative", "APK Path of current package is " + activity.getPackageCodePath());
		return activity.getPackageCodePath();
	}

	public String GetBundleVersion() {

		try {
			PackageManager manager = activity.getPackageManager();
			if (manager!=null)
			{
	            PackageInfo packageInfo = manager.getPackageInfo(activity.getPackageName(), 0);
	            return packageInfo.versionName;
			}
		} catch (Exception e) {
			System.out.println("GetBundleVersion() ERROR:" + e.toString());
		}

		return null;
	}


	// Do not call this function from the main thread. Otherwise,
	// an IllegalStateException will be thrown.
	public String GetAdvertisingIdentifier()
	{
		Info adInfo = null;
		try
		{
			adInfo = AdvertisingIdClient.getAdvertisingIdInfo(activity);
			return adInfo.getId();
		}
		catch (Exception e)
		{
			System.out.println("GetAdvertisingIDError" + e.toString());
			return "Unknown";
		}
	}

	public boolean IsLimitAdTrackingEnabled()
	{
		Info adInfo = null;
		try
		{
			adInfo = AdvertisingIdClient.getAdvertisingIdInfo(activity);
			return adInfo.isLimitAdTrackingEnabled();
		}
		catch (Exception e)
		{
			return false;
		}
	}

	public String GetUniqueDeviceIdentifier() { // note this will be null if device does not have a SIM card

		try {
			TelephonyManager mgr = (TelephonyManager)activity.getSystemService(Context.TELEPHONY_SERVICE);
			if (mgr != null)
				return mgr.getDeviceId ();
		} catch (Exception e) {
			System.out.println("GetUniqueDeviceIdentifier() ERROR:" + e.toString());
		}

		return null;
	}

	public String GetAndroidID() { // unique device identifier, won't change unless device is factory reset
		try {
			return Secure.getString(activity.getContentResolver(),Secure.ANDROID_ID);
		} catch (Exception e) {
			System.out.println("GetAndroidID() ERROR:" + e.toString());
		}

		return null;
	}

	public String GetMACAddress() { // semi-unique identifier, can be null if WiFi turned off when device booted or device has no WiFi

		try {
			WifiManager mgr = (WifiManager)activity.getSystemService(Context.WIFI_SERVICE);
			if (mgr != null) {
				WifiInfo info = mgr.getConnectionInfo();
				if (info != null) {
					return info.getMacAddress();
				} else {
					System.out.println("GetMACAddress() getConnectionInfo()==null");
				}
			} else {
				System.out.println("GetMACAddress() getSystemService(Context.WIFI_SERVICE)==null");
			}
		} catch (Exception e) {
			System.out.println("GetMACAddress() ERROR:" + e.toString());
		}

		return null;
	}

	public void ShowMessageBox(final String title, final String message, final int msg_id) {

		try {

			Runnable runnable = new Runnable() {
	            public void run() {
	            	try {
						AlertDialog.Builder builder = new AlertDialog.Builder(activity);
						builder.setTitle(title);
						builder.setMessage(message);
						builder.setCancelable(false)
							.setPositiveButton("Ok", new DialogInterface.OnClickListener() {
								public void onClick(DialogInterface dialog, int id) {
									if(msg_id != -1)
									{
										UnityPlayer.UnitySendMessage("FGOLNativeReceiver", "MessageBoxClick", "" + msg_id + ":OK");
									}
									dialog.cancel();
								}
							});
						AppCompatDialog alertdialog = builder.create();
						alertdialog.show();
	        		} catch (Exception e) {
	        			System.out.println("ShowMessageBox.run: " + e.toString());
	        		}
                 }
			};

			activity.runOnUiThread(runnable);

			System.out.println("ShowMessageBox: " + title + " msg=" + message);

		} catch (Exception e) {
			System.out.println("ShowMessageBoxWithButtons: " + e.toString());
		}
	}

	public void ShowMessageBoxWithButtons(final String title, final String message, final String ok_button, final String cancel_button, final int msg_id) {

		try {
			System.out.println("ShowMessageBoxWithButtons: " + title + " msg=" + message);

			Runnable runnable = new Runnable() {
	            public void run() {
	            	try {
						AlertDialog.Builder builder = new AlertDialog.Builder(activity);
						builder.setTitle(title);
						builder.setMessage(message);
						builder.setCancelable(false)
							.setPositiveButton(ok_button, new DialogInterface.OnClickListener() {
								public void onClick(DialogInterface dialog, int id) {
									if(msg_id != -1)
									{
										UnityPlayer.UnitySendMessage("FGOLNativeReceiver", "MessageBoxClick", "" + msg_id + ":OK");
									}
									dialog.cancel();
								}
							})
							.setNegativeButton(cancel_button, new DialogInterface.OnClickListener() {
								public void onClick(DialogInterface dialog, int id) {
									if(msg_id != -1)
									{
										UnityPlayer.UnitySendMessage("FGOLNativeReceiver", "MessageBoxClick", "" + msg_id + ":CANCEL");
									}
									dialog.cancel();
								}
							});
						AppCompatDialog alertdialog = builder.create();
						alertdialog.show();
	        		} catch (Exception e) {
	        			System.out.println("ShowMessageBoxWithButtons.run: " + e.toString());
	        		}
	              }
			};

			activity.runOnUiThread(runnable);

		} catch (Exception e) {
			System.out.println("ShowMessageBoxWithButtons: " + e.toString());
		}
	}


	public static void openURL(String URL) {
		Intent i = new Intent(Intent.ACTION_VIEW);
		i.setData(Uri.parse(URL));
		activity.startActivity(i);
	}

	public String getUserLocation() {

		String countryCode = null;

		try {
			TelephonyManager tm = (TelephonyManager)activity.getSystemService(Context.TELEPHONY_SERVICE);
		    countryCode = tm.getSimCountryIso();
		} catch (Exception e) {
			System.out.println("getUserLocation: SIM " + e.toString());
		}

		// what if no sim card?
	    if (countryCode==null || countryCode.equals("")) {
	    	try {
	    		countryCode = activity.getResources().getConfiguration().locale.getCountry();
	    	} catch (Exception e) {
				System.out.println("getUserLocation: Locale " + e.toString());
			}
	    }

	    // check we got something...
	    if (countryCode==null || countryCode.equals("")) {
	    	countryCode = "XX"; // give up!
	    }

	    return countryCode;
	}

	public boolean isAppInstalled(String packageName) {
	    PackageManager pm = activity.getPackageManager();
	    boolean installed = false;
	    try {
	       pm.getPackageInfo(packageName, PackageManager.GET_ACTIVITIES);
	       installed = true;
	    } catch (PackageManager.NameNotFoundException e) {
	       installed = false;
	    }
	    return installed;
	}

	public String getAppVersion(String packageName) {
	    PackageManager pm = activity.getPackageManager();
	    try {
	       PackageInfo pi = pm.getPackageInfo(packageName, PackageManager.GET_ACTIVITIES);
	       return pi.versionName;
	    } catch (Exception e) {
	       return "NA";
	    }
	}

	//This won't work with android N as it was deprecated
	/*public static String getInstallReferrer() {
		SharedPreferences prefs = activity.getSharedPreferences("FGOLNative", Context.MODE_MULTI_PROCESS);
		if (prefs.contains("referrer")) {
			return prefs.getString("referrer", null);
		} else {
			return null;
		}
	}*/

	@RequiresApi(21)
	public static boolean IsActivityRunning(String activityName)
	{
        ActivityManager activityManager = (ActivityManager) activity.getSystemService(Context.ACTIVITY_SERVICE);
        List<AppTask> tasks = activityManager.getAppTasks();

		System.out.println("Starting Task search: " + activityName);
        for (int i=0;i < tasks.size(); i++)
        {
			System.out.println("Task search: " + tasks.get(i).getTaskInfo().origActivity.getPackageName());
            if (tasks.get(i).getTaskInfo().origActivity.getPackageName().equalsIgnoreCase(activityName))
                return true;
        }
        return false;
    }

	public static int GetNumCertificates()
	{
		try
		{
			PackageInfo packageInfo = activity.getPackageManager().getPackageInfo(
					activity.getPackageName(),
					PackageManager.GET_SIGNATURES);

			return packageInfo.signatures.length;
		}
		catch (Exception e)
		{
			//requesting signatures from APK failed'
			Log.e("FGOLNative", "GetNumCertificates failed with exception: " + e.toString());
			return 0;
		}
	}

	public static String GetCertificateSignatureSHA(int index)
	{
		try
		{
			PackageInfo packageInfo = activity.getPackageManager().getPackageInfo(
					activity.getPackageName(),
					PackageManager.GET_SIGNATURES);
			if (index >= packageInfo.signatures.length)
			{
				Log.e("FGOLNative", "GetCertificateSignatureSHA index out of range.");
				return "";
			}
			else
			{
				Signature signature = packageInfo.signatures[index];
				MessageDigest md = MessageDigest.getInstance("SHA");
				md.update(signature.toByteArray());

				byte[] publicKey = md.digest();

                StringBuffer hexString = new StringBuffer();
                for (int i=0;i<publicKey.length;i++)
                {
                	if (i!= 0)
                	{
                		hexString.append(":");
                	}
                    String appendString = Integer.toHexString(0xFF & publicKey[i]);
                    if(appendString.length() == 1)
                    	hexString.append("0");
                    hexString.append(appendString);
                }

				Log.d("FGOLNative", "Cert signature " + index + ": "+ hexString.toString().toUpperCase());
				return hexString.toString().toUpperCase();
			}
		}
		catch (Exception e)
		{
			//requesting signatures from APK failed'
			Log.e("FGOLNative", "Requesting cert signatures from APK failed with exception");
			return "";
		}
	}

	public static boolean HasPermission(String permission)
	{
		int permissionCheck = ContextCompat.checkSelfPermission(activity, permission);
		if (permissionCheck == PackageManager.PERMISSION_GRANTED)
		{
			return true;
		}
		return false;
	}

	public static boolean IsAndroidTVDevice()
	{
		UiModeManager uiModeManager = (UiModeManager) activity.getSystemService(Context.UI_MODE_SERVICE);
		if (uiModeManager.getCurrentModeType() == Configuration.UI_MODE_TYPE_TELEVISION) {
		    return true;
		} else {
			return false;
		}
	}

	//http://developer.android.com/reference/android/support/v4/app/ActivityCompat.html#shouldShowRequestPermissionRationale(android.app.Activity, java.lang.String)
	public static void TryShowPermissionExplanation(final String permissions, final String messageTitle, final String messageInfo)
	{
        System.out.println("Requesting permissions: " + permissions);
        //split dat array
		String[] permissionRequestArray = permissions.split(",");
		ArrayList<String> permissionRequiredArray = new ArrayList<String>();

		for(int i=0; i<permissionRequestArray.length; i++)
		{
			if (HasPermission(permissionRequestArray[i]))
			{
				//tell unity that we got em
	            UnityPlayer.UnitySendMessage("FGOLNativeReceiver", "PermissionReceivedSuccess", ""+permissionRequestArray[i]);
			}
			else
			{
				//only request permissions we don't have
				permissionRequiredArray.add(permissionRequestArray[i]);
			}
		}
		if (permissionRequiredArray.size() == 0)
		{
			//all permissions are already granted, go home Jimmy
			return;
		}

		//now we gotta request permissions, lol
		final String[] permissionsToRequest = permissionRequiredArray.toArray(new String[permissionRequiredArray.size()]);
        System.out.println("ShowPermissionExplanation: " + messageTitle + " msg=" + messageInfo);
        try
        {
            Runnable runnable = new Runnable()
            {
                public void run()
                {
                    try
                    {
                        AlertDialog.Builder builder = new AlertDialog.Builder(activity);
                        builder.setTitle(messageTitle);
                        builder.setMessage(messageInfo);
                        builder.setCancelable(false)
                            .setPositiveButton("Ok", new DialogInterface.OnClickListener()
                            {
                                public void onClick(DialogInterface dialog, int id)
                                {
                                    dialog.cancel();
                                	activity.setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_SENSOR_LANDSCAPE);
                                    ActivityCompat.requestPermissions(activity, permissionsToRequest, PermissionRequestID);
                                }
                            });
                        AppCompatDialog alertdialog = builder.create();
                        alertdialog.show();
                    }
                    catch (Exception e)
                    {
                        System.out.println("ShowMessageBoxWithButtons.run: " + e.toString());
                    }
				}
			};
			activity.runOnUiThread(runnable);
		}
        catch (Exception e)
        {
            System.out.println("ShowMessageBoxWithButtons: " + e.toString());
        }
    }

	public static String GetConnectionType ()
	{
        ConnectivityManager cm = (ConnectivityManager) activity.getSystemService(Context.CONNECTIVITY_SERVICE);
        NetworkInfo info = cm.getActiveNetworkInfo();
        if(info==null || !info.isConnected())
            return "-"; //not connected
        if(info.getType() == ConnectivityManager.TYPE_WIFI)
            return "WIFI";
        if(info.getType() == ConnectivityManager.TYPE_MOBILE)
        {
            int networkType = info.getSubtype();
            switch (networkType)
            {
                case TelephonyManager.NETWORK_TYPE_GPRS:
                case TelephonyManager.NETWORK_TYPE_EDGE:
                case TelephonyManager.NETWORK_TYPE_CDMA:
                case TelephonyManager.NETWORK_TYPE_1xRTT:
                case TelephonyManager.NETWORK_TYPE_IDEN: //api<8 : replace by 11
                    return "2G";
                case TelephonyManager.NETWORK_TYPE_UMTS:
                case TelephonyManager.NETWORK_TYPE_EVDO_0:
                case TelephonyManager.NETWORK_TYPE_EVDO_A:
                case TelephonyManager.NETWORK_TYPE_HSDPA:
                case TelephonyManager.NETWORK_TYPE_HSUPA:
                case TelephonyManager.NETWORK_TYPE_HSPA:
                case TelephonyManager.NETWORK_TYPE_EVDO_B: //api<9 : replace by 14
                case TelephonyManager.NETWORK_TYPE_EHRPD:  //api<11 : replace by 12
                case TelephonyManager.NETWORK_TYPE_HSPAP:  //api<13 : replace by 15
                    return "3G";
            }
        }
        return "Unknown";
	}

	public static String GetGameLanguageISO()
	{
	    return Locale.getDefault().getLanguage();
	}

	public static String GetUserCountryISO()
	{
        return activity.getResources().getConfiguration().locale.getCountry();
	}

	public static String GetExternalStorageLocation()
	{
		String result = "";

		try
		{
			result =  activity.getExternalFilesDir(null).getAbsolutePath();
		}
		catch(Exception e)
		{
			System.out.println("Cannot retrieve external files directory" + e.toString());
		}

		return result;
	}

	public static String GetExpansionFileLocation()
	{
		String result = "";

		try
		{
			result = activity.getObbDir().getAbsolutePath();
		}
		catch(Exception e)
		{
			System.out.println("Cannot retrieve obb files directory" + e.toString());
		}

		return result;
	}

	public static void AttemptToRestoreFilesFromInternalToExternal()
	{
		Log.d("FGOLConsoleSaveRecovery", "Starting");
		//firstly, do any save files exist in External Storage?
		File extDir = new File(GetExternalStorageLocation());
		File extFiles[] = extDir.listFiles();
		boolean foundSave = false;
		for (int i=0; i< extFiles.length; i++)
		{
			if (extFiles[i].getName().contains(".sav"))
			{
				foundSave = true;
				break;
			}
		}
		if (!foundSave)
		{
			Log.d("FGOLConsoleSaveRecovery", "No Save File");
			//does the internal directory contain saves?
			String internalDirectory = activity.getFilesDir().getAbsolutePath();
			File intDir = new File(internalDirectory);
			File intFiles[] = intDir.listFiles();
			boolean timeToCopy = false;
			for (int i=0; i< intFiles.length; i++)
			{
				if (intFiles[i].getName().contains(".sav"))
				{
					timeToCopy = true;
					break;
				}
			}
			if (timeToCopy)
			{
				Log.d("FGOLConsoleSaveRecovery", "Save Files need copying");
				//there could be files in the users ext directory (creates by sdks and plugins and shit)
				//wipe that clean before
				for (int i=0; i< extFiles.length; i++)
				{
					extFiles[i].delete();
				}

				//now copy all from int to ext
				for (int i=0; i< intFiles.length; i++)
				{
					File destFile = new File(GetExternalStorageLocation()+"/"+intFiles[i].getName());
					Log.d("FGOLConsoleSaveRecovery", "Coping to " + destFile.getAbsolutePath());
					try
					{
						FGUtil.CopyFilesRecursively(intFiles[i], destFile);
					}
					catch (IOException e)
					{
						Log.e("FGOLConsoleSaveRecovery", "Something broke: "+e.toString());
					}
				}
			}
		}
	}

	// Calculates memory usage in runtime
	public int GetMemoryUsage()
	{
		return GetStatusIntValue("VmSize:") * 1024;	// from kb to bytes
	}
	// Calculates memory peak in runtime
	public int GetMaxMemoryUsage()
	{
			return GetStatusIntValue("VmPeak:") * 1024;	// from kb to bytes
	}

	private java.io.RandomAccessFile randomAccessFile = null;
	private int GetStatusIntValue(String _field)
	{
		try
		{
			// Try to create the process file if it doesn't exists
			if(randomAccessFile == null)
			{
				randomAccessFile = new java.io.RandomAccessFile("/proc/" + android.os.Process.myPid() + "/status", "r");
			}

			// Search VmPeak
			if (randomAccessFile != null)
			{
				randomAccessFile.seek(0);
				String str = "";
				do {
					str = randomAccessFile.readLine();
					if (str.startsWith(_field))
					{
						str = str.substring( _field.length() );
						str = str.trim();
						return Integer.parseInt(str.split(" ")[0]);
					}
				} while ( str != "" );
			}
		}
		catch(Exception e)
		{
			System.out.println("GetStatusIntValue Exception " + e.toString());
		}
		return 0;
	}

	// Get total memory using MemoryInfo - alternative method to GetMemoryUsage()
	public int GetTotalMemoryPSS()
	{
		int result = -1;

		try
		{
			MemoryInfo memoryInfo = new MemoryInfo();
			ActivityManager activityManager = (ActivityManager) activity.getSystemService(Context.ACTIVITY_SERVICE);
			activityManager.getMemoryInfo(memoryInfo);

			int processId = android.os.Process.myPid();
			android.os.Debug.MemoryInfo[] mi = activityManager.getProcessMemoryInfo(new int[]{processId});
			result = mi[0].getTotalPss();
		}
		catch(Exception e)
		{
			System.out.println("Memory Usage PSS Exception" + e.toString());
		}

		return result;
	}

	// Get max allowed memory on device
	public long GetMaxHeapMemory()
	{
		return Runtime.getRuntime().maxMemory();
	}

	// Get total used heap memory
	public long GetUsedHeapMemory()
	{
		return Runtime.getRuntime().totalMemory() - Runtime.getRuntime().freeMemory();
	}

	// Get max device memory (total RAM memory)
	public long GetMaxDeviceMemory()
	{
		MemoryInfo memoryInfo = new MemoryInfo();
		ActivityManager activityManager = (ActivityManager) activity.getSystemService(Context.ACTIVITY_SERVICE);
		activityManager.getMemoryInfo(memoryInfo);
		return memoryInfo.totalMem;
	}

	// Get total available memory
	public long GetAvailableDeviceMemory()
	{
		MemoryInfo memoryInfo = new MemoryInfo();
		ActivityManager activityManager = (ActivityManager) activity.getSystemService(Context.ACTIVITY_SERVICE);
		activityManager.getMemoryInfo(memoryInfo);
		return memoryInfo.availMem;
	}

	// Get memory threshold (reserved for OS)
	public long GetDeviceMemoryThreshold()
	{
		MemoryInfo memoryInfo = new MemoryInfo();
		ActivityManager activityManager = (ActivityManager) activity.getSystemService(Context.ACTIVITY_SERVICE);
		activityManager.getMemoryInfo(memoryInfo);
		return memoryInfo.threshold;
	}

	// Cached spinner values
	private RotateAnimation m_rotateAnimation;					// Rotate animation which is added to the spinner
	private LinearLayout 	m_spinnerLayout;					// Spinner layout, will be null until ToggleSpinner is called for the first time
	private final int		REFERENCE_SCREEN_HEIGHT = 1080;		// Reference screen size for spinner size calculation
	private final float		SPINNER_SCALE = 2.0f;				// Scale to apply to source image, it's cheaper than having larger image in res
	private final int		ROTATION_DURATION = 1500; 			// 360 rotation in milliseconds
	// Native loading indicator on top of Unity activity (doesn't pause Unity activity)
	// Usage - replace spinner.png file in drawable folder
	// IN bool enable - show or hide the spinner
	// IN float x, y - values from 0 to 1 which represents the coordinate to place the spinner, relative to left top corner
	public void ToggleSpinner(final boolean enable, final float x, final float y)
	{
		try
		{
			Runnable runnable = new Runnable() {
			public void run()
			{
				// Get screen size (doesn't report virtual buttons area)
				DisplayMetrics screenMetrics = new DisplayMetrics();

				// For Android > 17 try to get full screen size including virtual buttons area
				if (Build.VERSION.SDK_INT >= 17)
				{
					activity.getWindowManager().getDefaultDisplay().getRealMetrics(screenMetrics);
				}
				else
				{
					activity.getWindowManager().getDefaultDisplay().getMetrics(screenMetrics);
				}

				// Create spinner layout if it didn't exist
				if(m_spinnerLayout == null)
				{
					ImageView spinnerImage = new ImageView(activity);
					spinnerImage.setImageResource(R.drawable.spinner);

					// Set size based on screen height compared to target resolution height
					// Keep image aspect ratio based on height
					final Options opt = new BitmapFactory.Options();
					opt.inJustDecodeBounds = true;
					BitmapFactory.decodeResource(activity.getResources(), R.drawable.spinner, opt);
					int targetHeight = (int)(SPINNER_SCALE * opt.outHeight * (float)screenMetrics.heightPixels / REFERENCE_SCREEN_HEIGHT);
					int targetWidth = (int)(SPINNER_SCALE * (float)targetHeight / opt.outHeight * opt.outWidth);
					System.out.println("Spinner Width = " + targetWidth + " Spinner Height = " + targetHeight);
					spinnerImage.setLayoutParams(new LinearLayout.LayoutParams(targetWidth, targetHeight));

					// Create animation
					m_rotateAnimation = new RotateAnimation(0.0f, 360.0f, Animation.RELATIVE_TO_SELF, 0.5f, Animation.RELATIVE_TO_SELF, 0.5f);
					m_rotateAnimation.setInterpolator(new LinearInterpolator());
					m_rotateAnimation.setRepeatCount(Animation.INFINITE);
					// In milliseconds
					m_rotateAnimation.setDuration(ROTATION_DURATION);

					// Add the image to the view and apply animation
					m_spinnerLayout = new LinearLayout(activity);
					m_spinnerLayout.setLayoutParams(new LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.MATCH_PARENT));
					m_spinnerLayout.addView(spinnerImage, 0);
				}

				ImageView spinner = (ImageView)m_spinnerLayout.getChildAt(0);

				// Set image position + set alignment to center of the image rather than top left
				int marginLeft = (int)((x * screenMetrics.widthPixels - spinner.getLayoutParams().width / 2));
				int marginTop = (int)((y * screenMetrics.heightPixels - spinner.getLayoutParams().height / 2));

				// Make sure the spinner is fully within screen bounds
				if(marginLeft < 0)
				{
					marginLeft = 0;
				}
				if((marginLeft + spinner.getLayoutParams().width) > screenMetrics.widthPixels)
				{
					marginLeft = screenMetrics.widthPixels - spinner.getLayoutParams().width;
				}
				if(marginTop < 0)
				{
					marginTop = 0;
				}
				if(marginTop + spinner.getLayoutParams().height > screenMetrics.heightPixels)
				{
					marginTop = screenMetrics.heightPixels - spinner.getLayoutParams().height;
				}

				//System.out.println("Spinner Input X = " + x + " Spinner Input Y = " + y);
				//System.out.println("Spinner Margin Left = " + marginLeft + " Spinner Margin Top = " + marginTop);
				((LinearLayout.LayoutParams)spinner.getLayoutParams()).setMargins(marginLeft, marginTop, 0, 0);

				if(enable)
				{
					// Add the view only if it's not already displayed
					ViewGroup vg = (ViewGroup)(m_spinnerLayout.getParent());
					if(vg == null)
					{
						// For some reason animation getting removed after removing the view, therefore we assign it every time we show the spinner
						m_spinnerLayout.getChildAt(0).setAnimation(m_rotateAnimation);

						activity.addContentView(m_spinnerLayout, m_spinnerLayout.getLayoutParams());
					}
				}
				else
				{
					// Remove the view only if it's displayed
					ViewGroup vg = (ViewGroup)(m_spinnerLayout.getParent());
					if(vg != null)
					{
						vg.removeView(m_spinnerLayout);
					}
				}
			}};

			activity.runOnUiThread(runnable);
		}
		catch(Exception e)
		{
			System.out.println("Exception toggling loading spinner :: Exception = " +e.toString());
		}
	}

	/*
	 * Gets publicly writable documents directory.
	 */
	@SuppressLint("NewApi")
	public String GetDocumentsDirectory()
	{
		try {
			File docPath = new File(Environment.getExternalStorageDirectory() + "/Documents");
			if (docPath != null)
			{
				boolean exists = true;
				if (!docPath.exists())
				{
					exists = docPath.mkdir();
				}
				if (exists)
				{
					String path = docPath.getAbsolutePath();
					return path;
				}
			}
		}
		catch (Exception e)
		{
			System.out.println("Exception while fetching doc folder: " + e.toString());
		}
		return null;
	}

	public long GetAvailableDiskSpace()
	{
		try
		{
            StatFs stat = null;
            boolean hasExternalStorage = android.os.Environment.getExternalStorageState().equals(android.os.Environment.MEDIA_MOUNTED);

            if (hasExternalStorage)
            {
                stat = new StatFs(Environment.getExternalStorageDirectory().getPath());
            }
            else
            {
                stat = new StatFs(Environment.getDataDirectory().getPath());
            }

            return (long) stat.getBlockSize() * (long) stat.getAvailableBlocks();
		}
		catch (Exception e)
		{
			System.out.println("Exception while trying to read available disk space. " + e.toString());
		}
		return 0;
	}

	private static final String CHECK_OP_NO_THROW = "checkOpNoThrow";
	private static final String OP_POST_NOTIFICATION = "OP_POST_NOTIFICATION";

	public boolean GetPushDisabledByOSStatus()
	{
		try
		{
			//	Does not work due to v4 support 24.0.0+ requirements (GPGS uses 23 so we cant' update)
			//Context appContext = activity.getApplicationContext();
			//return !NotificationManagerCompat.from(appContext).areNotificationsEnabled();

			Context appContext = activity.getApplicationContext();
			AppOpsManager mAppOps = (AppOpsManager) appContext.getSystemService(Context.APP_OPS_SERVICE);

			ApplicationInfo appInfo = appContext.getApplicationInfo();
			String pkg = appContext.getPackageName();

			int uid = appInfo.uid;

			Class appOpsClass = null; /* Context.APP_OPS_MANAGER */
			appOpsClass = Class.forName(AppOpsManager.class.getName());
			Method checkOpNoThrowMethod = appOpsClass.getMethod(CHECK_OP_NO_THROW, Integer.TYPE, Integer.TYPE, String.class);
			Field opPostNotificationValue = appOpsClass.getDeclaredField(OP_POST_NOTIFICATION);
			int value = (int)opPostNotificationValue.get(Integer.class);
			return ((int)checkOpNoThrowMethod.invoke(mAppOps,value, uid, pkg) != AppOpsManager.MODE_ALLOWED);
		}
		catch (Exception e)
		{
			System.out.println("Can't get status of push notification: " + e.toString());
		}
		return false;
	}

	//	Checks if app bundle with passed ID is present and if we can read its version
	//	Returns NULL in any fail case, version only if it can be read
	public String GetInstalledAppVersion(String appID)
	{
		String toReturn = null;

		try
		{
			Context appContext = activity.getApplicationContext();
			PackageInfo pinfo = appContext.getPackageManager().getPackageInfo(appID, 0);
			int verCode = pinfo.versionCode;
			String verName = pinfo.versionName;

			System.out.println("FGOLNative: Got verCode: " + verCode + " verName: " + verName);

			return verName;
		}
		catch (Exception e)
		{
			System.out.println("FGOLNative: Exception while trying to get installed app version: " + e.toString());
		}

		return toReturn;
	}
}
