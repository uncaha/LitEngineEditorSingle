using System;
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

                twt.WriteLine("using LitEngine;");
                twt.WriteLine("using LitEngine.IO;");
                twt.WriteLine("using System.Collections.Generic;");
                twt.WriteLine("namespace Config{").Indent();
                twt.WriteLine($"public class {className} : ConfigBase{"{"}").Indent();
                twt.WriteLine($"public const string kConfigfile = {'"'}{className}.bytes{'"'};");
                twt.WriteLine("protected Dictionary<int,Data> mMaps = new Dictionary<int, Data>();");
                twt.WriteLine("public List<int> Keys { get; private set; }");
                twt.WriteLine("public List<Data> Values { get; private set; }");
                twt.WriteLine("public Dictionary<int, Data> Maps { get { return mMaps; } }");

                twt.WriteLine("public class Data{").Indent();
                for (int i = 0; i < data.c; i++)
                {
                    twt.WriteLine($"public readonly {data.objects[ExcelData.sTypeLine, i]} {data.objects[ExcelData.sFieldNameLine, i]};");
                }
                twt.WriteLine("public Data(LitEngine.IO.AESReader _reader){").Indent();
                for (int i = 0; i < data.c; i++)
                {
                    WriteReadStr(twt, data.objects[ExcelData.sTypeLine, i], data.objects[ExcelData.sFieldNameLine, i]);
                }
                twt.Outdent().WriteLine("}");
                twt.Outdent().WriteLine("}");

                twt.WriteLine($"public {className}(){"{"}").Indent();
                twt.WriteLine("byte[] tbys = LitEngine.LoaderManager.LoadConfigFile(kConfigfile);");
                twt.WriteLine("if (tbys == null) return;");
                twt.WriteLine("Values = new List<Data>();");
                twt.WriteLine("AESReader treader = new AESReader(tbys);");
                twt.WriteLine("int trow = treader.ReadInt32();");
                twt.WriteLine("for (int i = 0; i < trow; i++){").Indent();
                twt.WriteLine("Data tcfg = new Data(treader);");
                twt.WriteLine($"mMaps.Add(tcfg.{data.objects[ExcelData.sFieldNameLine, 0]}, tcfg);");
                twt.WriteLine("Values.Add(tcfg);");
                twt.Outdent().WriteLine("}");
                twt.WriteLine("treader.Close();");
                twt.WriteLine("Keys  = new List<int>(mMaps.Keys);");
                twt.Outdent().WriteLine("}");


                twt.WriteLine("public Data this[int _id]{").Indent();
                twt.WriteLine("get { if (!mMaps.ContainsKey(_id)) return null; return mMaps[_id]; }");
                twt.Outdent().WriteLine("}");

                twt.WriteLine("public int Count{").Indent();
                twt.WriteLine("get { return mMaps.Count; }");
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
