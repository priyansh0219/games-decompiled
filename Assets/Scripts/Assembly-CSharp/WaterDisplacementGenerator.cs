using System;
using UnityEngine;

[Serializable]
public class WaterDisplacementGenerator
{
	[Header("Shaders:")]
	public Shader updateDisplacementShader;

	private Material updateDisplacementMaterial;

	public ComputeShader updateSpectrumShader;

	private ComputeBuffer bufferH0;

	private ComputeBuffer bufferOmega;

	private ComputeBuffer bufferHt;

	private ComputeBuffer bufferDxyz;

	private FFT512x512 fft;

	private Mesh quadMesh;

	private int displacementMapSize = 512;

	private int xBlockSize = 16;

	private int yBlockSize = 16;

	public float patchLength = 2000f;

	[Header("Waves:")]
	public float choppyScale = 1.3f;

	public float minWaveSize = 0.01f;

	public float phillipsAmplitude = 0.35f;

	[Range(0f, 360f)]
	public float windAngle = 45f;

	public float windSpeed = 600f;

	[Range(0f, 1f)]
	public float windDependency = 0.07f;

	public void Create()
	{
		updateDisplacementMaterial = new Material(updateDisplacementShader);
		quadMesh = GraphicsUtil.CreateQuadMesh();
		int count = (displacementMapSize + 4) * (displacementMapSize + 1);
		int num = displacementMapSize * displacementMapSize;
		bufferH0 = new ComputeBuffer(count, 8, ComputeBufferType.GPUMemory);
		bufferOmega = new ComputeBuffer(count, 4, ComputeBufferType.GPUMemory);
		bufferHt = new ComputeBuffer(3 * num, 8, ComputeBufferType.GPUMemory);
		bufferDxyz = new ComputeBuffer(3 * num, 8, ComputeBufferType.GPUMemory);
		fft = new FFT512x512();
		fft.Create();
	}

	public void Destroy()
	{
		if (bufferH0 != null)
		{
			bufferH0.Dispose();
			bufferH0 = null;
		}
		if (bufferOmega != null)
		{
			bufferOmega.Dispose();
			bufferOmega = null;
		}
		if (bufferHt != null)
		{
			bufferHt.Dispose();
			bufferHt = null;
		}
		if (bufferDxyz != null)
		{
			bufferDxyz.Dispose();
			bufferDxyz = null;
		}
		if (fft != null)
		{
			fft.Destroy();
			fft = null;
		}
	}

	public float GetPatchLength()
	{
		return patchLength;
	}

	private void DrawQuad(Material material)
	{
		material.SetPass(0);
		Graphics.DrawMeshNow(quadMesh, Matrix4x4.identity);
	}

	public void FillComputeBuffers(float sequenceLength)
	{
		int num = (displacementMapSize + 4) * (displacementMapSize + 1);
		Vector2[] array = new Vector2[num];
		float[] array2 = new float[num];
		ComputeInitialSpectrum(array, array2, sequenceLength);
		bufferH0.SetData(array);
		bufferOmega.SetData(array2);
	}

	private float Phillips(Vector2 K, Vector2 W, float v, float a, float dirDepend, float gravity)
	{
		float num = v * v / gravity;
		float num2 = Vector2.Dot(K, K);
		float num3 = Vector2.Dot(K, W);
		float num4 = a * Mathf.Exp(-1f / (num * num * num2)) * (num3 * num3) / (num2 * num2 * num2);
		if (num3 < 0f)
		{
			num4 *= 1f - dirDepend;
		}
		float num5 = minWaveSize;
		return num4 * Mathf.Exp((0f - num2) * num5 * num5);
	}

	private static float Gauss()
	{
		float num = UnityEngine.Random.value;
		float value = UnityEngine.Random.value;
		if (num < 1E-06f)
		{
			num = 1E-06f;
		}
		return Mathf.Sqrt(-2f * Mathf.Log(num)) * Mathf.Cos((float)Math.PI * 2f * value);
	}

	private Vector2 WindDirection()
	{
		Vector2 result = default(Vector2);
		result.x = Mathf.Cos(windAngle * ((float)Math.PI / 180f));
		result.y = Mathf.Sin(windAngle * ((float)Math.PI / 180f));
		return result;
	}

