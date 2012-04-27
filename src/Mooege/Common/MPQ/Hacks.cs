using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Mooege.Common.MPQ.FileFormats.Types;
using System.Linq;
using Mooege.Core.GS.Common.Types.SNO;
using Mooege.Common.MPQ.FileFormats;
using Mooege.Core.GS.Items;
using System.Text.RegularExpressions;
using Mooege.Core.GS.Common.Types.TagMap;
using Mooege.Common.Logging;
using Mooege.Common.Versions;

namespace Mooege.Common.MPQ
{
    public class Hacks
    {
        public static Logger Logger = LogManager.CreateLogger();

        public static StreamWriter fout;

        public static Dictionary<int, string> SNOS;

        public static Dictionary<int, string> TagNames;

        private static string logAffix = VersionInfo.MPQ.RequiredPatchVersion.ToString();  // current d3 build version

        static Hacks()
        {
            SNOS = new Dictionary<int, string>();
            foreach (var assetGroup in MPQStorage.Data.Assets)
            {
                foreach (var asset in assetGroup.Value.Values)
                {
                    SNOS[asset.SNOId] = Path.GetFileName(asset.FileName);
                }
            }

            TagNames = new Dictionary<int, string>();
            using (var tagNameFile = File.OpenText(Path.Combine(Storage.Config.Instance.Root, "tagid_names.txt")))
            {
                string line;
                while ((line = tagNameFile.ReadLine()) != null)
                {
                    Match m = Regex.Match(line, @"(\S+) (\d+) (.+)");
                    if (m.Success)
                    {
                        TagNames[int.Parse(m.Groups[2].Value)] = m.Groups[3].Value;
                    }
                }
            }
        }

