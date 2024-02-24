using UnityEngine;

public class VehicleInterface_MapController : MonoBehaviour
{
	[AssertNotNull]
	public GameObject interfacePrefab;

	[AssertNotNull]
	public GameObject mapHolder;

	[AssertNotNull]
	public Transform mapSpawnPos;

	[AssertNotNull]
	public GameObject playerDot;

	[AssertNotNull]
	public GameObject lightVfx;

	[AssertNotNull]
	public Seaglide seaglide;

	[AssertNotNull]
	public GameObject seaglideMesh;

	private Renderer seaglideIllumRenderer;

	private MaterialPropertyBlock seaglideIllumPropertyBlock;

	private const int seaglideIllumMaterialIndex = 1;

	private MiniWorld miniWorld;

	private Color illumColor = Color.white;

	private GameObject mapObject;

	private void Awake()
	{
		seaglideIllumPropertyBlock = new MaterialPropertyBlock();
		seaglideIllumRenderer = seaglideMesh.GetComponent<SkinnedMeshRenderer>();
	}

	private void Start()
	{
		if (mapObject == null)
		{
			mapObject = Object.Instantiate(interfacePrefab);
			mapObject.transform.SetParent(mapHolder.transform, worldPositionStays: false);
			mapObject.transform.localPosition = Vector3.zero;
			mapObject.transform.localScale = Vector3.one;
			mapObject.transform.position = mapSpawnPos.position;
			miniWorld = mapObject.GetComponentInChildren<MiniWorld>();
		}
	}

	private void Update()
	{
		if (!seaglide.HasEnergy())
		{
			miniWorld.active = false;
		}
		else if (Player.main != null && (Player.main.currentSub != null || Player.main.currentEscapePod != null || !Player.main.IsUnderwater()))
		{
			miniWorld.active = false;
		}
		else if (AvatarInputHandler.main.IsEnabled() && GameInput.GetButtonDown(GameInput.Button.AltTool))
		{
			miniWorld.active = !miniWorld.active;
		}
		seaglideIllumRenderer.GetPropertyBlock(seaglideIllumPropertyBlock, 1);
		if (miniWorld.active)
		{
			playerDot.SetActive(value: true);
			lightVfx.SetActive(value: true);
			illumColor = Color.Lerp(seaglideIllumPropertyBlock.GetColor(ShaderPropertyID._GlowColor), Color.white, Time.deltaTime);
			seaglideIllumPropertyBlock.SetColor(ShaderPropertyID._GlowColor, illumColor);
		}
		else
		{
			playerDot.SetActive(value: false);
			lightVfx.SetActive(value: false);
			illumColor = Color.Lerp(seaglideIllumPropertyBlock.GetColor(ShaderPropertyID._GlowColor), Color.black, Time.deltaTime);
			seaglideIllumPropertyBlock.SetColor(ShaderPropertyID._GlowColor, illumColor);
		}
		seaglideIllumRenderer.SetPropertyBlock(seaglideIllumPropertyBlock, 1);
	}
}
