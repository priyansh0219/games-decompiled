using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugDisplay : MonoBehaviour
{
	private struct Line
	{
		public string text;

		public Color color;
	}

	private float scrollAmount;

	private bool autoScroll = true;

	private int lastCollectionCount1;

	private int lastCollectionCount2;

	private float lastCollectionTime;

	public Font font;

	public int fontSize;

	public FontStyle fontStyle;

	private Material solidMaterial;

	private static List<Line> lines = new List<Line>();

	public static void AddLine(string text)
	{
		AddLine(text, Color.white);
	}

	public static void AddLine(string text, Color color)
	{
		Line item = default(Line);
		item.text = text;
		item.color = color;
		lines.Add(item);
	}

	public void Start()
	{
		Shader debugDisplaySolid = ShaderManager.preloadedShaders.DebugDisplaySolid;
		solidMaterial = new Material(debugDisplaySolid);
		solidMaterial.hideFlags = HideFlags.HideAndDontSave;
		solidMaterial.SetInt(ShaderPropertyID._SrcBlend, 5);
		solidMaterial.SetInt(ShaderPropertyID._DstBlend, 10);
	}

	public void OnEnable()
	{
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(Render));
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(Render));
	}

	public void OnDisable()
	{
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(Render));
	}

	private void DrawText(float x, float y, string text, Color color)
	{
		font.RequestCharactersInTexture(text, fontSize, fontStyle);
		Vector3 vector = new Vector3(x, y, 0f);
		font.material.SetPass(0);
		GL.Begin(7);
		GL.Color(color);
		foreach (char ch in text)
		{
			if (font.GetCharacterInfo(ch, out var info, fontSize, fontStyle))
			{
				GL.TexCoord(info.uvTopLeft);
				GL.Vertex(vector + new Vector3(info.minX, info.maxY, 0f));
				GL.TexCoord(info.uvTopRight);
				GL.Vertex(vector + new Vector3(info.maxX, info.maxY, 0f));
				GL.TexCoord(info.uvBottomRight);
				GL.Vertex(vector + new Vector3(info.maxX, info.minY, 0f));
				GL.TexCoord(info.uvBottomLeft);
				GL.Vertex(vector + new Vector3(info.minX, info.minY, 0f));
				vector += new Vector3(info.advance, 0f, 0f);
			}
		}
		GL.End();
	}

	private float GetLineHeight()
	{
		return (float)(font.lineHeight * fontSize) / (float)font.fontSize;
	}

	public void Render(Camera camera)
	{
		GL.PushMatrix();
		GL.LoadPixelMatrix();
		solidMaterial.SetPass(0);
		float num = Screen.width;
		float num2 = Screen.height;
		GL.Begin(7);
		GL.Color(new Color(0f, 0f, 0f, 0.8f));
		GL.Vertex3(0f, 0f, 0f);
		GL.Vertex3(0f + num, 0f, 0f);
		GL.Vertex3(0f + num, 0f + num2, 0f);
		GL.Vertex3(0f, 0f + num2, 0f);
		GL.End();
		float lineHeight = GetLineHeight();
		float num3 = scrollAmount - lineHeight;
		int num4 = Math.Max(Mathf.FloorToInt((num3 - (float)Screen.height) / lineHeight), 0);
		int num5 = Math.Min(Mathf.CeilToInt(num3 / lineHeight), lines.Count - 1);
		float x = 0f;
		float num6 = num3 - (float)num4 * lineHeight;
		for (int i = num4; i <= num5; i++)
		{
			DrawText(x, num6, lines[i].text, lines[i].color);
			num6 -= lineHeight;
		}
		GL.PopMatrix();
	}

	private void Update()
	{
		if (Input.GetAxis("ControllerAxis6") > 0.001f)
		{
			autoScroll = true;
		}
		float num = GetLineHeight() * (float)lines.Count;
		if (autoScroll)
		{
			scrollAmount = num;
		}
		float axis = Input.GetAxis("ControllerAxis7");
		if (Mathf.Abs(axis) > 0.001f)
		{
			scrollAmount = Mathf.Max(scrollAmount - axis * Time.unscaledDeltaTime * 300f, 0f);
			autoScroll = false;
		}
		if (scrollAmount >= num)
		{
			scrollAmount = num;
			autoScroll = true;
		}
		if (Time.unscaledDeltaTime > 0.1f)
		{
			AddLine($"Frame {Time.frameCount - 1} took {Time.unscaledDeltaTime * 1000f}ms", Color.green);
		}
		int num2 = GC.CollectionCount(1);
		int num3 = GC.CollectionCount(2);
		if (num3 > lastCollectionCount2 || num2 > lastCollectionCount1)
		{
			AddLine(string.Format("Frame {0} had a garbage collection event", Time.frameCount - 1, Time.unscaledDeltaTime * 1000f), Color.blue);
		}
		lastCollectionCount1 = num2;
		lastCollectionCount2 = num3;
	}
}
