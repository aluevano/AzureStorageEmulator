using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBaseBlobContainer : IContainer, IDisposable
	{
		IAsyncResult BeginAcquireLease(LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state);

		IAsyncResult BeginBreakLease(TimeSpan? leaseBreakPeriod, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state);

		IAsyncResult BeginChangeLease(Guid leaseId, Guid proposedLeaseId, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state);

		IAsyncResult BeginCreateContainer(DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncCallback callback, object state);

		IAsyncResult BeginDeleteContainer(IContainerCondition conditions, Guid? leaseId, AsyncCallback callback, object state);

		IAsyncResult BeginReleaseLease(Guid leaseId, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state);

		IAsyncResult BeginRenewLease(LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state);

		void EndAcquireLease(IAsyncResult asyncResult);

		void EndBreakLease(IAsyncResult asyncResult);

		void EndChangeLease(IAsyncResult asyncResult);

		void EndCreateContainer(IAsyncResult ar);

		void EndDeleteContainer(IAsyncResult ar);

		void EndReleaseLease(IAsyncResult asyncResult);

		void EndRenewLease(IAsyncResult asyncResult);
	}
}