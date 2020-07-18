﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Collections;
using System.Text.RegularExpressions;

namespace Trader
{
    public class Parser
    {

        public static void parse()
        {
            string writePath = Application.StartupPath + @"\base.txt";
            string[] lines = File.ReadAllLines(writePath);
            for (int i = 0; i < lines.Length; i++)
                lines[i] += ",";
            File.WriteAllLines(writePath, lines);
        }

        public static void parse3()
        {
            string path = Application.StartupPath + @"\weapons.txt";
            string paintkitsPath = Application.StartupPath + @"\paintkits.json";
            string floatsPath = Application.StartupPath + @"\floats3.json";
            string writePath = Application.StartupPath + @"\baseFull.txt";
            string json = File.ReadAllText(path);
            string paintKitsJson = File.ReadAllText(paintkitsPath);
            string floatsJson = File.ReadAllText(floatsPath);
            WeaponClass weapon = JsonSerializer.Deserialize<WeaponClass>(json);
            Paintkits paintkits = JsonSerializer.Deserialize<Paintkits>(paintKitsJson);
            FloatPaintKit floatPaintKit = JsonSerializer.Deserialize<FloatPaintKit>(floatsJson); 

            List<string> content = new List<string>();

            foreach(KeyValuePair<string, Weapon> i in weapon.Weapons)
            {
                List<WeaponRow> rows = new List<WeaponRow>();
                Weapon q = i.Value;
                for (int t = 0; t < q.paintkit_names.Count; t++)
                {
                    WeaponRow newRow;
                    newRow.weapon = i.Key;
                    newRow.name = paintkits.names[q.paintkit_names[t]];
                    newRow.rarity = "" + q.paintkit_rarities[t];
                    newRow.min = 0.0f;
                    newRow.max = 1.0f;

                    if(floatPaintKit.pairs.ContainsKey(q.paintkit_names[t]))
                    {
                        FloatRange thisRange = floatPaintKit.pairs[q.paintkit_names[t]];
                        newRow.min = thisRange.min;
                        newRow.max = thisRange.max;
                    }

                    rows.Add(newRow);
                }
                rows.Sort(new Comparator());

                foreach(WeaponRow row in rows)
                {
                    content.Add(row.weapon + " | " + row.name + "," + row.rarity + "," + row.min.ToString("0.00") + "," + row.max.ToString("0.00"));
                }
            }

            File.WriteAllLines(writePath, content);
        }


        public static void parse4()
        {
            string path = Application.StartupPath + @"\paintkits.format";
            string writePath = Application.StartupPath + @"\floats2.txt";

            string text = File.ReadAllText(path);
            string[] bits = text.Split('}');
            char[] charList = { '\"' };
            char[] spaces = { ' ', '\t', '\"', '\n' };

            List<string> output = new List<string>();

            foreach (string q in bits)
            {
                if(q.IndexOf("wear_remap_min") != -1 || q.IndexOf("wear_remap_max") != -1)
                {
                    string current = q.Trim(charList);
                    string[] lines = current.Split('\n');
                    string name = "";
                    float min = 0.0f;
                    float max = 1.0f;

                    foreach(string xq in lines)
                    {
                        int found = -1;
                        if((found = xq.IndexOf("name")) != -1)
                        {
                            name = xq.Substring(found + 4).Trim(spaces);
                        }
                        else if ((found = xq.IndexOf("wear_remap_min")) != -1)
                        {
                            string newStr = xq.Substring(found + 14).Trim(spaces);
                            newStr = newStr.Substring(0, 4);
                            //MessageBox.Show(newStr);
                            min = float.Parse(newStr);
                        }
                        else if ((found = xq.IndexOf("wear_remap_max")) != -1)
                        {
                            string newStr = xq.Substring(found + 14).Trim(spaces);
                            newStr = newStr.Substring(0, 4);
                            max = float.Parse(newStr);
                        }
                    }

                    name.TrimEnd(spaces);
                    Regex rgx = new Regex("[^a-zA-Z0-9-_]");
                    name = rgx.Replace(name, "");
                    string newLine = name + "*" + min.ToString("0.00") + "#" + max.ToString("0.00");
                    output.Add(newLine);
                }
            }

            File.WriteAllLines(writePath, output);
        }

        public static void parse5()
        {
            string path = Application.StartupPath + @"\floats2.txt";
            string outPath = Application.StartupPath + @"\floats3.txt";
            string[] lines = File.ReadAllLines(path);

            List<string> output = new List<string>();

            for (int i = 0; i < lines.Length; i += 2)
            {
                try
                {
                    int s1 = lines[i].IndexOf('*');
                    int s2 = lines[i].IndexOf('#');
                    string q = lines[i].Substring(0, s1);
                    string q1 = lines[i].Substring(s1+1, s2-s1-1);
                    string q2 = lines[i].Substring(s2+1);

                    string newLine = "\"" + q + "\": {\"min\":" + q1 + ",\"max\":" + q2 + "},";
                    output.Add(newLine);

                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            File.WriteAllLines(outPath, output);
        }

        public static void parse2()
        {
            string path = Application.StartupPath + @"\base.txt";
            string pathFull = Application.StartupPath + @"\baseFull.txt";
            string pathOut = Application.StartupPath + @"\baseFinally.txt";
            string[] lines = File.ReadAllLines(pathFull);
            string[] linesAdd = File.ReadAllLines(path);

            for(int i=0;i<lines.Length;i++)
            {
                string[] values = linesAdd[i].Split(',');
                lines[i] += "," + values[values.Length - 1];
            }

            File.WriteAllLines(pathOut, lines);

        }
    }

    

    public struct WeaponRow
    {
        public string weapon;
        public string name;
        public string rarity;
        public float min;
        public float max;
    }

    public class Comparator : IComparer<WeaponRow>
    {
        public int Compare(WeaponRow x, WeaponRow y)
        {
            return String.Compare(x.name, y.name);
        }
    }

    public class FloatPaintKit
    {
        public Dictionary<string, FloatRange> pairs { get; set; }
        public FloatPaintKit() { }
    }

    public class FloatRange
    {
        public float min { get; set; }
        public float max { get; set; }
        public FloatRange()
        {
            min = 0.0f;
            max = 1.0f;
        }
    };


    public class Paintkits
    {
        public Dictionary<string, string> names { get; set; }
        public Paintkits() { }
    }

    public class WeaponClass
    {
        public Dictionary<string, Weapon> Weapons { get; set; }

        public WeaponClass() { }
    }

    public class Weapon
    {
        public IList<string> paintkit_names { get; set; }
        public IList<int> paintkit_rarities {get; set;}
        public Weapon() { }
    }
}
