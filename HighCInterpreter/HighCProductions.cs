using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighCInterpreterCore
{
    class HighCParser
    {
        private List<HighCToken> tokenList;
        private int currentToken;
        private  String debugLog;
        private String consoleText;
        private Boolean fullDebug = false;
        private List<String> debugList = new List<String>();
        private Boolean stopProgram = false;

        public HighCParser(List<HighCToken> newTokens)
        {
            tokenList = newTokens;
            currentToken = 0;
        }

        public Boolean parse() { return HC_program(); }

        public String getDebugLog()
        {
            foreach(String entry in debugList)
            {
                debugLog += entry;
            }
            return debugLog;
        }

        public String getConsoleText() { return consoleText; }

        private void addDebugInfo(String newEntry)
        {
            if(debugList.Contains(newEntry)==false)
            {
                debugList.Add(newEntry);
            }
        }

        private Boolean matchTerminal(String token, Boolean outputToken=false)
        {
            if (currentToken == tokenList.Count)
            {
                addDebugInfo("While parsing the tokens, no more valid tokens are available." + Environment.NewLine);
                return false;
            }
            Boolean matchStatus = tokenList[currentToken].Type == token;
            
            if(matchStatus==true)
            {
                Console.WriteLine("Matched Token: " + tokenList[currentToken].Text + " to "+token);
            }
            else if (outputToken == true)
            {
                addDebugInfo("(L" + tokenList[currentToken].Line + ", C" + tokenList[currentToken].Column + "): Expected to find: \"" + token + "\"" + Environment.NewLine);
                Console.WriteLine("  Current Token: " + tokenList[currentToken].Text + " " + tokenList[currentToken].Type);
            }
            currentToken++;
            return matchStatus;
        }

        private Boolean skipBlock()
        {
            int storeToken = currentToken;
            int bracketsToMatch;
            
            if (tokenList[storeToken].Type==HighCTokenLibrary.LEFT_CURLY_BRACKET)
            {
                Console.WriteLine("Skipping Block...");
                Console.WriteLine("Skipping: " + tokenList[storeToken].Text);
                bracketsToMatch = 1;
                storeToken++;
                while(storeToken<tokenList.Count && bracketsToMatch>0)
                {
                    Console.WriteLine("Skipping: " + tokenList[storeToken].Text);
                    if (tokenList[storeToken].Type == HighCTokenLibrary.LEFT_CURLY_BRACKET)
                    {
                        bracketsToMatch++;
                    }
                    else if (tokenList[storeToken].Type == HighCTokenLibrary.RIGHT_CURLY_BRACKET)
                    {
                        bracketsToMatch--;
                    }

                    storeToken++;
                }

                if(bracketsToMatch==0)
                {
                    currentToken = storeToken;
                    Console.Write("Current Token: " + currentToken+tokenList[currentToken].Type+Environment.NewLine);
                    return true;
                }
            }

            return false;
        }

        private Boolean _______________Productions_______________() { return false; }

        private Boolean HC_add_op(out String opType)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_add_op"); }
            int storeToken = currentToken;
            
            if (matchTerminal(HighCTokenLibrary.MINUS_SIGN))
            {
                opType = HighCTokenLibrary.MINUS_SIGN;
                Console.WriteLine(currentToken + " <add op> -> " + HighCTokenLibrary.MINUS_SIGN);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.PLUS_SIGN))
            {
                opType = HighCTokenLibrary.PLUS_SIGN;
                Console.WriteLine(currentToken + " <add op> -> " + HighCTokenLibrary.PLUS_SIGN);
                return true;
            }

            opType = "";
            return false;
        }

        private Boolean HC_arithmetic_expression(ref Double value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_expression"); }
            value = 0.0;
            Boolean integerOnly;
            Boolean integerOnly2;

            if (HC_arithmetic_term(ref value, out integerOnly) &&
               HC_arithmetic_expression_helper(ref value, out integerOnly2))
            {
                Console.WriteLine(currentToken + " <arithmetic expression> -> <arithmetic term><arithmetic expression with integer tracking>"+" -> "+value);
                return true;
            }

            return false;
        }

        private Boolean HC_arithmetic_expression_with_integer_tracking(ref Double value, out Boolean integerOnly)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_expression_with_integer_tracking"+" -> "+value); }
            Boolean integerOnly2;

            if (HC_arithmetic_term(ref value, out integerOnly) &&
               HC_arithmetic_expression_helper(ref value, out integerOnly2))
            {
                Console.WriteLine(currentToken + " <arithmetic expression with integer tracking>" + " -> " + value);
                integerOnly = integerOnly && integerOnly2;
                return true;
            }

            return false;
        }

        private Boolean HC_arithmetic_expression_helper(ref Double value, out Boolean integerOnly)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_expression_helper"); }
            int storeToken = currentToken;
            Double term1 = 0.0;
            String addOp;
            Boolean integerOnly2;

            if (HC_add_op(out addOp) &&
                HC_arithmetic_term(ref term1, out integerOnly))
            {
                if (addOp == HighCTokenLibrary.PLUS_SIGN)
                {
                    value = value + term1;
                    Console.WriteLine(currentToken + " <arithmetic expression'> -> + <arithmetic term>" + " -> " + value);
                }
                else if (addOp == HighCTokenLibrary.MINUS_SIGN)
                {
                    value = value + term1;
                    Console.WriteLine(currentToken + " <arithmetic expression'> -> - <arithmetic term>" + " -> " + value);
                }

                if (HC_arithmetic_expression_helper(ref value, out integerOnly2))
                {
                    integerOnly = integerOnly && integerOnly2;
                    return true;
                }
                else { return false; }
            }

            Console.WriteLine(currentToken + " <arithmetic expression'> -> null");
            integerOnly = true;
            currentToken = storeToken;
            return true;
        }

        private Boolean HC_arithmetic_factor(ref Double value, out Boolean integerOnly)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_factor"); }
            integerOnly = true;
            Boolean integerOnly2 = true;

            if (HC_arithmetic_primary(ref value, out integerOnly) &&
               HC_arithmetic_factor_helper(ref value, out integerOnly2))
            {
                Console.WriteLine(currentToken + " <arithmetic factor>-><arithmetic primary><arithmetic factor'>" + " -> " + value);
                integerOnly = integerOnly && integerOnly2;
                return true;
            }

            return false;
        }

        private Boolean HC_arithmetic_factor_helper(ref Double value, out Boolean integerOnly)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_factor_helper"); }
            int storeToken = currentToken;
            integerOnly = true;
            Boolean integerOnly2 = true;
            Double term1 = 0.0;

            if (matchTerminal(HighCTokenLibrary.CARET) &&
                HC_arithmetic_primary(ref term1, out integerOnly))
            {
                value = Math.Pow(value, term1);

                Console.WriteLine(currentToken + " <arithmetic factor'> -> ^ <arithmetic primary>" + " -> " + value);

                if (HC_arithmetic_factor_helper(ref value, out integerOnly2))
                {
                    integerOnly = integerOnly && integerOnly2;
                    return true;
                }
            }

            Console.WriteLine(currentToken + " <arithmetic factor'> -> null");
            currentToken = storeToken;
            return true;
        }

        private Boolean HC_arithmetic_primary(ref Double value, out Boolean integerOnly)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_primary"); }
            int storeToken = currentToken;
            String addOp;

            if (HC_add_op(out addOp) &&
                HC_arithmetic_primary(ref value, out integerOnly))
            {
                if (addOp == HighCTokenLibrary.MINUS_SIGN)
                {
                    value = value * -1;
                    Console.WriteLine(currentToken + " <arithmetic primary> -> -<arithmetic primary>" + " -> " + value);
                }
                else
                {
                    Console.WriteLine(currentToken + " <arithmetic primary> -> +<arithmetic primary>" + " -> " + value);
                }

                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS) &&
               HC_arithmetic_expression_with_integer_tracking(ref value, out integerOnly) &&
               matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS))
            {
                Console.WriteLine(currentToken + " <arithmetic primary> -> (<arithmetic expression with integer tracking>)" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_integer_constant(ref value))
            {
                integerOnly = true;

                Console.WriteLine(currentToken + " <arithmetic primary> -> <integer constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_float_constant(ref value))
            {
                integerOnly = false;

                Console.WriteLine(currentToken + " <arithmetic primary> -> <float constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_integer_variable())
            {
                integerOnly = true;
                Console.WriteLine(currentToken + " <arithmetic primary> -> <integer variable>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_float_variable())
            {
                integerOnly = false;
                Console.WriteLine(currentToken + " <arithmetic primary> -> <float variable>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_integer_function_call())
            {
                integerOnly = true;
                Console.WriteLine(currentToken + " <arithmetic primary> -> <integer function call>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_float_function_call())
            {
                integerOnly = false;
                Console.WriteLine(currentToken + " <arithmetic primary> -> <float function call>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.LENGTH) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS) &&
               HC_list_expression() &&
               matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS))
            {
                integerOnly = true;
                Console.WriteLine(currentToken + " <arithmetic primary> -> Length(<list expression>)" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            String localStringBuffer;
            if (matchTerminal(HighCTokenLibrary.LENGTH) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS) &&
               HC_string_expression(out localStringBuffer) &&
               matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS))
            {
                value = localStringBuffer.Length;
                integerOnly = true;
                Console.WriteLine(currentToken + " <arithmetic primary> -> Length(<string expression>)" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            String localStringBuffer2;
            if (matchTerminal(HighCTokenLibrary.MATCH) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS) &&
                HC_string_expression(out localStringBuffer) &&
                matchTerminal(HighCTokenLibrary.COMMA, true) &&
                HC_string_expression(out localStringBuffer2) &&
                matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS))
            {
                if (localStringBuffer2.Contains(localStringBuffer))
                {
                    value = localStringBuffer2.IndexOf(localStringBuffer);
                }
                else
                {
                    value = 0;
                }
                integerOnly = true;
                Console.WriteLine(currentToken + " <arithmetic primary> -> Match(<string expression>,<string expression>)" + " -> " + value);
                return true;
            }

            integerOnly = false;
            return false;
        }

        private Boolean HC_arithmetic_term(ref Double value, out Boolean integerOnly)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_term"); }
            integerOnly = true;
            Boolean integerOnly2 = true;

            if (HC_arithmetic_factor(ref value, out integerOnly) &&
               HC_arithmetic_term_helper(ref value, out integerOnly2))
            {
                Console.WriteLine(currentToken + " <arithmetic term>-><arithmetic factor><arithmetic term'>" + " -> " + value);
                integerOnly = integerOnly && integerOnly2;
                return true;
            }

            return false;
        }
        
        private Boolean HC_arithmetic_term_helper(ref Double value, out Boolean integerOnly)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_term_helper"); }
            int storeToken = currentToken;
            integerOnly = true;
            Boolean integerOnly2 = true;
            Double term1 = 0.0;
            String multOp;

            if (HC_mult_op(out multOp) &&
                HC_arithmetic_factor(ref term1, out integerOnly))
            {
                if (multOp == HighCTokenLibrary.ASTERICK)
                {
                    value = value * term1;
                    Console.WriteLine(currentToken + " <arithmetic term'> -> * <arithmetic term'>" + " -> " + value);
                }
                else if (multOp == HighCTokenLibrary.SLASH)
                {
                    value = value / term1;
                    Console.WriteLine(currentToken + " <arithmetic term'> -> / <arithmetic term'>" + " -> " + value);
                }
                else if (multOp == HighCTokenLibrary.PERCENT_SIGN)
                {
                    if (integerOnly == true && (value - (int)value < Double.Epsilon))
                    {
                        value = value % term1;
                        Console.WriteLine(currentToken + " <arithmetic term'> -> % <arithmetic term'>" + " -> " + value);
                    }
                    else
                    {
                        addDebugInfo("While performing a modulus operation at (L" + tokenList[currentToken - 1].Line + ", C" + tokenList[currentToken].Column + "): \"" + tokenList[currentToken].Type + "\" one or more operands were not integer values \"" + Environment.NewLine);
                        return false;
                    }
                }

                if (HC_arithmetic_term_helper(ref value, out integerOnly2))
                {
                    integerOnly = integerOnly && integerOnly2;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            Console.WriteLine(currentToken + " <arithmetic term'> -> null");
            currentToken = storeToken;
            return true;
        }
        
        private Boolean HC_block()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_block"); }
            if (matchTerminal(HighCTokenLibrary.LEFT_CURLY_BRACKET) == false)
            {
                addDebugInfo("(L" + tokenList[currentToken - 1].Line + ", C" + tokenList[currentToken - 1].Column + ") " + ": expected to find a \"" + HighCTokenLibrary.LEFT_CURLY_BRACKET + "\"" + Environment.NewLine);
                return false;
            }

            while (HC_declaration())
            {
                Console.WriteLine(currentToken + " <block> -> <declaration>");
            }

            while (HC_statement())
            {
                Console.WriteLine(currentToken + " <block> -> <statement>");
            }

            if (matchTerminal(HighCTokenLibrary.RIGHT_CURLY_BRACKET))
            {
                Console.WriteLine(currentToken + " <block>");
                return true;
            }
            else if (stopProgram != true)
            {
                addDebugInfo("(L" + tokenList[currentToken - 1].Line + ", C" + tokenList[currentToken - 1].Column + ") " + ": expected to find a \"" + HighCTokenLibrary.RIGHT_CURLY_BRACKET + "\"" + Environment.NewLine);
            }
            return false;
        }
        
        private Boolean HC_boolean_constant(ref Boolean value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_boolean_constant"); }
            int storeToken = currentToken;

            if (matchTerminal(HighCTokenLibrary.TRUE))
            {
                value = true;
                Console.WriteLine(currentToken + " <boolean constant> -> " + tokenList[currentToken - 1].Text);
                return true;
            }

            currentToken = storeToken;
            if(matchTerminal(HighCTokenLibrary.FALSE))
            {
                value = false;
                Console.WriteLine(currentToken + " <boolean constant> -> " + tokenList[currentToken - 1].Text);
                return true;
            }

            return false;
        }

        private Boolean HC_boolean_expression(ref Boolean value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_boolean_expression"); }
            value = false;

            if (HC_boolean_term(ref value) &&
               HC_boolean_expression_helper(ref value))
            {
                Console.WriteLine(currentToken + " <boolean expression> -> <boolean term><boolean expression helper>" + " -> " + value);

                int storeToken = currentToken;
                String opType;
                Boolean boolTerm2 = false;
                if (HC_equality_op(out opType) &&
                    HC_boolean_expression(ref boolTerm2))
                {
                    if (opType == HighCTokenLibrary.EQUAL) { value = value == boolTerm2; }
                    else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = value != boolTerm2; }

                    Console.WriteLine(currentToken + " <relational expression> -> <boolean expression><equality op><boolean expression>" + " -> " + value);

                    return true;
                }
                else
                {
                    currentToken = storeToken;
                }

                return true;
            }

            return false;
        }

        private Boolean HC_boolean_expression_helper(ref Boolean value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_boolean_expression_helper"); }
            int storeToken = currentToken;
            Boolean term1 = false;

            if (matchTerminal(HighCTokenLibrary.VERTICAL_BAR) &&
                HC_boolean_term(ref term1))
            {
               
                value = value || term1;
                Console.WriteLine(currentToken + " <boolean expression'> -> | <arithmetic term>" + " -> " + value);

                if (HC_boolean_expression_helper(ref value))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            Console.WriteLine(currentToken + " <boolean expression'> -> null");
            currentToken = storeToken;
            return true;
        }


        private Boolean HC_boolean_factor(ref Boolean value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_boolean_factor"); }
            int storeToken = currentToken;

            /*
             ~ <bool – factor>
            ( <bool – expr> )
            <bool – variable>
            <bool – constant>
            <rel – expr>
            <bool – func call>
             */
            
            if (matchTerminal(HighCTokenLibrary.TILDE) &&
                HC_boolean_factor(ref value))
            {
                value = !value;
                Console.WriteLine(currentToken + " <boolean factor> -> ~<boolean factor>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS) &&
               HC_boolean_expression(ref value) &&
               matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS))
            {
                Console.WriteLine(currentToken + " <boolean factor> -> (<boolean expression>)" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_boolean_constant(ref value))
            {
                Console.WriteLine(currentToken + " <boolean factor> -> <boolean constant>" + " -> " + value);
                return true;
            }
            
            currentToken = storeToken;
            if (HC_boolean_variable(ref value))
            {
                Console.WriteLine(currentToken + " <boolean factor> -> <boolean variable>" + " -> " + value);
                return true;
            }
            
            currentToken = storeToken;
            if (HC_boolean_function_call(ref value))
            {
                Console.WriteLine(currentToken + " <boolean factor> -> <boolean function call>" + " -> " + value);
                return true;
            }
            
            currentToken = storeToken;
            if (HC_relational_expression(ref value))
            {
                Console.WriteLine(currentToken + " <boolean factor> -> <relational expression>" + " -> " + value);
                return true;
            }
            
            return false;
        }
        
        private Boolean HC_boolean_term(ref Boolean value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_boolean_term"); }

            if (HC_boolean_factor(ref value) &&
                HC_boolean_term_helper(ref value))
            {
                Console.WriteLine(currentToken + " <boolean term> -> <boolean factor><boolean term'>" + " -> " + value);
                return true;
            }

            return false;
        }


        private Boolean HC_boolean_term_helper(ref Boolean value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_boolean_term_helper"); }
            int storeToken = currentToken;
            Boolean term1 = false;

            if (matchTerminal(HighCTokenLibrary.AMPERSAND) &&
                HC_boolean_factor(ref term1))
            {
                value = value && term1;
                Console.WriteLine(currentToken + " <boolean term'> -> & <boolean term'>" + " -> " + value);

                if (HC_boolean_term_helper(ref value))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            Console.WriteLine(currentToken + " <boolean term'> -> null");
            currentToken = storeToken;
            return true;
        }


        private Boolean HC_character_constant(out String stringBuffer)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_character_constant"); }
            stringBuffer = "";
            if (matchTerminal(HighCTokenLibrary.STRING_LITERAL) && tokenList[currentToken - 1].Text.Length == 3)
            {
                stringBuffer = tokenList[currentToken - 1].Text.Substring(1, tokenList[currentToken - 1].Text.Length - 2);
                Console.WriteLine(currentToken + " <char constant> -> " + tokenList[currentToken - 1].Text);
                return true;
            }

            return false;
        }

        private Boolean HC_character_expression(out String stringBuffer)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_character_expression"); }
            /*
                <string – expr> $ <int – expr>
                <char – variable>
                <char – constant>
                Next ( <char – expr> )
                Prev ( <char – expr> )
                <char – func cal>
             */
            stringBuffer = "";
            int storeToken = currentToken;

            //<string – expr> $ <int – expr> handled inside string expression

            if(HC_character_variable(out stringBuffer))
            {
                Console.WriteLine(currentToken + " <character expression> -> <char variable>" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (HC_character_constant(out stringBuffer))
            {
                Console.WriteLine(currentToken + " <character expression> -> <char constant>" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.NEXT) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS) &&
                HC_character_expression(out stringBuffer) &&
                matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS))
            {
                Console.WriteLine(currentToken + " <character expression> -> Next(<char expression>)" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.PREVIOUS) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS) &&
                HC_character_expression(out stringBuffer) &&
                matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS))
            {
                Console.WriteLine(currentToken + " <character expression> -> Prev(<char expression>)" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (HC_character_function_call(out stringBuffer))
            {
                Console.WriteLine(currentToken + " <character expression> -> <char constant>" + " -> " + stringBuffer);
                return true;
            }

            return false;
        }
        
        private Boolean HC_control_statement()
        {
            int storeToken = currentToken;

            /*
                stop
                <void call>
                <return>
                <if>
                <choice>
                <loop>
                <iterator>
             */
             
            if(matchTerminal(HighCTokenLibrary.STOP))
            {
                Console.WriteLine(currentToken + " <control statement> -> stop");
                stopProgram = true;
                return true;
            }

            currentToken = storeToken;
            if (HC_void_call())
            {
                Console.WriteLine(currentToken + " <control statement> -> <void call>");
                return true;
            }

            currentToken = storeToken;
            if (HC_return())
            {
                Console.WriteLine(currentToken + " <control statement> -> <return>");
                return true;
            }

            currentToken = storeToken;
            if (HC_if())
            {
                Console.WriteLine(currentToken + " <control statement> -> <if>");
                return true;
            }

            currentToken = storeToken;
            if (HC_choice())
            {
                Console.WriteLine(currentToken + " <control statement> -> <choice>");
                return true;
            }

            currentToken = storeToken;
            if (HC_loop())
            {
                Console.WriteLine(currentToken + " <control statement> -> <loop>");
                return true;
            }

            currentToken = storeToken;
            if (HC_iterator())
            {
                Console.WriteLine(currentToken + " <control statement> -> <iterator>");
                return true;
            }

            return false;
        }
        
        private Boolean HC_else_if(ref Boolean value)
        {
            /*
            elseif ( <bool – expr> ) <block>
            else if ( <bool – expr> ) <block>
             */
            int storeToken = currentToken;
            Boolean elseifFound = false;
            String elseifStyle = "";

            if(matchTerminal(HighCTokenLibrary.ELSE) &&
               matchTerminal(HighCTokenLibrary.IF))
            {
                elseifFound = true;
                elseifStyle = HighCTokenLibrary.ELSE+" "+ HighCTokenLibrary.IF;
            }

            if (elseifFound == false)
            {
                currentToken = storeToken;
                if (matchTerminal(HighCTokenLibrary.ELSE_IF))
                {
                    elseifFound = true;
                    elseifStyle = HighCTokenLibrary.ELSE_IF;
                }
            }

            if(elseifFound==true)
            {
                if(matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS,true))
                {
                    Boolean boolTerm1 = false;
                    if(HC_boolean_expression(ref boolTerm1))
                    {
                        if(matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS,true))
                        {
                            if(boolTerm1==true)
                            {
                                if(HC_block())
                                {
                                    Console.WriteLine(currentToken + " <else if> -> "+elseifStyle+" ( <boolean expression> ) <block> -> branch taken");
                                    return true;
                                }
                            }
                            else
                            {
                                skipBlock();
                                Console.WriteLine(currentToken + " <else if> -> " + elseifStyle + " ( <boolean expression> ) <block> -> no branch taken");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        addDebugInfo("(L" + tokenList[currentToken].Line + ", C" + tokenList[currentToken].Column + ") " + HighCTokenLibrary.IF + ": A boolean value was expected inside the paranthesis." + Environment.NewLine);
                    }
                }
            }

            return false;
        }

        private Boolean HC_equality_op(out String opType)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_equality_op"); }
            /*
            =
            ~=
             */

            opType = "";
            int storeToken = currentToken;
            
            if(matchTerminal(HighCTokenLibrary.EQUAL))
            {
                opType = tokenList[currentToken - 1].Type;
                Console.WriteLine(currentToken + " <equality op> -> " + HighCTokenLibrary.EQUAL);
                return true;
            }

            currentToken=storeToken;
            if (matchTerminal(HighCTokenLibrary.NOT_EQUAL))
            {
                opType = tokenList[currentToken - 1].Type;
                Console.WriteLine(currentToken + " <equality op> -> " + HighCTokenLibrary.EQUAL);
                return true;
            }

            return false;
        }

        private Boolean HC_float_constant(ref Double value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_float_constant"); }
            if (matchTerminal(HighCTokenLibrary.FLOAT_LITERAL))
            {
                Double.TryParse(tokenList[currentToken - 1].Text, out value);

                int storeToken = currentToken;
                if(matchTerminal(HighCTokenLibrary.EXPONENT,true))
                {
                    if(matchTerminal(HighCTokenLibrary.INTEGER_LITERAL))
                    {
                        int shift;
                        int.TryParse(tokenList[currentToken - 1].Text, out shift);
                        
                        while(shift>0)
                        {
                            value = value * 10;
                            shift--;
                        }
                        Console.WriteLine(currentToken + " <float constant> -> " + tokenList[currentToken - 3].Text+HighCTokenLibrary.EXPONENT+tokenList[currentToken-1].Text + " -> " +value);
                        return true;
                    }
                    else
                    {
                        addDebugInfo("(L" + tokenList[currentToken].Line + ", C" + tokenList[currentToken].Column + ") " + HighCTokenLibrary.EXPONENT + ": An integer value was expected after the exponent." + Environment.NewLine);
                        currentToken = storeToken;
                    }
                }
                else
                {
                    currentToken = storeToken;
                }
                Console.WriteLine(currentToken + " <float constant> -> "+tokenList[currentToken-1].Text);
                return true;
            }
            return false;
        }
        
        private Boolean HC_if()
        {
            /*
             * if ( <bool – expr> ) <block> <else – if>* else <block>
             */

            int storeToken = currentToken;

            if(matchTerminal(HighCTokenLibrary.IF) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS,true))
            {
                Boolean boolTerm1=false;
                if(HC_boolean_expression(ref boolTerm1) &&
                    matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS,true))
                {
                    if (boolTerm1==true)
                    {
                        if (HC_block())
                        {
                            Console.WriteLine(currentToken + " <if> -> if ( <boolean expression> ) <block> <else_if>* else <block> -> if branch");
                            return true;
                        }
                    }
                    else
                    {
                        if(skipBlock()==false)
                        {
                            //error
                            return false;
                        }

                        storeToken = currentToken;
                        while(HC_else_if(ref boolTerm1))
                        {
                            if(boolTerm1==true)
                            {
                                if (HC_block())
                                {
                                    Console.WriteLine(currentToken + " <if> -> if ( <boolean expression> ) <block> <else_if>* else <block> -> else if branch");
                                    return true;
                                }
                            }
                            else if (skipBlock() == false)
                            {
                                //error
                            }
                            storeToken = currentToken;
                        }
                        currentToken = storeToken;
                        if(matchTerminal(HighCTokenLibrary.ELSE))
                        {
                            if (HC_block())
                            {
                                Console.WriteLine(currentToken + " <if> -> if ( <boolean expression> ) <block> <else_if>* else <block> -> else branch");
                                return true;
                            }
                        }
                        currentToken = storeToken;
                    }

                    Console.WriteLine(currentToken + " <if> -> if ( <boolean expression> ) <block> <else_if>* else <block> -> no branch");

                    return true;
                }
                else
                {
                    addDebugInfo("(L" + tokenList[currentToken].Line + ", C" + tokenList[currentToken].Column + ") " + HighCTokenLibrary.IF + ": A boolean value was expected inside the paranthesis." + Environment.NewLine);
                }
            }

            return false;
        }

        private Boolean HC_integer_constant(ref Double value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_integer_constant"); }
            if (matchTerminal(HighCTokenLibrary.INTEGER_LITERAL))
            {
                Double.TryParse(tokenList[currentToken - 1].Text, out value);
                Console.WriteLine(currentToken + " <integer constant> -> " + tokenList[currentToken - 1].Text);
                return true;
            }

            return false;
        }

        private Boolean HC_integer_expression(out Int64 integerValue)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_integer_expression"); }
            integerValue = 0;
            Double term1 = 0;
            Boolean integerOnly;

            if (HC_arithmetic_expression_with_integer_tracking(ref term1, out integerOnly))
            {
                if (integerOnly == true)
                {
                    integerValue = (Int64)term1;
                    Console.WriteLine(currentToken + " <integer expression> -> arithmetic expression with integer tracking" + " -> " + integerValue);
                    return true;
                }
            }
            return false;
        }

        private Boolean HC_mult_op(out String type)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_mult_op"); }
            int storeToken = currentToken;

            if (matchTerminal(HighCTokenLibrary.ASTERICK))
            {
                type = HighCTokenLibrary.ASTERICK;
                Console.WriteLine(currentToken + " <mult op>" + " -> " + type);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.SLASH))
            {
                type = HighCTokenLibrary.SLASH;
                Console.WriteLine(currentToken + " <mult op>" + " -> " + type);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.PERCENT_SIGN))
            {
                type = HighCTokenLibrary.PERCENT_SIGN;
                Console.WriteLine(currentToken + " <mult op>" + " -> " + type);
                return true;
            }

            type = "";
            return false;
        }

        private Boolean HC_out_element(out String stringBuffer)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_out_element"); }
            int storeToken = currentToken;

            stringBuffer = "";
            Int64 term1;
            Int64 term2;
            Double term3 = 0;

            //Ensures Minimum Length
            if (HC_scalar_expression(out stringBuffer) &&
                matchTerminal(HighCTokenLibrary.COLON) &&
                HC_integer_expression(out term1))
            {
                if (stringBuffer.Length < term1)
                {
                    stringBuffer = stringBuffer.PadRight((int)term1);
                }
                Console.WriteLine(currentToken + " <out element> -> <scalar expression>:<integer expression>" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (HC_scalar_expression(out stringBuffer))
            {
                Console.WriteLine(currentToken + " <out element> -> <scalar expression>" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (HC_arithmetic_expression(ref term3) &&
                matchTerminal(HighCTokenLibrary.COLON) &&
                HC_integer_expression(out term1) &&
                matchTerminal(HighCTokenLibrary.PERIOD) &&
                HC_integer_expression(out term2))
            {
                Console.WriteLine(currentToken + " <out element> -> <arithmetic expression>:<integer expression>.<integer expression>" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.END_OF_LINE))
            {
                stringBuffer = Environment.NewLine;
                Console.WriteLine(currentToken + " <out element> -> endl" + " -> " + stringBuffer);
                return true;
            }

            return false;
        }

        private Boolean HC_output()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_output"); }
            int storeToken = currentToken;
            if (matchTerminal(HighCTokenLibrary.OUT) == false) { return false; }

            //List of Tokens
            Boolean atLeastOneFound = false;
            Boolean needAnotherOut = false;
            String stringBuffer="";
            String sBuffer1;
            while (HC_out_element(out sBuffer1))
            {
                storeToken = currentToken;
                stringBuffer += sBuffer1;
                atLeastOneFound = true;
                needAnotherOut = false;
                if (matchTerminal(HighCTokenLibrary.COMMA) == false) { break; }
                else
                {
                    needAnotherOut = true;
                }
            }
            
            currentToken = storeToken;

            if (needAnotherOut==true)
            {
                addDebugInfo("(L" + tokenList[currentToken].Line + ", C" + tokenList[currentToken].Column + ") "+HighCTokenLibrary.OUT+": another element was expected after the comma." + Environment.NewLine);
                return false;
            }

            if (atLeastOneFound == false)
            {
                addDebugInfo("(L" + tokenList[currentToken].Line + ", C" + tokenList[currentToken].Column + ") " + HighCTokenLibrary.OUT+": at least one element was expected." + Environment.NewLine);
                return false;
            }
            else
            {
                if(needAnotherOut==true)
                {
                    Console.WriteLine(currentToken + " <output> -> <out element>,...,<out element>" + " -> " + stringBuffer);
                }
                else
                {
                    Console.WriteLine(currentToken + " <output> -> <out element>" + " -> " + stringBuffer);
                }
                
                consoleText +=stringBuffer;
                return true;
            }
        }

        private Boolean HC_relational_expression(ref Boolean value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_relational_expression"); }
            
            /*
            <arith – expr> <rel – op> <arith – expr>
            <string – expr> <rel – op> <string – expr>
            <char – expr> <rel – op> <char – expr>
            <enum – expr> <rel – op> <enum – expr>

            <bool – expr> <eq – op> <bool – expr>
            <array – expr> <eq – op> <bool – expr>
            <list – expr> <eq – op> <list – expr>
            <object – expr> <eq – op> <object – expr>
            <object – expr> instof <class name>
             */
            int storeToken = currentToken;
            String opType;
            Double term1=0.0;
            Double term2=0.0;
            String stringTerm1 = "";
            String stringTerm2 = "";
            Boolean boolTerm2 = false;
            
            if (HC_arithmetic_expression(ref term1) &&
                HC_relational_op(out opType) &&
                HC_arithmetic_expression(ref term2))
            {

                if (opType==HighCTokenLibrary.EQUAL) { value = term1 == term2; }
                else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = term1 != term2; }
                else if (opType == HighCTokenLibrary.LESS_THAN) { value = term1 < term2; }
                else if (opType == HighCTokenLibrary.GREATER_THAN) { value = term1 > term2; }
                else if (opType == HighCTokenLibrary.LESS_THAN_EQUAL) { value = term1 <= term2; }
                else if (opType == HighCTokenLibrary.GREATER_THAN_EQUAL) { value = term1 >= term2; }

                Console.WriteLine(currentToken + " <relational expression> -> <arithmetic expression><relational op><arithmetic expression>" + " -> " + value);

                return true;
            }
            
            currentToken = storeToken;
            if (HC_string_expression(out stringTerm1) &&
                HC_relational_op(out opType) &&
                HC_string_expression(out stringTerm2))
            {
                int order = String.Compare(stringTerm1, stringTerm2);

                if (opType == HighCTokenLibrary.EQUAL) { value = stringTerm1 == stringTerm2; }
                else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = stringTerm1 != stringTerm2; }
                else if (opType == HighCTokenLibrary.LESS_THAN) { value = order < 0; }
                else if (opType == HighCTokenLibrary.GREATER_THAN) { value = order > 0; }
                else if (opType == HighCTokenLibrary.LESS_THAN_EQUAL) { value = order <= 0; }
                else if (opType == HighCTokenLibrary.GREATER_THAN_EQUAL) { value = order >= 0; }

                Console.WriteLine(currentToken + " <relational expression> -> <string expression><relational op><string expression>" + " -> " + value);

                return true;
            }

            currentToken = storeToken;
            if (HC_character_expression(out stringTerm1) &&
                HC_relational_op(out opType) &&
                HC_character_expression(out stringTerm2))
            {
                int order = String.Compare(stringTerm1, stringTerm2);

                if (opType == HighCTokenLibrary.EQUAL) { value = stringTerm1 == stringTerm2; }
                else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = stringTerm1 != stringTerm2; }
                else if (opType == HighCTokenLibrary.LESS_THAN) { value = order < 0; }
                else if (opType == HighCTokenLibrary.GREATER_THAN) { value = order > 0; }
                else if (opType == HighCTokenLibrary.LESS_THAN_EQUAL) { value = order <= 0; }
                else if (opType == HighCTokenLibrary.GREATER_THAN_EQUAL) { value = order >= 0; }

                Console.WriteLine(currentToken + " <relational expression> -> <character expression><relational op><character expression>" + " -> " + value);

                return true;
            }

            currentToken = storeToken;
            if (HC_enum_expression() &&
                HC_relational_op(out opType) &&
                HC_enum_expression())
            {
                /*
                if (opType == HighCTokenLibrary.EQUAL) { value = stringTerm1 == stringTerm2; }
                else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = stringTerm1 != stringTerm2; }
                else if (opType == HighCTokenLibrary.LESS_THAN) { value = order < 0; }
                else if (opType == HighCTokenLibrary.GREATER_THAN) { value = order > 0; }
                else if (opType == HighCTokenLibrary.LESS_THAN_EQUAL) { value = order <= 0; }
                else if (opType == HighCTokenLibrary.GREATER_THAN_EQUAL) { value = order >= 0; }
                */

                Console.WriteLine(currentToken + " <relational expression> -> <enum expression><relational op><enum expression>" + " -> " + value);

                return true;
            }

            //<boolean expression><equality op><boolean expression> moved to <boolean expression>
           

            currentToken = storeToken;
            if (HC_array_expression() &&
                HC_equality_op(out opType) &&
                HC_boolean_expression(ref boolTerm2))
            {
                /*
                if (opType == HighCTokenLibrary.EQUAL) { value = boolTerm1 == boolTerm2; }
                else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = boolTerm1 != boolTerm2; }
                */

                Console.WriteLine(currentToken + " <relational expression> -> <array expression><equality op><boolean expression>" + " -> " + value);

                return true;
            }

            currentToken = storeToken;
            if (HC_list_expression() &&
                HC_equality_op(out opType) &&
                HC_list_expression())
            {
                /*
                if (opType == HighCTokenLibrary.EQUAL) { value = boolTerm1 == boolTerm2; }
                else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = boolTerm1 != boolTerm2; }
                */

                Console.WriteLine(currentToken + " <relational expression> -> <list expression><equality op><list expression>" + " -> " + value);

                return true;
            }

            currentToken = storeToken;
            if (HC_object_expression() &&
                HC_equality_op(out opType) &&
                HC_object_expression())
            {
                /*
                if (opType == HighCTokenLibrary.EQUAL) { value = boolTerm1 == boolTerm2; }
                else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = boolTerm1 != boolTerm2; }
                */

                Console.WriteLine(currentToken + " <relational expression> -> <object expression><equality op><object expression>" + " -> " + value);

                return true;
            }

            currentToken = storeToken;
            if (HC_object_expression() &&
                matchTerminal(HighCTokenLibrary.INSTANCE_OF) &&
                HC_class_name())
            {
                /*
                if (opType == HighCTokenLibrary.EQUAL) { value = boolTerm1 == boolTerm2; }
                else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = boolTerm1 != boolTerm2; }
                */

                Console.WriteLine(currentToken + " <relational expression> -> <object expression> " + HighCTokenLibrary.INSTANCE_OF + " <class name>" + " -> " + value);

                return true;
            }

            return false;
        }

        private Boolean HC_relational_op(out String opType)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_relational_op"); }
            /*
            <eq – op>
            <
            >
            <=
            >=
             */
            opType = "";
            int storeToken = currentToken;

            if(HC_equality_op(out opType))
            {
                Console.WriteLine(currentToken + " <relational op> -> <equality op>" + " -> " + opType);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.LESS_THAN))
            {
                opType = HighCTokenLibrary.LESS_THAN;
                Console.WriteLine(currentToken + " <relational op> -> " + HighCTokenLibrary.LESS_THAN);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.GREATER_THAN))
            {
                opType = HighCTokenLibrary.GREATER_THAN;
                Console.WriteLine(currentToken + " <relational op> -> " + HighCTokenLibrary.GREATER_THAN);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.LESS_THAN_EQUAL))
            {
                opType = HighCTokenLibrary.LESS_THAN_EQUAL;
                Console.WriteLine(currentToken + " <relational op> -> " + HighCTokenLibrary.LESS_THAN_EQUAL);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.GREATER_THAN_EQUAL))
            {
                opType = HighCTokenLibrary.GREATER_THAN_EQUAL;
                Console.WriteLine(currentToken + " <relational op> -> " + HighCTokenLibrary.GREATER_THAN_EQUAL);
                return true;
            }

            return false;
        }

        private Boolean HC_program()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_program"); }
            while (HC_compiler_directive()) { }
            while (HC_user_constant()) { }
            while (HC_global_variable()) { }
            while (HC_class()) { }
            while (HC_function()) { }

            if (matchTerminal(HighCTokenLibrary.MAIN))
            {
                if (HC_block())
                {
                    Console.WriteLine(currentToken + " <program>");
                    return true;
                }
            }
            else
            {
                addDebugInfo("(L" + tokenList[currentToken - 1].Line + ", C" + tokenList[currentToken - 1].Column + ") " + ": expected to find a \"" + HighCTokenLibrary.MAIN + "\"" + Environment.NewLine);
            }
            
            if(stopProgram==true)
            {
                return true;
            }
            return false;
        }

        private Boolean HC_scalar_expression(out String stringBuffer)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_scalar_expression"); }
            int storeToken = currentToken;

            stringBuffer = "";
            Double term1 = 0.0;
            Boolean boolTerm = false;
            
            if (HC_arithmetic_expression(ref term1) == true)
            {
                stringBuffer = term1.ToString();
                Console.WriteLine(currentToken + " <scalar expression> -> <arithmetic expression>" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (HC_boolean_expression(ref boolTerm) == true)
            {
                stringBuffer = boolTerm.ToString();
                Console.WriteLine(currentToken + " <scalar expression> -> <boolean expression>" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (HC_string_expression(out stringBuffer) == true)
            {
                Console.WriteLine(currentToken + " <scalar expression> -> <string expression>" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (HC_character_expression(out stringBuffer) == true)
            {
                Console.WriteLine(currentToken + " <scalar expression> -> <character expression>" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (HC_enum_expression() == true)
            {
                Console.WriteLine(currentToken + " <scalar expression> -> <enum expression>" + " -> " + stringBuffer);
                return true;
            }

            return false;
        }

        private Boolean HC_statement()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_statement"); }
            int storeToken = currentToken;

            if(stopProgram==true)
            {
                return false;
            }

            if (HC_assignment() == true) { Console.WriteLine(currentToken + " <statement> -> <assignment>"); return true; }

            currentToken = storeToken;
            if (HC_type_assignment() == true) { Console.WriteLine(currentToken + " <statement> -> <type assignment>"); return true; }

            currentToken = storeToken;
            if (HC_list_command() == true) { Console.WriteLine(currentToken + " <statement> -> <list command>"); return true; }

            currentToken = storeToken;
            if (HC_input() == true) { Console.WriteLine(currentToken + " <statement> -> <input>"); return true; }

            currentToken = storeToken;
            if (HC_output() == true) { Console.WriteLine(currentToken + " <statement> -> <output>"); return true; }

            currentToken = storeToken;
            if (HC_control_statement() == true) { Console.WriteLine(currentToken + " <statement> -> <control statement>"); return true; }

            return false;
        }
        
        private Boolean HC_string_expression(out String stringBuffer)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_string_expression"); }
            stringBuffer = "";

            String sBuffer1="";
            String sBuffer2="";
            
            if (HC_string_term(out sBuffer1) &&
                HC_string_expression_helper(out sBuffer2))
            {
                stringBuffer = sBuffer1 + sBuffer2;
                Console.WriteLine(currentToken + " <string expression>" + " -> " + stringBuffer);
                return true;
            }

            return false;
        }

        private Boolean HC_string_expression_helper(out String stringBuffer)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_string_expression_helper"); }
            int storeToken = currentToken;

            stringBuffer = "";
            String sBuffer1 = "";
            String sBuffer2 = "";

            if (matchTerminal(HighCTokenLibrary.POUND_SIGN) &&
                HC_string_term(out sBuffer1) &&
                HC_string_expression_helper(out sBuffer2))
            {
                stringBuffer = sBuffer1 + sBuffer2;
                Console.WriteLine(currentToken + " <string expression'> -> #<string term><string expression'>" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            return true;
        }
        
        private Boolean HC_string_term(out String stringBuffer)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_string_term"); }
            stringBuffer = "";

            int storeToken = currentToken;
            Boolean derivationFound = false;
            
            if (HC_character_expression(out stringBuffer) == true)
            {
                Console.WriteLine(currentToken + " <string term> -> <character expression>" + " -> " + stringBuffer);
                derivationFound = true;
            }

            if (!derivationFound)
            {
                currentToken = storeToken;
                if (matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS) &&
                    HC_string_expression(out stringBuffer) &&
                    matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS))
                {
                    Console.WriteLine(currentToken + " <string term> -> (<string expression>)" + " -> " + stringBuffer);
                    derivationFound = true;
                }
            }

            if (!derivationFound)
            {
                currentToken = storeToken;
                if (HC_string_variable() == true)
                {
                    Console.WriteLine(currentToken + " <string term> -> <string variable>" + " -> " + stringBuffer);
                    derivationFound = true;
                }
            }

            if (!derivationFound)
            {
                currentToken = storeToken;
                if (HC_string_constant(out stringBuffer) == true)
                {
                    Console.WriteLine(currentToken + " <string term> -> <string constant>" + " -> " + stringBuffer);
                    derivationFound = true;
                }
            }

            if (!derivationFound)
            {
                currentToken = storeToken;
                if (HC_string_function_call() == true)
                {
                    Console.WriteLine(currentToken + " <string term> -> <string function call>" + " -> " + stringBuffer);
                    derivationFound = true;
                }
            }

            //Check for the Substring operator
            if(derivationFound)
            {
                Int64 term1 = 0;
                Int64 term2 = 0;
                
                storeToken = currentToken;
                
                if (matchTerminal(HighCTokenLibrary.DOLLAR_SIGN) &&
                    HC_integer_expression(out term1) &&
                    matchTerminal(HighCTokenLibrary.ELLIPSES) &&
                    HC_integer_expression(out term2))
                {
                    //Add Error Checking Here
                    int length = (int)(term2 - term1)+1;
                    stringBuffer = stringBuffer.Substring((int)term1 - 1, length);
                    Console.WriteLine(currentToken + " <string term> -> <string expression> $ <int expression> … <int expression>" + " -> " + stringBuffer);
                    return true;
                }
                
                currentToken = storeToken;
                if (matchTerminal(HighCTokenLibrary.DOLLAR_SIGN) &&
                    HC_integer_expression(out term1))
                {
                    //Add Error Checking Here
                    
                    stringBuffer = stringBuffer.Substring((int)term1 - 1, 1);
                    Console.WriteLine(currentToken + " <string term> -> <string expression> $ <int expression>" + " -> " + stringBuffer);

                    return true;
                }
                currentToken = storeToken;
            }
            
            return derivationFound;
        }
        
        private Boolean HC_string_constant(out String stringBuffer)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_string_constant"); }
            stringBuffer = "";

            if (matchTerminal(HighCTokenLibrary.STRING_LITERAL))
            {
                stringBuffer = tokenList[currentToken - 1].Text.Substring(1, tokenList[currentToken - 1].Text.Length - 2);
                Console.WriteLine(currentToken + " <string constant> -> " + tokenList[currentToken - 1].Text);
                return true;
            }

            return false;

        }

        private Boolean _______________Unimplemented_Functions_______________() { return false; }

        private Boolean HC_array_expression() { return false; }
        private Boolean HC_assignment() { return false; }
        private Boolean HC_body() { return false; }
        private Boolean HC_boolean_function_call(ref Boolean value) { value = false; return false; }
        private Boolean HC_boolean_variable(ref Boolean value) { value = false; return false; }
        private Boolean HC_case() { return false; }
        private Boolean HC_character() { return false; }
        private Boolean HC_character_function_call(out String stringBuffer) { stringBuffer = ""; return false; }
        private Boolean HC_character_variable(out String stringBuffer) { stringBuffer = ""; return false; }
        private Boolean HC_choice() { return false; }
        private Boolean HC_class() { return false; }
        private Boolean HC_class_name() { return false; }
        private Boolean HC_compiler_directive() { return false; }
        private Boolean HC_constant() { return false; }
        private Boolean HC_data_field() { return false; }
        private Boolean HC_declaration() { return false; }
        private Boolean HC_dir() { return false; }
        private Boolean HC_direction() { return false; }
        private Boolean HC_discrete_constant() { return false; }
        private Boolean HC_discrete_type() { return false; }
        private Boolean HC_element() { return false; }
        private Boolean HC_element_constant() { return false; }
        private Boolean HC_element_expression() { return false; }
        private Boolean HC_element_or_list() { return false; }
        private Boolean HC_expression() { return false; }
        private Boolean HC_enum_expression() { return false; }
        private Boolean HC_field_assign() { return false; }
        private Boolean HC_field_constant() { return false; }
        private Boolean HC_float_function_call() { return false; }
        private Boolean HC_float_variable() { return false; }
        private Boolean HC_formal_parameter() { return false; }
        private Boolean HC_function() { return false; }
        private Boolean HC_function_expression() { return false; }
        private Boolean HC_function_call() { return false; }
        private Boolean HC_global_variable() { return false; }
        private Boolean HC_id() { return false; }
        private Boolean HC_initiated_field() { return false; }
        private Boolean HC_initiated_variable() { return false; }
        private Boolean HC_input() { return false; }
        private Boolean HC_integer_function_call() { return false; }
        private Boolean HC_integer_variable() { return false; }
        private Boolean HC_iterator() { return false; }
        private Boolean HC_label() { return false; }
        private Boolean HC_list_command() { return false; }
        private Boolean HC_list_constant() { return false; }
        private Boolean HC_list_expression() { return false; }
        private Boolean HC_loop() { return false; }
        private Boolean HC_method() { return false; }
        private Boolean HC_modifiers() { return false; }
        private Boolean HC_object_constant() { return false; }
        private Boolean HC_object_expression() { return false; }
        private Boolean HC_option() { return false; }
        private Boolean HC_parameter() { return false; }
        private Boolean HC_parent() { return false; }
        private Boolean HC_prompt_variable() { return false; }
        private Boolean HC_qualifier() { return false; }
        private Boolean HC_result() { return false; }
        private Boolean HC_return() { return false; }
        private Boolean HC_return_subscript() { return false; }
        private Boolean HC_return_type() { return false; }
        private Boolean HC_scalar_type() { return false; }
        private Boolean HC_sign() { return false; }
        private Boolean HC_slice() { return false; }
        private Boolean HC_string_function_call() { return false; }
        private Boolean HC_string_variable() { return false; }
        private Boolean HC_subscript() { return false; }
        private Boolean HC_subscript_expression() { return false; }
        private Boolean HC_subscript_parameter() { return false; }
        private Boolean HC_type() { return false; }
        private Boolean HC_type_assignment() { return false; }
        private Boolean HC_type_group() { return false; }
        private Boolean HC_type_parameters() { return false; }
        private Boolean HC_type_specifier_list() { return false; }
        private Boolean HC_user_constant() { return false; }
        private Boolean HC_var() { return false; }
        private Boolean HC_variable() { return false; }
        private Boolean HC_void_call() { return false; }














    }
}
