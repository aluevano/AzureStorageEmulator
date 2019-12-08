using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public enum RestMethod
	{
		Unknown,
		[SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId="Member")]
		GET,
		[SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId="Member")]
		PUT,
		[SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId="Member")]
		POST,
		[SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId="Member")]
		MERGE,
		[SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId="Member")]
		DELETE,
		[SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId="Member")]
		HEAD,
		[SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId="Member")]
		OPTIONS,
		[SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId="Member")]
		TRACE,
		[SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId="Member")]
		CONNECT,
		[SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId="Member")]
		PATCH
	}
}