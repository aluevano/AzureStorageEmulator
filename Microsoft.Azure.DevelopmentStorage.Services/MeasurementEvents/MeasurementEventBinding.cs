using System;

namespace MeasurementEvents
{
	public abstract class MeasurementEventBinding : IDisposable
	{
		protected MeasurementEventBinding()
		{
		}

		public void Dispose()
		{
			this.Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public abstract void QueueRecordEvent<TEvent>(string eventProducer, TEvent ev, DateTime endTime)
		where TEvent : MeasurementEvent<TEvent>;
	}
}