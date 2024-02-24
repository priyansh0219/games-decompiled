using System;
using System.Collections;
using UWE;
using UnityEngine;

namespace WorldStreaming
{
	public class WorldStreamerTuner : MonoBehaviour
	{
		[AssertNotNull]
		[SerializeField]
		private WorldStreamer streamer;

		[SerializeField]
		private float smoothTime;

		[SerializeField]
		private float maxSpeed;

		[SerializeField]
		private float octreesFactor = 5f;

		[SerializeField]
		private float clipsFactor = 20f;

		[NonSerialized]
		private float octreesPrior;

		[NonSerialized]
		private float octreesSmooth;

		[NonSerialized]
		private float octreesVel;

		[NonSerialized]
		private float clipsPrior;

		[NonSerialized]
		private float clipsSmooth;

		[NonSerialized]
		private float clipsVel;

		[NonSerialized]
		private float cellsPrior;

		[NonSerialized]
		private float cellsSmooth;

		[NonSerialized]
		private float cellsVel;

		[NonSerialized]
		private float buildPrior;

		[NonSerialized]
		private float buildSmooth;

		[NonSerialized]
		private float buildVel;

		[NonSerialized]
		private float destroyPrior;

		[NonSerialized]
		private float destroySmooth;

		[NonSerialized]
		private float destroyVel;

		[NonSerialized]
		private float playerSpeed;

		private IEnumerator Start()
		{
			Vector3 lastPosition = Vector3.zero;
			WaitForSeconds wait = new WaitForSeconds(1f);
			while (true)
			{
				Player main = Player.main;
				if ((bool)main)
				{
					Vector3 position = main.transform.position;
					playerSpeed = (position - lastPosition).magnitude;
					lastPosition = position;
				}
				yield return wait;
			}
		}

		private void Update()
		{
			float deltaTime = Time.deltaTime;
			float num = Mathf.Max(playerSpeed, 1f);
			octreesPrior = num * octreesFactor;
			clipsPrior = num * clipsFactor;
			int queueLength = GetQueueLength(streamer.octreesStreamer);
			octreesSmooth = Mathf.SmoothDamp(octreesSmooth, queueLength, ref octreesVel, smoothTime, maxSpeed, deltaTime);
			streamer.numOctreesPerSecond = (int)(octreesPrior + octreesSmooth);
			int queueLength2 = GetQueueLength(streamer.clipmapStreamer);
			clipsSmooth = Mathf.SmoothDamp(clipsSmooth, queueLength2, ref clipsVel, smoothTime, maxSpeed, deltaTime);
			int num2 = (int)(clipsPrior + clipsSmooth);
			cellsPrior = num2 * 3 / 4;
			buildPrior = num2 / 2;
			destroyPrior = num2 / 2;
			streamer.numClipsPerSecond = num2;
			int queueLength3 = GetQueueLength(streamer.visibilityUpdater);
			cellsSmooth = Mathf.SmoothDamp(cellsSmooth, queueLength3, ref cellsVel, smoothTime, maxSpeed, deltaTime);
			streamer.numCellsPerSecond = (int)(cellsPrior + cellsSmooth);
			int queueLength4 = GetQueueLength(streamer.GetBuildLayersThread());
			buildSmooth = Mathf.SmoothDamp(buildSmooth, queueLength4, ref buildVel, smoothTime, maxSpeed, deltaTime);
			streamer.numBuildLayersPerSecond = (int)(buildPrior + buildSmooth);
			int queueLength5 = GetQueueLength(streamer.GetDestroyChunksThread());
			destroySmooth = Mathf.SmoothDamp(destroySmooth, queueLength5, ref destroyVel, smoothTime, maxSpeed, deltaTime);
			streamer.numDestroyChunksPerSecond = (int)(destroyPrior + destroySmooth);
		}

		private static int GetQueueLength(IThread thread)
		{
			return thread?.GetQueueLength() ?? 0;
		}

		private static int GetQueueLength(IPipeline pipeline)
		{
			return pipeline?.GetQueueLength() ?? 0;
		}

		public void DebugGUI()
		{
			GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.BeginHorizontal();
			Label("Streamer");
			Label("Prior");
			Label("Smooth");
			Label("Velocity");
			GUILayout.EndHorizontal();
			Row("Octrees", octreesPrior, octreesSmooth, octreesVel);
			Row("Clips", clipsPrior, clipsSmooth, clipsVel);
			Row("Cells", cellsPrior, cellsSmooth, cellsVel);
			Row("Build", buildPrior, buildSmooth, buildVel);
			Row("Destroy", destroyPrior, destroySmooth, destroyVel);
			GUILayout.EndVertical();
		}

		private static void Row(string label, float prior, float smooth, float vel)
		{
			GUILayout.BeginHorizontal();
			Label(label);
			Field(prior);
			Field(smooth);
			Field(vel);
			GUILayout.EndHorizontal();
		}

		private static void Label(string label)
		{
			GUILayout.Label(label, GUILayout.Width(60f));
		}

		private static void Field(float value)
		{
			GUILayout.TextField(value.ToString(), GUILayout.Width(60f));
		}
	}
}
