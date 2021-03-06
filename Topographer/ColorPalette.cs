﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Topographer
{
    public class ColorPalette
    {
        private static Dictionary<String, Color> blockTable = null;
        private static Dictionary<byte, Color> biomeTable = null;

        private static void LoadBlockTable()
        {
            blockTable = new Dictionary<String, Color>();

            Regex linePattern = new Regex(@"^([0-9:,]+);(?:([0-9,]+);)?([0-9a-fA-F]{6,8})\s*(?:#.*)?$");
            Regex idPattern = new Regex(@"^\d+(:?\:\d+)?$");
            Regex biomePattern = new Regex(@"^\d+$");

            String[] lines = File.ReadAllLines(String.Format("{0}{1}Blocks.txt",Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.DirectorySeparatorChar));

            foreach (String line in lines)
            {
                Match m = linePattern.Match(line);
                if (m.Groups.Count >= 3)
                {
                    Color color = Color.FromArgb(Convert.ToInt32(m.Groups[m.Groups.Count - 1].Value.PadLeft(8, 'f'), 16));

                    List<String> append = new List<string>();
                    if (m.Groups.Count == 4)
                    {
                        String[] biomes = m.Groups[2].Value.Split(',');
                        foreach (String biome in biomes)
                        {
                            if (biomePattern.IsMatch(biome))
                            {
                                append.Add(biome);
                            }
                        }
                    }

                    String[] ids = m.Groups[1].Value.Split(',');
                    foreach (String id in ids)
                    {
                        if (idPattern.IsMatch(id))
                        {
                            if (append.Count > 0)
                            {
                                foreach (String s in append)
                                    blockTable.Add(String.Format("{0}b{1}", id, s), color);
                            }
                            else
                                blockTable.Add(id, color);
                        }
                    }
                }
                #if DEBUG
                else
                {
                    if(line.Trim().Length > 0)
                        throw new Exception(String.Format("Malformed line:\"{0}\"", line));
                }
                #endif
            }
        }

        private static void LoadBiomeTable()
        {
            biomeTable = new Dictionary<byte, Color>();
            
            String[] lines = File.ReadAllLines(String.Format("{0}{1}Biomes.txt", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.DirectorySeparatorChar));
            Regex pattern = new Regex(@"^([0-9,]+);([0-9a-fA-F]{6})\s*(?:#.*)?$");
            foreach (String line in lines)
            {
                Match m = pattern.Match(line);
                if (m.Groups.Count == 3)
                {
                    Color color = Color.FromArgb(Convert.ToInt32(m.Groups[2].Value.PadLeft(8, 'f'), 16));
                    String[] ids = m.Groups[1].Value.Split(',');
                    foreach (String id in ids)
                    {
                        biomeTable.Add(byte.Parse(id), color);
                    }
                }
                #if DEBUG
                else
                {
                    if (line.Trim().Length > 0)
                        throw new Exception(String.Format("Malformed line:\"{0}\"", line));
                }
                #endif
            }

        }

        public static Color Lookup(int block, int data, byte biome)
        {
            if (blockTable == null)
                LoadBlockTable();

            String idDataBiome = String.Format("{0}:{1}b{2}", block, data, biome);
            String idData = String.Format("{0}:{1}", block, data);
            String idBiome = String.Format("{0}b{1}", block, biome);
            String id = block.ToString();

            if (blockTable.ContainsKey(idDataBiome))
                return blockTable[idDataBiome];
            else if (blockTable.ContainsKey(idData))
                return blockTable[idData];
            else if (blockTable.ContainsKey(idBiome))
                return blockTable[idBiome];
            else if (blockTable.ContainsKey(id))
                return blockTable[id];
            else
                return Color.Black;
        }

        public static Color Lookup(byte biome)
        {
            if (biomeTable == null)
                LoadBiomeTable();

            if (biomeTable.ContainsKey(biome))
                return biomeTable[biome];
            else
                return Color.Black;
        }
    }
}
