using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MeasurementEvents
{
	public abstract class MeasurementEvent<TEvent> : IMeasurementEvent, IDisposable
	where TEvent : MeasurementEvent<TEvent>
	{
		private readonly DateTime startTime;

		private bool completed;

		private bool disposed;

		private TimeSpan completedDuration;

		private TimeSpan completedProcessingTime;

		private MeasurementEventStatus completedStatus;

		private bool completeOnlyOnDisposal;

		private readonly static MeasurementEventMetadata<TEvent> staticMetadata;

		private readonly MeasurementEventMetadata<TEvent> metadata;

		public bool Completed
		{
			get
			{
				return this.completed;
			}
		}

		[MeasurementEventParameter]
		public TimeSpan Duration
		{
			get
			{
				return this.completedDuration;
			}
		}

		public int EventId
		{
			get
			{
				return this.metadata.EventId;
			}
		}

		public string EventName
		{
			get
			{
				return this.metadata.EventName;
			}
		}

		public string EventProducer
		{
			get
			{
				return this.metadata.EventProducer;
			}
		}

		internal MeasurementEventMetadata<TEvent> Metadata
		{
			get
			{
				return this.metadata;
			}
		}

		[MeasurementEventParameter]
		public TimeSpan ProcessingTime
		{
			get
			{
				return this.completedProcessingTime;
			}
		}

		[MeasurementEventParameter]
		public MeasurementEventStatus Status
		{
			get
			{
				return this.completedStatus;
			}
		}

		static MeasurementEvent()
		{
			MeasurementEvent<TEvent>.staticMetadata = new MeasurementEventMetadata<TEvent>();
		}

		protected MeasurementEvent()
		{
		}

		public void Complete(MeasurementEventStatus status)
		{
			DateTime utcNow = DateTime.UtcNow;
			this.Complete(status, utcNow, utcNow - this.startTime, utcNow - this.startTime, false);
		}

		public void Complete(MeasurementEventStatus status, TimeSpan duration)
		{
			this.Complete(status, DateTime.UtcNow, duration, DateTime.UtcNow - this.startTime, false);
		}

		public void Complete(MeasurementEventStatus status, TimeSpan duration, TimeSpan processingTime)
		{
			this.Complete(status, DateTime.UtcNow, duration, processingTime, false);
		}

		private void Complete(MeasurementEventStatus status, DateTime endTime, TimeSpan duration, TimeSpan processingTime, bool disposing)
		{
			if (this.completed && !disposing)
			{
				throw new InvalidOperationException("Event.Complete called twice on the same event instance.");
			}
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(TEvent).FullName);
			}
			this.completedStatus = status;
			this.completed = true;
			this.completedDuration = (duration < TimeSpan.Zero ? TimeSpan.Zero : duration);
			this.completedProcessingTime = (processingTime < TimeSpan.Zero ? TimeSpan.Zero : processingTime);
			if (!this.completeOnlyOnDisposal || disposing)
			{
				MeasurementEventBindingGroup.RecordEvent<TEvent>(this.Metadata.EventProducer, (TEvent)this, endTime);
			}
		}

		internal void CompleteOnlyOnDisposal()
		{
			if (this.completed)
			{
				throw new InvalidOperationException("Event.CompleteOnlyOnDisposal called after event instance already completed");
			}
			if (this.disposed)
			{
				throw new ObjectDisposedException(typeof(TEvent).FullName);
			}
			this.completeOnlyOnDisposal = true;
		}

		[SuppressMessage("Microsoft.Design", "CA1063", Justification="We are explicitly *not* implementing the full dispose pattern here. Specifically, we aren't declaring a finalizer, because we don't want the overhead associated with them.  Therefore, we don't need to use the full pattern.")]
		public void Dispose()
		{
			if (!this.disposed)
			{
				if (!this.completed || this.completeOnlyOnDisposal)
				{
					DateTime utcNow = DateTime.UtcNow;
					this.Complete(this.completedStatus, utcNow, utcNow - this.startTime, this.completedProcessingTime, true);
				}
				this.disposed = true;
			}
		}

		protected string GenerateObjectKeyFrom(params string[] args)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string[] strArrays = args;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				stringBuilder.AppendFormat("/{0}", strArrays[i]);
			}
			return stringBuilder.ToString();
		}

		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
		public static MeasurementEvent<TEvent>.NumericParameterAccessor GetNumericParameterAccessor(string parameterName)
		{
			return MeasurementEvent<TEvent>.staticMetadata.GetNumericParameterAccessor(parameterName);
		}

		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
		public static MeasurementEvent<TEvent>.StringParameterAccessor GetStringParameterAccessor(string parameterName)
		{
			return MeasurementEvent<TEvent>.staticMetadata.GetStringParameterAccessor(parameterName);
		}

		public delegate double NumericParameterAccessor(TEvent ev);

		public delegate string StringParameterAccessor(TEvent ev);
	}
}