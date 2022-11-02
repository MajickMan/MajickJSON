using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MajickJSON
{
    public class MajickJsonObject
    {
        static string empty_array = "^(\"(\\w+?)\")\\s??:\\s??\\[\\]";
        static string object_array = "^(\"(\\w+?)\")\\s??:\\s??\\[{";
        static string attribute_array = "^(\"(\\w+?)\")\\s??:\\s??\\[\"??[\\w\\d\\W]";
        static string json_object = "^(\"(\\w+?)\")\\s??:\\s??{";
        static string json_attribute = "^(\"(\\w+?)\")\\s??:\\s??\"??[\\w\\d\\s\\W]";
        static Regex EmptyArray = new Regex(empty_array);
        static Regex ObjectArray = new Regex(object_array);
        static Regex AttributeArray = new Regex(attribute_array);
        static Regex JsonObjectPattern = new Regex(json_object);
        static Regex JsonAttributePattern = new Regex(json_attribute);
        private Dictionary<string, MajickJsonAttribute> _attributes;
        private Dictionary<string, MajickJsonObject> _objects;
        private Dictionary<string, IReadOnlyList<MajickJsonAttribute>> _attributeLists;
        private Dictionary<string, IReadOnlyList<MajickJsonObject>> _objectLists;
        public string Name { get; internal set; }
        public string RawText { get; internal set; }
        public IReadOnlyDictionary<string, MajickJsonAttribute> Attributes { get { return _attributes; } }
        public IReadOnlyDictionary<string, MajickJsonObject> Objects { get { return _objects; } }
        public IReadOnlyDictionary<string, IReadOnlyList<MajickJsonAttribute>> AttributeLists { get { return _attributeLists; } }
        public IReadOnlyDictionary<string, IReadOnlyList<MajickJsonObject>> ObjectLists { get { return _objectLists; } }
        public MajickJsonObject()
        {
            _attributes = new Dictionary<string, MajickJsonAttribute>();
            _objects = new Dictionary<string, MajickJsonObject>();
            _attributeLists = new Dictionary<string, IReadOnlyList<MajickJsonAttribute>>();
            _objectLists = new Dictionary<string, IReadOnlyList<MajickJsonObject>>();
        }
        public MajickJsonObject(string object_text)
        {
            _attributes = new Dictionary<string, MajickJsonAttribute>();
            _objects = new Dictionary<string, MajickJsonObject>();
            _attributeLists = new Dictionary<string, IReadOnlyList<MajickJsonAttribute>>();
            _objectLists = new Dictionary<string, IReadOnlyList<MajickJsonObject>>();

            string current_attribute_name;
            string object_clone = object_text.Replace("\\\"", "~%").Trim();
            if (object_clone.StartsWith("{")) { object_clone = object_clone.Substring(1).Trim(); }
            if (object_clone.EndsWith("}")) { object_clone = object_clone.Substring(0, object_clone.Length - 1).Trim(); }
            object_clone = object_clone.Trim();
            while (object_clone.Length > 0)
            {
                if (object_clone.StartsWith(",")) { object_clone = object_clone.Substring(1).Trim(); }
                Match EmptyArrayMatch = EmptyArray.Match(object_clone);
                Match ObjectArrayMatch = ObjectArray.Match(object_clone);
                Match AttributeArrayMatch = AttributeArray.Match(object_clone);
                Match JsonObjectMatch = JsonObjectPattern.Match(object_clone);
                Match JsonAttributeMatch = JsonAttributePattern.Match(object_clone);
                current_attribute_name = object_clone.Substring(0, object_clone.IndexOf(":")).Replace("\"", "").Trim();
                object_clone = object_clone.Substring(object_clone.IndexOf(":") + 1).Trim();
                if (ObjectArrayMatch.Index == 0 && ObjectArrayMatch.Success)
                {
                    List<MajickJsonObject> inner_object_list = new List<MajickJsonObject>();
                    int inner_array_length = 0;
                    string object_array_text = "";
                    char[] data_array = object_clone.ToArray();
                    int open = 0;
                    int close = 0;
                    foreach (char current_char in data_array)
                    {
                        inner_array_length += 1;
                        object_array_text += current_char;
                        if (current_char == '[') { open += 1; }
                        if (current_char == ']') { close += 1; }
                        if (open > 0 && open == close) { break; }
                    }
                    if (object_array_text == object_clone) { object_clone = ""; }
                    else { object_clone = object_clone.Substring(inner_array_length + 1).Trim(); }
                    object_array_text = object_array_text.Substring(1, object_array_text.Length - 2).Trim();
                    while (object_array_text.Length > 0)
                    {
                        int inner_object_length = 0;
                        string inner_object_text = "";
                        char[] object_array_data = object_array_text.ToArray();
                        int object_open = 0;
                        int object_close = 0;
                        foreach (char current_char in object_array_data)
                        {
                            inner_object_length += 1;
                            inner_object_text += current_char;
                            if (current_char == '{') { object_open += 1; }
                            if (current_char == '}') { object_close += 1; }
                            if (object_open > 0 && object_open == object_close) { break; }
                        }
                        if (inner_object_text == object_array_text) { object_array_text = ""; }
                        else { object_array_text = object_array_text.Substring(inner_object_length + 1).Trim(); }
                        if (object_array_text.StartsWith(",")) { object_array_text = object_array_text.Substring(1).Trim(); }
                        MajickJsonObject inner_object = new MajickJsonObject(inner_object_text);
                        inner_object_list.Add(inner_object);
                    }
                    AddObjectList(current_attribute_name, inner_object_list);
                }
                else if (EmptyArrayMatch.Index == 0 && EmptyArrayMatch.Success)
                {
                    AddAttributeList(current_attribute_name, new List<MajickJsonAttribute>());
                    if (object_clone == "[]") { object_clone = ""; }
                    else { object_clone = object_clone.Substring(3).Trim(); }
                }
                else if (AttributeArrayMatch.Index == 0 && AttributeArrayMatch.Success)
                {
                    List<MajickJsonAttribute> inner_attribute_array = new List<MajickJsonAttribute>();
                    int inner_array_length = 0;
                    string attribute_array_text = "";
                    char[] data_array = object_clone.ToArray();
                    int open = 0;
                    int close = 0;
                    foreach (char current_char in data_array)
                    {
                        inner_array_length += 1;
                        attribute_array_text += current_char;
                        if (current_char == '[') { open += 1; }
                        if (current_char == ']') { close += 1; }
                        if (open > 0 && open == close) { break; }
                    }
                    if (attribute_array_text == object_clone) { object_clone = ""; }
                    else { object_clone = object_clone.Substring(inner_array_length + 1).Trim(); }
                    if (attribute_array_text.StartsWith("[")) { attribute_array_text = attribute_array_text.Substring(1).Trim(); }
                    if (attribute_array_text.EndsWith("]")) { attribute_array_text = attribute_array_text.Substring(0, attribute_array_text.Length - 1).Trim(); }
                    if (attribute_array_text.Contains("\""))
                    {
                        while (attribute_array_text.Length > 0)
                        {
                            int array_item_length = 0;
                            string array_item_text = "";
                            char[] inner_data_array = attribute_array_text.ToArray();
                            int quotes = 0;
                            foreach (char current_char in inner_data_array)
                            {
                                array_item_length += 1;
                                array_item_text += current_char;
                                if (current_char == '\"') { quotes += 1; }
                                if (quotes == 2) { break; }
                            }
                            if (array_item_text == attribute_array_text) { attribute_array_text = ""; }
                            else { attribute_array_text = attribute_array_text.Substring(array_item_length + 1).Trim(); }
                            array_item_text = array_item_text.Substring(1, array_item_text.Length - 2).Trim();
                            array_item_text = array_item_text.Replace("~%", "\\\"");
                            MajickJsonAttribute inner_array_item = new MajickJsonAttribute(array_item_text);
                            inner_attribute_array.Add(inner_array_item);
                        }
                    }
                    else
                    {
                        if (attribute_array_text != "")
                        {
                            string[] attribute_data_array = attribute_array_text.Split(',');
                            foreach (string attribute_data in attribute_data_array)
                            {
                                MajickJsonAttribute inner_array_item = new MajickJsonAttribute(attribute_data, true);
                                inner_attribute_array.Add(inner_array_item);
                            }
                        }
                    }
                    AddAttributeList(current_attribute_name, inner_attribute_array);
                }
                else if (JsonObjectMatch.Index == 0 && JsonObjectMatch.Success)
                {
                    int inner_object_length = 0;
                    string inner_object_text = "";
                    char[] data_array = object_clone.ToArray();
                    int open = 0;
                    int close = 0;
                    foreach (char current_char in data_array)
                    {
                        inner_object_length += 1;
                        inner_object_text += current_char;
                        if (current_char == '{') { open += 1; }
                        if (current_char == '}') { close += 1; }
                        if (open > 0 && open == close) { break; }
                    }
                    if (inner_object_text == object_clone) { object_clone = ""; }
                    else { object_clone = object_clone.Substring(inner_object_length + 1).Trim(); }
                    AddObject(current_attribute_name, new MajickJsonObject(inner_object_text));
                }
                else if (JsonAttributeMatch.Index == 0 && JsonAttributeMatch.Success)
                {
                    int attribute_length = 0;
                    string attribute_text = "";
                    if (current_attribute_name == "_trace")
                    {
                        attribute_text = object_clone;
                        object_clone = "";
                        AddAttribute(current_attribute_name, attribute_text);
                    }
                    else if (object_clone.StartsWith("\""))
                    {
                        char[] data_array = object_clone.ToArray();
                        int quotes = 0;
                        foreach (char current_char in data_array)
                        {
                            attribute_length += 1;
                            attribute_text += current_char;
                            if (current_char == '\"') { quotes += 1; }
                            if (quotes == 2) { break; }
                        }
                        if (attribute_text == object_clone) { object_clone = ""; }
                        else { object_clone = object_clone.Substring(attribute_length + 1).Trim(); }
                        attribute_text = attribute_text.Substring(1, attribute_text.Length - 2).Trim();
                        attribute_text = attribute_text.Replace("~%", "\\\"");
                        AddAttribute(current_attribute_name, attribute_text);
                    }
                    else if (object_clone.Contains(","))
                    {
                        attribute_text = object_clone.Substring(0, object_clone.IndexOf(",")).Trim();
                        object_clone = object_clone.Substring(object_clone.IndexOf(",") + 1).Trim();
                        AddAttribute(current_attribute_name, attribute_text, true);
                    }
                    else
                    {
                        attribute_text = object_clone.Trim();
                        object_clone = "";
                        AddAttribute(current_attribute_name, attribute_text, true);
                    }
                }
                else
                {
                    //This should never be hit?
                }
            }
        }
        public void AddAttribute(string Name, string TextValue, bool IsNumeric = false) { _attributes.Add(Name, new MajickJsonAttribute(Name, TextValue, IsNumeric)); }
        public void AddObject(string Name, MajickJsonObject ObjectValue) { _objects.Add(Name, ObjectValue); }
        public void AddAttributeList(string Name, List<MajickJsonAttribute> AttributeList) { _attributeLists.Add(Name, AttributeList); }
        public void AddObjectList(string Name, List<MajickJsonObject> ObjectList) { _objectLists.Add(Name, ObjectList); }
        public string ToRawText(bool use_name = true)
        {
            string RawText = "";
            if (Name != "" & use_name) { RawText = "\"" + Name + "\":{"; }
            else RawText = "{";
            //fill the object values in here
            foreach (string AttributeName in Attributes.Keys)
            {
                if (Attributes[AttributeName].is_numeric) { RawText += Attributes[AttributeName].ToRawText().ToLower() + ","; }
                else
                {
                    if (Attributes[AttributeName].text_value != "") { RawText += Attributes[AttributeName].ToRawText() + ","; }
                    else { RawText += "\"" + AttributeName + "\":\"\","; }
                }
            }
            foreach (string ListName in AttributeLists.Keys)
            {
                RawText += "\"" + ListName + "\":[";
                foreach (MajickJsonAttribute Value in AttributeLists[ListName])
                {
                    RawText += Value.ToListText() + ",";
                }
                RawText = RawText.Substring(0, RawText.Length - 1);
                RawText += "],";
            }
            foreach (string ObjectName in Objects.Keys)
            {
                RawText += "\"" + ObjectName + "\":" + Objects[ObjectName].ToRawText(false) + ",";
            }
            foreach (string ObjListName in ObjectLists.Keys)
            {
                RawText += "\"" + ObjListName + "\":[";
                foreach (MajickJsonObject Value in ObjectLists[ObjListName])
                {
                    RawText += Value.ToRawText(false) + ",";
                }
                if (RawText.EndsWith(",")) { RawText = RawText.Substring(0, RawText.Length - 1); }
                RawText += "],";
            }
            if (RawText.EndsWith(",")) { RawText = RawText.Substring(0, RawText.Length - 1); }
            RawText += "}";
            return RawText;
        }
    }
    public class MajickJsonAttribute
    {
        public string name;
        public bool is_numeric;
        public string text_value;
        public MajickJsonAttribute()
        {
            name = "";
            is_numeric = false;
            text_value = "";
        }
        public MajickJsonAttribute(string new_name, string new_value, bool numeric = false)
        {
            name = new_name;
            text_value = new_value;
            is_numeric = numeric;
        }
        public MajickJsonAttribute(string new_value, bool numeric = false)
        {
            name = "";
            text_value = new_value;
            is_numeric = numeric;
        }
        public string ToRawText()
        {
            string RawText = "\"" + name + "\":";
            if (is_numeric) { RawText += text_value; }
            else { RawText += "\"" + text_value + "\""; }
            return RawText;
        }
        public string ToListText()
        {
            if (is_numeric) { return text_value; }
            else { return "\"" + text_value + "\""; }
        }
    }
}
