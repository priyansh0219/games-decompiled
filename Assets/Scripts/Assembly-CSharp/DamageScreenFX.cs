using System;
using FMOD.Studio;
using UnityEngine;

[ExecuteInEditMode]
public class DamageScreenFX : MonoBehaviour
{
	[Serializable]
	public class TextureSlot
	{
		public string texName = "";

		public Texture2D tex;
	}

	[Serializable]
	public class ScreenFXOverlay
	{
		public DamageType damageType;

		public float fadeInDuration;

		public float holdDuration;

		public float fadeOutDuration = 0.5f;

		public FMODAsset sound;

		public float amount;

		public float targetAmount;

		public string shaderKeyword;

		public bool isEnabled;

		private int amountShaderID;

		private float animTime;

		private int state = -1;

		private EventInstance soundInstance;

		public void Init(Material mat, bool debug)
		{
			string text = damageType.ToString();
			shaderKeyword = "FX_" + text.ToUpper() + "_DMG";
			string text2 = "_" + text + "Amount";
			amountShaderID = Shader.PropertyToID(text2);
			if (debug)
			{
				Debug.Log("ShaderKeyword: " + shaderKeyword + "  -  " + text2 + "   ID: " + amountShaderID);
			}
		}

		public void Clear()
		{
			state = -1;
			amount = 0f;
			targetAmount = 0f;
			animTime = 0f;
			StopSound(STOP_MODE.IMMEDIATE);
		}

		public void IncrementTargetAmount(float scalar)
		{
			targetAmount = Mathf.Clamp01(targetAmount + scalar);
			state = 0;
			if (!soundInstance.isValid())
			{
				StartSound();
			}
		}

		public void UpdateAmount(Material mat, bool debug)
		{
			if (debug)
			{
				state = 1;
			}
			else
			{
				if (state == 0 && GetSafeDuration(fadeInDuration) <= 0f)
				{
					amount = targetAmount;
					animTime = 0f;
					state = 1;
				}
				if (state == 1 && holdDuration <= 0f)
				{
					state = 2;
					StopSound(STOP_MODE.ALLOWFADEOUT);
				}
				if (state == 2 && GetSafeDuration(fadeOutDuration) <= 0f)
				{
					state = -1;
					animTime = 0f;
					amount = 0f;
					targetAmount = 0f;
				}
			}
			switch (state)
			{
			case 0:
				amount += targetAmount * (Time.deltaTime / GetSafeDuration(fadeInDuration));
				if (amount >= targetAmount)
				{
					amount = targetAmount;
					animTime = 0f;
					state = 1;
				}
				break;
			case 1:
				if (debug)
				{
					amount = targetAmount;
					break;
				}
				animTime += Time.deltaTime / holdDuration;
				if (animTime >= 1f)
				{
					animTime = 0f;
					state = 2;
					StopSound(STOP_MODE.ALLOWFADEOUT);
				}
				break;
			case 2:
				amount -= targetAmount * (Time.deltaTime / GetSafeDuration(fadeOutDuration));
				if (amount <= 0f)
				{
					state = -1;
					amount = 0f;
					targetAmount = 0f;
				}
				break;
			}
			if (amount > 0f)
			{
				mat.SetFloat(amountShaderID, Mathf.Clamp01(amount));
				if (!isEnabled)
				{
					mat.EnableKeyword(shaderKeyword);
					isEnabled = true;
				}
			}
			else if (isEnabled)
			{
				mat.DisableKeyword(shaderKeyword);
				isEnabled = false;
				StopSound(STOP_MODE.ALLOWFADEOUT);
			}
		}

		private float GetSafeDuration(float duration)
		{
			if (MiscSettings.flashes)
			{
				return duration;
			}
			if (Mathf.Approximately(duration, 0f))
			{
				return 1f;
			}
			return duration * 3f;
		}

		private void StartSound()
		{
			if ((bool)sound)
			{
				soundInstance = FMODUWE.GetEvent(sound);
				if (soundInstance.isValid())
				{
					soundInstance.start();
				}
			}
		}

		private void StopSound(STOP_MODE stopMode)
		{
			if (soundInstance.isValid())
			{
				soundInstance.stop(stopMode);
				soundInstance.release();
				soundInstance.clearHandle();
			}
		}
	}

	public bool debug;

	public Shader shader;

	public TextureSlot[] textures;

	public ScreenFXOverlay[] overlays;

	private Material mat;

	private void Start()
	{
		InitMaterial();
	}

	private void OnEnable()
	{
		if (debug)
		{
			InitMaterial();
		}
	}

	private void InitMaterial()
	{
		mat = new Material(shader);
		mat.hideFlags = HideFlags.HideAndDontSave;
		Resolution resolution = DisplayManager.GetResolution();
		float value = (float)resolution.width / (float)resolution.height;
		mat.SetFloat(ShaderPropertyID._AspectRatio, value);
		for (int i = 0; i < textures.Length; i++)
		{
			int nameID = Shader.PropertyToID(textures[i].texName);
			mat.SetTexture(nameID, textures[i].tex);
		}
		for (int j = 0; j < overlays.Length; j++)
		{
			overlays[j].Init(mat, debug);
		}
	}

	private void UpdateMaterial()
	{
		if (MiscSettings.flashes)
		{
			mat.SetFloat(ShaderPropertyID._TimeScale, 1f);
		}
		else
		{
			mat.SetFloat(ShaderPropertyID._TimeScale, 0.01f);
		}
		for (int i = 0; i < overlays.Length; i++)
		{
			overlays[i].UpdateAmount(mat, debug);
		}
	}

	public void Play(DamageInfo damageInfo)
	{
		if (!base.enabled)
		{
			base.enabled = true;
		}
		for (int i = 0; i < overlays.Length; i++)
		{
			if (damageInfo.type == overlays[i].damageType)
			{
				overlays[i].IncrementTargetAmount(1f);
				return;
			}
		}
		overlays[0].IncrementTargetAmount(1f);
	}

	public void ClearAll()
	{
		for (int i = 0; i < overlays.Length; i++)
		{
			overlays[i].Clear();
		}
	}

	private void DisableWhenUnused()
	{
		bool flag = false;
		for (int i = 0; i < overlays.Length; i++)
		{
			if (overlays[i].isEnabled)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			base.enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!mat)
		{
			InitMaterial();
		}
		UpdateMaterial();
		if (!debug)
		{
			DisableWhenUnused();
		}
		Graphics.Blit(source, destination, mat);
	}
}
