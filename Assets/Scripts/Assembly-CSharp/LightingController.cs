using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using mset;

public class LightingController : MonoBehaviour
{
	public class Timer
	{
		private float _elapsed;

		private float _timeToReach;

		private bool _isFinished;

		private bool _isStarted;

		public void Start(float timeToReach)
		{
			_elapsed = 0f;
			_timeToReach = timeToReach;
			_isStarted = true;
			_isFinished = false;
		}

		public void Stop()
		{
			_isStarted = false;
		}

		public void Resume()
		{
			if (!_isFinished)
			{
				_isStarted = true;
			}
		}

		public void Update(float deltaTime)
		{
			if (_isStarted && !_isFinished)
			{
				_elapsed += deltaTime;
			}
			if (_elapsed >= _timeToReach)
			{
				_isFinished = true;
			}
		}

		public bool IsStarted()
		{
			return _isStarted;
		}

		public bool IsFinished()
		{
			return _isFinished;
		}

		public float GetFraction()
		{
			return Mathf.Clamp01(_elapsed / _timeToReach);
		}
	}

	[Serializable]
	public class MultiStatesSky
	{
		public Sky sky;

		public float[] masterIntensities = new float[3] { 1f, 1f, 1f };

		public float[] diffIntensities = new float[3] { 1f, 1f, 1f };

		public float[] specIntensities = new float[3] { 1f, 1f, 1f };

		private float startMasterIntensity = 1f;

		private float startDiffuseIntensity = 1f;

		private float startSpecIntensity = 1f;

		public void InitLerp()
		{
			if (sky != null)
			{
				startMasterIntensity = sky.MasterIntensity;
				startDiffuseIntensity = sky.DiffIntensity;
				startSpecIntensity = sky.SpecIntensity;
			}
		}

		public void UpdateIntensity(int targetState, float scalar)
		{
			if (sky != null)
			{
				if (targetState < masterIntensities.Length)
				{
					sky.MasterIntensity = Mathf.Lerp(startMasterIntensity, masterIntensities[targetState], scalar);
				}
				if (targetState < diffIntensities.Length)
				{
					sky.DiffIntensity = Mathf.Lerp(startDiffuseIntensity, diffIntensities[targetState], scalar);
				}
				if (targetState < specIntensities.Length)
				{
					sky.SpecIntensity = Mathf.Lerp(startSpecIntensity, specIntensities[targetState], scalar);
				}
			}
		}

		public void SetIntensity(int targetState)
		{
			if (sky != null)
			{
				if (targetState < masterIntensities.Length)
				{
					sky.MasterIntensity = masterIntensities[targetState];
				}
				if (targetState < diffIntensities.Length)
				{
					sky.DiffIntensity = diffIntensities[targetState];
				}
				if (targetState < specIntensities.Length)
				{
					sky.SpecIntensity = specIntensities[targetState];
				}
			}
		}
	}

	[Serializable]
	public class MultiStatesEmissive
	{
		public float[] intensities = new float[3] { 0f, 1f, 1f };

		private HashSet<Renderer> renderers = new HashSet<Renderer>();

		private MaterialPropertyBlock block;

		private float startIntensity;

		private float currentIntensity;

		private void ApplyCurrentIntensity()
		{
			if (block == null)
			{
				block = new MaterialPropertyBlock();
			}
			HashSet<Renderer>.Enumerator enumerator = renderers.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Renderer current = enumerator.Current;
				if (!(current == null))
				{
					block.Clear();
					current.GetPropertyBlock(block);
					block.SetFloat(ShaderPropertyID._UwePowerLoss, Mathf.Clamp01(1f - currentIntensity));
					current.SetPropertyBlock(block);
				}
			}
		}

		public void InitLerp()
		{
			if (renderers != null)
			{
				startIntensity = currentIntensity;
			}
		}

		public void UpdateIntensity(int targetState, float scalar)
		{
			if (targetState < intensities.Length)
			{
				currentIntensity = Mathf.Lerp(startIntensity, intensities[targetState], scalar);
				ApplyCurrentIntensity();
			}
		}

		public void SetIntensity(int targetState)
		{
			if (targetState < intensities.Length)
			{
				currentIntensity = intensities[targetState];
				ApplyCurrentIntensity();
			}
		}

		public void RegisterRenderer(Renderer rend)
		{
			renderers.Add(rend);
		}

		public void RegisterRenderers(Renderer[] rends)
		{
			foreach (Renderer rend in rends)
			{
				RegisterRenderer(rend);
			}
		}

