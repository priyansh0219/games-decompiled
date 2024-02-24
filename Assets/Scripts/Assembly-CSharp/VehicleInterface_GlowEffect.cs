using UnityEngine;

public class VehicleInterface_GlowEffect : MonoBehaviour
{
	public float cycle = 20f;

	public float minAlpha;

	public float maxAlpha = 0.05f;

	private Material myMat;

	private void Start()
	{
		myMat = GetComponent<Renderer>().material;
	}

	private void Update()
	{
		float a = myMat.GetColor(ShaderPropertyID._Color).a;
		a = Mathf.Max(minAlpha, Mathf.PingPong(Time.time / cycle, maxAlpha));
		Color white = Color.white;
		white.a = a;
		myMat.SetColor(ShaderPropertyID._Color, white);
	}
}
