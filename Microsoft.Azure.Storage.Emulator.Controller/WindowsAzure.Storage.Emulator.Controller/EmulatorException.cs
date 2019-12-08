using System;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	[Serializable]
	public class EmulatorException : Exception
	{
		private const string ErrorCodePrefix = "EmulatorErrorMessage";

		public Microsoft.WindowsAzure.Storage.Emulator.Controller.EmulatorErrorCode EmulatorErrorCode
		{
			get;
			private set;
		}

		public EmulatorException()
		{
			this.EmulatorErrorCode = Microsoft.WindowsAzure.Storage.Emulator.Controller.EmulatorErrorCode.UnknownError;
		}

		public EmulatorException(Microsoft.WindowsAzure.Storage.Emulator.Controller.EmulatorErrorCode errorCode) : this(errorCode, string.Empty)
		{
		}

		public EmulatorException(Microsoft.WindowsAzure.Storage.Emulator.Controller.EmulatorErrorCode errorCode, string message) : this(errorCode, message, null)
		{
		}

		public EmulatorException(Microsoft.WindowsAzure.Storage.Emulator.Controller.EmulatorErrorCode errorCode, Exception inner) : this(errorCode, string.Empty, inner)
		{
		}

		public EmulatorException(Microsoft.WindowsAzure.Storage.Emulator.Controller.EmulatorErrorCode errorCode, string message, Exception inner) : base(EmulatorException.GetFormatedMessage(errorCode, message), inner)
		{
			this.EmulatorErrorCode = errorCode;
		}

		protected EmulatorException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.EmulatorErrorCode = (Microsoft.WindowsAzure.Storage.Emulator.Controller.EmulatorErrorCode)info.GetValue("this.EmulatorErrorCode", typeof(Microsoft.WindowsAzure.Storage.Emulator.Controller.EmulatorErrorCode));
		}

		private static string GetFormatedMessage(Microsoft.WindowsAzure.Storage.Emulator.Controller.EmulatorErrorCode errorCode, string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				return message;
			}
			return Resource.ResourceManager.GetString(string.Format("{0}{1}", "EmulatorErrorMessage", errorCode.ToString()));
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.EmulatorErrorCode", this.EmulatorErrorCode);
			base.GetObjectData(info, context);
		}
	}
}