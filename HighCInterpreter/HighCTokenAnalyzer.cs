using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighCInterpreterCore
{
    class HighCTokenAnalyzer
    {
        public static void analyzeTokens(List<HighCToken> tokens)
        {
            HighCToken previousToken = new HighCToken("",0,0);
            foreach(HighCToken token in tokens)
            {
                switch(token.Text)
                {
                    case HighCTokenLibrary.UNDERSCORE:
                        token.Type = HighCTokenLibrary.UNDERSCORE; break;
                    case HighCTokenLibrary.MINUS_SIGN:
                        token.Type = HighCTokenLibrary.MINUS_SIGN; break;
                    case HighCTokenLibrary.COMMA:
                        token.Type = HighCTokenLibrary.COMMA; break;
                    case HighCTokenLibrary.COLON:
                        token.Type = HighCTokenLibrary.COLON; break;
                    case HighCTokenLibrary.PERIOD:
                        token.Type = HighCTokenLibrary.PERIOD; break;
                    case HighCTokenLibrary.ELLIPSES:
                        token.Type = HighCTokenLibrary.ELLIPSES; break;
                    case HighCTokenLibrary.DOUBLE_QUOTE:
                        token.Type = HighCTokenLibrary.DOUBLE_QUOTE; break;
                    case HighCTokenLibrary.LEFT_PARENTHESIS:
                        token.Type = HighCTokenLibrary.LEFT_PARENTHESIS; break;
                    case HighCTokenLibrary.RIGHT_PARENTHESIS:
                        token.Type = HighCTokenLibrary.RIGHT_PARENTHESIS; break;
                    case HighCTokenLibrary.LEFT_SQUARE_BRACKET:
                        token.Type = HighCTokenLibrary.LEFT_SQUARE_BRACKET; break;
                    case HighCTokenLibrary.RIGHT_SQUARE_BRACKET:
                        token.Type = HighCTokenLibrary.RIGHT_SQUARE_BRACKET; break;
                    case HighCTokenLibrary.LEFT_CURLY_BRACKET:
                        token.Type = HighCTokenLibrary.LEFT_CURLY_BRACKET; break;
                    case HighCTokenLibrary.RIGHT_CURLY_BRACKET:
                        token.Type = HighCTokenLibrary.RIGHT_CURLY_BRACKET; break;
                    case HighCTokenLibrary.AT_SIGN:
                        token.Type = HighCTokenLibrary.AT_SIGN; break;
                    case HighCTokenLibrary.ASTERICK:
                        token.Type = HighCTokenLibrary.ASTERICK; break;
                    case HighCTokenLibrary.SLASH:
                        token.Type = HighCTokenLibrary.SLASH; break;
                    case HighCTokenLibrary.AMPERSAND:
                        token.Type = HighCTokenLibrary.AMPERSAND; break;
                    case HighCTokenLibrary.POUND_SIGN:
                        token.Type = HighCTokenLibrary.POUND_SIGN; break;
                    case HighCTokenLibrary.PERCENT_SIGN:
                        token.Type = HighCTokenLibrary.PERCENT_SIGN; break;
                    case HighCTokenLibrary.PLUS_SIGN:
                        token.Type = HighCTokenLibrary.PLUS_SIGN; break;
                    case HighCTokenLibrary.LESS_THAN:
                        token.Type = HighCTokenLibrary.LESS_THAN; break;
                    case HighCTokenLibrary.GREATER_THAN:
                        token.Type = HighCTokenLibrary.GREATER_THAN; break;
                    case HighCTokenLibrary.LESS_THAN_EQUAL:
                        token.Type = HighCTokenLibrary.LESS_THAN_EQUAL; break;
                    case HighCTokenLibrary.GREATER_THAN_EQUAL:
                        token.Type = HighCTokenLibrary.GREATER_THAN_EQUAL; break;
                    case HighCTokenLibrary.EQUAL:
                        token.Type = HighCTokenLibrary.EQUAL; break;
                    case HighCTokenLibrary.ARROW:
                        token.Type = HighCTokenLibrary.ARROW; break;
                    case HighCTokenLibrary.VERTICAL_BAR:
                        token.Type = HighCTokenLibrary.VERTICAL_BAR; break;
                    case HighCTokenLibrary.TILDE:
                        token.Type = HighCTokenLibrary.TILDE; break;
                    case HighCTokenLibrary.NOT_EQUAL:
                        token.Type = HighCTokenLibrary.NOT_EQUAL; break;
                    case HighCTokenLibrary.DOLLAR_SIGN:
                        token.Type = HighCTokenLibrary.DOLLAR_SIGN; break;
                    case HighCTokenLibrary.ABSTRACT:
                        token.Type = HighCTokenLibrary.ABSTRACT; break;
                    case HighCTokenLibrary.ADD_TO:
                        token.Type = HighCTokenLibrary.ADD_TO; break;
                    case HighCTokenLibrary.APPEND:
                        token.Type = HighCTokenLibrary.APPEND; break;
                    case HighCTokenLibrary.ARRAY:
                        token.Type = HighCTokenLibrary.ARRAY; break;
                    case HighCTokenLibrary.BOOLEAN:
                        token.Type = HighCTokenLibrary.BOOLEAN; break;
                    case HighCTokenLibrary.CALL:
                        token.Type = HighCTokenLibrary.CALL; break;
                    case HighCTokenLibrary.CHARACTER:
                        token.Type = HighCTokenLibrary.CHARACTER; break;
                    case HighCTokenLibrary.CHOICE:
                        token.Type = HighCTokenLibrary.CHOICE; break;
                    case HighCTokenLibrary.CLASS:
                        token.Type = HighCTokenLibrary.CLASS; break;
                    case HighCTokenLibrary.CONSTANT:
                        token.Type = HighCTokenLibrary.CONSTANT; break;
                    case HighCTokenLibrary.CREATE:
                        token.Type = HighCTokenLibrary.CREATE; break;
                    case HighCTokenLibrary.DISCRETE:
                        token.Type = HighCTokenLibrary.DISCRETE; break;
                    case HighCTokenLibrary.CARET:
                        token.Type = HighCTokenLibrary.CARET; break;
                    case HighCTokenLibrary.E:
                    case HighCTokenLibrary.UPPERCASE_E:
                        if (previousToken.Type == HighCTokenLibrary.FLOAT_LITERAL &&
                            previousToken.Line == token.Line &&
                            previousToken.Column+previousToken.Text.Length == token.Column)
                        {
                            token.Type = HighCTokenLibrary.EXPONENT;
                        }
                        else
                        {
                            token.Type = HighCTokenLibrary.IDENTIFIER;
                        }
                        break;
                    case HighCTokenLibrary.ELSE:
                        token.Type = HighCTokenLibrary.ELSE; break;
                    case HighCTokenLibrary.ELSE_IF:
                        token.Type = HighCTokenLibrary.ELSE_IF; break;
                    case HighCTokenLibrary.END_OF_LINE:
                        token.Type = HighCTokenLibrary.END_OF_LINE; break;
                    case HighCTokenLibrary.ENUMERATION:
                        token.Type = HighCTokenLibrary.ENUMERATION; break;
                    case HighCTokenLibrary.FALSE:
                        token.Type = HighCTokenLibrary.FALSE; break;
                    case HighCTokenLibrary.FINAL:
                        token.Type = HighCTokenLibrary.FINAL; break;
                    case HighCTokenLibrary.FLOAT:
                        token.Type = HighCTokenLibrary.FLOAT; break;
                    case HighCTokenLibrary.FOR:
                        token.Type = HighCTokenLibrary.FOR; break;
                    case HighCTokenLibrary.FOREACH_ARRAY:
                        token.Type = HighCTokenLibrary.FOREACH_ARRAY; break;
                    case HighCTokenLibrary.FOREACH_LIST:
                        token.Type = HighCTokenLibrary.FOREACH_LIST; break;
                    case HighCTokenLibrary.FUNCTION:
                        token.Type = HighCTokenLibrary.FUNCTION; break;
                    case HighCTokenLibrary.GLOBAL:
                        token.Type = HighCTokenLibrary.GLOBAL; break;
                    case HighCTokenLibrary.IF:
                        token.Type = HighCTokenLibrary.IF; break;
                    case HighCTokenLibrary.IN:
                        token.Type = HighCTokenLibrary.IN; break;
                    case HighCTokenLibrary.IN_OUT:
                        token.Type = HighCTokenLibrary.IN_OUT; break;
                    case HighCTokenLibrary.IN_REVERSE:
                        token.Type = HighCTokenLibrary.IN_REVERSE; break;
                    case HighCTokenLibrary.INSERT:
                        token.Type = HighCTokenLibrary.INSERT; break;
                    case HighCTokenLibrary.INSTANCE_OF:
                        token.Type = HighCTokenLibrary.INSTANCE_OF; break;
                    case HighCTokenLibrary.INTEGER:
                        token.Type = HighCTokenLibrary.INTEGER; break;
                    case HighCTokenLibrary.LAST:
                        token.Type = HighCTokenLibrary.LAST; break;
                    case HighCTokenLibrary.LENGTH:
                        token.Type = HighCTokenLibrary.LENGTH; break;
                    case HighCTokenLibrary.LOOP:
                        token.Type = HighCTokenLibrary.LOOP; break;
                    case HighCTokenLibrary.MAIN:
                        token.Type = HighCTokenLibrary.MAIN; break;
                    case HighCTokenLibrary.MATCH:
                        token.Type = HighCTokenLibrary.MATCH; break;
                    case HighCTokenLibrary.METHOD:
                        token.Type = HighCTokenLibrary.METHOD; break;
                    case HighCTokenLibrary.MULTIPLE:
                        token.Type = HighCTokenLibrary.MULTIPLE; break;
                    case HighCTokenLibrary.NEXT:
                        token.Type = HighCTokenLibrary.NEXT; break;
                    case HighCTokenLibrary.ON:
                        token.Type = HighCTokenLibrary.ON; break;
                    case HighCTokenLibrary.OTHER:
                        token.Type = HighCTokenLibrary.OTHER; break;
                    case HighCTokenLibrary.OUT:
                        token.Type = HighCTokenLibrary.OUT; break;
                    case HighCTokenLibrary.PREVIOUS:
                        token.Type = HighCTokenLibrary.PREVIOUS; break;
                    case HighCTokenLibrary.PRIVATE:
                        token.Type = HighCTokenLibrary.PRIVATE; break;
                    case HighCTokenLibrary.PUBLIC:
                        token.Type = HighCTokenLibrary.PUBLIC; break;
                    case HighCTokenLibrary.PURE:
                        token.Type = HighCTokenLibrary.PURE; break;
                    case HighCTokenLibrary.RECURSIVE:
                        token.Type = HighCTokenLibrary.RECURSIVE; break;
                    case HighCTokenLibrary.REMOVE:
                        token.Type = HighCTokenLibrary.REMOVE; break;
                    case HighCTokenLibrary.RETURN:
                        token.Type = HighCTokenLibrary.RETURN; break;
                    case HighCTokenLibrary.RETYPE:
                        token.Type = HighCTokenLibrary.RETYPE; break;
                    case HighCTokenLibrary.SCALAR:
                        token.Type = HighCTokenLibrary.SCALAR; break;
                    case HighCTokenLibrary.SET:
                        token.Type = HighCTokenLibrary.SET; break;
                    case HighCTokenLibrary.STOP:
                        token.Type = HighCTokenLibrary.STOP; break;
                    case HighCTokenLibrary.STRING:
                        token.Type = HighCTokenLibrary.STRING; break;
                    case HighCTokenLibrary.TRUE:
                        token.Type = HighCTokenLibrary.TRUE; break;
                    case HighCTokenLibrary.UNTIL:
                        token.Type = HighCTokenLibrary.UNTIL; break;
                    case HighCTokenLibrary.VOID:
                        token.Type = HighCTokenLibrary.VOID; break;
                    default:
                        Char firstLetter = token.Text[0];

                        switch(firstLetter)
                        {
                            case '\"':
                                token.Type = HighCTokenLibrary.STRING_LITERAL; break;
                            case '/':
                                token.Type = HighCTokenLibrary.COMMENT; break;
                            case '?':
                                token.Type = HighCTokenLibrary.IDENTIFIER; break;
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                            case '.':
                                if (token.Text.Contains("."))
                                {
                                    token.Type = HighCTokenLibrary.FLOAT_LITERAL;
                                }
                                else
                                {
                                    token.Type = HighCTokenLibrary.INTEGER_LITERAL;
                                }
                                break;
                            default:
                                token.Type = HighCTokenLibrary.IDENTIFIER; break;
                        }
                        break;
                }
                previousToken = token;
            }
        }
    }
}
