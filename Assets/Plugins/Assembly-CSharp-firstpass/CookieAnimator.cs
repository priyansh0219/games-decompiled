using UnityEngine;

public class CookieAnimator : MonoBehaviour
{
	public string frameResourceFormatString = "frame_{0:000}.png";

	public int firstFrameNum;

	public int numFrames = 10;

	private Texture2D[] frames;

	public float fps;

	private int currFrame;

	private void Awake()
	{
		frames = new Texture2D[numFrames];
		for (int i = firstFrameNum; i < firstFrameNum + numFrames; i++)
		{
			string text = string.Format(frameResourceFormatString, i);
			frames[i - firstFrameNum] = Resources.Load(text) as Texture2D;
			if (frames[i - firstFrameNum] == null)
			{
				Debug.LogError("Could not load resource: " + text);
			}
		}
	}

	private void Start()
	{
		InvokeRepeating("NextFrame", 1f / fps, 1f / fps);
	}

	private void NextFrame()
	{
		currFrame++;
		GetComponent<Light>().cookie = frames[currFrame % frames.Length];
	}
}