		public void UnregisterRenderer(Renderer rend)
		{
			renderers.Remove(rend);
		}

		public void UnregisterRenderers(Renderer[] rends)
		{
			foreach (Renderer rend in rends)
			{
				UnregisterRenderer(rend);
			}
		}
	}

	public enum LightingState
	{
		Operational = 0,
		Danger = 1,
		Damaged = 2
	}

	public LightingState state;

	public float fadeDuration = 1f;

	public MultiStatesSky[] skies = new MultiStatesSky[1];

	public MultiStatesLight[] lights;

	public MultiStatesEmissive emissiveController = new MultiStatesEmissive();

	private int prevState = -1;

	private Timer timer = new Timer();

	private bool lerpFinalized;

	private bool appliersToRefresh = true;

	public void SnapToState(int targetState)
	{
		timer.Stop();
		state = (LightingState)targetState;
		if (skies != null)
		{
			for (int i = 0; i < skies.Length; i++)
			{
				skies[i].SetIntensity(targetState);
			}
		}
		if (lights != null)
		{
			for (int j = 0; j < lights.Length; j++)
			{
				lights[j].SetIntensity(targetState);
			}
		}
		if (emissiveController != null)
		{
			emissiveController.SetIntensity(targetState);
		}
		appliersToRefresh = false;
	}

	public void SnapToState()
	{
		SnapToState((int)state);
	}

	public void LerpToState(int targetState, float inSeconds)
	{
		timer.Stop();
		if (skies != null)
		{
			for (int i = 0; i < skies.Length; i++)
			{
				skies[i].InitLerp();
			}
		}
		if (lights != null)
		{
			for (int j = 0; j < lights.Length; j++)
			{
				lights[j].InitLerp();
			}
		}
		if (emissiveController != null)
		{
			emissiveController.InitLerp();
		}
		state = (LightingState)targetState;
		timer.Start(inSeconds);
		lerpFinalized = false;
	}

	public void LerpToState(int targetState)
	{
		LerpToState(targetState, fadeDuration);
	}

	private void UpdateIntensities()
	{
		if (timer.IsStarted() && !timer.IsFinished())
		{
			int targetState = (int)state;
			if (skies != null)
			{
				for (int i = 0; i < skies.Length; i++)
				{
					skies[i].UpdateIntensity(targetState, timer.GetFraction());
				}
			}
			if (lights != null)
			{
				for (int j = 0; j < lights.Length; j++)
				{
					lights[j].UpdateIntensity(targetState, timer.GetFraction());
				}
			}
			if (emissiveController != null)
			{
				emissiveController.UpdateIntensity(targetState, timer.GetFraction());
			}
		}
		else if (!lerpFinalized || appliersToRefresh)
		{
			SnapToState();
			lerpFinalized = true;
		}
		appliersToRefresh = false;
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		if (!(deltaTime <= 0f))
		{
			timer.Update(deltaTime);
			int num = (int)state;
			if (prevState != num)
			{
				LerpToState(num);
				prevState = num;
			}
			UpdateIntensities();
		}
	}

	public void RegisterLight(MultiStatesLight lightToRegister)
	{
		HashSet<MultiStatesLight> hashSet = new HashSet<MultiStatesLight>(lights);
		hashSet.Add(lightToRegister);
		lights = hashSet.ToArray();
	}

	public void RegisterLights(MultiStatesLight[] lightsToRegister)
	{
		List<MultiStatesLight> list = new List<MultiStatesLight>(lights);
		list.AddRange(lightsToRegister);
		lights = Enumerable.ToArray(list);
	}

	public void UnregisterLight(MultiStatesLight lightToUnregister)
	{
		HashSet<MultiStatesLight> hashSet = new HashSet<MultiStatesLight>(lights);
		hashSet.Remove(lightToUnregister);
		lights = hashSet.ToArray();
	}

	public void UnregisterLights(MultiStatesLight[] lightsToUnregister)
	{
		HashSet<MultiStatesLight> hashSet = new HashSet<MultiStatesLight>(lights);
		foreach (MultiStatesLight item in lightsToUnregister)
		{
			hashSet.Remove(item);
		}
		lights = hashSet.ToArray();
	}

	public void RegisterSkyApplier(SkyApplier app)
	{
		appliersToRefresh = true;
		emissiveController.RegisterRenderers(app.renderers);
	}

	public void UnregisterSkyApplier(SkyApplier app)
	{
		emissiveController.UnregisterRenderers(app.renderers);
	}
}
