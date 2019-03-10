using System;
using System.Collections.Generic;
using System.Linq;

namespace SLang.Service
{
    public class JsonIr
    {
        public static string nl { get; set; } = Environment.NewLine;
        public static string indent { get; set; } = "    ";

        private string type;
        private string value;
        private uint num_children = 0;  // ushort???
        private List<JsonIr> children = new List<JsonIr>();

        public JsonIr(string type) : this(type, null) { }

        public JsonIr(string type, string value)
        {
            this.type = type;
            this.value = value;
        }

        public void AppendChild(JsonIr child)
        {
            ++num_children;
            children.Add(child);
        }

        public string Serialize(bool indentation)
        {
            return String.Format(
                "{{0}" +
                "{1}\"type\":{2},{0}" +
                "{1}\"value\":{3},{0}" +
                "{1}\"num_children\":{4},{0}" +
                "{1}\"children\":[{0}" +
                "{1}{1}{5}{0}" +
                "{1}]{0}" +
                "}",
                indentation ? nl: "",
                indentation ? indent : "",
                Jsonify(type),
                Jsonify(value),
                num_children,
                string.Join(
                    "," + (indentation ? nl + indent + indent : ""),
                    children.Select(
                        o => o.Serialize(indentation).
                            Replace(nl, nl + indent + indent)
                        )
                    )
                );
        }

        private string Jsonify(string s)
        {
            if (s == null) return "null";
            return System.Web.Helpers.Json.Encode(s);
        }
    }
}
