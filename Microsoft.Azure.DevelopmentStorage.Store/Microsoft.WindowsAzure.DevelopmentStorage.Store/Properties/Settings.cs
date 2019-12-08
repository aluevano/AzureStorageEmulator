using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store.Properties
{
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance;

		public static Settings Default
		{
			get
			{
				return Settings.defaultInstance;
			}
		}

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("Data Source=NIRANJANN2;Initial Catalog=DevelopmentStore;Integrated Security=True")]
		[SpecialSetting(SpecialSetting.ConnectionString)]
		public string DevelopmentStoreConnectionString
		{
			get
			{
				return (string)this["DevelopmentStoreConnectionString"];
			}
		}

		static Settings()
		{
			Settings.defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
		}

		public Settings()
		{
		}
	}
}