using System;
using System.Data.Services.Internal;

namespace Microsoft.UtilityComputing
{
	public sealed class ProjectedWrapper0Creator : ProjectedWrapperCreator
	{
		public ProjectedWrapper0Creator()
		{
		}

		public override ProjectedWrapper CreateProjectedWrapper(object[] propertyValues, string propertyNameList, string resourceTypeName)
		{
			return base.CreateProjectedWrapperInternal<ProjectedWrapper0>(propertyNameList, resourceTypeName);
		}
	}
}