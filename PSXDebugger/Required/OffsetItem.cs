namespace MultiPlatformDebugger
{
    using System;
    using System.Runtime.CompilerServices;

    internal class OffsetItem
    {
        public OffsetItem()
        {
        }

        public OffsetItem(string Offset, uint UintOffset, string HexValue, string Value, int ByteLength)
        {
            this.Offset = Offset;
            this.HexValue = HexValue;
            this.Value = Value;
            this.UintOffset = UintOffset;
            this.ByteLength = ByteLength;
        }

        public int ByteLength { get; set; }

        public string HexValue { get; set; }

        public string Offset { get; set; }

        public uint UintOffset { get; set; }

        public string Value { get; set; }
    }
}

