using System.IO;
using UnityEngine;

public class RandomStart : MonoBehaviour
{
	public static RandomStart main;

	public Texture2D validStartPointTexture;

	private const float kWorldExtents = 2048f;

	public bool debugPlayer;

	public float autoGenNewStartInterval;

	private float timeOfLastStart;

	private void Awake()
	{
		main = this;
		DevConsole.RegisterConsoleCommand(this, "savestartmap");
		DevConsole.RegisterConsoleCommand(this, "debugstartmap");
	}

	private bool IsStartPointValid(Vector3 point, bool debug = false)
	{
		float num = Mathf.Clamp01((point.x + 2048f) / 4096f);
		float num2 = Mathf.Clamp01((point.z + 2048f) / 4096f);
		int num3 = (int)(num * (float)validStartPointTexture.width);
		int num4 = (int)(num2 * (float)validStartPointTexture.height);
		Color pixel = validStartPointTexture.GetPixel(num3, num4);
		bool result = pixel.g > 0.5f;
		if (debug)
		{
			Debug.Log("GetIsStartPointValid(" + point.ToString() + ") pixel: " + num3 + "/" + num4 + " + valid: " + result.ToString() + " (color: " + pixel.ToString() + ") - width/height: " + validStartPointTexture.width + "/" + validStartPointTexture.height);
		}
		return result;
	}

	public Vector3 GetRandomStartPoint()
	{
		timeOfLastStart = Time.time;
		for (int i = 0; i < 1000; i++)
		{
			Vector3 vector = new Vector3(Random.Range(-2048f, 2048f), 0f, Random.Range(-2048f, 2048f));
			if (IsStartPointValid(vector))
			{
				return vector;
			}
		}
		Debug.LogWarning("Could not find valid random start. Using (0,0,0) instead.", this);
		return Vector3.zero;
	}

	private void OnConsoleCommand_savestartmap()
	{
		int num = 512;
		Texture2D texture2D = new Texture2D(num, num, TextureFormat.RGB24, mipChain: false);
		Vector3 point = default(Vector3);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				point.x = ((float)i / (float)num - 0.5f) * 2f * 2048f;
				point.z = (0f - ((float)j / (float)num - 0.5f)) * 2f * 2048f;
				Color color = (IsStartPointValid(point) ? Color.green : Color.black);
				texture2D.SetPixel(i, j, color);
			}
		}
		byte[] array = texture2D.EncodeToPNG();
		string text = Application.dataPath + "/startmap.png";
		FileStream fileStream = File.OpenWrite(text);
		fileStream.Write(array, 0, array.Length);
		fileStream.Close();
		ErrorMessage.AddDebug("Wrote startmap to \"" + text + "\".");
	}

	private void OnConsoleCommand_debugstartmap()
	{
		for (int i = 0; i < 1000; i++)
		{
			Vector3 randomStartPoint = GetRandomStartPoint();
			GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = randomStartPoint;
		}
	}

	private void Update()
	{
		if (debugPlayer)
		{
			IsStartPointValid(Player.main.transform.position, debug: true);
		}
		if (autoGenNewStartInterval > 0f && Time.time > timeOfLastStart + autoGenNewStartInterval)
		{
			DevConsole.SendConsoleCommand("randomstart");
		}
	}
}
