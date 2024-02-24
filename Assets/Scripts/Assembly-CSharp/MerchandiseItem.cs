using UnityEngine;

public class MerchandiseItem : MonoBehaviour
{
	public MeshRenderer iconQuad;

	private Texture2D iconTex;

	public void SetIcon(Texture2D texture)
	{
		iconTex = texture;
	}
}
