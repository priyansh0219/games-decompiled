using System;
using UWE;
using UnityEngine;

public class FloodSim : MonoBehaviour
{
	private const byte CellUsedMask = 64;

	public Int3 size;

	public float spaceDensity = 0.1f;

	public int seed = 42;

	private Array3<byte> flowGrid;

	private Array3<float> valueGrid;

	private Array3<GameObject> blocks;

	private void ResetSimState()
	{
		flowGrid.Clear();
		valueGrid.Clear();
		System.Random random = new System.Random(seed);
		foreach (Int3 item in flowGrid.Indices())
		{
			if (random.NextDouble() < (double)spaceDensity)
			{
				byte value = byte.MaxValue;
				flowGrid.Set(item, value);
			}
			else
			{
				flowGrid.Set<byte>(item, 0);
			}
			valueGrid.Set(item, 0f);
		}
		Int2.RangeEnumerator enumerator = Int2.Range(Int2.zero, size.xz - 1).GetEnumerator();
		while (enumerator.MoveNext())
		{
			Int3 p = enumerator.Current.XZToInt3(size.y - 1);
			valueGrid.Set(p, 1f);
		}
		foreach (GameObject block in blocks)
		{
			UnityEngine.Object.Destroy(block);
		}
		foreach (Int3 item2 in blocks.Indices())
		{
			if ((flowGrid.Get(item2) & 0x40) > 0)
			{
				GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
				UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
				blocks.Set(item2, gameObject);
				gameObject.transform.position = item2.ToVector3() + UWE.Utils.half3;
				gameObject.AddComponent<FloodSimDebugBox>();
			}
			else
			{
				blocks.Set(item2, null);
			}
		}
	}

	private void Awake()
	{
		blocks = new Array3<GameObject>(size.x, size.y, size.z);
		flowGrid = new Array3<byte>(size.x, size.y, size.z);
		valueGrid = new Array3<float>(size.x, size.y, size.z);
		ResetSimState();
	}

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		FloodSimStep(ref flowGrid.data[0], ref valueGrid.data[0], size, size, Time.fixedDeltaTime);
	}

	private void Update()
	{
		foreach (Int3 item in flowGrid.Indices())
		{
			if ((flowGrid.Get(item) & 0x40) > 0)
			{
				blocks.Get(item).GetComponent<Renderer>().material.color = Color.Lerp(Color.blue, Color.red, valueGrid.Get(item));
			}
		}
	}

	private static void FloodSimStep(ref byte flowGrid, ref float valueGrid, Int3 arrayDims, Int3 usedDims, float dt)
	{
		UnityUWE.FloodSimStep(ref flowGrid, ref valueGrid, arrayDims, usedDims, dt);
	}
}
