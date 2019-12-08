using System;
using System.Data.Services.Internal;

namespace Microsoft.UtilityComputing
{
	public sealed class ProjectedWrapper7Creator : ProjectedWrapperCreator
	{
		public ProjectedWrapper7Creator()
		{
		}

		public override ProjectedWrapper CreateProjectedWrapper(object[] propertyValues, string propertyNameList, string resourceTypeName)
		{
			ProjectedWrapper7 projectedWrapper7 = base.CreateProjectedWrapperInternal<ProjectedWrapper7>(propertyNameList, resourceTypeName);
			projectedWrapper7.ProjectedProperty0 = propertyValues[0];
			projectedWrapper7.ProjectedProperty1 = propertyValues[1];
			projectedWrapper7.ProjectedProperty2 = propertyValues[2];
			projectedWrapper7.ProjectedProperty3 = propertyValues[3];
			projectedWrapper7.ProjectedProperty4 = propertyValues[4];
			projectedWrapper7.ProjectedProperty5 = propertyValues[5];
			projectedWrapper7.ProjectedProperty6 = propertyValues[6];
			return projectedWrapper7;
		}
	}
}