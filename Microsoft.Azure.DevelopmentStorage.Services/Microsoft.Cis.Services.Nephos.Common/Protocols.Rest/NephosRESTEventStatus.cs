using MeasurementEvents;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public static class NephosRESTEventStatus
	{
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus Success;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus SuccessOutsideSLA;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus UnexpectedSuccess;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus UnknownFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus PreauthenticationFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus UnexpectedAuthenticationFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus UnexpectedAuthenticationThrottleFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus UnexpectedAuthenticationTimeoutFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AuthenticationFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AuthorizationFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus ConditionNotMetFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus ExpectedFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus ProtocolFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus TimeoutFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus NetworkFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus SuccessCacheHit;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus ExpectedTimeoutFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus ThrottlingFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus ExpectedThrottlingFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus IPThrottlingFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus PermissionFailureQuota;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus PermissionFailureSAS;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousSuccess;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousSuccessOutsideSLA;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousUnexpectedSuccess;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousUnknownFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousExpectedFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousTimeoutFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousExpectedTimeoutFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousThrottlingFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousExpectedThrottlingFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousAuthorizationFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousConditionNotMetFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousProtocolFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousNetworkFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousSuccessCacheHit;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus AnonymousExpectedGetPreconditionFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus UnexpectedLocationServiceFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus UnexpectedStampFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus UnexpectedStampUnavailableFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus UnexpectedStampTimeoutFailure;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="According to FxCop, this message can be excluded if the field is in fact immutable.  It is.")]
		public readonly static MeasurementEventStatus UnexpectedStampThrottleFailure;

		static NephosRESTEventStatus()
		{
			NephosRESTEventStatus.Success = new MeasurementEventStatus("Success");
			NephosRESTEventStatus.SuccessOutsideSLA = new MeasurementEventStatus("SuccessOutsideSLA");
			NephosRESTEventStatus.UnexpectedSuccess = new MeasurementEventStatus("UnexpectedSuccess");
			NephosRESTEventStatus.UnknownFailure = MeasurementEventStatus.UnknownFailure;
			NephosRESTEventStatus.PreauthenticationFailure = new MeasurementEventStatus("PreauthenticationFailure");
			NephosRESTEventStatus.UnexpectedAuthenticationFailure = new MeasurementEventStatus("UnexpectedAuthenticationFailure");
			NephosRESTEventStatus.UnexpectedAuthenticationThrottleFailure = new MeasurementEventStatus("UnexpectedAuthenticationThrottleFailure");
			NephosRESTEventStatus.UnexpectedAuthenticationTimeoutFailure = new MeasurementEventStatus("UnexpectedAuthenticationTimeoutFailure");
			NephosRESTEventStatus.AuthenticationFailure = new MeasurementEventStatus("AuthenticationFailure");
			NephosRESTEventStatus.AuthorizationFailure = new MeasurementEventStatus("AuthorizationFailure");
			NephosRESTEventStatus.ConditionNotMetFailure = new MeasurementEventStatus("ConditionNotMetFailure");
			NephosRESTEventStatus.ExpectedFailure = new MeasurementEventStatus("ExpectedFailure");
			NephosRESTEventStatus.ProtocolFailure = new MeasurementEventStatus("ProtocolFailure");
			NephosRESTEventStatus.TimeoutFailure = new MeasurementEventStatus("TimeoutFailure");
			NephosRESTEventStatus.NetworkFailure = new MeasurementEventStatus("NetworkFailure");
			NephosRESTEventStatus.SuccessCacheHit = new MeasurementEventStatus("SuccessCacheHit");
			NephosRESTEventStatus.ExpectedTimeoutFailure = new MeasurementEventStatus("ExpectedTimeoutFailure");
			NephosRESTEventStatus.ThrottlingFailure = new MeasurementEventStatus("ThrottlingFailure");
			NephosRESTEventStatus.ExpectedThrottlingFailure = new MeasurementEventStatus("ExpectedThrottlingFailure");
			NephosRESTEventStatus.IPThrottlingFailure = new MeasurementEventStatus("IPThrottlingFailure");
			NephosRESTEventStatus.PermissionFailureQuota = new MeasurementEventStatus("PermissionFailureQuota");
			NephosRESTEventStatus.PermissionFailureSAS = new MeasurementEventStatus("PermissionFailureSAS");
			NephosRESTEventStatus.AnonymousSuccess = new MeasurementEventStatus("AnonymousSuccess");
			NephosRESTEventStatus.AnonymousSuccessOutsideSLA = new MeasurementEventStatus("AnonymousSuccessOutsideSLA");
			NephosRESTEventStatus.AnonymousUnexpectedSuccess = new MeasurementEventStatus("AnonymousUnexpectedSuccess");
			NephosRESTEventStatus.AnonymousUnknownFailure = new MeasurementEventStatus("AnonymousUnknownFailure");
			NephosRESTEventStatus.AnonymousExpectedFailure = new MeasurementEventStatus("AnonymousExpectedFailure");
			NephosRESTEventStatus.AnonymousTimeoutFailure = new MeasurementEventStatus("AnonymousTimeoutFailure");
			NephosRESTEventStatus.AnonymousExpectedTimeoutFailure = new MeasurementEventStatus("AnonymousExpectedTimeoutFailure");
			NephosRESTEventStatus.AnonymousThrottlingFailure = new MeasurementEventStatus("AnonymousThrottlingFailure");
			NephosRESTEventStatus.AnonymousExpectedThrottlingFailure = new MeasurementEventStatus("AnonymousExpectedThrottlingFailure");
			NephosRESTEventStatus.AnonymousAuthorizationFailure = new MeasurementEventStatus("AnonymousAuthorizationFailure");
			NephosRESTEventStatus.AnonymousConditionNotMetFailure = new MeasurementEventStatus("AnonymousConditionNotMetFailure");
			NephosRESTEventStatus.AnonymousProtocolFailure = new MeasurementEventStatus("AnonymousProtocolFailure");
			NephosRESTEventStatus.AnonymousNetworkFailure = new MeasurementEventStatus("AnonymousNetworkFailure");
			NephosRESTEventStatus.AnonymousSuccessCacheHit = new MeasurementEventStatus("AnonymousSuccessCacheHit");
			NephosRESTEventStatus.AnonymousExpectedGetPreconditionFailure = new MeasurementEventStatus("AnonymousExpectedGetPreconditionFailure");
			NephosRESTEventStatus.UnexpectedLocationServiceFailure = new MeasurementEventStatus("UnexpectedLocationServiceFailure");
			NephosRESTEventStatus.UnexpectedStampFailure = new MeasurementEventStatus("UnexpectedStampFailure");
			NephosRESTEventStatus.UnexpectedStampUnavailableFailure = new MeasurementEventStatus("UnexpectedStampUnavailableFailure");
			NephosRESTEventStatus.UnexpectedStampTimeoutFailure = new MeasurementEventStatus("UnexpectedStampTimeoutFailure");
			NephosRESTEventStatus.UnexpectedStampThrottleFailure = new MeasurementEventStatus("UnexpectedStampThrottleFailure");
		}
	}
}