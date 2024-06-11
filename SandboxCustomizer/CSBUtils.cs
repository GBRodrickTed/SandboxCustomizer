using SandboxCustomizer.Info;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static UnityEngine.ResourceManagement.Util.BinaryStorageBuffer.TypeSerializer;

namespace SandboxCustomizer
{
    public static class CSBUtils
    {
        //not very idiot proof, thankfully i'm not an idiot :)
        //TODO: I'm an idiot
        public static string version = "1.0.0";

        public static void CreateContainer(string name, string path)
        {
            if (!string.Equals(".csb", Path.GetExtension(path), StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("Can't create field because \"" + path + "\" is not a csb");
                return;
            }
            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            bool field_found = file.Contains("[" + name + "]");

            if (field_found)
            {
                Debug.Log("Field \"" + name + "\" with same name already exists");
                return;
            }

            string new_field = "\n[" + name + "]\n{\n}";
            file += new_field;

            StreamWriter sw = new StreamWriter(path);
            sw.Write(file);
            sw.Close();
        }
        public static string CreateCSB(string name, string directory)
        {
            string path = Path.Combine(directory, name + ".csb");

            if (File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" already made");
                return path;
            }

            StreamWriter sw = new StreamWriter(path);
            sw.Write("|" + version + "|");
            sw.Close();

            return path;
        }

        public static List<string> GetFieldsInContainer(string file, int container_start)
        {
            List<string> fields = new List<string>();
            string string_o_fields = "";
            string jump_size = "";
            bool read_to_jump = false;
            int file_cursor = container_start;
            int parenthesis = 0;
            int curly_brackets = 1; //we're (idealy) starting right after the start bracket
            int brackets = 0;
            while (file_cursor < file.Count()) //MAYBE: Make this not require a loop
            {
                if (file[file_cursor] == '{') curly_brackets++;
                if (file[file_cursor] == '}') curly_brackets--;

                if (curly_brackets == 0) break;

                if (file[file_cursor] == '(') parenthesis++;
                if (file[file_cursor] == ')') parenthesis--;

                if (file[file_cursor] == '[') {
                    brackets++;
                    if (parenthesis == 1)
                    {
                        read_to_jump = true;
                        file_cursor++;
                        continue;
                    }
                }
                if (file[file_cursor] == ']') {
                    brackets--;
                    if (parenthesis == 1)
                    {
                        read_to_jump = false;
                        int jump = 0;
                        if (int.TryParse(jump_size, out jump))
                        {
                            // the 3 represents the ']){' we're (hopefully) jumping over
                            //Debug.Log("Jump: " + jump_size);
                            parenthesis--;
                            curly_brackets++;
                            file_cursor += (3 + jump);
                            jump_size = "";
                            continue;
                        }
                        else
                        {
                            //Debug.Log("(:/): " + jump_size);
                        }
                    }
                }

                if (read_to_jump)
                {
                    jump_size += file[file_cursor];
                }

                //string_o_fields += file[file_cursor];
                file_cursor++;
            }

            string_o_fields = file.Substring(container_start, file_cursor - container_start);

            fields = string_o_fields.Split('\n').ToList();

            // Makes the assumptions that the first and last list elements will be empty
            fields.RemoveAt(0);
            fields.RemoveAt(fields.Count - 1);
            return fields;
        }
        public static string RemoveFieldsInContainter(string file, int container_start)
        {
            List<string> fields = new List<string>();
            string string_o_fields = "";
            string jump_size = "";
            bool read_to_jump = false;
            int file_cursor = container_start;
            int parenthesis = 0;
            int curly_brackets = 1; //we're (idealy) starting right after the start bracket
            int brackets = 0;
            while (file_cursor < file.Count()) //TODO Make this not require a loop
            {
                if (file[file_cursor] == '{') curly_brackets++;
                if (file[file_cursor] == '}') curly_brackets--;

                if (curly_brackets == 0) break;

                if (file[file_cursor] == '(') parenthesis++;
                if (file[file_cursor] == ')') parenthesis--;

                if (file[file_cursor] == '[')
                {
                    brackets++;
                    if (parenthesis == 1)
                    {
                        read_to_jump = true;
                        file_cursor++;
                        continue;
                    }
                }
                if (file[file_cursor] == ']')
                {
                    brackets--;
                    if (parenthesis == 1)
                    {
                        read_to_jump = false;
                        int jump = 0;
                        if (int.TryParse(jump_size, out jump))
                        {
                            // the 3 represents the ']){' we're (hopefully) jumping over
                            //Debug.Log("Jump: " + jump_size);
                            parenthesis--;
                            curly_brackets++;
                            file_cursor += (3 + jump);
                            jump_size = "";
                            continue;
                        }
                        else
                        {
                            //Debug.Log("(:/): " + jump_size);
                        }
                    }
                }

                if (read_to_jump)
                {
                    jump_size += file[file_cursor];
                }

                //string_o_fields += file[file_cursor];
                file_cursor++;
            }
            string dest = file.Remove(container_start, file_cursor - container_start);
            return dest;
        }

