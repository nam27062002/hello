
[System.Serializable]
public struct Reward  {
	public int score;

	[Separator("", 7)]
	public int coins;
	public int pc;

	[Separator("", 7)]
	public float health;
	public float energy;
	public float fury;

	[Separator("", 7)]
	public float xp;
}
