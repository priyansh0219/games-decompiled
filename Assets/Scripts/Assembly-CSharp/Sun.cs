using UnityEngine;

[RequireComponent(typeof(Light))]
public class Sun : MonoBehaviour
{
	public static Sun main;

	public Light sunLight;

	private void Awake()
	{
		Debug.Log("Sun.Awake");
		main = this;
		base.gameObject.SetActive(value: false);
	}
}
