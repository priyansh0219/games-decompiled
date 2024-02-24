using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using WorldStreaming;

public class FPSCounter : MonoBehaviour
{
	[SerializeField]
	[AssertNotNull]
	private TextMeshProUGUI text;

	private readonly StringBuilder strBuffer = new StringBuilder(2048);

	private float timeNextSample = -1f;

	private float timeNextUpdate = -1f;

	private long lastTotalMem;

	private long diffTotalMem;

	private float accumulatedFrameTime;

	private int numAccumulatedFrames;

	private int lastCollectionCount1;

	private int lastCollectionCount2;

	private float lastCollectionTime;

	private float timeBetweenCollections;

	private float avgFrameTime;

	private int numCollections;

	private int numFixedUpdates;

	private int numUpdates;

	private float avgFixedUpdatesPerFrame;

	private void OnConsoleCommand_fps()
	{
		base.enabled = !base.enabled;
	}

	private void Start()
	{
		DevConsole.RegisterConsoleCommand(this, "fps");
		base.enabled = false;
	}

	private void OnEnable()
	{
		text.enabled = true;
	}

	private void OnDisable()
	{
		text.enabled = false;
	}

	private void Update()
	{
		numAccumulatedFrames++;
		numUpdates++;
		accumulatedFrameTime += Time.unscaledDeltaTime;
		bool flag = false;
		if (Time.unscaledTime > timeNextSample)
		{
			SampleTotalMemory();
			timeNextSample = Time.unscaledTime + 1f;
			flag = true;
		}
		if (Time.unscaledTime > timeNextUpdate)
		{
			SampleFrameRate();
			flag = true;
			timeNextUpdate = Time.unscaledTime + 0.1f;
			if (numUpdates > 0)
			{
				avgFixedUpdatesPerFrame = (float)numFixedUpdates / (float)numUpdates;
				numUpdates = 0;
				numFixedUpdates = 0;
			}
		}
		int num = GC.CollectionCount(1);
		int num2 = GC.CollectionCount(2);
		if (num2 > lastCollectionCount2 || num > lastCollectionCount1)
		{
			float unscaledTime = Time.unscaledTime;
			timeBetweenCollections = unscaledTime - lastCollectionTime;
			lastCollectionTime = unscaledTime;
			numCollections++;
			flag = true;
		}
		lastCollectionCount1 = num;
		lastCollectionCount2 = num2;
		if (flag)
		{
			UpdateDisplay();
		}
	}

	private void FixedUpdate()
	{
		numFixedUpdates++;
	}

	private void SampleTotalMemory()
	{
		long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
		diffTotalMem = totalMemory - lastTotalMem;
		lastTotalMem = totalMemory;
	}

	private void SampleFrameRate()
	{
		avgFrameTime = accumulatedFrameTime / (float)numAccumulatedFrames;
		numAccumulatedFrames = 0;
		accumulatedFrameTime = 0f;
	}

	private void UpdateDisplay()
	{
		float x = (float)lastTotalMem * 9.536743E-07f;
		float x2 = (float)diffTotalMem * 9.536743E-07f;
		float x3 = (float)BatchOctreesAllocator.octreePool.EstimateBytes() * 9.536743E-07f;
		int num = Mathf.CeilToInt(ScalableBufferManager.widthScaleFactor * (float)Screen.width);
		int num2 = Mathf.CeilToInt(ScalableBufferManager.heightScaleFactor * (float)Screen.height);
		strBuffer.Clear();
		strBuffer.Append(IntStringCache.GetStringForInt((int)(1f / avgFrameTime))).Append(" FPS ");
		strBuffer.Append(IntStringCache.GetStringForInt((int)(avgFrameTime * 1000f))).AppendLine("ms");
		long totalUnusedReservedMemoryLong = Profiler.GetTotalUnusedReservedMemoryLong();
		strBuffer.Append((float)totalUnusedReservedMemoryLong * 9.536743E-07f).AppendLine(" MiB UnusedMemory");
		strBuffer.Append("World Streaming: ").Append(x3.ToTwoDecimalString()).AppendLine(" MB");
		strBuffer.Append("GC: ").Append(x.ToTwoDecimalString()).AppendLine(" MB");
		strBuffer.Append("+").Append(x2.ToTwoDecimalString()).AppendLine(" MB/s");
		strBuffer.Append(IntStringCache.GetStringForInt((int)timeBetweenCollections)).Append("s between GC (").Append(IntStringCache.GetStringForInt(numCollections))
			.AppendLine(")");
		strBuffer.Append(avgFixedUpdatesPerFrame.ToTwoDecimalString()).AppendLine(" FixedUpdates per frame");
		strBuffer.AppendFormat("{0} x {1} Scaled Resolution", num, num2).AppendLine();
		text.SetText(strBuffer);
	}
}
