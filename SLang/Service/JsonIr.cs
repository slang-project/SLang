using System;
using System.Collections.Generic;
using System.Linq;

namespace SLang.Service
{
    public class JsonIr
    {
        public const string JSON_NULL = "null";
        public static string nl { get; set; } = Environment.NewLine;
        public static string indent { get; set; } = "    ";

        private string type;
        private string value;
        private uint num_children = 0;  // ushort???
        private List<JsonIr> children = new List<JsonIr>();

        public JsonIr(Type type) : this(type, null) { }

        public JsonIr(Type type, string value) : this(type.Name, value) { }

        public JsonIr(string typeName) : this(typeName, null) { }

        public JsonIr(string typeName, string value)
        {
            this.type = typeName ?? throw new ArgumentNullException();
            this.value = value;
        }

        public JsonIr SetValue(string value)
        {
            this.value = value;
            return this;
        }

        public JsonIr AppendChild(JsonIr child)
        {
            ++num_children;
            children.Add(child);
            return child == null ? GetIrNull() : this;
        }

        public virtual string Serialize(bool indentation)
        {
            return String.Format(
                "{{{0}" +
                "{1}\"type\":{2},{0}" +
                "{1}\"value\":{3},{0}" +
                "{1}\"num_children\":{4},{0}" +
                "{1}\"children\":[" +
                  (num_children > 0 ? "{0}{1}{1}{5}{0}{1}" : "") +
                  "]{0}" +
                "}}",
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
            if (s == null) return JSON_NULL;
            //return System.Web.Helpers.Json.Encode(s);
            return "\"" + s + "\"";  // TODO: escape
        }

        public static JsonIr ListToJSON<T>(List<T> entities_list) where T : ENTITY
        {
            if (entities_list == null)
                throw new ArgumentNullException();

            JsonIr irList = new JsonIr(typeof(T).Name + "_LIST");
            foreach (ENTITY e in entities_list)
                irList.AppendChild(e.ToJSON());
            return irList;
        }

        public static JsonIr GetIrNull()
        {
            return JsonIrNull.Get();
        }

        private class JsonIrNull : JsonIr
        {
            private static JsonIrNull instance;

            private JsonIrNull() : base(JSON_NULL) { }

            public static JsonIrNull Get()
            {
                if (instance == null)
                    instance = new JsonIrNull();
                return instance;
            }

            public override string Serialize(bool indentation)
            {
                return JSON_NULL;
            }
        }
    }
}
