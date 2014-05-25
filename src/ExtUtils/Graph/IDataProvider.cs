using System;

namespace ExtUtils.Graph
{
	public interface IDataProvider
	{
		IEdge CreateEdge(IVertex u, IVertex v, int cost, object value);
		IVertex CreateVertex();
	}
}
