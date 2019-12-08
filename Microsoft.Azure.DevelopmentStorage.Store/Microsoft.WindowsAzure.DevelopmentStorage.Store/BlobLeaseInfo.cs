using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class BlobLeaseInfo : ILeaseInfo
	{
		private Blob m_blob;

		private DateTime m_InfoValidAt;

		private LeaseType m_LeaseType;

		private LeaseState? m_LeaseState;

		public TimeSpan? Duration
		{
			get
			{
				return new TimeSpan?(this.m_blob.LeaseDuration);
			}
		}

		public DateTime? EndTime
		{
			get
			{
				return this.m_blob.LeaseEndTime;
			}
		}

		public Guid? Id
		{
			get
			{
				return this.m_blob.LeaseId;
			}
		}

		internal DateTime LeaseInfoValidAt
		{
			get
			{
				return this.m_InfoValidAt;
			}
		}

		public LeaseState? State
		{
			get
			{
				return this.m_LeaseState;
			}
		}

		public LeaseType Type
		{
			get
			{
				return this.m_LeaseType;
			}
		}

		public BlobLeaseInfo(Blob blob, DateTime utcTime)
		{
			this.m_blob = blob;
			this.m_InfoValidAt = utcTime;
			this.SetLeaseState();
		}

		internal void SetBlob(Blob blob, DateTime utcTime)
		{
			this.m_blob = blob;
			this.m_InfoValidAt = utcTime;
			this.SetLeaseState();
		}

		private void SetLeaseState()
		{
			switch (this.m_blob.LeaseState)
			{
				case 0:
				{
					this.m_LeaseState = new LeaseState?(LeaseState.Available);
					this.m_LeaseType = LeaseType.None;
					return;
				}
				case 1:
				{
					if (this.m_blob.LeaseEndTime.Value <= this.m_InfoValidAt)
					{
						this.m_LeaseState = new LeaseState?(LeaseState.Expired);
						this.m_LeaseType = LeaseType.None;
						return;
					}
					this.m_LeaseState = new LeaseState?(LeaseState.Leased);
					this.m_LeaseType = LeaseType.ReadWrite;
					return;
				}
				case 2:
				{
					this.m_LeaseState = new LeaseState?(LeaseState.Expired);
					this.m_LeaseType = LeaseType.None;
					return;
				}
				case 3:
				{
					if (this.m_blob.LeaseEndTime.Value <= this.m_InfoValidAt)
					{
						this.m_LeaseState = new LeaseState?(LeaseState.Broken);
						this.m_LeaseType = LeaseType.None;
						return;
					}
					this.m_LeaseState = new LeaseState?(LeaseState.Breaking);
					this.m_LeaseType = LeaseType.ReadWrite;
					return;
				}
				case 4:
				{
					this.m_LeaseState = new LeaseState?(LeaseState.Broken);
					this.m_LeaseType = LeaseType.None;
					return;
				}
				default:
				{
					return;
				}
			}
		}
	}
}