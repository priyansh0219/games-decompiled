using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class OVRHaptics
{
	public static class Config
	{
		public static int SampleRateHz { get; private set; }

		public static int SampleSizeInBytes { get; private set; }

		public static int MinimumSafeSamplesQueued { get; private set; }

		public static int MinimumBufferSamplesCount { get; private set; }

		public static int OptimalBufferSamplesCount { get; private set; }

		public static int MaximumBufferSamplesCount { get; private set; }

		static Config()
		{
			Load();
		}

		public static void Load()
		{
			OVRPlugin.HapticsDesc controllerHapticsDesc = OVRPlugin.GetControllerHapticsDesc(2u);
			SampleRateHz = controllerHapticsDesc.SampleRateHz;
			SampleSizeInBytes = controllerHapticsDesc.SampleSizeInBytes;
			MinimumSafeSamplesQueued = controllerHapticsDesc.MinimumSafeSamplesQueued;
			MinimumBufferSamplesCount = controllerHapticsDesc.MinimumBufferSamplesCount;
			OptimalBufferSamplesCount = controllerHapticsDesc.OptimalBufferSamplesCount;
			MaximumBufferSamplesCount = controllerHapticsDesc.MaximumBufferSamplesCount;
		}
	}

	public class OVRHapticsChannel
	{
		private OVRHapticsOutput m_output;

		public OVRHapticsChannel(uint outputIndex)
		{
			m_output = m_outputs[outputIndex];
		}

		public void Preempt(OVRHapticsClip clip)
		{
			m_output.Preempt(clip);
		}

		public void Queue(OVRHapticsClip clip)
		{
			m_output.Queue(clip);
		}

		public void Mix(OVRHapticsClip clip)
		{
			m_output.Mix(clip);
		}

		public void Clear()
		{
			m_output.Clear();
		}
	}

	private class OVRHapticsOutput
	{
		private class ClipPlaybackTracker
		{
			public int ReadCount { get; set; }

			public OVRHapticsClip Clip { get; set; }

			public ClipPlaybackTracker(OVRHapticsClip clip)
			{
				Clip = clip;
			}
		}

		private bool m_lowLatencyMode = true;

		private int m_prevSamplesQueued;

		private float m_prevSamplesQueuedTime;

		private int m_numPredictionHits;

		private int m_numPredictionMisses;

		private int m_numUnderruns;

		private List<ClipPlaybackTracker> m_pendingClips = new List<ClipPlaybackTracker>();

		private uint m_controller;

		private OVRNativeBuffer m_nativeBuffer = new OVRNativeBuffer(Config.MaximumBufferSamplesCount * Config.SampleSizeInBytes);

		private OVRHapticsClip m_paddingClip = new OVRHapticsClip();

		public OVRHapticsOutput(uint controller)
		{
			m_controller = controller;
		}

		public void Process()
		{
			OVRPlugin.HapticsState controllerHapticsState = OVRPlugin.GetControllerHapticsState(m_controller);
			float num = Time.realtimeSinceStartup - m_prevSamplesQueuedTime;
			if (m_prevSamplesQueued > 0)
			{
				int num2 = m_prevSamplesQueued - (int)(num * (float)Config.SampleRateHz + 0.5f);
				if (num2 < 0)
				{
					num2 = 0;
				}
				if (controllerHapticsState.SamplesQueued - num2 == 0)
				{
					m_numPredictionHits++;
				}
				else
				{
					m_numPredictionMisses++;
				}
				if (num2 > 0 && controllerHapticsState.SamplesQueued == 0)
				{
					m_numUnderruns++;
				}
				m_prevSamplesQueued = controllerHapticsState.SamplesQueued;
				m_prevSamplesQueuedTime = Time.realtimeSinceStartup;
			}
			int num3 = Config.OptimalBufferSamplesCount;
			if (m_lowLatencyMode)
			{
				float num4 = 1000f / (float)Config.SampleRateHz;
				int num5 = (int)Mathf.Ceil(num * 1000f / num4);
				int num6 = Config.MinimumSafeSamplesQueued + num5;
				if (num6 < num3)
				{
					num3 = num6;
				}
			}
			if (controllerHapticsState.SamplesQueued > num3)
			{
				return;
			}
			if (num3 > Config.MaximumBufferSamplesCount)
			{
				num3 = Config.MaximumBufferSamplesCount;
			}
			if (num3 > controllerHapticsState.SamplesAvailable)
			{
				num3 = controllerHapticsState.SamplesAvailable;
			}
			int num7 = 0;
			int num8 = 0;
			while (num7 < num3 && num8 < m_pendingClips.Count)
			{
				int num9 = num3 - num7;
				int num10 = m_pendingClips[num8].Clip.Count - m_pendingClips[num8].ReadCount;
				if (num9 > num10)
				{
					num9 = num10;
				}
				if (num9 > 0)
				{
					int length = num9 * Config.SampleSizeInBytes;
					int byteOffset = num7 * Config.SampleSizeInBytes;
					int startIndex = m_pendingClips[num8].ReadCount * Config.SampleSizeInBytes;
					Marshal.Copy(m_pendingClips[num8].Clip.Samples, startIndex, m_nativeBuffer.GetPointer(byteOffset), length);
					m_pendingClips[num8].ReadCount += num9;
					num7 += num9;
				}
				num8++;
			}
			int num11 = m_pendingClips.Count - 1;
			while (num11 >= 0 && m_pendingClips.Count > 0)
			{
				if (m_pendingClips[num11].ReadCount >= m_pendingClips[num11].Clip.Count)
				{
					m_pendingClips.RemoveAt(num11);
				}
				num11--;
			}
			int num12 = num3 - (controllerHapticsState.SamplesQueued + num7);
			if (num12 < Config.MinimumBufferSamplesCount - num7)
			{
				num12 = Config.MinimumBufferSamplesCount - num7;
			}
			if (num12 > controllerHapticsState.SamplesAvailable)
			{
				num12 = controllerHapticsState.SamplesAvailable;
			}
			if (num12 > 0)
			{
				int length2 = num12 * Config.SampleSizeInBytes;
				int byteOffset2 = num7 * Config.SampleSizeInBytes;
				int startIndex2 = 0;
				Marshal.Copy(m_paddingClip.Samples, startIndex2, m_nativeBuffer.GetPointer(byteOffset2), length2);
				num7 += num12;
			}
			if (num7 > 0)
			{
				OVRPlugin.HapticsBuffer hapticsBuffer = default(OVRPlugin.HapticsBuffer);
				hapticsBuffer.Samples = m_nativeBuffer.GetPointer();
				hapticsBuffer.SamplesCount = num7;
				OVRPlugin.SetControllerHaptics(m_controller, hapticsBuffer);
				m_prevSamplesQueued = OVRPlugin.GetControllerHapticsState(m_controller).SamplesQueued;
				m_prevSamplesQueuedTime = Time.realtimeSinceStartup;
			}
		}

		public void Preempt(OVRHapticsClip clip)
		{
			m_pendingClips.Clear();
			m_pendingClips.Add(new ClipPlaybackTracker(clip));
		}

		public void Queue(OVRHapticsClip clip)
		{
			m_pendingClips.Add(new ClipPlaybackTracker(clip));
		}

		public void Mix(OVRHapticsClip clip)
		{
			int num = 0;
			int num2 = 0;
			int num3 = clip.Count;
			while (num3 > 0 && num < m_pendingClips.Count)
			{
				int num4 = m_pendingClips[num].Clip.Count - m_pendingClips[num].ReadCount;
				num3 -= num4;
				num2 += num4;
				num++;
			}
			if (num3 > 0)
			{
				num2 += num3;
				num3 = 0;
			}
			if (num > 0)
			{
				OVRHapticsClip oVRHapticsClip = new OVRHapticsClip(num2);
				int i = 0;
				for (int j = 0; j < num; j++)
				{
					OVRHapticsClip clip2 = m_pendingClips[j].Clip;
					for (int k = m_pendingClips[j].ReadCount; k < clip2.Count; k++)
					{
						if (Config.SampleSizeInBytes == 1)
						{
							byte sample = 0;
							if (i < clip.Count && k < clip2.Count)
							{
								sample = (byte)Mathf.Clamp(clip.Samples[i] + clip2.Samples[k], 0, 255);
								i++;
							}
							else if (k < clip2.Count)
							{
								sample = clip2.Samples[k];
							}
							oVRHapticsClip.WriteSample(sample);
						}
					}
				}
				for (; i < clip.Count; i++)
				{
					if (Config.SampleSizeInBytes == 1)
					{
						oVRHapticsClip.WriteSample(clip.Samples[i]);
					}
				}
				m_pendingClips[0] = new ClipPlaybackTracker(oVRHapticsClip);
				for (int l = 1; l < num; l++)
				{
					m_pendingClips.RemoveAt(1);
				}
			}
			else
			{
				m_pendingClips.Add(new ClipPlaybackTracker(clip));
			}
		}

		public void Clear()
		{
			m_pendingClips.Clear();
		}
	}

	public static readonly OVRHapticsChannel[] Channels;

	public static readonly OVRHapticsChannel LeftChannel;

	public static readonly OVRHapticsChannel RightChannel;

	private static readonly OVRHapticsOutput[] m_outputs;

	static OVRHaptics()
	{
		Config.Load();
		m_outputs = new OVRHapticsOutput[2]
		{
			new OVRHapticsOutput(1u),
			new OVRHapticsOutput(2u)
		};
		Channels = new OVRHapticsChannel[2]
		{
			LeftChannel = new OVRHapticsChannel(0u),
			RightChannel = new OVRHapticsChannel(1u)
		};
	}

	public static void Process()
	{
		Config.Load();
		for (int i = 0; i < m_outputs.Length; i++)
		{
			m_outputs[i].Process();
		}
	}
}
