public class SimplexNoise
{
	private static byte[] perm = new byte[512]
	{
		151, 160, 137, 91, 90, 15, 131, 13, 201, 95,
		96, 53, 194, 233, 7, 225, 140, 36, 103, 30,
		69, 142, 8, 99, 37, 240, 21, 10, 23, 190,
		6, 148, 247, 120, 234, 75, 0, 26, 197, 62,
		94, 252, 219, 203, 117, 35, 11, 32, 57, 177,
		33, 88, 237, 149, 56, 87, 174, 20, 125, 136,
		171, 168, 68, 175, 74, 165, 71, 134, 139, 48,
		27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
		60, 211, 133, 230, 220, 105, 92, 41, 55, 46,
		245, 40, 244, 102, 143, 54, 65, 25, 63, 161,
		1, 216, 80, 73, 209, 76, 132, 187, 208, 89,
		18, 169, 200, 196, 135, 130, 116, 188, 159, 86,
		164, 100, 109, 198, 173, 186, 3, 64, 52, 217,
		226, 250, 124, 123, 5, 202, 38, 147, 118, 126,
		255, 82, 85, 212, 207, 206, 59, 227, 47, 16,
		58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
		119, 248, 152, 2, 44, 154, 163, 70, 221, 153,
		101, 155, 167, 43, 172, 9, 129, 22, 39, 253,
		19, 98, 108, 110, 79, 113, 224, 232, 178, 185,
		112, 104, 218, 246, 97, 228, 251, 34, 242, 193,
		238, 210, 144, 12, 191, 179, 162, 241, 81, 51,
		145, 235, 249, 14, 239, 107, 49, 192, 214, 31,
		181, 199, 106, 157, 184, 84, 204, 176, 115, 121,
		50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
		222, 114, 67, 29, 24, 72, 243, 141, 128, 195,
		78, 66, 215, 61, 156, 180, 151, 160, 137, 91,
		90, 15, 131, 13, 201, 95, 96, 53, 194, 233,
		7, 225, 140, 36, 103, 30, 69, 142, 8, 99,
		37, 240, 21, 10, 23, 190, 6, 148, 247, 120,
		234, 75, 0, 26, 197, 62, 94, 252, 219, 203,
		117, 35, 11, 32, 57, 177, 33, 88, 237, 149,
		56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
		74, 165, 71, 134, 139, 48, 27, 166, 77, 146,
		158, 231, 83, 111, 229, 122, 60, 211, 133, 230,
		220, 105, 92, 41, 55, 46, 245, 40, 244, 102,
		143, 54, 65, 25, 63, 161, 1, 216, 80, 73,
		209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
		135, 130, 116, 188, 159, 86, 164, 100, 109, 198,
		173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
		5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
		207, 206, 59, 227, 47, 16, 58, 17, 182, 189,
		28, 42, 223, 183, 170, 213, 119, 248, 152, 2,
		44, 154, 163, 70, 221, 153, 101, 155, 167, 43,
		172, 9, 129, 22, 39, 253, 19, 98, 108, 110,
		79, 113, 224, 232, 178, 185, 112, 104, 218, 246,
		97, 228, 251, 34, 242, 193, 238, 210, 144, 12,
		191, 179, 162, 241, 81, 51, 145, 235, 249, 14,
		239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
		184, 84, 204, 176, 115, 121, 50, 45, 127, 4,
		150, 254, 138, 236, 205, 93, 222, 114, 67, 29,
		24, 72, 243, 141, 128, 195, 78, 66, 215, 61,
		156, 180
	};

	private static byte[,] simplex = new byte[64, 4]
	{
		{ 0, 1, 2, 3 },
		{ 0, 1, 3, 2 },
		{ 0, 0, 0, 0 },
		{ 0, 2, 3, 1 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 1, 2, 3, 0 },
		{ 0, 2, 1, 3 },
		{ 0, 0, 0, 0 },
		{ 0, 3, 1, 2 },
		{ 0, 3, 2, 1 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 1, 3, 2, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 1, 2, 0, 3 },
		{ 0, 0, 0, 0 },
		{ 1, 3, 0, 2 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 2, 3, 0, 1 },
		{ 2, 3, 1, 0 },
		{ 1, 0, 2, 3 },
		{ 1, 0, 3, 2 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 2, 0, 3, 1 },
		{ 0, 0, 0, 0 },
		{ 2, 1, 3, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 2, 0, 1, 3 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 3, 0, 1, 2 },
		{ 3, 0, 2, 1 },
		{ 0, 0, 0, 0 },
		{ 3, 1, 2, 0 },
		{ 2, 1, 0, 3 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 0, 0, 0, 0 },
		{ 3, 1, 0, 2 },
		{ 0, 0, 0, 0 },
		{ 3, 2, 0, 1 },
		{ 3, 2, 1, 0 }
	};

