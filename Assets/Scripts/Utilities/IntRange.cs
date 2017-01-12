using System;

[Serializable]
public class IntRange
{
	public int minimum;
	public int maximum;


	// Constructor to set the values.
	public IntRange(int min, int max)
	{
		minimum = min;
		maximum = max;
	}


	// Get a random value from the range.
	public int Random
	{
		get { return UnityEngine.Random.Range(minimum, maximum); }
	}
}