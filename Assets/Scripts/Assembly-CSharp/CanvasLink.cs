using UnityEngine;
using UnityEngine.UI;

public class CanvasLink : MonoBehaviour
{
	[SerializeField]
	[AssertNotNull]
	private Canvas[] canvases;

	[SerializeField]
	private RectMask2D[] rectMasks;

	[SerializeField]
	[AssertNotNull]
	private Renderer renderer;

	private void Start()
	{
		SetCanvasesEnabled(renderer.isVisible);
		SetRectMasksEnabled(renderer.isVisible);
	}

	private void OnBecameVisible()
	{
		if (base.enabled)
		{
			SetCanvasesEnabled(enabled: true);
			SetRectMasksEnabled(enabled: true);
		}
	}

	private void OnBecameInvisible()
	{
		if (base.enabled)
		{
			SetCanvasesEnabled(enabled: false);
			SetRectMasksEnabled(enabled: false);
		}
	}

	private void SetCanvasesEnabled(bool enabled)
	{
		Canvas[] array = canvases;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = enabled;
		}
	}

	private void SetRectMasksEnabled(bool enabled)
	{
		RectMask2D[] array = rectMasks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = enabled;
		}
	}
}
