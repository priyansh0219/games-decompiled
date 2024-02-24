using UnityEngine;

public class DebugComponentEvents : MonoBehaviour
{
	private void Awake()
	{
		Debug.Log("Awake frame " + Time.frameCount);
	}

	private void Start()
	{
		Debug.Log("Start frame " + Time.frameCount);
	}

	private void OnEnable()
	{
		Debug.Log("OnEnable frame " + Time.frameCount);
	}

	private void Update()
	{
	}
}
