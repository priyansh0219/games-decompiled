using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Layouts;

internal static class DualSenseEdgeSupport
{
	static DualSenseEdgeSupport()
	{
		Initialize();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitializeInPlayer()
	{
	}

	private static void Initialize()
	{
		InputSystem.RegisterLayout<DualSenseGamepadHID>(null, default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 1356).WithCapability("productId", 3570));
	}
}
