using System.Collections.Generic;

public class CoinList : MonoSingleton<CoinList>
{
	public List<Coin> revolverCoinsList = new List<Coin>();

	public void AddCoin(Coin coin)
	{
		if (!revolverCoinsList.Contains(coin))
		{
			revolverCoinsList.Add(coin);
		}
	}

	public void RemoveCoin(Coin coin)
	{
		if (revolverCoinsList.Contains(coin))
		{
			revolverCoinsList.Remove(coin);
		}
	}
}
