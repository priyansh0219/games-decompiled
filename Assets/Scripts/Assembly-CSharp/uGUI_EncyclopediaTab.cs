using System;
using System.Collections.Generic;
using System.Text;
using Gendarme;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
public class uGUI_EncyclopediaTab : uGUI_PDATab, uGUI_IListEntryManager, uGUI_INavigableIconGrid, INotificationListener, uGUI_IScrollReceiver, ICompileTimeCheckable, IScreenshotClient
{
	private class EntryComparer : IComparer<TreeNode>
	{
		public int Compare(TreeNode node1, TreeNode node2)
		{
			CraftNode craftNode = node1 as CraftNode;
			CraftNode craftNode2 = node2 as CraftNode;
			string strA = craftNode?.string1;
			string strB = craftNode2?.string1;
			return string.Compare(strA, strB);
		}
	}

	private static EntryComparer entryComparer = new EntryComparer();

	[AssertLocalization]
	private const string encyclopediaLabelKey = "EncyclopediaLabel";

	[AssertLocalization(1)]
	private const string pressToPlayKey = "PressToPlayFormat";

	[AssertLocalization(1)]
	private const string pressToStopKey = "PressToStopFormat";

	[AssertLocalization]
	private const string timeCapsuleFetchError = "TimeCapsuleContentFetchError";

	public const string popupId = "PDAEncyclopediaTab";

	private const int messageParagraphSpacingWithAudio = 80;

	[AssertNotNull]
	public CanvasGroup content;

	[AssertNotNull]
	public TextMeshProUGUI encyclopediaLabel;

	[AssertNotNull]
	public GameObject prefabEntry;

	[AssertNotNull]
	public RectTransform listCanvas;

	[AssertNotNull]
	public RectTransform contentCanvas;

	[AssertNotNull]
	public ScrollRect listScrollRect;

	[AssertNotNull]
	public ScrollRect contentScrollRect;

	[AssertNotNull]
	public RawImage image;

	[AssertNotNull]
	public LayoutElement imageLayout;

	[AssertNotNull]
	public Texture2D defaultTexture;

	[AssertNotNull]
	public Graphic progressBar;

	[AssertNotNull]
	public TextMeshProUGUI title;

	[AssertNotNull]
	public TextMeshProUGUI message;

	[AssertNotNull]
	public Sprite iconExpand;

	[AssertNotNull]
	public Sprite iconCollapse;

	[AssertNotNull]
	public GameObject audioContainer;

	[AssertNotNull]
	public TextMeshProUGUI textControlsHint;

	[AssertNotNull]
	public Image audioTimeline;

	[AssertNotNull]
	public Image audioIcon;

	[AssertNotNull]
	public Image audioIconBackground;

	[AssertNotNull]
	public Sprite iconAudioPlay;

	[AssertNotNull]
	public Sprite iconAudioStop;

	[AssertNotNull]
	public Sprite iconAudioPlayBackground;

	[AssertNotNull]
	public Sprite iconAudioStopBackground;

	[AssertNotNull]
	public Button playAudioButton;

	public float indentStep = 10f;

	public UISpriteData pathNodeSprites;

	public UISpriteData entryNodeSprites;

	private CraftNode tree = new CraftNode("Root")
	{
		string0 = string.Empty,
		string1 = string.Empty,
		monoBehaviour0 = null,
		bool0 = false,
		int0 = 0
	};

	private uGUI_ListEntry activeEntry;

	private string timeCapsuleImageFileName;

	private PrefabPool<uGUI_ListEntry> pool;

	private string audio;

	private string subtitle;

	private int audioPlaying = -1;

	private float audioPosition = -1f;

	private float audioLength = -1f;

	private StringBuilder textBuilder = new StringBuilder(1000);

	public override int notificationsCount => NotificationManager.main.GetCount(NotificationManager.Group.Encyclopedia);

