using UWE;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
	public int resolution = 200;

	public int visRadiusCells = 5;

	public Event<Int2> pixelSeenEvent = new Event<Int2>();

	public Event<FogOfWar> onAllVis = new Event<FogOfWar>();

	private FiniteGrid<bool> seenMask = new FiniteGrid<bool>();

	public bool HasSeen(Int2 pixel)
	{
		return seenMask.Get(pixel);
	}

	public bool HasSeen(Vector3 wsPos)
	{
		Vector2 uv = Landscape.main.WorldPointToUV(wsPos);
		Int2 u = UVToPixel(uv);
		return seenMask.Get(u);
	}

	public Int2 UVToPixel(Vector2 uv)
	{
		return new Int2(Mathf.FloorToInt(uv.x * (float)resolution), Mathf.FloorToInt(uv.y * (float)resolution));
	}

	public Vector2 PixelToUV(Int2 pixel)
	{
		return new Vector2(((float)pixel.x + 0.5f) / (float)resolution, ((float)pixel.y + 0.5f) / (float)resolution);
	}

	private void Awake()
	{
		seenMask.Reset(resolution, resolution);
		seenMask.SetAll(val: false);
		DevConsole.RegisterConsoleCommand(this, "seeall");
	}

	private void Update()
	{
		if (!Landscape.main || !Landscape.main.IsReady())
		{
			return;
		}
		Vector2 uv = Landscape.main.WorldPointToUV(base.transform.position);
		Int2 v = UVToPixel(uv);
		for (int i = -visRadiusCells; i <= visRadiusCells; i++)
		{
			for (int j = -visRadiusCells; j <= visRadiusCells; j++)
			{
				Int2 @int = new Int2(v.x + i, v.y + j);
				if (seenMask.IsInbound(@int) && @int.GetDistance(v) < (float)visRadiusCells && !seenMask.Get(@int))
				{
					seenMask.Set(@int, val: true);
					pixelSeenEvent.Trigger(@int);
				}
			}
		}
	}

	private void OnConsoleCommand_seeall()
	{
		seenMask.SetAll(val: true);
		for (int i = 0; i < resolution; i++)
		{
			for (int j = 0; j < resolution; j++)
			{
				pixelSeenEvent.Trigger(new Int2(i, j));
			}
		}
	}
}
