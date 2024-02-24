using UWE;
using UnityEngine;

public class MapDisplay : HandTarget, IHandTarget
{
	public GameObject autoPilotDestMarker;

	public GameObject subArrow;

	public Color fogOfWarColor = Color.black;

	public Texture2D colormapTexture;

	public MeshRenderer screen;

	private Texture2D mapTexture;

	private SubRoot sub;

	private FogOfWar fog;

	private SubControl SubControl;

	private bool inited;

	private Bounds landBounds;

	private bool dirty;

	[AssertLocalization]
	private const string setAutopilotHandText = "SetAutopilot";

	public Color GetColorForFraction(float frac)
	{
		int x = Mathf.FloorToInt((float)colormapTexture.width * (1f - frac));
		return colormapTexture.GetPixel(x, 0);
	}

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
		if (hand.IsFreeToInteract())
		{
			if (sub.GetComponent<SubControl>().IsAutoMode())
			{
				ErrorMessage.AddMessage("Autopilot canceled.");
				sub.GetComponent<SubControl>().SetGameObjectsMode();
				return;
			}
			Vector3 activeHitPosition = hand.GetActiveHitPosition();
			Vector3 lsPos = screen.transform.InverseTransformPoint(activeHitPosition);
			ScreenPointToUV(lsPos);
			autoPilotDestMarker.SetActive(value: true);
			autoPilotDestMarker.transform.position = activeHitPosition;
			sub.GetComponent<SubControl>().SetAutoMode();
			ErrorMessage.AddMessage("Autopilot engaged.");
		}
	}

	public Vector3 UVToSurfacePoint(Vector2 uv, float surfaceOffset)
	{
		Vector3 min = screen.gameObject.GetComponent<MeshFilter>().mesh.bounds.min;
		Vector3 max = screen.gameObject.GetComponent<MeshFilter>().mesh.bounds.max;
		return new Vector3(Mathf.Lerp(min.x, max.x, uv.x), max.y + surfaceOffset, Mathf.Lerp(min.z, max.z, uv.y));
	}

	public Vector2 ScreenPointToUV(Vector3 lsPos)
	{
		Vector3 min = screen.gameObject.GetComponent<MeshFilter>().mesh.bounds.min;
		Vector3 max = screen.gameObject.GetComponent<MeshFilter>().mesh.bounds.max;
		return new Vector2(UWE.Utils.Unlerp(lsPos.x, min.x, max.x), UWE.Utils.Unlerp(lsPos.z, min.z, max.z));
	}

	private void Initialize()
	{
		if (!Landscape.main.IsReady())
		{
			return;
		}
		sub = Utils.FindAncestorWithComponent<SubRoot>(base.gameObject);
		fog = Utils.GetLocalPlayer().GetComponent<FogOfWar>();
		SubControl = sub.GetComponent<SubControl>();
		SubControl.modeChangedEvent.AddHandler(base.gameObject, OnControlModeChanged);
		fog.pixelSeenEvent.AddHandler(this, OnFogOfWarPixelChanged);
		autoPilotDestMarker.SetActive(value: false);
		mapTexture = new Texture2D(fog.resolution, fog.resolution);
		mapTexture.name = "MapDisplay.MapTexture";
		for (int i = 0; i < fog.resolution; i++)
		{
			for (int j = 0; j < fog.resolution; j++)
			{
				OnFogOfWarPixelChanged(new Int2(i, j));
			}
		}
		mapTexture.Apply();
		screen.material.mainTexture = mapTexture;
		landBounds = Landscape.main.GetBounds();
		inited = true;
	}

	private void OnFogOfWarPixelChanged(Int2 pixel)
	{
		if (fog.HasSeen(pixel))
		{
			Vector2 uv = fog.PixelToUV(pixel);
			float topographicHeight = Landscape.main.GetTopographicHeight(uv);
			float y = landBounds.min.y;
			float y2 = landBounds.max.y;
			float frac = UWE.Utils.Unlerp(topographicHeight, y, y2);
			mapTexture.SetPixel(pixel.x, pixel.y, GetColorForFraction(frac));
		}
		else
		{
			mapTexture.SetPixel(pixel.x, pixel.y, fogOfWarColor);
		}
		dirty = true;
	}

	private void Update()
	{
		if (!inited)
		{
			Initialize();
		}
		if (inited)
		{
			Vector2 uv = Landscape.main.WorldPointToUV(sub.transform.position);
			Vector3 localPosition = UVToSurfacePoint(uv, 0.1f);
			subArrow.transform.localPosition = localPosition;
			Vector3 localEulerAngles = subArrow.transform.localEulerAngles;
			localEulerAngles.y = sub.subAxis.eulerAngles.y;
			subArrow.transform.localEulerAngles = localEulerAngles;
			if (dirty)
			{
				mapTexture.Apply();
				screen.material.mainTexture = mapTexture;
				dirty = false;
			}
		}
	}
}
