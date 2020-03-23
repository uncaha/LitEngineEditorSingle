using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProtoBuf.Reflection
{
    public class ILCodeGenerator : CommonCodeGenerator
    {
        /// <summary>
        /// Reusable code-generator instance
        /// </summary>
        public static ILCodeGenerator Default { get; } = new ILCodeGenerator();
        /// <summary>
        /// Create a new CSharpCodeGenerator instance
        /// </summary>
        protected ILCodeGenerator() { }
        /// <summary>
        /// Returns the language name
        /// </summary>
        public override string Name => "C#";
        /// <summary>
        /// Returns the default file extension
        /// </summary>
        protected override string DefaultFileExtension => "cs";
        /// <summary>
        /// Escapes language keywords
        /// </summary>
        protected override string Escape(string identifier)
        {
            switch (identifier)
            {
                case "abstract":
                case "event":
                case "new":
                case "struct":
                case "as":
                case "explicit":
                case "null":
                case "switch":
                case "base":
                case "extern":
                case "object":
                case "this":
                case "bool":
                case "false":
                case "operator":
                case "throw":
                case "break":
                case "finally":
                case "out":
                case "true":
                case "byte":
                case "fixed":
                case "override":
                case "try":
                case "case":
                case "float":
                case "params":
                case "typeof":
                case "catch":
                case "for":
                case "private":
                case "uint":
                case "char":
                case "foreach":
                case "protected":
                case "ulong":
                case "checked":
                case "goto":
                case "public":
                case "unchecked":
                case "class":
                case "if":
                case "readonly":
                case "unsafe":
                case "const":
                case "implicit":
                case "ref":
                case "ushort":
                case "continue":
                case "in":
                case "return":
                case "using":
                case "decimal":
                case "int":
                case "sbyte":
                case "virtual":
                case "default":
                case "interface":
                case "sealed":
                case "volatile":
                case "delegate":
                case "internal":
                case "short":
                case "void":
                case "do":
                case "is":
                case "sizeof":
                case "while":
                case "double":
                case "lock":
                case "stackalloc":
                case "else":
                case "long":
                case "static":
                case "enum":
                case "namespace":
                case "string":
                    return "@" + identifier;
                default:
                    return identifier;
            }
        }
        /// <summary>
        /// Start a file
        /// </summary>
        protected override void WriteFileHeader(GeneratorContext ctx, FileDescriptorProto file, ref object state)
        {
            ctx.WriteLine("// 基于protobuf-net修改.http://mgravell.github.io/protobuf-net/releasenotes")
               .WriteLine($"// Input: {Path.GetFileName(ctx.File.Name)}").WriteLine();


            var @namespace = ctx.NameNormalizer.GetName(file);

            if (!string.IsNullOrEmpty(@namespace))
            {
                state = @namespace;
                ctx.WriteLine($"namespace {@namespace}");
                ctx.WriteLine("{").Indent().WriteLine();
            }

        }
        /// <summary>
        /// End a file
        /// </summary>
        protected override void WriteFileFooter(GeneratorContext ctx, FileDescriptorProto file, ref object state)
        {
            var @namespace = (string)state;
            if (!string.IsNullOrEmpty(@namespace))
            {
                ctx.Outdent().WriteLine("}").WriteLine();
            }
            //ctx.WriteLine("#pragma warning restore CS1591, CS0612, CS3021");
        }
        /// <summary>
        /// Start an enum
        /// </summary>
        protected override void WriteEnumHeader(GeneratorContext ctx, EnumDescriptorProto obj, ref object state)
        {
            //var name = ctx.NameNormalizer.GetName(obj);
            string name = obj.Name;
            var tw = ctx.Output;
            WriteOptions(ctx, obj.Options);
            ctx.WriteLine($"{GetAccess(GetAccess(obj))} enum {Escape(name)}").WriteLine("{").Indent();
        }
        /// <summary>
        /// End an enum
        /// </summary>

        protected override void WriteEnumFooter(GeneratorContext ctx, EnumDescriptorProto obj, ref object state)
        {
            ctx.Outdent().WriteLine("}").WriteLine();
        }
        /// <summary>
        /// Write an enum value
        /// </summary>
        protected override void WriteEnumValue(GeneratorContext ctx, EnumValueDescriptorProto obj, ref object state)
        {
            // var name = ctx.NameNormalizer.GetName(obj);
            string name = obj.Name;
            WriteOptions(ctx, obj.Options);
            ctx.WriteLine($"{Escape(name)} = {obj.Number},");
        }

        /// <summary>
        /// End a message
        /// </summary>
        protected override void WriteMessageFooter(GeneratorContext ctx, DescriptorProto obj, ref object state)
        {
            ctx.Outdent().WriteLine("}").WriteLine();
        }
        /// <summary>
        /// Start a message
        /// </summary>
        protected override void WriteMessageHeader(GeneratorContext ctx, DescriptorProto obj, ref object state)
        {
            // var name = ctx.NameNormalizer.GetName(obj);
            string name = obj.Name;
            var tw = ctx.Output;
            WriteOptions(ctx, obj.Options);
            tw = ctx.Write($"{GetAccess(GetAccess(obj))} class {Escape(name)}");
            if (obj.ExtensionRanges.Count != 0) tw.Write(" : global::ProtoBuf.IExtensible");
            tw.WriteLine();
            ctx.WriteLine("{").Indent();
            //if (obj.Options?.MessageSetWireFormat == true)
            //{
            //    ctx.WriteLine("#error message_set_wire_format is not currently implemented").WriteLine();
            //}
            //if (obj.ExtensionRanges.Count != 0)
            //{
            //    ctx.WriteLine($"private global::ProtoBuf.IExtension {FieldPrefix}extensionData;")
            //        .WriteLine($"global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)").Indent()
            //        .WriteLine($"=> global::ProtoBuf.Extensible.GetExtensionObject(ref {FieldPrefix}extensionData, createIfMissing);").Outdent().WriteLine();
            //}
        }

        private static void WriteOptions<T>(GeneratorContext ctx, T obj) where T : class, ISchemaOptions
        {
            if (obj == null) return;
            if (obj.Deprecated)
            {
                ctx.WriteLine($"[global::System.Obsolete]");
            }
        }

        const string FieldPrefix = "__pbn__";

        /// <summary>
        /// Get the language specific keyword representing an access level
        /// </summary>
        public override string GetAccess(Access access)
        {
            switch (access)
            {
                case Access.Internal: return "internal";
                case Access.Public: return "public";
                case Access.Private: return "private";
                default: return base.GetAccess(access);
            }
        }

        protected void ReadBaseDataStr(GeneratorContext ctx, FieldDescriptorProto.Type _type, string _value, string _typename)
        {
            switch (_type)
            {
                case FieldDescriptorProto.Type.TypeSfixed32:
                    ctx.WriteLine($"{_value} = input.ReadSFixed32();");
                    break;
                case FieldDescriptorProto.Type.TypeFixed32:
                    ctx.WriteLine($"{_value} = input.ReadFixed32();");
                    break;
                case FieldDescriptorProto.Type.TypeSfixed64:
                    ctx.WriteLine($"{_value} = input.ReadSFixed64();");
                    break;
                case FieldDescriptorProto.Type.TypeFixed64:
                    ctx.WriteLine($"{_value} = input.ReadFixed64();");
                    break;
                case FieldDescriptorProto.Type.TypeInt32:
                    ctx.WriteLine($"{_value} = input.ReadInt32();");
                    break;
                case FieldDescriptorProto.Type.TypeInt64:
                    ctx.WriteLine($"{_value} = input.ReadInt64();");
                    break;
                case FieldDescriptorProto.Type.TypeSint32:
                    ctx.WriteLine($"{_value} = input.ReadSInt32();");
                    break;
                case FieldDescriptorProto.Type.TypeSint64:
                    ctx.WriteLine($"{_value} = input.ReadSInt64();");
                    break;
                case FieldDescriptorProto.Type.TypeUint32:
                    ctx.WriteLine($"{_value} = input.ReadUInt32();");
                    break;
                case FieldDescriptorProto.Type.TypeUint64:
                    ctx.WriteLine($"{_value} = input.ReadUInt64();");
                    break;


                case FieldDescriptorProto.Type.TypeFloat:
                    ctx.WriteLine($"{_value} = input.ReadFloat();");
                    break;
                case FieldDescriptorProto.Type.TypeDouble:
                    ctx.WriteLine($"{_value} = input.ReadDouble();");
                    break;

                case FieldDescriptorProto.Type.TypeBool:
                    ctx.WriteLine($"{_value} = input.ReadBool();");
                    break;
                case FieldDescriptorProto.Type.TypeEnum:
                    ctx.WriteLine($"{_value} = ({_typename})input.ReadEnum();");
                    break;

                case FieldDescriptorProto.Type.TypeString:
                    ctx.WriteLine($"{_value} = input.ReadString();");
                    break;

                case FieldDescriptorProto.Type.TypeBytes:
                    ctx.WriteLine($"{_value} = input.ReadBytes().ToByteArray();");
                    break;
            }
        }

        protected void ReadCreatObjectStr(GeneratorContext ctx, string _typename, string _valuename)
        {
            ctx.WriteLine($"byte[] tchildbuffer = input.ReadBytes().ToByteArray();");
            ctx.WriteLine($"{_typename} {_valuename} = new {_typename}();");
            ctx.WriteLine($"{_valuename}.MergeFrom(tchildbuffer);");
        }
        protected void ReaderString(GeneratorContext ctx, FieldDescriptorProto obj)
        {
            bool isRepeated = obj.label == FieldDescriptorProto.Label.LabelRepeated;
            var typeName = GetTypeName(ctx, obj, out var dataFormat, out var isMap);

            string tfirstvalue = obj.Name;
            if (isRepeated)
            {
                tfirstvalue = $"{typeName} tvalue";
                ctx.WriteLine("{").Indent();
            }

            ReadBaseDataStr(ctx, obj.type, tfirstvalue, typeName);

            if (isRepeated)
            {
                var mapMsgType = isMap ? ctx.TryFind<DescriptorProto>(obj.TypeName) : null;
                if (mapMsgType != null)
                {
                    var keyTypeName = GetTypeName(ctx, mapMsgType.Fields.Single(x => x.Number == 1),
                        out var keyDataFormat, out var _);
                    var valueTypeName = GetTypeName(ctx, mapMsgType.Fields.Single(x => x.Number == 2),
                        out var valueDataFormat, out var _);

                    string tkey = "tkey";
                    if (FieldDescriptorProto.Type.TypeMessage == mapMsgType.Fields[0].type)
                        ReadCreatObjectStr(ctx, keyTypeName, tkey);
                    else
                        ReadBaseDataStr(ctx, mapMsgType.Fields[0].type, $"{keyTypeName} {tkey}", keyTypeName);

                    string tvalue = "tvalue";
                    if (FieldDescriptorProto.Type.TypeMessage == mapMsgType.Fields[1].type)
                        ReadCreatObjectStr(ctx, valueTypeName, tvalue);
                    else
                        ReadBaseDataStr(ctx, mapMsgType.Fields[1].type, $"{valueTypeName} {tvalue}", valueTypeName);

                    ctx.WriteLine($"{obj.Name}.Add({tkey}, {tvalue});");
                }
                else
                {
                    if (FieldDescriptorProto.Type.TypeMessage == obj.type)
                    {
                        ReadCreatObjectStr(ctx, typeName, "tobj");
                        ctx.WriteLine($"{obj.Name}.Add(tobj);");
                    }
                    else
                        ctx.WriteLine($"{obj.Name}.Add(tvalue);");
                }
                ctx.Outdent().WriteLine("}");

            }
            else
            {
                switch (obj.type)
                {

                    case FieldDescriptorProto.Type.TypeMessage:
                        ctx.WriteLine("{").Indent();
                        ctx.WriteLine($"byte[] tchildbuffer = input.ReadBytes().ToByteArray();");
                        ctx.WriteLine($"{tfirstvalue}.MergeFrom(tchildbuffer);");
                        ctx.Outdent().WriteLine("}");
                        break;
                }

            }

        }

        protected void WriteFileRead(GeneratorContext ctx, FieldDescriptorProto obj, OneOfStub[] oneOfs)
        {
            bool isRepeated = obj.label == FieldDescriptorProto.Label.LabelRepeated;
            var typeName = GetTypeName(ctx, obj, out var dataFormat, out var isMap);
            Google.Protobuf.WireFormat.WireType ttwtype = ProtoTool.ILHelper.GetWireType(obj.type);
            uint ttag = Google.Protobuf.WireFormat.MakeTag(obj.Number, ttwtype);
            ctx.WriteLine($"case {ttag}:").Indent();
            ReaderString(ctx, obj);
            ctx.WriteLine("break;").Outdent();
        }

        protected override void WriteReadFun(GeneratorContext ctx, DescriptorProto obj, OneOfStub[] oneOfs)
        {
            ctx.WriteLine($"public void MergeFrom(byte[] _bytes){"{"}").Indent();
            ctx.WriteLine("Google.Protobuf.CodedInputStream input = new Google.Protobuf.CodedInputStream(_bytes);");
            ctx.WriteLine("uint tag;");
            ctx.WriteLine("while ((tag = input.ReadTag()) != 0) {").Indent();
            ctx.WriteLine("switch(tag) {").Indent();
            ctx.WriteLine("default:").Indent();
            ctx.WriteLine("input.SkipLastField();");
            ctx.WriteLine("break;").Outdent();

            foreach (var inner in obj.Fields)
            {
                WriteFileRead(ctx, inner, oneOfs);
            }

            ctx.Outdent().WriteLine("}");//switch

            ctx.Outdent().WriteLine("}");//while

            ctx.WriteLine("input.Dispose();");

            ctx.Outdent().WriteLine("}");

        }

        protected void WriteFieldWrite(GeneratorContext ctx, FieldDescriptorProto obj, OneOfStub[] oneOfs)
        {

            bool isOptional = obj.label == FieldDescriptorProto.Label.LabelOptional;
            bool isRepeated = obj.label == FieldDescriptorProto.Label.LabelRepeated;

            Google.Protobuf.WireFormat.WireType ttwtype = ProtoTool.ILHelper.GetWireType(obj.type);
            uint ttag = Google.Protobuf.WireFormat.MakeTag(obj.Number, ttwtype);
            string tagstr = $"output.WriteTag({ttag});";

            var typeName = GetTypeName(ctx, obj, out var dataFormat, out var isMap);
            if (isOptional)
            {
                string tifvalue = "";
                if (obj.type == FieldDescriptorProto.Type.TypeString)
                    tifvalue = $"if(!string.IsNullOrEmpty({obj.Name})){"{"}";
                else
                    tifvalue = $"if({obj.Name} != default({typeName})){"{"}";
                ctx.WriteLine(tifvalue).Indent();
                //代码块
                ctx.WriteLine(tagstr);
                ctx.WriteLine(WriteString(obj.type, obj.Name));

                ctx.Outdent();
                ctx.WriteLine($"{"}"}");
            }
            else if (isRepeated)
            {
                var mapMsgType = isMap ? ctx.TryFind<DescriptorProto>(obj.TypeName) : null;
                if (mapMsgType != null)
                {
                    var keyTypeName = GetTypeName(ctx, mapMsgType.Fields.Single(x => x.Number == 1),
                        out var keyDataFormat, out var _);
                    var valueTypeName = GetTypeName(ctx, mapMsgType.Fields.Single(x => x.Number == 2),
                        out var valueDataFormat, out var _);
                    // System.Collections.Generic.Dictionary<int, int> ttt = new Dictionary<int, int>();
                    // System.Collections.Generic.KeyValuePair<int,int> ttt
                    ctx.WriteLine($"if({obj.Name}.Count > 0 ){"{"}").Indent();
                    ctx.WriteLine($"foreach (System.Collections.Generic.KeyValuePair<{keyTypeName}, {valueTypeName}> item in {obj.Name}){"{"}").Indent();

                    ctx.WriteLine(tagstr);
                    ctx.WriteLine(WriteString(mapMsgType.Fields[0].type, "item.Key"));
                    ctx.WriteLine(WriteString(mapMsgType.Fields[1].type, "item.Value"));

                    ctx.Outdent().WriteLine("}");
                    ctx.Outdent().WriteLine("}");
                }
                else
                {
                    ctx.WriteLine($"if({obj.Name}.Count > 0 ){"{"}").Indent();
                    ctx.WriteLine($"for (int i = 0; i < {obj.Name}.Count; i++){"{"}").Indent();

                    ctx.WriteLine(tagstr);
                    ctx.WriteLine(WriteString(obj.type, obj.Name + "[i]"));

                    ctx.Outdent().WriteLine("}");

                    ctx.Outdent().WriteLine("}");
                }



            }
            else
            {
                ctx.WriteLine(tagstr);
                ctx.WriteLine(WriteString(obj.type, obj.Name));
            }
        }

        protected override void WriteWriteFun(GeneratorContext ctx, DescriptorProto obj, OneOfStub[] oneOfs)
        {
            ctx.WriteLine($"public byte[] GetBytes(){"{"}").Indent();
            ctx.WriteLine("System.IO.MemoryStream tmemsteam = new System.IO.MemoryStream();");
            ctx.WriteLine("Google.Protobuf.CodedOutputStream output = new Google.Protobuf.CodedOutputStream(tmemsteam,512,true);");
            foreach (var inner in obj.Fields)
            {
                WriteFieldWrite(ctx, inner, oneOfs);
            }
            ctx.WriteLine($"output.Flush();");
            ctx.WriteLine($"byte[] ret = tmemsteam.ToArray();");
            ctx.WriteLine($"output.Dispose();");
            ctx.WriteLine($"tmemsteam.Dispose();");
            ctx.WriteLine($"return ret;");
            ctx.Outdent();
            ctx.WriteLine($"{"}"}");
        }

        protected string WriteString(FieldDescriptorProto.Type _type, string _name)
        {
            string ret = "";
            switch (_type)
            {
                case FieldDescriptorProto.Type.TypeSfixed32:
                    ret = $"output.WriteSFixed32({_name});";
                    break;
                case FieldDescriptorProto.Type.TypeFixed32:
                    ret = $"output.WriteFixed32({_name});";
                    break;
                case FieldDescriptorProto.Type.TypeSfixed64:
                    ret = $"output.WriteSFixed64({_name});";
                    break;
                case FieldDescriptorProto.Type.TypeFixed64:
                    ret = $"output.WriteFixed64({_name});";
                    break;
                case FieldDescriptorProto.Type.TypeInt32:
                    ret = $"output.WriteInt32({_name});";
                    break;
                case FieldDescriptorProto.Type.TypeInt64:
                    ret = $"output.WriteInt64({_name});";
                    break;
                case FieldDescriptorProto.Type.TypeSint32:
                    ret = $"output.WriteSInt32({_name});";
                    break;
                case FieldDescriptorProto.Type.TypeSint64:
                    ret = $"output.WriteSInt64({_name});";
                    break;
                case FieldDescriptorProto.Type.TypeUint32:
                    ret = $"output.WriteUInt32({_name});";
                    break;
                case FieldDescriptorProto.Type.TypeUint64:
                    ret = $"output.WriteUInt64({_name});";
                    break;


                case FieldDescriptorProto.Type.TypeFloat:
                    ret = $"output.WriteFloat({_name});";
                    break;
                case FieldDescriptorProto.Type.TypeDouble:
                    ret = $"output.WriteDouble({_name});";
                    break;

                case FieldDescriptorProto.Type.TypeBool:
                    ret = $"output.WriteBool({_name});";
                    break;
                case FieldDescriptorProto.Type.TypeEnum:
                    ret = $"output.WriteEnum((int){_name});";
                    break;
                case FieldDescriptorProto.Type.TypeMessage:
                    ret = $"output.WriteBytes(Google.Protobuf.ByteString.CopyFrom({_name}.GetBytes()));";
                    break;

                case FieldDescriptorProto.Type.TypeString:
                    ret = $"output.WriteString({_name});";
                    break;
                case FieldDescriptorProto.Type.TypeBytes:
                    ret = $"output.WriteBytes(Google.Protobuf.ByteString.CopyFrom({_name}));";
                    break;
            }

            return ret;
        }

        protected string GetDefaltValue(GeneratorContext ctx, FieldDescriptorProto obj, string _typename)
        {
            string defaultValue = obj.DefaultValue;
            if (obj.type == FieldDescriptorProto.Type.TypeString)
            {
                if (!string.IsNullOrEmpty(defaultValue))
                    defaultValue = '"' + defaultValue + '"';
            }
            else if (obj.type == FieldDescriptorProto.Type.TypeDouble)
            {
                switch (defaultValue)
                {
                    case "inf": defaultValue = "double.PositiveInfinity"; break;
                    case "-inf": defaultValue = "double.NegativeInfinity"; break;
                    case "nan": defaultValue = "double.NaN"; break;
                    default:
                        if (!string.IsNullOrEmpty(defaultValue) && !defaultValue.EndsWith("f"))
                            defaultValue += "f";
                        break;
                }
            }
            else if (obj.type == FieldDescriptorProto.Type.TypeFloat)
            {
                switch (defaultValue)
                {
                    case "inf": defaultValue = "float.PositiveInfinity"; break;
                    case "-inf": defaultValue = "float.NegativeInfinity"; break;
                    case "nan": defaultValue = "float.NaN"; break;
                    default:
                        if (!string.IsNullOrEmpty(defaultValue) && !defaultValue.EndsWith("f"))
                            defaultValue += "f";
                        break;
                }
            }
            else if (obj.type == FieldDescriptorProto.Type.TypeMessage)
            {
                defaultValue = $"new {_typename}()";
            }
            else if (obj.type == FieldDescriptorProto.Type.TypeEnum)
            {
                var enumType = ctx.TryFind<EnumDescriptorProto>(obj.TypeName);
                if (enumType != null)
                {
                    EnumValueDescriptorProto found = null;
                    if (!string.IsNullOrEmpty(defaultValue))
                    {
                        found = enumType.Values.FirstOrDefault(x => x.Name == defaultValue);
                    }
                    else if (ctx.Syntax == FileDescriptorProto.SyntaxProto2)
                    {
                        found = enumType.Values.FirstOrDefault();
                    }

                    if (found != null)
                    {
                        // defaultValue = ctx.NameNormalizer.GetName(found);
                        defaultValue = found.Name;
                    }
                    if (!string.IsNullOrEmpty(defaultValue))
                    {
                        var typeName = GetTypeName(ctx, obj, out var dataFormat, out var isMap);
                        defaultValue = typeName + "." + defaultValue;
                    }
                }
            }

            return defaultValue;
        }
        /// <summary>
        /// Write a field
        /// </summary>
        protected override void WriteField(GeneratorContext ctx, FieldDescriptorProto obj, ref object state, OneOfStub[] oneOfs)
        {
            //var name = ctx.NameNormalizer.GetName(obj);
            var tw = ctx.Output;

            bool isOptional = obj.label == FieldDescriptorProto.Label.LabelOptional;
            bool isRepeated = obj.label == FieldDescriptorProto.Label.LabelRepeated;

            OneOfStub oneOf = obj.ShouldSerializeOneofIndex() ? oneOfs?[obj.OneofIndex] : null;
            if (oneOf != null && oneOf.CountTotal == 1)
            {
                oneOf = null; // not really a one-of, then!
            }
            bool explicitValues = isOptional && oneOf == null && ctx.Syntax == FileDescriptorProto.SyntaxProto2
                && obj.type != FieldDescriptorProto.Type.TypeMessage
                && obj.type != FieldDescriptorProto.Type.TypeGroup;


            string defaultValue = null;
            bool suppressDefaultAttribute = !isOptional;
            var typeName = GetTypeName(ctx, obj, out var dataFormat, out var isMap);

            defaultValue = GetDefaltValue(ctx, obj, typeName);
            if (isRepeated)
            {
                var mapMsgType = isMap ? ctx.TryFind<DescriptorProto>(obj.TypeName) : null;
                if (mapMsgType != null)
                {
                    var keyTypeName = GetTypeName(ctx, mapMsgType.Fields.Single(x => x.Number == 1),
                        out var keyDataFormat, out var _);
                    var valueTypeName = GetTypeName(ctx, mapMsgType.Fields.Single(x => x.Number == 2),
                        out var valueDataFormat, out var _);
                    ctx.WriteLine($"{GetAccess(GetAccess(obj))} System.Collections.Generic.Dictionary<{keyTypeName}, {valueTypeName}> {Escape(obj.Name)} = new System.Collections.Generic.Dictionary<{keyTypeName}, {valueTypeName}>();");
                }
                // else if (UseArray(obj))
                //  {
                // ctx.WriteLine($"{GetAccess(GetAccess(obj))} {typeName}[] {Escape(name)} ;");
                //  }
                else
                {
                    ctx.WriteLine($"{GetAccess(GetAccess(obj))} System.Collections.Generic.List<{typeName}> {Escape(obj.Name)} = new System.Collections.Generic.List<{typeName}>();");
                }
            }
            else
            {

                if (!string.IsNullOrEmpty(defaultValue))
                    ctx.WriteLine($"{GetAccess(GetAccess(obj))} {typeName} {obj.Name} = {defaultValue};");
                else
                    ctx.WriteLine($"{GetAccess(GetAccess(obj))} {typeName} {obj.Name} ;");
            }
        }
        /// <summary>
        /// Starts an extgensions block
        /// </summary>
        protected override void WriteExtensionsHeader(GeneratorContext ctx, FileDescriptorProto obj, ref object state)
        {
            var name = obj?.Options?.GetOptions()?.ExtensionTypeName;
            if (string.IsNullOrEmpty(name)) name = "Extensions";
            ctx.WriteLine($"{GetAccess(GetAccess(obj))} static class {Escape(name)}").WriteLine("{").Indent();
        }
        /// <summary>
        /// Ends an extgensions block
        /// </summary>
        protected override void WriteExtensionsFooter(GeneratorContext ctx, FileDescriptorProto obj, ref object state)
        {
            ctx.Outdent().WriteLine("}");
        }
        /// <summary>
        /// Starts an extensions block
        /// </summary>
        protected override void WriteExtensionsHeader(GeneratorContext ctx, DescriptorProto obj, ref object state)
        {
            var name = obj?.Options?.GetOptions()?.ExtensionTypeName;
            if (string.IsNullOrEmpty(name)) name = "Extensions";
            ctx.WriteLine($"{GetAccess(GetAccess(obj))} static class {Escape(name)}").WriteLine("{").Indent();
        }
        /// <summary>
        /// Ends an extensions block
        /// </summary>
        protected override void WriteExtensionsFooter(GeneratorContext ctx, DescriptorProto obj, ref object state)
        {
            ctx.Outdent().WriteLine("}");
        }
        /// <summary>
        /// Write an extension
        /// </summary>
        protected override void WriteExtension(GeneratorContext ctx, FieldDescriptorProto field)
        {
            var type = GetTypeName(ctx, field, out string dataFormat, out bool isMap);

            if (isMap)
            {
                ctx.WriteLine("#error map extensions not yet implemented");
            }
            else if (field.label == FieldDescriptorProto.Label.LabelRepeated)
            {
                ctx.WriteLine("#error repeated extensions not yet implemented");
            }
            else
            {
                var msg = ctx.TryFind<DescriptorProto>(field.Extendee);
                var extendee = MakeRelativeName(field, msg, ctx.NameNormalizer);

                var @this = field.Parent is FileDescriptorProto ? "this " : "";
                string name = field.Name;
                var tw = ctx.WriteLine($"{GetAccess(GetAccess(field))} static {type} Get{name}({@this}{extendee} obj)")
                    .Write($"=> obj == null ? default({type}) : global::ProtoBuf.Extensible.GetValue<{type}>(obj, {field.Number}");
                if (!string.IsNullOrEmpty(dataFormat))
                {
                    tw.Write($", global::ProtoBuf.DataFormat.{dataFormat}");
                }
                tw.WriteLine(");");
                ctx.WriteLine();
                //  GetValue<TValue>(IExtensible instance, int tag, DataFormat format)
            }
        }

        private static bool UseArray(FieldDescriptorProto field)
        {
            switch (field.type)
            {
                case FieldDescriptorProto.Type.TypeBool:
                case FieldDescriptorProto.Type.TypeDouble:
                case FieldDescriptorProto.Type.TypeFixed32:
                case FieldDescriptorProto.Type.TypeFixed64:
                case FieldDescriptorProto.Type.TypeFloat:
                case FieldDescriptorProto.Type.TypeInt32:
                case FieldDescriptorProto.Type.TypeInt64:
                case FieldDescriptorProto.Type.TypeSfixed32:
                case FieldDescriptorProto.Type.TypeSfixed64:
                case FieldDescriptorProto.Type.TypeSint32:
                case FieldDescriptorProto.Type.TypeSint64:
                case FieldDescriptorProto.Type.TypeUint32:
                case FieldDescriptorProto.Type.TypeUint64:
                    return true;
                default:
                    return false;
            }
        }

        private string GetTypeName(GeneratorContext ctx, FieldDescriptorProto field, out string dataFormat, out bool isMap)
        {
            dataFormat = "";
            isMap = false;
            switch (field.type)
            {
                case FieldDescriptorProto.Type.TypeDouble:
                    return "double";
                case FieldDescriptorProto.Type.TypeFloat:
                    return "float";
                case FieldDescriptorProto.Type.TypeBool:
                    return "bool";
                case FieldDescriptorProto.Type.TypeString:
                    return "string";
                case FieldDescriptorProto.Type.TypeSint32:
                    dataFormat = nameof(DataFormat.ZigZag);
                    return "int";
                case FieldDescriptorProto.Type.TypeInt32:
                    return "int";
                case FieldDescriptorProto.Type.TypeSfixed32:
                    dataFormat = nameof(DataFormat.FixedSize);
                    return "int";
                case FieldDescriptorProto.Type.TypeSint64:
                    dataFormat = nameof(DataFormat.ZigZag);
                    return "long";
                case FieldDescriptorProto.Type.TypeInt64:
                    return "long";
                case FieldDescriptorProto.Type.TypeSfixed64:
                    dataFormat = nameof(DataFormat.FixedSize);
                    return "long";
                case FieldDescriptorProto.Type.TypeFixed32:
                    dataFormat = nameof(DataFormat.FixedSize);
                    return "uint";
                case FieldDescriptorProto.Type.TypeUint32:
                    return "uint";
                case FieldDescriptorProto.Type.TypeFixed64:
                    dataFormat = nameof(DataFormat.FixedSize);
                    return "ulong";
                case FieldDescriptorProto.Type.TypeUint64:
                    return "ulong";
                case FieldDescriptorProto.Type.TypeBytes:
                    return "byte[]";
                case FieldDescriptorProto.Type.TypeEnum:
                    switch (field.TypeName)
                    {
                        case ".bcl.DateTime.DateTimeKind":
                            return "global::System.DateTimeKind";
                    }
                    var enumType = ctx.TryFind<EnumDescriptorProto>(field.TypeName);
                    return MakeRelativeName(field, enumType, ctx.NameNormalizer);
                case FieldDescriptorProto.Type.TypeGroup:
                case FieldDescriptorProto.Type.TypeMessage:
                    switch (field.TypeName)
                    {
                        case WellKnownTypeTimestamp:
                            dataFormat = "WellKnown";
                            return "global::System.DateTime?";
                        case WellKnownTypeDuration:
                            dataFormat = "WellKnown";
                            return "global::System.TimeSpan?";
                        case ".bcl.NetObjectProxy":
                            return "object";
                        case ".bcl.DateTime":
                            return "global::System.DateTime?";
                        case ".bcl.TimeSpan":
                            return "global::System.TimeSpan?";
                        case ".bcl.Decimal":
                            return "decimal?";
                        case ".bcl.Guid":
                            return "global::System.Guid?";
                    }
                    var msgType = ctx.TryFind<DescriptorProto>(field.TypeName);
                    if (field.type == FieldDescriptorProto.Type.TypeGroup)
                    {
                        dataFormat = nameof(DataFormat.Group);
                    }
                    isMap = msgType?.Options?.MapEntry ?? false;
                    return MakeRelativeName(field, msgType, ctx.NameNormalizer);
                default:
                    return field.TypeName;
            }
        }

        private string MakeRelativeName(FieldDescriptorProto field, IType target, NameNormalizer normalizer)
        {
            if (target == null) return Escape(field.TypeName); // the only thing we know

            var declaringType = field.Parent;

            if (declaringType is IType)
            {
                var name = FindNameFromCommonAncestor((IType)declaringType, target, normalizer);
                if (!string.IsNullOrEmpty(name)) return name;
            }
            return Escape(field.TypeName); // give up!
        }

        // k, what we do is; we have two types; each knows the parent, but nothing else, so:
        // for each, use a stack to build the ancestry tree - the "top" of the stack will be the
        // package, the bottom of the stack will be the type itself. They will often be stacks
        // of different heights.
        //
        // Find how many is in the smallest stack; now take that many items, in turn, until we
        // get something that is different (at which point, put that one back on the stack), or 
        // we run out of items in one of the stacks.
        //
        // There are now two options:
        // - we ran out of things in the "target" stack - in which case, they are common enough to not
        //   need any resolution - just give back the fixed name
        // - we have things left in the "target" stack - in which case we have found a common ancestor,
        //   or the target is a descendent; either way, just concat what is left (including the package
        //   if the package itself was different)

        private string FindNameFromCommonAncestor(IType declaring, IType target, NameNormalizer normalizer)
        {
            // trivial case; asking for self, or asking for immediate child
            if (ReferenceEquals(declaring, target) || ReferenceEquals(declaring, target.Parent))
            {
                if (target is DescriptorProto) return Escape(normalizer.GetName((DescriptorProto)target));
                if (target is EnumDescriptorProto) return Escape(normalizer.GetName((EnumDescriptorProto)target));
                return null;
            }

            var origTarget = target;
            var xStack = new Stack<IType>();

            while (declaring != null)
            {
                xStack.Push(declaring);
                declaring = declaring.Parent;
            }
            var yStack = new Stack<IType>();

            while (target != null)
            {
                yStack.Push(target);
                target = target.Parent;
            }
            int lim = Math.Min(xStack.Count, yStack.Count);
            for (int i = 0; i < lim; i++)
            {
                declaring = xStack.Peek();
                target = yStack.Pop();
                if (!ReferenceEquals(target, declaring))
                {
                    // special-case: if both are the package (file), and they have the same namespace: we're OK
                    if (target is FileDescriptorProto && declaring is FileDescriptorProto &&
                        normalizer.GetName((FileDescriptorProto)declaring) == normalizer.GetName((FileDescriptorProto)target))
                    {
                        // that's fine, keep going
                    }
                    else
                    {
                        // put it back
                        yStack.Push(target);
                        break;
                    }
                }
            }
            // if we used everything, then the target is an ancestor-or-self
            if (yStack.Count == 0)
            {
                target = origTarget;
                if (target is DescriptorProto) return Escape(normalizer.GetName((DescriptorProto)target));
                if (target is EnumDescriptorProto) return Escape(normalizer.GetName((EnumDescriptorProto)target));
                return null;
            }

            var sb = new StringBuilder();
            while (yStack.Count != 0)
            {
                target = yStack.Pop();

                string nextName;
                if (target is FileDescriptorProto) nextName = normalizer.GetName((FileDescriptorProto)target);
                else if (target is DescriptorProto) nextName = normalizer.GetName((DescriptorProto)target);
                else if (target is EnumDescriptorProto) nextName = normalizer.GetName((EnumDescriptorProto)target);
                else return null;

                if (!string.IsNullOrEmpty(nextName))
                {
                    if (sb.Length == 0 && target is FileDescriptorProto) sb.Append("global::");
                    else if (sb.Length != 0) sb.Append('.');
                    sb.Append(Escape(nextName));
                }
            }
            return sb.ToString();
        }

        static bool IsAncestorOrSelf(IType parent, IType child)
        {
            while (parent != null)
            {
                if (ReferenceEquals(parent, child)) return true;
                parent = parent.Parent;
            }
            return false;
        }
        const string WellKnownTypeTimestamp = ".google.protobuf.Timestamp",
                     WellKnownTypeDuration = ".google.protobuf.Duration";
    }

}
