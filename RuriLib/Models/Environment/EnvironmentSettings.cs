using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Environment;

/// <summary>
///     Settings for customizable data types used in the library.
/// </summary>
public class EnvironmentSettings
{
    public List<WordlistType> WordlistTypes { get; set; } = new();
    public List<CustomStatus> CustomStatuses { get; set; } = new();
    public List<ExportFormat> ExportFormats { get; set; } = new();

    /// <summary>
    ///     Automatically recognizes a Wordlist Type between the ones available by matching the Regex patterns and returning
    ///     the first successful match.
    /// </summary>
    /// <param name="data">The data for which you want to recognize the Wordlist Type</param>
    /// <returns>The correct Wordlist Type or (if every Regex match failed) the first one</returns>
    public string RecognizeWordlistType(string data)
    {
        foreach (var type in WordlistTypes)
            if (Regex.Match(data, type.Regex).Success)
                return type.Name;

        return WordlistTypes.First().Name;
    }

    /// <summary>
    ///     Parses the EnvironmentSettings from a file.
    /// </summary>
    /// <param name="envFile">The .ini file of the settings</param>
    /// <returns>The loaded EnvironmentSettings object</returns>
    public static EnvironmentSettings FromIni(string envFile)
    {
        var env = new EnvironmentSettings();
        var lines = File.ReadAllLines(envFile).Where(l => !string.IsNullOrEmpty(l)).ToArray();
        for (var i = 0; i < lines.Count(); i++)
        {
            var line = lines[i];
            if (line.StartsWith("#")) continue;
            if (line.StartsWith("["))
            {
                Type type;
                var header = line;
                switch (line.Trim())
                {
                    case "[WORDLIST TYPE]":
                        type = typeof(WordlistType);
                        break;
                    case "[CUSTOM STATUS]":
                        type = typeof(CustomStatus);
                        break;
                    case "[EXPORT FORMAT]":
                        type = typeof(ExportFormat);
                        break;
                    default: throw new Exception("Unrecognized ini header");
                }

                var parameters = new List<string>();
                var j = i + 1;
                for (; j < lines.Count(); j++)
                {
                    line = lines[j];
                    if (line.StartsWith("[")) break;
                    if (line.Trim() == string.Empty || line.StartsWith("#")) continue;
                    parameters.Add(line);
                }

                switch (header)
                {
                    case "[WORDLIST TYPE]":
                        env.WordlistTypes.Add((WordlistType)ParseObjectFromIni(type, parameters));
                        break;
                    case "[CUSTOM STATUS]":
                        env.CustomStatuses.Add((CustomStatus)ParseObjectFromIni(type, parameters));
                        break;
                    case "[EXPORT FORMAT]":
                        env.ExportFormats.Add((ExportFormat)ParseObjectFromIni(type, parameters));
                        break;
                }

                i = j - 1;
            }
        }

        return env;
    }

    private static object ParseObjectFromIni(Type type, List<string> parameters)
    {
        var obj = Activator.CreateInstance(type);
        foreach (var pair in parameters
                     .Where(p => !string.IsNullOrEmpty(p))
                     .Select(p => p.Split(new[] { '=' }, 2)))
        {
            var prop = type.GetProperty(pair[0]);
            var propObj = prop.GetValue(obj);
            dynamic value = null;

            switch (propObj)
            {
                case string x:
                    value = pair[1];
                    break;

                case int x:
                    value = int.Parse(pair[1]);
                    break;

                case bool x:
                    value = bool.Parse(pair[1]);
                    break;

                case string[] x:
                    value = pair[1].Split(',');
                    break;
            }

            prop.SetValue(obj, value);
        }

        return obj;
    }
}