        //It's not was doesn't work that scares me, it's what does.
        //Colors
        public static void CreateColorField(string container_name, Color color, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return;
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return;
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);
            file = RemoveFieldsInContainter(file, container_start);

            string color_field = "(color){" 
                + color.r + ","
                + color.g + "," 
                + color.b + ","
                + color.a
                + "}";
            fields.Add(color_field);

            string string_o_fields_mk2 = "";
            string_o_fields_mk2 += "\n";
            foreach (string field in fields)
            {
                string_o_fields_mk2 += field + "\n";
            }
            file = file.Insert(container_start, string_o_fields_mk2);

            StreamWriter sw = new StreamWriter(path);
            sw.Write(file);
            sw.Close();
        }
        public static Color GetColorField(string container_name, int index, string path)
        {
            //TODO: probably could be more efficient
            Color color = Color.clear;
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return Color.clear;
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return Color.clear;
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);

            if (index >= fields.Count || index < 0)
            {
                Debug.Log("Field index out of range");
                return Color.clear;
            }

            if (!fields[index].Contains("(color"))
            {
                Debug.Log("Color could not be found at field index: " + index);
                return Color.clear;
            }
            int field_cursor = fields[index].IndexOf("){") + "){".Length;

            //assumes format's gonna look like (field type){...} exactly
            string string_o_values = fields[index].Substring(field_cursor, (fields[index].Length - 1) - field_cursor);

            List<string> values = new List<string>();
            values = string_o_values.Split(',').ToList();

