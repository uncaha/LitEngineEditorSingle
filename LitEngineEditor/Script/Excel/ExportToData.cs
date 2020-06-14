using System;
using System.IO;

namespace ExportTool
{
    public class ExportToData
    {
        string filename;
        string tempFile;
        ExcelData data;

        public ExportToData(string pName, ExcelData pData)
        {
            filename = pName;
            data = pData;

            tempFile = filename + ".temp";
        }

        public void StartExport()
        {
            try
            {
                DLog.Log("开始导出:" + tempFile);
                FileStream tfile = File.OpenWrite(tempFile);
                BinaryWriter twt = new BinaryWriter(tfile);
                int tline = data.r - ExcelData.sStartLine;
                twt.Write(tline);
                DLog.LogFormat("共有 {0} 行数据",tline);
                for (int i = ExcelData.sStartLine; i < data.r; i++)
                {
                    if(string.IsNullOrEmpty(data.objects[i, 0]))
                    {
                        twt.Flush();
                        twt.Close();
                        tfile.Close();
                        File.Delete(tempFile);
                        ShowError(i, 0,"配置表第一列id不能为空,请检查配置表.是否有空列,并删除.");
                        return;
                    }
                    for (int j = 0; j < data.c; j++)
                    {
                        if (!data.IsNeed(j)) continue;
                        System.Exception terro = WriteData(twt, data.objects[ExcelData.sTypeLine, j], data.objects[i, j]);
                        if (terro != null)
                        {
                            twt.Flush();
                            twt.Close();
                            tfile.Close();
                            File.Delete(tempFile);
                            ShowError(i,j,terro.Message);
                            return;
                        }
                    }
                }
                twt.Flush();
                twt.Close();
                tfile.Close();
                if (File.Exists(filename))
                    File.Delete(filename);
                File.Move(tempFile, filename);
            }
            catch (Exception ex)
            {
                DLog.LogError(ex);
            }

        }

        void ShowError(int i,int j,string pErro)
        {
            if (UnityEditor.EditorUtility.DisplayDialog("Error", $"表 {filename} 生成配置出现错误第{i}行,第{j}列.erro = {pErro}", "ok"))
            {
                DLog.LogError($"表 {filename} 生成配置出现错误第{i}行,第{j}列.erro = {pErro}");
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
            if (string.IsNullOrEmpty(_value))
            {
                WriteNullValue(_write, _typestr, _value);
                return;
            }

            switch (_typestr)
            {
                case "int":
                    _write.Write(int.Parse(_value));
                    break;
                case "float":
                    _write.Write(float.Parse(_value));
                    break;
                case "string":
                    {
                        byte[] tbytes = System.Text.UTF8Encoding.UTF8.GetBytes(_value);
                        _write.Write(tbytes.Length);
                        _write.Write(tbytes);
                    }
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

        protected void WriteNullValue(BinaryWriter _write, string _typestr, string _value)
        {
            switch (_typestr)
            {
                case "int":
                    _write.Write((int)0);
                    break;
                case "float":
                    _write.Write((float)0);
                    break;
                case "string":
                    _write.Write((int)0);
                    break;
                case "long":
                    _write.Write((long)0);
                    break;
                case "byte":
                    _write.Write((byte)0);
                    break;
                case "short":
                    _write.Write((short)0);
                    break;
                case "bool":
                    _write.Write(false);
                    break;
            }
        }
    }
}
