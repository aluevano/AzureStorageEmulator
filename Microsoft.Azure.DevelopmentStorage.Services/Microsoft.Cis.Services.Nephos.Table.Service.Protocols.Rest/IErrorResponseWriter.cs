using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;
using System.IO;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public interface IErrorResponseWriter
	{
		void Write(MemoryStream stream, NephosErrorDetails errorDetails, Exception errorException, bool useVerboseErrors);
	}
}