using System;
using System.Collections;
using UnityEngine;

public class CoroutineTween : IEnumerator
{
	public enum Mode : byte
	{
		Once = 0,
		Loop = 1,
		PingPong = 2
	}

	public delegate void OnStart();

	public delegate void OnUpdate(float scalar);

	public delegate void OnStop();

	public Mode mode;

	public Func<float> deltaTimeProvider;

	public bool ignoreTimeScale;

	public OnStart onStart;

	public OnUpdate onUpdate;

	public OnStop onStop;

	private MonoBehaviour _container;

	private bool _isRunning;

	private float _time;

	private float _duration = 1f;

	public float duration
	{
		get
		{
			return _duration;
		}
		set
		{
			if (value < 0f)
			{
				value = 0f;
			}
			if (_duration != value)
			{
				float num = ((_duration > 0f) ? (_time / _duration) : 0f);
				_duration = value;
				_time = num * _duration;
			}
		}
	}

	public float time => _time;

	public bool isRunning => _isRunning;

	public object Current => null;

	public CoroutineTween(MonoBehaviour container)
	{
		if (container == null)
		{
			throw new ArgumentException("CoroutineTween container cannot be null!");
		}
		_container = container;
	}

	public bool MoveNext()
	{
		if (!_isRunning)
		{
			return false;
		}
		if (_duration <= 0f)
		{
			return true;
		}
		_time += ((deltaTimeProvider != null) ? deltaTimeProvider() : uGUI.DefaultDeltaTimeProvider());
		if (mode == Mode.Once)
		{
			if (_time > _duration)
			{
				_time = _duration;
			}
			float scalar = _time / _duration;
			NotifyUpdate(scalar);
			if (_time < _duration)
			{
				return true;
			}
		}
		else
		{
			if (mode == Mode.Loop)
			{
				_time %= _duration;
				float scalar2 = _time / _duration;
				NotifyUpdate(scalar2);
				return true;
			}
			if (mode == Mode.PingPong)
			{
				_time %= _duration;
				float num = _time / _duration;
				float scalar3 = (num - Mathf.Floor(num - 0.5f)) * 2f - 2f;
				NotifyUpdate(scalar3);
				return true;
			}
		}
		_isRunning = false;
		NotifyStop();
		return false;
	}

	public void Reset()
	{
	}

	public void Start()
	{
		Stop();
		_time = 0f;
		if (!(_container == null) && _container.gameObject.activeInHierarchy)
		{
			_isRunning = true;
			NotifyStart();
			_container.StartCoroutine(this);
		}
	}

	public void Stop()
	{
		if (_isRunning)
		{
			if (_container != null)
			{
				_container.StopCoroutine(this);
			}
			_isRunning = false;
			NotifyStop();
		}
	}

	private void NotifyStart()
	{
		if (onStart != null)
		{
			onStart();
		}
	}

	private void NotifyUpdate(float scalar)
	{
		if (onUpdate != null)
		{
			onUpdate(scalar);
		}
	}

	private void NotifyStop()
	{
		if (onStop != null)
		{
			onStop();
		}
	}
}
