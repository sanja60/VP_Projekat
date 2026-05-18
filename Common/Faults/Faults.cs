using System;
using System.Runtime.Serialization;

namespace Common.Faults
{
    [DataContract]
    public class ValidationFault
    {
        [DataMember] public string Message { get; set; }
        [DataMember] public int RowIndex { get; set; }

        public ValidationFault(string message, int rowIndex)
        {
            Message = message;
            RowIndex = rowIndex;
        }
    }

    [DataContract]
    public class DataFormatFault
    {
        [DataMember] public string Message { get; set; }
        [DataMember] public int RowIndex { get; set; }
        [DataMember] public string RawLine { get; set; }

        public DataFormatFault(string message, int rowIndex, string rawLine)
        {
            Message = message;
            RowIndex = rowIndex;
            RawLine = rawLine;
        }
    }
}
