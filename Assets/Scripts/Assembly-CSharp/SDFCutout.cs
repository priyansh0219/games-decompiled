using System;
using System.Collections;
using System.Collections.Generic;
using UWE;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SDFCutout : MonoBehaviour, ICompileTimeCheckable
{
	private const float scoreUpdateInterval = 1f;

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.LateUpdate;

	private const string keywordCutoutEnabled = "FX_CUTOUT_ENABLED";

	private const float distanceNear = 2f;

	private const float distanceFar = 50f;

	private const int scoreNear = 10;

	private const int scoreFar = 0;

	private const int minScore = 5;

	private static bool initialized;

	private static List<SDFCutout> all = new List<SDFCutout>();

	private static Coroutine updateScoresCoroutine;

	private static bool cutoutEnabled;

	public AssetReferenceDistanceField distanceFieldRef;

	private AsyncOperationHandle<DistanceField> request;

	private Texture3D distanceFieldTexture;

	private Vector3 distanceFieldMin;

	private Vector3 distanceFieldMax;

	private Vector3 distanceFieldSizeRcp;

	private Bounds distanceFieldBounds;

	private int score;

	private static void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdate, OnUpdate);
			updateScoresCoroutine = CoroutineHost.StartCoroutine(UpdateScoresAsync());
		}
	}

	private static IEnumerator UpdateScoresAsync()
	{
		WaitForSecondsRealtime wait = new WaitForSecondsRealtime(1f);
		Comparison<SDFCutout> comparer = (SDFCutout a, SDFCutout b) => -a.score.CompareTo(b.score);
		while (true)
		{
			yield return wait;
			for (int num = all.Count - 1; num >= 0; num--)
			{
				SDFCutout sDFCutout = all[num];
				if (sDFCutout == null)
				{
					all.RemoveAt(num);
				}
				else
				{
					sDFCutout.UpdateScore();
				}
			}
			all.Sort(comparer);
		}
	}

	public static void Deinitialize()
	{
		if (initialized)
		{
			initialized = false;
			ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdate, OnUpdate);
			CoroutineHost.StopCoroutine(updateScoresCoroutine);
			updateScoresCoroutine = null;
			SetCutout(null);
		}
	}

	private static void OnUpdate()
	{
		SDFCutout sDFCutout = ((all.Count > 0) ? all[0] : null);
		if (sDFCutout != null && sDFCutout.score < 5)
		{
			sDFCutout = null;
		}
		SetCutout(sDFCutout);
	}

	private static void SetCutout(SDFCutout instance)
	{
		bool flag = false;
		if (instance != null && instance.distanceFieldTexture != null)
		{
			flag = true;
			Shader.SetGlobalMatrix(ShaderPropertyID._Cutout1Matrix, instance.transform.worldToLocalMatrix);
			Shader.SetGlobalTexture(ShaderPropertyID._Cutout1Texture, instance.distanceFieldTexture);
			Shader.SetGlobalVector(ShaderPropertyID._Cutout1Min, instance.distanceFieldMin);
			Shader.SetGlobalVector(ShaderPropertyID._Cutout1SizeRcp, instance.distanceFieldSizeRcp);
		}
		if (cutoutEnabled != flag)
		{
			cutoutEnabled = flag;
			if (cutoutEnabled)
			{
				Shader.EnableKeyword("FX_CUTOUT_ENABLED");
			}
			else
			{
				Shader.DisableKeyword("FX_CUTOUT_ENABLED");
			}
		}
	}

	private IEnumerator Start()
	{
		Initialize();
		request = AddressablesUtility.LoadAsync<DistanceField>(distanceFieldRef.RuntimeKey);
		yield return request;
		DistanceField distanceField = request.Result;
		if (distanceField == null)
		{
			Debug.LogErrorFormat("Couldn't load '{0}'", distanceFieldRef.RuntimeKey);
			distanceField = new DistanceField();
		}
		distanceFieldMin = distanceField.min;
		distanceFieldMax = distanceField.max;
		Vector3 size = distanceFieldMax - distanceFieldMin;
		distanceFieldSizeRcp = new Vector3(1f / size.x, 1f / size.y, 1f / size.z);
		distanceFieldBounds = new Bounds((distanceFieldMin + distanceFieldMax) * 0.5f, size);
		distanceFieldTexture = distanceField.texture;
	}

	private void OnEnable()
	{
		all.Add(this);
	}

	private void OnDisable()
	{
		all.Remove(this);
	}

	private void OnDestroy()
	{
		distanceFieldTexture = null;
		distanceFieldMin = Vector3.zero;
		distanceFieldMax = Vector3.zero;
		distanceFieldSizeRcp = Vector3.zero;
		distanceFieldBounds = default(Bounds);
		if (request.IsValid())
		{
			AddressablesUtility.QueueRelease(ref request);
		}
	}

	private void UpdateScore()
	{
		score = 0;
		if (!(distanceFieldTexture != null))
		{
			return;
		}
		Vehicle componentInParent = GetComponentInParent<Vehicle>();
		if (componentInParent != null)
		{
			score++;
			if (componentInParent.playerFullyEntered)
			{
				score += 1000;
				return;
			}
			if (!componentInParent.docked)
			{
				score++;
			}
			score += GetDistanceScore();
			return;
		}
		SubRoot componentInParent2 = GetComponentInParent<SubRoot>();
		if (componentInParent2 != null && !componentInParent2.subDestroyed)
		{
			score += 2;
			if (componentInParent2.playerInside)
			{
				score += 1000;
			}
			else
			{
				score += GetDistanceScore();
			}
		}
	}

	private int GetDistanceScore()
	{
		Vector3 vector = ((Player.main != null) ? Player.main.transform.position : Vector3.zero);
		float t = ((ClosestPointOnBounds(vector, base.transform.worldToLocalMatrix, base.transform.localToWorldMatrix, distanceFieldBounds) - vector).magnitude - 2f) / 48f;
		return Mathf.RoundToInt(Mathf.Lerp(10f, 0f, t));
	}

	public static Vector3 ClosestPointOnBounds(Vector3 worldPos, Matrix4x4 worldToLocalMatrix, Matrix4x4 localToWorldMatrix, Bounds bounds)
	{
		Vector3 point = worldToLocalMatrix.MultiplyPoint3x4(worldPos);
		Vector3 point2 = bounds.ClosestPoint(point);
		return localToWorldMatrix.MultiplyPoint3x4(point2);
	}

	private static void DrawDebug()
	{
		Dbg.Write("{0}: {1}", "FX_CUTOUT_ENABLED", Shader.IsKeywordEnabled("FX_CUTOUT_ENABLED"));
		for (int i = 0; i < all.Count; i++)
		{
			SDFCutout sDFCutout = all[i];
			PrefabIdentifier componentInParent = sDFCutout.GetComponentInParent<PrefabIdentifier>();
			string text = ((componentInParent != null) ? componentInParent.name : sDFCutout.name);
			Dbg.Write("{0} {1} {2}", sDFCutout.GetInstanceID(), text, sDFCutout.score);
		}
	}

	public string CompileTimeCheck()
	{
		return null;
	}
}
