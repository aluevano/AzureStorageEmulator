using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default construcor taking no arguments is explicitly omitted since the block ID must be provided.")]
	public class InvalidBlockIdProtocolException : ProtocolException
	{
		private string blockId;

		private int? lineNumber;

		private int? linePosition;

		public string BlockId
		{
			get
			{
				return this.blockId;
			}
		}

		public int? LineNumber
		{
			get
			{
				return this.lineNumber;
			}
		}

		public int? LinePosition
		{
			get
			{
				return this.linePosition;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return BlobStatusEntries.InvalidBlockId;
			}
		}

		public InvalidBlockIdProtocolException(string blockId, IXmlLineInfo lineInfo) : this(blockId, lineInfo, null)
		{
		}

		public InvalidBlockIdProtocolException(string blockId, IXmlLineInfo lineInfo, Exception innerException) : this(blockId, (lineInfo != null ? new int?(lineInfo.LineNumber) : null), (lineInfo != null ? new int?(lineInfo.LinePosition) : null), innerException)
		{
		}

		private InvalidBlockIdProtocolException(string blockId, int? lineNum, int? linePos, Exception inner) : base((!lineNum.HasValue || !linePos.HasValue ? string.Format(CultureInfo.InvariantCulture, "Block Id {0} is invalid.", new object[] { blockId }) : string.Format(CultureInfo.InvariantCulture, "Block Id {0} is invalid. Line Info: ({1}, {2}).", new object[] { blockId, lineNum.Value, linePos.Value })), inner)
		{
			this.blockId = blockId;
			this.lineNumber = lineNum;
			this.linePosition = linePos;
		}

		protected InvalidBlockIdProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.blockId = info.GetString("this.blockId");
			this.lineNumber = (int?)info.GetValue("this.lineNumber", typeof(int?));
			this.linePosition = (int?)info.GetValue("this.linePosition", typeof(int?));
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			NameValueCollection nameValueCollection = new NameValueCollection(3)
			{
				{ "InvalidBlockId", this.BlockId }
			};
			if (this.LineNumber.HasValue)
			{
				int value = this.LineNumber.Value;
				nameValueCollection.Add("LineNumber", value.ToString(CultureInfo.InvariantCulture));
			}
			if (this.LinePosition.HasValue)
			{
				int num = this.LinePosition.Value;
				nameValueCollection.Add("LinePosition", num.ToString(CultureInfo.InvariantCulture));
			}
			return nameValueCollection;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.blockId", this.blockId);
			info.AddValue("this.lineNumber", this.lineNumber);
			info.AddValue("this.linePosition", this.linePosition);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidBlockIdProtocolException(this.BlockId, this.LineNumber, this.LinePosition, this);
		}
	}
}