        public static StreamWriter CreateDesktopLogFile(string name)
        {
            return new StreamWriter(Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory),
                name + "_" + logAffix + ".txt"));
        }

        public static void Log(string msg, params object[] objs)
        {
            if (fout == null)
                fout = CreateDesktopLogFile("hacks");

            fout.WriteLine(msg, objs);
            fout.Flush();
        }

        public static string StringSNO(int sno)
        {
            if (SNOS.ContainsKey(sno))
                return SNOS[sno];
            else
                return "UnknownSNO";
        }

        public static string StringTagID(int tagID)
        {
            if (TagNames.ContainsKey(tagID))
                return TagNames[tagID];
            else
                return "UnknownTagID";
        }

        public static void DumpPowerTagmaps()
        {
            using (StreamWriter log = CreateDesktopLogFile("power_tagmaps"))
            {
                foreach (var powerEntry in MPQStorage.Data.Assets[SNOGroup.Power].Values)
                {
                    log.WriteLine("tagmaps for {0}", Path.GetFileName(powerEntry.FileName));
                    Power power = (Power)powerEntry.Data;

                    var tagmaps = new Dictionary<string, TagMap>()
                    {
                        { "TagMap", power.Powerdef.TagMap },
                        { "GeneralTagMap", power.Powerdef.GeneralTagMap },
                        { "PVPGeneralTagMap", power.Powerdef.PVPGeneralTagMap },
                        { "ContactTagMap0", power.Powerdef.ContactTagMap0 },
                        { "ContactTagMap1", power.Powerdef.ContactTagMap1 },
                        { "ContactTagMap2", power.Powerdef.ContactTagMap2 },
                        { "ContactTagMap3", power.Powerdef.ContactTagMap3 },
                        { "PVPContactTagMap0", power.Powerdef.PVPContactTagMap0 },
                        { "PVPContactTagMap1", power.Powerdef.PVPContactTagMap1 },
                        { "PVPContactTagMap2", power.Powerdef.PVPContactTagMap2 },
                        { "PVPContactTagMap3", power.Powerdef.PVPContactTagMap3 },
                    };

                    foreach (var kv in tagmaps)
                    {
                        DumpTagmap(log, kv.Key, kv.Value);
                    }

                    log.WriteLine("");
                    log.WriteLine("");
                }
            }
        }

        public static void DumpTagmap(StreamWriter log, string tagMapName, TagMap tagmap)
        {
            int counter = 0;
            foreach (var tgm in tagmap.OrderBy((e) => e.TagID))
            {
                log.WriteLine("{0}[{1}].TagID {2}", tagMapName, counter, tgm.TagID);
                log.WriteLine("{0}[{1}].TagIDLookup \"{2}\"", tagMapName, counter, StringTagID(tgm.TagID));
                log.WriteLine("{0}[{1}].Type {2}", tagMapName, counter, tgm.Type);
                switch (tgm.Type)
                {
                    case 0:
                        log.WriteLine("{0}[{1}].Int {2}", tagMapName, counter, tgm.Int);
                        break;
                    case 1:
                        log.WriteLine("{0}[{1}].Float0 {2}", tagMapName, counter, tgm.Float);
                        break;
                    case 2: // SNO
                        log.WriteLine("{0}[{1}].Int {2}", tagMapName, counter, tgm.Int);
                        log.WriteLine("{0}[{1}].SNOFile: {2}", tagMapName, counter, StringSNO(tgm.Int));
                        break;
                    case 4:
                        log.WriteLine("{0}[{1}].OpCodeName: {2}", tagMapName, counter, tgm.ScriptFormula.OpCodeName);
                        break;
                    default:
                        log.WriteLine("{0}[{1}].Int? {2}", tagMapName, counter, tgm.Int);
                        break;
                }
                log.WriteLine("");
                ++counter;
            }
        }
        
        public static void DumpPowerFormulas(bool simple)
        {
            using (StreamWriter log = CreateDesktopLogFile(simple ? "simple_power_formulas" : "power_formulas"))
            {
                foreach (var powerEntry in MPQStorage.Data.Assets[SNOGroup.Power].Values)
                {
                    log.WriteLine("script formulas for {0}", Path.GetFileName(powerEntry.FileName));
                    Power power = (Power)powerEntry.Data;

                    var tagmaps = new Dictionary<string, TagMap>()
                    {
                        { "TagMap", power.Powerdef.TagMap },
                        { "GeneralTagMap", power.Powerdef.GeneralTagMap },
                        { "PVPGeneralTagMap", power.Powerdef.PVPGeneralTagMap },
                        { "ContactTagMap0", power.Powerdef.ContactTagMap0 },
                        { "ContactTagMap1", power.Powerdef.ContactTagMap1 },
                        { "ContactTagMap2", power.Powerdef.ContactTagMap2 },
                        { "ContactTagMap3", power.Powerdef.ContactTagMap3 },
                        { "PVPContactTagMap0", power.Powerdef.PVPContactTagMap0 },
                        { "PVPContactTagMap1", power.Powerdef.PVPContactTagMap1 },
                        { "PVPContactTagMap2", power.Powerdef.PVPContactTagMap2 },
                        { "PVPContactTagMap3", power.Powerdef.PVPContactTagMap3 },
                    };

                    foreach (var kv in tagmaps)
                    {
                        foreach (var tagEntry in kv.Value.OrderBy(e => e.TagID))
                        {
                            if (tagEntry.Type == 4)
                            {
                                string name = StringTagID(tagEntry.TagID);
                                log.WriteLine("{0}.\"{1}\"", kv.Key, name);
                                log.WriteLine("   tagid: {0}", tagEntry.TagID);

                                int sf_n;
                                Match nameMatch = Regex.Match(name, "Script Formula (\\d+)");
                                if (nameMatch.Success)
                                {
                                    sf_n = int.Parse(nameMatch.Groups[1].Value);
                                }
                                else
                                {
                                    sf_n = -1;
                                }

                                if (sf_n != -1 && power.ScriptFormulaDetails.Count > sf_n)
                                {
                                    log.WriteLine("   detail 1: {0}", power.ScriptFormulaDetails[sf_n].CharArray1);
                                    log.WriteLine("   detail 2: {0}", power.ScriptFormulaDetails[sf_n].CharArray2);
                                }

                                log.WriteLine("   opname: {0}", tagEntry.ScriptFormula.OpCodeName);

                                if (!simple)
                                {
                                    log.WriteLine("   opbin: {0}", tagEntry.ScriptFormula.OpCodeArray.Aggregate("", (a, b) => a + " " + b));

                                    List<int> opcodes = new List<int>();
                                    while (opcodes.Count * 4 < tagEntry.ScriptFormula.OpCodeArray.Length)
                                        opcodes.Add(BitConverter.ToInt32(tagEntry.ScriptFormula.OpCodeArray, opcodes.Count * 4));

                                    log.WriteLine("   opdecode: {0}", FormulaScript.ToString(opcodes.ToArray()));
                                }
                                log.WriteLine("");
                            }
                        }
                    }
                    log.WriteLine("");
                    log.WriteLine("");
                }
            }
        }

        public static void DumpSNOS()
        {
            using (StreamWriter log = CreateDesktopLogFile("snodatabase"))
            {
                foreach (var assetGroup in MPQStorage.Data.Assets)
                {
                    foreach (var asset in assetGroup.Value.Values.OrderBy((v) => Path.GetFileName(v.FileName)))
                    {
                        log.WriteLine("{0,-8} {1,-8} {2}", asset.SNOId.ToString("x"), asset.SNOId, Path.GetFileName(asset.FileName));
                    }
                }
            }
        }

        public static void DumpPowersForMonsters()
        {
            using (StreamWriter log = CreateDesktopLogFile("powers_for_monsters"))
            {
                foreach (var powerEntry in MPQStorage.Data.Assets[SNOGroup.Power].Values)
                {
                    List<Asset> users = new List<Asset>();

                    foreach (var monsterEntry in MPQStorage.Data.Assets[SNOGroup.Monster].Values)
                    {
                        var monData = (Monster)monsterEntry.Data;
                        foreach (var skill in monData.SkillDeclarations)
                        {
                            if (skill.SNOPower == powerEntry.SNOId)
                            {
                                users.Add(monsterEntry);
                            }
                        }
                    }

                    if (users.Count > 0)
                    {
                        log.WriteLine("monsters using {0} ({1})", StringSNO(powerEntry.SNOId), powerEntry.SNOId);
                        foreach (var monsterEntry in users.OrderBy(mon => StringSNO(mon.SNOId)))
                            log.WriteLine("   {0} ({1})", StringSNO(monsterEntry.SNOId), monsterEntry.SNOId);

                        log.WriteLine("");
                    }
                }
            }
        }

        public static void DumpBehaviorsForMonsters()
        {
            using (StreamWriter log = CreateDesktopLogFile("behaviors_for_monsters"))
            {
                foreach (var aibEntry in MPQStorage.Data.Assets[SNOGroup.AiBehavior].Values)
                {
                    List<Asset> users = new List<Asset>();

                    foreach (var monsterEntry in MPQStorage.Data.Assets[SNOGroup.Monster].Values)
                    {
                        var monData = (Monster)monsterEntry.Data;
                        foreach (var behavior in monData.AIBehavior)
                        {
                            if (aibEntry.SNOId == behavior)
                            {
                                users.Add(monsterEntry);
                            }
                        }
                    }

                    if (users.Count > 0)
                    {
                        log.WriteLine("monsters using {0} ({1})", StringSNO(aibEntry.SNOId), aibEntry.SNOId);
                        foreach (var monsterEntry in users.OrderBy(mon => StringSNO(mon.SNOId)))
                            log.WriteLine("   {0} ({1})", StringSNO(monsterEntry.SNOId), monsterEntry.SNOId);

                        log.WriteLine("");
                    }
                }
            }
        }

        public static void DumpActorsWithTag(int tagKey)
        {
            foreach (var entry in MPQStorage.Data.Assets[SNOGroup.Actor].Values)
            {
                if (entry.Data != null)
                {
                    var data = (Actor)entry.Data;
                    if (data.TagMap.ContainsKey(tagKey))
                    {
                        Log("{0} in {1} ({2})", data.TagMap[tagKey], StringSNO(entry.SNOId), entry.SNOId);
                    }
                }
            }
        }

        public static void OnMPQLoaded()
        {
            DumpPowerFormulas(true);
            DumpPowerTagmaps();

            Logger.Debug("OnMPQLoaded() finished!");
        }
    }
}
