using UnityEngine;

public class PrecursorPeeperPipes : MonoBehaviour, ICompileTimeCheckable
{
	[AssertNotNull]
	public GameObject[] peepers;

	public float chance = 0.5f;

	private void peeper1start()
	{
		RandomShow(peepers[0]);
	}

	private void peeper2start()
	{
		RandomShow(peepers[1]);
	}

	private void peeper3start()
	{
		RandomShow(peepers[2]);
	}

	private void peeper4start()
	{
		RandomShow(peepers[3]);
	}

	private void peeper5start()
	{
		RandomShow(peepers[4]);
	}

	private void peeper6start()
	{
		RandomShow(peepers[5]);
	}

	private void peeper7start()
	{
		RandomShow(peepers[6]);
	}

	private void peeper8start()
	{
		RandomShow(peepers[7]);
	}

	private void peeper9start()
	{
		RandomShow(peepers[8]);
	}

	private void peeper10start()
	{
		RandomShow(peepers[9]);
	}

	private void peeper11start()
	{
		RandomShow(peepers[10]);
	}

	private void peeper12start()
	{
		RandomShow(peepers[11]);
	}

	private void peeper13start()
	{
		RandomShow(peepers[12]);
	}

	private void peeper14start()
	{
		RandomShow(peepers[13]);
	}

	private void peeper1end()
	{
		Hide(peepers[0]);
	}

	private void peeper2end()
	{
		Hide(peepers[1]);
	}

	private void peeper3end()
	{
		Hide(peepers[2]);
	}

	private void peeper4end()
	{
		Hide(peepers[3]);
	}

	private void peeper5end()
	{
		Hide(peepers[4]);
	}

	private void peeper6end()
	{
		Hide(peepers[5]);
	}

	private void peeper7end()
	{
		Hide(peepers[6]);
	}

	private void peeper8end()
	{
		Hide(peepers[7]);
	}

	private void peeper9end()
	{
		Hide(peepers[8]);
	}

	private void peeper10end()
	{
		Hide(peepers[9]);
	}

	private void peeper11end()
	{
		Hide(peepers[10]);
	}

	private void peeper12end()
	{
		Hide(peepers[11]);
	}

	private void peeper13end()
	{
		Hide(peepers[12]);
	}

	private void peeper14end()
	{
		Hide(peepers[13]);
	}

	private void RandomShow(GameObject go)
	{
		if (Random.value < chance)
		{
			go.SetActive(value: true);
		}
	}

	private void Hide(GameObject go)
	{
		go.SetActive(value: false);
	}

	public string CompileTimeCheck()
	{
		if (peepers.Length != 14)
		{
			return "Peeper count must match event count";
		}
		return null;
	}
}
