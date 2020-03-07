using System;

namespace business
{
    public static class Extensions
    {
        public static bool IsNumericType(this object o)
        {   
            return IsNumeric(o.GetType());
        }

        public static bool IsNumeric(this Type t)
        {   
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                return true;
                default:
                return false;
            }
        }

    }
}