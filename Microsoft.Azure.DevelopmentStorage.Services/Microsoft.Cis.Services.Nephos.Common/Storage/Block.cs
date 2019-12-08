using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class Block : IBlock
	{
		private byte[] identifier;

		private long length;

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="byte[] is the standard type for the block identifier")]
		public byte[] Identifier
		{
			get
			{
				return this.identifier;
			}
		}

		public long Length
		{
			get
			{
				return this.length;
			}
		}

		public Block(byte[] identifier, long length)
		{
			this.identifier = identifier;
			this.length = length;
		}
	}
}