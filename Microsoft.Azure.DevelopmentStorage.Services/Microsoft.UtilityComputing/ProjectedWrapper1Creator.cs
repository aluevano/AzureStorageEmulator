using System;
using System.Data.Services.Internal;

namespace Microsoft.UtilityComputing
{
	public sealed class ProjectedWrapper1Creator : ProjectedWrapperCreator
	{
		public ProjectedWrapper1Creator()
		{
		}

		public override ProjectedWrapper CreateProjectedWrapper(object[] propertyValues, string propertyNameList, string resourceTypeName)
		{
			ProjectedWrapper1 projectedWrapper1 = base.CreateProjectedWrapperInternal<ProjectedWrapper1>(propertyNameList, resourceTypeName);
			projectedWrapper1.ProjectedProperty0 = propertyValues[0];
			return projectedWrapper1;
		}
	}
}