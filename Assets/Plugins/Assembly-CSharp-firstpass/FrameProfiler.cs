using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

public class FrameProfiler : MonoBehaviour
{
	private struct Node
	{
		public string name;

		public long ticks;

		public int numCalls;

		public int parentIndex;

		public int childIndex;

		public int siblingIndex;
	}

	private class Frame
	{
		public Node[] nodes = new Node[maxNodes];

		public int numNodes;

		public long ticks;

		public long usedMemory;

		public bool garbageCollection;
	}

	private static bool capturing = true;

	private static int maxNodes = 1024;

	private static Frame currentFrame;

	private static int currentNodeIndex = -1;

	private static int currentFrameIndex = 0;

	private static long frameStartTicks = 0L;

	private static int lastCollectionCount2 = 0;

	private static int lastCollectionCount1 = 0;

	private static int maxFrames = 100;

	private static Frame[] frames = new Frame[maxFrames];

	public Font font;

	public int fontSize;

	public FontStyle fontStyle;

	private Material solidMaterial;

	private int displayFrameIndex = -1;

	private float xMoveRepeatTime;

	private static int AddNode(string name, int parentIndex, int siblingIndex = -1)
	{
		int numNodes = currentFrame.numNodes;
		currentFrame.numNodes++;
		currentFrame.nodes[numNodes].name = name;
		currentFrame.nodes[numNodes].parentIndex = parentIndex;
		currentFrame.nodes[numNodes].siblingIndex = siblingIndex;
		currentFrame.nodes[numNodes].numCalls = 0;
		currentFrame.nodes[numNodes].childIndex = -1;
		currentFrame.nodes[numNodes].ticks = 0L;
		return numNodes;
	}

	private static float TicksToMs(long ticks)
	{
		return (float)((double)ticks / ((double)Stopwatch.Frequency / 1000.0));
	}

	public static void Reset()
	{
		if (currentFrame != null)
		{
			currentFrame.ticks = Stopwatch.GetTimestamp() - frameStartTicks;
			currentFrame.usedMemory = Profiler.GetMonoUsedSizeLong();
			int num = GC.CollectionCount(1);
			int num2 = GC.CollectionCount(2);
			currentFrame.garbageCollection = num2 > lastCollectionCount2 || num > lastCollectionCount1;
			lastCollectionCount1 = num;
			lastCollectionCount2 = num2;
		}
		if (capturing)
		{
			currentFrameIndex = (currentFrameIndex + 1) % maxFrames;
			if (frames[currentFrameIndex] == null)
			{
				frames[currentFrameIndex] = new Frame();
			}
			currentFrame = frames[currentFrameIndex];
			currentFrame.ticks = 0L;
			currentFrame.numNodes = 0;
			currentNodeIndex = AddNode("Root", -1);
			frameStartTicks = Stopwatch.GetTimestamp();
		}
		else
		{
			currentFrame = null;
		}
	}

	public static void BeginSample(string name, UnityEngine.Object obj)
	{
		BeginSample(name);
	}

	public static void BeginSample(string name)
	{
		if (currentFrame == null)
		{
			return;
		}
		int num = currentFrame.nodes[currentNodeIndex].childIndex;
		while (num != -1 && !(currentFrame.nodes[num].name == name))
		{
			num = currentFrame.nodes[num].siblingIndex;
		}
		if (num == -1)
		{
			if (currentFrame.numNodes == maxNodes)
			{
				UnityEngine.Debug.LogFormat("Out of profiling nodes (maximum of {0})", maxNodes);
				return;
			}
			num = AddNode(name, currentNodeIndex, currentFrame.nodes[currentNodeIndex].childIndex);
			currentFrame.nodes[currentNodeIndex].childIndex = num;
		}
		currentNodeIndex = num;
		currentFrame.nodes[num].numCalls++;
		currentFrame.nodes[num].ticks -= Stopwatch.GetTimestamp();
	}

	public static void EndSample()
	{
		if (currentFrame != null)
		{
			currentFrame.nodes[currentNodeIndex].ticks += Stopwatch.GetTimestamp();
			currentNodeIndex = currentFrame.nodes[currentNodeIndex].parentIndex;
		}
	}

	public void Start()
	{
		Shader shader = Shader.Find("Hidden/Internal-Colored");
		solidMaterial = new Material(shader);
		solidMaterial.hideFlags = HideFlags.HideAndDontSave;
		solidMaterial.SetInt(ShaderPropertyID._SrcBlend, 5);
		solidMaterial.SetInt(ShaderPropertyID._DstBlend, 10);
	}

