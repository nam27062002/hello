using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace AssetBundles
{
    public class LaunchAssetBundleServer : ScriptableSingleton<LaunchAssetBundleServer>
    {        

        [SerializeField]
        int     m_ServerPID = 0;

        private static string sm_remoteAssetsFolderName = "Downloadables";

        public static string GetRemoteAssetsFolderName()
        {
            return sm_remoteAssetsFolderName;
        }

        public static void SetRemoteAssetsFolderName(string name)
        {
            sm_remoteAssetsFolderName = name;
        }

        public static void ToggleLocalAssetBundleServer()
        {
            bool isRunning = IsRunning();
            if (!isRunning)
            {
                Run();
            }
            else
            {
                KillRunningAssetBundleServer();
            }
        }
        
        public static bool ToggleLocalAssetBundleServerValidate(string key)
        {
            bool isRunnning = IsRunning();
            Menu.SetChecked(key, isRunnning);
            return true;
        }

        public static bool IsRunning()
        {
            if (instance.m_ServerPID == 0)
                return false;

            try
            {
                var process = Process.GetProcessById(instance.m_ServerPID);
                if (process == null)
                    return false;

                return !process.HasExited;
            }
            catch
            {
                return false;
            }
        }

        public static void KillRunningAssetBundleServer()
        {
            // Kill the last time we ran
            try
            {
                if (instance.m_ServerPID == 0)
                    return;

                var lastProcess = Process.GetProcessById(instance.m_ServerPID);
                if (lastProcess != null)
                    lastProcess.Kill();

                instance.m_ServerPID = 0;
            }
            catch
            {
            }
        }

        public static string overloadedDevelopmentServerURL = "";

        public static string GetServerURL()
        {
            string downloadURL;
            if (string.IsNullOrEmpty(overloadedDevelopmentServerURL) == false)
            {
                downloadURL = overloadedDevelopmentServerURL;
            }
            else
            {
                IPHostEntry host;
                string localIP = "";
             
                try
                {
                    host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (IPAddress ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            localIP = ip.ToString();
                            break;
                        }
                    }
                }
                catch 
                {

                }
                
                downloadURL = "http://" + localIP + ":7888/";
            }

            return downloadURL;
        }

        public static void WriteServerURL()
        {
            string downloadURL = GetServerURL();

            UnityEngine.Debug.Log("Server url = " + downloadURL);

            string assetBundleManagerResourcesDirectory = "Assets/Resources/Addressables/AssetBundles";
            string assetBundleUrlPath = Path.Combine(assetBundleManagerResourcesDirectory, "AssetBundleServerURL.bytes");
            if (!Directory.Exists(assetBundleManagerResourcesDirectory))
            {
                Directory.CreateDirectory(assetBundleManagerResourcesDirectory);
            }

            File.WriteAllText(assetBundleUrlPath, downloadURL);
            AssetDatabase.Refresh();
        }

        static void Run()
        {
            string pathToAssetServer = Path.GetFullPath("Assets/UbiPackages/AssetBundles/Editor/AssetBundleServer/AssetBundleServer.exe");
            string assetBundlesDirectory = Path.Combine(Environment.CurrentDirectory, sm_remoteAssetsFolderName);

            KillRunningAssetBundleServer();

            if (!Directory.Exists(assetBundlesDirectory))
                Directory.CreateDirectory(assetBundlesDirectory);

            WriteServerURL();

            string args = assetBundlesDirectory;
            args = string.Format("\"{0}\" {1}", args, Process.GetCurrentProcess().Id);
            ProcessStartInfo startInfo = ExecuteInternalMono.GetProfileStartInfoForMono(MonoInstallationFinder.GetMonoInstallation("MonoBleedingEdge"), GetMonoProfileVersion(), pathToAssetServer, args, true);
            startInfo.WorkingDirectory = assetBundlesDirectory;
            startInfo.UseShellExecute = false;
            Process launchProcess = Process.Start(startInfo);
            if (launchProcess == null || launchProcess.HasExited == true || launchProcess.Id == 0)
            {
                //Unable to start process
                UnityEngine.Debug.LogError("Unable Start AssetBundleServer process");
            }
            else
            {
                //We seem to have launched, let's save the PID
                instance.m_ServerPID = launchProcess.Id;                
            }
        }

        static string GetMonoProfileVersion()
        {
            string path = Path.Combine(Path.Combine(MonoInstallationFinder.GetMonoInstallation("MonoBleedingEdge"), "lib"), "mono");

            string[] folders = Directory.GetDirectories(path);
            string[] foldersWithApi = folders.Where(f => f.Contains("-api")).ToArray();
            float profileVersion = 1.0f;

            string candidate;
            string[] tokens;
            for (int i = 0; i < foldersWithApi.Length; i++)
            {
                candidate = foldersWithApi[i].Split(Path.DirectorySeparatorChar).Last();
                candidate = candidate.Split('-').First();

                tokens = candidate.Split('.');
                if (tokens.Length > 2)
                {
                    candidate = tokens[0] + '.' + tokens[1];
                    for (int j = 2; j < tokens.Length; j++)
                    {
                        candidate += tokens[j];
                    }
                }

                if (float.Parse(candidate) > profileVersion)
                {
                    profileVersion = float.Parse(candidate);
                }
            }

            return profileVersion.ToString();
        }
    }
}