	public static float Noise(float x)
	{
		int num = FastFloor(x);
		int num2 = num + 1;
		float num3 = x - (float)num;
		float num4 = num3 - 1f;
		float num5 = 1f - num3 * num3;
		float num6 = num5 * num5;
		float num7 = num6 * num6 * Grad(perm[num & 0xFF], num3);
		float num8 = 1f - num4 * num4;
		float num9 = num8 * num8;
		float num10 = num9 * num9 * Grad(perm[num2 & 0xFF], num4);
		return 0.25f * (num7 + num10);
	}

	public static float Noise(float x, float y)
	{
		float num = (x + y) * 0.3660254f;
		float x2 = x + num;
		float x3 = y + num;
		int num2 = FastFloor(x2);
		int num3 = FastFloor(x3);
		float num4 = (float)(num2 + num3) * 0.21132487f;
		float num5 = (float)num2 - num4;
		float num6 = (float)num3 - num4;
		float num7 = x - num5;
		float num8 = y - num6;
		int num9;
		int num10;
		if (num7 > num8)
		{
			num9 = 1;
			num10 = 0;
		}
		else
		{
			num9 = 0;
			num10 = 1;
		}
		float num11 = num7 - (float)num9 + 0.21132487f;
		float num12 = num8 - (float)num10 + 0.21132487f;
		float num13 = num7 - 1f + 0.42264974f;
		float num14 = num8 - 1f + 0.42264974f;
		int num15 = num2 % 256;
		int num16 = num3 % 256;
		float num17 = 0.5f - num7 * num7 - num8 * num8;
		float num18;
		if (num17 < 0f)
		{
			num18 = 0f;
		}
		else
		{
			num17 *= num17;
			num18 = num17 * num17 * Grad(perm[num15 + perm[num16]], num7, num8);
		}
		float num19 = 0.5f - num11 * num11 - num12 * num12;
		float num20;
		if (num19 < 0f)
		{
			num20 = 0f;
		}
		else
		{
			num19 *= num19;
			num20 = num19 * num19 * Grad(perm[num15 + num9 + perm[num16 + num10]], num11, num12);
		}
		float num21 = 0.5f - num13 * num13 - num14 * num14;
		float num22;
		if (num21 < 0f)
		{
			num22 = 0f;
		}
		else
		{
			num21 *= num21;
			num22 = num21 * num21 * Grad(perm[num15 + 1 + perm[num16 + 1]], num13, num14);
		}
		return 40f * (num18 + num20 + num22);
	}

