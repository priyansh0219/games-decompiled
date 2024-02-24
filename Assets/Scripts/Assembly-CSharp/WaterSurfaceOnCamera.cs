using UnityEngine;

[RequireComponent(typeof(Camera))]
public class WaterSurfaceOnCamera : MonoBehaviour
{
	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.UpdateGUIHand;

	public WaterSurface waterSurface;

	private bool visible = true;

	private Camera camera;

	public float cullingDepth = 200f;

	private bool enableDepthCulling;

	private bool depthCulled;

	private bool didRenderThisFrame;

	private bool didBeginRender;

	private void Awake()
	{
		camera = GetComponent<Camera>();
	}

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateGUIHand, OnUpdate);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UpdateGUIHand, OnUpdate);
	}

	public void SetVisible(bool _visible)
	{
		visible = _visible;
	}

	public bool GetVisible()
	{
		return visible;
	}

	private void OnPreRender()
	{
		waterSurface.PreRender(camera);
	}

	private void OnUpdate()
	{
		didBeginRender = false;
		if (visible && ((1 << waterSurface.gameObject.layer) & camera.cullingMask) != 0)
		{
			float num = waterSurface.transform.position.y + waterSurface.waterOffset;
			depthCulled = enableDepthCulling && num - camera.transform.position.y >= cullingDepth;
			if (!depthCulled)
			{
				didBeginRender = true;
				waterSurface.BeginRenderWaterSurface(camera);
			}
		}
	}

	private void OnPreCull()
	{
		if (didBeginRender)
		{
			didRenderThisFrame = waterSurface.RenderWaterSurface(camera);
		}
	}

	private void OnPostRender()
	{
		waterSurface.DoUpdate(didRenderThisFrame);
	}
}
