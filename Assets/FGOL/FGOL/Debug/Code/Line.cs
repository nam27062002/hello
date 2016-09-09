using UnityEngine;

struct Line
{
	public Line(string text, LogType type) : this()
	{
		this.text = text;
		this.type = type;
	}

	public string text { get; private set; }
	public LogType type { get; private set; }
}