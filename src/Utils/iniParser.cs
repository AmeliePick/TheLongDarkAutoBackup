using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TheLongDarkAutoBackup.Utils
{
    class IniParser
    {
        private string fileName;
        private string[] fileSource;


        public IniParser(string fileName)
        {
            this.fileName = fileName;
            LoadFile(fileName);
        }


        private void ReadFile(string file)
        {
            try
            {
                fileSource = File.ReadAllLines(file);
            }
            catch { }
        }


        public void LoadFile(string path)
        {
            this.fileName = path;
            ReadFile(path);
        }


        public Dictionary<string, string> GetValuesFromSection(string sectionName)
        {
            Dictionary<string, string> sectionSource = null;
            foreach (string line in fileSource)
            {
                // Find the section in the file
                if (line == '[' + sectionName + ']')
                {
                    sectionSource = new Dictionary<string, string>();
                    continue;
                }

                // Get all paramaters from the section.
                if (sectionSource != null && line != "" && !line.Contains('[') && !line.Contains(';'))
                {
                    // Parse the string.
                    string key = "", value = "";
                    bool stopWord = false;
                    foreach (char symbol in line)
                    {
                        if (symbol == ' ' || symbol == '=')
                        {
                            stopWord = true;
                            continue;
                        }
                        else if (stopWord == false)
                            key += symbol;
                        else if (stopWord == true)
                            value += symbol;
                    }
                    sectionSource.Add(key, value);
                }
                else if (sectionSource != null && line.Contains('['))
                    break;
            }
            return sectionSource;
        }

        public string GetValue(string sectionName, string parameter)
        {
            string value = null;
            foreach (string line in fileSource)
            {
                if (line == "[" + sectionName + "]")
                {
                    value = "";
                    continue;
                }
                else if (value != null && line.Contains("[")) break;

                if (value != null && line.Contains(parameter))
                {
                    value = line;

                    if (value.IndexOf('=') + 1 >= 0 && value[value.IndexOf('=') - 1] == ' ')
                        value = value.Remove(value.IndexOf('=') - 1, 1);

                    if (value.IndexOf('=') + 1 < value.Length && value[value.IndexOf('=') + 1] == ' ')
                        value = value.Remove(value.IndexOf('=') + 1, 1);

                    value = value.Replace("=", "").Replace(parameter, "");
                    break;
                }
            }

            return value != null ? value : "";
        }

        public void SetValue(string sectionName, string parameter, object value)
        {
            bool isSection = false;
            string line = "";
            for (int lineNumber = 0; lineNumber < fileSource.Length; ++lineNumber)
            {
                line = fileSource[lineNumber];

                if (line == "[" + sectionName + "]")
                {
                    isSection = true;
                    continue;
                }
                else if (isSection && line.Contains("[")) break;

                if(isSection && line.Contains(parameter))
                {
                    fileSource[lineNumber] = parameter + " = " + value.ToString();
                    File.WriteAllLines(fileName, fileSource);

                    return;
                }
            }
        }

    }
}