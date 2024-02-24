using UnityEngine;

public abstract class FadeLightBase : MonoBehaviour
{
	public abstract void Fade(float fade);

	public abstract Color EditorPreview(float fade, float dayLightScalar, Color color, float fraction);

	public abstract void ResetPreview();
}
