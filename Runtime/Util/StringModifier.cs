using System;
using System.Text;

namespace com.swecial.util {
    public class StringModifier {

        private StringBuilder sb;
        private int _index;

        public StringModifier(string s) {
            sb = new StringBuilder(s);
        }
        public bool toNext(string match) {
            return toNext(match, false);
        }
        public bool toNext(string match, bool ignoreCase) {
            int i = indexOf(_index, match, ignoreCase);
            return setIndex(i);
        }
        public int indexOf(int startIndex, string match) {
            return indexOf(startIndex, match, false);
        }
        public int indexOf(int startIndex, string match, bool ignoreCase) {
            int si = startIndex;
            int mi = 0;
            int mLength = match.Length;
            int maxSearchIndex = sb.Length - mLength;
            if (String.IsNullOrEmpty(match)) {
                return -1;
            }
            if (startIndex > maxSearchIndex) {
                return -1;
            }
            while (true) {
                if (si > maxSearchIndex) {
                    return -1;
                }
                char sc = sb[si + mi];
                char mc = match[mi];
                if (ignoreCase) {
                    sc = char.ToLower(sc);
                    mc = char.ToLower(mc);
                }
                if (sc == mc) {
                    if (mi >= mLength - 1) {
                        return si;
                    } else {
                        mi++;
                    }
                } else {
                    si++;
                    mi = 0;
                }
            }
        }
        public string getBlock(string blockStart, string blockEnd) {
            int i = _index;
            if (i == -1) {
                i = 0;
            }
            int nextStart = 0;
            int nextEnd = 0;
            int starts = 0;
            int firstStart = indexOf(i, blockStart);
            while (true) {
                nextStart = indexOf(i, blockStart);
                nextEnd = indexOf(i, blockEnd);
                if (nextEnd > -1) {
                    if (nextStart > -1 && nextStart < nextEnd) {
                        starts++;
                        i = nextStart + 1;
                    } else {
                        if (starts > 1) {
                            starts--;
                            i = nextEnd + 1;
                        } else {
                            return sb.ToString(firstStart, nextEnd - firstStart + 1);
                        }
                    }
                } else {
                    return null;
                }
            }
        }
        public void forward() {
            move(1);
        }
        public void back() {
            move(-1);
        }
        public bool toStartOfNextLine() {
            int nextLf = indexOf(_index, "\n");
            int nextCr = indexOf(_index, "\r");
            int nextCrLf = indexOf(_index, "\r\n");
            if (nextCrLf > -1 && nextCrLf < nextLf) {
                return setIndex(nextCrLf + 2);
            } else if (nextCr > -1 && nextCr < nextLf) {
                return setIndex(nextCr + 1);
            } else if (nextLf > -1) {
                return setIndex(nextLf + 1);
            } else {
                return false;
            }
        }
        public bool setIndex(int i) {
            if (i < sb.Length && i > -1) {
                _index = i;
                return true;
            } else {
                return false;
            }
        }
        public void toStart() {
            _index = 0;
        }
        public bool move(int add) {
            return setIndex(_index + add);
        }
        public string getCodeBlock() {
            return getBlock("{", "}");
        }
        public void inject(string s) {
            sb.Insert(_index, s);
        }
        public override string ToString() {
            return sb.ToString();
        }
    }
}
