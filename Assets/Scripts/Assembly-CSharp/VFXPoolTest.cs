using UnityEngine;

[ExecuteInEditMode]
public class VFXPoolTest : MonoBehaviour
{
	public bool usePooled;

	public float rate = 2f;

	public float randomPos = 10f;

	public GameObject[] fxPrefabs;

	public GameObject[] fxPooledPrefabs;

	private VFXPool.FX[] fx;

	private float timer;

	private void PlayRandomFX()
	{
		Vector3 position = base.transform.position;
		position.x += Random.Range(0f - randomPos, randomPos);
		position.z += Random.Range(0f - randomPos, randomPos);
		if (usePooled)
		{
			if (VFXPool.main != null)
			{
				int num = Random.Range(0, fx.Length);
				if (fx[num] != null)
				{
					VFXPool.main.Play(fx[num], position);
				}
			}
		}
		else
		{
			int num2 = Random.Range(0, fxPrefabs.Length);
			Object.Instantiate(fxPrefabs[num2], position, Quaternion.identity).GetComponent<ParticleSystem>().Play();
		}
	}

	private void Start()
	{
		fx = new VFXPool.FX[fxPooledPrefabs.Length];
		for (int i = 0; i < fxPooledPrefabs.Length; i++)
		{
			fx[i] = VFXPool.main.GetFX(fxPooledPrefabs[i]);
		}
	}

	private void Update()
	{
		timer += Time.deltaTime;
		if (timer > 1f / rate)
		{
			PlayRandomFX();
			timer = 0f;
		}
	}
}
