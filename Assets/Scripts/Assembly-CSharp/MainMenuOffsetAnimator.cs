using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuOffsetAnimator : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public Transform target;

	public Vector3 offset;

	public float speed = 1f;

	private bool hovered;

	private Vector3 origin;

	private void Awake()
	{
		origin = target.localPosition;
	}

	private void Update()
	{
		Vector3 b = (hovered ? (origin + offset) : origin);
		target.localPosition = Vector3.Lerp(target.localPosition, b, speed * Time.deltaTime);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hovered = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hovered = false;
	}
}
