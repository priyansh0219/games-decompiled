using System;
using System.Collections.Generic;
using UnityEngine;

namespace UWE
{
	public class ConditionRules : MonoBehaviour
	{
		public delegate bool ConditionFunction();

		public delegate void HandlerFunction();

		public delegate void BoolHandlerFunction(bool value);

		public class SustainHandler
		{
			public HandlerFunction func;

			public float sustainTime;

			public bool sustainValue;
		}

		public abstract class Rule
		{
			public abstract void Update(float dt);
		}

		public class ChangeRule<T> : Rule where T : IEquatable<T>
		{
			public delegate T EvalFunction();

			public delegate T OnChangeFunction(T prevValue, T currValue);

			public EvalFunction evaler;

			public OnChangeFunction onChange;

			public T prevValue;

			public T currValue;

			public ChangeRule(EvalFunction evaler, OnChangeFunction onChange)
			{
				this.evaler = evaler;
				this.onChange = onChange;
				currValue = evaler();
			}

			public override void Update(float dt)
			{
				prevValue = currValue;
				currValue = evaler();
				if (!prevValue.Equals(currValue))
				{
					onChange(prevValue, currValue);
				}
			}
		}

		public class BoolRule : Rule
		{
			public ConditionFunction func;

			public bool prevValue;

			public bool currValue;

			public HandlerFunction whenBecomesTrue;

			public HandlerFunction whenBecomesFalse;

			public BoolHandlerFunction whenChanges;

			public List<SustainHandler> sustainHandlers = new List<SustainHandler>();

			private float[] sustainTimes = new float[2];

			public static int Bool2Id(bool value)
			{
				if (!value)
				{
					return 0;
				}
				return 1;
			}

			public override void Update(float dt)
			{
				prevValue = currValue;
				currValue = func();
				if (!prevValue && currValue)
				{
					sustainTimes[1] = 0f;
					if (whenBecomesTrue != null)
					{
						whenBecomesTrue();
					}
					if (whenChanges != null)
					{
						whenChanges(currValue);
					}
				}
				else if (prevValue && !currValue)
				{
					sustainTimes[0] = 0f;
					if (whenBecomesFalse != null)
					{
						whenBecomesFalse();
					}
					if (whenChanges != null)
					{
						whenChanges(currValue);
					}
				}
				for (int i = 0; i < sustainHandlers.Count; i++)
				{
					SustainHandler sustainHandler = sustainHandlers[i];
					if (sustainHandler.sustainValue == currValue)
					{
						float num = sustainTimes[Bool2Id(currValue)];
						if (num < sustainHandler.sustainTime && num + dt >= sustainHandler.sustainTime)
						{
							sustainHandler.func();
						}
					}
				}
				sustainTimes[Bool2Id(currValue)] += dt;
			}
		}

		public class BoolRuleInterface
		{
			private BoolRule cond;

			public BoolRuleInterface(BoolRule cond)
			{
				this.cond = cond;
			}

			public BoolRuleInterface WhenBecomesTrue(HandlerFunction func)
			{
				cond.whenBecomesTrue = func;
				return this;
			}

			public BoolRuleInterface WhenBecomesFalse(HandlerFunction func)
			{
				cond.whenBecomesFalse = func;
				return this;
			}

			public BoolRuleInterface WhenChanges(BoolHandlerFunction func)
			{
				cond.whenChanges = func;
				return this;
			}

			public BoolRuleInterface WhenSustainedFor(bool value, float time, HandlerFunction func)
			{
				SustainHandler sustainHandler = new SustainHandler();
				sustainHandler.sustainTime = time;
				sustainHandler.sustainValue = value;
				sustainHandler.func = func;
				cond.sustainHandlers.Add(sustainHandler);
				return this;
			}

			public BoolRuleInterface WhenTrueFor(float time, HandlerFunction func)
			{
				return WhenSustainedFor(value: true, time, func);
			}

			public BoolRuleInterface WhenFalseFor(float time, HandlerFunction func)
			{
				return WhenSustainedFor(value: false, time, func);
			}

			public bool Get()
			{
				return cond.currValue;
			}

			public bool GetPrev()
			{
				return cond.prevValue;
			}
		}

		private List<Rule> rules = new List<Rule>();

		public BoolRuleInterface AddCondition(ConditionFunction func)
		{
			BoolRule boolRule = new BoolRule();
			boolRule.func = func;
			boolRule.currValue = func();
			rules.Add(boolRule);
			return new BoolRuleInterface(boolRule);
		}

		public void AddChangeRule<T>(ChangeRule<T>.EvalFunction evaler, ChangeRule<T>.OnChangeFunction onChange) where T : IEquatable<T>
		{
			ChangeRule<T> item = new ChangeRule<T>(evaler, onChange);
			rules.Add(item);
		}

		private void Update()
		{
			for (int i = 0; i < rules.Count; i++)
			{
				rules[i].Update(Time.deltaTime);
			}
		}
	}
}
