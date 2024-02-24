using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Telemetry : MonoBehaviour, IScheduledUpdateBehaviour, IManagedBehaviour, IOnQuitBehaviour
{
	private PlatformServices platformServices;

	public const string endpointURL = "https://analytics.unknownworlds.com/api";

	private const int productId = 264710;

	private int sessionId;

	private int csId;

	private LogSettingsResponse logSettings = new LogSettingsResponse();

	private float lastUpdateTime;

	private string platformName;

	private string userId;

	private readonly Queue<IEnumerator> sendEventAsynchronousQueue = new Queue<IEnumerator>();

	public static Telemetry Instance { get; private set; }

	public int statisticsPeriod => logSettings.statistics_period;

	public int SessionID => sessionId;

	public int scheduledUpdateIndex { get; set; }

	public bool IsAnalyzingSession()
	{
		return sessionId > 0;
	}

	private void SetAnalyzingSessionEnd()
	{
		sessionId = 0;
	}

	public string GetProfileTag()
	{
		return "Telemetry";
	}

	private void Awake()
	{
		Instance = this;
		PlatformUtils.RegisterOnQuitBehaviour(this);
	}

	private void Start()
	{
		StartCoroutine(SessionStart());
	}

	private void OnEnable()
	{
		UpdateSchedulerUtils.Register(this);
	}

	private void OnDisable()
	{
		UpdateSchedulerUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		PlatformUtils.DeregisterOnQuitBehaviour(this);
		UpdateSchedulerUtils.Deregister(this);
		if (IsAnalyzingSession())
		{
			SessionEnd();
		}
	}

	private void OnApplicationQuit()
	{
		OnQuit();
	}

	public void OnQuit()
	{
		if (SceneManager.GetSceneByName("Main").IsValid())
		{
			SendGameQuit(quitToDesktop: true);
		}
		if (IsAnalyzingSession())
		{
			SessionEnd();
		}
	}

	public void ScheduledUpdate()
	{
		if (IsAnalyzingSession() && Time.realtimeSinceStartup > lastUpdateTime + (float)logSettings.session_log_resolution)
		{
			lastUpdateTime = Time.realtimeSinceStartup;
			StartCoroutine(SendSessionUpdate());
		}
	}

	private IEnumerator SessionStart()
	{
		while (PlatformUtils.main.GetServices() == null)
		{
			yield return null;
		}
		platformServices = PlatformUtils.main.GetServices();
		while (!platformServices.IsUserLoggedIn())
		{
			yield return null;
		}
		yield return DownloadLoggingSettings();
		yield return SendSesionStart(platformServices.GetName(), platformServices.GetUserId());
	}

	public bool SessionEnd()
	{
		IEnumerator enumerator = SendSesionEnd();
		float num = Time.realtimeSinceStartup + 5f;
		while (enumerator.MoveNext() && !(Time.realtimeSinceStartup > num))
		{
		}
		return true;
	}

	public void SendAnalyticsEvent(TelemetryEventCategory category, string name, string value, bool synchronous, bool playthrough, bool singleInstance, bool queued = true)
	{
		int num = logSettings.category_settings.Length;
		bool flag = (int)category >= num || logSettings.category_settings[(int)category];
		if (!(IsAnalyzingSession() && flag))
		{
			return;
		}
		if (synchronous)
		{
			IEnumerator enumerator = SendEvent(category, name, value, playthrough, singleInstance, queued: false);
			float num2 = Time.realtimeSinceStartup + 5f;
			while (enumerator.MoveNext() && !(Time.realtimeSinceStartup > num2))
			{
			}
			return;
		}
		IEnumerator enumerator2 = SendEvent(category, name, value, playthrough, singleInstance, queued);
		if (queued)
		{
			sendEventAsynchronousQueue.Enqueue(enumerator2);
			if (sendEventAsynchronousQueue.Count == 1)
			{
				StartCoroutine(enumerator2);
			}
		}
		else
		{
			StartCoroutine(enumerator2);
		}
	}

	private IEnumerator DownloadLoggingSettings()
	{
		yield return platformServices.TryEnsureServerAccessAsync();
		if (!platformServices.CanAccessServers())
		{
			yield break;
		}
		UnityWebRequest webRequest = UnityWebRequest.Get(string.Format("{0}/log-settings", "https://analytics.unknownworlds.com/api"));
		yield return webRequest.SendWebRequest();
		if (webRequest.isNetworkError)
		{
			Debug.LogError(webRequest.error);
			yield break;
		}
		string text = webRequest.downloadHandler.text;
		try
		{
			logSettings = JsonUtility.FromJson<LogSettingsResponse>(text);
		}
		catch (Exception exception)
		{
			Debug.LogErrorFormat("Bad server log-settings json response: \"{0}\"", text);
			Debug.LogException(exception);
			logSettings = new LogSettingsResponse();
		}
	}

	private IEnumerator SendSesionStart(string setPlatformName, string setUserId)
	{
		yield return platformServices.TryEnsureServerAccessAsync();
		if (platformServices.CanAccessServers())
		{
			platformName = (string.IsNullOrEmpty(setPlatformName) ? "Null" : setPlatformName);
			userId = (string.IsNullOrEmpty(setUserId) ? "Null" : setUserId);
			csId = SNUtils.GetPlasticChangeSetOfBuild(0);
			WWWForm wWWForm = new WWWForm();
			wWWForm.AddField("product_id", 264710);
			wWWForm.AddField("platform", platformName);
			wWWForm.AddField("platform_user_id", userId);
			wWWForm.AddField("cs_id", csId);
			wWWForm.AddField("language", Language.main.GetCurrentLanguage());
			wWWForm.AddField("arguments", string.Join(", ", Environment.GetCommandLineArgs()));
			wWWForm.AddField("used_cheats", DevConsole.HasUsedConsole().ToString());
			wWWForm.AddField("gpu_name", SystemInfo.graphicsDeviceName);
			wWWForm.AddField("gpu_memory", SystemInfo.graphicsMemorySize);
			wWWForm.AddField("gpu_api", SystemInfo.graphicsDeviceType.ToString());
			wWWForm.AddField("cpu_name", SystemInfo.processorType);
			wWWForm.AddField("system_memory", SystemInfo.systemMemorySize);
			wWWForm.AddField("system_os", SystemInfo.operatingSystem);
			wWWForm.AddField("quality", QualitySettings.GetQualityLevel());
			wWWForm.AddField("res_x", Screen.width);
			wWWForm.AddField("res_y", Screen.height);
			UnityWebRequest webRequest = UnityWebRequest.Post(string.Format("{0}/session-start", "https://analytics.unknownworlds.com/api"), wWWForm);
			yield return webRequest.SendWebRequest();
			if (webRequest.isNetworkError || webRequest.isHttpError)
			{
				Debug.LogError(webRequest.error);
				yield break;
			}
			SessionStartResponse sessionStartResponse = SessionStartResponse.CreateFromJSON(webRequest.downloadHandler.text);
			sessionId = sessionStartResponse.session_id;
			Debug.LogFormat("Telemetry session started. Platform: '{0}', UserId: '{1}', SessionId: {2}", platformName, userId, sessionId);
		}
	}

	private IEnumerator SendSessionUpdate()
	{
		if (IsAnalyzingSession())
		{
			yield return platformServices.TryEnsureServerAccessAsync();
			if (platformServices.CanAccessServers())
			{
				WWWForm wWWForm = new WWWForm();
				wWWForm.AddField("session_id", sessionId);
				wWWForm.AddField("used_cheats", DevConsole.HasUsedConsole().ToString());
				wWWForm.AddField("session_length", Mathf.RoundToInt(Time.realtimeSinceStartup));
				wWWForm.AddField("total_game_length", Mathf.RoundToInt(SaveLoadManager.main.timePlayedTotal));
				wWWForm.AddField("fov", MainCamera.camera.fieldOfView.ToString());
				UnityWebRequest unityWebRequest = UnityWebRequest.Post(string.Format("{0}/session-update", "https://analytics.unknownworlds.com/api"), wWWForm);
				yield return unityWebRequest.SendWebRequest();
			}
		}
	}

	private IEnumerator SendSesionEnd()
	{
		if (IsAnalyzingSession())
		{
			int lastActiveSessionId = sessionId;
			SetAnalyzingSessionEnd();
			yield return platformServices.TryEnsureServerAccessAsync();
			if (platformServices.CanAccessServers())
			{
				WWWForm wWWForm = new WWWForm();
				wWWForm.AddField("session_id", lastActiveSessionId);
				wWWForm.AddField("used_cheats", DevConsole.HasUsedConsole().ToString());
				wWWForm.AddField("has_end", "true");
				wWWForm.AddField("session_length", Mathf.RoundToInt(Time.realtimeSinceStartup));
				wWWForm.AddField("total_game_length", Mathf.RoundToInt(SaveLoadManager.main.timePlayedTotal));
				UnityWebRequest unityWebRequest = UnityWebRequest.Post(string.Format("{0}/session-update", "https://analytics.unknownworlds.com/api"), wWWForm);
				yield return unityWebRequest.SendWebRequest();
			}
		}
	}

	private IEnumerator SendEvent(TelemetryEventCategory category, string name, string value, bool playthrough, bool singleInstance, bool queued)
	{
		if (!singleInstance && value != null && value.Length > 150)
		{
			Debug.LogException(new ArgumentOutOfRangeException("value", "Backend does not support analytics event data with more than 150 characters."));
			yield return FinalizeSendEvent(queued);
			yield break;
		}
		yield return platformServices.TryEnsureServerAccessAsync();
		if (!platformServices.CanAccessServers())
		{
			yield return FinalizeSendEvent(queued);
			yield break;
		}
		Vector3 vector = Vector3.zero;
		Camera camera = MainCamera.camera;
		if (camera != null)
		{
			vector = camera.transform.position;
		}
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("product_id", 264710);
		wWWForm.AddField("platform", platformName);
		wWWForm.AddField("platform_user_id", userId);
		wWWForm.AddField("cs_id", csId);
		wWWForm.AddField("session_id", sessionId);
		wWWForm.AddField("position_x", vector.x.ToString(CultureInfo.InvariantCulture));
		wWWForm.AddField("position_y", vector.y.ToString(CultureInfo.InvariantCulture));
		wWWForm.AddField("position_z", vector.z.ToString(CultureInfo.InvariantCulture));
		wWWForm.AddField("game_time", GameAnalytics.timeNow);
		wWWForm.AddField("event_category", category.ToString());
		wWWForm.AddField("event_name", name);
		wWWForm.AddField("event_value", value);
		if (SaveLoadManager.main != null)
		{
			wWWForm.AddField("total_game_length", Mathf.RoundToInt(SaveLoadManager.main.timePlayedTotal));
		}
		if (playthrough)
		{
			wWWForm.AddField("playthrough_id", AnalyticsController.playthroughId);
		}
		if (singleInstance)
		{
			wWWForm.AddField("single_instance", 1);
		}
		UnityWebRequest unityWebRequest = UnityWebRequest.Post(string.Format("{0}/events", "https://analytics.unknownworlds.com/api"), wWWForm);
		yield return unityWebRequest.SendWebRequest();
		yield return FinalizeSendEvent(queued);
	}

	public static void SendGameQuit(bool quitToDesktop)
	{
		using (GameAnalytics.EventData eventData = GameAnalytics.CustomEvent(GameAnalytics.Event.GameQuit))
		{
			eventData.Add("quit_to_desktop", quitToDesktop);
			GameModeUtils.GetGameMode(out var mode, out var cheats);
			eventData.Add("game_mode", (int)mode);
			eventData.Add("game_cheats", (int)cheats);
			eventData.synchronous = quitToDesktop;
		}
	}

	private IEnumerator FinalizeSendEvent(bool queued)
	{
		if (queued)
		{
			sendEventAsynchronousQueue.Dequeue();
			if (sendEventAsynchronousQueue.Count > 0)
			{
				yield return sendEventAsynchronousQueue.Peek();
			}
		}
	}
}
