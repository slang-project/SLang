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

        public JsonIr(Type type) : this(type, null) { }

        public JsonIr(Type type, string value) : this(type.Name, null) { }

        public JsonIr(string typeName) : this(typeName, null) { }

        public JsonIr(string typeName, string value)
        {
            this.type = typeName;
            this.value = value;
        }

        public JsonIr AppendChild(JsonIr child)
        {
            ++num_children;
            children.Add(child);
            return this;
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

        public static JsonIr ListToJSON<T>(List<T> entitiesList) where T : ENTITY
        {
            if (entitiesList == null) return null;
            JsonIr irList = new JsonIr(typeof(T).Name + "_LIST");
            foreach (ENTITY e in entitiesList) irList.AppendChild(e.ToJSON());
            return irList;
        }
    }
}
