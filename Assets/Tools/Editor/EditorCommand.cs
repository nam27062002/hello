// EditorCommand.cs
// Hungry Dragon
// 
// Created by Jordi Riambau on 18/06/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

using System.Diagnostics;
using System.Text;

public class EditorCommand
{
	static string CommandApp
	{
		get
		{
			string app = "";
#if UNITY_EDITOR_WIN
			app = "cmd.exe";
#elif UNITY_EDITOR_OSX
			app = "bash";
#endif
			return app;
		}
	}

	public static void Execute(string cmd, bool showOutputWindow = true, System.Action onCompleted = null, string windowTitle = "Output")
	{
		EditorCommandWindow window = GetWindow(showOutputWindow, windowTitle);
		LaunchProcess(cmd, showOutputWindow, onCompleted, windowTitle, window);
	}

	public static void ExecuteAsync(string cmd, bool showOutputWindow = true, System.Action onCompleted = null, string windowTitle = "Output")
	{
		EditorCommandWindow window = GetWindow(showOutputWindow, windowTitle);
		System.Threading.ThreadPool.QueueUserWorkItem(delegate (object state)
		{
			LaunchProcess(cmd, showOutputWindow, onCompleted, windowTitle, window);
		});
	}

	static void LaunchProcess(string cmd, bool showOutputWindow = true, System.Action onCompleted = null, string windowTitle = "Output", EditorCommandWindow window = null)
	{
		ProcessStartInfo start = new ProcessStartInfo(CommandApp);

#if UNITY_EDITOR_OSX
		start.Arguments = "-c";
#elif UNITY_EDITOR_WIN
        start.Arguments = "/c";
#endif

		start.Arguments += (" \"" + cmd + " \"");
		start.CreateNoWindow = true;
		start.ErrorDialog = true;
		start.UseShellExecute = false;
		start.WorkingDirectory = "";
		start.RedirectStandardOutput = true;
		start.RedirectStandardError = true;
		start.RedirectStandardInput = true;
		start.StandardOutputEncoding = Encoding.UTF8;
		start.StandardErrorEncoding = Encoding.UTF8;

		Process process = new Process
		{
			StartInfo = start,
			EnableRaisingEvents = true
		};

		process.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
		{
			EditorCommandWindow.onResponse.Invoke(e.Data);
		});

		process.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
		{
			if (!string.IsNullOrEmpty(e.Data))
				EditorCommandWindow.onError.Invoke(e.Data);
		});

		process.Exited += new System.EventHandler((s, e) =>
		{
			onCompleted?.Invoke();
			if (showOutputWindow)
				window.Show();
		});

		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		process.WaitForExit();
		process.Close();
		process.Dispose();
	}

    static EditorCommandWindow GetWindow(bool showOutputWindow = true, string windowTitle = "Output")
    {
		EditorCommandWindow window = null;
		if (showOutputWindow)
		{
			window = EditorCommandWindow.GetWindow(windowTitle);
			window.Init();
		}

		return window;
	}
}
