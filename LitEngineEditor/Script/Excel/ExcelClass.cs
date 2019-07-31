using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
namespace LitEngine.Excel
{
    public class ExcelData
    {
        public string name;
        public int c;
        public int r;
        public string[,] objects;
        private bool inited = false;
        public void ReadExcelToArray(ISheet _sheet)
        {
            if (inited) return;
            name = _sheet.SheetName;
            IRow firstRow = _sheet.GetRow(0);
            c = firstRow.Cells.Count;
            r = _sheet.PhysicalNumberOfRows;
            objects = new string[r, c];
            int reali = 0;
            for (int i = 0; i < r; i++)
            {
                IRow trow = _sheet.GetRow(reali);
                if (trow == null) continue;
                bool tishave = false;
                for (int j = 0; j < c; j++)
                {
                    objects[reali, j] = "";
                    ICell tcell = trow.GetCell(j);
                    if (tcell == null) continue;
                    if (!tishave)
                        tishave = true;
                    objects[reali, j] = tcell.ToString().Trim();
                }
                if (tishave)
                    reali++;
            }
            inited = true;
            r = reali;
        }
    }
    class ExcelClass
    {
        //   private Excel._Application excelApp;
        private string fileName = string.Empty;
        // private Excel.Workbook wbclass;
        private string savepath;

        private FileStream mFile = null;
        private HSSFWorkbook mWorkbook;

        static public int sStartLine = 3;
        public ExcelClass(string _filename, string _savepath)
        {
            fileName = _filename;
            savepath = _savepath;

            try
            {
                mFile = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fileName.IndexOf(".xls") > 0) // 2003版本
                    mWorkbook = new HSSFWorkbook(mFile);
                else
                    Console.WriteLine("只支持xls格式。");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                if (mFile != null)
                    mFile.Close();
            }
        }

        public List<string> GetSheetNames()
        {
            List<string> list = new List<string>();
            int tlen = mWorkbook.NumberOfSheets;
            for (int i = 0; i < tlen; i++)
            {
                ISheet tsheet = mWorkbook[i];
                list.Add(tsheet.SheetName);
            }
            return list;
        }
        public ISheet GetWorksheetByName(string name)
        {
            return mWorkbook.GetSheet(name);
        }

        public ExcelData GetContentHaveValue(string sheetName)
        {
            ExcelData ret = new ExcelData();
            ISheet sheet = GetWorksheetByName(sheetName);
            if (sheet != null)
            {
                ret.ReadExcelToArray(sheet);
            }
            else
            {
                ret.c = 0;
                ret.r = 0;
            }
            return ret;
        }

        public void Close()
        {
            if (mWorkbook != null)
                mWorkbook.Close();
            if (mFile != null)
                mFile.Close();

        }
        public void SaveFile()
        {
            List<string> tsheetnames = GetSheetNames();
            foreach (string tname in tsheetnames)
            {
                ExcelData tdata = GetContentHaveValue(tname);
                string tfullname = savepath + "/" + tname + ".bytes";
                string ttempfullname = savepath + "/" + tname + ".bytes.temp";
                FileStream tfile = File.OpenWrite(ttempfullname);
                BinaryWriter twt = new BinaryWriter(tfile);
                twt.Write(tdata.r - sStartLine);
                for (int i = sStartLine; i < tdata.r; i++)
                {
                    for (int j = 0; j < tdata.c; j++)
                    {
                        System.Exception terro = WriteData(twt, tdata.objects[1, j], tdata.objects[i, j]);
                        if (terro != null)
                        {
                            twt.Flush();
                            twt.Close();
                            tfile.Close();
                            File.Delete(ttempfullname);
                            if (UnityEditor.EditorUtility.DisplayDialog("Error", $"表 {tname} 生成配置出现错误第{i}行,第{j}列.erro = {terro.ToString()}", "ok"))
                            {
                                DLog.LogError($"表 {tname} 生成配置出现错误第{i}行,第{j}列.erro = {terro.ToString()}");
                            }
                            return;
                        }
                    }
                }
                twt.Flush();
                twt.Close();
                tfile.Close();
                if (File.Exists(tfullname))
                    File.Delete(tfullname);
                File.Move(ttempfullname, tfullname);
            }
        }

        public System.Exception WriteData(BinaryWriter _write, string _typestr, string _value)
        {
            try
            {
                if (_typestr.Contains("[]"))
                    WriteArray(_write, _typestr, _value);
                else
                    WriteValue(_write, _typestr, _value);
            }
            catch (System.Exception _erro)
            {
                return _erro;
            }

            return null;
        }


        protected void WriteArray(BinaryWriter _write, string _typestr, string _value)
        {
            if (!string.IsNullOrEmpty(_value))
            {
                string[] tarry = _value.Split(',');
                _write.Write(tarry.Length);

                string tctype = _typestr.Replace("[]", "");
                for (int i = 0; i < tarry.Length; i++)
                    WriteData(_write, tctype, tarry[i]);
            }
            else
            {
                _write.Write(0);
            }
        }
        protected void WriteValue(BinaryWriter _write, string _typestr, string _value)
        {
            switch (_typestr)
            {
                case "int":
                    _write.Write(int.Parse(_value));
                    break;
                case "float":
                    _write.Write(float.Parse(_value));
                    break;
                case "string":
                    _write.Write(_value);
                    break;
                case "long":
                    _write.Write(long.Parse(_value));
                    break;
                case "byte":
                    _write.Write(byte.Parse(_value));
                    break;
                case "short":
                    _write.Write(short.Parse(_value));
                    break;
                case "bool":
                    _write.Write(bool.Parse(_value));
                    break;
            }
        }