	public void OnEnable()
	{
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(Render));
	}

	public void OnDisable()
	{
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(Render));
	}

	public void Update()
	{
		Reset();
		ProcessInput();
		if (capturing)
		{
			displayFrameIndex = currentFrameIndex;
		}
	}

	private void ProcessInput()
	{
		Vector2 vector = default(Vector2);
		vector.x = Input.GetAxis("ControllerDPadX");
		vector.y = Input.GetAxis("ControllerDPadY");
		if ((Input.GetKeyDown(KeyCode.JoystickButton4) && Input.GetKey(KeyCode.JoystickButton5)) || (Input.GetKeyDown(KeyCode.JoystickButton5) && Input.GetKey(KeyCode.JoystickButton4)))
		{
			capturing = !capturing;
		}
		if (capturing)
		{
			return;
		}
		float num = 0.1f;
		if (Time.unscaledTime > xMoveRepeatTime)
		{
			if (vector.x > 0f)
			{
				displayFrameIndex = (displayFrameIndex + 1) % maxFrames;
				xMoveRepeatTime = Time.unscaledTime + num;
			}
			if (vector.x < 0f)
			{
				displayFrameIndex = (displayFrameIndex + maxFrames - 1) % maxFrames;
				xMoveRepeatTime = Time.unscaledTime + num;
			}
		}
	}

	public void Render(Camera camera)
	{
		GL.PushMatrix();
		GL.LoadPixelMatrix();
		Frame frame = frames[displayFrameIndex];
		if (!capturing && frame != null)
		{
			int childIndex = frame.nodes[0].childIndex;
			if (childIndex != -1)
			{
				DrawNode(frame, childIndex, 0f, Screen.height);
			}
		}
		long num = 0L;
		long num2 = 0L;
		for (int i = 0; i < maxFrames; i++)
		{
			Frame frame2 = frames[i];
			if (frame2 != null)
			{
				if (frame2.ticks > num2)
				{
					num2 = frame2.ticks;
				}
				if (frame2.usedMemory > num)
				{
					num = frame2.usedMemory;
				}
			}
		}
		float num3 = (float)Screen.width / (float)maxFrames;
		float num4 = 2f;
		float num5 = 100f;
		if (num2 > 0)
		{
			for (int j = 0; j < maxFrames; j++)
			{
				int num6 = (j + currentFrameIndex + 1) % maxFrames;
				Frame frame3 = frames[num6];
				if (frame3 != null)
				{
					float x = num3 * (float)j;
					float ySize = (float)((double)(num5 * (float)frame3.ticks) / (double)num2);
					Color color = Color.blue;
					if (frame3 == frame)
					{
						color = Color.yellow;
					}
					else if (frame3.numNodes == 1)
					{
						color = new Color(0f, 0f, 0.25f);
					}
					if (frame3.garbageCollection)
					{
						color = new Color(1f, 0.5f, 0f);
					}
					DrawBar(x, 0f, num3 - num4, ySize, color);
				}
			}
		}
		if (num > 0)
		{
			solidMaterial.SetPass(0);
			GL.Begin(1);
			GL.Color(Color.yellow);
			float x2 = 0f;
			float y = 0f;
			for (int k = 0; k < maxFrames; k++)
			{
				Frame frame4 = frames[(k + currentFrameIndex + 1) % maxFrames];
				if (frame4 != null)
				{
					float num7 = num3 * (float)k;
					float num8 = (float)((double)(num5 * (float)frame4.usedMemory) / (double)num);
					GL.Vertex3(x2, y, 0f);
					GL.Vertex3(num7, num8, 0f);
					x2 = num7;
					y = num8;
				}
			}
			GL.End();
		}
		GL.PopMatrix();
	}

	private void DrawNode(Frame frame, int nodeIndex, float x, float y)
	{
		float num = (float)(font.lineHeight * fontSize) / (float)font.fontSize;
		float num2 = 15f;
		float num3 = TicksToMs(frame.nodes[nodeIndex].ticks);
		float num4 = (float)((double)frame.nodes[nodeIndex].ticks / (double)frame.ticks);
		DrawBar(x, y - num, num4 * (float)Screen.width, num, Color.blue);
		string text = $"{frame.nodes[nodeIndex].name} {num3} ms {frame.nodes[nodeIndex].numCalls} calls ({num4:P2}) {num3 / (float)frame.nodes[nodeIndex].numCalls} ms per call";
		DrawText(x, y - num, text);
		y -= num;
		if (frame.nodes[nodeIndex].childIndex != -1)
		{
			DrawNode(frame, frame.nodes[nodeIndex].childIndex, x + num2, y);
			y -= num;
		}
		if (frame.nodes[nodeIndex].siblingIndex != -1)
		{
			DrawNode(frame, frame.nodes[nodeIndex].siblingIndex, x, y);
			y -= num;
		}
	}

	private void DrawBar(float x, float y, float xSize, float ySize, Color color)
	{
		solidMaterial.SetPass(0);
		GL.Begin(7);
		GL.Color(color);
		GL.Vertex3(x, y, 0f);
		GL.Vertex3(x + xSize, y, 0f);
		GL.Vertex3(x + xSize, y + ySize, 0f);
		GL.Vertex3(x, y + ySize, 0f);
		GL.End();
	}

	private void DrawText(float x, float y, string text)
	{
		font.RequestCharactersInTexture(text, fontSize, fontStyle);
		Vector3 vector = new Vector3(x, y, 0f);
		font.material.SetPass(0);
		GL.Begin(7);
		GL.Color(Color.white);
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
}
