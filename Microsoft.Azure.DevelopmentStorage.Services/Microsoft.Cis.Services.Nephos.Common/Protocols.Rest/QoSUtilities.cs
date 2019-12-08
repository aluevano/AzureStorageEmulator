using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public static class QoSUtilities
	{
		public static MeasurementEventStatus TransformNephosStatusForAuthenticatedRequests(MeasurementEventStatus status, bool isAnonymous, bool completedWithinSla)
		{
			MeasurementEventStatus anonymousSuccess = status;
			if (!isAnonymous)
			{
				anonymousSuccess = status;
			}
			else if (status.Equals(NephosRESTEventStatus.Success))
			{
				anonymousSuccess = NephosRESTEventStatus.AnonymousSuccess;
			}
			else if (status.Equals(NephosRESTEventStatus.UnknownFailure))
			{
				anonymousSuccess = NephosRESTEventStatus.AnonymousUnknownFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.ExpectedFailure))
			{
				anonymousSuccess = NephosRESTEventStatus.AnonymousExpectedFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.TimeoutFailure))
			{
				anonymousSuccess = NephosRESTEventStatus.AnonymousTimeoutFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.ExpectedTimeoutFailure))
			{
				anonymousSuccess = NephosRESTEventStatus.AnonymousExpectedTimeoutFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.ThrottlingFailure))
			{
				anonymousSuccess = NephosRESTEventStatus.AnonymousThrottlingFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.ExpectedThrottlingFailure))
			{
				anonymousSuccess = NephosRESTEventStatus.AnonymousExpectedThrottlingFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.SuccessCacheHit))
			{
				anonymousSuccess = NephosRESTEventStatus.AnonymousSuccessCacheHit;
			}
			else if (status.Equals(NephosRESTEventStatus.ConditionNotMetFailure))
			{
				anonymousSuccess = NephosRESTEventStatus.AnonymousConditionNotMetFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.NetworkFailure))
			{
				anonymousSuccess = NephosRESTEventStatus.AnonymousNetworkFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.ProtocolFailure))
			{
				anonymousSuccess = NephosRESTEventStatus.AnonymousProtocolFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.AuthorizationFailure))
			{
				anonymousSuccess = NephosRESTEventStatus.AnonymousAuthorizationFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.PermissionFailureQuota))
			{
				anonymousSuccess = NephosRESTEventStatus.PermissionFailureQuota;
			}
			else if (status.Equals(NephosRESTEventStatus.PermissionFailureSAS))
			{
				anonymousSuccess = NephosRESTEventStatus.PermissionFailureSAS;
			}
			else if (!status.Equals(NephosRESTEventStatus.AnonymousExpectedGetPreconditionFailure))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Billing: TransformNephosStatusForAuthenticatedRequests some unrecognized failure before authentication with status code {0}", new object[] { status });
				anonymousSuccess = NephosRESTEventStatus.AnonymousUnknownFailure;
			}
			else
			{
				anonymousSuccess = NephosRESTEventStatus.AnonymousExpectedGetPreconditionFailure;
			}
			return anonymousSuccess;
		}

		public static MeasurementEventStatus TransformNephosStatusForPreAuthFailures(MeasurementEventStatus status)
		{
			MeasurementEventStatus unexpectedAuthenticationTimeoutFailure = status;
			if (status.Equals(NephosRESTEventStatus.TimeoutFailure))
			{
				unexpectedAuthenticationTimeoutFailure = NephosRESTEventStatus.UnexpectedAuthenticationTimeoutFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.ProtocolFailure))
			{
				unexpectedAuthenticationTimeoutFailure = NephosRESTEventStatus.PreauthenticationFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.IPThrottlingFailure))
			{
				unexpectedAuthenticationTimeoutFailure = NephosRESTEventStatus.IPThrottlingFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.AuthenticationFailure))
			{
				unexpectedAuthenticationTimeoutFailure = NephosRESTEventStatus.AuthenticationFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.ThrottlingFailure))
			{
				unexpectedAuthenticationTimeoutFailure = NephosRESTEventStatus.UnexpectedAuthenticationThrottleFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.ExpectedThrottlingFailure))
			{
				unexpectedAuthenticationTimeoutFailure = NephosRESTEventStatus.ExpectedThrottlingFailure;
			}
			else if (status.Equals(NephosRESTEventStatus.PermissionFailureQuota))
			{
				unexpectedAuthenticationTimeoutFailure = NephosRESTEventStatus.PermissionFailureQuota;
			}
			else if (status.Equals(NephosRESTEventStatus.PermissionFailureSAS))
			{
				unexpectedAuthenticationTimeoutFailure = NephosRESTEventStatus.PermissionFailureSAS;
			}
			else if (!status.Equals(NephosRESTEventStatus.AuthorizationFailure))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Billing: TransformNephosStatusForPreAuthFailures some unrecognized failure before authentication with status code {0}", new object[] { status });
				unexpectedAuthenticationTimeoutFailure = NephosRESTEventStatus.PreauthenticationFailure;
			}
			else
			{
				unexpectedAuthenticationTimeoutFailure = NephosRESTEventStatus.PreauthenticationFailure;
			}
			return unexpectedAuthenticationTimeoutFailure;
		}

		public static MeasurementEventStatus TransformNephosStatusForPreOperationFailures(MeasurementEventStatus status, bool isAnonymous, bool completedWithinSla)
		{
			MeasurementEventStatus measurementEventStatu = status;
			measurementEventStatu = (!status.Equals(NephosRESTEventStatus.AuthenticationFailure) ? QoSUtilities.TransformNephosStatusForAuthenticatedRequests(status, isAnonymous, completedWithinSla) : NephosRESTEventStatus.AuthenticationFailure);
			return measurementEventStatu;
		}
	}
}