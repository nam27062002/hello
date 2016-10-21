using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Log
{
	readonly int m_visibleLines;
	readonly List<LogEntry> m_entries = new List<LogEntry>();
	readonly List<LogEntry> m_filteredEntries = new List<LogEntry>();

	List<LogEntry> m_previousFilteredEntries;
	int m_anchorEntryIndex;
	int m_anchorOffset;

	bool m_messages = true;
	bool m_warnings = true;
	bool m_errors = true;
	bool m_exceptions = true;
	bool m_stacktraces = true;
	string m_filter = "";

	public int first { get; private set; }
	public int lines { get; private set; }
	public float width { get; private set; }

	public Log(int visibleLines)
	{
		m_visibleLines = visibleLines;
	}

	public void Add(LogEntry e)
	{
		m_entries.Add(e);

		if (!MatchesFilter(e))
			return;

		if (first > 0)
			m_previousFilteredEntries = new List<LogEntry>(m_filteredEntries);

		m_filteredEntries.Add(e);
		lines += e.LineCount(m_stacktraces);

		if (e.width > width)
			width = e.width;

		if (first > 0)
			AdjustFirst();
	}

	public void SetFirstVisible(int line)
	{
		first = line;
	}

	public void SetFilter(bool messages, bool warnings, bool errors, bool exceptions, bool stacktraces, string filter)
	{
		m_messages = messages;
		m_warnings = warnings;
		m_errors = errors;
		m_exceptions = exceptions;
		m_stacktraces = stacktraces;
		m_filter = filter;

		m_previousFilteredEntries = new List<LogEntry>(m_filteredEntries);

		m_filteredEntries.Clear();
		lines = 0;
		width = 0;

		foreach (var entry in m_entries.Where(MatchesFilter))
		{
			m_filteredEntries.Add(entry);
			lines += entry.LineCount(m_stacktraces);

			if (entry.width > width)
				width = entry.width;
		}

		AdjustFirst();
	}

	public IEnumerable<Line> GetVisibleLines()
	{
		var line = 0;
		var anchored = false;

		for (var entryIndex = m_filteredEntries.Count - 1; entryIndex >= 0; entryIndex--)
		{
			var entry = m_filteredEntries[entryIndex];
			if (line + entry.LineCount(m_stacktraces) > first)
			{
				var firstLineOfEntry = Math.Max(0, first - line);
				var lastLineOfEntry = Math.Min(entry.LineCount(m_stacktraces), firstLineOfEntry + m_visibleLines - Math.Max(0, line - first));

				if (!anchored)
				{
					anchored = true;
					m_anchorEntryIndex = entryIndex;
					m_anchorOffset = first - line;
				}

				for (var lineIndex = firstLineOfEntry; lineIndex < lastLineOfEntry; lineIndex++)
					yield return new Line(entry[lineIndex], entry.logType);

				line += lastLineOfEntry;
			}
			else
			{
				line += entry.LineCount(m_stacktraces);
			}

			if (line == first + m_visibleLines)
				break;
		}
	}

	public string GetDump()
	{
		var sb = new StringBuilder();

		for (var i = m_entries.Count - 1; i >= 0; i--)
		{
			var entry = m_entries[i];

			for (var j = 0; j < entry.LineCount(true); j++)
			{
				if (j == 0)
					sb.AppendLine(GetPrefix(entry.logType) + entry[j]);
				else
					sb.AppendLine(entry[j]);
			}
		}

		return sb.ToString();
	}

	public void Clear()
	{
		m_entries.Clear();
		m_filteredEntries.Clear();
		lines = 0;
		width = 0;
	}

	void AdjustFirst()
	{
		if (m_previousFilteredEntries.Count == 0)
		{
			first = 0;
		}
		else if (m_filteredEntries.Contains(m_previousFilteredEntries[m_anchorEntryIndex]))
		{
			first = LinesToAnchor() + m_anchorOffset;
		}
		else
		{
			int offset;
			var next = GetFirstEntryAfterAnchor(out offset);

			if (next != null)
				first = GetLinesToEntryAfterAnchor(next) - offset;
			else
				first = lines - m_visibleLines;
		}
	}

	int LinesToAnchor()
	{
		var linesToAnchor = 0;
		for (var i = m_filteredEntries.Count - 1; i >= 0; i--)
		{
			if (m_filteredEntries[i] == m_previousFilteredEntries[m_anchorEntryIndex])
				break;
			linesToAnchor += m_filteredEntries[i].LineCount(m_stacktraces);
		}
		return linesToAnchor;
	}

	LogEntry GetFirstEntryAfterAnchor(out int offset)
	{
		LogEntry next = null;
		offset = 0;

		for (var i = m_anchorEntryIndex; i >= 0; i--)
		{
			if (m_filteredEntries.Contains(m_previousFilteredEntries[i]))
			{
				next = m_previousFilteredEntries[i];
				break;
			}
			offset += m_previousFilteredEntries[i].LineCount(m_exceptions);
		}

		offset -= m_anchorOffset;
		return next;
	}

	int GetLinesToEntryAfterAnchor(LogEntry next)
	{
		var linesToEntryAfterAnchor = 0;

		for (var i = m_filteredEntries.Count - 1; i >= 0; i--)
		{
			if (m_filteredEntries[i] == next)
				break;

			linesToEntryAfterAnchor += m_filteredEntries[i].LineCount(m_exceptions);
		}
		return linesToEntryAfterAnchor;
	}

	bool MatchesFilter(LogEntry entry)
	{
		return (entry.logType == LogType.Log && m_messages
		        || entry.logType == LogType.Warning && m_warnings
		        || entry.logType == LogType.Error && m_errors
		        || entry.logType == LogType.Exception && m_exceptions)
		       && entry.Matches(m_filter);
	}

	static string GetPrefix(LogType logType)
	{
		switch (logType)
		{
			case LogType.Error:
				return "Error: ";
			case LogType.Warning:
				return "Warning: ";
			case LogType.Log:
				return "Message: ";
			default:
				return "";
		}
	}
}