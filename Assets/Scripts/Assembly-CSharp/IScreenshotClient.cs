using UnityEngine;

public interface IScreenshotClient
{
	void OnProgress(string fileName, float progress);

	void OnDone(string fileName, Texture2D texture);

	void OnRemoved(string fileName);
}