            if (values.Count() > 2)
            {
                float[] maybe_values;

                maybe_values = new float[values.Count];

                for (int i = 0; i < values.Count; i++)
                {
                    if (!float.TryParse(values[i], out maybe_values[i]))
                    {
                        Debug.Log(container_name + ": " + index + " | Could not determine color value " + i);
                        maybe_values[i] = 0;
                    }
                }
                color.r = maybe_values[0];
                color.g = maybe_values[1];
                color.b = maybe_values[2];
                if (values.Count() > 3)
                {
                    color.a = maybe_values[3];
                }
                else
                {
                    color.a = 1;
                }
            } else
            {
                Debug.Log("Not enough values in color field " + container_name + ": " + index + " to determine color");
                return Color.clear;
            }
            return color;
        }
        public static void SetColorField(string container_name, Color color, int index, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return;
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return;
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);
            file = RemoveFieldsInContainter(file, container_start);

            if (index >= fields.Count || index < 0)
            {
                Debug.Log("Field index out of range");
                return;
            }

            string color_field = "(color){"
                + color.r + ","
                + color.g + ","
                + color.b + ","
                + color.a
                + "}";
            fields[index] = color_field;

            string string_o_fields_mk2 = "";
            string_o_fields_mk2 += "\n";
            foreach (string field in fields)
            {
                string_o_fields_mk2 += field + "\n";
            }
            file = file.Insert(container_start, string_o_fields_mk2);

            StreamWriter sw = new StreamWriter(path);
            sw.Write(file);
            sw.Close();
        }

        //Floats
        public static void CreateFloatField(string container_name, float num, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return;
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return;
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);
            file = RemoveFieldsInContainter(file, container_start);

            string float_field = "(float){" + num + "}";
            fields.Add(float_field);

            string string_o_fields_mk2 = "";
            string_o_fields_mk2 += "\n";
            foreach (string field in fields)
            {
                string_o_fields_mk2 += field + "\n";
            }
            file = file.Insert(container_start, string_o_fields_mk2);

            StreamWriter sw = new StreamWriter(path);
            sw.Write(file);
            sw.Close();
        }
        public static float GetFloatField(string container_name, int index, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return 0;
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return 0;
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);

            if (index >= fields.Count || index < 0)
            {
                Debug.Log("Field index out of range");
                return 0;
            }

            if (!fields[index].Contains("(float)"))
            {
                Debug.Log("Float could not be found at field index: " + index);
                return 0;
            }
            int field_cursor = fields[index].IndexOf("){") + "){".Length;

            //assumes format's gonna look like (field type){...} exactly
            string le_value = fields[index].Substring(field_cursor, (fields[index].Length - 1) - field_cursor);

            float dest;
            if (!float.TryParse(le_value, out dest))
            {
                Debug.Log(container_name + ": " + index + " | Could not determine float value");
                return 0;
            }
            return dest;
        }
        public static void SetFloatField(string container_name, float num, int index, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return;
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return;
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);
            file = RemoveFieldsInContainter(file, container_start);

            if (index >= fields.Count || index < 0)
            {
                Debug.Log("Field index out of range");
                return;
            }

            string float_field = "(float){" + num + "}";
            fields[index] = float_field;

            string string_o_fields_mk2 = "";
            string_o_fields_mk2 += "\n";
            foreach (string field in fields)
            {
                string_o_fields_mk2 += field + "\n";
            }
            file = file.Insert(container_start, string_o_fields_mk2);

            StreamWriter sw = new StreamWriter(path);
            sw.Write(file);
            sw.Close();
        }

        //Bool
        public static void CreateBoolField(string container_name, bool value, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return;
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return;
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);
            file = RemoveFieldsInContainter(file, container_start);

            string bool_field = "(bool){" + value + "}";
            fields.Add(bool_field);

            string string_o_fields_mk2 = "";
            string_o_fields_mk2 += "\n";
            foreach (string field in fields)
            {
                string_o_fields_mk2 += field + "\n";
            }
            file = file.Insert(container_start, string_o_fields_mk2);

            StreamWriter sw = new StreamWriter(path);
            sw.Write(file);
            sw.Close();
        }
        public static bool GetBoolField(string container_name, int index, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return false;
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return false;
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);

            if (index >= fields.Count || index < 0)
            {
                Debug.Log("Field index out of range");
                return false;
            }

            if (!fields[index].Contains("(bool"))
            {
                Debug.Log("Bool could not be found at field index: " + index);
                return false;
            }
            int field_cursor = fields[index].IndexOf("){") + "){".Length;

            //assumes format's gonna look like (field type){...} exactly
            string le_value = fields[index].Substring(field_cursor, (fields[index].Length - 1) - field_cursor);

            bool dest;
            if (!bool.TryParse(le_value, out dest))
            {
                Debug.Log(container_name + ": " + index + " | Could not determine boolean value");
                return false;
            }
            return dest;
        }
        public static void SetBoolField(string container_name, bool value, int index, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return;
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return;
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);
            file = RemoveFieldsInContainter(file, container_start);

            if (index >= fields.Count || index < 0)
            {
                Debug.Log("Field index out of range");
                return;
            }

            string bool_field = "(bool){" + value + "}";
            fields[index] = bool_field;

            string string_o_fields_mk2 = "";
            string_o_fields_mk2 += "\n";
            foreach (string field in fields)
            {
                string_o_fields_mk2 += field + "\n";
            }
            file = file.Insert(container_start, string_o_fields_mk2);

            StreamWriter sw = new StreamWriter(path);
            sw.Write(file);
            sw.Close();
        }

        //Base64 (would have been so peak if it didn't make the file so big)
        public static void CreateBase64Field(string container_name, string data, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return;
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return;
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);
            file = RemoveFieldsInContainter(file, container_start);

            string data_field = "(base64[" + data.Length + "]){" + data + "}";
            fields.Add(data_field);

            string string_o_fields_mk2 = "";
            string_o_fields_mk2 += "\n";
            foreach (string field in fields)
            {
                string_o_fields_mk2 += field + "\n";
            }
            file = file.Insert(container_start, string_o_fields_mk2);

            StreamWriter sw = new StreamWriter(path);
            sw.Write(file);
            sw.Close();
        }
        public static string GetBase64Field(string container_name, int index, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return "";
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return "";
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);

            if (index >= fields.Count || index < 0)
            {
                Debug.Log("Field index out of range");
                return "";
            }

            if (!fields[index].Contains("(base64"))
            {
                Debug.Log("Base64 could not be found at field index: " + index);
                return "";
            }
            int field_cursor = fields[index].IndexOf("){") + "){".Length;

            //assumes format's gonna look like (field type){...} exactly
            string le_value = fields[index].Substring(field_cursor, (fields[index].Length - 1) - field_cursor);

            return le_value;
        }
        public static void SetBase64Field(string container_name, string data, int index, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return;
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return;
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);
            file = RemoveFieldsInContainter(file, container_start);

            if (index >= fields.Count || index < 0)
            {
                Debug.Log("Field index out of range");
                return;
            }

            string data_field = "(base64[" + data.Length + "]){" + data + "}";
            fields[index] = data_field;

            string string_o_fields_mk2 = "";
            string_o_fields_mk2 += "\n";
            foreach (string field in fields)
            {
                string_o_fields_mk2 += field + "\n";
            }
            file = file.Insert(container_start, string_o_fields_mk2);

            StreamWriter sw = new StreamWriter(path);
            sw.Write(file);
            sw.Close();
        }

        //String
        public static void CreateStringField(string container_name, string str, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return;
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return;
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);
            file = RemoveFieldsInContainter(file, container_start);

            string string_field = "(string){" + str + "}";
            fields.Add(string_field);

            string string_o_fields_mk2 = "";
            string_o_fields_mk2 += "\n";
            foreach (string field in fields)
            {
                string_o_fields_mk2 += field + "\n";
            }
            file = file.Insert(container_start, string_o_fields_mk2);

            StreamWriter sw = new StreamWriter(path);
            sw.Write(file);
            sw.Close();
        }
        public static string GetStringField(string container_name, int index, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return "";
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return "";
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);

            if (index >= fields.Count || index < 0)
            {
                Debug.Log("Field index out of range");
                return "";
            }

            if (!fields[index].Contains("(string"))
            {
                Debug.Log("String could not be found at field index: " + index);
                return "";
            }
            int field_cursor = fields[index].IndexOf("){") + "){".Length;

            //assumes format's gonna look like (field type){...} exactly
            string le_value = fields[index].Substring(field_cursor, (fields[index].Length - 1) - field_cursor);

            return le_value;
        }
        public static void SetStringField(string container_name, string str, int index, string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File \"" + path + "\" is not found");
                return;
            }

            StreamReader sr = new StreamReader(path);
            string file = sr.ReadToEnd();
            sr.Close();

            string what_we_want = "[" + container_name + "]\n{";
            bool container_found = file.Contains(what_we_want);
            if (!container_found)
            {
                Debug.Log("Container " + container_name + " is not found");
                return;
            }

            int container_start = file.IndexOf(what_we_want) + what_we_want.Length;
            List<string> fields = GetFieldsInContainer(file, container_start);
            file = RemoveFieldsInContainter(file, container_start);

            if (index >= fields.Count || index < 0)
            {
                Debug.Log("Field index out of range");
                return;
            }

            string string_field = "(string){" + str + "}";
            fields[index] = string_field;

            string string_o_fields_mk2 = "";
            string_o_fields_mk2 += "\n";
            foreach (string field in fields)
            {
                string_o_fields_mk2 += field + "\n";
            }
            file = file.Insert(container_start, string_o_fields_mk2);

            StreamWriter sw = new StreamWriter(path);
            sw.Write(file);
            sw.Close();
        }

    }
}
