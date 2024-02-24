using UnityEngine;

public class Ocean : MonoBehaviour
{
	public enum DepthClass
	{
		Surface = 0,
		Safe = 1,
		Unsafe = 2,
		Crush = 3
	}

	public static Ocean main;

	public float defaultOceanLevel;

	private void Awake()
	{
		main = this;
	}

	public static float GetOceanLevel()
	{
		if ((bool)main)
		{
			return main.transform.position.y;
		}
		return 0f;
	}

	public static void SetOceanLevel(float level)
	{
		if ((bool)main)
		{
			Vector3 position = main.transform.position;
			position.y = level;
			main.transform.position = position;
		}
	}

	public void RestoreOceanLevel()
	{
		SetOceanLevel(defaultOceanLevel);
	}

	public static float GetDepthOf(GameObject obj)
	{
		return GetDepthOf(obj.transform.position);
	}

	public static float GetDepthOf(Transform tr)
	{
		return GetDepthOf(tr.position);
	}

	public static float GetDepthOf(Vector3 pos)
	{
		return Mathf.Max(0f, GetOceanLevel() - pos.y);
	}
}
