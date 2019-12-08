using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[CompilerGenerated]
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	public class Resources
	{
		private static System.Resources.ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		public static string BlobSummary
		{
			get
			{
				return Resources.ResourceManager.GetString("BlobSummary", Resources.resourceCulture);
			}
		}

		public static string CommitBlockList
		{
			get
			{
				return Resources.ResourceManager.GetString("CommitBlockList", Resources.resourceCulture);
			}
		}

		public static string CompilationErrors
		{
			get
			{
				return Resources.ResourceManager.GetString("CompilationErrors", Resources.resourceCulture);
			}
		}

		public static string CreateViewTemplate
		{
			get
			{
				return Resources.ResourceManager.GetString("CreateViewTemplate", Resources.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		public static string ForeignKeys
		{
			get
			{
				return Resources.ResourceManager.GetString("ForeignKeys", Resources.resourceCulture);
			}
		}

		public static string LastModificationTimeTriggers
		{
			get
			{
				return Resources.ResourceManager.GetString("LastModificationTimeTriggers", Resources.resourceCulture);
			}
		}

		public static string MyDataServiceTemplate
		{
			get
			{
				return Resources.ResourceManager.GetString("MyDataServiceTemplate", Resources.resourceCulture);
			}
		}

		public static string PageBlob
		{
			get
			{
				return Resources.ResourceManager.GetString("PageBlob", Resources.resourceCulture);
			}
		}

		public static string QueueFunctions
		{
			get
			{
				return Resources.ResourceManager.GetString("QueueFunctions", Resources.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static System.Resources.ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(Resources.resourceMan, null))
				{
					Resources.resourceMan = new System.Resources.ResourceManager("Microsoft.WindowsAzure.DevelopmentStorage.Store.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		internal Resources()
		{
		}
	}
}