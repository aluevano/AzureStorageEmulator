using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification="IDisposable is inherited via IContainer (via IBaseBlobContainer), implemented in Container")]
	public class BaseBlobContainer : Container, IBaseBlobContainer, IContainer, IDisposable
	{
		public override Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType ContainerType
		{
			get
			{
				return this.InternalContainer.ContainerType;
			}
		}

		protected new IBaseBlobContainer InternalContainer
		{
			get
			{
				return (IBaseBlobContainer)base.InternalContainer;
			}
		}

		public override Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get
			{
				return this.InternalContainer.OperationStatus;
			}
			set
			{
				this.InternalContainer.OperationStatus = value;
			}
		}

		public override Microsoft.Cis.Services.Nephos.Common.ProviderInjection ProviderInjection
		{
			get
			{
				return this.InternalContainer.ProviderInjection;
			}
			set
			{
				this.InternalContainer.ProviderInjection = value;
			}
		}

		internal BaseBlobContainer(IBaseBlobContainer container) : base(container)
		{
		}

		private IEnumerator<IAsyncResult> AcquireLeaseImpl(LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.InternalContainer.BeginAcquireLease(leaseType, leaseDuration, proposedLeaseId, Helpers.Convert(condition), updateLastModificationTime, useContainerNotFoundError, context.GetResumeCallback(), context.GetResumeState("BaseBlobContainer.AcquireLeaseImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndAcquireLease(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		public IAsyncResult BeginAcquireLease(LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("BaseBlobContainer.AcquireLease", callback, state);
			asyncIteratorContext.Begin(this.AcquireLeaseImpl(leaseType, leaseDuration, proposedLeaseId, condition, updateLastModificationTime, useContainerNotFoundError, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginBreakLease(TimeSpan? leaseBreakPeriod, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("BaseBlobContainer.BreakLease", callback, state);
			asyncIteratorContext.Begin(this.BreakLeaseImpl(leaseBreakPeriod, condition, updateLastModificationTime, useContainerNotFoundError, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginChangeLease(Guid leaseId, Guid proposedLeaseId, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("BaseBlobContainer.ChangeLease", callback, state);
			asyncIteratorContext.Begin(this.ChangeLeaseImpl(leaseId, proposedLeaseId, condition, updateLastModificationTime, useContainerNotFoundError, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginCreateContainer(DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("BaseBlobContainer.CreateContainer", callback, state);
			asyncIteratorContext.Begin(this.CreateContainerImpl(expiryTime, serviceMetadata, applicationMetadata, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginDeleteContainer(IContainerCondition conditions, Guid? leaseId, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("BaseBlobContainer.DeleteContainer", callback, state);
			asyncIteratorContext.Begin(this.DeleteContainerImpl(conditions, leaseId, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginReleaseLease(Guid leaseId, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("BaseBlobContainer.ReleaseLease", callback, state);
			asyncIteratorContext.Begin(this.ReleaseLeaseImpl(leaseId, condition, updateLastModificationTime, useContainerNotFoundError, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginRenewLease(LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("BaseBlobContainer.RenewLease", callback, state);
			asyncIteratorContext.Begin(this.RenewLeaseImpl(leaseType, leaseId, leaseDuration, condition, updateLastModificationTime, useContainerNotFoundError, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private IEnumerator<IAsyncResult> BreakLeaseImpl(TimeSpan? leaseBreakPeriod, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.InternalContainer.BeginBreakLease(leaseBreakPeriod, Helpers.Convert(condition), updateLastModificationTime, useContainerNotFoundError, context.GetResumeCallback(), context.GetResumeState("BaseBlobContainer.BreakLeaseImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndBreakLease(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> ChangeLeaseImpl(Guid leaseId, Guid proposedLeaseId, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.InternalContainer.BeginChangeLease(leaseId, proposedLeaseId, Helpers.Convert(condition), updateLastModificationTime, useContainerNotFoundError, context.GetResumeCallback(), context.GetResumeState("BaseBlobContainer.ChangeLeaseImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndChangeLease(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> CreateContainerImpl(DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { expiryTime, serviceMetadata, applicationMetadata, base.Timeout };
			verboseDebug.Log("CreateContainerImpl({0},{1},{2},{3})", objArray);
			try
			{
				asyncResult = this.InternalContainer.BeginCreateContainer(StorageStampHelpers.AdjustNullableDatetimeRange(expiryTime), serviceMetadata, applicationMetadata, context.GetResumeCallback(), context.GetResumeState("BaseBlobContainer.CreateContainerImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndCreateContainer(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> DeleteContainerImpl(IContainerCondition conditions, Guid? leaseId, AsyncIteratorContext<NoResults> context)
		{
			object obj;
			IAsyncResult asyncResult;
			IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			object[] timeout = new object[2];
			object[] objArray = timeout;
			obj = (leaseId.HasValue ? leaseId.ToString() : "<null>");
			objArray[0] = obj;
			timeout[1] = base.Timeout;
			verboseDebug.Log("DeleteContainerImpl({0},{1})", timeout);
			try
			{
				asyncResult = this.InternalContainer.BeginDeleteContainer(Helpers.Convert(conditions), leaseId, context.GetResumeCallback(), context.GetResumeState("BaseBlobContainer.DeleteBlobContainerImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndDeleteContainer(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		public void EndAcquireLease(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndBreakLease(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndChangeLease(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndCreateContainer(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndDeleteContainer(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndReleaseLease(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndRenewLease(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		private IEnumerator<IAsyncResult> ReleaseLeaseImpl(Guid leaseId, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.InternalContainer.BeginReleaseLease(leaseId, Helpers.Convert(condition), updateLastModificationTime, useContainerNotFoundError, context.GetResumeCallback(), context.GetResumeState("BaseBlobContainer.ReleaseLeaseImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndReleaseLease(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> RenewLeaseImpl(LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.InternalContainer.BeginRenewLease(leaseType, leaseId, leaseDuration, Helpers.Convert(condition), updateLastModificationTime, useContainerNotFoundError, context.GetResumeCallback(), context.GetResumeState("BaseBlobContainer.RenewLeaseImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndRenewLease(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}
	}
}