using UnityEngine;
using UnityEngine.Rendering;

public class OVRGridCube : MonoBehaviour
{
	public KeyCode GridKey = KeyCode.G;

	private GameObject CubeGrid;

	private bool CubeGridOn;

	private bool CubeSwitchColorOld;

	private bool CubeSwitchColor;

	private int gridSizeX = 6;

	private int gridSizeY = 4;

	private int gridSizeZ = 6;

	private float gridScale = 0.3f;

	private float cubeScale = 0.03f;

	private OVRCameraRig CameraController;

	private void Update()
	{
		UpdateCubeGrid();
	}

	public void SetOVRCameraController(ref OVRCameraRig cameraController)
	{
		CameraController = cameraController;
	}

	private void UpdateCubeGrid()
	{
		if (Input.GetKeyDown(GridKey))
		{
			if (!CubeGridOn)
			{
				CubeGridOn = true;
				Debug.LogWarning("CubeGrid ON");
				if (CubeGrid != null)
				{
					CubeGrid.SetActive(value: true);
				}
				else
				{
					CreateCubeGrid();
				}
			}
			else
			{
				CubeGridOn = false;
				Debug.LogWarning("CubeGrid OFF");
				if (CubeGrid != null)
				{
					CubeGrid.SetActive(value: false);
				}
			}
		}
		if (CubeGrid != null)
		{
			CubeSwitchColor = !OVRManager.tracker.isPositionTracked;
			if (CubeSwitchColor != CubeSwitchColorOld)
			{
				CubeGridSwitchColor(CubeSwitchColor);
			}
			CubeSwitchColorOld = CubeSwitchColor;
		}
	}

	private void CreateCubeGrid()
	{
		Debug.LogWarning("Create CubeGrid");
		CubeGrid = new GameObject("CubeGrid");
		CubeGrid.layer = CameraController.gameObject.layer;
		for (int i = -gridSizeX; i <= gridSizeX; i++)
		{
			for (int j = -gridSizeY; j <= gridSizeY; j++)
			{
				for (int k = -gridSizeZ; k <= gridSizeZ; k++)
				{
					int num = 0;
					if ((i == 0 && j == 0) || (i == 0 && k == 0) || (j == 0 && k == 0))
					{
						num = ((i != 0 || j != 0 || k != 0) ? 1 : 2);
					}
					GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
					obj.GetComponent<BoxCollider>().enabled = false;
					obj.layer = CameraController.gameObject.layer;
					Renderer component = obj.GetComponent<Renderer>();
					component.shadowCastingMode = ShadowCastingMode.Off;
					component.receiveShadows = false;
					switch (num)
					{
					case 0:
						component.material.color = Color.red;
						break;
					case 1:
						component.material.color = Color.white;
						break;
					default:
						component.material.color = Color.yellow;
						break;
					}
					obj.transform.position = new Vector3((float)i * gridScale, (float)j * gridScale, (float)k * gridScale);
					float num2 = 0.7f;
					if (num == 1)
					{
						num2 = 1f;
					}
					if (num == 2)
					{
						num2 = 2f;
					}
					obj.transform.localScale = new Vector3(cubeScale * num2, cubeScale * num2, cubeScale * num2);
					obj.transform.parent = CubeGrid.transform;
				}
			}
		}
	}

	private void CubeGridSwitchColor(bool CubeSwitchColor)
	{
		Color color = Color.red;
		if (CubeSwitchColor)
		{
			color = Color.blue;
		}
		foreach (Transform item in CubeGrid.transform)
		{
			Material material = item.GetComponent<Renderer>().material;
			if (material.color == Color.red || material.color == Color.blue)
			{
				material.color = color;
			}
		}
	}
}
