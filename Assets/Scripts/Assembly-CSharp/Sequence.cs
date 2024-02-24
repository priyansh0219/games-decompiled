using UnityEngine;

public class Sequence
{
	private float _t;

	private float _time;

	private bool _target;

	private float dir;

	private bool _active;

	private SequenceCallback callback;

	public float t => _t;

	public bool active => _active;

	public bool target => _target;

	public float time => _time;

	public Sequence()
	{
		Reset();
	}

	public Sequence(bool initialState)
	{
		Set(0f, initialState, initialState);
	}

	public void Reset()
	{
		_t = 0f;
		_time = 1f;
		_target = true;
		dir = 1f;
		_active = false;
		callback = null;
	}

	public void ForceState(bool state)
	{
		if (state)
		{
			_t = 1f;
			_target = true;
			dir = 1f;
		}
		else
		{
			_t = 0f;
			_target = false;
			dir = -1f;
		}
		_time = 1f;
		callback = null;
		_active = true;
	}

	public void Set(float time, bool current, bool target, SequenceCallback callback = null)
	{
		_t = (current ? 1f : 0f);
		Set(time, target, callback);
	}

	public void Set(float time, bool target, SequenceCallback callback = null)
	{
		_time = time;
		_target = target;
		dir = (target ? 1f : (-1f));
		this.callback = callback;
		_active = true;
	}

	public void SetT(float t)
	{
		_t = Mathf.Clamp01(t);
	}

	public void Update(float dT)
	{
		if (!_active)
		{
			return;
		}
		if (_t == (_target ? 1f : 0f))
		{
			_active = false;
			if (callback != null)
			{
				callback();
			}
		}
		else if (_time == 0f)
		{
			_t = (_target ? 1f : 0f);
		}
		else
		{
			_t = Mathf.Clamp01(_t + dir * dT / _time);
		}
	}

	public void Update()
	{
		Update(Time.deltaTime);
	}
}
