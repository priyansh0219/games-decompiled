using UnityEngine;

[RequireComponent(typeof(StarshipDoor))]
public class StarshipDoorLocked : MonoBehaviour
{
	public Texture2D unlockedTexture;

	public Texture2D lockedTexture;

	public GameObject materialObject;

	private Material myMat;

	public string textureReplaceString = "_Illum";

	private void Awake()
	{
		if ((bool)materialObject)
		{
			myMat = materialObject.GetComponent<MeshRenderer>().material;
		}
		if (NoCostConsoleCommand.main.unlockDoors)
		{
			SetDoorLockState(locked: false);
		}
	}

	public void SetDoorLockState(bool locked)
	{
		if (myMat != null)
		{
			Texture2D value = (locked ? lockedTexture : unlockedTexture);
			int nameID = Shader.PropertyToID(textureReplaceString);
			myMat.SetTexture(nameID, value);
		}
	}
}
