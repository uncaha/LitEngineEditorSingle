using Google.Protobuf.Reflection;
using Google.Protobuf;


namespace ProtoTool
{

    //Google.Protobuf.WireFormat.WireType tt = ProtoTool.ILHelper.GetWireType(obj.type);
   // uint ttag = Google.Protobuf.WireFormat.MakeTag(obj.Number, tt);
    public class ILHelper
    {
        static public WireFormat.WireType GetWireType(FieldDescriptorProto.Type _type)
        {
            switch(_type)
            {
                case FieldDescriptorProto.Type.TypeBool:             
                case FieldDescriptorProto.Type.TypeFixed32:
                case FieldDescriptorProto.Type.TypeFixed64:
                case FieldDescriptorProto.Type.TypeInt32:
                case FieldDescriptorProto.Type.TypeInt64:
                case FieldDescriptorProto.Type.TypeUint32:
                case FieldDescriptorProto.Type.TypeUint64:  
                case FieldDescriptorProto.Type.TypeSint32:
                case FieldDescriptorProto.Type.TypeSint64:
                case FieldDescriptorProto.Type.TypeSfixed32:
                case FieldDescriptorProto.Type.TypeSfixed64:
                case FieldDescriptorProto.Type.TypeEnum:
                    return WireFormat.WireType.Varint;

                case FieldDescriptorProto.Type.TypeFloat:
                    return WireFormat.WireType.Fixed32;
                case FieldDescriptorProto.Type.TypeDouble:
                    return WireFormat.WireType.Fixed64;

                case FieldDescriptorProto.Type.TypeBytes:
                case FieldDescriptorProto.Type.TypeString:
                    return WireFormat.WireType.LengthDelimited;

                case FieldDescriptorProto.Type.TypeMessage:
                    return WireFormat.WireType.LengthDelimited;

            }

            return WireFormat.WireType.Varint;
        }
    }
}