	private uGUI_ListEntry selectedEntry
	{
		get
		{
			if (UISelection.HasSelection)
			{
				return UISelection.selected as uGUI_ListEntry;
			}
			return null;
		}
	}

	bool uGUI_INavigableIconGrid.ShowSelector => true;

	bool uGUI_INavigableIconGrid.EmulateRaycast => true;

	protected override void Awake()
	{
		NotificationManager.main.Subscribe(this, NotificationManager.Group.Encyclopedia, string.Empty);
		SetImage(null);
		SetAudio(null);
		PDAEncyclopedia.onUpdate = (PDAEncyclopedia.OnUpdate)Delegate.Combine(PDAEncyclopedia.onUpdate, new PDAEncyclopedia.OnUpdate(OnUpdateEntry));
		PDAEncyclopedia.onRemove = (PDAEncyclopedia.OnRemove)Delegate.Combine(PDAEncyclopedia.onRemove, new PDAEncyclopedia.OnRemove(OnRemoveEntry));
		pool = new PrefabPool<uGUI_ListEntry>(prefabEntry, listCanvas, 10, 4, delegate(uGUI_ListEntry entry)
		{
			entry.Uninitialize();
		}, delegate(uGUI_ListEntry entry)
		{
			entry.Uninitialize();
		});
	}

	private void OnDestroy()
	{
		PDAEncyclopedia.onUpdate = (PDAEncyclopedia.OnUpdate)Delegate.Remove(PDAEncyclopedia.onUpdate, new PDAEncyclopedia.OnUpdate(OnUpdateEntry));
		PDAEncyclopedia.onRemove = (PDAEncyclopedia.OnRemove)Delegate.Remove(PDAEncyclopedia.onRemove, new PDAEncyclopedia.OnRemove(OnRemoveEntry));
	}

	public override void OnOpenPDA(PDATab tab, bool explicitly)
	{
		Expand(tree);
		UpdatePositions();
		PDAEncyclopedia.onAdd = (PDAEncyclopedia.OnAdd)Delegate.Combine(PDAEncyclopedia.onAdd, new PDAEncyclopedia.OnAdd(OnAddEntry));
		if (!(tab == PDATab.Encyclopedia && explicitly))
		{
			return;
		}
		uGUI_PopupNotification main = uGUI_PopupNotification.main;
		if (!(main != null) || !(main.id == "PDAEncyclopediaTab"))
		{
			return;
		}
		string data = main.data;
		if (string.IsNullOrEmpty(data))
		{
			return;
		}
		CraftNode node = ExpandTo(data);
		if (Activate(node) && !string.IsNullOrEmpty(audio))
		{
			SoundQueue queue = PDASounds.queue;
			if (queue != null && queue.current != audio)
			{
				queue.PlayImmediately(audio);
			}
		}
		main.Hide();
	}

	public override void OnClosePDA()
	{
		PDAEncyclopedia.onAdd = (PDAEncyclopedia.OnAdd)Delegate.Remove(PDAEncyclopedia.onAdd, new PDAEncyclopedia.OnAdd(OnAddEntry));
		Collapse(tree);
	}

	public override void Open()
	{
		DeselectItem();
		content.SetVisible(visible: true);
	}

	public override void Close()
	{
		listScrollRect.velocity = Vector2.zero;
		contentScrollRect.velocity = Vector2.zero;
		DeselectItem();
		content.SetVisible(visible: false);
	}

	public override void OnUpdate(bool isOpen)
	{
		if (!isOpen)
		{
			return;
		}
		bool playing = false;
		float position = 0f;
		float length = 0f;
		if (!string.IsNullOrEmpty(audio))
		{
			SoundQueue queue = PDASounds.queue;
			if (queue != null && queue.current == audio)
			{
				playing = true;
				position = queue.positionSeconds;
				length = queue.lengthSeconds;
			}
		}
		SetAudioState(playing, position, length);
	}

