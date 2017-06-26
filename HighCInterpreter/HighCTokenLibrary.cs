using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighCInterpreterCore
{
    public static class HighCTokenLibrary
    {
        //Operators
        public const String UNDERSCORE = "_";
        public const String MINUS_SIGN = "-";
        public const String COMMA = ",";
        public const String COLON = ":";
        public const String PERIOD = ".";
        public const String ELLIPSES = "...";
        public const String DOUBLE_QUOTE = "\"";
        public const String LEFT_PARENTHESIS = "(";
        public const String RIGHT_PARENTHESIS = ")";
        public const String LEFT_SQUARE_BRACKET = "[";
        public const String RIGHT_SQUARE_BRACKET = "]";
        public const String LEFT_CURLY_BRACKET = "{";
        public const String RIGHT_CURLY_BRACKET = "}";
        public const String AT_SIGN = "@";
        public const String ASTERICK = "*";
        public const String SLASH = "/";
        public const String AMPERSAND = "&";
        public const String POUND_SIGN = "#";
        public const String PERCENT_SIGN = "%";
        public const String CARET = "^";
        public const String PLUS_SIGN = "+";
        public const String LESS_THAN = "<";
        public const String GREATER_THAN = ">";
        public const String LESS_THAN_EQUAL = "<=";
        public const String GREATER_THAN_EQUAL = ">=";
        public const String EQUAL = "=";
        public const String ARROW = "=>";
        public const String VERTICAL_BAR = "|";
        public const String TILDE = "~";
        public const String NOT_EQUAL = "~=";
        public const String DOLLAR_SIGN = "$";
        //Keywords
        public const String ABSTRACT = "abstract";
        public const String ADD_TO = "Addto";
        public const String APPEND = "append";
        public const String ARRAY = "Array";
        public const String CALL = "call";
        public const String CHOICE = "choice";
        public const String CLASS = "class";
        public const String CREATE = "create";
        public const String CONSTANT = "constant";
        public const String DISCRETE = "discrete";
        public const String E = "e";
        public const String UPPERCASE_E = "E";
        public const String ELSE = "else";
        public const String ELSE_IF = "elseif";
        public const String END_OF_LINE = "endl";
        public const String ENUMERATION = "enum";
        public const String FINAL = "final";
        public const String FOR = "for";
        public const String FOREACH_ARRAY = "for[]";
        public const String FOREACH_LIST = "for@";
        public const String FUNCTION = "func";
        public const String GLOBAL = "global";
        public const String IF = "if";
        public const String IN = "in";
        public const String IN_OUT = "inout";
        public const String IN_REVERSE = "inrev";
        public const String INSERT = "insert";
        public const String INSTANCE_OF = "instof";
        public const String LAST = "last";
        public const String LENGTH = "Length";
        public const String LOOP = "loop";
        public const String MAIN = "main";
        public const String MATCH = "Match";
        public const String METHOD = "method";
        public const String MULTIPLE = "multi";
        public const String NEXT = "Next";
        public const String ON = "on";
        public const String OTHER = "other";
        public const String OUT = "out";
        public const String PREVIOUS = "Prev";
        public const String PRIVATE = "private";
        public const String PUBLIC = "public";
        public const String PURE = "pure";
        public const String RECURSIVE = "recurs";
        public const String REMOVE = "remove";
        public const String RETURN = "return";
        public const String RETYPE = "retype";
        public const String SCALAR = "scalar";
        public const String SET = "set";
        public const String STOP = "stop";
        public const String UNTIL = "until";
        public const String VOID = "void";
        //Primitive Constants
        public const String FALSE = "false";
        public const String TRUE = "true";
        //Data Types
        public const String BOOLEAN = "BOOL";
        public const String FLOAT = "FLOAT";
        public const String INTEGER = "INT";
        public const String CHARACTER = "CHAR";
        public const String STRING = "STRING";
        //Other
        public const String IDENTIFIER = "Identifier";
        public const String COMMENT = "Comment";
        public const String STRING_LITERAL = "String Literal";
        public const String INTEGER_LITERAL = "Integer Literal";
        public const String FLOAT_LITERAL = "Float Literal";
        public const String LIST = "List";

        //public const String NUMERIC_LITERAL = "Numeric Literal";
        public const String EXPONENT = "Exponent";

        public static List<String> getKeywords()
        {
            List<String> keywords = new List<String>();

            //Keywords
            keywords.Add( ABSTRACT );
            keywords.Add( ADD_TO );
            keywords.Add( APPEND );
            keywords.Add( ARRAY );
            keywords.Add( CALL );
            keywords.Add( CHARACTER );
            keywords.Add( CHOICE );
            keywords.Add( CLASS );
            keywords.Add( CONSTANT );
            keywords.Add( CREATE );
            keywords.Add( DISCRETE );
            keywords.Add( E );
            keywords.Add( UPPERCASE_E );
            keywords.Add( ELSE );
            keywords.Add( ELSE_IF );
            keywords.Add( END_OF_LINE );
            keywords.Add( ENUMERATION );
            keywords.Add( FINAL );
            keywords.Add( FOR );
            keywords.Add( FOREACH_ARRAY );
            keywords.Add( FOREACH_LIST );
            keywords.Add( FUNCTION );
            keywords.Add( GLOBAL );
            keywords.Add( IF );
            keywords.Add( IN );
            keywords.Add( IN_OUT );
            keywords.Add( IN_REVERSE );
            keywords.Add( INSERT );
            keywords.Add( INSTANCE_OF );
            keywords.Add( LAST );
            keywords.Add( LENGTH );
            keywords.Add( LOOP );
            keywords.Add( MAIN );
            keywords.Add( MATCH );
            keywords.Add( METHOD );
            keywords.Add( MULTIPLE );
            keywords.Add( NEXT );
            keywords.Add( ON );
            keywords.Add( OTHER );
            keywords.Add( OUT );
            keywords.Add( PREVIOUS );
            keywords.Add( PRIVATE );
            keywords.Add( PUBLIC );
            keywords.Add( PURE );
            keywords.Add( RECURSIVE );
            keywords.Add( REMOVE );
            keywords.Add( RETURN );
            keywords.Add( RETYPE );
            keywords.Add( SCALAR );
            keywords.Add( SET );
            keywords.Add( STOP );
            keywords.Add( UNTIL );
            keywords.Add( VOID );
            //Primitive Constants
            keywords.Add( FALSE );
            keywords.Add( TRUE );
            //Data Types
            keywords.Add( BOOLEAN );
            keywords.Add( FLOAT );
            keywords.Add( INTEGER );
            keywords.Add( STRING );
            
            return keywords;
        }
    }
}
