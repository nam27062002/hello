
[System.Serializable]
public struct Reward  {
	public float score;

	[Separator("", 7)]
	public float coins;
	public int pc;

	[Separator("", 7)]
	public float health;
	public float energy;
	public float fury;	// Main source for fury is score, this is an extra

	[Separator("", 7)]
	public float xp;

	public string origin;

	public string category;


	//--------------------------------------------------------------------------//
	// OPERATORS																//
	//--------------------------------------------------------------------------//
	/// <summary>
	/// Addition operator for the reward class.
	/// </summary>
	/// <returns>The result of adding component by component _r1 with _r2.</returns>
	/// <param name="_r1">The first reward object.</param>
	/// <param name="_r2">The second reward object.</param>
	public static Reward operator + (Reward _r1, Reward _r2) {
		Reward newReward;
		newReward.score = _r1.score + _r2.score;
		newReward.coins = _r1.coins + _r2.coins;
		newReward.pc = _r1.pc + _r2.pc;
		newReward.health = _r1.health + _r2.health;
		newReward.energy = _r1.energy + _r2.energy;
		newReward.fury = _r1.fury + _r2.fury;
		newReward.xp = _r1.xp + _r2.xp;
		newReward.origin = _r1.origin + _r2.origin;
		newReward.category = "Misc";
		return newReward;
	}

	/// <summary>
	/// Subtraction operator for the reward class.
	/// </summary>
	/// <returns>The result of subtracting component by component _r1 with _r2.</returns>
	/// <param name="_r1">The first reward object.</param>
	/// <param name="_r2">The second reward object.</param>
	public static Reward operator - (Reward _r1, Reward _r2) {
		Reward newReward;
		newReward.score = _r1.score - _r2.score;
		newReward.coins = _r1.coins - _r2.coins;
		newReward.pc = _r1.pc - _r2.pc;
		newReward.health = _r1.health - _r2.health;
		newReward.energy = _r1.energy - _r2.energy;
		newReward.fury = _r1.fury - _r2.fury;
		newReward.xp = _r1.xp - _r2.xp;
		newReward.origin = "-";
		newReward.category = "Misc";
		return newReward;
	}

	/// <summary>
	/// Multiplication operator for the reward class.
	/// </summary>
	/// <returns>The result of multiplying component by component _r1 with _r2.</returns>
	/// <param name="_r1">The first reward object.</param>
	/// <param name="_r2">The second reward object.</param>
	public static Reward operator * (Reward _r1, Reward _r2) {
		Reward newReward;
		newReward.score = _r1.score * _r2.score;
		newReward.coins = _r1.coins * _r2.coins;
		newReward.pc = _r1.pc * _r2.pc;
		newReward.health = _r1.health * _r2.health;
		newReward.energy = _r1.energy * _r2.energy;
		newReward.fury = _r1.fury * _r2.fury;
		newReward.xp = _r1.xp * _r2.xp;
		newReward.origin = "*";
		newReward.category = "Misc";
		return newReward;
	}

	/// <summary>
	/// Division operator for the reward class.
	/// </summary>
	/// <returns>The result of dividing component by component _r1 with _r2.</returns>
	/// <param name="_r1">The first reward object.</param>
	/// <param name="_r2">The second reward object.</param>
	public static Reward operator / (Reward _r1, Reward _r2) {
		Reward newReward;
		newReward.score = _r1.score / _r2.score;
		newReward.coins = _r1.coins / _r2.coins;
		newReward.pc = _r1.pc / _r2.pc;
		newReward.health = _r1.health / _r2.health;
		newReward.energy = _r1.energy / _r2.energy;
		newReward.fury = _r1.fury / _r2.fury;
		newReward.xp = _r1.xp / _r2.xp;
		newReward.origin = "/";
		newReward.category = "Misc";
		return newReward;
	}

	//--------------------------------------------------------------------------//
	// SCALAR OPERATORS															//
	//--------------------------------------------------------------------------//
	/// <summary>
	/// Expand operator for the reward class.
	/// </summary>
	/// <returns>The result of adding _amount to each component in _r1.</returns>
	/// <param name="_r1">The reward object.</param>
	/// <param name="_amount">The amount to be added.</param>
	public static Reward operator + (Reward _r1, float _amount) {
		Reward newReward;
		newReward.score = (int)(_r1.score + _amount);
		newReward.coins = (_r1.coins + _amount);
		newReward.pc = (int)(_r1.pc + _amount);
		newReward.health = _r1.health + _amount;
		newReward.energy = _r1.energy + _amount;
		newReward.fury = _r1.fury + _amount;
		newReward.xp = _r1.xp + _amount;
		newReward.origin = _r1.origin;
		newReward.category = _r1.category;
		return newReward;
	}

	/// <summary>
	/// Scale operator for the reward class.
	/// </summary>
	/// <returns>The result of multiplying each component in _r1 by the given factor.</returns>
	/// <param name="_r1">The reward object.</param>
	/// <param name="_factor">The factor to be applied.</param>
	public static Reward operator * (Reward _r1, float _factor) {
		Reward newReward;
		newReward.score = (int)(_r1.score * _factor);
		newReward.coins = (_r1.coins * _factor);
		newReward.pc = (int)(_r1.pc * _factor);
		newReward.health = _r1.health * _factor;
		newReward.energy = _r1.energy * _factor;
		newReward.fury = _r1.fury * _factor;
		newReward.xp = _r1.xp * _factor;
		newReward.origin = _r1.origin;
		newReward.category = _r1.category;
		return newReward;
	}

	/// <summary>
	/// Sets the no reward.
	/// </summary>
	public void SetNoReward(){		
		score = pc = 0;
		coins = health = energy = xp = fury = 0f;
	}
}
