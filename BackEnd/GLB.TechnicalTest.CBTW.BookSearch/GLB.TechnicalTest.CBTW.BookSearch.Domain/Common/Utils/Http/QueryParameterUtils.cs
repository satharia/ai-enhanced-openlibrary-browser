using System.Text;

namespace GLB.TechnicalTest.CBTW.BookSearch.Domain.Common.Utils.Http
{
    public static class QueryParameterUtils
    {
        public static string BuildPathWithQueryParameterString(string path, IEnumerable<KeyValuePair<string,string?>> parameters)
        {
            var stringBuilder = new StringBuilder(path);
            var separator = path.Contains("?") ? "&" : "?";

            foreach (var (key, value) in parameters)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                stringBuilder
                    .Append(separator)
                    .Append(Uri.EscapeDataString(key))
                    .Append("=")
                    .Append(Uri.EscapeDataString(value))
                    ;
                separator = "&";
            }

            return stringBuilder.ToString();
        }
    }
}
