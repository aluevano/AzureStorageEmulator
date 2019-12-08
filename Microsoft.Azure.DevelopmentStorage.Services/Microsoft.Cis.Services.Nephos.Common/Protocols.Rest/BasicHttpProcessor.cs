using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using Microsoft.Cis.Services.Nephos.Common.Versioning;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public abstract class BasicHttpProcessor : IProcessor, IDisposable
	{
		private bool started;

		private bool completed;

		private bool isDisposed;

		private Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext;

		private RestMethod method;

		private bool statusCodeIsSet;

		private bool canStatusCodeBeSet = true;

		private WrapperStream wrappedResponseStream;

		private bool responseIsClosed;

		private long pendingBytesToWrite;

		private DateTime startOfLastWrite = DateTime.MinValue;

		private long writtenBytes;

		private double lastDetectedRate = double.MaxValue;

		private VersionedRequestSettings requestSettings;

		public readonly static TimeSpan DefaultMaxAllowedTimeout;

		private static Regex s_charsetRegex;

		public bool Aborted
		{
			get;
			private set;
		}

		protected bool CanStatusCodeBeSet
		{
			get
			{
				return this.canStatusCodeBeSet;
			}
		}

		protected string ContainerAclAdjustedRequestRestVersion
		{
			get;
			set;
		}

		public double CurrentNetworkRate
		{
			get
			{
				if (this.lastDetectedRate != double.MaxValue || !this.WritingResponseStarted)
				{
					return this.lastDetectedRate;
				}
				double num = (double)this.pendingBytesToWrite;
				DateTime utcNow = DateTime.UtcNow;
				return num / utcNow.Subtract(this.startOfLastWrite).TotalSeconds;
			}
		}

		protected string HttpVerb
		{
			get
			{
				return this.requestContext.HttpMethod;
			}
		}

		protected virtual bool IgnoreDisposeOnResponseStream
		{
			get
			{
				return false;
			}
		}

		public bool IsRequestAnonymous
		{
			get
			{
				return !this.IsRequestAuthenticated;
			}
		}

		public bool IsRequestAnonymousAndUnversioned
		{
			get
			{
				return !this.IsRequestAuthenticatedOrAnonymousVersioned;
			}
		}

		public bool IsRequestAuthenticated
		{
			get
			{
				return this.RequestHeadersCollection["Authorization"] != null;
			}
		}

		public bool IsRequestAuthenticatedOrAnonymousVersioned
		{
			get
			{
				if (this.IsRequestAuthenticated)
				{
					return true;
				}
				if (this.IsRequestAuthenticated)
				{
					return false;
				}
				return this.RequestHeaderRestVersion != null;
			}
		}

		protected bool IsUsingHttps
		{
			get
			{
				return this.requestContext.IsSecureConnection;
			}
		}

		protected RestMethod Method
		{
			get
			{
				return this.method;
			}
		}

		public int OverallConcurrentRequestCount
		{
			get
			{
				return JustDecompileGenerated_get_OverallConcurrentRequestCount();
			}
			set
			{
				JustDecompileGenerated_set_OverallConcurrentRequestCount(value);
			}
		}

		private int JustDecompileGenerated_OverallConcurrentRequestCount_k__BackingField;

		public int JustDecompileGenerated_get_OverallConcurrentRequestCount()
		{
			return this.JustDecompileGenerated_OverallConcurrentRequestCount_k__BackingField;
		}

		public void JustDecompileGenerated_set_OverallConcurrentRequestCount(int value)
		{
			this.JustDecompileGenerated_OverallConcurrentRequestCount_k__BackingField = value;
		}

		protected virtual string Range
		{
			get;
			set;
		}

		public Encoding RequestContentEncoding
		{
			get
			{
				if (string.IsNullOrEmpty(this.RequestContentType) || !BasicHttpProcessor.s_charsetRegex.Match(this.RequestContentType).Success)
				{
					return Encoding.UTF8;
				}
				return this.requestContext.RequestContentEncoding;
			}
		}

		protected long RequestContentLength
		{
			get
			{
				return this.requestContext.RequestContentLength;
			}
		}

		protected string RequestContentType
		{
			get
			{
				return this.requestContext.RequestContentType;
			}
		}

		protected Microsoft.Cis.Services.Nephos.Common.RequestContext RequestContext
		{
			get
			{
				return this.requestContext;
			}
		}

		protected string RequestCopySource
		{
			get
			{
				return this.RequestHeadersCollection["x-ms-copy-source"];
			}
		}

		protected string RequestHeaderRestVersion
		{
			get
			{
				return this.requestContext.RequestHeaderRestVersion;
			}
		}

		protected WebHeaderCollection RequestHeadersCollection
		{
			get
			{
				return this.requestContext.RequestHeaders;
			}
		}

		protected string RequestHost
		{
			get
			{
				return this.requestContext.UserHostName;
			}
		}

		public Guid RequestId
		{
			get;
			private set;
		}

		protected Version RequestProtocolVersion
		{
			get
			{
				return this.requestContext.ProtocolVersion;
			}
		}

		protected NameValueCollection RequestQueryParameters
		{
			get
			{
				return this.requestContext.QueryParameters;
			}
		}

		protected string RequestRawUrlString
		{
			get
			{
				return this.requestContext.RequestUrlString;
			}
		}

		public string RequestRestVersion
		{
			get
			{
				if (this.SASVersion != null)
				{
					return this.SASVersion;
				}
				string requestHeaderRestVersion = this.RequestHeaderRestVersion;
				if (!string.IsNullOrEmpty(requestHeaderRestVersion))
				{
					return requestHeaderRestVersion;
				}
				if (!string.IsNullOrEmpty(this.ServiceSettingAdjustedRequestRestVersion))
				{
					return this.ServiceSettingAdjustedRequestRestVersion;
				}
				if (!string.IsNullOrEmpty(this.ContainerAclAdjustedRequestRestVersion))
				{
					return this.ContainerAclAdjustedRequestRestVersion;
				}
				return VersioningConfigurationLookup.Instance.DefaultVersion;
			}
		}

		public VersionedRequestSettings RequestSettings
		{
			get
			{
				if (this.requestSettings == null)
				{
					VersioningConfigurationLookup.Instance.TryGetSettingsForVersion(this.RequestRestVersion, out this.requestSettings);
				}
				return this.requestSettings;
			}
		}

		protected Stream RequestStream
		{
			get
			{
				return this.requestContext.InputStream;
			}
		}

		protected Uri RequestUrl
		{
			get
			{
				return this.requestContext.RequestUrl;
			}
		}

		protected string RequestVia
		{
			get
			{
				return this.RequestHeadersCollection["Via"];
			}
		}

		protected IHttpListenerResponse Response
		{
			get
			{
				if (this.isDisposed)
				{
					throw new ObjectDisposedException("BasicHttpProcessor");
				}
				return this.requestContext.Response;
			}
		}

		protected bool ResponseIsClosed
		{
			get
			{
				return this.responseIsClosed;
			}
		}

		protected Stream ResponseStream
		{
			get
			{
				if (this.wrappedResponseStream == null)
				{
					this.wrappedResponseStream = new WrapperStream(this.Response.OutputStream, false)
					{
						OnBeforeWrite = (byte[]& param0, int& param1, int& param2) => {
							if (this.canStatusCodeBeSet)
							{
								this.ResetContentLength(false);
							}
							this.canStatusCodeBeSet = false;
						},
						OnBeforeDispose = () => {
							if (this.IgnoreDisposeOnResponseStream)
							{
								Logger<IRestProtocolHeadLogger>.Instance.InfoDebug.Log(string.Concat("Ignoring Dispose on ResponseStream called at stack trace ", Environment.StackTrace));
							}
							else
							{
								NephosAssertionException.Fail("ResponseStream must not be disposed directly. This indicates a code error. Current Stack trace {0}", new object[] { Environment.StackTrace });
							}
							return false;
						}
					};
				}
				return this.wrappedResponseStream;
			}
		}

		protected string SafeRequestUrlString
		{
			get
			{
				return this.requestContext.SafeRequestUrlString;
			}
		}

		protected string SASVersion
		{
			get;
			set;
		}

		protected string ServiceSettingAdjustedRequestRestVersion
		{
			get;
			set;
		}

		public Microsoft.Cis.Services.Nephos.Common.ServiceType ServiceType
		{
			get
			{
				return this.requestContext.ServiceType;
			}
		}

		protected HttpStatusCode StatusCode
		{
			get
			{
				NephosAssertionException.Assert(this.statusCodeIsSet, "HTTP Status code is not set.");
				return (HttpStatusCode)this.Response.StatusCode;
			}
			set
			{
				NephosAssertionException.Assert(this.CanStatusCodeBeSet, "Status code is being set AFTER the response headers are sent. Possible code bug.");
				IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
				object[] objArray = new object[] { (int)value };
				verboseDebug.Log("Setting status code to {0}", objArray);
				this.Response.StatusCode = (int)value;
				this.statusCodeIsSet = true;
			}
		}

		protected bool StatusCodeIsSet
		{
			get
			{
				return this.statusCodeIsSet;
			}
		}

		protected string StatusDescription
		{
			get
			{
				return this.Response.StatusDescription;
			}
			set
			{
				this.Response.StatusDescription = value.Replace("\r\n", " ");
				Logger<IRestProtocolHeadLogger>.Instance.InfoDebug.Log("Setting status description as \"{0}\"", new object[] { this.StatusDescription });
			}
		}

		public bool WritingResponseStarted
		{
			get
			{
				if (this.pendingBytesToWrite != (long)0)
				{
					return true;
				}
				return this.writtenBytes != (long)0;
			}
		}

		static BasicHttpProcessor()
		{
			BasicHttpProcessor.DefaultMaxAllowedTimeout = TimeSpan.FromSeconds(30);
			BasicHttpProcessor.s_charsetRegex = new Regex(";[ \t]*charset=", RegexOptions.IgnoreCase);
		}

		protected BasicHttpProcessor(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext)
		{
			this.requestContext = requestContext;
			this.isDisposed = false;
		}

		protected void AbortResponse()
		{
			NephosAssertionException.Assert(!this.responseIsClosed, "Response is already closed.");
			try
			{
				try
				{
					this.PerformPreCloseTasks();
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Aborting HttpListenerResponse of this request...");
					this.Response.Abort();
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("The HttpListenerResponse of this request has been aborted successfully");
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					Logger<IRestProtocolHeadLogger>.Instance.NetworkFailure.Log("Could not abort the connection to the client: {0}", new object[] { exception.Message });
				}
			}
			finally
			{
				this.responseIsClosed = true;
			}
		}

		public IAsyncResult BeginProcess(AsyncCallback callback, object state)
		{
			IAsyncResult asyncResult;
			if (this.isDisposed)
			{
				throw new ObjectDisposedException("BasicHttpProcessor");
			}
			AsyncIteratorContext<NoResults> asyncIteratorContext = null;
			try
			{
				NephosAssertionException.Assert(!this.started, "Processor already started. You can call BeginProcess only once.");
				this.started = true;
				this.SetRequestIdForResponse();
				this.OnProcessorStarted(this, new EventArgs());
				this.method = this.TranslateHttpVerbToRestMethod(this.HttpVerb);
				asyncIteratorContext = new AsyncIteratorContext<NoResults>("BasicHttpProcessor.BeginProcess", callback, state);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("BeginProcess encountered an exception {0}", new object[] { exception });
				this.StatusCode = HttpStatusCode.InternalServerError;
				this.CloseResponse();
				throw;
			}
			try
			{
				asyncIteratorContext.Begin(this.ProcessImpl(asyncIteratorContext));
				asyncResult = asyncIteratorContext;
			}
			catch (Exception exception3)
			{
				Exception exception2 = exception3;
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("asyncContext.Begin(ProcessImpl(asyncContext)) encountered an exception before it returns: {0}", new object[] { exception2 });
				NephosAssertionException.Assert(this.StatusCodeIsSet, "StatusCode is not set In asyncContext.Begin(ProcessImpl(asyncContext))", exception2);
				NephosAssertionException.Assert(this.ResponseIsClosed, "Response is not closed In asyncContext.Begin(ProcessImpl(asyncContext))", exception2);
				throw;
			}
			return asyncResult;
		}

		protected void CloseResponse()
		{
			NephosAssertionException.Assert(!this.responseIsClosed, "Response is already closed.");
			try
			{
				this.ResetContentLength(true);
				this.PerformPreCloseTasks();
			}
			finally
			{
				try
				{
					try
					{
						Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Closing HttpListenerResponse of this request...");
						this.Response.Close();
						Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("The HttpListenerResponse of this request has been closed successfully");
					}
					catch (HttpListenerException httpListenerException1)
					{
						HttpListenerException httpListenerException = httpListenerException1;
						IStringDataEventStream networkFailure = Logger<IRestProtocolHeadLogger>.Instance.NetworkFailure;
						object[] stringForHttpListenerException = new object[] { HttpUtilities.GetStringForHttpListenerException(httpListenerException) };
						networkFailure.Log("SecurityWarning: Could not close the response: '{0}'", stringForHttpListenerException);
					}
					catch (InvalidOperationException invalidOperationException1)
					{
						InvalidOperationException invalidOperationException = invalidOperationException1;
						IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
						object[] logString = new object[] { invalidOperationException.GetLogString() };
						info.Log("Could not close the response: '{0}'", logString);
					}
				}
				finally
				{
					this.responseIsClosed = true;
				}
			}
		}

		public static List<SASIdentifier> DecodeSASIdentifiersFromStream(Stream inputStream, XmlReaderSettings readerSettings, bool allowEmpty, bool allowEmptyPolicyFields, SASPermission sasPermissionType)
		{
			List<SASIdentifier> sASIdentifiers;
			List<SASIdentifier> sASIdentifiers1 = new List<SASIdentifier>();
			try
			{
				using (XmlReader xmlReader = XmlReader.Create(inputStream, readerSettings))
				{
					IXmlLineInfo xmlLineInfo = xmlReader as IXmlLineInfo;
					xmlReader.Read();
					if (!allowEmpty || !xmlReader.IsStartElement("SignedIdentifiers") || !xmlReader.IsEmptyElement)
					{
						xmlReader.ReadStartElement("SignedIdentifiers");
						while (xmlReader.IsStartElement("SignedIdentifier"))
						{
							xmlReader.ReadStartElement("SignedIdentifier");
							string empty = string.Empty;
							string str = string.Empty;
							string empty1 = string.Empty;
							string str1 = string.Empty;
							while (xmlReader.IsStartElement("Id") || xmlReader.IsStartElement("AccessPolicy"))
							{
								if (!xmlReader.IsStartElement("Id"))
								{
									if (!xmlReader.IsStartElement("AccessPolicy"))
									{
										continue;
									}
									if (!allowEmptyPolicyFields || !xmlReader.IsEmptyElement)
									{
										xmlReader.ReadStartElement("AccessPolicy");
										bool flag = true;
										while (xmlReader.IsStartElement("Start") || xmlReader.IsStartElement("Expiry") || xmlReader.IsStartElement("Permission"))
										{
											if (xmlReader.IsStartElement("Start") && !string.IsNullOrEmpty(str) && flag)
											{
												throw new InvalidXmlProtocolException("Duplicate start tag found in policy");
											}
											str = (xmlReader.IsStartElement("Start") ? xmlReader.ReadElementString("Start") : str);
											if (xmlReader.IsStartElement("Expiry") && !string.IsNullOrEmpty(empty1) && flag)
											{
												throw new InvalidXmlProtocolException("Duplicate expiry tag found in policy");
											}
											empty1 = (xmlReader.IsStartElement("Expiry") ? xmlReader.ReadElementString("Expiry") : empty1);
											str1 = (xmlReader.IsStartElement("Permission") ? xmlReader.ReadElementString("Permission") : str1);
											if (xmlReader.IsStartElement("Permission") && !string.IsNullOrEmpty(str1) && flag)
											{
												throw new InvalidXmlProtocolException("Duplicate permission tag found in policy");
											}
											if (string.IsNullOrEmpty(str1))
											{
												continue;
											}
											SASUtilities.ValidatePermissionOrdering(str1, sasPermissionType);
										}
										xmlReader.ReadEndElement();
									}
									else
									{
										xmlReader.ReadStartElement("AccessPolicy");
									}
								}
								else
								{
									empty = xmlReader.ReadElementString("Id");
								}
							}
							xmlReader.ReadEndElement();
							if (string.IsNullOrEmpty(empty) || empty.Length > SASIdentifier.MaxIdLength)
							{
								throw new InvalidXmlProtocolException(string.Concat("Signed identifier ID cannot be empty or over ", SASIdentifier.MaxIdLength, " characters in length"), xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
							}
							foreach (SASIdentifier sASIdentifier in sASIdentifiers1)
							{
								if (sASIdentifier.Id != empty)
								{
									continue;
								}
								throw new InvalidXmlProtocolException(string.Concat("Signed identifier ", empty, " not unique"), xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
							}
							sASIdentifiers1.Add(new SASIdentifier(empty, str, empty1, str1));
						}
						xmlReader.ReadEndElement();
						if (xmlReader.Read())
						{
							throw new InvalidXmlProtocolException("Error parsing Xml content. There are nodes beyond the root element SignedIdentifiers");
						}
					}
					else
					{
						sASIdentifiers = sASIdentifiers1;
						return sASIdentifiers;
					}
				}
				return sASIdentifiers1;
			}
			catch (FormatException formatException1)
			{
				FormatException formatException = formatException1;
				throw new InvalidXmlProtocolException(string.Concat("Error parsing Xml content. Error parsing data.", formatException.GetLogString()));
			}
			catch (XmlException xmlException1)
			{
				XmlException xmlException = xmlException1;
				throw new InvalidXmlProtocolException(string.Concat("Error parsing Xml content. ", xmlException.GetLogString()), xmlException.LineNumber, xmlException.LinePosition);
			}
			return sASIdentifiers;
		}

		protected abstract void DecrementAccountConcurrentRequestCountIfNecessary();

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (!this.completed)
					{
						this.IndicateComplete(false);
					}
					if (this.wrappedResponseStream != null)
					{
						this.wrappedResponseStream.OnBeforeDispose = null;
						this.wrappedResponseStream.Dispose();
						this.wrappedResponseStream = null;
					}
				}
			}
			finally
			{
				this.isDisposed = true;
			}
		}

		public void EndProcess(IAsyncResult result)
		{
			Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("Entering BasicHttpRestProcessor.EndProcess");
			Exception exception = null;
			try
			{
				if (this.isDisposed)
				{
					throw new ObjectDisposedException("BasicHttpProcessor");
				}
				NephosAssertionException.Assert(result != null);
				NephosAssertionException.Assert(this.started, "Processor is not started. You can call EndProcess only after calling BeginProcess.");
				((AsyncIteratorContext<NoResults>)result).End(out exception);
				if (exception != null)
				{
					Logger<IRestProtocolHeadLogger>.Instance.ErrorDebug.Log("Exception raised during processing of request. {0}", new object[] { exception });
					throw exception;
				}
			}
			finally
			{
				this.DecrementAccountConcurrentRequestCountIfNecessary();
				if (!this.ResponseIsClosed)
				{
					this.Response.StatusCode = 500;
					this.CloseResponse();
					NephosAssertionException.Fail("Response is not closed in ProcessImpl.", exception);
				}
			}
		}

		protected static TimeSpan GetDefaultMaxTimeoutForListCommands(string version)
		{
			string str = version;
			string str1 = str;
			if (str == null || !(str1 == "2008-10-27") && !(str1 == "2009-04-14"))
			{
				return BasicHttpProcessor.DefaultMaxAllowedTimeout;
			}
			return TimeSpan.FromSeconds(60);
		}

		public static string GetETag(DateTime lastModifiedTime, bool addQuotes)
		{
			return ETagHelper.GetETagFromDateTime(lastModifiedTime, addQuotes);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="0#e", Justification="eTag is an accepted capitalization of the word ETag (added to the custom dictionary for Nephos) for parameter names.")]
		public static DateTime GetLastModifiedTimeFromETag(string eTag)
		{
			DateTime lastModifiedTimeFromETag;
			try
			{
				lastModifiedTimeFromETag = ETagHelper.GetLastModifiedTimeFromETag(eTag);
			}
			catch (FormatException formatException)
			{
				throw new InvalidHeaderProtocolException("ETag", eTag, formatException);
			}
			catch (OverflowException overflowException)
			{
				throw new InvalidHeaderProtocolException("ETag", eTag, overflowException);
			}
			catch (ArgumentOutOfRangeException argumentOutOfRangeException)
			{
				throw new InvalidHeaderProtocolException("ETag", eTag, argumentOutOfRangeException);
			}
			return lastModifiedTimeFromETag;
		}

		protected Uri GetRequestUrlHidingNonStandardPorts()
		{
			return this.RequestUrl;
		}

		public void IndicateComplete(bool success)
		{
			if (this.isDisposed)
			{
				throw new ObjectDisposedException("BasicHttpProcessor");
			}
			NephosAssertionException.Assert(this.started, "Processor is not started. You can call IndicateComplete only after calling BeginProcess.");
			NephosAssertionException.Assert(!this.completed, "Processor is already completed. You can call IndicateComplete only once.");
			this.completed = true;
			ProcessorCompletionEventArgs processorCompletionEventArg = new ProcessorCompletionEventArgs(this.RequestContext.ElapsedTime, success);
			this.OnProcessorCompletion(this, processorCompletionEventArg);
		}

		public bool IsRequestVersionAtLeast(string earliestRequiredVersion)
		{
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(this.RequestRestVersion))
			{
				return false;
			}
			return VersioningHelper.CompareVersions(this.RequestRestVersion, earliestRequiredVersion) >= 0;
		}

		private void OnProcessorCompletion(object sender, ProcessorCompletionEventArgs e)
		{
			if (this.isDisposed)
			{
				throw new ObjectDisposedException("BasicHttpProcessor");
			}
			EventHandler<ProcessorCompletionEventArgs> eventHandler = this.ProcessorCompletion;
			if (eventHandler != null)
			{
				eventHandler(sender, e);
			}
		}

		private void OnProcessorStarted(object sender, EventArgs e)
		{
			if (this.isDisposed)
			{
				throw new ObjectDisposedException("BasicHttpProcessor");
			}
			EventHandler eventHandler = this.ProcessorStarted;
			if (eventHandler != null)
			{
				eventHandler(sender, e);
			}
		}

		protected virtual void PerformPreCloseTasks()
		{
		}

		protected abstract IEnumerator<IAsyncResult> ProcessImpl(AsyncIteratorContext<NoResults> async);

		protected void RateTrackerBytesWrittenEventHandler(object sender, ByteCountEventArgs e)
		{
			double totalSeconds = DateTime.UtcNow.Subtract(this.startOfLastWrite).TotalSeconds;
			double num = this.lastDetectedRate;
			if (num == double.MaxValue)
			{
				num = 0;
			}
			this.lastDetectedRate = num * (double)this.writtenBytes / (double)(this.writtenBytes + e.Bytes) + (double)e.Bytes / totalSeconds * ((double)e.Bytes / (double)(this.writtenBytes + e.Bytes));
			this.writtenBytes += e.Bytes;
		}

		protected void RateTrackerWrittingEventHandler(object sender, ByteCountEventArgs e)
		{
			this.startOfLastWrite = DateTime.UtcNow;
			Thread.MemoryBarrier();
			this.pendingBytesToWrite = e.Bytes;
		}

		private void ResetContentLength(bool isClosing)
		{
			if (!this.ResponseIsClosed && HttpUtilities.StatusCodeIndicatesNoResponseBody(this.StatusCode))
			{
				this.Response.ContentLength64 = (long)0;
				if (isClosing)
				{
					this.Response.SendChunked = false;
				}
			}
		}

		protected void SetRequestIdForResponse()
		{
			this.RequestId = Trace.ActivityId;
			if (this.RequestId != Guid.Empty)
			{
				string str = this.RequestId.ToString("D");
				this.Response.AddHeader("x-ms-request-id", str);
				return;
			}
			IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
			object[] requestUrl = new object[] { this.RequestUrl, this.Method };
			error.Log("Activity ID is not set for the request URI {0}, method: {1}", requestUrl);
		}

		protected virtual void SetStatusCodeAndServiceHeaders(HttpStatusCode statusCode)
		{
			this.StatusCode = statusCode;
		}

		private RestMethod TranslateHttpVerbToRestMethod(string verb)
		{
			if (this.isDisposed)
			{
				throw new ObjectDisposedException("BasicHttpProcessor");
			}
			if (string.IsNullOrEmpty(verb))
			{
				return RestMethod.Unknown;
			}
			if (Comparison.StringEquals("GET", verb))
			{
				return RestMethod.GET;
			}
			if (Comparison.StringEquals("PUT", verb))
			{
				return RestMethod.PUT;
			}
			if (Comparison.StringEquals("POST", verb))
			{
				return RestMethod.POST;
			}
			if (Comparison.StringEquals("MERGE", verb))
			{
				return RestMethod.MERGE;
			}
			if (Comparison.StringEquals("DELETE", verb))
			{
				return RestMethod.DELETE;
			}
			if (Comparison.StringEquals("HEAD", verb))
			{
				return RestMethod.HEAD;
			}
			if (Comparison.StringEquals("TRACE", verb))
			{
				return RestMethod.TRACE;
			}
			if (Comparison.StringEquals("CONNECT", verb))
			{
				return RestMethod.CONNECT;
			}
			if (Comparison.StringEquals("OPTIONS", verb))
			{
				return RestMethod.OPTIONS;
			}
			if (Comparison.StringEquals("PATCH", verb))
			{
				return RestMethod.PATCH;
			}
			return RestMethod.Unknown;
		}

		public event EventHandler<ProcessorCompletionEventArgs> ProcessorCompletion;

		public event EventHandler ProcessorStarted;
	}
}