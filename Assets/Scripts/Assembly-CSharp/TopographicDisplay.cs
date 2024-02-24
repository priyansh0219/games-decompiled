using UWE;
using UnityEngine;

public class TopographicDisplay : HandTarget, IHandTarget
{
	public GameObject autoPilotDestMarker;

	public Color fogOfWarColor = Color.black;

	public Terrain terrain;

	private SubRoot sub;

	private FogOfWar fog;

	private SubControl SubControl;

	private bool inited;

	private float[,] heightMap;

	private Bounds landBounds;

	private bool dirty;

	[AssertLocalization]
	private const string setAutopilotHandText = "SetAutopilot";

	public void OnControlModeChanged(SubControl.Mode newMode)
	{
		if (newMode != 0)
		{
			autoPilotDestMarker.SetActive(value: false);
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		if (hand.IsFreeToInteract())
		{
			HandReticle.main.SetText(HandReticle.TextType.Use, "SetAutopilot", translate: true, GameInput.Button.Exit);
			HandReticle.main.SetText(HandReticle.TextType.UseSubscript, string.Empty, translate: true);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
	}

	private void Initialize()
	{
		if (!Landscape.main.IsReady())
		{
			return;
		}
		sub = Utils.FindAncestorWithComponent<SubRoot>(base.gameObject);
		fog = Utils.GetLocalPlayer().GetComponent<FogOfWar>();
		int num = Mathf.Min(fog.resolution, terrain.terrainData.heightmapResolution);
		heightMap = new float[num, num];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				heightMap[i, j] = 0f;
			}
		}
		terrain.terrainData.SetHeights(0, 0, heightMap);
		SubControl = sub.GetComponent<SubControl>();
		SubControl.modeChangedEvent.AddHandler(base.gameObject, OnControlModeChanged);
		fog.pixelSeenEvent.AddHandler(this, OnFogOfWarPixelChanged);
		autoPilotDestMarker.SetActive(value: false);
		landBounds = Landscape.main.GetBounds();
		inited = true;
	}

	private void OnFogOfWarPixelChanged(Int2 pixel)
	{
		Vector2 uv = fog.PixelToUV(pixel);
		float topographicHeight = Landscape.main.GetTopographicHeight(uv);
		float y = landBounds.min.y;
		float y2 = landBounds.max.y;
		float num = UWE.Utils.Unlerp(topographicHeight, y, y2);
		heightMap[Mathf.FloorToInt(uv.y * (float)heightMap.GetLength(0)), Mathf.FloorToInt(uv.x * (float)heightMap.GetLength(1))] = num;
		dirty = true;
	}

	private void Update()
	{
		if (!inited)
		{
			Initialize();
		}
		if (inited && dirty)
		{
			terrain.terrainData.SetHeights(0, 0, heightMap);
			dirty = false;
		}
	}
}
