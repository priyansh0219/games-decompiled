using UWE;

namespace WorldStreaming
{
	public static class BatchOctreesAllocator
	{
		public static readonly SplitNativeArrayPool<byte> octreePool = new SplitNativeArrayPool<byte>(1, 32, 4, 0, 0, 2048, 3, 0);

		public static bool isInitialized { get; private set; }

		public static void Initialize()
		{
			if (!isInitialized)
			{
				WarmupOctreePools();
				isInitialized = true;
			}
		}

		public static void Deinitialize()
		{
			if (isInitialized)
			{
				octreePool.Reset();
				isInitialized = false;
			}
		}

		private static void WarmupOctreePools()
		{
			octreePool.poolSmall.WarmupElement(0, 15360);
			octreePool.poolSmall.ResetCacheStats();
			octreePool.poolBig.WarmupElement(0, 760);
			octreePool.poolBig.WarmupElement(1, 270);
			int i;
			for (i = 2; i <= 12; i++)
			{
				octreePool.poolBig.WarmupElement(i, 224);
			}
			for (; i <= 15; i++)
			{
				octreePool.poolBig.WarmupElement(i, 144);
			}
			for (; i <= 17; i++)
			{
				octreePool.poolBig.WarmupElement(i, 74);
			}
			for (; i <= 20; i++)
			{
				octreePool.poolBig.WarmupElement(i, 56);
			}
			for (; i <= 39; i++)
			{
				octreePool.poolBig.WarmupElement(i, 24);
			}
			for (; i <= 52; i++)
			{
				octreePool.poolBig.WarmupElement(i, 16);
			}
			for (; i <= 72; i++)
			{
				octreePool.poolBig.WarmupElement(i, 8);
			}
			for (; i <= 74; i++)
			{
				octreePool.poolBig.WarmupElement(i, 1);
			}
			octreePool.poolBig.ResetCacheStats();
		}
	}
}