	public override void OnLanguageChanged()
	{
		Language main = Language.main;
		encyclopediaLabel.text = main.Get("EncyclopediaLabel");
		PDAEncyclopedia.OnLanguageChanged();
		using (IEnumerator<CraftNode> enumerator = tree.Traverse(includeSelf: false))
		{
			while (enumerator.MoveNext())
			{
				CraftNode current = enumerator.Current;
				CraftNode node = PDAEncyclopedia.GetNode(current.id);
				string text = (current.string1 = ((node != null) ? node.string1 : string.Empty));
				uGUI_ListEntry nodeListEntry = GetNodeListEntry(current);
				if (nodeListEntry != null)
				{
					nodeListEntry.SetText(text);
				}
			}
		}
		using (IEnumerator<CraftNode> enumerator2 = tree.Traverse())
		{
			while (enumerator2.MoveNext())
			{
				enumerator2.Current.Sort(entryComparer);
			}
		}
		UpdatePositions();
		string text2 = ((activeEntry != null) ? activeEntry.Key : null);
		if (!string.IsNullOrEmpty(text2))
		{
			DisplayEntry(text2);
		}
	}

	public override uGUI_INavigableIconGrid GetInitialGrid()
	{
		return this;
	}

	bool uGUI_IListEntryManager.OnButtonDown(string key, GameInput.Button button)
	{
		if (!(tree.FindNodeById(key) is CraftNode craftNode))
		{
			return false;
		}
		GetNodeListEntry(craftNode);
		TreeAction action = craftNode.action;
		if (button == GameInput.button0)
		{
			switch (action)
			{
			case TreeAction.Expand:
				ToggleExpand(craftNode);
				break;
			case TreeAction.Craft:
				Activate(craftNode);
				break;
			}
			return true;
		}
		if (button == GameInput.button2)
		{
			ToggleAudio();
		}
		else
		{
			switch (button)
			{
			case GameInput.Button.UIRight:
				switch (action)
				{
				case TreeAction.Expand:
					if (GetNodeExpanded(craftNode))
					{
						using (IEnumerator<CraftNode> enumerator = craftNode.GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								CraftNode current = enumerator.Current;
								SelectItem(GetNodeListEntry(current));
							}
						}
					}
					else
					{
						ToggleExpand(craftNode);
					}
					break;
				case TreeAction.Craft:
					Activate(craftNode);
					break;
				}
				return true;
			case GameInput.Button.UILeft:
				switch (action)
				{
				case TreeAction.Expand:
					if (GetNodeExpanded(craftNode))
					{
						ToggleExpand(craftNode);
					}
					else if (craftNode.parent is CraftNode craftNode3 && craftNode3.action == TreeAction.Expand)
					{
						SelectItem(GetNodeListEntry(craftNode3));
					}
					break;
				case TreeAction.Craft:
					if (craftNode.parent is CraftNode craftNode2 && craftNode2.action == TreeAction.Expand)
					{
						SelectItem(GetNodeListEntry(craftNode2));
					}
					break;
				}
				return true;
			}
		}
		return false;
	}

	public void OnAddEntry(CraftNode srcNode, bool verbose)
	{
		List<TreeNode> reversedPath = srcNode.GetReversedPath(includeSelf: true);
		CraftNode craftNode = tree;
		bool flag = false;
		int num = reversedPath.Count - 1;
		while (num >= 0 && craftNode != null && GetNodeExpanded(craftNode))
		{
			srcNode = reversedPath[num] as CraftNode;
			string id = srcNode.id;
			CraftNode craftNode2 = craftNode[id] as CraftNode;
			if (craftNode2 == null)
			{
				craftNode2 = CreateNode(srcNode, craftNode);
				if (craftNode2 != null)
				{
					craftNode.Sort(entryComparer);
					flag = true;
				}
			}
			craftNode = craftNode2;
			num--;
		}
		if (flag)
		{
			UpdatePositions();
		}
	}

	[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidUnnecessarySpecializationRule")]
	public void OnUpdateEntry(CraftNode srcNode)
	{
		CraftNode dstNode = GetDstNode(srcNode);
		if (dstNode != null)
		{
			dstNode.string1 = srcNode.string1;
			uGUI_ListEntry nodeListEntry = GetNodeListEntry(dstNode);
			if (nodeListEntry != null)
			{
				nodeListEntry.SetText(srcNode.string1);
			}
			dstNode.parent.Sort(entryComparer);
			UpdatePositions();
			if (activeEntry != null && string.Equals(activeEntry.Key, srcNode.id, StringComparison.Ordinal))
			{
				DisplayEntry(srcNode.id);
			}
		}
	}

	[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidUnnecessarySpecializationRule")]
	public void OnRemoveEntry(CraftNode srcNode)
	{
		List<TreeNode> reversedPath = srcNode.GetReversedPath(includeSelf: true);
		CraftNode craftNode = tree;
		for (int num = reversedPath.Count - 1; num >= 0; num--)
		{
			if (craftNode == null)
			{
				return;
			}
			string id = reversedPath[num].id;
			craftNode = craftNode[id] as CraftNode;
		}
		if (craftNode != null)
		{
			if (activeEntry != null && string.Equals(activeEntry.Key, srcNode.id, StringComparison.Ordinal))
			{
				DisplayEntry(null);
			}
			uGUI_ListEntry nodeListEntry = GetNodeListEntry(craftNode);
			if (nodeListEntry != null)
			{
				NotificationManager.main.UnregisterTarget(nodeListEntry);
				pool.Release(nodeListEntry);
			}
			craftNode.parent.RemoveNode(craftNode);
			UpdatePositions();
		}
	}

	private CraftNode GetDstNode(CraftNode srcNode)
	{
		List<TreeNode> reversedPath = srcNode.GetReversedPath(includeSelf: true);
		CraftNode craftNode = tree;
		int num = reversedPath.Count - 1;
		while (num >= 0 && craftNode != null)
		{
			string id = reversedPath[num].id;
			craftNode = craftNode[id] as CraftNode;
			num--;
		}
		return craftNode;
	}

	private void UpdatePositions()
	{
		using (IEnumerator<CraftNode> enumerator = tree.Traverse(includeSelf: false))
		{
			int num = 0;
			while (enumerator.MoveNext())
			{
				uGUI_ListEntry nodeListEntry = GetNodeListEntry(enumerator.Current);
				if (nodeListEntry != null)
				{
					nodeListEntry.rectTransform.SetSiblingIndex(num);
				}
				num++;
			}
		}
	}

	private void DisplayEntry(string key)
	{
		textBuilder.Clear();
		if (key != null && PDAEncyclopedia.GetEntryData(key, out var entryData))
		{
			if (entryData.kind == PDAEncyclopedia.EntryData.Kind.TimeCapsule)
			{
				if (TimeCapsuleContentProvider.GetData(key, out var text, out var text2, out var imageUrl))
				{
					SetTitle(text);
					SetText(textBuilder.Append(text2));
					if (!string.IsNullOrEmpty(imageUrl))
					{
						SetImage(defaultTexture);
						SetProgress(0f);
						ScreenshotManager.AddRequest(ScreenshotManager.Combine("timecapsules", imageUrl), TimeCapsuleContentProvider.GetAbsoluteImageUrl(imageUrl), this, highPriority: true);
					}
					else
					{
						SetImage(null);
						SetProgress(-1f);
					}
				}
				else
				{
					SetTitle(entryData.key);
					SetText(textBuilder.Append(Language.main.Get("TimeCapsuleContentFetchError")));
					SetImage(null);
					SetProgress(-1f);
				}
				SetAudio(null);
			}
			else
			{
				SetTitle(Language.main.Get("Ency_" + key));
				string key2 = "EncyDesc_" + key;
				int paragraphSpacing = (entryData.audio ? 80 : 0);
				textBuilder.Append(Language.main.Get(key2));
				SetText(textBuilder, paragraphSpacing);
				SetImage(entryData.image);
				SetProgress(-1f);
				SetAudio(entryData.audio, key2);
			}
		}
		else
		{
			SetTitle(string.Empty);
			SetText(textBuilder);
			SetImage(null);
			SetProgress(-1f);
			SetAudio(null);
		}
	}

	private void SetProgress(float progress)
	{
		if (progress >= 0f)
		{
			progressBar.material.SetFloat(ShaderPropertyID._Amount, progress);
			progressBar.enabled = true;
		}
		else
		{
			progressBar.enabled = false;
		}
	}

	private void SetTitle(string value)
	{
		title.text = (string.IsNullOrEmpty(value) ? "\u200b" : value);
	}

	private void SetText(StringBuilder sb, int paragraphSpacing = 0)
	{
		if (sb.Length == 0)
		{
			sb.Append('\u200b');
		}
		message.SetText(sb);
		message.paragraphSpacing = paragraphSpacing;
	}

	private void SetImage(Texture2D texture)
	{
		if (!string.IsNullOrEmpty(timeCapsuleImageFileName))
		{
			ScreenshotManager.RemoveRequest(timeCapsuleImageFileName, this);
			timeCapsuleImageFileName = null;
		}
		image.texture = texture;
		if (texture != null)
		{
			float num = (float)texture.height / (float)texture.width;
			float num2 = image.rectTransform.rect.width * num;
			imageLayout.minHeight = num2;
			imageLayout.preferredHeight = num2;
			image.gameObject.SetActive(value: true);
		}
		else
		{
			image.gameObject.SetActive(value: false);
		}
	}

	private void SetAudio(FMODAsset asset, string subtitle = null)
	{
		SetAudioState(playing: false, 0f, 0f);
		if (asset != null && !string.IsNullOrEmpty(asset.id))
		{
			audio = asset.id;
			this.subtitle = subtitle;
			audioContainer.SetActive(value: true);
		}
		else
		{
			audio = null;
			this.subtitle = subtitle;
			audioContainer.SetActive(value: false);
		}
	}

	private void SetAudioState(bool playing, float position, float length)
	{
		int num = (playing ? 1 : 0);
		if (audioPlaying != num)
		{
			audioPlaying = num;
			if (playing)
			{
				audioIcon.sprite = iconAudioStop;
				audioIconBackground.sprite = iconAudioStopBackground;
			}
			else
			{
				audioIcon.sprite = iconAudioPlay;
				audioIconBackground.sprite = iconAudioPlayBackground;
			}
		}
		if (audioPosition != position || audioLength != length)
		{
			audioPosition = position;
			audioLength = length;
			float value = ((audioLength > 0f) ? (audioPosition / audioLength) : 0f);
			audioTimeline.material.SetFloat(ShaderPropertyID._Amount, value);
		}
		if (GameInput.IsPrimaryDeviceGamepad())
		{
			textControlsHint.enabled = true;
			textControlsHint.SetText(LanguageCache.GetButtonFormat(playing ? "PressToStopFormat" : "PressToPlayFormat", GameInput.button2));
		}
		else
		{
			textControlsHint.enabled = false;
		}
	}

	public void ToggleAudio()
	{
		if (string.IsNullOrEmpty(audio))
		{
			return;
		}
		SoundQueue queue = PDASounds.queue;
		if (queue != null)
		{
			if (queue.current == audio)
			{
				queue.Stop();
			}
			else
			{
				queue.PlayImmediately(audio);
			}
		}
	}

	public void OnTimelineClick(BaseEventData baseEventData)
	{
		if (string.IsNullOrEmpty(audio))
		{
			return;
		}
		SoundQueue queue = PDASounds.queue;
		if (queue != null && MaterialExtensions.GetBarValue(audioTimeline.rectTransform, baseEventData, audioTimeline.material, horizontal: true, out var v))
		{
			int position = Mathf.FloorToInt(v * (float)queue.length);
			if (queue.current != audio)
			{
				queue.PlayImmediately(audio);
			}
			queue.position = position;
		}
	}

	private CraftNode CreateNode(CraftNode srcNode, CraftNode parentNode)
	{
		if (srcNode == null || parentNode == null)
		{
			return null;
		}
		string id = srcNode.id;
		if (parentNode[id] != null)
		{
			return null;
		}
		TreeAction action = srcNode.action;
		Sprite sprite = null;
		int depth = parentNode.depth;
		float num = 0f;
		UISpriteData spriteData;
		switch (action)
		{
		case TreeAction.Expand:
			spriteData = pathNodeSprites;
			sprite = iconExpand;
			num = (float)(depth + 1) * indentStep;
			break;
		case TreeAction.Craft:
			spriteData = entryNodeSprites;
			sprite = null;
			num = (float)depth * indentStep;
			break;
		default:
			return null;
		}
		string @string = srcNode.string0;
		string string2 = srcNode.string1;
		uGUI_ListEntry uGUI_ListEntry2 = pool.Get();
		uGUI_ListEntry2.Initialize(this, id, spriteData);
		uGUI_ListEntry2.SetIcon(sprite);
		uGUI_ListEntry2.SetIndent(num);
		uGUI_ListEntry2.SetText(string2);
		int num2 = 0;
		CraftNode craftNode = new CraftNode(id, action)
		{
			string0 = @string,
			string1 = string2,
			monoBehaviour0 = uGUI_ListEntry2,
			bool0 = false,
			int0 = num2
		};
		parentNode.AddNode(craftNode);
		switch (action)
		{
		case TreeAction.Expand:
		{
			using (IEnumerator<CraftNode> enumerator = srcNode.Traverse(includeSelf: false))
			{
				while (enumerator.MoveNext())
				{
					CraftNode current = enumerator.Current;
					if (current.action == TreeAction.Craft && NotificationManager.main.Contains(NotificationManager.Group.Encyclopedia, current.id))
					{
						num2++;
					}
				}
			}
			SetNodeNotificationsCount(craftNode, num2);
			UpdateNotificationsCount(uGUI_ListEntry2, num2);
			break;
		}
		case TreeAction.Craft:
			NotificationManager.main.RegisterTarget(NotificationManager.Group.Encyclopedia, srcNode.id, uGUI_ListEntry2);
			break;
		}
		return craftNode;
	}

	private void ToggleExpand(CraftNode node)
	{
		if (GetNodeExpanded(node))
		{
			Collapse(node);
			return;
		}
		Expand(node);
		UpdatePositions();
	}

	private void Expand(CraftNode node)
	{
		SetNodeExpanded(node, state: true);
		uGUI_ListEntry nodeListEntry = GetNodeListEntry(node);
		if (nodeListEntry != null)
		{
			nodeListEntry.SetNotificationAlpha(0f);
			nodeListEntry.SetIcon(iconCollapse);
		}
		CraftNode node2 = PDAEncyclopedia.GetNode(node.id);
		if (node2 == null)
		{
			return;
		}
		foreach (CraftNode item in node2)
		{
			string id = item.id;
			if (!(node[id] is CraftNode node3))
			{
				CraftNode craftNode = CreateNode(item, node);
				continue;
			}
			GetNodeListEntry(node3).gameObject.SetActive(value: true);
			if (GetNodeExpanded(node3))
			{
				Expand(node3);
			}
		}
		node.Sort(entryComparer);
	}

	private CraftNode ExpandTo(string id)
	{
		if (string.IsNullOrEmpty(id) || tree == null)
		{
			return null;
		}
		CraftNode result = null;
		List<TreeNode> list = new List<TreeNode>();
		if (PDAEncyclopedia.tree.FindNodeById(id, list))
		{
			CraftNode craftNode = tree;
			for (int num = list.Count - 2; num >= 0; num--)
			{
				CraftNode craftNode2 = list[num] as CraftNode;
				string id2 = craftNode2.id;
				CraftNode craftNode3 = craftNode[id2] as CraftNode;
				if (craftNode3 == null)
				{
					craftNode3 = CreateNode(craftNode2, craftNode);
					if (craftNode3 != null)
					{
						craftNode.Sort(entryComparer);
					}
				}
				if (craftNode3.action == TreeAction.Expand && !GetNodeExpanded(craftNode3))
				{
					Expand(craftNode3);
				}
				if (num == 0)
				{
					result = craftNode3;
				}
				craftNode = craftNode3;
			}
			UpdatePositions();
		}
		return result;
	}

	private bool Activate(CraftNode node)
	{
		if (node == null || node.action != TreeAction.Craft)
		{
			return false;
		}
		uGUI_ListEntry nodeListEntry = GetNodeListEntry(node);
		bool result = false;
		if (activeEntry != nodeListEntry)
		{
			if (activeEntry != null)
			{
				activeEntry.SetSelected(state: false);
			}
			activeEntry = nodeListEntry;
			if (activeEntry != null)
			{
				DisplayEntry(node.id);
				activeEntry.SetSelected(state: true);
				result = true;
			}
		}
		return result;
	}

	private void Collapse(CraftNode node)
	{
		SetNodeExpanded(node, state: false);
		uGUI_ListEntry nodeListEntry = GetNodeListEntry(node);
		if (nodeListEntry != null)
		{
			UpdateNotificationsCount(nodeListEntry, GetNodeNotificationsCount(node));
			nodeListEntry.SetIcon(iconExpand);
		}
		using (IEnumerator<CraftNode> enumerator = node.Traverse(includeSelf: false))
		{
			while (enumerator.MoveNext())
			{
				uGUI_ListEntry nodeListEntry2 = GetNodeListEntry(enumerator.Current);
				if (nodeListEntry2 != null)
				{
					nodeListEntry2.gameObject.SetActive(value: false);
				}
			}
		}
	}

	private void Clear(CraftNode node)
	{
		if (node == null)
		{
			return;
		}
		using (IEnumerator<CraftNode> enumerator = node.Traverse(includeSelf: false))
		{
			while (enumerator.MoveNext())
			{
				uGUI_ListEntry nodeListEntry = GetNodeListEntry(enumerator.Current);
				if (nodeListEntry != null)
				{
					NotificationManager.main.UnregisterTarget(nodeListEntry);
					pool.Release(nodeListEntry);
				}
			}
		}
		node.Clear();
	}

	private static uGUI_ListEntry GetNodeListEntry(CraftNode node)
	{
		return node.monoBehaviour0 as uGUI_ListEntry;
	}

	private static bool GetNodeExpanded(CraftNode node)
	{
		return node.bool0;
	}

	private static void SetNodeExpanded(CraftNode node, bool state)
	{
		node.bool0 = state;
	}

	private static int GetNodeNotificationsCount(CraftNode node)
	{
		return node.int0;
	}

	private static void SetNodeNotificationsCount(CraftNode node, int count)
	{
		node.int0 = count;
	}

	private static void UpdateNotificationsCount(uGUI_ListEntry listEntry, int count)
	{
		if (!(listEntry == null))
		{
			if (listEntry.SetNotificationAlpha((count > 0) ? 1f : 0f))
			{
				listEntry.SetNotificationBackgroundColor(NotificationManager.notificationColor);
			}
			listEntry.SetNotificationText(IntStringCache.GetStringForInt(count));
		}
	}

	private void ChangeNotificationsCount(string key, bool increase)
	{
		if (tree == null)
		{
			return;
		}
		CraftNode node = PDAEncyclopedia.GetNode(key);
		if (node == null)
		{
			return;
		}
		List<TreeNode> reversedPath = node.GetReversedPath(includeSelf: false);
		CraftNode craftNode = tree;
		int num = reversedPath.Count - 1;
		while (num >= 0 && (craftNode = craftNode[reversedPath[num].id] as CraftNode) != null)
		{
			int nodeNotificationsCount = GetNodeNotificationsCount(craftNode);
			nodeNotificationsCount = (increase ? (nodeNotificationsCount + 1) : (nodeNotificationsCount - 1));
			SetNodeNotificationsCount(craftNode, nodeNotificationsCount);
			if (!GetNodeExpanded(craftNode))
			{
				UpdateNotificationsCount(GetNodeListEntry(craftNode), nodeNotificationsCount);
			}
			num--;
		}
	}

	void INotificationListener.OnAdd(NotificationManager.Group group, string key)
	{
		ChangeNotificationsCount(key, increase: true);
	}

	void INotificationListener.OnRemove(NotificationManager.Group group, string key)
	{
		ChangeNotificationsCount(key, increase: false);
	}

	public void OnProgress(string fileName, float progress)
	{
		SetProgress(progress);
	}

	public void OnDone(string fileName, Texture2D texture)
	{
		SetImage(texture);
		timeCapsuleImageFileName = fileName;
		SetProgress(-1f);
	}

	public void OnRemoved(string fileName)
	{
		SetImage(null);
	}

	public object GetSelectedItem()
	{
		return selectedEntry;
	}

	public Graphic GetSelectedIcon()
	{
		uGUI_ListEntry uGUI_ListEntry2 = selectedEntry;
		if (!(uGUI_ListEntry2 != null))
		{
			return null;
		}
		return uGUI_ListEntry2.background;
	}

	public void SelectItem(object item)
	{
		uGUI_ListEntry uGUI_ListEntry2 = item as uGUI_ListEntry;
		if (!(uGUI_ListEntry2 == null))
		{
			DeselectItem();
			UISelection.selected = uGUI_ListEntry2;
			uGUI_ListEntry2.OnPointerEnter(null);
			listScrollRect.ScrollTo(uGUI_ListEntry2.rectTransform, xRight: true, yUp: false, new Vector4(10f, 10f, 10f, 10f));
		}
	}

	public void DeselectItem()
	{
		if (!(selectedEntry == null))
		{
			selectedEntry.OnPointerExit(null);
			UISelection.selected = null;
		}
	}

	public bool SelectFirstItem()
	{
		if (activeEntry != null)
		{
			ExpandTo(activeEntry.Key);
			SelectItem(activeEntry);
			return true;
		}
		using (IEnumerator<CraftNode> enumerator = tree.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				SelectItem(GetNodeListEntry(enumerator.Current));
				return true;
			}
		}
		return false;
	}

	public bool SelectItemClosestToPosition(Vector3 worldPos)
	{
		return false;
	}

	public bool SelectItemInDirection(int dirX, int dirY)
	{
		if (selectedEntry == null)
		{
			return SelectFirstItem();
		}
		if (dirY == 0)
		{
			return false;
		}
		int siblingIndex = selectedEntry.transform.GetSiblingIndex();
		Transform parent = selectedEntry.transform.parent;
		int num = ((dirY > 0) ? (siblingIndex + 1) : (siblingIndex - 1));
		int num2 = ((dirY > 0) ? 1 : (-1));
		for (int i = num; i >= 0 && i < parent.childCount; i += num2)
		{
			uGUI_ListEntry component = parent.GetChild(i).GetComponent<uGUI_ListEntry>();
			if (component.gameObject.activeInHierarchy)
			{
				SelectItem(component);
				return true;
			}
		}
		return false;
	}

	public uGUI_INavigableIconGrid GetNavigableGridInDirection(int dirX, int dirY)
	{
		return null;
	}

	bool uGUI_IScrollReceiver.OnScroll(float scrollDelta, float speedMultiplier)
	{
		contentScrollRect.Scroll(scrollDelta, speedMultiplier);
		return true;
	}

	public string CompileTimeCheck()
	{
		if (prefabEntry.GetComponent<uGUI_ListEntry>() == null)
		{
			return "uGUI_EncyclopediaTab : uGUI_ListEntry component is missing on prefabEntry prefab!";
		}
		return null;
	}
}
