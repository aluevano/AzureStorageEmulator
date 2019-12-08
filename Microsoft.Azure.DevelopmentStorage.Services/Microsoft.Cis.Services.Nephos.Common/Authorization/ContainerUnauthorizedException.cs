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
	public class ContainerUnauthorizedException : Exception, IRethrowableException
	{
		private string resourceAccount;

		private string resourceContainer;

		private string resourceIdentifier;

		private IAccountIdentifier requestor;

		private Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel;

		private SASPermission signedPermission;

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

		public ContainerUnauthorizedException()
		{
		}

		public ContainerUnauthorizedException(string message) : base(message)
		{
		}

		public ContainerUnauthorizedException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public ContainerUnauthorizedException(string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel) : this(resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, SASPermission.None, null)
		{
		}

		public ContainerUnauthorizedException(string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, SASPermission signedPermission) : this(resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, signedPermission, null)
		{
		}

		public ContainerUnauthorizedException(string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, Exception innerException) : this(resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, SASPermission.None, innerException)
		{
		}

		public ContainerUnauthorizedException(string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, SASPermission signedPermission, Exception innerException) : this(string.Format(CultureInfo.InvariantCulture, "{0} does not have {1} access to resource {2}", new object[] { requestor.AccountName, permissionLevel, RealServiceManager.GetResourceString(resourceAccount, resourceContainer, resourceIdentifier) }), resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, signedPermission, innerException)
		{
		}

		public ContainerUnauthorizedException(string message, string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel) : this(message, resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, null)
		{
		}

		public ContainerUnauthorizedException(string message, string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, SASPermission signedPermission) : this(message, resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, signedPermission, null)
		{
		}

		public ContainerUnauthorizedException(string message, string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, Exception innerException) : this(message, resourceAccount, resourceContainer, resourceIdentifier, requestor, permissionLevel, SASPermission.None, innerException)
		{
		}

		public ContainerUnauthorizedException(string message, string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, Microsoft.Cis.Services.Nephos.Common.Authorization.PermissionLevel permissionLevel, SASPermission signedPermission, Exception innerException) : base(message, innerException)
		{
			this.requestor = requestor;
			this.resourceAccount = resourceAccount;
			this.resourceContainer = resourceContainer;
			this.resourceIdentifier = resourceIdentifier;
			this.permissionLevel = permissionLevel;
			this.signedPermission = signedPermission;
		}

		protected ContainerUnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context)
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
			base.GetObjectData(info, context);
		}

		public Exception GetRethrowableClone()
		{
			return new ContainerUnauthorizedException(this.Message, this.resourceAccount, this.resourceContainer, this.resourceIdentifier, this.Requestor, this.PermissionLevel, this);
		}
	}
}