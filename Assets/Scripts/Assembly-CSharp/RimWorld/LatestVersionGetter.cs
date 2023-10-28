using System;
using System.Collections;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace RimWorld
{
	public class LatestVersionGetter : MonoBehaviour
	{
		private int latestPublicBuild = -1;

		private UnityWebRequest webRequest;

		private bool Errored
		{
			get
			{
				if (webRequest.error == null)
				{
					return !webRequest.error.NullOrEmpty();
				}
				return true;
			}
		}

		private IEnumerator Start()
		{
			webRequest = UnityWebRequest.Get("http://rimworldgame.com/ingame/latest.txt");
			yield return webRequest.SendWebRequest();
			if (Errored)
			{
				Log.Warning("Unable to get latest version information from rimworldgame.com. (" + webRequest.error + ")");
				yield break;
			}
			try
			{
				string value = Encoding.UTF8.GetString(webRequest.downloadHandler.data, 0, webRequest.downloadHandler.data.Length).Split('.')[2];
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				latestPublicBuild = Convert.ToInt32(value, invariantCulture);
			}
			catch (Exception ex)
			{
				Log.Warning("Exception parsing latest version: " + ex);
			}
		}

		public void DrawAt(Rect rect)
		{
			if (Errored)
			{
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
				Widgets.Label(rect, "ErrorGettingVersionInfo".Translate(webRequest.error));
			}
			else if (!webRequest.isDone)
			{
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
				Widgets.Label(rect, "LoadingVersionInfo".Translate());
			}
			else if (latestPublicBuild > VersionControl.CurrentBuild)
			{
				GUI.color = Color.yellow;
				Widgets.Label(rect, "BuildNowAvailable".Translate(webRequest.downloadHandler.text));
			}
			else
			{
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
				Widgets.Label(rect, "BuildUpToDate".Translate());
			}
			GUI.color = Color.white;
		}
	}
}
