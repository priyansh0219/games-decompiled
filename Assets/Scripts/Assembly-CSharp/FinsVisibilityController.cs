using UnityEngine;

public class FinsVisibilityController : MonoBehaviour
{
	public Renderer renderer;

	private Player player;

	private void Awake()
	{
		player = GetComponentInParent<Player>();
	}

	private void Update()
	{
		if (!(renderer == null) && !(player == null))
		{
			renderer.enabled = player.IsSwimming();
		}
	}
}
