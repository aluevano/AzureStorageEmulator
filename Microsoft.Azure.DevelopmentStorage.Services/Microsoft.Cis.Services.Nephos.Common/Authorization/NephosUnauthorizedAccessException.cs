using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Authorization
{
	[Serializable]
	public class NephosUnauthorizedAccessException : Exception, IRethrowableException
	{
		private string resourceAccount;

		private string resourceContainer;

		private string resourceIdentifier;

		private IAccountIdentifier requestor;

		private Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel;

		private SASPermission signedPermission;

		private AuthorizationFailureReason failureReason;

		public AuthorizationFailureReason FailureReason
		{
			get
			{
				return this.failureReason;
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel PermissionLevel
		{
			get
			{
				return this.permissionLevel;
			}
		}

		public IAccountIdentifier Requestor
		{
			get
			{
				return this.requestor;
			}
		}

		public string Resource
		{
			get
			{
				return RealServiceManager.GetResourceString(this.resourceAccount, this.resourceContainer, this.resourceIdentifier);
			}
		}

		public SASPermission SignedPermission
		{
			get
			{
				return this.signedPermission;
			}
		}

		public NephosUnauthorizedAccessException()
		{
		}

		public NephosUnauthorizedAccessException(string message) : base(message)
		{
		}

		public NephosUnauthorizedAccessException(string message, AuthorizationFailureReason failureReason) : base(message)
		{
			this.failureReason = failureReason;
		}

		public NephosUnauthorizedAccessException(string message, AuthorizationFailureReason failureReason, IAccountIdentifier requestor) : base(message)
		{
			this.failureReason = failureReason;
			this.requestor = requestor;
		}

		public NephosUnauthorizedAccessException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public NephosUnauthorizedAccessException(string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, AuthorizationFailureReason failureReason) : this(resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, SASPermission.None, failureReason, null)
		{
		}

		public NephosUnauthorizedAccessException(string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, SASPermission signedPermission, AuthorizationFailureReason failureReason) : this(resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, signedPermission, failureReason, null)
		{
		}

		public NephosUnauthorizedAccessException(string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, AuthorizationFailureReason failureReason, Exception innerException) : this(resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, SASPermission.None, failureReason, innerException)
		{
		}

		public NephosUnauthorizedAccessException(string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, SASPermission signedPermission, AuthorizationFailureReason failureReason, Exception innerException) : this(string.Format(CultureInfo.InvariantCulture, "{0} does not have {1} access to resource {2}", new object[] { requestor.AccountName, permissionLevel, RealServiceManager.GetResourceString(resourceAccount, resourceContainer, resourceIdentifier) }), resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, signedPermission, innerException, failureReason)
		{
		}

		public NephosUnauthorizedAccessException(string message, string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, AuthorizationFailureReason failureReason) : this(message, resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, null, failureReason)
		{
		}

		public NephosUnauthorizedAccessException(string message, string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, SASPermission signedPermission, AuthorizationFailureReason failureReason) : this(message, resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, signedPermission, null, failureReason)
		{
		}

		public NephosUnauthorizedAccessException(string message, string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, Exception innerException, AuthorizationFailureReason failureReason) : this(message, resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, SASPermission.None, innerException, failureReason)
		{
		}

		public NephosUnauthorizedAccessException(string message, string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, SASPermission signedPermission, Exception innerException, AuthorizationFailureReason failureReason) : base(message, innerException)
		{
			this.requestor = requestor;
			this.resourceAccount = resourceAccount;
			this.resourceContainer = resourceContainer;
			this.resourceIdentifier = resourceIdentifier;
			this.permissionLevel = permissionLevel;
			this.signedPermission = signedPermission;
			this.failureReason = failureReason;
		}

		protected NephosUnauthorizedAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.requestor = (IAccountIdentifier)info.GetValue("this.requestor", typeof(IAccountIdentifier));
			this.resourceAccount = info.GetString("this.resourceAccount");
			this.resourceContainer = info.GetString("this.resourceContainer");
			this.resourceIdentifier = info.GetString("this.resourceIdentifier");
			this.permissionLevel = (Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel)info.GetValue("this.permissionLevel", typeof(Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel));
			this.signedPermission = (SASPermission)info.GetValue("this.signedPermission", typeof(SASPermission));
			this.failureReason = (AuthorizationFailureReason)info.GetValue("this.failureReason", typeof(AuthorizationFailureReason));
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.requestor", this.requestor);
			info.AddValue("this.resourceAccount", this.resourceAccount);
			info.AddValue("this.resourceContainer", this.resourceContainer);
			info.AddValue("this.resourceIdentifier", this.resourceIdentifier);
			info.AddValue("this.permissionLevel", this.permissionLevel);
			info.AddValue("this.signedPermission", this.signedPermission);
			info.AddValue("this.failureReason", this.failureReason);
			base.GetObjectData(info, context);
		}

		public Exception GetRethrowableClone()
		{
			return new NephosUnauthorizedAccessException(this.Message, this.resourceAccount, this.resourceContainer, this.resourceIdentifier, this.Requestor, this.PermissionLevel, this, this.failureReason);
		}
	}
}