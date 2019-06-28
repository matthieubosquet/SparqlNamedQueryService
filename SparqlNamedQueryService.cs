namespace Sparql
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;
    using VDS.RDF;
    using VDS.RDF.Query;
    using VDS.RDF.Storage;
    using VDS.RDF.Writing.Formatting;

    /// <summary>
    /// Issues parameterizable named queries, get graphs or SPARQL result sets.
    /// </summary>
    public class SparqlNamedQueryService : ISparqlNamedQueryService, IDisposable
    {
        private SparqlRemoteEndpoint endpoint;
        private SparqlConnector connector;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparqlNamedQueryService"/> class.
        /// </summary>
        /// <param name="config">Stores connection details such as the SPARQL endpoint URI under SparqlService:Uri.</param>
        public SparqlNamedQueryService(IConfiguration config)
        {
            this.endpoint = new SparqlRemoteEndpoint(new Uri(config.GetSection("SparqlService")["Uri"]));
            this.connector = new SparqlConnector(this.endpoint);
        }

        /// <inheritdoc/>
        public IGraph GetGraph(string name, IDictionary<string, IEnumerable<object>> parameters = null)
        {
            return this.connector.Query(GetSparqlQueryStringFromEmbeddedResource(name, parameters)) as IGraph;
        }

        /// <inheritdoc/>
        public SparqlResultSet GetSparqlResultSet(string name, IDictionary<string, IEnumerable<object>> parameters = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        void IDisposable.Dispose()
        {
            this.connector.Dispose();
        }

        private static string GetSparqlQueryStringFromEmbeddedResource(string name, IDictionary<string, IEnumerable<object>> parameters)
        {
            var sparqlParameterizedString = null as SparqlParameterizedString;

            using (var sparqlResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                using (var reader = new StreamReader(sparqlResourceStream))
                {
                    sparqlParameterizedString = new SparqlParameterizedString(reader.ReadToEnd());
                }
            }

            return SetQueryParameters(sparqlParameterizedString, parameters);
        }

        private static string SetQueryParameters(SparqlParameterizedString sparqlParameterizedString, IDictionary<string, IEnumerable<object>> parameters)
        {
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    if (parameter.Value is IEnumerable<string> stringValues)
                    {
                        SetLiterals(sparqlParameterizedString, parameter.Key, stringValues);
                    }
                    else if (parameter.Value is IEnumerable<Uri> uriValues)
                    {
                        SetUris(sparqlParameterizedString, parameter.Key, uriValues);
                    }
                    else if (parameter.Value is null)
                    {
                        SetUndef(sparqlParameterizedString, parameter.Key);
                    }
                }
            }

            return sparqlParameterizedString.ToString();
        }

        private static void SetLiterals(SparqlParameterizedString sparqlParameterizedString, string parameter, IEnumerable<string> values)
        {
            // Create literal nodes
            var formatter = new SparqlFormatter();
            var factory = new NodeFactory();
            sparqlParameterizedString.CommandText = sparqlParameterizedString.CommandText.Replace($"@{parameter}", string.Join(" ", values.Select(value => formatter.Format(factory.CreateLiteralNode(value)))));
        }

        private static void SetUris(SparqlParameterizedString sparqlParameterizedString, string parameter, IEnumerable<Uri> values)
        {
            // Enclose Uris in <>
            var formatter = new SparqlFormatter();
            var factory = new NodeFactory();
            sparqlParameterizedString.CommandText = sparqlParameterizedString.CommandText.Replace($"@{parameter}", string.Join(" ", values.Select(uri => formatter.Format(factory.CreateUriNode(uri)))));
        }

        private static void SetUndef(SparqlParameterizedString sparqlParameterizedString, string parameter)
        {
            sparqlParameterizedString.CommandText = sparqlParameterizedString.CommandText.Replace($"@{parameter}", "undef");
        }
    }
}
