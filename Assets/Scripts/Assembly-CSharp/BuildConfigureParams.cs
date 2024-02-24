using UnityEngine;

public class BuildConfigureParams
{
	public GameObject player;

	public GameObject target;

	public BuildConfigureParams(GameObject p, GameObject t)
	{
		player = p;
		target = t;
	}
}
