using Microsoft.Cis.Services.Nephos.Common.Versioning;
using System;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class VersioningHelper
	{
		private static object lockObject;

		private static List<string> preSeptember09Versions;

		private static List<string> preDecember09Versions;

		private static List<string> preAugust11Versions;

		private static List<string> preFebruary12Versions;

		private static List<string> preJuly13Versions;

		private static List<string> preAugust13Versions;

		private static List<string> preFebruary14Versions;

		private static List<string> preFebruary15Versions;

		private static List<string> preApril15Versions;

		private static List<string> preJuly15Versions;

		private static List<string> preDecember15Versions;

		private static List<string> preFebruary16Versions;

		private static List<string> preMay16Versions;

		private static List<string> preOctober16Versions;

		private static List<string> preApril17Versions;

		public static List<string> PreApril15Versions
		{
			get
			{
				if (VersioningHelper.preApril15Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preApril15Versions == null)
						{
							List<string> strs = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19",
								"2011-08-18",
								"2012-02-12",
								"2013-08-15",
								"2014-02-14",
								"2015-02-21"
							};
							VersioningHelper.preApril15Versions = strs;
						}
					}
				}
				return VersioningHelper.preApril15Versions;
			}
		}

		public static List<string> PreApril17Versions
		{
			get
			{
				if (VersioningHelper.preApril17Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preApril17Versions == null)
						{
							List<string> strs = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19",
								"2011-08-18",
								"2012-02-12",
								"2013-08-15",
								"2014-02-14",
								"2015-02-21",
								"2015-04-05",
								"2015-07-08",
								"2015-12-11",
								"2016-02-19",
								"2016-05-31",
								"2016-10-16"
							};
							VersioningHelper.preApril17Versions = strs;
						}
					}
				}
				return VersioningHelper.preApril17Versions;
			}
		}

		public static List<string> PreAugust11Versions
		{
			get
			{
				if (VersioningHelper.preAugust11Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preAugust11Versions == null)
						{
							VersioningHelper.preAugust11Versions = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19"
							};
						}
					}
				}
				return VersioningHelper.preAugust11Versions;
			}
		}

		public static List<string> PreAugust13Versions
		{
			get
			{
				if (VersioningHelper.preAugust13Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preAugust13Versions == null)
						{
							VersioningHelper.preAugust13Versions = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19",
								"2011-08-18",
								"2012-02-12"
							};
						}
					}
				}
				return VersioningHelper.preAugust13Versions;
			}
		}

		public static List<string> PreDecember09Versions
		{
			get
			{
				if (VersioningHelper.preDecember09Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preDecember09Versions == null)
						{
							VersioningHelper.preDecember09Versions = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19"
							};
						}
					}
				}
				return VersioningHelper.preDecember09Versions;
			}
		}

		public static List<string> PreDecember15Versions
		{
			get
			{
				if (VersioningHelper.preDecember15Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preDecember15Versions == null)
						{
							List<string> strs = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19",
								"2011-08-18",
								"2012-02-12",
								"2013-08-15",
								"2014-02-14",
								"2015-02-21",
								"2015-04-05",
								"2015-07-08"
							};
							VersioningHelper.preDecember15Versions = strs;
						}
					}
				}
				return VersioningHelper.preDecember15Versions;
			}
		}

		public static List<string> PreFebruary12Versions
		{
			get
			{
				if (VersioningHelper.preFebruary12Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preFebruary12Versions == null)
						{
							VersioningHelper.preFebruary12Versions = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19",
								"2011-08-18"
							};
						}
					}
				}
				return VersioningHelper.preFebruary12Versions;
			}
		}

		public static List<string> PreFebruary14Versions
		{
			get
			{
				if (VersioningHelper.preFebruary14Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preFebruary14Versions == null)
						{
							VersioningHelper.preFebruary14Versions = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19",
								"2011-08-18",
								"2012-02-12",
								"2013-08-15"
							};
						}
					}
				}
				return VersioningHelper.preFebruary14Versions;
			}
		}

		public static List<string> PreFebruary15Versions
		{
			get
			{
				if (VersioningHelper.preFebruary15Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preFebruary15Versions == null)
						{
							List<string> strs = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19",
								"2011-08-18",
								"2012-02-12",
								"2013-08-15",
								"2014-02-14"
							};
							VersioningHelper.preFebruary15Versions = strs;
						}
					}
				}
				return VersioningHelper.preFebruary15Versions;
			}
		}

		public static List<string> PreFebruary16Versions
		{
			get
			{
				if (VersioningHelper.preFebruary16Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preFebruary16Versions == null)
						{
							List<string> strs = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19",
								"2011-08-18",
								"2012-02-12",
								"2013-08-15",
								"2014-02-14",
								"2015-02-21",
								"2015-04-05",
								"2015-07-08",
								"2015-12-11"
							};
							VersioningHelper.preFebruary16Versions = strs;
						}
					}
				}
				return VersioningHelper.preFebruary16Versions;
			}
		}

		public static List<string> PreJuly13Versions
		{
			get
			{
				if (VersioningHelper.preJuly13Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preJuly13Versions == null)
						{
							VersioningHelper.preJuly13Versions = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19",
								"2011-08-18",
								"2012-02-12"
							};
						}
					}
				}
				return VersioningHelper.preJuly13Versions;
			}
		}

		public static List<string> PreJuly15Versions
		{
			get
			{
				if (VersioningHelper.preJuly15Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preJuly15Versions == null)
						{
							List<string> strs = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19",
								"2011-08-18",
								"2012-02-12",
								"2013-08-15",
								"2014-02-14",
								"2015-02-21",
								"2015-04-05"
							};
							VersioningHelper.preJuly15Versions = strs;
						}
					}
				}
				return VersioningHelper.preJuly15Versions;
			}
		}

		public static List<string> PreMay16Versions
		{
			get
			{
				if (VersioningHelper.preMay16Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preMay16Versions == null)
						{
							List<string> strs = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19",
								"2011-08-18",
								"2012-02-12",
								"2013-08-15",
								"2014-02-14",
								"2015-02-21",
								"2015-04-05",
								"2015-07-08",
								"2015-12-11",
								"2016-02-19"
							};
							VersioningHelper.preMay16Versions = strs;
						}
					}
				}
				return VersioningHelper.preMay16Versions;
			}
		}

		public static List<string> PreOctober16Versions
		{
			get
			{
				if (VersioningHelper.preOctober16Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preOctober16Versions == null)
						{
							List<string> strs = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17",
								"2009-09-19",
								"2011-08-18",
								"2012-02-12",
								"2013-08-15",
								"2014-02-14",
								"2015-02-21",
								"2015-04-05",
								"2015-07-08",
								"2015-12-11",
								"2016-02-19",
								"2016-05-31"
							};
							VersioningHelper.preOctober16Versions = strs;
						}
					}
				}
				return VersioningHelper.preOctober16Versions;
			}
		}

		public static List<string> PreSeptember09Versions
		{
			get
			{
				if (VersioningHelper.preSeptember09Versions == null)
				{
					lock (VersioningHelper.lockObject)
					{
						if (VersioningHelper.preSeptember09Versions == null)
						{
							VersioningHelper.preSeptember09Versions = new List<string>()
							{
								"2008-10-27",
								"2009-04-14",
								"2009-07-17"
							};
						}
					}
				}
				return VersioningHelper.preSeptember09Versions;
			}
		}

		static VersioningHelper()
		{
			VersioningHelper.lockObject = new object();
			VersioningHelper.preSeptember09Versions = null;
			VersioningHelper.preDecember09Versions = null;
			VersioningHelper.preAugust11Versions = null;
			VersioningHelper.preFebruary12Versions = null;
			VersioningHelper.preJuly13Versions = null;
			VersioningHelper.preAugust13Versions = null;
			VersioningHelper.preFebruary14Versions = null;
			VersioningHelper.preFebruary15Versions = null;
			VersioningHelper.preApril15Versions = null;
			VersioningHelper.preJuly15Versions = null;
			VersioningHelper.preDecember15Versions = null;
			VersioningHelper.preFebruary16Versions = null;
			VersioningHelper.preMay16Versions = null;
			VersioningHelper.preOctober16Versions = null;
			VersioningHelper.preApril17Versions = null;
		}

		public static int CompareVersions(string version1, string version2)
		{
			NephosAssertionException.Assert(!string.IsNullOrEmpty(version1));
			NephosAssertionException.Assert(!string.IsNullOrEmpty(version2));
			if (version1.Equals("2008-10-27", StringComparison.InvariantCulture))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2012-09-19", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2012-09-19", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2013-07-14", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2013-07-14", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2013-08-15", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2013-08-15", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2014-02-14", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2013-08-15", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2014-02-14", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2015-02-21", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2013-08-15", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2014-02-14", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2015-02-21", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2015-04-05", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2013-08-15", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2014-02-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-02-21", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2015-04-05", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2015-07-08", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2013-08-15", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2014-02-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-02-21", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-04-05", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2015-07-08", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2015-12-11", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2013-08-15", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2014-02-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-02-21", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-04-05", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-07-08", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2015-12-11", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2016-02-19", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2013-08-15", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2014-02-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-02-21", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-04-05", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-07-08", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-12-11", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2016-02-19", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2016-05-31", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2013-08-15", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2014-02-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-02-21", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-04-05", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-07-08", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-12-11", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2016-02-19", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2016-05-31", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (version1.Equals("2016-10-16", StringComparison.InvariantCultureIgnoreCase))
			{
				if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2013-08-15", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2014-02-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-02-21", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-04-05", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-07-08", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-12-11", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2016-02-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2016-05-31", StringComparison.InvariantCultureIgnoreCase))
				{
					return 1;
				}
				if (version2.Equals("2016-10-16", StringComparison.InvariantCultureIgnoreCase))
				{
					return 0;
				}
				return -1;
			}
			if (!version1.Equals("2017-04-17", StringComparison.InvariantCultureIgnoreCase))
			{
				NephosAssertionException.Fail("version1 = '{0}' is an invalid version.", new object[] { version1 });
				NephosAssertionException.Fail("Should never get to this point in VersioningHelper.CompareVersions. This is a coding bug.");
				return 0;
			}
			if (version2.Equals("2008-10-27", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-07-17", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-09-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2009-04-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2011-08-18", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2012-02-12", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2013-08-15", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2014-02-14", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-02-21", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-04-05", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-07-08", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2015-12-11", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2016-02-19", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2016-05-31", StringComparison.InvariantCultureIgnoreCase) || version2.Equals("2016-10-16", StringComparison.InvariantCultureIgnoreCase))
			{
				return 1;
			}
			if (version2.Equals("2017-04-17", StringComparison.InvariantCultureIgnoreCase))
			{
				return 0;
			}
			return -1;
		}

		public static bool IsPreApril15OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2015-04-05") < 0;
		}

		public static bool IsPreApril17OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2017-04-17") < 0;
		}

		public static bool IsPreAugust11OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2011-08-18") < 0;
		}

		public static bool IsPreAugust13OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2013-08-15") < 0;
		}

		public static bool IsPreDecember15OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2015-12-11") < 0;
		}

		public static bool IsPreFebruary12OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2012-02-12") < 0;
		}

		public static bool IsPreFebruary14OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2014-02-14") < 0;
		}

		public static bool IsPreFebruary15OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2015-02-21") < 0;
		}

		public static bool IsPreFebruary16OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2016-02-19") < 0;
		}

		public static bool IsPreJuly09OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2009-07-17") < 0;
		}

		public static bool IsPreJuly13OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2013-07-14") < 0;
		}

		public static bool IsPreJuly15OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2015-07-08") < 0;
		}

		public static bool IsPreMay16OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2016-05-31") < 0;
		}

		public static bool IsPreOctober16OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2016-10-16") < 0;
		}

		public static bool IsPreSeptember09OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2009-09-19") < 0;
		}

		public static bool IsPreSeptember12OrInvalidVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version))
			{
				return true;
			}
			return VersioningHelper.CompareVersions(version, "2012-09-19") < 0;
		}
	}
}