        public void ExoprtCfg(int starLine, StreamWriter logWriter, bool contentLog, bool isCrypt)
        {
            sStartLine = starLine;
            List<string> tsheetnames = GetSheetNames();
            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < 1; k++)
            {
                string tname = tsheetnames[k];
                ExcelData tdata = GetContentHaveValue(tname);
                string cfgPath = savepath + "/" + tname + ".cfg";

                FileStream tfile = File.OpenWrite(savepath + "/" + tname + ".bytes");
                BinaryWriter twt = new BinaryWriter(tfile);

                int rowNum = 0;
                for (int i = sStartLine; i < tdata.r; i++)
                {
                    if (tdata.objects[i, 0] == null)
                    {
                        break;
                    }
                    else if (string.IsNullOrEmpty(tdata.objects[i, 0].ToString()))
                    {
                        break;
                    }
                    rowNum++;
                }
                twt.Write(rowNum);
                logWriter.WriteLine("生成 " + cfgPath + ", 共" + rowNum + "行");
                for (int i = sStartLine - 1; i < tdata.r; i++)
                {
                    if (tdata.objects[i, 0] == null)
                    {
                        break;
                    }
                    else if (string.IsNullOrEmpty(tdata.objects[i, 0].ToString()))
                    {
                        break;
                    }
                    sb.Append((i - sStartLine + 1) + "行:[");
                    for (int j = 0; j < tdata.c; j++)
                    {
                        sb.Append(tdata.objects[i, j]);
                        WriteData(twt, tdata.objects[1, j], tdata.objects[i, j]);
                    }
                    sb.Append("]");
                    sb.AppendLine();
                }
                twt.Flush();
                twt.Close();
                tfile.Close();
            }
            if (contentLog)
            {
                logWriter.WriteLine(sb.ToString());
            }
            Console.WriteLine(sb.ToString());
        }

        public List<string> ExportReadClass()
        {
            List<string> tsheetnames = GetSheetNames();
            foreach (string tname in tsheetnames)
            {
                ExcelData tdata = GetContentHaveValue(tname);
                string tfullname = savepath + "/" + tname + ".cs";
                if (File.Exists(tfullname))
                    File.Delete(tfullname);
                FileStream tfile = File.OpenWrite(tfullname);
                TextWriter twt = new TextWriter(tfile);

                twt.WriteLine("using LitEngine;");
                twt.WriteLine("using LitEngine.IO;");
                twt.WriteLine("using System.Collections.Generic;");
                twt.WriteLine("namespace Config{").Indent();
                twt.WriteLine($"public class {tname} : ConfigBase{"{"}").Indent();
                twt.WriteLine($"public const string kConfigfile = {'"'}{tname}.bytes{'"'};");
                twt.WriteLine("protected Dictionary<string,Data> mMaps = new Dictionary<string, Data>();");
                twt.WriteLine("public List<string> Keys { get; private set; }");
                twt.WriteLine("public List<Data> Values { get; private set; }");
                twt.WriteLine("public Dictionary<string, Data> Maps { get { return mMaps; } }");

                twt.WriteLine("public class Data{").Indent();
                for (int i = 0; i < tdata.c; i++)
                {
                    twt.WriteLine($"public readonly {tdata.objects[1, i]} {tdata.objects[2, i]};");
                }
                twt.WriteLine("public Data(LitEngine.IO.AESReader _reader){").Indent();
                for (int i = 0; i < tdata.c; i++)
                {
                    WriteReadStr(twt, tdata.objects[1, i], tdata.objects[2, i]);
                }
                twt.Outdent().WriteLine("}");
                twt.Outdent().WriteLine("}");

                twt.WriteLine($"public {tname}(){"{"}").Indent();
                twt.WriteLine("byte[] tbys = LitEngine.LoaderManager.LoadConfigFile(kConfigfile);");
                twt.WriteLine("if (tbys == null) return;");
                twt.WriteLine("Values = new List<Data>();");
                twt.WriteLine("AESReader treader = new AESReader(tbys);");
                twt.WriteLine("int trow = treader.ReadInt32();");
                twt.WriteLine("for (int i = 0; i < trow; i++){").Indent();
                twt.WriteLine("Data tcfg = new Data(treader);");
                twt.WriteLine("mMaps.Add(tcfg.id, tcfg);");
                twt.WriteLine("Values.Add(tcfg);");
                twt.Outdent().WriteLine("}");
                twt.WriteLine("treader.Close();");
                twt.WriteLine("Keys  = new List<string>(mMaps.Keys);");
                twt.Outdent().WriteLine("}");


                twt.WriteLine("public Data this[string _id]{").Indent();
                twt.WriteLine("get { if (!mMaps.ContainsKey(_id)) return null; return mMaps[_id]; }");
                twt.Outdent().WriteLine("}");

                twt.WriteLine("public Data this[int _index]{").Indent();
                twt.WriteLine("get { if (_index >= Values.Count) return null; return Values[_index]; }");
                twt.Outdent().WriteLine("}");

                twt.WriteLine("public int Count{").Indent();
                twt.WriteLine("get { return mMaps.Count; }");
                twt.Outdent().WriteLine("}");


                twt.Outdent().WriteLine("}");
                twt.Outdent().WriteLine("}");
                twt.Close();
                tfile.Close();
            }

            return tsheetnames;
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
