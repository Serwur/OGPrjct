namespace ColdCry.Utility
{
    public static class StringOperations
    {
        public static string InsertEvery(string text, int startIndex, int every, char @char)
        {
            if (startIndex < 0 || startIndex >= text.Length) {
                return text;
            }
            float addingChars = ( text.Length - startIndex ) / every;
            if (addingChars > (int) addingChars) {
                addingChars = (int) ( addingChars + 1 );
            }
            int len = (int) addingChars + text.Length;
            char[] newText = new char[len];
            char[] oldText = text.ToCharArray();

            var o = 0;
            var i = 0;

            // Filling first chars of old array until it reaches start index
            for (; i < len; i++) {
                if (i >= startIndex)
                    break;
                newText[i] = oldText[o];
                o++;
            }

            // Filling new chars and old array
            for (; i < len; i++) {
                if (( i - startIndex ) % ( every + 1 ) == 0) {
                    newText[i] = @char;
                } else {
                    newText[i] = oldText[o];
                    o++;
                }
            }

            return new string( newText );
        }

        public static string InsertEveryEnd(string text, int startIndex, int every, char @char)
        {
            if (startIndex < 0 || startIndex >= text.Length) {
                return text;
            }
            float addingChars = (float) ( startIndex ) / every;
            if (addingChars > ( (int) addingChars )) {
                addingChars = (int) ( addingChars + 1 );
            }
            int len = (int) addingChars + text.Length;
            char[] newText = new char[len];
            char[] oldText = text.ToCharArray();

            var newStartIndex = startIndex + (len - text.Length) - 1;
            var o = text.Length - 1;
            var i = len - 1;

            // Filling new chars and old array
            for (; o >= 0; i--) {
                if (o < startIndex)
                    break;
                newText[i] = oldText[o];
                o--;
            }

            // Filling the rest of old array
            for (; i >= 0; i--) {
                if (( newStartIndex - i ) % ( every + 1 ) == 0) {
                    newText[i] = @char;
                } else {
                    newText[i] = oldText[o];
                    o--;
                }
            }

            return new string( newText );
        }
    }
}
