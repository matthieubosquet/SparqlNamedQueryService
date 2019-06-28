namespace Sparql
{
    using System.Collections.Generic;
    using VDS.RDF;
    using VDS.RDF.Query;

    /// <summary>
    /// Gets graph or SPARQL result set corresponding to a named query and set of parameters.
    /// </summary>
    public interface ISparqlNamedQueryService
    {
        /// <summary>
        /// Issues a parameterized named query, get graph.
        /// </summary>
        /// <param name="name">The name of the Query.</param>
        /// <param name="parameters">The parameters to apply to the Query.</param>
        /// <returns>The Graph corresponding to name and parameters.</returns>
        IGraph GetGraph(string name, IDictionary<string, IEnumerable<object>> parameters = null);

        /// <summary>
        /// Issues a parameterized named query, get SPARQL results.
        /// </summary>
        /// <param name="name">The name of the Query.</param>
        /// <param name="parameters">The parameters to apply to the Query.</param>
        /// <returns>The SPARQL result set corresponding to name and parameters.</returns>
        SparqlResultSet GetSparqlResultSet(string name, IDictionary<string, IEnumerable<object>> parameters = null);
    }
}