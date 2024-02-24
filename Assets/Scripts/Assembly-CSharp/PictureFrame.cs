using System;
using System.Collections;
using System.IO;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class PictureFrame : MonoBehaviour, IScreenshotClient, IProtoEventListener
{
	private enum State
	{
		None = 0,
		Thumbnail = 1,
		Full = 2
	}

	[AssertNotNull]
	public Renderer imageRenderer;

	public float distance = 2f;

	public float updateInterval = 3f;

	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 2;

	[NonSerialized]
	[ProtoMember(2)]
	public string fileName;

	private Transform tr;

	private RectScaleMode mode = RectScaleMode.Envelope;

	private Coroutine routine;

	private bool playerIsNear;

	private State current;

	[AssertLocalization]
	private const string pictureFrameEditLabel = "PictureFrameEditLabel";

	[AssertLocalization]
	private const string screenshotUploadTitle = "ScreenshotUpload";

	[AssertLocalization]
	private const string screenshotUploadTooltip = "ScreenshotUploadTooltip";

	private State desired
	{
		get
		{
			if (!base.enabled || string.IsNullOrEmpty(fileName))
			{
				return State.None;
			}
			if (playerIsNear)
			{
				return State.Full;
			}
			return State.Thumbnail;
		}
	}

	private void Awake()
	{
		tr = GetComponent<Transform>();
		imageRenderer.enabled = false;
	}

	private void OnDestroy()
	{
		if (imageRenderer != null && imageRenderer.material != null)
		{
			UnityEngine.Object.Destroy(imageRenderer.material);
		}
	}

	private void OnEnable()
	{
		routine = StartCoroutine(UpdateRoutine());
	}

	private void OnDisable()
	{
		if (routine != null)
		{
			StopCoroutine(routine);
			routine = null;
		}
		SetState(State.None);
	}

	private void SetState(State newState)
	{
		if (current == newState)
		{
			return;
		}
		SetTexture(null);
		if (current == State.Full)
		{
			ScreenshotManager.RemoveRequest(fileName, this);
		}
		current = State.None;
		if (newState != 0)
		{
			Texture2D thumbnail = ScreenshotManager.GetThumbnail(fileName);
			if (thumbnail != null)
			{
				SetTexture(thumbnail);
				current = State.Thumbnail;
			}
			if (newState == State.Full)
			{
				ScreenshotManager.AddRequest(fileName, this);
			}
		}
	}

	public void SelectImage(string image)
	{
		if (!string.Equals(fileName, image, StringComparison.Ordinal))
		{
			SetState(State.None);
			fileName = image;
			SetState(desired);
		}
	}

	private IEnumerator UpdateRoutine()
	{
		yield return new WaitForSeconds(2f);
		while (true)
		{
			Player main = Player.main;
			playerIsNear = (tr.position - main.transform.position).sqrMagnitude <= distance * distance;
			Material material = imageRenderer.material;
			if (material != null)
			{
				Texture texture = material.GetTexture(ShaderPropertyID._MainTex);
				if (current == State.Thumbnail && texture == null)
				{
					SetState(State.None);
					fileName = null;
				}
			}
			SetState(desired);
			yield return new WaitForSeconds(updateInterval);
		}
	}

	private void SetTexture(Texture2D texture)
	{
		if (texture == null)
		{
			imageRenderer.enabled = false;
			return;
		}
		imageRenderer.enabled = true;
		Material material = imageRenderer.material;
		if (material != null)
		{
			Vector3 localScale = imageRenderer.transform.localScale;
			MathExtensions.RectFit(texture.width, texture.height, localScale.x, localScale.y, mode, out var scale, out var offset);
			material.SetTexture(ShaderPropertyID._MainTex, texture);
			material.SetTextureScale(ShaderPropertyID._MainTex, scale);
			material.SetTextureOffset(ShaderPropertyID._MainTex, offset);
		}
	}

	void IScreenshotClient.OnProgress(string fileName, float progress)
	{
	}

	void IScreenshotClient.OnDone(string fileName, Texture2D texture)
	{
		if (texture != null)
		{
			current = State.Full;
			SetTexture(texture);
		}
		else
		{
			SetState(State.None);
			this.fileName = null;
			ScreenshotManager.RemoveRequest(fileName, this);
		}
	}

	void IScreenshotClient.OnRemoved(string fileName)
	{
		SetState(State.None);
		this.fileName = null;
	}

	public void OnHandHover(HandTargetEventData eventData)
	{
		if (base.enabled)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "PictureFrameEditLabel", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Interact);
		}
	}

	public void OnHandClick(HandTargetEventData eventData)
	{
		if (base.enabled)
		{
			PDA pDA = Player.main.GetPDA();
			(pDA.ui.GetTab(PDATab.Gallery) as uGUI_GalleryTab).SetSelectListener(SelectImage, "ScreenshotUpload", "ScreenshotUploadTooltip");
			pDA.Open(PDATab.Gallery, tr);
		}
	}

	void IProtoEventListener.OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	void IProtoEventListener.OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (version < 2 && !string.IsNullOrEmpty(fileName))
		{
			fileName = ScreenshotManager.Combine("screenshots", Path.GetFileName(fileName));
		}
		version = 2;
	}
}
