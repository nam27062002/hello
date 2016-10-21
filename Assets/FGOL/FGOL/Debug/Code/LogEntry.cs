using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class LogEntry
{
	readonly IList<string> m_message;
	readonly IList<string> m_stacktrace;

	public float width { get; private set; }
	public LogType logType { get; private set; }
	public string this[int index] { get { return index < m_message.Count ? m_message[index] : m_stacktrace[index - m_message.Count]; } }

	internal LogEntry(IList<string> message, IList<string> stacktrace, float width, LogType logType)
	{
		m_message = message;
		m_stacktrace = stacktrace;
		this.width = width;
		this.logType = logType;
	}

	internal int LineCount(bool includeStacktrace)
	{
		return includeStacktrace ? m_message.Count + m_stacktrace.Count : m_message.Count;
	}

	internal bool Matches(string pattern)
	{
		pattern = pattern.ToLower();
		return Matches(m_message, pattern) || Matches(m_stacktrace, pattern);
	}

	static bool Matches(IEnumerable<string> strings, string pattern)
	{
		return strings.Any(s => s.ToLower().Contains(pattern));
	}
}