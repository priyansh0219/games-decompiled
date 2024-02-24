using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Gendarme;
using LitJson;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Networking;

[ProtoContract]
public class PlayerTimeCapsule : MonoBehaviour, IProtoTreeEventListener, IScreenshotClient, IProtoEventListener
{
	public delegate void SubmitResultListener(bool success, string status);

	public delegate void OnTextureChanged(Texture2D texture);

	private const string urlSubmit = "https://subnautica.unknownworlds.com/api/v1/time-capsule-submit";

	private const string urlSpawn = "https://subnautica.unknownworlds.com/api/v1/time-capsule-spawn";

	private const string urlOpen = "https://subnautica.unknownworlds.com/api/v1/time-capsule-open";

	private const string urlSync = "https://subnautica.unknownworlds.com/api/v1/time-capsule-sync";

	public const string urlImage = "https://s3.amazonaws.com/subnautica-unknownworlds-com/time-capsule-images/";

	private static readonly HashSet<TechType> blacklist = new HashSet<TechType>
	{
		TechType.RabbitrayEggUndiscovered,
		TechType.JellyrayEggUndiscovered,
		TechType.StalkerEggUndiscovered,
		TechType.ReefbackEggUndiscovered,
		TechType.JumperEggUndiscovered,
		TechType.BonesharkEggUndiscovered,
		TechType.GasopodEggUndiscovered,
		TechType.MesmerEggUndiscovered,
		TechType.SandsharkEggUndiscovered,
		TechType.ShockerEggUndiscovered,
		TechType.CrashEggUndiscovered,
		TechType.CrabsquidEggUndiscovered,
		TechType.CutefishEggUndiscovered,
		TechType.LavaLizardEggUndiscovered,
		TechType.CrabsnakeEggUndiscovered,
		TechType.SpadefishEggUndiscovered
	};

	private const int imageQuality = 90;

	private const int imageWidth = 580;

	private static readonly float[] submitRetryDelays = new float[5] { 1f, 5f, 15f, 15f, 15f };

	private static readonly float[] spawnRetryDelays = new float[2] { 1f, 5f };

	private static readonly float[] openRetryDelays = new float[8] { 1f, 5f, 15f, 60f, 120f, 240f, 480f, 960f };

	private static readonly float[] syncRetryDelays = new float[2] { 30f, 60f };

	private const float imageLoadTimeout = 10f;

	private const int submitTimeout = 40;

	private const int spawnTimeout = 15;

	private const int openTimeout = 15;

	private const int syncTimeout = 30;

	public const int titleMaxLength = 60;

	public const int textMaxLength = 1000;

	private const string tokenPlatform = "platform";

	private const string tokenPlatformUserId = "platform_user_id";

	private const string tokenLanguage = "language";

	private const string tokenUserName = "user_name";

	private const string tokenInstanceId = "instance_id";

	private const string tokenTimeCapsule = "time_capsule";

	private const string tokenSyncTimeCapsules = "time_capsules";

	private const string tokenPlayTime = "play_time";

	private const string contentTypeUrlEncoded = "application/x-www-form-urlencoded";

	private const string contentTypeMultipartFormat = "multipart/form-data; boundary={0}";

	private const string contentTypeText = "text/plain";

	private const string contentTypeJpeg = "image/jpeg";

	[AssertLocalization]
	private const string timeCapsuleStorageLabel = "TimeCapsuleStorageLabel";

	[AssertLocalization]
	private const string submitFailMessage = "TimeCapsuleSubmitFail";

	[AssertLocalization]
	private const string submitSuccessMessage = "TimeCapsuleSubmitSuccess";

	[AssertLocalization(1)]
	private const string titleFormatKey = "TimeCapsuleTitleFormat";

	private static PlayerTimeCapsule _main;

	[AssertNotNull]
	public ChildObjectIdentifier storageRoot;

	public OnTextureChanged onTextureChanged;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int _serializedVersion = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public string _text = string.Empty;

	[NonSerialized]
	[ProtoMember(3)]
	public string _title = string.Empty;

	[NonSerialized]
	[ProtoMember(4, OverwriteList = true)]
	public byte[] _image;

	[NonSerialized]
	[ProtoMember(5)]
	public bool _submit = true;

