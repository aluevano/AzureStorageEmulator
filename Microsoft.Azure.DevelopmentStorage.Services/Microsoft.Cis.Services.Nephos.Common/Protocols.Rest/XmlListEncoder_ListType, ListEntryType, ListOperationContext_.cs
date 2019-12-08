using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public abstract class XmlListEncoder<ListType, ListEntryType, ListOperationContext>
	where ListType : IEnumerable
	{
		public const int MemoryBufferSizeForListOutput = 262144;

		private XmlWriterSettings xmlWriterSettings;

		private bool isCompletingManually;

		private AccumulatorStream outStreamAccumulator;

		private XmlWriter xmlWriter;

		public int TotalCount
		{
			get;
			private set;
		}

		public XmlListEncoder()
		{
			this.xmlWriterSettings.NewLineHandling = NewLineHandling.Entitize;
		}

		public XmlListEncoder(bool isCompletingManually)
		{
			this.xmlWriterSettings.NewLineHandling = NewLineHandling.Entitize;
			this.isCompletingManually = isCompletingManually;
		}

		private static bool AccumulatorStreamNeedsFlush(AccumulatorStream accStream, int bufferSize)
		{
			return (double)accStream.BytesAccumulated > (double)bufferSize * 0.9;
		}

		public IAsyncResult BeginEncodeListsToStream(Uri requestUrl, IList<ListType> results, IList<ListOperationContext> listOperationContexts, string rootElement, Stream outStream, TimeSpan timeout, AsyncCallback callback, object state)
		{
			this.TotalCount = 0;
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("XmlListEncoder.EncodeListsToStream", callback, state);
			asyncIteratorContext.Begin(this.EncodeListsToStreamImpl(requestUrl, results, listOperationContexts, rootElement, outStream, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginEncodeListToStream(Uri requestUrl, ListType result, ListOperationContext loc, Stream outStream, TimeSpan timeout, AsyncCallback callback, object state)
		{
			this.TotalCount = 0;
			List<ListType> listTypes = new List<ListType>(1);
			List<ListOperationContext> listOperationContexts = new List<ListOperationContext>(1);
			listTypes.Add(result);
			listOperationContexts.Add(loc);
			return this.BeginEncodeListsToStream(requestUrl, listTypes, listOperationContexts, null, outStream, timeout, callback, state);
		}

		public void Complete()
		{
			NephosAssertionException.Assert(this.outStreamAccumulator != null, "The underlying AccumulatorStream must not be null!");
			NephosAssertionException.Assert(this.xmlWriter != null, "The underlying XML Writer must not be null!");
			this.xmlWriter.Close();
			this.outStreamAccumulator.Dispose();
			this.xmlWriter = null;
			this.outStreamAccumulator = null;
		}

		protected abstract void EncodeEndElements(XmlWriter xmlWriter, ListType result, ListOperationContext loc);

		protected abstract void EncodeEntry(Uri requestUrl, ListOperationContext loc, ListEntryType entry, XmlWriter xmlWriter);

		protected abstract void EncodeInitialElements(Uri requestUrl, ListOperationContext loc, ListType result, XmlWriter xmlWriter);

		protected IEnumerator<IAsyncResult> EncodeListsToStreamImpl(Uri requestUrl, IList<ListType> results, IList<ListOperationContext> listOperationContexts, string rootElement, Stream outStream, TimeSpan timeout, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult innerStream;
			NephosAssertionException.Assert(results.Count == listOperationContexts.Count, "The number of lists did not match the number of list operation contexts.");
			Duration startingNow = Duration.StartingNow;
			int bytesAccumulated = 0;
			if (this.outStreamAccumulator != null)
			{
				NephosAssertionException.Assert(this.xmlWriter != null, "The underlying XML writer must not be null if the underlying AccumulatorStream is not null.");
			}
			else
			{
				NephosAssertionException.Assert(this.xmlWriter == null, "The underlying XML writer must be null if the underlying AccumulatorStream is null.");
				this.outStreamAccumulator = new AccumulatorStream(outStream, 262144, false);
				this.xmlWriter = XmlWriter.Create(this.outStreamAccumulator, this.xmlWriterSettings);
			}
			bool flag = !string.IsNullOrEmpty(rootElement);
			if (!flag)
			{
				NephosAssertionException.Assert(results.Count == 1, "If there is no root element specified, only one list must be being encoded.");
			}
			else
			{
				this.xmlWriter.WriteStartElement(rootElement);
			}
			int num = 0;
			foreach (ListType listType in results)
			{
				ListOperationContext item = listOperationContexts[num];
				this.EncodeInitialElements(requestUrl, item, listType, this.xmlWriter);
				foreach (ListEntryType listEntryType in listType)
				{
					XmlListEncoder<ListType, ListEntryType, ListOperationContext> totalCount = this;
					totalCount.TotalCount = totalCount.TotalCount + 1;
					this.EncodeEntry(requestUrl, item, listEntryType, this.xmlWriter);
					if (!XmlListEncoder<ListType, ListEntryType, ListOperationContext>.AccumulatorStreamNeedsFlush(this.outStreamAccumulator, 262144))
					{
						continue;
					}
					bytesAccumulated += this.outStreamAccumulator.BytesAccumulated;
					innerStream = this.outStreamAccumulator.BeginFlushToInnerStream(startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("HttpRestProcessor.EncodeListsToStreamImpl"));
					yield return innerStream;
					this.outStreamAccumulator.EndFlushToInnerStream(innerStream);
				}
				this.EncodeEndElements(this.xmlWriter, listType, item);
				num++;
			}
			if (flag)
			{
				this.xmlWriter.WriteEndElement();
			}
			this.xmlWriter.Flush();
			bytesAccumulated += this.outStreamAccumulator.BytesAccumulated;
			innerStream = this.outStreamAccumulator.BeginFlushToInnerStream(startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("HttpRestProcessor.EncodeListsToStreamImpl"));
			yield return innerStream;
			this.outStreamAccumulator.EndFlushToInnerStream(innerStream);
			IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			verbose.Log("Finished flushing the whole list data into the underlying stream. Total bytes: {0}", new object[] { bytesAccumulated });
			if (!this.isCompletingManually)
			{
				this.Complete();
			}
		}

		public void EndEncodeListsToStream(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndEncodeListToStream(IAsyncResult asyncResult)
		{
			this.EndEncodeListsToStream(asyncResult);
		}
	}
}