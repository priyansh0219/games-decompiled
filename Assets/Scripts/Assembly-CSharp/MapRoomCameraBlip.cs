using TMPro;
using UnityEngine;

public class MapRoomCameraBlip : MonoBehaviour
{
	[AssertNotNull]
	public Transform blipTransform;

	[AssertNotNull]
	public Transform canvasTransform;

	[AssertNotNull]
	public TextMeshProUGUI cameraName;

	private void LateUpdate()
	{
		Vector3 forward = MainCamera.camera.transform.forward;
		blipTransform.rotation = Quaternion.LookRotation(forward);
		forward.y = 0f;
		canvasTransform.rotation = Quaternion.LookRotation(forward);
	}
}