	public static float Noise(float x, float y, float z)
	{
		float num = (x + y + z) * (1f / 3f);
		float x2 = x + num;
		float x3 = y + num;
		float x4 = z + num;
		int num2 = FastFloor(x2);
		int num3 = FastFloor(x3);
		int num4 = FastFloor(x4);
		float num5 = (float)(num2 + num3 + num4) * (1f / 6f);
		float num6 = (float)num2 - num5;
		float num7 = (float)num3 - num5;
		float num8 = (float)num4 - num5;
		float num9 = x - num6;
		float num10 = y - num7;
		float num11 = z - num8;
		int num12;
		int num13;
		int num14;
		int num15;
		int num16;
		int num17;
		if (num9 >= num10)
		{
			if (num10 >= num11)
			{
				num12 = 1;
				num13 = 0;
				num14 = 0;
				num15 = 1;
				num16 = 1;
				num17 = 0;
			}
			else if (num9 >= num11)
			{
				num12 = 1;
				num13 = 0;
				num14 = 0;
				num15 = 1;
				num16 = 0;
				num17 = 1;
			}
			else
			{
				num12 = 0;
				num13 = 0;
				num14 = 1;
				num15 = 1;
				num16 = 0;
				num17 = 1;
			}
		}
		else if (num10 < num11)
		{
			num12 = 0;
			num13 = 0;
			num14 = 1;
			num15 = 0;
			num16 = 1;
			num17 = 1;
		}
		else if (num9 < num11)
		{
			num12 = 0;
			num13 = 1;
			num14 = 0;
			num15 = 0;
			num16 = 1;
			num17 = 1;
		}
		else
		{
			num12 = 0;
			num13 = 1;
			num14 = 0;
			num15 = 1;
			num16 = 1;
			num17 = 0;
		}
		float num18 = num9 - (float)num12 + 1f / 6f;
		float num19 = num10 - (float)num13 + 1f / 6f;
		float num20 = num11 - (float)num14 + 1f / 6f;
		float num21 = num9 - (float)num15 + 1f / 3f;
		float num22 = num10 - (float)num16 + 1f / 3f;
		float num23 = num11 - (float)num17 + 1f / 3f;
		float num24 = num9 - 1f + 0.5f;
		float num25 = num10 - 1f + 0.5f;
		float num26 = num11 - 1f + 0.5f;
		int num27 = num2 % 256;
		int num28 = num3 % 256;
		int num29 = num4 % 256;
		float num30 = 0.6f - num9 * num9 - num10 * num10 - num11 * num11;
		float num31;
		if (num30 < 0f)
		{
			num31 = 0f;
		}
		else
		{
			num30 *= num30;
			num31 = num30 * num30 * Grad(perm[num27 + perm[num28 + perm[num29]]], num9, num10, num11);
		}
		float num32 = 0.6f - num18 * num18 - num19 * num19 - num20 * num20;
		float num33;
		if (num32 < 0f)
		{
			num33 = 0f;
		}
		else
		{
			num32 *= num32;
			num33 = num32 * num32 * Grad(perm[num27 + num12 + perm[num28 + num13 + perm[num29 + num14]]], num18, num19, num20);
		}
		float num34 = 0.6f - num21 * num21 - num22 * num22 - num23 * num23;
		float num35;
		if (num34 < 0f)
		{
			num35 = 0f;
		}
		else
		{
			num34 *= num34;
			num35 = num34 * num34 * Grad(perm[num27 + num15 + perm[num28 + num16 + perm[num29 + num17]]], num21, num22, num23);
		}
		float num36 = 0.6f - num24 * num24 - num25 * num25 - num26 * num26;
		float num37;
		if (num36 < 0f)
		{
			num37 = 0f;
		}
		else
		{
			num36 *= num36;
			num37 = num36 * num36 * Grad(perm[num27 + 1 + perm[num28 + 1 + perm[num29 + 1]]], num24, num25, num26);
		}
		return 32f * (num31 + num33 + num35 + num37);
	}

