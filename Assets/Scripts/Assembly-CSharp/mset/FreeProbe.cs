using System;
using System.Collections.Generic;
using UnityEngine;

namespace mset
{
	[RequireComponent(typeof(Camera))]
	public class FreeProbe : MonoBehaviour
	{
		private enum Stage
		{
			NEXTSKY = 0,
			PRECAPTURE = 1,
			CAPTURE = 2,
			CONVOLVE = 3,
			DONE = 4
		}

		private class ProbeTarget
		{
			public Cubemap cube;

			public bool HDR;

			public Vector3 position = Vector3.zero;

			public Quaternion rotation = Quaternion.identity;
		}

		private RenderTexture RT;

		public Action<float> ProgressCallback;

		public Action DoneCallback;

		public bool linear = true;

		public int maxExponent = 512;

		public Vector4 exposures = Vector4.one;

		public float convolutionScale = 1f;

		private List<Camera> disabledCameras = new List<Camera>();

		private Cubemap _targetCube;

		private Texture2D faceTexture;

		private Stage stage = Stage.DONE;

		private int drawShot;

		private int targetMip;

		private int mipCount;

		private int captureSize;

		private bool captureHDR = true;

		private int progress;

		private int progressTotal;

		private Vector3 lookPos = Vector3.zero;

		private Quaternion lookRot = Quaternion.identity;

		private Vector3 forwardLook = Vector3.forward;

		private Vector3 rightLook = Vector3.right;

		private Vector3 upLook = Vector3.up;

		private Queue<ProbeTarget> probeQueue;

		private int defaultCullMask = -1;

		private Material sceneSkybox;

		private Material convolveSkybox;

		private int frameID;

		private Material blitMat;

		private Cubemap targetCube
		{
			get
			{
				return _targetCube;
			}
			set
			{
				_targetCube = value;
				UpdateFaceTexture();
			}
		}

