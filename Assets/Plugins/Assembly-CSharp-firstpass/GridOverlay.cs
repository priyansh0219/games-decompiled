using UnityEngine;

public class GridOverlay : MonoBehaviour
{
	public GameObject plane;

	public bool showMain = true;

	public bool showSub;

	public int gridSizeX;

	public int gridSizeY;

	public int gridSizeZ;

	public float smallStep;

	public float largeStep;

	public float startX;

	public float startY;

	public float startZ;

	private float offsetY;

	private float scrollRate = 0.1f;

	private float lastScroll;

	private Material lineMaterial;

	private Color mainColor = new Color(0f, 1f, 0f, 1f);

	private Color subColor = new Color(0f, 0.5f, 0f, 1f);

	private void Start()
	{
	}

	private void Update()
	{
		if (lastScroll + scrollRate < Time.time)
		{
			if (Input.GetKey(KeyCode.KeypadPlus))
			{
				plane.transform.position = new Vector3(plane.transform.position.x, plane.transform.position.y + smallStep, plane.transform.position.z);
				offsetY += smallStep;
				lastScroll = Time.time;
			}
			if (Input.GetKey(KeyCode.KeypadMinus))
			{
				plane.transform.position = new Vector3(plane.transform.position.x, plane.transform.position.y - smallStep, plane.transform.position.z);
				offsetY -= smallStep;
				lastScroll = Time.time;
			}
		}
	}

	private void CreateLineMaterial()
	{
		if (!lineMaterial)
		{
			lineMaterial = new Material("Shader \"Lines/Colored Blended\" {SubShader { Pass {     Blend SrcAlpha OneMinusSrcAlpha     ZWrite Off Cull Off Fog { Mode Off }     BindChannels {      Bind \"vertex\", vertex Bind \"color\", color }} } }");
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	private void OnPostRender()
	{
		CreateLineMaterial();
		lineMaterial.SetPass(0);
		GL.Begin(1);
		if (showSub)
		{
			GL.Color(subColor);
			for (float num = 0f; num <= (float)gridSizeY; num += smallStep)
			{
				for (float num2 = 0f; num2 <= (float)gridSizeZ; num2 += smallStep)
				{
					GL.Vertex3(startX, num + offsetY, startZ + num2);
					GL.Vertex3(gridSizeX, num + offsetY, startZ + num2);
				}
				for (float num3 = 0f; num3 <= (float)gridSizeX; num3 += smallStep)
				{
					GL.Vertex3(startX + num3, num + offsetY, startZ);
					GL.Vertex3(startX + num3, num + offsetY, gridSizeZ);
				}
			}
			for (float num4 = 0f; num4 <= (float)gridSizeZ; num4 += smallStep)
			{
				for (float num5 = 0f; num5 <= (float)gridSizeX; num5 += smallStep)
				{
					GL.Vertex3(startX + num5, startY + offsetY, startZ + num4);
					GL.Vertex3(startX + num5, (float)gridSizeY + offsetY, startZ + num4);
				}
			}
		}
		if (showMain)
		{
			GL.Color(mainColor);
			for (float num6 = 0f; num6 <= (float)gridSizeY; num6 += largeStep)
			{
				for (float num7 = 0f; num7 <= (float)gridSizeZ; num7 += largeStep)
				{
					GL.Vertex3(startX, num6 + offsetY, startZ + num7);
					GL.Vertex3(gridSizeX, num6 + offsetY, startZ + num7);
				}
				for (float num8 = 0f; num8 <= (float)gridSizeX; num8 += largeStep)
				{
					GL.Vertex3(startX + num8, num6 + offsetY, startZ);
					GL.Vertex3(startX + num8, num6 + offsetY, gridSizeZ);
				}
			}
			for (float num9 = 0f; num9 <= (float)gridSizeZ; num9 += largeStep)
			{
				for (float num10 = 0f; num10 <= (float)gridSizeX; num10 += largeStep)
				{
					GL.Vertex3(startX + num10, startY + offsetY, startZ + num9);
					GL.Vertex3(startX + num10, (float)gridSizeY + offsetY, startZ + num9);
				}
			}
		}
		GL.End();
	}
}
