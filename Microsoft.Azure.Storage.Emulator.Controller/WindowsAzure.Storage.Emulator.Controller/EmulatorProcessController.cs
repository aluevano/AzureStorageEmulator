using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	internal static class EmulatorProcessController
	{
		[PermissionSet(SecurityAction.Demand, Name="FullTrust")]
		public static void EnsureRunning(int timeoutInMilliseconds)
		{
			if (EmulatorProcessController.IsRunning())
			{
				EmulatorProcessController.InternalWaitForStorageEmulator(timeoutInMilliseconds);
				throw new EmulatorException(EmulatorErrorCode.AlreadyRunning);
			}
			ProcessWrapper processWrapper = new ProcessWrapper("AzureStorageEmulator.exe", "start /InProcess");
			if (timeoutInMilliseconds == 0)
			{
				processWrapper.EnsureStarted();
				return;
			}
			bool flag = false;
			StringBuilder stringBuilder = new StringBuilder();
			if (!CheckStoragePrerequisites.CheckPrerequisites((string msg) => stringBuilder.Append(msg), out flag))
			{
				if (!flag)
				{
					string str = stringBuilder.ToString();
					if (!string.IsNullOrEmpty(str))
					{
						throw new EmulatorException(EmulatorErrorCode.StartFailed, str);
					}
					throw new EmulatorException(EmulatorErrorCode.StartFailed);
				}
				try
				{
					Initialization initialization = new Initialization(false, false, true, null, null, false, false);
					initialization.PerformEmulatorInitialization();
				}
				catch (Exception exception)
				{
					throw new EmulatorException(EmulatorErrorCode.InitializationRequired, exception);
				}
			}
			processWrapper.EnsureStarted();
			EmulatorProcessController.InternalWaitForStorageEmulator(timeoutInMilliseconds);
		}

		[PermissionSet(SecurityAction.Demand, Name="FullTrust")]
		private static void InternalWaitForStorageEmulator(int timeoutInMilliseconds)
		{
			int num;
			TimeSpan timeSpan = TimeSpan.FromSeconds(0.5);
			EventWaitHandle eventWaitHandle = null;
			EventWaitHandle eventWaitHandle1 = null;
			try
			{
				int num1 = 240;
				do
				{
					try
					{
						eventWaitHandle = EventWaitHandle.OpenExisting("DevelopmentStorage-7580AFBB-2BEC-4269-B083-46C1361A69B0");
						eventWaitHandle1 = EventWaitHandle.OpenExisting("DevelopmentStorage-Failure-7580AFBB-2BEC-4269-B083-46C1361A69B0");
						break;
					}
					catch (WaitHandleCannotBeOpenedException waitHandleCannotBeOpenedException)
					{
						Thread.Sleep(timeSpan);
					}
					num = num1;
					num1 = num - 1;
				}
				while (num > 0);
				if (num1 <= 0)
				{
					throw new TimeoutException(Resource.MessageUnableToOpenWaitHandle);
				}
				WaitHandle[] waitHandleArray = new WaitHandle[] { eventWaitHandle, eventWaitHandle1 };
				int num2 = WaitHandle.WaitAny(waitHandleArray, timeoutInMilliseconds);
				if (num2 == 258)
				{
					throw new TimeoutException(Resource.MessageDidNotReceiveReadySignalFromStorageEmulator);
				}
				if (num2 == 1)
				{
					throw new EmulatorException(EmulatorErrorCode.StartFailed);
				}
			}
			finally
			{
				if (eventWaitHandle != null)
				{
					eventWaitHandle.Close();
				}
				if (eventWaitHandle1 != null)
				{
					eventWaitHandle1.Close();
				}
			}
		}

		public static bool IsRunning()
		{
			bool flag;
			try
			{
				using (EventWaitHandle eventWaitHandle = EventWaitHandle.OpenExisting("DevelopmentStorage-7580AFBB-2BEC-4269-B083-46C1361A69B0"))
				{
					flag = true;
				}
			}
			catch (WaitHandleCannotBeOpenedException waitHandleCannotBeOpenedException)
			{
				flag = false;
			}
			return flag;
		}

		public static void Shutdown()
		{
			if (!EmulatorProcessController.IsRunning())
			{
				throw new EmulatorException(EmulatorErrorCode.AlreadyStopped);
			}
			try
			{
				using (EventWaitHandle eventWaitHandle = EventWaitHandle.OpenExisting("DevelopmentStorage-3D75486A-E34F-447c-BF4B-A35284FA8D96"))
				{
					eventWaitHandle.Set();
				}
			}
			catch (WaitHandleCannotBeOpenedException waitHandleCannotBeOpenedException)
			{
			}
			Process[] instances = (new ProcessWrapper("AzureStorageEmulator.exe", "start /InProcess")).GetInstances();
			for (int i = 0; i < (int)instances.Length; i++)
			{
				Process process = instances[i];
				if (process.Id != Process.GetCurrentProcess().Id)
				{
					try
					{
						if (!process.WaitForExit(15000))
						{
							process.Kill();
							process.WaitForExit(15000);
						}
					}
					catch (InvalidOperationException invalidOperationException)
					{
					}
					catch (Win32Exception win32Exception)
					{
					}
				}
			}
		}
	}
}