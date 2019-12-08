using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class XmlErrorResponseWriter : IErrorResponseWriter
	{
		private readonly static System.Text.Encoding Encoding;

		private readonly XmlWriterSettings xmlWriterSettings;

		static XmlErrorResponseWriter()
		{
			XmlErrorResponseWriter.Encoding = System.Text.Encoding.UTF8;
		}

		public XmlErrorResponseWriter()
		{
			this.xmlWriterSettings = new XmlWriterSettings()
			{
				CheckCharacters = false,
				ConformanceLevel = ConformanceLevel.Fragment,
				Encoding = XmlErrorResponseWriter.Encoding,
				Indent = true,
				NewLineHandling = NewLineHandling.Entitize
			};
		}

		private XmlWriter CreateXmlWriterAndWriteProcessingInstruction(Stream stream)
		{
			XmlWriter xmlWriter = XmlWriter.Create(stream, this.xmlWriterSettings);
			xmlWriter.WriteProcessingInstruction("xml", string.Concat("version=\"1.0\" encoding=\"", XmlErrorResponseWriter.Encoding.WebName, "\" standalone=\"yes\""));
			return xmlWriter;
		}

		private static void SerializeXmlError(XmlWriter writer, NephosErrorDetails errorDetails, Exception errorException, bool useVerboseErrors)
		{
			NephosStatusEntry statusEntry = errorDetails.StatusEntry;
			writer.WriteStartElement("error", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
			XmlErrorResponseWriter.WriteElementString(writer, "code", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", statusEntry.StatusId);
			writer.WriteStartElement("message", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
			writer.WriteAttributeString("xml", "lang", null, "en-US");
			writer.WriteString(errorDetails.UserSafeErrorMessage);
			writer.WriteEndElement();
			if (useVerboseErrors)
			{
				XmlErrorResponseWriter.SerializeXmlException(writer, errorException);
			}
			writer.WriteEndElement();
		}

		private static void SerializeXmlException(XmlWriter writer, Exception exception)
		{
			string str = "innererror";
			int num = 0;
			while (exception != null)
			{
				writer.WriteStartElement(str, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
				num++;
				string message = exception.Message ?? string.Empty;
				string fullName = exception.GetType().FullName;
				string stackTrace = exception.StackTrace ?? string.Empty;
				XmlErrorResponseWriter.WriteElementString(writer, "message", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", message);
				XmlErrorResponseWriter.WriteElementString(writer, "type", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", fullName);
				XmlErrorResponseWriter.WriteElementString(writer, "stacktrace", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", stackTrace);
				exception = exception.InnerException;
				str = "internalexception";
			}
			while (num > 0)
			{
				writer.WriteEndElement();
				num--;
			}
		}

		public void Write(MemoryStream stream, NephosErrorDetails errorDetails, Exception errorException, bool useVerboseErrors)
		{
			using (XmlWriter xmlWriter = this.CreateXmlWriterAndWriteProcessingInstruction(stream))
			{
				XmlErrorResponseWriter.SerializeXmlError(xmlWriter, errorDetails, errorException, useVerboseErrors);
			}
		}

		private static void WriteElementString(XmlWriter writer, string elementName, string ns, string text)
		{
			writer.WriteStartElement(elementName, ns);
			writer.WriteString(text);
			writer.WriteEndElement();
		}
	}
}