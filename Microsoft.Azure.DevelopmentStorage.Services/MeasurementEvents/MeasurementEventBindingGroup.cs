using System;
using System.Collections.Generic;

namespace MeasurementEvents
{
	public static class MeasurementEventBindingGroup
	{
		private static object syncObj;

		private static List<MeasurementEventBinding> bindings;

		static MeasurementEventBindingGroup()
		{
			MeasurementEventBindingGroup.syncObj = new object();
			MeasurementEventBindingGroup.bindings = new List<MeasurementEventBinding>();
		}

		public static void AddBinding(MeasurementEventBinding binding)
		{
			lock (MeasurementEventBindingGroup.syncObj)
			{
				MeasurementEventBindingGroup.bindings = new List<MeasurementEventBinding>(MeasurementEventBindingGroup.bindings)
				{
					binding
				};
			}
		}

		public static void ClearBindings()
		{
			lock (MeasurementEventBindingGroup.syncObj)
			{
				MeasurementEventBindingGroup.bindings = new List<MeasurementEventBinding>();
			}
		}

		public static void RecordEvent<TEvent>(string eventProducer, TEvent ev, DateTime endTime)
		where TEvent : MeasurementEvent<TEvent>
		{
			foreach (MeasurementEventBinding binding in MeasurementEventBindingGroup.bindings)
			{
				binding.QueueRecordEvent<TEvent>(eventProducer, ev, endTime);
			}
		}

		public static void RemoveBinding(MeasurementEventBinding binding)
		{
			lock (MeasurementEventBindingGroup.syncObj)
			{
				List<MeasurementEventBinding> measurementEventBindings = new List<MeasurementEventBinding>(MeasurementEventBindingGroup.bindings);
				measurementEventBindings.Remove(binding);
				MeasurementEventBindingGroup.bindings = measurementEventBindings;
			}
		}
	}
}