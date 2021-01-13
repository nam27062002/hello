using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

class LogWindow : MonoBehaviour
{
	const int ButtonSize = 60;
	const int MaximizeButtonSize = 80;
	const int LabelBuffer = 5;

	bool m_minimized = true;
	bool m_messages = true;
	bool m_warnings = true;
	bool m_errors = true;
	bool m_exceptions = true;
	bool m_stacktraces = true;
	string m_filter = "";

	bool m_clearPopUp;
	Vector2 m_scrollPosition;

	float m_lineHeight;
	int m_amountOfLines;
	float m_heightIncludingScrollbar;
	float m_scrollViewHeight;
	Rect m_windowRect;
	
	GUIStyle m_labelStyle;
	GUIStyle m_textFieldStyle;

	Log m_log;

	List<Entry> m_preInitializationEntries = new List<Entry>();

	void Awake()
	{
        //Register with LogCallbackHandler to allow for multiple Application.RegisterLogCallback Handlers!
        LogCallbackHandler.RegisterLogCallbackThreaded(Log);
	}

	void Initialize()
	{
		m_labelStyle = new GUIStyle(GUI.skin.label) { wordWrap = false };
		m_textFieldStyle = new GUIStyle(GUI.skin.textField) { fontSize = 30 };

		if (Screen.dpi > 72)
		{
			var factor = Screen.dpi / 100;
			m_labelStyle.fontSize = (int)(10 * factor);
			GUI.skin.horizontalScrollbar.fixedHeight *= factor;
			GUI.skin.horizontalScrollbarThumb.fixedHeight *= factor;
			GUI.skin.verticalScrollbar.fixedWidth *= factor;
			GUI.skin.verticalScrollbarThumb.fixedWidth *= factor;
		}

		var height = .5f*Screen.height;
		m_lineHeight = m_labelStyle.CalcHeight(new GUIContent("A"), 100);
		m_scrollViewHeight = height - ButtonSize;
		m_scrollViewHeight -= m_scrollViewHeight%m_lineHeight;
		m_heightIncludingScrollbar = m_scrollViewHeight + GUI.skin.horizontalScrollbar.fixedHeight;
		m_windowRect = new Rect(0, Screen.height - m_heightIncludingScrollbar, Screen.width, m_heightIncludingScrollbar);
		m_amountOfLines = (int) (m_scrollViewHeight/m_lineHeight);
		m_log = new Log(m_amountOfLines);

		foreach (var entry in m_preInitializationEntries)
			ProcessLog(entry.message, entry.stacktrace, entry.type);

		m_preInitializationEntries = null;
	}

	void Log(string message, string stacktrace, LogType type)
	{
		if (m_labelStyle == null)
			m_preInitializationEntries.Add(new Entry(message, stacktrace, type));
		else
			ProcessLog(message, stacktrace, type);
	}

	void ProcessLog(string message, string stacktrace, LogType type)
	{
		if (type == LogType.Error || type == LogType.Exception)
			m_minimized = false;

		var messageLines = message.Split('\n');

		var stacktraceLines =
			stacktrace.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
			.Skip(type == LogType.Exception ? 0 : 1)
			.Select(s => "  " + s)
			.ToArray();

		var width = 0f;
		// ReSharper disable once LoopCanBeConvertedToQuery
		// IEnumerable.Max() causes AOT error
		foreach (var l in messageLines.Concat(stacktraceLines))
			width = Math.Max(width, Width(l));
		m_log.Add(new LogEntry(messageLines, stacktraceLines, width, type));
		UpdateScrollPos();
	}

	float Width(string message)
	{
        return m_labelStyle.CalcSize(new GUIContent(message)).x + 2 * LabelBuffer;
	}

	void OnGUI()
	{
		if (m_labelStyle == null)
			Initialize();

		if (m_clearPopUp)
			ClearPopUp();
		else if (m_minimized)
			MinimizedUi();
		else
			MaximizedUi();
	}

	void ClearPopUp()
	{
		const int height = 50;
		const int width = 100;

		var hSpacing = (Screen.width - 2*width)/3;
		var vSpacing = (Screen.height - height)/2;

		if (GUI.Button(new Rect(hSpacing, Screen.height - vSpacing, width, height), "<b>CLEAR</b>"))
		{
			m_log.Clear();
			m_clearPopUp = false;
		}

		if (GUI.Button(new Rect(2*hSpacing + width, Screen.height - vSpacing, width, height), "<b>CANCEL</b>"))
			m_clearPopUp = false;
	}

