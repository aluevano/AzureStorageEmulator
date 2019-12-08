using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Data.OData;
using System;
using System.Collections.Specialized;
using System.IO;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	internal class ODataErrorResponseWriter : IErrorResponseWriter
	{
		public ODataErrorResponseWriter()
		{
		}

		public void Write(MemoryStream stream, NephosErrorDetails errorDetails, Exception errorException, bool useVerboseErrors)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (errorDetails == null)
			{
				throw new ArgumentNullException("errorDetails");
			}
			string item = null;
			string str = null;
			if (errorDetails.ResponseHeaders != null)
			{
				item = errorDetails.ResponseHeaders["Content-Type"];
				str = errorDetails.ResponseHeaders["DataServiceVersion"];
			}
			if (string.IsNullOrWhiteSpace(item))
			{
				throw new ArgumentException(string.Format("errorDetails should contain a valid value for the key [{0}] in ResponseHeader.", "Content-Type"));
			}
			if (string.IsNullOrWhiteSpace(str))
			{
				throw new ArgumentException(string.Format("errorDetails should contain a valid value for the key [{0}] in ResponseHeader.", "MaxDataServiceVersion"));
			}
			ODataError oDataError = new ODataError()
			{
				ErrorCode = errorDetails.StatusEntry.StatusId,
				Message = errorDetails.UserSafeErrorMessage,
				MessageLanguage = "en-US",
				InnerError = new ODataInnerError(errorException)
			};
			ODataMessageWriterSettings oDataMessageWriterSetting = new ODataMessageWriterSettings()
			{
				DisableMessageStreamDisposal = true
			};
			ResponseMessage responseMessage = new ResponseMessage(stream);
			responseMessage.SetHeader("MaxDataServiceVersion", str);
			responseMessage.SetHeader("Content-Type", item);
			using (ODataMessageWriter oDataMessageWriter = new ODataMessageWriter(responseMessage, oDataMessageWriterSetting))
			{
				oDataMessageWriter.WriteError(oDataError, useVerboseErrors);
			}
		}
	}
}