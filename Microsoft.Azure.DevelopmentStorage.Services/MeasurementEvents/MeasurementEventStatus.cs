using System;
using System.Diagnostics.CodeAnalysis;

namespace MeasurementEvents
{
	public struct MeasurementEventStatus
	{
		private string status;

		public readonly static MeasurementEventStatus UnknownFailure;

		static MeasurementEventStatus()
		{
			MeasurementEventStatus.UnknownFailure = new MeasurementEventStatus("UnknownFailure");
		}

		public MeasurementEventStatus(string status)
		{
			if (string.IsNullOrEmpty(status))
			{
				throw new ArgumentNullException("status");
			}
			this.status = status;
		}

		[SuppressMessage("Microsoft.Design", "CA1062", Justification="I don't see what FxCop is complaining about here.  We *are* validating <obj>")]
		public override bool Equals(object obj)
		{
			if (!(obj is MeasurementEventStatus) && !(obj is string))
			{
				return false;
			}
			return this.ToString().Equals(obj.ToString());
		}

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		[SuppressMessage("Microsoft.Naming", "CA1704")]
		public static bool operator ==(MeasurementEventStatus a, MeasurementEventStatus b)
		{
			return a.Equals(b);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704")]
		public static bool operator !=(MeasurementEventStatus a, MeasurementEventStatus b)
		{
			return !a.Equals(b);
		}

		public override string ToString()
		{
			if (this.status == null)
			{
				return "UnknownFailure";
			}
			return this.status;
		}
	}
}