		private void UpdateFaceTexture()
		{
			if (!(_targetCube == null) && (faceTexture == null || faceTexture.width != _targetCube.width))
			{
				if ((bool)faceTexture)
				{
					UnityEngine.Object.DestroyImmediate(faceTexture);
				}
				faceTexture = new Texture2D(_targetCube.width, _targetCube.width, TextureFormat.ARGB32, mipChain: true, linear: false);
				RT = RenderTexture.GetTemporary(_targetCube.width, _targetCube.width, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
				RT.isCubemap = false;
				RT.useMipMap = false;
				RT.autoGenerateMips = false;
				if (!RT.IsCreated() && !RT.Create())
				{
					Debug.LogWarning("Failed to create HDR RenderTexture, capturing in LDR mode.");
					RenderTexture.ReleaseTemporary(RT);
					RT = null;
				}
			}
		}

		private void FreeFaceTexture()
		{
			if ((bool)faceTexture)
			{
				UnityEngine.Object.DestroyImmediate(faceTexture);
				faceTexture = null;
			}
			if ((bool)RT)
			{
				if (RenderTexture.active == RT)
				{
					RenderTexture.active = null;
				}
				RenderTexture.ReleaseTemporary(RT);
				RT = null;
			}
			probeQueue = null;
		}

		private void Start()
		{
			UpdateFaceTexture();
			convolveSkybox = new Material(Shader.Find("Hidden/Marmoset/RGBM Convolve"));
			convolveSkybox.name = "Internal Convolution Skybox";
		}

		private void Awake()
		{
			sceneSkybox = RenderSettings.skybox;
			SkyManager skyManager = SkyManager.Get();
			if ((bool)skyManager && (bool)skyManager.ProbeCamera)
			{
				GetComponent<Camera>().CopyFrom(skyManager.ProbeCamera);
			}
			else if ((bool)MainCamera.camera)
			{
				GetComponent<Camera>().CopyFrom(MainCamera.camera);
			}
		}

		public void QueueSkies(Sky[] skiesToProbe)
		{
			if (probeQueue == null)
			{
				probeQueue = new Queue<ProbeTarget>();
			}
			else
			{
				probeQueue.Clear();
			}
			foreach (Sky sky in skiesToProbe)
			{
				if (sky != null && sky.SpecularCube as Cubemap != null)
				{
					QueueCubemap(sky.SpecularCube as Cubemap, sky.HDRSpec, sky.transform.position, sky.transform.rotation);
				}
			}
		}

		public void QueueCubemap(Cubemap cube, bool HDR, Vector3 pos, Quaternion rot)
		{
			if (cube != null)
			{
				ProbeTarget probeTarget = new ProbeTarget();
				probeTarget.cube = cube;
				probeTarget.position = pos;
				probeTarget.rotation = rot;
				probeTarget.HDR = HDR;
				probeQueue.Enqueue(probeTarget);
				progressTotal++;
			}
		}

		private void ClearQueue()
		{
			probeQueue = null;
			progressTotal = 0;
			progress = 0;
		}

		public void RunQueue()
		{
			probeQueue.Enqueue(null);
			SkyProbe.buildRandomValueTable();
			SkyManager skyManager = SkyManager.Get();
			if ((bool)skyManager.ProbeCamera)
			{
				GetComponent<Camera>().CopyFrom(skyManager.ProbeCamera);
				defaultCullMask = skyManager.ProbeCamera.cullingMask;
			}
			else if ((bool)MainCamera.camera)
			{
				GetComponent<Camera>().CopyFrom(MainCamera.camera);
				defaultCullMask = GetComponent<Camera>().cullingMask;
			}
			disabledCameras.Clear();
			Camera[] allCameras = Camera.allCameras;
			foreach (Camera camera in allCameras)
			{
				if (camera.enabled)
				{
					camera.enabled = false;
					disabledCameras.Add(camera);
				}
			}
			GetComponent<Camera>().enabled = true;
			GetComponent<Camera>().fieldOfView = 90f;
			GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
			GetComponent<Camera>().cullingMask = defaultCullMask;
			GetComponent<Camera>().useOcclusionCulling = false;
			StartStage(Stage.NEXTSKY);
		}

		private void StartStage(Stage nextStage)
		{
			if (probeQueue == null)
			{
				nextStage = Stage.DONE;
			}
			if (nextStage == Stage.NEXTSKY)
			{
				RenderSettings.skybox = sceneSkybox;
				ProbeTarget probeTarget = probeQueue.Dequeue();
				if (probeTarget != null)
				{
					progress++;
					if (ProgressCallback != null && progressTotal > 0)
					{
						ProgressCallback((float)progress / (float)progressTotal);
					}
					targetCube = probeTarget.cube;
					captureHDR = probeTarget.HDR && RT != null;
					lookPos = probeTarget.position;
					lookRot = probeTarget.rotation;
				}
				else
				{
					nextStage = Stage.DONE;
				}
			}
			if (nextStage == Stage.CAPTURE)
			{
				drawShot = -1;
				RenderSettings.skybox = sceneSkybox;
				targetMip = 0;
				captureSize = targetCube.width;
				mipCount = QPow.Log2i(captureSize) - 1;
				GetComponent<Camera>().cullingMask = defaultCullMask;
			}
			if (nextStage == Stage.CONVOLVE)
			{
				Shader.SetGlobalVector(ShaderPropertyID._UniformOcclusion, Vector4.one);
				drawShot = 0;
				targetMip = 1;
				if (targetMip < mipCount)
				{
					GetComponent<Camera>().cullingMask = 0;
					RenderSettings.skybox = convolveSkybox;
					Matrix4x4 identity = Matrix4x4.identity;
					convolveSkybox.SetMatrix(ShaderPropertyID._SkyMatrix, identity);
					convolveSkybox.SetTexture(ShaderPropertyID._CubeHDR, targetCube);
					toggleKeyword("MARMO_RGBM_INPUT_ON", captureHDR && RT != null);
					toggleKeyword("MARMO_RGBM_OUTPUT_ON", captureHDR && RT != null);
					SkyProbe.bindRandomValueTable(convolveSkybox, "_PhongRands", targetCube.width);
				}
			}
			if (nextStage == Stage.DONE)
			{
				RenderSettings.skybox = sceneSkybox;
				ClearQueue();
				FreeFaceTexture();
				foreach (Camera disabledCamera in disabledCameras)
				{
					disabledCamera.enabled = true;
				}
				disabledCameras.Clear();
				if (DoneCallback != null)
				{
					DoneCallback();
					DoneCallback = null;
				}
			}
			stage = nextStage;
		}

		private void OnPreCull()
		{
			if (stage == Stage.CAPTURE || stage == Stage.CONVOLVE || stage == Stage.PRECAPTURE)
			{
				if (stage == Stage.CONVOLVE)
				{
					captureSize = 1 << mipCount - targetMip;
					float value = QPow.clampedDownShift(maxExponent, targetMip - 1, 1);
					convolveSkybox.SetFloat(ShaderPropertyID._SpecularExp, value);
					convolveSkybox.SetFloat(ShaderPropertyID._SpecularScale, convolutionScale);
				}
				if (stage == Stage.CAPTURE || stage == Stage.PRECAPTURE)
				{
					Shader.SetGlobalVector(ShaderPropertyID._UniformOcclusion, exposures);
				}
				int num = captureSize;
				float width = (float)num / (float)Screen.width;
				float height = (float)num / (float)Screen.height;
				GetComponent<Camera>().rect = new Rect(0f, 0f, width, height);
				GetComponent<Camera>().pixelRect = new Rect(0f, 0f, num, num);
				base.transform.position = lookPos;
				base.transform.rotation = lookRot;
				if (stage == Stage.CAPTURE || stage == Stage.PRECAPTURE)
				{
					upLook = base.transform.up;
					forwardLook = base.transform.forward;
					rightLook = base.transform.right;
				}
				else
				{
					upLook = Vector3.up;
					forwardLook = Vector3.forward;
					rightLook = Vector3.right;
				}
				if (drawShot == 0)
				{
					base.transform.LookAt(lookPos + forwardLook, upLook);
				}
				else if (drawShot == 1)
				{
					base.transform.LookAt(lookPos - forwardLook, upLook);
				}
				else if (drawShot == 2)
				{
					base.transform.LookAt(lookPos - rightLook, upLook);
				}
				else if (drawShot == 3)
				{
					base.transform.LookAt(lookPos + rightLook, upLook);
				}
				else if (drawShot == 4)
				{
					base.transform.LookAt(lookPos + upLook, forwardLook);
				}
				else if (drawShot == 5)
				{
					base.transform.LookAt(lookPos - upLook, -forwardLook);
				}
				GetComponent<Camera>().ResetWorldToCameraMatrix();
			}
		}

		private void Update()
		{
			frameID++;
			if ((bool)RT && captureHDR && stage == Stage.CAPTURE)
			{
				stage = Stage.PRECAPTURE;
				bool allowHDR = GetComponent<Camera>().allowHDR;
				GetComponent<Camera>().allowHDR = true;
				RenderTexture.active = RenderTexture.active;
				RenderTexture.active = RT;
				GetComponent<Camera>().targetTexture = RT;
				GetComponent<Camera>().Render();
				GetComponent<Camera>().allowHDR = allowHDR;
				GetComponent<Camera>().targetTexture = null;
				RenderTexture.active = null;
				stage = Stage.CAPTURE;
			}
		}

		private void OnPostRender()
		{
			if (captureHDR && (bool)RT && stage == Stage.CAPTURE)
			{
				int width = RT.width;
				int num = 0;
				if (!blitMat)
				{
					blitMat = new Material(Shader.Find("Hidden/Marmoset/RGBM Blit"));
				}
				toggleKeyword("MARMO_RGBM_INPUT_ON", yes: false);
				toggleKeyword("MARMO_RGBM_OUTPUT_ON", yes: true);
				GL.PushMatrix();
				GL.LoadPixelMatrix(0f, width, width, 0f);
				Graphics.DrawTexture(new Rect(0f, num, width, width), RT, blitMat);
				GL.PopMatrix();
			}
			if (stage == Stage.NEXTSKY)
			{
				if (targetCube != null)
				{
					StartStage(Stage.CAPTURE);
				}
				else
				{
					StartStage(Stage.DONE);
				}
			}
			else
			{
				if (stage != Stage.CAPTURE && stage != Stage.CONVOLVE)
				{
					return;
				}
				int num2 = captureSize;
				bool convertHDR = !captureHDR;
				if (num2 > Screen.width || num2 > Screen.height)
				{
					Debug.LogWarning("<b>Skipping Cubemap</b> - The viewport is too small (" + Screen.width + "x" + Screen.height + ") to probe the cubemap \"" + targetCube.name + "\" (" + num2 + "x" + num2 + ")");
					StartStage(Stage.NEXTSKY);
					return;
				}
				if (drawShot == 0)
				{
					faceTexture.ReadPixels(new Rect(0f, 0f, num2, num2), 0, 0);
					faceTexture.Apply();
					SetFacePixels(targetCube, CubemapFace.PositiveZ, faceTexture, targetMip, flipHorz: false, flipVert: true, convertHDR);
				}
				else if (drawShot == 1)
				{
					faceTexture.ReadPixels(new Rect(0f, 0f, num2, num2), 0, 0);
					faceTexture.Apply();
					SetFacePixels(targetCube, CubemapFace.NegativeZ, faceTexture, targetMip, flipHorz: false, flipVert: true, convertHDR);
				}
				else if (drawShot == 2)
				{
					faceTexture.ReadPixels(new Rect(0f, 0f, num2, num2), 0, 0);
					faceTexture.Apply();
					SetFacePixels(targetCube, CubemapFace.NegativeX, faceTexture, targetMip, flipHorz: false, flipVert: true, convertHDR);
				}
				else if (drawShot == 3)
				{
					faceTexture.ReadPixels(new Rect(0f, 0f, num2, num2), 0, 0);
					faceTexture.Apply();
					SetFacePixels(targetCube, CubemapFace.PositiveX, faceTexture, targetMip, flipHorz: false, flipVert: true, convertHDR);
				}
				else if (drawShot == 4)
				{
					faceTexture.ReadPixels(new Rect(0f, 0f, num2, num2), 0, 0);
					faceTexture.Apply();
					SetFacePixels(targetCube, CubemapFace.PositiveY, faceTexture, targetMip, flipHorz: true, flipVert: false, convertHDR);
				}
				else if (drawShot == 5)
				{
					faceTexture.ReadPixels(new Rect(0f, 0f, num2, num2), 0, 0);
					faceTexture.Apply();
					SetFacePixels(targetCube, CubemapFace.NegativeY, faceTexture, targetMip, flipHorz: true, flipVert: false, convertHDR);
					if (stage == Stage.CAPTURE)
					{
						targetCube.Apply(updateMipmaps: true, makeNoLongerReadable: false);
						StartStage(Stage.CONVOLVE);
						return;
					}
					targetCube.Apply(updateMipmaps: false, makeNoLongerReadable: false);
					targetMip++;
					if (targetMip < mipCount)
					{
						drawShot = 0;
					}
					else
					{
						StartStage(Stage.NEXTSKY);
					}
					return;
				}
				drawShot++;
			}
		}

		private static void SetFacePixels(Cubemap cube, CubemapFace face, Texture2D tex, int mip, bool flipHorz, bool flipVert, bool convertHDR)
		{
			Color[] pixels = tex.GetPixels();
			Color black = Color.black;
			int num = tex.width >> mip;
			int num2 = tex.height >> mip;
			Color[] array = new Color[num * num2];
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					int num3 = i + j * tex.width;
					int num4 = i + j * num;
					array[num4] = pixels[num3];
					if (convertHDR)
					{
						array[num4].a = 1f / 6f;
					}
				}
			}
			if (flipHorz)
			{
				for (int k = 0; k < num / 2; k++)
				{
					for (int l = 0; l < num2; l++)
					{
						int num5 = num - k - 1;
						int num6 = k + l * num;
						int num7 = num5 + l * num;
						black = array[num7];
						array[num7] = array[num6];
						array[num6] = black;
					}
				}
			}
			if (flipVert)
			{
				for (int m = 0; m < num; m++)
				{
					for (int n = 0; n < num2 / 2; n++)
					{
						int num8 = num2 - n - 1;
						int num9 = m + n * num;
						int num10 = m + num8 * num;
						black = array[num10];
						array[num10] = array[num9];
						array[num9] = black;
					}
				}
			}
			cube.SetPixels(array, face, mip);
		}

		private static void toggleKeyword(string on, bool yes)
		{
			if (yes)
			{
				Shader.EnableKeyword(on);
			}
			else
			{
				Shader.DisableKeyword(on);
			}
		}

		private static void toggleKeyword(Material mat, string on, bool yes)
		{
			if (yes)
			{
				mat.EnableKeyword(on);
			}
			else
			{
				mat.DisableKeyword(on);
			}
		}
	}
}