	void MinimizedUi()
	{
		if (GUI.Button(new Rect(0, Screen.height - MaximizeButtonSize, MaximizeButtonSize, MaximizeButtonSize), "", GUIStyle.none))
			m_minimized = false;
	}

	void MaximizedUi()
	{
		TopBar();
		ScrollView();
	}

	void TopBar()
	{
		var rect = new Rect(0, Screen.height - (m_heightIncludingScrollbar + ButtonSize), ButtonSize, ButtonSize);

		Toggle("<b>Msg</b>", ref m_messages, ref rect);
		Toggle("<b>Wrn</b>", ref m_warnings, ref rect);
		Toggle("<b>Err</b>", ref m_errors, ref rect);
		Toggle("<b>Exc</b>", ref m_exceptions, ref rect);
		Toggle("<b>Trc</b>", ref m_stacktraces, ref rect);

		rect.xMax = Screen.width - 4*ButtonSize;
		var newFilter = GUI.TextField(rect, m_filter, m_textFieldStyle);

		if (newFilter != m_filter)
		{
			m_filter = newFilter;
			FilterChanged();
		}

		if (Button("<b>X</b>", ref rect))
		{
			m_filter = "";
			FilterChanged();
		}

		if (Button("<b>Clr</b>", ref rect))
			m_clearPopUp = true;

		if (Button("<b>Dmp</b>", ref rect))
		{                        
			var path = Path.Combine(FGOL.Plugins.Native.NativeBinding.Instance.GetPersistentDataPath(), "Log_dump_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt");
			File.WriteAllText(path, m_log.GetDump());
			Debug.Log("Log dumped to " + path);         
		}

		if (Button("<b>_</b>", ref rect))
			m_minimized = true;
	}

	void Toggle(string text, ref bool field, ref Rect rect)
	{
		//if (!field)
		//{
		//	GUI.skin.button.normal.textColor = Color.grey;
		//	GUI.skin.button.hover.textColor = Color.grey;
		//}
	
		if (GUI.Button(rect, text))
		{
			field = !field;
			FilterChanged();
		}

		rect.xMin += ButtonSize;
		rect.xMax += ButtonSize;
		//GUI.skin.button.normal.textColor = Color.white;
		//GUI.skin.button.hover.textColor = Color.white;
	}

	static bool Button(string text, ref Rect rect)
	{
		rect.xMin = rect.xMax;
		rect.xMax += ButtonSize;
		return GUI.Button(rect, text);
	}

	void ScrollView()
	{
		GUI.Box(m_windowRect, "");
		
		var labelRect = new Rect(
			m_windowRect.x - m_scrollPosition.x + LabelBuffer,
			m_windowRect.y,
			Screen.width + m_scrollPosition.x - LabelBuffer - GUI.skin.verticalScrollbar.fixedWidth,
			m_lineHeight);

		foreach (var line in m_log.GetVisibleLines())
		{
			m_labelStyle.normal.textColor = GetColor(line.type);
			GUI.Label(labelRect, line.text, m_labelStyle);
			labelRect.y += m_lineHeight;
		}

		var viewWidth = Math.Max(Screen.width - GUI.skin.verticalScrollbar.fixedWidth, m_log.width);
		var viewHeight = Math.Max(m_scrollViewHeight, m_log.lines*m_lineHeight);
		var viewRect = new Rect(0, 0, viewWidth, viewHeight);

		m_scrollPosition = GUI.BeginScrollView(m_windowRect, m_scrollPosition, viewRect, true, true);
		GUI.EndScrollView();
		m_log.SetFirstVisible((int) (m_scrollPosition.y/m_lineHeight));
	}

	void FilterChanged()
	{
		m_log.SetFilter(m_messages, m_warnings, m_errors, m_exceptions, m_stacktraces, m_filter);
		UpdateScrollPos();
	}

	void UpdateScrollPos()
	{
		if (m_log.lines - m_amountOfLines <= 0)
			m_scrollPosition.y = 0;
		else
			m_scrollPosition.y = m_lineHeight*m_log.first;
	}

	static Color GetColor(LogType logType)
	{
		switch (logType)
		{
			case LogType.Exception:
				return Color.red;
			case LogType.Error:
				return new Color(1, .4f, 0);
			case LogType.Warning:
				return Color.yellow;
			default:
				return Color.white;
		}
	}

	class Entry
	{
		public string message { get; private set; }
		public string stacktrace { get; private set; }
		public LogType type { get; private set; }

		public Entry(string message, string stacktrace, LogType type)
		{
			this.message = message;
			this.stacktrace = stacktrace;
			this.type = type;
		}
	}
}