namespace MultiPlatformDebugger
{
    using MultiLib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class SearchHandler
    {
        private byte[] endBytes;
        private string endOffset = "0x1";
        private MultiConsoleAPI PS3;
        private string searchEndValue = "";
        private int searchMode;
        private string searchStartValue = "";
        private byte[] startBytes;
        private string startOffset = "0x0";

        public SearchHandler(string startOffset, string endOffset, MultiConsoleAPI PS3, byte[] startBytes, byte[] endBytes, string searchStartValue, string searchEndValue, int searchMode)
        {
            this.startOffset = startOffset;
            this.endOffset = endOffset;
            this.PS3 = PS3;
            this.startBytes = startBytes;
            this.endBytes = endBytes;
            this.searchMode = searchMode;
            this.searchStartValue = searchStartValue;
            this.searchEndValue = searchEndValue;
        }

        public List<OffsetItem> getRefreshResults(List<OffsetItem> resultList)
        {
            if (resultList != null)
            {
                byte[] bytes = null;
                foreach (OffsetItem item in resultList)
                {
                    bytes = PS3.Extension.ReadBytes(item.UintOffset, item.ByteLength);
                    if (bytes.Length > 4)
                    {
                        item.Value = Encoding.Default.GetString(bytes);
                    }
                    else if (bytes.Length >= 4)
                    {
                        item.HexValue = BitConverter.ToString(bytes).Replace("-", "").PadLeft(8, '0');
                        Array.Reverse(bytes);
                        item.Value = BitConverter.ToInt32(bytes, 0).ToString();
                    }
                    else if (bytes.Length >= 2)
                    {
                        item.HexValue = BitConverter.ToString(bytes).Replace("-", "").PadLeft(4, '0');
                        Array.Reverse(bytes);
                        item.Value = BitConverter.ToInt16(bytes, 0).ToString();
                    }
                    else if (bytes.Length == 1)
                    {
                        item.HexValue = BitConverter.ToString(bytes).Replace("-", "").PadLeft(2, '0');
                        Array.Reverse(bytes);
                        item.Value = bytes[0].ToString();
                    }
                }
                return resultList;
            }
            resultList = new List<OffsetItem>();
            return resultList;
        }

        public List<OffsetItem> getResults(string startOffset, string endOffset)
        {
            uint num = Convert.ToUInt32(startOffset, 0x10);
            int length = Convert.ToInt32(Convert.ToUInt32(endOffset, 0x10)) - Convert.ToInt32(num);
            return SimpleBoyerMooreSearch(PS3.Extension.ReadBytes(num, length), this.startBytes, num, this.searchStartValue);
        }

        public List<OffsetItem> getSeatchResults(List<OffsetItem> resultList)
        {
            List<OffsetItem> list = new List<OffsetItem>();
            if (resultList == null)
            {
                return list;
            }
            if (resultList.Count < 500)
            {
                byte[] strB = null;
                foreach (OffsetItem item in resultList)
                {
                    strB = PS3.Extension.ReadBytes(item.UintOffset, item.ByteLength);
                    if (safeEquals(this.startBytes, strB))
                    {
                        if (strB.Length > 4)
                        {
                            item.Value = Encoding.Default.GetString(strB);
                        }
                        else if (strB.Length >= 4)
                        {
                            item.HexValue = BitConverter.ToString(strB).Replace("-", "").PadLeft(8, '0');
                            Array.Reverse(strB);
                            item.Value = BitConverter.ToInt32(strB, 0).ToString();
                        }
                        else if (strB.Length >= 2)
                        {
                            item.HexValue = BitConverter.ToString(strB).Replace("-", "").PadLeft(4, '0');
                            Array.Reverse(strB);
                            item.Value = BitConverter.ToInt16(strB, 0).ToString();
                        }
                        else if (strB.Length == 1)
                        {
                            item.HexValue = BitConverter.ToString(strB).Replace("-", "").PadLeft(2, '0');
                            Array.Reverse(strB);
                            item.Value = strB[0].ToString();
                        }
                        list.Add(item);
                    }
                }
                return list;
            }
            string offset = resultList.First<OffsetItem>().Offset;
            string endOffset = resultList.Last<OffsetItem>().Offset;
            List<OffsetItem> list2 = new List<OffsetItem>();
            using (List<OffsetItem>.Enumerator enumerator2 = this.getResults(offset, endOffset).GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    Func<OffsetItem, bool> predicate = null;
                    OffsetItem currentItem = enumerator2.Current;
                    predicate = offsetitem => offsetitem.Offset == currentItem.Offset;
                    if (resultList.Any<OffsetItem>(predicate))
                    {
                        resultList.Remove(currentItem);
                        list2.Add(currentItem);
                    }
                }
            }
            return list2;
        }

        private static bool safeEquals(byte[] strA, byte[] strB)
        {
            int length = strA.Length;
            if (length != strB.Length)
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (strA[i] != strB[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static List<OffsetItem> SimpleBoyerMooreSearch(byte[] haystack, byte[] needle, uint uintStartOffset, string searchStartValue)
        {
            uint num = 0;
            int length = needle.Length;
            string hexValue = BitConverter.ToString(needle).Replace("-", "");
            List<OffsetItem> list = new List<OffsetItem>();
            int[] numArray = new int[0x100];
            for (int i = 0; i < numArray.Length; i++)
            {
                numArray[i] = needle.Length;
            }
            for (int j = 0; j < needle.Length; j++)
            {
                numArray[needle[j]] = (needle.Length - j) - 1;
            }
            int index = needle.Length - 1;
            byte num6 = needle.Last<byte>();
            while (index < haystack.Length)
            {
                byte num7 = haystack[index];
                if (haystack[index] != num6)
                {
                    goto Label_0111;
                }
                bool flag = true;
                for (int k = needle.Length - 2; k >= 0; k--)
                {
                    if (haystack[((index - needle.Length) + k) + 1] != needle[k])
                    {
                        goto Label_00BD;
                    }
                }
                goto Label_00C0;
            Label_00BD:
                flag = false;
            Label_00C0:
                if (flag)
                {
                    num = uintStartOffset + Convert.ToUInt32((int) ((index - needle.Length) + 1));
                    string offset = "0x" + string.Format("{0:X}", num);
                    list.Add(new OffsetItem(offset, num, hexValue, searchStartValue, length));
                    index++;
                }
                else
                {
                    index++;
                }
                continue;
            Label_0111:
                index += numArray[num7];
            }
            return list;
        }
    }
}

