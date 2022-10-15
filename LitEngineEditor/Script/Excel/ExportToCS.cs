﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ExportTool
{
    public class ExportToCS
    {
        string filename;
        string tempFile;
        string className;
        ExcelData data;

        public ExportToCS(string pName,string pFullPath, ExcelData pData)
        {
            className = pName;
            filename = pFullPath;
            data = pData;

            tempFile = filename + ".temp";
        }

        public void StartExport()
        {
            FileStream tfile = null;
            TextWriter twt = null;
            try
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                tfile = File.OpenWrite(tempFile);
                twt = new TextWriter(tfile);
                
                var tfirstTypeStr = data.objects[ExcelData.sTypeLine, 0];
                var tfirstNameStr = data.objects[ExcelData.sFieldNameLine, 0];

                twt.WriteLine("using LitEngine;");
                twt.WriteLine("using LitEngine.IO;");
                twt.WriteLine("using System.Collections.Generic;");
                twt.WriteLine("namespace Config{").Indent();
                twt.WriteLine($"public partial class {className} {{").Indent();
                twt.WriteLine($"public const string kConfigfile = {'"'}{className}.bytes{'"'};");
                twt.WriteLine($"public Dictionary<{tfirstTypeStr},Data> Maps {{ get; private set; }} = new Dictionary<{tfirstTypeStr}, Data>();");
                twt.WriteLine($"public List<{tfirstTypeStr}> Keys {{ get; private set; }}");
                
                twt.WriteLine("public List<Data> Values { get; private set; }");

                twt.WriteLine("public class Data{").Indent();
                for (int i = 0; i < data.c; i++)
                {
                    if (!data.IsNeed(i)) continue;
                    var ttypeStr = data.objects[ExcelData.sTypeLine, i];
                    var tnameStr = data.objects[ExcelData.sFieldNameLine, i];
                    twt.WriteLine($"public readonly {ttypeStr} {tnameStr};");
                }
                twt.WriteLine("public Data(System.IO.BinaryReader _reader){").Indent();
                
                for (int i = 0; i < data.c; i++)
                {
                    if (!data.IsNeed(i)) continue;
                    var ttypeStr = data.objects[ExcelData.sTypeLine, i];
                    var tnameStr = data.objects[ExcelData.sFieldNameLine, i];
                    WriteReadStr(twt, ttypeStr, tnameStr);
                }
                twt.Outdent().WriteLine("}");
                twt.Outdent().WriteLine("}");
                
                //constructor
                twt.WriteLine($"public {className}(){{").Indent();
                twt.WriteLine("byte[] tbys = LitEngine.LoaderManager.LoadConfigFile(kConfigfile);");
                twt.WriteLine("if (tbys == null) return;");
                
                twt.WriteLine("var treader = new System.IO.BinaryReader(new System.IO.MemoryStream(tbys));");
                twt.WriteLine("int trow = treader.ReadInt32();");
                twt.WriteLine("Values = new List<Data>(trow);");
                twt.WriteLine("for (int i = 0; i < trow; i++){").Indent();
                twt.WriteLine("Data tcfg = new Data(treader);");
                
                twt.WriteLine($"Add(tcfg.{tfirstNameStr}, tcfg);");
                
                twt.WriteLine("Values.Add(tcfg);");
                twt.Outdent().WriteLine("}");
                twt.WriteLine("treader.Close();");
                twt.WriteLine($"Keys  = new List<{tfirstTypeStr}>(Maps.Keys);");
                twt.Outdent().WriteLine("}");
                
                //add
                twt.WriteLine($"void Add({tfirstTypeStr} pKey,Data pData){{").Indent();
                
                twt.WriteLine("if (Maps.ContainsKey(pKey)){").Indent();
                twt.WriteLine("DLog.LogError(\"The same key in map.key = \" + pKey);");
                twt.WriteLine("return;");
                twt.Outdent().WriteLine("}");
                twt.WriteLine("Maps.Add(pKey, pData);");
                twt.Outdent().WriteLine("}");
                
                //this[]
                twt.WriteLine($"public Data this[{tfirstTypeStr} pKey]{{").Indent();
                twt.WriteLine("get { if (!Maps.ContainsKey(pKey)) return null; return Maps[pKey]; }");
                twt.Outdent().WriteLine("}");
                
                //count
                twt.WriteLine("public int Count{").Indent();
                twt.WriteLine("get { return Maps.Count; }");
                twt.Outdent().WriteLine("}");
                
                //ContainsKey
                twt.WriteLine($"public bool ContainsKey({tfirstTypeStr} pKey){{").Indent();
                twt.WriteLine("return Maps.ContainsKey(pKey)");
                twt.Outdent().WriteLine("}");
                

                twt.Outdent().WriteLine("}");
                twt.Outdent().WriteLine("}");
                twt?.Close();
                tfile?.Close();

                if (File.Exists(filename))
                    File.Delete(filename);
                File.Move(tempFile, filename);

            }
            catch (Exception ex)
            {
                twt?.Close();
                tfile?.Close();
                DLog.LogError(ex.ToString());
            }

            
        }

        public void WriteReadStr(TextWriter _writer, string _typestr, string _valuename)
        {
            if (_typestr.Contains("[]"))
            {
                string tctype = _typestr.Replace("[]", "");
                _writer.WriteLine("");
                _writer.WriteLine($"int tcount{_valuename} = _reader.ReadInt32();");
                _writer.WriteLine($"{_valuename} = new {tctype}[tcount{_valuename}];");
                _writer.WriteLine($"for(int i=0; i< tcount{_valuename}; i++){"{"}").Indent();
                WriteReadStr(_writer, tctype, _valuename + "[i]");
                _writer.Outdent().WriteLine("}");
                _writer.WriteLine("");
            }
            else
            {
                switch (_typestr)
                {
                    case "int":
                        _writer.WriteLine($"{_valuename} = _reader.ReadInt32();");
                        break;
                    case "float":
                        _writer.WriteLine($"{_valuename} = _reader.ReadSingle();");
                        break;
                    case "string":
                        _writer.WriteLine($"{_valuename} = _reader.ReadString();");
                        break;
                    case "long":
                        _writer.WriteLine($"{_valuename} = _reader.ReadInt64();");
                        break;
                    case "byte":
                        _writer.WriteLine($"{_valuename} = _reader.ReadByte();");
                        break;
                    case "short":
                        _writer.WriteLine($"{_valuename} = _reader.ReadInt16();");
                        break;
                    case "bool":
                        _writer.WriteLine($"{_valuename} = _reader.ReadBoolean();");
                        break;
                }
            }

        }
    }
}
