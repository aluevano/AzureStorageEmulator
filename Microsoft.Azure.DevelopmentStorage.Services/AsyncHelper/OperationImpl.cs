using System;
using System.Collections.Generic;

namespace AsyncHelper
{
	public delegate IEnumerator<IAsyncResult> OperationImpl(AsyncIteratorContext<NoResults> asyncContext);
}