using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighCInterpreterCore
{
    class HighCTokenizer
    {
        private String buffer="";
        private List<HighCToken> tokens;

        private int tokenStartPosition = -1;
        private int lineNumber = 1;
        private int columnNumber = 1;
        private int startingColumnNumber = 1;
        private int mode = modes.CONSTRUCTING_NEW_TOKEN;
        private String currentToken="";
        private String debugLog = "";

        private static class modes
        {
            public const int CONSTRUCTING_NEW_TOKEN = 0;
            public const int CHECKING_FOR_COMMENTS = 1;
            public const int CONSTRUCTING_COMMENT = 2;
            public const int CONSTRUCTING_DOUBLE_QUOTE = 3;
            public const int CHECKING_FOR_ELLIPSES = 4;
            public const int CHECKING_FOR_ELLIPSES_CONTINUED = 5;
            public const int CHECKING_FOR_EQUAL_SIGN = 6;
            public const int CHECKING_FOR_GREATER_THAN = 7;
            public const int CONSTRUCTING_IDENTIFIER_OR_LITERAL = 8;
            public const int CHECKING_FOR_LETTER = 9;
            public const int CONSTRUCTING_NUMBER_PRE_PERIOD = 10;
            public const int CONSTRUCTING_NUMBER_POST_PERIOD_NUMERAL_REQUIRED = 11;
            public const int CONSTRUCTING_NUMBER_POST_PERIOD = 12;
            public const int CHECKING_FOR_EXPONENT = 13;
        }

        public HighCTokenizer()
        {
            tokens = new List<HighCToken>();
        }

        public List<HighCToken> getTokens()
        {
            return tokens;
        }

        public bool tokenize(String newBuffer)
        {
            //If a token is already being formed, store the contents
            if(tokenStartPosition!=-1)
            {
                currentToken = buffer.Substring(tokenStartPosition);
                tokenStartPosition = 0;
            }

            buffer = newBuffer;

            int i = 0;
            
            while(i<buffer.Length)
            {
                Char characterBuffer = buffer.ElementAt<char>(i);
                bool advance = true;

                switch (mode)
                {
                    case modes.CONSTRUCTING_NEW_TOKEN:
                        //Brackets immediately form a new token
                        //Eg: { } ( ) [ ]
                        if (isBracket(characterBuffer))
                        {
                            tokens.Add(new HighCToken(buffer.Substring(i, 1), lineNumber, columnNumber));
                            currentToken = "";
                        }
                        //The period token . can also be part of the ellipses token ...
                        else if (characterBuffer == '.')
                        {
                            tokenStartPosition = i;
                            currentToken = ".";
                            startingColumnNumber = columnNumber;
                            mode = modes.CHECKING_FOR_ELLIPSES;
                        }
                        else if (characterBuffer == '<' || characterBuffer == '>' || characterBuffer == '~')
                        {
                            tokenStartPosition = i;
                            currentToken += characterBuffer;
                            startingColumnNumber = columnNumber;
                            mode = modes.CHECKING_FOR_EQUAL_SIGN;
                        }
                        else if (characterBuffer == '=')
                        {
                            tokenStartPosition = i;
                            currentToken += characterBuffer;
                            startingColumnNumber = columnNumber;
                            mode = modes.CHECKING_FOR_GREATER_THAN;
                        }
                        //Check for /, could be the start of a comment
                        else if (characterBuffer == '/')
                        {
                            tokenStartPosition = i;
                            currentToken = "/";
                            startingColumnNumber = columnNumber;
                            mode = modes.CHECKING_FOR_COMMENTS;
                        }
                        //Check for " to create a string symbol
                        else if (characterBuffer == '\"')
                        {
                            tokenStartPosition = i;
                            currentToken = "\"";
                            startingColumnNumber = columnNumber;
                            mode = modes.CONSTRUCTING_DOUBLE_QUOTE;
                        }
                        //Identifiers, Keywords, and Non-Numeric Constants
                        else if (characterBuffer == '?')
                        {
                            currentToken = "?";
                            tokenStartPosition = i;
                            startingColumnNumber = columnNumber;
                            mode = modes.CHECKING_FOR_LETTER;
                        }
                        /*
                        else if(characterBuffer == 'e' || characterBuffer == 'E')
                        {
                            currentToken += characterBuffer;
                            tokenStartPosition = i;
                            startingColumnNumber = columnNumber;
                            mode = modes.CHECKING_FOR_EXPONENT;
                        }
                        */
                        else if (isLetter(characterBuffer))
                        {
                            //Start forming token
                            currentToken += characterBuffer;
                            tokenStartPosition = i;
                            startingColumnNumber = columnNumber;
                            mode = modes.CONSTRUCTING_IDENTIFIER_OR_LITERAL;
                        }
                        //Number Constants
                        else if (isNumber(characterBuffer))
                        {
                            currentToken += characterBuffer;
                            tokenStartPosition = i;
                            startingColumnNumber = columnNumber;
                            mode = modes.CONSTRUCTING_NUMBER_PRE_PERIOD;
                        }
                        //Single character operators also form tokens immediately
                        //Eg: +, -, @, ~, <, =, etc
                        else if (isOperator(characterBuffer))
                        {
                            tokens.Add(new HighCToken(buffer.Substring(i, 1), lineNumber, columnNumber));
                            currentToken = "";
                        }
                        //White Space is ignored
                        else if (isWhiteSpace(characterBuffer)) { }
                        //Anything unexpected should result in an error
                        else
                        {
                            Console.WriteLine("Error: Unexpected Character Found: '" + characterBuffer +"' ASCII#"+(int)(characterBuffer));
                            debugLog = "An unexpected character was found on (L"+lineNumber+", C"+columnNumber+"): '" + characterBuffer +"' ASCII#"+(int)(characterBuffer);
                            return false;
                        }
                        break;
                    case modes.CHECKING_FOR_COMMENTS:
                        if (characterBuffer == '/')
                        {
                            mode = modes.CONSTRUCTING_COMMENT;
                            currentToken += '/';
                        }
                        else
                        {
                            tokens.Add(new HighCToken(currentToken, lineNumber, startingColumnNumber));
                            tokenStartPosition = -1;
                            currentToken = "";
                            advance = false;
                            mode = modes.CONSTRUCTING_NEW_TOKEN;
                        }
                        break;
                    case modes.CONSTRUCTING_COMMENT:
                        //Unicode 10: Line Feed
                        //Unicode 13: Carriage Return
                        if (characterBuffer == 10 || characterBuffer == 13)
                        {
                            //tokens.Add(new HighCToken(currentToken, lineNumber, startingColumnNumber));
                            currentToken = "";
                            tokenStartPosition = -1;
                            mode = modes.CONSTRUCTING_NEW_TOKEN;
                        }
                        else
                        {
                            currentToken += characterBuffer.ToString();
                        }
                        break;
                    case modes.CONSTRUCTING_DOUBLE_QUOTE:
                        if (characterBuffer == '\"')
                        {
                            currentToken += '\"';
                            tokens.Add(new HighCToken(currentToken, lineNumber, startingColumnNumber));
                            currentToken = "";
                            tokenStartPosition = -1;
                            mode = modes.CONSTRUCTING_NEW_TOKEN;
                        }
                        else
                        {
                            currentToken += characterBuffer.ToString();
                        }
                        break;
                    case modes.CHECKING_FOR_ELLIPSES:
                        if (characterBuffer == '.')
                        {
                            currentToken += '.';
                            mode = modes.CHECKING_FOR_ELLIPSES_CONTINUED;
                        }
                        else if (isNumber(characterBuffer))
                        {
                            currentToken += characterBuffer;
                            mode = modes.CONSTRUCTING_NUMBER_POST_PERIOD;
                        }
                        else
                        {
                            tokens.Add(new HighCToken(currentToken, lineNumber, startingColumnNumber));
                            tokenStartPosition = -1;
                            currentToken = "";
                            advance = false;
                            mode = modes.CONSTRUCTING_NEW_TOKEN;
                        }
                        break;
                    case modes.CHECKING_FOR_ELLIPSES_CONTINUED:
                        if (characterBuffer == '.')
                        {
                            currentToken += '.';
                            tokens.Add(new HighCToken(currentToken, lineNumber, startingColumnNumber));
                            tokenStartPosition = -1;
                            currentToken = "";
                            mode = modes.CONSTRUCTING_NEW_TOKEN;
                            
                        }
                        //The token .. is not valid outside of a comment/quote
                        else
                        {
                            Console.WriteLine("Error: Unexpected Character Found: '" + characterBuffer +"' ASCII#"+(int)(characterBuffer));
                            debugLog = "While generating an ellipse, an unexpected character was found on (L" + lineNumber + ", C" + columnNumber + "): '" + characterBuffer +"' ASCII#"+(int)(characterBuffer);
                            debugLog += Environment.NewLine+"The Tokenizer was expecting a '.'";
                            return false;
                            
                        }
                        break;
                    case modes.CHECKING_FOR_EQUAL_SIGN:
                        if (characterBuffer == '=')
                        {
                            currentToken += '=';
                        }
                        else
                        {
                            advance = false;
                        }
                        tokens.Add(new HighCToken(currentToken, lineNumber, startingColumnNumber));
                        tokenStartPosition = -1;
                        currentToken = "";
                        mode = modes.CONSTRUCTING_NEW_TOKEN;
                        break;
                    case modes.CHECKING_FOR_GREATER_THAN:
                        if (characterBuffer == '>')
                        {
                            currentToken += '>';
                        }
                        else
                        {
                            advance = false;
                        }
                        tokens.Add(new HighCToken(currentToken, lineNumber, startingColumnNumber));
                        tokenStartPosition = -1;
                        currentToken = "";
                        mode = modes.CONSTRUCTING_NEW_TOKEN;
                        break;
                    case modes.CHECKING_FOR_LETTER:
                        if (isLetter(characterBuffer))
                        {
                            currentToken += characterBuffer;
                            mode = modes.CONSTRUCTING_IDENTIFIER_OR_LITERAL;
                        }
                        //? must immediately be followed by a letter
                        else
                        {
                            Console.WriteLine("Error: Unexpected Character Found: '" + characterBuffer +"' ASCII#"+(int)(characterBuffer));
                            debugLog = "While generating an identifier, an unexpected character was found on (L" + lineNumber + ", C" + columnNumber + "): '" + characterBuffer +"' ASCII#"+(int)(characterBuffer);
                            debugLog += Environment.NewLine + "The Tokenizer was expecting a letter.";
                            return false;
                        }
                        break;
                    case modes.CONSTRUCTING_IDENTIFIER_OR_LITERAL:
                        if (isAlphaNumeric(characterBuffer))
                        {
                            currentToken += characterBuffer;
                        }
                        else if(isBracket(characterBuffer) || isOperator(characterBuffer) || isWhiteSpace(characterBuffer))
                        {
                            tokens.Add(new HighCToken(currentToken, lineNumber, startingColumnNumber));
                            tokenStartPosition = -1;
                            currentToken = "";
                            mode = modes.CONSTRUCTING_NEW_TOKEN;
                            advance = false;
                        }
                        else
                        {
                            Console.WriteLine("Error: Unexpected Character Found: '" + characterBuffer + "' ASCII#" + (int)(characterBuffer));
                            debugLog = "While generating an identifier, an unexpected character was found on (L" + lineNumber + ", C" + columnNumber + "): '" + characterBuffer + "' ASCII#" + (int)(characterBuffer);
                            debugLog += Environment.NewLine + "The Tokenizer was expecting a letter, numeral, white space, bracket, or operator.";
                            return false;
                        }
                        break;
                    case modes.CONSTRUCTING_NUMBER_PRE_PERIOD:
                        if (isNumber(characterBuffer))
                        {
                            currentToken += characterBuffer;
                        }
                        else if (characterBuffer == '.')
                        {
                            currentToken += '.';
                            mode = modes.CONSTRUCTING_NUMBER_POST_PERIOD;
                        }
                        else if (isBracket(characterBuffer) || isOperator(characterBuffer) || isWhiteSpace(characterBuffer))
                        {
                            tokens.Add(new HighCToken(currentToken, lineNumber, startingColumnNumber));
                            tokenStartPosition = -1;
                            currentToken = "";
                            mode = modes.CONSTRUCTING_NEW_TOKEN;
                            advance = false;
                        }
                        else
                        {
                            Console.WriteLine("Error: Unexpected Character Found: '" + characterBuffer + "' ASCII#" + (int)(characterBuffer));
                            debugLog = "While generating a number, an unexpected character was found on (L" + lineNumber + ", C" + columnNumber + "): '" + characterBuffer + "' ASCII#" + (int)(characterBuffer);
                            debugLog += Environment.NewLine + "The Tokenizer was expecting a numeral, a period, white space, bracket, or operator.";
                            return false;
                        }
                        break;
                        /*
                         *High C does NOT require a numeral following after the period in a numeric literal unless there were no numbers prior to the period.
                         */ 
                    case modes.CONSTRUCTING_NUMBER_POST_PERIOD_NUMERAL_REQUIRED:
                        if (isNumber(characterBuffer))
                        {
                            currentToken += characterBuffer;
                            mode = modes.CONSTRUCTING_NUMBER_POST_PERIOD;
                        }
                        else
                        {
                            Console.WriteLine("Error: Unexpected Character Found: '" + characterBuffer +"' ASCII#"+(int)(characterBuffer));
                            debugLog = "While generating a number, an unexpected character was found on (L" + lineNumber + ", C" + columnNumber + "): '" + characterBuffer +"' ASCII#"+(int)(characterBuffer);
                            debugLog += Environment.NewLine + "The Tokenizer was expecting a numeral.";
                            return false;
                        }
                        break;
                    case modes.CONSTRUCTING_NUMBER_POST_PERIOD:
                        if (isNumber(characterBuffer))
                        {
                            currentToken += characterBuffer;
                        }
                        else if (characterBuffer == 'e' || characterBuffer == 'E')
                        {
                            tokens.Add(new HighCToken(currentToken, lineNumber, startingColumnNumber));
                            tokenStartPosition = -1;
                            currentToken = ""+characterBuffer;
                            tokens.Add(new HighCToken(currentToken, lineNumber, columnNumber));
                            currentToken = "";
                            mode = modes.CHECKING_FOR_EXPONENT;
                        }
                        else if(characterBuffer=='.')
                        {
                            currentToken = currentToken.Substring(0, currentToken.Length - 1);
                            tokens.Add(new HighCToken(currentToken, lineNumber, startingColumnNumber));
                            startingColumnNumber = columnNumber - 1;
                            currentToken = "..";
                            mode = modes.CHECKING_FOR_ELLIPSES_CONTINUED;
                        }
                        else if (isBracket(characterBuffer) || isOperator(characterBuffer) || isWhiteSpace(characterBuffer))
                        {
                            tokens.Add(new HighCToken(currentToken, lineNumber, startingColumnNumber));
                            tokenStartPosition = -1;
                            currentToken = "";
                            mode = modes.CONSTRUCTING_NEW_TOKEN;
                            advance = false;
                        }
                        else
                        {
                            Console.WriteLine("Error: Unexpected Character Found: '" + characterBuffer + "' ASCII#" + (int)(characterBuffer));
                            debugLog = "While generating a number, an unexpected character was found on (L" + lineNumber + ", C" + columnNumber + "): '" + characterBuffer + "' ASCII#" + (int)(characterBuffer);
                            debugLog += Environment.NewLine + "The Tokenizer was expecting a numeral, white space, bracket, or operator.";
                            return false;
                        }
                        break;
                    case modes.CHECKING_FOR_EXPONENT:
                        if(isNumber(characterBuffer))
                        {
                            mode = modes.CONSTRUCTING_NEW_TOKEN;
                        }
                        else
                        {
                            Console.WriteLine("Error: Unexpected Character Found: '" + characterBuffer + "' ASCII#" + (int)(characterBuffer));
                            debugLog = "While generating an exponent, an unexpected character was found on (L" + lineNumber + ", C" + columnNumber + "): '" + characterBuffer + "' ASCII#" + (int)(characterBuffer);
                            debugLog += Environment.NewLine + "The Tokenizer was expecting a numeral";
                            return false;
                        }
                        advance = false;
                        break;
                    default:
                        Console.WriteLine("Error: This case should never be initiated.");
                        break;
                }
                
                if(advance==true)
                {
                    columnNumber++;

                    //Line and Column Tracking
                    //Unicode 10: Line Feed
                    //Unicode 13: Carriage Return
                    if (characterBuffer == 10/* || characterBuffer == 13*/)
                    {
                        lineNumber++;
                        columnNumber = 1;
                    }
                    else if (characterBuffer == 13)
                    {
                        columnNumber = 1;
                    }
                    i++;
                }
            }

            return true;
        }

        public void finalizeTokenization()
        {
            if (mode != modes.CONSTRUCTING_NEW_TOKEN)
            {
                tokens.Add(new HighCToken(currentToken, lineNumber, startingColumnNumber));
                tokenStartPosition = -1;
            }
        }

        public String getDebugLog( )
        {
            return debugLog;
        }

        private Boolean isLetter(char currentCharacter)
        {
            return char.IsLetter(currentCharacter);
        }

        private Boolean isNumber(char currentCharacter)
        {
            return char.IsNumber(currentCharacter);
        }

        private Boolean isAlphaNumeric(char currentCharacter)
        {
            return char.IsLetterOrDigit(currentCharacter) || currentCharacter=='_';
        }

        private Boolean isBracket(char currentCharacter)
        {
            if(currentCharacter=='{' || currentCharacter=='}' ||
               currentCharacter=='(' || currentCharacter==')' ||
               currentCharacter=='[' || currentCharacter==']')
            {
                return true;
            }
            return false;
        }

        private Boolean isOperator(char currentCharacter)
        {
            //Math Operators
            if (currentCharacter == '+' || currentCharacter == '-' ||
                currentCharacter == '*' || currentCharacter == '/' ||
                currentCharacter == '%' || currentCharacter == '^')
            {
                return true;
            }
            //Logical Operators
            if (currentCharacter == '=' || currentCharacter == '~' ||
                currentCharacter == '<' || currentCharacter == '>' ||
                currentCharacter == '&' || currentCharacter == '|')
            {
                return true;
            }
            //Misc Operators
            if (currentCharacter == '@' || currentCharacter == '$' ||
                currentCharacter == ':' || currentCharacter == '#' ||
                currentCharacter == '.' || currentCharacter == ',')
            {
                return true;
            }

            return false;
        }
        
        private Boolean isWhiteSpace(Char currentCharacter)
        {
            return char.IsWhiteSpace(currentCharacter);
        }
    }

    class HighCToken
    {
        private String text;
        private String type = "Unknown";
        private int line;
        private int column;

        public HighCToken(String newText, int lineNumber, int columnNumber)
        {
            text = newText;
            line = lineNumber;
            column = columnNumber;
        }
        
        public String Text
        {
            get { return text; }
        }

        public String Type
        {
            get { return type; }
            set { type = value; }
        }

        public int Line
        {
            get { return line; }
        }

        public int Column
        {
            get { return column; }
        }

        public override string ToString()
        {
            if(type=="Unknown")
            {
                return "Line: " + line + " \tColumn: " + column +" \t"+ text;
            }

            return "Line: " + line + " \tColumn: " + column + " \tType: " + type + getSpaces(20-type.Length) + text;
        }

        private String getSpaces(int spaces)
        {
            String temp = "";

            int i = 0;
            while(i<spaces)
            {
                temp += " ";
                i++;
            }

            return temp;
        }
    }
}
