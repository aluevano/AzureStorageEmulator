using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class ContainerLeaseInfo : ILeaseInfo
	{
		private Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer m_container;

		private DateTime m_InfoValidAt;

		private LeaseType m_LeaseType;

		private LeaseState? m_LeaseState;

		public TimeSpan? Duration
		{
			get
			{
				return new TimeSpan?(this.m_container.LeaseDuration);
			}
		}

		public DateTime? EndTime
		{
			get
			{
				return this.m_container.LeaseEndTime;
			}
		}

		public Guid? Id
		{
			get
			{
				return this.m_container.LeaseId;
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

		public ContainerLeaseInfo(Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer container, DateTime utcTime)
		{
			this.m_container = container;
			this.m_InfoValidAt = utcTime;
			this.SetLeaseState();
		}

		internal void SetBlobContainer(Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer container, DateTime utcTime)
		{
			this.m_container = container;
			this.m_InfoValidAt = utcTime;
			this.SetLeaseState();
		}

		private void SetLeaseState()
		{
			switch (this.m_container.LeaseState)
			{
				case 0:
				{
					this.m_LeaseState = new LeaseState?(LeaseState.Available);
					this.m_LeaseType = LeaseType.None;
					return;
				}
				case 1:
				{
					if (this.m_container.LeaseEndTime.Value <= this.m_InfoValidAt)
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
					if (this.m_container.LeaseEndTime.Value <= this.m_InfoValidAt)
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