	[NonSerialized]
	[ProtoMember(6)]
	public readonly List<string> _openQueue = new List<string>();

	private Coroutine submitRoutine;

	private Coroutine spawnRoutine;

	private Coroutine openRoutine;

	private Coroutine syncRoutine;

	private SubmitResultListener submitListener;

	private List<TimeCapsule> spawnQueue = new List<TimeCapsule>();

	private ItemsContainer _container;

	private string _imageFileName;

	private Texture2D _imageTexture;

	public static PlayerTimeCapsule main => _main;

	public string title
	{
		get
		{
			return _title;
		}
		set
		{
			_title = value;
		}
	}

	public string text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
		}
	}

	public Texture2D imageTexture
	{
		get
		{
			if (_imageTexture == null && _image != null)
			{
				_imageTexture = new Texture2D(4, 4, TextureFormat.RGB24, mipChain: false);
				_imageTexture.name = "PlatformTimeCapsule.imageTexture";
				_imageTexture.LoadImage(_image);
			}
			return _imageTexture;
		}
	}

	public bool hasItems => container.count > 0;

	public bool hasTitle => true;

	public bool hasText => !string.IsNullOrEmpty(_text);

	public bool hasImage => _image != null;

	public bool submit
	{
		get
		{
			return _submit;
		}
		set
		{
			_submit = value;
		}
	}

	public ItemsContainer container
	{
		get
		{
			if (_container == null)
			{
				_container = new ItemsContainer(2, 3, storageRoot.transform, "TimeCapsuleStorageLabel", null);
				ItemsContainer itemsContainer = _container;
				itemsContainer.isAllowedToAdd = (IsAllowedToAdd)Delegate.Combine(itemsContainer.isAllowedToAdd, new IsAllowedToAdd(IsAllowedToAdd));
				ItemsContainer itemsContainer2 = _container;
				itemsContainer2.isAllowedToRemove = (IsAllowedToRemove)Delegate.Combine(itemsContainer2.isAllowedToRemove, new IsAllowedToRemove(IsAllowedToRemove));
				_container.onAddItem += OnAddItem;
				_container.onRemoveItem += OnRemoveItem;
			}
			return _container;
		}
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void Awake()
	{
		if (GetComponent<Player>() != null)
		{
			UnityEngine.Object.Destroy(this);
		}
		else if (_main != null)
		{
			Debug.LogError("Multiple PlayerTimeCapsules in scene!");
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			_main = this;
		}
	}

	private void OnDestroy()
	{
		Deinitialize();
	}

	private void Deinitialize()
	{
		if (submitRoutine != null)
		{
			StopCoroutine(submitRoutine);
			submitRoutine = null;
		}
		if (spawnRoutine != null)
		{
			StopCoroutine(spawnRoutine);
			spawnRoutine = null;
		}
		spawnQueue.Clear();
		if (openRoutine != null)
		{
			StopCoroutine(openRoutine);
			openRoutine = null;
		}
		if (syncRoutine != null)
		{
			StopCoroutine(syncRoutine);
			syncRoutine = null;
		}
	}

	public bool IsValid()
	{
		if (hasText && hasImage)
		{
			return hasItems;
		}
		return false;
	}

	public void SetImage(string fileName)
	{
		bool num = !string.Equals(_imageFileName, fileName, StringComparison.Ordinal);
		if (num || (_image != null && string.IsNullOrEmpty(fileName)))
		{
			_image = null;
			if (_imageTexture != null)
			{
				_imageTexture = null;
				NotifyTextureChanged(_imageTexture);
			}
		}
		if (num && !string.IsNullOrEmpty(_imageFileName))
		{
			_imageFileName = null;
			ScreenshotManager.RemoveRequest(_imageFileName, this);
		}
		_imageFileName = fileName;
		if (!string.IsNullOrEmpty(_imageFileName))
		{
			ScreenshotManager.AddRequest(_imageFileName, this, highPriority: true);
		}
	}

	public void Submit(SubmitResultListener listener)
	{
		if (submitRoutine != null)
		{
			Debug.LogError("Time Capsule submit error - submitRoutine already running");
			return;
		}
		if (!IsValid())
		{
			Debug.LogError("Not submitting Time Capsule - not readied");
			return;
		}
		if (!IsValid())
		{
			Debug.LogError("Time Capsule submit cancelled - content not valid");
			return;
		}
		submitListener = listener;
		submitRoutine = StartCoroutine(SubmitRoutine());
	}

	public void RegisterSpawn(TimeCapsule timeCapsule)
	{
		if (!spawnQueue.Contains(timeCapsule))
		{
			spawnQueue.Add(timeCapsule);
			if (spawnRoutine == null)
			{
				spawnRoutine = StartCoroutine(SpawnRoutine());
			}
		}
	}

	public void UnregisterSpawn(TimeCapsule timeCapsule)
	{
		spawnQueue.Remove(timeCapsule);
		if (spawnQueue.Count == 0 && spawnRoutine != null)
		{
			StopCoroutine(spawnRoutine);
			spawnRoutine = null;
		}
	}

	public void RegisterOpen(string instanceId)
	{
		if (!_openQueue.Contains(instanceId))
		{
			_openQueue.Add(instanceId);
			if (openRoutine == null)
			{
				openRoutine = StartCoroutine(OpenRoutine());
			}
		}
	}

	public void SyncContent(string data)
	{
		if (syncRoutine == null && !string.IsNullOrEmpty(data))
		{
			syncRoutine = StartCoroutine(SyncRoutine(data));
		}
	}

	private static string GetTimeCapsulePlatformGroup(string servicesPlatform)
	{
		if (servicesPlatform == "Discord" || servicesPlatform == "Epic")
		{
			return "Steam";
		}
		return servicesPlatform;
	}

	private IEnumerator SubmitRoutine()
	{
		PlatformUtils platformUtils = PlatformUtils.main;
		if (platformUtils == null)
		{
			Debug.LogError("Time Capsule submit error - PlatformUtils is null");
			NotifySubmitStatus(success: false, Language.main.Get("TimeCapsuleSubmitFail"));
			submitRoutine = null;
			yield break;
		}
		PlatformServices platformServices = platformUtils.GetServices();
		if (platformServices == null)
		{
			Debug.LogError("Time Capsule submit error - PlatformServices is null");
			NotifySubmitStatus(success: false, Language.main.Get("TimeCapsuleSubmitFail"));
			submitRoutine = null;
			yield break;
		}
		if (!platformServices.CanAccessUGC())
		{
			Debug.LogWarning("PlayerTimeCapsule.Submit - Platform user cannot access UGC! Bailing");
			NotifySubmitStatus(success: false, Language.main.Get("TimeCapsuleSubmitFail"));
			submitRoutine = null;
			yield break;
		}
		string timeCapsulePlatformGroup = GetTimeCapsulePlatformGroup(platformServices.GetName());
		string userId = platformServices.GetUserId();
		string sha256Hash = getSha256Hash(userId);
		string userName = platformServices.GetUserName();
		string currentLanguage = Language.main.GetCurrentLanguage();
		string text = TimeCapsuleContentProvider.SerializeItems(container);
		if (string.IsNullOrEmpty(timeCapsulePlatformGroup))
		{
			Debug.LogError("Time Capsule submit error - platform value is null or empty");
			NotifySubmitStatus(success: false, Language.main.Get("TimeCapsuleSubmitFail"));
			submitRoutine = null;
			yield break;
		}
		if (string.IsNullOrEmpty(userId))
		{
			Debug.LogError("Time Capsule submit error - platformUserId value is null or empty");
			NotifySubmitStatus(success: false, Language.main.Get("TimeCapsuleSubmitFail"));
			submitRoutine = null;
			yield break;
		}
		if (string.IsNullOrEmpty(currentLanguage))
		{
			Debug.LogError("Time Capsule submit error - language value is null or empty");
			NotifySubmitStatus(success: false, Language.main.Get("TimeCapsuleSubmitFail"));
			submitRoutine = null;
			yield break;
		}
		if (string.IsNullOrEmpty(userName))
		{
		}
		List<IMultipartFormSection> list = new List<IMultipartFormSection>();
		string format = Language.main.GetFormat("TimeCapsuleTitleFormat", Guid.NewGuid().GetHashCode());
		list.Add(new MultipartFormDataSection("platform", timeCapsulePlatformGroup, "text/plain"));
		list.Add(new MultipartFormDataSection("platform_user_id", sha256Hash, "text/plain"));
		list.Add(new MultipartFormDataSection("language", currentLanguage, "text/plain"));
		list.Add(new MultipartFormDataSection("title", format, "text/plain"));
		if (!string.IsNullOrEmpty(_text))
		{
			list.Add(new MultipartFormDataSection("text", _text, "text/plain"));
		}
		if (!string.IsNullOrEmpty(text))
		{
			list.Add(new MultipartFormDataSection("items", text, "text/plain"));
		}
		if (_image != null)
		{
			list.Add(new MultipartFormFileSection("image", _image, "image.jpg", "image/jpeg"));
		}
		byte[] array = UnityWebRequest.GenerateBoundary();
		byte[] payload = UnityWebRequest.SerializeFormSections(list, array);
		string contentType = $"multipart/form-data; boundary={Encoding.UTF8.GetString(array, 0, array.Length)}";
		bool requestSuccess = false;
		bool submitSuccess = false;
		int attempt = 0;
		while (attempt <= submitRetryDelays.Length)
		{
			if (attempt > 0)
			{
				yield return new WaitForSecondsRealtime(submitRetryDelays[attempt - 1]);
			}
			yield return platformServices.TryEnsureServerAccessAsync();
			if (platformServices.CanAccessServers())
			{
				using (UnityWebRequest request = MakePostRequest("https://subnautica.unknownworlds.com/api/v1/time-capsule-submit", payload, contentType, 40))
				{
					yield return request.Send();
					string text2 = string.Empty;
					if (!request.isNetworkError)
					{
						requestSuccess = true;
						string text3 = ((request.downloadHandler != null) ? request.downloadHandler.text : null);
						if (!string.IsNullOrEmpty(text3))
						{
							if (request.responseCode == 201)
							{
								submitSuccess = true;
								Debug.Log("Time Capsule was successfully submitted");
							}
							else
							{
								text2 = $"responseCode is not 201. Response: {text3}";
							}
						}
						else
						{
							text2 = "responseText is null or empty";
						}
					}
					else
					{
						text2 = $"request.isError {request.error}";
					}
					if (!string.IsNullOrEmpty(text2))
					{
						Debug.LogErrorFormat("Time Capsule submit error: '{0}', responseCode: '{1}'", text2, request.responseCode.ToString());
					}
				}
			}
			else
			{
				Debug.LogError("Time Capsule submit error: User cannot access servers.");
			}
			attempt++;
			if (requestSuccess)
			{
				break;
			}
		}
		NotifySubmitStatus(submitSuccess, Language.main.Get(submitSuccess ? "TimeCapsuleSubmitSuccess" : "TimeCapsuleSubmitFail"));
		if (!requestSuccess)
		{
			Debug.LogErrorFormat("Time Capsule submit error - failed to submit Time Capsule after {0} attempts.", attempt.ToString());
		}
		submitRoutine = null;
	}

	private IEnumerator SpawnRoutine()
	{
		PlatformUtils platformUtils = PlatformUtils.main;
		if (platformUtils == null)
		{
			Debug.LogError("Time Capsule spawn error - PlatformUtils is null");
			spawnQueue.Clear();
			spawnRoutine = null;
			yield break;
		}
		PlatformServices platformServices = platformUtils.GetServices();
		if (platformServices == null)
		{
			Debug.LogError("Time Capsule spawn error - PlatformServices is null");
			spawnQueue.Clear();
			spawnRoutine = null;
			yield break;
		}
		if (!platformServices.CanAccessUGC())
		{
			Debug.Log("PlayerTimeCapsule.Spawn - Platform user cannot access UGC. Bailing");
			spawnQueue.Clear();
			spawnRoutine = null;
			yield break;
		}
		string timeCapsulePlatformGroup = GetTimeCapsulePlatformGroup(platformServices.GetName());
		string userId = platformServices.GetUserId();
		string currentLanguage = Language.main.GetCurrentLanguage();
		int num = Mathf.FloorToInt(SaveLoadManager.main.timePlayedTotal);
		if (string.IsNullOrEmpty(timeCapsulePlatformGroup))
		{
			Debug.LogError("Time Capsule spawn error - platform value is null or empty");
			spawnQueue.Clear();
			spawnRoutine = null;
			yield break;
		}
		if (string.IsNullOrEmpty(userId))
		{
			Debug.LogError("Time Capsule spawn error - platformUserId value is null or empty");
			spawnQueue.Clear();
			spawnRoutine = null;
			yield break;
		}
		if (string.IsNullOrEmpty(currentLanguage))
		{
			Debug.LogError("Time Capsule spawn error - language value is null or empty");
			spawnQueue.Clear();
			spawnRoutine = null;
			yield break;
		}
		string s = string.Format("{0}={1}&{2}={3}&{4}={5}&{6}={7}", "platform", Uri.EscapeDataString(timeCapsulePlatformGroup), "platform_user_id", Uri.EscapeDataString(userId), "language", Uri.EscapeDataString(currentLanguage), "play_time", num.ToString());
		byte[] payload = Encoding.UTF8.GetBytes(s);
		while (spawnQueue.Count > 0)
		{
			int index = spawnQueue.Count - 1;
			TimeCapsule client = spawnQueue[index];
			if (client == null)
			{
				spawnQueue.RemoveAt(index);
				continue;
			}
			bool success = false;
			bool spawn = false;
			int attempt = 0;
			while (attempt <= spawnRetryDelays.Length)
			{
				if (attempt > 0)
				{
					yield return new WaitForSecondsRealtime(spawnRetryDelays[attempt - 1]);
				}
				yield return platformServices.TryEnsureServerAccessAsync();
				if (platformServices.CanAccessServers())
				{
					using (UnityWebRequest request = MakePostRequest("https://subnautica.unknownworlds.com/api/v1/time-capsule-spawn", payload, "application/x-www-form-urlencoded", 15))
					{
						yield return request.Send();
						string text = string.Empty;
						if (!request.isNetworkError)
						{
							success = true;
							string text2 = ((request.downloadHandler != null) ? request.downloadHandler.text : null);
							if (!string.IsNullOrEmpty(text2))
							{
								if (request.responseCode == 201)
								{
									try
									{
										JsonData jsonData = JsonMapper.ToObject(text2);
										JsonData timeCapsule = jsonData["time_capsule"];
										string @string = ((IJsonWrapper)jsonData["instance_id"]).GetString();
										string id;
										string error;
										TimeCapsuleContent content = TimeCapsuleContentProvider.DeserializeContent(timeCapsule, out id, out error);
										if (!string.IsNullOrEmpty(error))
										{
											throw new ArgumentException(error);
										}
										TimeCapsuleContentProvider.Set(id, content);
										spawn = true;
										if (client != null)
										{
											client.Spawn(@string, id);
										}
									}
									catch (Exception ex)
									{
										text = $"Exception during parsing sever response Json: '{ex.ToString()}' responseText: '{text2}'";
									}
								}
							}
							else
							{
								text = "responseText is null or empty";
							}
						}
						else
						{
							text = $"request.isError {request.error}";
						}
						if (!string.IsNullOrEmpty(text))
						{
							Debug.LogErrorFormat("Time Capsule spawn error: '{0}', responseCode: '{1}'", text, request.responseCode.ToString());
						}
					}
				}
				else
				{
					Debug.LogError("Time Capsule spawn error: User cannot access servers.");
				}
				attempt++;
				if (success)
				{
					break;
				}
			}
			if (!success)
			{
				Debug.LogErrorFormat("Time Capsule spawn error - failed to spawn Time Capsule after {0} attempts.", attempt.ToString());
			}
			spawnQueue.Remove(client);
			if (client != null && !spawn)
			{
				client.DoNotSpawn();
			}
		}
		spawnRoutine = null;
	}

	private IEnumerator OpenRoutine()
	{
		PlatformUtils platformUtils = PlatformUtils.main;
		if (platformUtils == null)
		{
			Debug.LogError("Time Capsule open error - PlatformUtils is null");
			openRoutine = null;
			yield break;
		}
		PlatformServices platformServices = platformUtils.GetServices();
		if (platformServices == null)
		{
			Debug.LogError("Time Capsule open error - PlatformServices is null");
			openRoutine = null;
			yield break;
		}
		if (!platformServices.CanAccessUGC())
		{
			Debug.Log("PlayerTimeCapsule.Open - Platform user cannot access UGC. Bailing");
			openRoutine = null;
			yield break;
		}
		string platform = GetTimeCapsulePlatformGroup(platformServices.GetName());
		string platformUserId = platformServices.GetUserId();
		if (string.IsNullOrEmpty(platform))
		{
			Debug.LogError("Time Capsule open error - platform value is null or empty");
			openRoutine = null;
			yield break;
		}
		if (string.IsNullOrEmpty(platformUserId))
		{
			Debug.LogError("Time Capsule open error - platformUserId value is null or empty");
			openRoutine = null;
			yield break;
		}
		while (_openQueue.Count > 0)
		{
			int index = _openQueue.Count - 1;
			string instanceId = _openQueue[index];
			if (string.IsNullOrEmpty(instanceId))
			{
				_openQueue.RemoveAt(index);
				continue;
			}
			string s = string.Format("{0}={1}&{2}={3}&{4}={5}", "platform", Uri.EscapeDataString(platform), "platform_user_id", Uri.EscapeDataString(platformUserId), "instance_id", Uri.EscapeDataString(instanceId));
			byte[] payload = Encoding.UTF8.GetBytes(s);
			bool success = false;
			int attempt = 0;
			while (attempt <= openRetryDelays.Length)
			{
				if (attempt > 0)
				{
					yield return new WaitForSecondsRealtime(openRetryDelays[attempt - 1]);
				}
				yield return platformServices.TryEnsureServerAccessAsync();
				if (platformServices.CanAccessServers())
				{
					using (UnityWebRequest request = MakePostRequest("https://subnautica.unknownworlds.com/api/v1/time-capsule-open", payload, "application/x-www-form-urlencoded", 15))
					{
						yield return request.Send();
						string text = string.Empty;
						if (!request.isNetworkError)
						{
							success = true;
							string text2 = ((request.downloadHandler != null) ? request.downloadHandler.text : null);
							if (!string.IsNullOrEmpty(text2))
							{
								if (request.responseCode != 201)
								{
									text = $"responseCode is not 201. Response: '{text2}'";
								}
							}
							else
							{
								text = "responseText is null or empty";
							}
						}
						else
						{
							text = $"request.isError {request.error}";
						}
						if (!string.IsNullOrEmpty(text))
						{
							Debug.LogErrorFormat("Time Capsule open error: '{0}', responseCode: '{1}'", text, request.responseCode.ToString());
						}
					}
				}
				else
				{
					Debug.LogError("Time Capsule open error: User cannot access servers.");
				}
				attempt++;
				if (success)
				{
					break;
				}
			}
			if (success)
			{
				_openQueue.Remove(instanceId);
				continue;
			}
			Debug.LogErrorFormat("Time Capsule open error - failed to send Time Capsule open event after {0} attempts.", attempt.ToString());
		}
		openRoutine = null;
	}

	private IEnumerator SyncRoutine(string requestData)
	{
		PlatformUtils platformUtils = PlatformUtils.main;
		if (platformUtils == null)
		{
			Debug.LogError("Time Capsule sync error - PlatformUtils is null");
			syncRoutine = null;
			yield break;
		}
		PlatformServices platformServices = platformUtils.GetServices();
		if (platformServices == null)
		{
			Debug.LogError("Time Capsule sync error - PlatformServices is null");
			syncRoutine = null;
			yield break;
		}
		if (!platformServices.CanAccessUGC())
		{
			Debug.Log("PlayerTimeCapsule.SyncRoutine - Platform user cannot access UGC. Bailing");
			syncRoutine = null;
			yield break;
		}
		string timeCapsulePlatformGroup = GetTimeCapsulePlatformGroup(platformServices.GetName());
		string userId = platformServices.GetUserId();
		if (string.IsNullOrEmpty(timeCapsulePlatformGroup))
		{
			Debug.LogError("Time Capsule sync error - platform value is null or empty");
			syncRoutine = null;
			yield break;
		}
		if (string.IsNullOrEmpty(userId))
		{
			Debug.LogError("Time Capsule sync error - platformUserId value is null or empty");
			syncRoutine = null;
			yield break;
		}
		List<IMultipartFormSection> list = new List<IMultipartFormSection>();
		list.Add(new MultipartFormDataSection("platform", timeCapsulePlatformGroup, "text/plain"));
		list.Add(new MultipartFormDataSection("platform_user_id", userId, "text/plain"));
		if (!string.IsNullOrEmpty(requestData))
		{
			list.Add(new MultipartFormDataSection("time_capsules", requestData, "text/plain"));
		}
		byte[] array = UnityWebRequest.GenerateBoundary();
		byte[] payload = UnityWebRequest.SerializeFormSections(list, array);
		string contentType = $"multipart/form-data; boundary={Encoding.UTF8.GetString(array, 0, array.Length)}";
		bool success = false;
		int attempt = 0;
		while (attempt <= syncRetryDelays.Length)
		{
			if (attempt > 0)
			{
				yield return new WaitForSecondsRealtime(syncRetryDelays[attempt - 1]);
			}
			yield return platformServices.TryEnsureServerAccessAsync();
			if (platformServices.CanAccessServers())
			{
				using (UnityWebRequest request = MakePostRequest("https://subnautica.unknownworlds.com/api/v1/time-capsule-sync", payload, contentType, 30))
				{
					yield return request.Send();
					string text = string.Empty;
					if (!request.isNetworkError)
					{
						success = true;
						string text2 = ((request.downloadHandler != null) ? request.downloadHandler.text : null);
						if (request.responseCode == 200)
						{
							if (!string.IsNullOrEmpty(text2))
							{
								try
								{
									JsonData jsonData = JsonMapper.ToObject(text2)["time_capsules"];
									if (!jsonData.IsArray)
									{
										throw new ArgumentException(string.Format("{0} json field is not an array", "time_capsules"));
									}
									for (int i = 0; i < jsonData.Count; i++)
									{
										string id;
										string error;
										TimeCapsuleContent timeCapsuleContent = TimeCapsuleContentProvider.DeserializeContent(jsonData[i], out id, out error);
										if (string.IsNullOrEmpty(error))
										{
											string imageUrl = TimeCapsuleContentProvider.GetImageUrl(id);
											if (!string.IsNullOrEmpty(imageUrl))
											{
												ScreenshotManager.Delete(ScreenshotManager.Combine("timecapsules", imageUrl));
											}
											TimeCapsuleContentProvider.Set(id, timeCapsuleContent);
											if (timeCapsuleContent.isActive)
											{
												PDAEncyclopedia.UpdateTimeCapsule(id);
											}
											else
											{
												PDAEncyclopedia.RemoveTimeCapsule(id);
											}
										}
										else
										{
											Debug.LogErrorFormat("Error deserializing TimeCapsule content: {0}", error);
										}
									}
								}
								catch (Exception ex)
								{
									text = $"Exception during parsing sever response Json: '{ex.ToString()}' responseText: '{text2}'";
								}
							}
						}
						else
						{
							text = $"responseCode is not 201. Response: {text2}";
						}
					}
					else
					{
						text = $"request.isError {request.error}";
					}
					if (!string.IsNullOrEmpty(text))
					{
						Debug.LogErrorFormat("Time Capsule sync error: '{0}', responseCode: '{1}'", text, request.responseCode.ToString());
					}
				}
			}
			else
			{
				Debug.LogError("Time Capsule sync error: User cannot access servers.");
			}
			attempt++;
			if (success)
			{
				break;
			}
		}
		if (!success)
		{
			Debug.LogErrorFormat("Time Capsule sync error - failed to update Time Capsule content after {0} attempts.", attempt.ToString());
		}
		syncRoutine = null;
	}

	private void OnSubmitResult(bool success, string status)
	{
		Hint hint = Hint.main;
		if (!(hint == null))
		{
			uGUI_PopupMessage message = hint.message;
			message.ox = 60f;
			message.oy = 0f;
			message.anchor = TextAnchor.MiddleLeft;
			message.SetBackgroundColor(new Color(1f, 1f, 1f, 1f));
			message.SetText(status, TextAnchor.MiddleLeft);
			message.Show(3f);
		}
	}

	private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
	{
		if (pickupable == null)
		{
			return false;
		}
		TechType techType = pickupable.GetTechType();
		if (CraftData.IsAllowed(techType) && !blacklist.Contains(techType))
		{
			return true;
		}
		if (verbose)
		{
			ErrorMessage.AddError("TimeCapsuleItemNotAllowed");
		}
		return false;
	}

	private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
	{
		return true;
	}

	private void OnAddItem(InventoryItem item)
	{
	}

	private void OnRemoveItem(InventoryItem item)
	{
	}

	private UnityWebRequest MakePostRequest(string url, byte[] payload, string contentType, int timeout)
	{
		UploadHandlerRaw uploadHandlerRaw = new UploadHandlerRaw(payload);
		uploadHandlerRaw.contentType = contentType;
		DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();
		return new UnityWebRequest
		{
			url = url,
			method = "POST",
			uploadHandler = uploadHandlerRaw,
			downloadHandler = downloadHandler,
			disposeUploadHandlerOnDispose = true,
			disposeDownloadHandlerOnDispose = true,
			redirectLimit = 32,
			timeout = timeout,
			chunkedTransfer = true,
			useHttpContinue = true
		};
	}

	private void NotifySubmitStatus(bool success, string status)
	{
		if (submitListener != null)
		{
			submitListener(success, status);
		}
	}

	private void NotifyTextureChanged(Texture2D texture)
	{
		if (onTextureChanged != null)
		{
			onTextureChanged(texture);
		}
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	[SuppressMessage("Subnautica.Rules", "AvoidStringConcatenation")]
	[SuppressMessage("Subnautica.Rules", "EnsureLocalDisposalRule")]
	private string getSha256Hash(string data)
	{
		byte[] bytes = Encoding.UTF8.GetBytes($"C4qHC@RgKxL)p@gw-_G3ekPV{data}");
		byte[] array = new SHA256Managed().ComputeHash(bytes);
		string text = string.Empty;
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			text += $"{b:x2}";
		}
		return text;
	}

	public void OnProgress(string fileName, float progress)
	{
	}

	public void OnDone(string fileName, Texture2D texture)
	{
		if (texture != null)
		{
			_imageTexture = MathExtensions.ScaleTexture(texture, 580, mipmap: false);
			_image = _imageTexture.EncodeToJPG(90);
			ScreenshotManager.RemoveRequest(fileName, this);
			_imageFileName = null;
			NotifyTextureChanged(_imageTexture);
		}
		else
		{
			_imageFileName = null;
		}
	}

	public void OnRemoved(string fileName)
	{
		_imageFileName = null;
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		StorageHelper.TransferItems(storageRoot.gameObject, container);
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (_openQueue.Count > 0 && openRoutine == null)
		{
			openRoutine = StartCoroutine(OpenRoutine());
		}
	}

	private string GetDebugInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("submitRoutine: {0}\n", (submitRoutine != null) ? submitRoutine.ToString() : "Null");
		stringBuilder.AppendFormat("spawnRoutine: {0}, spawnQueue: {1}\n", (spawnRoutine != null) ? spawnRoutine.ToString() : "Null", spawnQueue.Count.ToString());
		stringBuilder.AppendFormat("openRoutine: {0}, _openQueue: {1}\n", (openRoutine != null) ? openRoutine.ToString() : "Null", (_openQueue != null) ? _openQueue.Count.ToString() : "Null");
		stringBuilder.AppendFormat("syncRoutine: {0}", (syncRoutine != null) ? syncRoutine.ToString() : "Null");
		stringBuilder.AppendFormat("TimceCapsule isValid: {0}\n", IsValid().ToString());
		stringBuilder.AppendFormat("_image: {0}\n", (_image != null) ? $"{_image.Length.ToString()} bytes" : "Null");
		stringBuilder.AppendFormat("_imageTexture: {0}\n", (_imageTexture != null) ? _imageTexture.name : "Null");
		stringBuilder.AppendFormat("_imageFileName: {0}\n", _imageFileName);
		return stringBuilder.ToString();
	}
}
