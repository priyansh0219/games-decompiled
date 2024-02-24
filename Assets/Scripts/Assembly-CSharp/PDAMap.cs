using System.Collections.Generic;
using UnityEngine;

public class PDAMap : HandTarget
{
	public enum ColorMode
	{
		Height = 0,
		Biome = 1
	}

	public GameObject poiMarkerPrefab;

	public GameObject playerMarker;

	public Color fogOfWarColor = Color.black;

	public Texture2D colormapTexture;

	public ColorMode colorMode = ColorMode.Biome;

	public MeshRenderer screen;

	public bool keepInCameraCorner;

	public float depthFromCamera = 1f;

	public Camera hostCamera;

	public float mapSize = 1000f;

	private Texture2D mapTexture;

	private FogOfWar fog;

	private bool inited;

	private Dictionary<PointOfInterest, GameObject> poiMarkers = new Dictionary<PointOfInterest, GameObject>();

	private bool dirty;

	public Color GetColorForFraction(float frac)
	{
		int x = Mathf.FloorToInt((float)colormapTexture.width * (1f - frac));
		return colormapTexture.GetPixel(x, 0);
	}

	public Vector3 UVToMapScreenPoint(Vector2 uv, float surfaceOffset)
	{
		Vector3 min = screen.gameObject.GetComponent<MeshFilter>().mesh.bounds.min;
		Vector3 max = screen.gameObject.GetComponent<MeshFilter>().mesh.bounds.max;
		return new Vector3(Mathf.Lerp(min.x, max.x, uv.x), max.y + surfaceOffset, Mathf.Lerp(min.z, max.z, uv.y));
	}

	private void Initialize()
	{
		if (Utils.GetLocalPlayer() == null)
		{
			return;
		}
		fog = Utils.GetLocalPlayer().GetComponent<FogOfWar>();
		fog.pixelSeenEvent.AddHandler(this, OnFogOfWarPixelChanged);
		mapTexture = new Texture2D(fog.resolution, fog.resolution);
		mapTexture.name = "PDAMap.MapTexture";
		for (int i = 0; i < fog.resolution; i++)
		{
			for (int j = 0; j < fog.resolution; j++)
			{
				OnFogOfWarPixelChanged(new Int2(i, j));
			}
		}
		mapTexture.Apply();
		screen.material.mainTexture = mapTexture;
		inited = true;
	}

	private void OnFogOfWarPixelChanged(Int2 pixel)
	{
	}

	private void UpdatePOIMarker(GameObject marker, Transform xform, Vector3 centerPos)
	{
		float num = mapSize;
		Vector3 v = (xform.position - centerPos) / num + new Vector3(0.5f, 0.5f, 0.5f);
		Vector3 localPosition = UVToMapScreenPoint(v.XZ(), 0.1f);
		marker.transform.localPosition = localPosition;
		Vector3 localEulerAngles = marker.transform.localEulerAngles;
		localEulerAngles.y = xform.eulerAngles.y;
		marker.transform.localEulerAngles = localEulerAngles;
	}

	private void Update()
	{
		if (!inited)
		{
			Initialize();
		}
		if (!inited)
		{
			return;
		}
		if (dirty)
		{
			mapTexture.Apply();
			dirty = false;
		}
		foreach (PointOfInterest pOI in Utils.GetLocalPlayer().GetComponent<POIMemory>().GetPOIs())
		{
			if (!poiMarkers.ContainsKey(pOI))
			{
				GameObject gameObject = Utils.SpawnFromPrefab(poiMarkerPrefab, screen.transform);
				gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				poiMarkers.Add(pOI, gameObject);
			}
		}
		Vector3 position = Utils.GetLocalPlayer().transform.position;
		foreach (PointOfInterest key in poiMarkers.Keys)
		{
			if (key != null)
			{
				UpdatePOIMarker(poiMarkers[key], key.target, position);
			}
		}
		UpdatePOIMarker(playerMarker, Utils.GetLocalPlayer().transform, position);
	}

	private void LateUpdate()
	{
		if (keepInCameraCorner)
		{
			Vector3 position = hostCamera.ViewportToWorldPoint(new Vector3(0f, 0f, depthFromCamera));
			screen.transform.position = position;
		}
	}
}
