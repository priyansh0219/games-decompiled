using UnityEngine;

public class RendererDebugger : MonoBehaviour
{
	[SerializeField]
	private Renderer[] renderers;

	private void OnDrawGizmos()
	{
		if (renderers == null)
		{
			return;
		}
		bool flag = true;
		Bounds bounds = default(Bounds);
		Renderer[] array = renderers;
		for (int i = 0; i < array.Length; i++)
		{
			Bounds bounds2 = array[i].bounds;
			Gizmos.DrawWireCube(bounds2.center, bounds2.size);
			if (flag)
			{
				bounds = bounds2;
				flag = false;
			}
			else
			{
				bounds.Encapsulate(bounds2);
			}
		}
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}
}
