using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class AlertsManager
	{
		private const int maxAlertLoggingMapSize = 1000;

		private readonly static Dictionary<string, DateTime> alertLoggingMap;

		private readonly static object mapSync;

		private static bool alreadyAlertedOnCrash;

		static AlertsManager()
		{
			AlertsManager.alertLoggingMap = new Dictionary<string, DateTime>();
			AlertsManager.mapSync = new object();
			AlertsManager.alreadyAlertedOnCrash = false;
		}

		public AlertsManager()
		{
		}

		public static void AlertOrLogException(string alertMessage, string alertKey = null, TimeSpan? alertIntervalOverride = null)
		{
			if (AlertsManager.ShouldSendAlert(alertKey ?? alertMessage, alertIntervalOverride))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Critical.Log(alertMessage);
				return;
			}
			Logger<IRestProtocolHeadLogger>.Instance.Error.Log(alertMessage);
		}

		public static void AlertOrLogException(Exception e, string alertMessage = null, string alertKey = null, TimeSpan? alertIntervalOverride = null)
		{
			if (AlertsManager.ShouldAlertForException(e))
			{
				if (AlertsManager.ShouldSendAlert(alertKey ?? e.GetType().ToString(), alertIntervalOverride))
				{
					IStringDataEventStream critical = Logger<IRestProtocolHeadLogger>.Instance.Critical;
					object[] objArray = new object[] { alertMessage ?? "[FEUnhandledException] An unexpected non-fatal exception was encountered", e.ToString() };
					critical.Log("{0}: {1}", objArray);
					return;
				}
			}
			IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
			object[] objArray1 = new object[] { alertMessage ?? "[FEUnhandledException] An unexpected non-fatal exception was encountered", e.ToString() };
			error.Log("{0}: {1}", objArray1);
		}

		private static bool ShouldAlertForException(Exception e)
		{
			if (e is InvalidUrlException)
			{
				return false;
			}
			return true;
		}

		public static bool ShouldAlertOnCrash()
		{
			bool flag = !AlertsManager.alreadyAlertedOnCrash;
			AlertsManager.alreadyAlertedOnCrash = true;
			return flag;
		}

		public static bool ShouldSendAlert(string alertKey, TimeSpan? alertIntervalOverride = null)
		{
			bool flag = false;
			int num = 60;
			TimeSpan timeSpan = (alertIntervalOverride.HasValue ? alertIntervalOverride.Value : TimeSpan.FromMinutes((double)num));
			lock (AlertsManager.mapSync)
			{
				if (!AlertsManager.alertLoggingMap.ContainsKey(alertKey) || DateTime.UtcNow > AlertsManager.alertLoggingMap[alertKey])
				{
					if (AlertsManager.alertLoggingMap.Count > 1000)
					{
						AlertsManager.alertLoggingMap.Clear();
						IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
						object[] count = new object[] { AlertsManager.alertLoggingMap.Count };
						verbose.Log("AlertsManager: map reached {0} entries, clearing map.", count);
					}
					flag = true;
					AlertsManager.alertLoggingMap[alertKey] = DateTime.UtcNow + timeSpan;
					IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] objArray = new object[] { alertKey, timeSpan.TotalMinutes };
					stringDataEventStream.Log("AlertsManager: next alert for message '{0}' will be in {1} minutes", objArray);
				}
			}
			return flag;
		}

		public static void XEventOrLogError(string xEventMessage = null, string xEventKey = null, TimeSpan? xEventIntervalOverride = null)
		{
			if (AlertsManager.ShouldSendAlert(xEventKey ?? xEventMessage, xEventIntervalOverride))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Event.Log(xEventMessage);
				return;
			}
			Logger<IRestProtocolHeadLogger>.Instance.Error.Log(xEventMessage);
		}
	}
}