
namespace FGOL.Plugins.Native
{
    //  Methods supported by all native implementations
    public interface INativeImplementation
    {
        //  App identity
        string GetBundleVersion();
        string GetUniqueUserID();
        string GetUserLocation();
        string GetAdvertisingIdentifier();
		string GetGameLanguageISO(); //following ISO 639-1 norm
		string GetUserCountryISO(); ////following ISO 3166-1 alpha-2 norm
		string GetConnectionType(); //[3G | 4G | Edge | Wifi ]
		string GetDeviceName();
        void DontBackupDirectory(string directory);

        string GetPersistentDataPath();
		string GetExpansionFilePath();
		string[] GetCertificateSignatureSHA(); //used by Android

		//	Audio & multitasking
		bool IsAudioPlayingFromOtherApps();		
		void RequestExclusiveAudio(bool exclusiveAudio);

		// General info
		int GetMemoryUsage();
		int GetMaxMemoryUsage();
		long GetMaxHeapMemory();
		long GetUsedHeapMemory();
		long GetMaxDeviceMemory();
		long GetAvailableDeviceMemory();
		long GetDeviceMemoryThreshold();
		int GetMemoryPSS();						// Alternative method to GetMemoryUsage

		bool IsAppInstalled(string identifier);

        //  Alerts
        void ShowMessageBox(string title, string message, int msg_id = -1);
        void ShowMessageBoxWithButtons(string title, string message, string ok_button, string cancel_button, int msg_id = -1);

		// Permissions
		void TryShowPermissionExplanation(string[] permissions, string messageTitle, string messageInfo);
        bool HasPermissions(string[] permissions);

		// Native visuals
		void ToggleSpinner(bool enable, float x = 0, float y = 0);

		//	Used only on iOS
		//	https://developer.apple.com/library/ios/documentation/NetworkingInternet/Conceptual/StoreKitGuide/Chapters/RequestPayment.html#//apple_ref/doc/uid/TP40008267-CH4-SW6
		string HashedValueForAccountName(string userAccountName);
    }
}