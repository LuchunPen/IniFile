/*
Copyright (c) Luchunpen (bwolf88).  All rights reserved.
Date: 25.07.2016 18:37:20
*/

using System;
using System.Collections.Generic;

//using Nano3;

namespace System.IO
{
    public class IniFile
    {
        private string _fileName;
        private string _filePath;
        private string _defaultSection;

        public string FileName { get { return _fileName; } }
        public string FilePath { get { return _filePath; } }
        public string DefaultSection
        {
            get { return _defaultSection; }
            set
            {
                if (value == null) return;
                if (_defaultSection == null)
                {
                    _defaultSection = value;
                    if (!_iniStorage.ContainsKey(_defaultSection))
                    {
                        _iniStorage.Add(_defaultSection, new Dictionary<string, string>());
                        return;
                    }
                }
                if (_defaultSection.Equals(value)) return;

                Dictionary<string, string> defSect = null;
                if (_defaultSection != null){
                    _iniStorage.TryGetValue(_defaultSection, out defSect);
                }
                if (defSect == null) { defSect = new Dictionary<string, string>(); }
                _iniStorage.Remove(_defaultSection);
                _defaultSection = value;
                _iniStorage.Add(_defaultSection, defSect);
            }
        }

        private Dictionary<string, Dictionary<string, string>> _iniStorage = new Dictionary<string, Dictionary<string, string>>();

        public IniFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { throw new ArgumentNullException("File name is null or empty"); }
            _fileName = fileName;
        }

        public static IniFile Load(string directory, string fileName)
        {
            IniFile ini = new IniFile(fileName);
            string fullPath = directory + @"\" + fileName + ".ini";

            try
            {
                bool fE = File.Exists(fullPath);
                if (!fE) { return ini; }

                string[] str = File.ReadAllLines(fullPath);
                ini._fileName = fileName;
                ini._filePath = directory;

                if (str == null || str.Length == 0) { return ini; }

                string currentSection = null;
                for (int i = 0; i < str.Length; i++)
                {
                    string tSect = ParseSection(str[i]);
                    if (tSect != null) { currentSection = tSect; continue; }

                    string k = null, v = null;
                    bool parse = ParseKeyValue(str[i], ref k, ref v);
                    if (parse && !string.IsNullOrEmpty(currentSection)){
                        ini.Write(currentSection, k, v);
                    }
                }
                return ini;
            }
            catch(Exception){ return ini; }
        }

        private static string ParseSection(string line)
        {
            if (!line.StartsWith("[")) return null;
            if (!line.EndsWith("]")) return null;
            if (line.Length < 3) return null;
            return line.Substring(1, line.Length - 2);
        }

        private static bool ParseKeyValue(string line, ref string key, ref string value)
        {
            int i;
            if ((i = line.IndexOf('=')) <= 0) return false;

            int j = line.Length - i - 1;
            key = line.Substring(0, i).Trim();
            if (key.Length <= 0) return false;

            value = (j > 0) ? (line.Substring(i + 1, j).Trim()) : ("");
            return true;
        }

        public void Write(string key, string value)
        {
            if (!string.IsNullOrEmpty(_defaultSection)){
                Write(_defaultSection, key, value);
            }
        }

        public void Write(string section, string key, string value)
        {
            if (string.IsNullOrEmpty(section)) { throw new ArgumentNullException("Section is null or empty"); }
            if (string.IsNullOrEmpty(key)) { throw new ArgumentNullException("Key is null or empty"); }

            Dictionary<string, string> dSect;
            _iniStorage.TryGetValue(section, out dSect);
            if (dSect == null) {
                dSect = new Dictionary<string, string>();
                _iniStorage[section] = dSect;
            }

            dSect[key] = value;
        }

        public string ReadValue(string key)
        {
            if (string.IsNullOrEmpty(_defaultSection)) return null;
            return ReadValue(_defaultSection, key);
        }

        public string ReadValue(string section, string key)
        {
            if (string.IsNullOrEmpty(section)) { throw new ArgumentNullException("Section is null or empty"); }
            if (string.IsNullOrEmpty(key)) { throw new ArgumentNullException("Key is null or empty"); }

            Dictionary<string, string> dSect;
            _iniStorage.TryGetValue(section, out dSect);

            string result = null;
            if (dSect != null) { dSect.TryGetValue(key, out result); }
            return result;
        }
        public string[] GetSections()
        {
            List<string> sect = new List<string>();
            foreach(string key in _iniStorage.Keys)
            {
                sect.Add(key);
            }
            return sect.ToArray();
        }
        public Dictionary<string, string> GetSectionKeyValues(string section)
        {
            Dictionary<string, string> dSect;
            _iniStorage.TryGetValue(section, out dSect);
            return dSect;
        }

        public void Save(string directory)
        {
            if (string.IsNullOrEmpty(directory)){
                _filePath = Environment.CurrentDirectory;
            }
            else { _filePath = directory; }

            bool dE = Directory.Exists(_filePath);
            if (!dE)
            {
                try { Directory.CreateDirectory(_filePath); }
                catch (Exception ex) {
                    //AppLogger.Log(ex);
                }
            }

            List<string> ini = new List<string>();
            foreach (string key in _iniStorage.Keys)
            {
                Dictionary<string, string> dSect = _iniStorage[key];
                if (dSect.Count == 0) { continue; }

                ini.Add("[" + key + "]");
                foreach (string dKey in dSect.Keys)
                {
                    string dValue = dSect[dKey];
                    string kv = dKey + "=" + dValue;
                    ini.Add(kv);
                }
                ini.Add("");
            }

            string fullPath = FilePath + @"\" + FileName + ".ini";
            bool fE = System.IO.File.Exists(fullPath);
            if (!fE || (fE && !FileInUse(fullPath))){
                File.WriteAllLines(fullPath, ini.ToArray());
            }
            else {
                //AppLogger.Log("File in use: " + fullPath);
            }
        }
        private static bool FileInUse(string fullFilePath)
        {
            FileStream stream = null;
            try { stream = File.Open(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.None); }
            catch (IOException){ return true; }
            finally{ if (stream != null) { stream.Close(); } }
            return false;
        }
    }
}
