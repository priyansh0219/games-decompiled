using System;
using System.Collections.Generic;

public class ShuffleBag<C>
{
	private Random random = new Random();

	private List<C> data;

	private C currentItem;

	private int currentPosition = -1;

	private int Capacity => data.Capacity;

	public int Size => data.Count;

	public ShuffleBag(int initCapacity)
	{
		data = new List<C>(initCapacity);
	}

	public void Add(C item, int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			data.Add(item);
		}
		currentPosition = Size - 1;
	}

	public C Next()
	{
		if (currentPosition < 1)
		{
			currentPosition = Size - 1;
			currentItem = data[0];
			return currentItem;
		}
		int index = random.Next(currentPosition);
		currentItem = data[index];
		data[index] = data[currentPosition];
		data[currentPosition] = currentItem;
		currentPosition--;
		return currentItem;
	}
}