	public static float Noise(float x, float y, float z, float w)
	{
		float num = (x + y + z + w) * 0.309017f;
		float x2 = x + num;
		float x3 = y + num;
		float x4 = z + num;
		float x5 = w + num;
		int num2 = FastFloor(x2);
		int num3 = FastFloor(x3);
		int num4 = FastFloor(x4);
		int num5 = FastFloor(x5);
		float num6 = (float)(num2 + num3 + num4 + num5) * 0.1381966f;
		float num7 = (float)num2 - num6;
		float num8 = (float)num3 - num6;
		float num9 = (float)num4 - num6;
		float num10 = (float)num5 - num6;
		float num11 = x - num7;
		float num12 = y - num8;
		float num13 = z - num9;
		float num14 = w - num10;
		int num15 = ((num11 > num12) ? 32 : 0);
		int num16 = ((num11 > num13) ? 16 : 0);
		int num17 = ((num12 > num13) ? 8 : 0);
		int num18 = ((num11 > num14) ? 4 : 0);
		int num19 = ((num12 > num14) ? 2 : 0);
		int num20 = ((num13 > num14) ? 1 : 0);
		int num21 = num15 + num16 + num17 + num18 + num19 + num20;
		int num22 = ((simplex[num21, 0] >= 3) ? 1 : 0);
		int num23 = ((simplex[num21, 1] >= 3) ? 1 : 0);
		int num24 = ((simplex[num21, 2] >= 3) ? 1 : 0);
		int num25 = ((simplex[num21, 3] >= 3) ? 1 : 0);
		int num26 = ((simplex[num21, 0] >= 2) ? 1 : 0);
		int num27 = ((simplex[num21, 1] >= 2) ? 1 : 0);
		int num28 = ((simplex[num21, 2] >= 2) ? 1 : 0);
		int num29 = ((simplex[num21, 3] >= 2) ? 1 : 0);
		int num30 = ((simplex[num21, 0] >= 1) ? 1 : 0);
		int num31 = ((simplex[num21, 1] >= 1) ? 1 : 0);
		int num32 = ((simplex[num21, 2] >= 1) ? 1 : 0);
		int num33 = ((simplex[num21, 3] >= 1) ? 1 : 0);
		float num34 = num11 - (float)num22 + 0.1381966f;
		float num35 = num12 - (float)num23 + 0.1381966f;
		float num36 = num13 - (float)num24 + 0.1381966f;
		float num37 = num14 - (float)num25 + 0.1381966f;
		float num38 = num11 - (float)num26 + 0.2763932f;
		float num39 = num12 - (float)num27 + 0.2763932f;
		float num40 = num13 - (float)num28 + 0.2763932f;
		float num41 = num14 - (float)num29 + 0.2763932f;
		float num42 = num11 - (float)num30 + 0.41458982f;
		float num43 = num12 - (float)num31 + 0.41458982f;
		float num44 = num13 - (float)num32 + 0.41458982f;
		float num45 = num14 - (float)num33 + 0.41458982f;
		float num46 = num11 - 1f + 0.5527864f;
		float num47 = num12 - 1f + 0.5527864f;
		float num48 = num13 - 1f + 0.5527864f;
		float num49 = num14 - 1f + 0.5527864f;
		int num50 = num2 % 256;
		int num51 = num3 % 256;
		int num52 = num4 % 256;
		int num53 = num5 % 256;
		float num54 = 0.6f - num11 * num11 - num12 * num12 - num13 * num13 - num14 * num14;
		float num55;
		if (num54 < 0f)
		{
			num55 = 0f;
		}
		else
		{
			num54 *= num54;
			num55 = num54 * num54 * Grad(perm[num50 + perm[num51 + perm[num52 + perm[num53]]]], num11, num12, num13, num14);
		}
		float num56 = 0.6f - num34 * num34 - num35 * num35 - num36 * num36 - num37 * num37;
		float num57;
		if (num56 < 0f)
		{
			num57 = 0f;
		}
		else
		{
			num56 *= num56;
			num57 = num56 * num56 * Grad(perm[num50 + num22 + perm[num51 + num23 + perm[num52 + num24 + perm[num53 + num25]]]], num34, num35, num36, num37);
		}
		float num58 = 0.6f - num38 * num38 - num39 * num39 - num40 * num40 - num41 * num41;
		float num59;
		if (num58 < 0f)
		{
			num59 = 0f;
		}
		else
		{
			num58 *= num58;
			num59 = num58 * num58 * Grad(perm[num50 + num26 + perm[num51 + num27 + perm[num52 + num28 + perm[num53 + num29]]]], num38, num39, num40, num41);
		}
		float num60 = 0.6f - num42 * num42 - num43 * num43 - num44 * num44 - num45 * num45;
		float num61;
		if (num60 < 0f)
		{
			num61 = 0f;
		}
		else
		{
			num60 *= num60;
			num61 = num60 * num60 * Grad(perm[num50 + num30 + perm[num51 + num31 + perm[num52 + num32 + perm[num53 + num33]]]], num42, num43, num44, num45);
		}
		float num62 = 0.6f - num46 * num46 - num47 * num47 - num48 * num48 - num49 * num49;
		float num63;
		if (num62 < 0f)
		{
			num63 = 0f;
		}
		else
		{
			num62 *= num62;
			num63 = num62 * num62 * Grad(perm[num50 + 1 + perm[num51 + 1 + perm[num52 + 1 + perm[num53 + 1]]]], num46, num47, num48, num49);
		}
		return 27f * (num55 + num57 + num59 + num61 + num63);
	}

	private static int FastFloor(float x)
	{
		if (!(x > 0f))
		{
			return (int)x - 1;
		}
		return (int)x;
	}

	private static float Grad(int hash, float x)
	{
		int num = hash & 0xF;
		float num2 = 1f + (float)(num & 7);
		if (((uint)num & 8u) != 0)
		{
			num2 = 0f - num2;
		}
		return num2 * x;
	}

	private static float Grad(int hash, float x, float y)
	{
		int num = hash & 7;
		float num2 = ((num < 4) ? x : y);
		float num3 = ((num < 4) ? y : x);
		return ((((uint)num & (true ? 1u : 0u)) != 0) ? (0f - num2) : num2) + ((((uint)num & 2u) != 0) ? (-2f * num3) : (2f * num3));
	}

	private static float Grad(int hash, float x, float y, float z)
	{
		int num = hash & 0xF;
		float num2 = ((num < 8) ? x : y);
		float num3 = ((num < 4) ? y : ((num == 12 || num == 14) ? x : z));
		return ((((uint)num & (true ? 1u : 0u)) != 0) ? (0f - num2) : num2) + ((((uint)num & 2u) != 0) ? (0f - num3) : num3);
	}

	private static float Grad(int hash, float x, float y, float z, float t)
	{
		int num = hash & 0x1F;
		float num2 = ((num < 24) ? x : y);
		float num3 = ((num < 16) ? y : z);
		float num4 = ((num < 8) ? z : t);
		return ((((uint)num & (true ? 1u : 0u)) != 0) ? (0f - num2) : num2) + ((((uint)num & 2u) != 0) ? (0f - num3) : num3) + ((((uint)num & 4u) != 0) ? (0f - num4) : num4);
	}
}
