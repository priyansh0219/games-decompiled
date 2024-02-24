using UnityEngine;

public class FFT512x512
{
	private ComputeShader computeShader;

	private ComputeBuffer tempBuffer;

	private int slices = 3;

	private int coherencyGranularity = 128;

	public void Create()
	{
		computeShader = Resources.Load("FFT512x512") as ComputeShader;
		tempBuffer = new ComputeBuffer(512 * slices * 512, 8, ComputeBufferType.GPUMemory);
	}

	public void Destroy()
	{
		if (tempBuffer != null)
		{
			tempBuffer.Dispose();
			tempBuffer = null;
		}
		computeShader = null;
	}

	private void radix008A(ComputeBuffer dstBuffer, ComputeBuffer srcBuffer, int threadCount, int istride, int ostride, int pstride, float phaseBase)
	{
		int threadGroupsX = threadCount / coherencyGranularity;
		int kernelIndex = computeShader.FindKernel((istride > 1) ? "Radix008A_CS" : "Radix008A_CS2");
		computeShader.SetInt(ShaderPropertyID.thread_count, threadCount);
		computeShader.SetInt(ShaderPropertyID.istride, istride);
		computeShader.SetInt(ShaderPropertyID.ostride, ostride);
		computeShader.SetInt(ShaderPropertyID.pstride, pstride);
		computeShader.SetFloat(ShaderPropertyID.phase_base, phaseBase);
		computeShader.SetBuffer(kernelIndex, ShaderPropertyID.g_SrcData, srcBuffer);
		computeShader.SetBuffer(kernelIndex, ShaderPropertyID.g_DstData, dstBuffer);
		computeShader.Dispatch(kernelIndex, threadGroupsX, 1, 1);
	}

	public void Compute(ComputeBuffer srcBuffer, ComputeBuffer dstBuffer)
	{
		int threadCount = slices * 262144 / 8;
		int num = 32768;
		int num2 = num;
		double num3 = -2.3968450477696024E-05;
		radix008A(tempBuffer, srcBuffer, threadCount, num2, num, 512, (float)num3);
		num2 /= 8;
		num3 *= 8.0;
		radix008A(dstBuffer, tempBuffer, threadCount, num2, num, 512, (float)num3);
		num2 /= 8;
		num3 *= 8.0;
		radix008A(tempBuffer, dstBuffer, threadCount, num2, num, 512, (float)num3);
		num2 /= 8;
		num /= 512;
		num3 *= 8.0;
		radix008A(dstBuffer, tempBuffer, threadCount, num2, num, 1, (float)num3);
		num2 /= 8;
		num3 *= 8.0;
		radix008A(tempBuffer, dstBuffer, threadCount, num2, num, 1, (float)num3);
		num2 /= 8;
		num3 *= 8.0;
		radix008A(dstBuffer, tempBuffer, threadCount, num2, num, 1, (float)num3);
	}
}
