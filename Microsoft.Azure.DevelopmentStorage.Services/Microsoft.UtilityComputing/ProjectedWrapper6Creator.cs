using System;
using System.Data.Services.Internal;

namespace Microsoft.UtilityComputing
{
	public sealed class ProjectedWrapper6Creator : ProjectedWrapperCreator
	{
		public ProjectedWrapper6Creator()
		{
		}

		public override ProjectedWrapper CreateProjectedWrapper(object[] propertyValues, string propertyNameList, string resourceTypeName)
		{
			ProjectedWrapper6 projectedWrapper6 = base.CreateProjectedWrapperInternal<ProjectedWrapper6>(propertyNameList, resourceTypeName);
			projectedWrapper6.ProjectedProperty0 = propertyValues[0];
			projectedWrapper6.ProjectedProperty1 = propertyValues[1];
			projectedWrapper6.ProjectedProperty2 = propertyValues[2];
			projectedWrapper6.ProjectedProperty3 = propertyValues[3];
			projectedWrapper6.ProjectedProperty4 = propertyValues[4];
			projectedWrapper6.ProjectedProperty5 = propertyValues[5];
			return projectedWrapper6;
		}
	}
}