	private void ComputeInitialSpectrum(Vector2[] h0, float[] omega, float sequenceLength)
	{
		float num = 981f;
		float num2 = Mathf.Sqrt(2f) / 2f;
		float a = phillipsAmplitude * 1E-07f;
		float v = windSpeed;
		Vector2 w = WindDirection();
		float num3 = (float)Math.PI * 2f / sequenceLength;
		for (int i = 0; i <= displacementMapSize; i++)
		{
			Vector2 k = default(Vector2);
			k.y = ((float)(-displacementMapSize) / 2f + (float)i) * ((float)Math.PI * 2f / patchLength);
			for (int j = 0; j <= displacementMapSize; j++)
			{
				k.x = ((float)(-displacementMapSize) / 2f + (float)j) * ((float)Math.PI * 2f / patchLength);
				float num4 = 0f;
				if (k.x != 0f && k.y != 0f)
				{
					num4 = Mathf.Sqrt(Phillips(k, w, v, a, windDependency, num));
				}
				h0[i * (displacementMapSize + 4) + j].x = num4 * Gauss() * num2;
				h0[i * (displacementMapSize + 4) + j].y = num4 * Gauss() * num2;
				float magnitude = k.magnitude;
				float num5 = Mathf.Sqrt(num * magnitude);
				if (sequenceLength > 0f)
				{
					num5 = Mathf.Floor(num5 / num3) * num3;
				}
				omega[i * (displacementMapSize + 4) + j] = num5;
			}
		}
	}

	public void GenerateDisplacementMap(RenderTexture displacementTexture, float time)
	{
		int threadGroupsX = (displacementMapSize + xBlockSize - 1) / xBlockSize;
		int threadGroupsY = (displacementMapSize + yBlockSize - 1) / yBlockSize;
		int kernelIndex = updateSpectrumShader.FindKernel("UpdateSpectrumCS");
		updateSpectrumShader.SetBuffer(kernelIndex, ShaderPropertyID._InputH0, bufferH0);
		updateSpectrumShader.SetBuffer(kernelIndex, ShaderPropertyID._InputOmega, bufferOmega);
		updateSpectrumShader.SetBuffer(kernelIndex, ShaderPropertyID._OutputHt, bufferHt);
		int num = displacementMapSize * displacementMapSize;
		int num2 = displacementMapSize * displacementMapSize * 2;
		updateSpectrumShader.SetInt(ShaderPropertyID._ActualDim, displacementMapSize);
		updateSpectrumShader.SetInt(ShaderPropertyID._InWidth, displacementMapSize + 4);
		updateSpectrumShader.SetInt(ShaderPropertyID._OutWidth, displacementMapSize);
		updateSpectrumShader.SetInt(ShaderPropertyID._OutHeight, displacementMapSize);
		updateSpectrumShader.SetInt(ShaderPropertyID._DtxAddressOffset, num);
		updateSpectrumShader.SetInt(ShaderPropertyID._DtyAddressOffset, num2);
		updateSpectrumShader.SetFloat(ShaderPropertyID._Time, time);
		updateSpectrumShader.SetFloat(ShaderPropertyID._ChoppyScale, choppyScale);
		updateSpectrumShader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, 1);
		fft.Compute(bufferHt, bufferDxyz);
		Graphics.SetRenderTarget(displacementTexture);
		GL.Viewport(new Rect(0f, 0f, displacementMapSize, displacementMapSize));
		updateDisplacementMaterial.SetInt(ShaderPropertyID._OutWidth, displacementMapSize);
		updateDisplacementMaterial.SetInt(ShaderPropertyID._OutHeight, displacementMapSize);
		updateDisplacementMaterial.SetInt(ShaderPropertyID._DxAddressOffset, num);
		updateDisplacementMaterial.SetInt(ShaderPropertyID._DyAddressOffset, num2);
		updateDisplacementMaterial.SetFloat(ShaderPropertyID._ChoppyScale, choppyScale);
		updateDisplacementMaterial.SetBuffer(ShaderPropertyID._InputDxyz, bufferDxyz);
		DrawQuad(updateDisplacementMaterial);
	}
}
