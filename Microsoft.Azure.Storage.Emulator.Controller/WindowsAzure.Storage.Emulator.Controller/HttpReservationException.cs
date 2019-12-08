using System;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	[Serializable]
	internal class HttpReservationException : Exception
	{
		private string message;

		public override string Message
		{
			get
			{
				return this.message;
			}
		}

		internal HttpReservationException(string message)
		{
			this.message = message;
		}
	}
}