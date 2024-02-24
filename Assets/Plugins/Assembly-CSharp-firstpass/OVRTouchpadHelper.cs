using System;
using UnityEngine;

public sealed class OVRTouchpadHelper : MonoBehaviour
{
	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		OVRTouchpad.TouchHandler += LocalTouchEventCallback;
	}

	private void Update()
	{
		OVRTouchpad.Update();
	}

	public void OnDisable()
	{
		OVRTouchpad.OnDisable();
	}

	private void LocalTouchEventCallback(object sender, EventArgs args)
	{
		switch (((OVRTouchpad.TouchArgs)args).TouchType)
		{
		}
	}
}
