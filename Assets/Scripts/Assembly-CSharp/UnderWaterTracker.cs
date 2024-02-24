using UnityEngine;

public class UnderWaterTracker : MonoBehaviour
{
	private bool _isUnderWater;

	public bool isUnderWater => _isUnderWater;

	public void UpdateWaterState()
	{
		bool flag = false;
		flag = ((!(Player.main != null)) ? (base.transform.position.y < 0f) : Player.main.IsUnderwater());
		if (flag != isUnderWater)
		{
			_isUnderWater = flag;
			if (_isUnderWater)
			{
				base.gameObject.SendMessage("OnWaterEnter", this, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				base.gameObject.SendMessage("OnWaterExit", this, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void FixedUpdate()
	{
		UpdateWaterState();
	}
}
