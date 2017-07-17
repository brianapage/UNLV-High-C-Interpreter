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
        private String debugLog;
        private String consoleText;
        private Boolean fullDebug = false;
        private List<String> debugList = new List<String>();
        private Boolean stopProgram = false;
        private Boolean errorFound = false;
        //private HighCEnvironment primitiveEnvironment;
        private HighCEnvironment globalEnvironment;
        private HighCEnvironment currentEnvironment;
        
        public HighCParser(List<HighCToken> newTokens)
        {
            tokenList = newTokens;
            currentToken = 0;
            /*
            primitiveEnvironment = new HighCEnvironment();
            List<String> keywords = HighCTokenLibrary.getKeywords();
            foreach(String keyword in keywords)
            {
                primitiveEnvironment.addNewItem(keyword, new HighCData("Keyword", keyword));
            }
            globalEnvironment = new HighCEnvironment(primitiveEnvironment);
            */
            globalEnvironment = new HighCEnvironment();
            currentEnvironment = globalEnvironment;
        }

        public Boolean parse()
        {
            if (HC_program() &&
                errorFound == false)
            {
                return true;
            }
            return false;
        }

        public String getDebugLog()
        {
            foreach (String entry in debugList)
            {
                debugLog += entry;
            }
            return debugLog;
        }

        public String getConsoleText() { return consoleText; }

        private void addDebugInfo(String newEntry, Boolean addTokenPosition = true)
        {
            if (currentToken - 1 < tokenList.Count)
            {
                if (addTokenPosition == true)
                {
                    newEntry = "(L" + tokenList[currentToken - 1].Line + ", C" + tokenList[currentToken - 1].Column + "): " + newEntry;
                }

                if (debugList.Contains(newEntry) == false)
                {
                    debugList.Add(newEntry);
                }
            }
            else
            {
                Console.WriteLine("Error: Token out of range.");
            }
        }

        private Boolean matchTerminal(String token, Boolean outputToken = false)
        {
            if (currentToken == tokenList.Count)
            {
                addDebugInfo("While parsing the tokens looking for \"" + token + "\", no more valid tokens are available." + Environment.NewLine, false);
                return false;
            }
            Boolean matchStatus = tokenList[currentToken].Type == token;

            if (matchStatus == true)
            {
                Console.WriteLine("Matched Token: " + tokenList[currentToken].Text + " to " + token);
            }
            else if (outputToken == true)
            {
                currentToken++;
                addDebugInfo("Expected to find: \"" + token + "\" but found \"" + tokenList[currentToken - 1].Text + "\" instead." + Environment.NewLine);
                currentToken--;
                Console.WriteLine("  Current Token: " + tokenList[currentToken].Text + " " + tokenList[currentToken].Type);
            }
            currentToken++;
            return matchStatus;
        }

        private Boolean skipBlock()
        {
            int storeToken = currentToken;
            int bracketsToMatch;

            if (tokenList[storeToken].Type == HighCTokenLibrary.LEFT_CURLY_BRACKET)
            {
                Console.WriteLine("Skipping Block...");
                Console.WriteLine("Skipping: " + tokenList[storeToken].Text);
                bracketsToMatch = 1;
                storeToken++;
                while (storeToken < tokenList.Count && bracketsToMatch > 0)
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

                if (bracketsToMatch == 0)
                {
                    currentToken = storeToken;
                    return true;
                }
            }
            else
            {
                Console.WriteLine("Error: Block not found.");
            }
            return false;
        }

        private Boolean skipCase(HighCData value, out List<String> labelsUsed)
        {
            if (fullDebug == true) { Console.WriteLine("Skipping case..."); }
            int storeToken = currentToken;
            Boolean foundLabel = false;
            Boolean needAnother = false;
            Boolean matchFound = false;
            String stringBuffer = "";
            labelsUsed = new List<String>();
            
            /*
             * on ( <label list> ) <block>
             */
            if (matchTerminal(HighCTokenLibrary.ON) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
            {
                storeToken = currentToken;
                while (HC_label(value, out matchFound, out stringBuffer))
                {
                    foundLabel = true;
                    storeToken = currentToken;
                    labelsUsed.Add(stringBuffer);
                    if (matchTerminal(HighCTokenLibrary.COMMA))
                    {
                        needAnother = true;
                    }
                    else
                    {
                        break;
                    }
                }
                currentToken = storeToken;

                if (matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true) == false)
                {
                    return false;
                }

                if (foundLabel == false)
                {
                    addDebugInfo(HighCTokenLibrary.ON + ": at least one element was expected." + Environment.NewLine);
                    return false;
                }

                if (needAnother == true)
                {
                    addDebugInfo(HighCTokenLibrary.ON + ": another element was expected after the comma." + Environment.NewLine);
                    return false;
                }

                Console.WriteLine(currentToken + " on ( <label list> ) <block> -> " + value + " -> Block Not Taken");
                return skipBlock();
            }

            return false;
        }

        private void error()
        {
            stopProgram = true;
            errorFound = true;
        }

        private Boolean _______________Productions_______________() { return false; }

        private Boolean HC_add_op(out String opType)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_add_op"); }
            int storeToken = currentToken;

            /*
                +
                -
             */

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

        private Boolean HC_arithmetic_expression(ref HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_expression"); }

            /*
             <arith – expr> <add – op> <arith – term>
             <arith – term>
             */

            if (HC_arithmetic_term(ref value) &&
               HC_arithmetic_expression_helper(ref value))
            {
                Console.WriteLine(currentToken + " <arithmetic expression> -> <arithmetic term><arithmetic expression with integer tracking>" + " -> " + value);
                return true;
            }

            return false;
        }

        private Boolean HC_arithmetic_expression_with_integer_tracking(ref HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_expression_with_integer_tracking" + " -> " + value); }
            
            if (HC_arithmetic_term(ref value) &&
               HC_arithmetic_expression_helper(ref value))
            {
                Console.WriteLine(currentToken + " <arithmetic expression with integer tracking>" + " -> " + value);
                return true;
            }

            return false;
        }

        private Boolean HC_arithmetic_expression_helper(ref HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_expression_helper"); }
            int storeToken = currentToken;
            HighCData term1 = new HighCData(HighCTokenLibrary.INTEGER,0);
            String addOp;

            if (HC_add_op(out addOp) &&
                HC_arithmetic_term(ref term1))
            {
                if (addOp == HighCTokenLibrary.PLUS_SIGN)
                {
                    if (value.type == HighCTokenLibrary.FLOAT &&
                       term1.type == HighCTokenLibrary.FLOAT)
                    {
                        value.data = (Double)value.data + (Double)term1.data;
                        value.type = HighCTokenLibrary.FLOAT;
                    }
                    else if (value.type == HighCTokenLibrary.INTEGER &&
                             term1.type == HighCTokenLibrary.FLOAT)
                    {
                        value.data = (Int64)value.data + (Double)term1.data;
                        value.type = HighCTokenLibrary.FLOAT;
                    }
                    else if (value.type == HighCTokenLibrary.FLOAT &&
                             term1.type == HighCTokenLibrary.INTEGER)
                    {
                        value.data = (Double)value.data + (Int64)term1.data;
                        value.type = HighCTokenLibrary.FLOAT;
                    }
                    else
                    {
                        value.data = (Int64)value.data + (Int64)term1.data;
                        value.type = HighCTokenLibrary.INTEGER;
                    }
                    Console.WriteLine(currentToken + " <arithmetic expression'> -> + <arithmetic term>" + " -> " + value);
                }
                else if (addOp == HighCTokenLibrary.MINUS_SIGN)
                {
                    if (value.type == HighCTokenLibrary.FLOAT ||
                       term1.type == HighCTokenLibrary.FLOAT)
                    {
                        value.data = (Double)value.data - (Double)term1.data;
                        value.type = HighCTokenLibrary.FLOAT;
                    }
                    else if (value.type == HighCTokenLibrary.INTEGER &&
                             term1.type == HighCTokenLibrary.FLOAT)
                    {
                        value.data = (Int64)value.data - (Double)term1.data;
                        value.type = HighCTokenLibrary.FLOAT;
                    }
                    else if (value.type == HighCTokenLibrary.FLOAT &&
                             term1.type == HighCTokenLibrary.INTEGER)
                    {
                        value.data = (Double)value.data - (Int64)term1.data;
                        value.type = HighCTokenLibrary.FLOAT;
                    }
                    else
                    {
                        value.data = (Int64)value.data - (Int64)term1.data;
                        value.type = HighCTokenLibrary.INTEGER;
                    }
                    Console.WriteLine(currentToken + " <arithmetic expression'> -> - <arithmetic term>" + " -> " + value);
                }

                if (HC_arithmetic_expression_helper(ref value))
                {
                    return true;
                }
                else { return false; }
            }

            Console.WriteLine(currentToken + " <arithmetic expression'> -> null");
            currentToken = storeToken;
            return true;
        }

        private Boolean HC_arithmetic_factor(ref HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_factor"); }

            /*
            <arith – factor> ^ <arith – primary>
            <arith – primary>
             */

            if (HC_arithmetic_primary(ref value) &&
               HC_arithmetic_factor_helper(ref value))
            {
                Console.WriteLine(currentToken + " <arithmetic factor>-><arithmetic primary><arithmetic factor'>" + " -> " + value);
                return true;
            }

            return false;
        }

        private Boolean HC_arithmetic_factor_helper(ref HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_factor_helper"); }
            int storeToken = currentToken;
            HighCData term1 = new HighCData(HighCTokenLibrary.INTEGER,0);

            if (matchTerminal(HighCTokenLibrary.CARET) &&
                HC_arithmetic_primary(ref term1))
            {
                if (value.type == HighCTokenLibrary.FLOAT &&
                    term1.type == HighCTokenLibrary.FLOAT)
                {
                    value.data = Math.Pow((Double)value.data, (Double)term1.data);
                    value.type = HighCTokenLibrary.FLOAT;
                }
                else if (value.type == HighCTokenLibrary.INTEGER &&
                         term1.type == HighCTokenLibrary.FLOAT)
                {
                    value.data = Math.Pow((Int64)value.data , (Double)term1.data);
                    value.type = HighCTokenLibrary.FLOAT;
                }
                else if (value.type == HighCTokenLibrary.FLOAT &&
                         term1.type == HighCTokenLibrary.INTEGER)
                {
                    value.data = Math.Pow((Double)value.data , (Int64)term1.data);
                    value.type = HighCTokenLibrary.FLOAT;
                }
                else if((Int64)term1.data < 0)
                {
                    value.data = Math.Pow((Int64)value.data, (Int64)term1.data);
                    value.type = HighCTokenLibrary.FLOAT;
                }
                else
                {
                    value.data = Math.Pow((Int64)value.data, (Int64)term1.data);
                    value.type = HighCTokenLibrary.INTEGER;
                }
                
                Console.WriteLine(currentToken + " <arithmetic factor'> -> ^ <arithmetic primary>" + " -> " + value);

                if (HC_arithmetic_factor_helper(ref value))
                {
                    return true;
                }
            }

            Console.WriteLine(currentToken + " <arithmetic factor'> -> null");
            currentToken = storeToken;
            return true;
        }

        private Boolean HC_arithmetic_primary(ref HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_primary"); }
            int storeToken = currentToken;
            String addOp;

            /*
            <add – op> <arith – primary>
            ( <arith – expr> )
            <int – constant>
            <float – constant>
            <int – variable>
            <float – variable>
            <int – func call>
            <float – func call>
             */

            if (HC_add_op(out addOp) &&
                HC_arithmetic_primary(ref value))
            {
                if (addOp == HighCTokenLibrary.MINUS_SIGN)
                {
                    if(value.type==HighCTokenLibrary.INTEGER)
                    {
                        value.data = ((Int64)value.data) * -1;
                    }
                    else if(value.type == HighCTokenLibrary.FLOAT)
                    {
                        value.data = ((Double)value.data) * -1;
                    }
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
               HC_arithmetic_expression_with_integer_tracking(ref value) &&
               matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS))
            {
                Console.WriteLine(currentToken + " <arithmetic primary> -> (<arithmetic expression with integer tracking>)" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            Int64 intBuffer = 0;
            if (HC_integer_constant(out intBuffer))
            {
                value = new HighCData(HighCTokenLibrary.INTEGER, intBuffer);
                Console.WriteLine(currentToken + " <arithmetic primary> -> <integer constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            Double floatBuffer = 0.0;
            if (HC_float_constant(out floatBuffer))
            {
                value = new HighCData(HighCTokenLibrary.FLOAT, floatBuffer);
                Console.WriteLine(currentToken + " <arithmetic primary> -> <float constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            Int64 intValue;
            if (HC_integer_variable(out intValue))
            {
                value = new HighCData(HighCTokenLibrary.INTEGER, intValue);
                Console.WriteLine(currentToken + " <arithmetic primary> -> <integer variable>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            Double floatValue;
            if (HC_float_variable(out floatValue))
            {
                value = new HighCData(HighCTokenLibrary.FLOAT, floatValue);
                Console.WriteLine(currentToken + " <arithmetic primary> -> <float variable>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_integer_function_call(out intValue))
            {
                value = new HighCData(HighCTokenLibrary.INTEGER, intValue);
                Console.WriteLine(currentToken + " <arithmetic primary> -> <integer function call>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_float_function_call(out floatValue))
            {
                value = new HighCData(HighCTokenLibrary.FLOAT, floatValue);
                Console.WriteLine(currentToken + " <arithmetic primary> -> <float function call>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.LENGTH) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS) &&
               HC_list_expression() &&
               matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS))
            {
                
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
                value = new HighCData(HighCTokenLibrary.INTEGER, localStringBuffer.Length);
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
                    value = new HighCData(HighCTokenLibrary.INTEGER, localStringBuffer2.IndexOf(localStringBuffer));
                }
                else
                {
                    value = new HighCData(HighCTokenLibrary.INTEGER, 0);
                }
                Console.WriteLine(currentToken + " <arithmetic primary> -> Match(<string expression>,<string expression>)" + " -> " + value);
                return true;
            }
            
            return false;
        }

        private Boolean HC_arithmetic_term(ref HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_term"); }

            /*
            <arith – term> <mult – op> <arith – factor>
            <arith – factor>
            */

            if (HC_arithmetic_factor(ref value) &&
               HC_arithmetic_term_helper(ref value))
            {
                Console.WriteLine(currentToken + " <arithmetic term>-><arithmetic factor><arithmetic term'>" + " -> " + value);
                return true;
            }
            
            return false;
        }

        private Boolean HC_arithmetic_term_helper(ref HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_arithmetic_term_helper"); }
            int storeToken = currentToken;
            HighCData term1 = new HighCData(HighCTokenLibrary.INTEGER, 0);
            String multOp;

            if (HC_mult_op(out multOp) &&
                HC_arithmetic_factor(ref term1))
            {
                if (multOp == HighCTokenLibrary.ASTERICK)
                {
                    if (value.type == HighCTokenLibrary.FLOAT &&
                        term1.type == HighCTokenLibrary.FLOAT)
                    {
                        value.data = (Double)value.data * (Double)term1.data;
                        value.type = HighCTokenLibrary.FLOAT;
                    }
                    else if (value.type == HighCTokenLibrary.INTEGER &&
                         term1.type == HighCTokenLibrary.FLOAT)
                    {
                        value.data = (Int64)value.data * (Double)term1.data;
                        value.type = HighCTokenLibrary.FLOAT;
                    }
                    else if (value.type == HighCTokenLibrary.FLOAT &&
                             term1.type == HighCTokenLibrary.INTEGER)
                    {
                        value.data = (Double)value.data * (Int64)term1.data;
                        value.type = HighCTokenLibrary.FLOAT;
                    }
                    else
                    {
                        value.data = (Int64)value.data * (Int64)term1.data;
                        value.type = HighCTokenLibrary.INTEGER;
                    }
                    Console.WriteLine(currentToken + " <arithmetic term'> -> * <arithmetic term'>" + " -> " + value);
                }
                else if (multOp == HighCTokenLibrary.SLASH)
                {
                    if (value.type == HighCTokenLibrary.FLOAT &&
                        term1.type == HighCTokenLibrary.FLOAT)
                    {
                        value.data = (Double)value.data / (Double)term1.data;
                        value.type = HighCTokenLibrary.FLOAT;
                    }
                    else if (value.type == HighCTokenLibrary.INTEGER &&
                         term1.type == HighCTokenLibrary.FLOAT)
                    {
                        value.data = (Double)((Int64)value.data) / (Double)term1.data;
                        value.type = HighCTokenLibrary.FLOAT;
                    }
                    else if (value.type == HighCTokenLibrary.FLOAT &&
                             term1.type == HighCTokenLibrary.INTEGER)
                    {
                        value.data = (Double)value.data / (Int64)term1.data;
                        value.type = HighCTokenLibrary.FLOAT;
                    }
                    else
                    {
                        value.data = (Double)((Int64)value.data) / (Double)((Int64)term1.data);
                        value.type = HighCTokenLibrary.FLOAT;
                    }
                    
                    Console.WriteLine(currentToken + " <arithmetic term'> -> / <arithmetic term'>" + " -> " + value);
                }
                else if (multOp == HighCTokenLibrary.PERCENT_SIGN)
                {
                    if (value.type==HighCTokenLibrary.INTEGER && 
                        term1.type==HighCTokenLibrary.INTEGER)
                    {
                        value.data = (Int64)value.data % (Int64)term1.data;
                        Console.WriteLine(currentToken + " <arithmetic term'> -> % <arithmetic term'>" + " -> " + value);
                    }
                    else
                    {
                        addDebugInfo("While performing a modulus operation one or more operands were not integer values." + Environment.NewLine);
                        return false;
                    }
                }

                if (HC_arithmetic_term_helper(ref value))
                {
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

        private Boolean HC_assignment()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_assignment"); }
            int storeToken = currentToken;
            HighCData value = null;
            HighCData foundItem = null;
            String identifier = "";
            /*
            set <variable> = <expr>
             */

            if(matchTerminal(HighCTokenLibrary.SET))
            {
                if(HC_variable(out identifier))
                {
                    if(matchTerminal(HighCTokenLibrary.EQUAL,true))
                    {
                        if(HC_expression(out value))
                        {
                            if (currentEnvironment.changeItem(identifier, value, out foundItem))
                            {
                                Console.WriteLine(currentToken + " <assignment> -> set <variable> = <expression>");
                                return true;
                            }
                            else
                            {
                                if(foundItem == null)
                                {
                                    addDebugInfo(HighCTokenLibrary.SET + ": The specified identifier could not be found." + Environment.NewLine);
                                }
                                else if(foundItem.writable==false)
                                {
                                    addDebugInfo(HighCTokenLibrary.SET + ": The specified identifier is a constant which cannot be changed after declaration." + Environment.NewLine);
                                }
                                else 
                                {
                                    addDebugInfo(HighCTokenLibrary.SET + ": The specified variable could not be altered due to a type mismatch: expected <"+foundItem.type+"> but given <"+value.type+">." + Environment.NewLine);
                                }
                                stopProgram = true;
                                errorFound = true;
                                return false;
                            }
                        }
                        else
                        {
                            addDebugInfo(HighCTokenLibrary.SET + ": An expression was expected after the equal sign." + Environment.NewLine);
                        }
                    }
                }
                else
                {
                    addDebugInfo(HighCTokenLibrary.SET + ": An identifier name was expected." + Environment.NewLine);
                }
            }

            return false;
        }

        private Boolean HC_block()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_block"); }
            int storeToken = currentToken;

            if (matchTerminal(HighCTokenLibrary.LEFT_CURLY_BRACKET, true) == false)
            {
                return false;
            }

            storeToken = currentToken;
            HighCEnvironment storeEnvironment = currentEnvironment;
            currentEnvironment = new HighCEnvironment(storeEnvironment);

            while (HC_declaration())
            {
                storeToken = currentToken;
                Console.WriteLine(currentToken + " <block> -> <declaration>");
            }
            currentToken = storeToken;

            while (HC_statement())
            {
                storeToken = currentToken;
                Console.WriteLine(currentToken + " <block> -> <statement>");
            }
            currentToken = storeToken;

            if (stopProgram == false &&
                matchTerminal(HighCTokenLibrary.RIGHT_CURLY_BRACKET, true))
            {
                Console.WriteLine(currentToken + " <block>");
                currentEnvironment = storeEnvironment;
                return true;
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
            if (matchTerminal(HighCTokenLibrary.FALSE))
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

        private Boolean HC_boolean_variable(ref Boolean value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_boolean_variable"); }
            int storeToken = currentToken;
            value = false;
            String identifier;

            if (HC_id(out identifier))
            {
                HighCData temp = currentEnvironment.getItem(identifier);

                if (temp == null)
                {
                    addDebugInfo("Boolean Variable: The specified variable \"" + identifier + "\" could not be found." + Environment.NewLine);
                    return false;
                }
                else if(temp.readable == false)
                {
                    addDebugInfo("Boolean Variable: The specified variable \"" + identifier + "\" cannot be referenced." + Environment.NewLine);
                    errorFound = true;
                    stopProgram = true;
                    return false;
                }
                else if (temp.type == HighCTokenLibrary.BOOLEAN)
                {
                    value = (Boolean)temp.data;
                    Console.WriteLine(currentToken + " <boolean variable> -> <id> -> " + identifier + " " + value);
                    return true;
                }
            }

            
            return false;
        }


        private Boolean HC_case(HighCData value, out Boolean matchFound, out List<String> labelsUsed)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_case"); }
            matchFound = false;
            int storeToken = currentToken;
            Boolean foundLabel = false;
            Boolean needAnother = false;
            Boolean atLeastOneMatchFound = false;
            String stringBuffer = "";
            labelsUsed = new List<string>();
            
            /*
             * on ( <label list> ) <block>
             */
            if (matchTerminal(HighCTokenLibrary.ON) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
            {
                storeToken = currentToken;
                while (HC_label(value, out matchFound, out stringBuffer))
                {
                    foundLabel = true;
                    storeToken = currentToken;
                    atLeastOneMatchFound = atLeastOneMatchFound || matchFound;
                    labelsUsed.Add(stringBuffer);
                    needAnother = false;
                    if (matchTerminal(HighCTokenLibrary.COMMA))
                    {
                        needAnother = true;
                    }
                    else
                    {
                        break;
                    }
                }
                currentToken = storeToken;

                if (matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true) == false)
                {
                    return false;
                }

                if (foundLabel == false)
                {
                    addDebugInfo(HighCTokenLibrary.ON + ": at least one element was expected." + Environment.NewLine);
                    return false;
                }

                if (needAnother == true)
                {
                    addDebugInfo(HighCTokenLibrary.ON + ": another element was expected after the comma." + Environment.NewLine);
                    return false;
                }

                if (atLeastOneMatchFound == true)
                {
                    if (HC_block())
                    {
                        Console.WriteLine(currentToken + " on ( <label list> ) <block> -> " + value.data + " " + value.type + " -> Block Taken");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine(currentToken + " on ( <label list> ) <block> -> " + value.data + " " + value.type + " -> Block Not Taken");
                    return skipBlock();
                }
            }

            return false;
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

            if (HC_character_variable(out stringBuffer))
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
                if (stringBuffer.Length > 0)
                {
                    Char newChar = (Char)((Char)stringBuffer[0] + 1);
                    stringBuffer = "" + newChar;
                }
                Console.WriteLine(currentToken + " <character expression> -> Next(<char expression>)" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.PREVIOUS) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS) &&
                HC_character_expression(out stringBuffer) &&
                matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS))
            {
                if (stringBuffer.Length > 0)
                {
                    Char newChar = (Char)((Char)stringBuffer[0] - 1);
                    stringBuffer = "" + newChar;
                }
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

        private Boolean HC_character_variable(out String stringBuffer)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_character_variable"); }
            int storeToken = currentToken;
            stringBuffer = "";
            String identifier;

            if (HC_id(out identifier))
            {
                HighCData temp = currentEnvironment.getItem(identifier);

                if (temp == null)
                {
                    addDebugInfo("Boolean Variable: The specified variable \"" + identifier + "\" could not be found." + Environment.NewLine);
                    return false;
                }
                else if (temp.readable == false)
                {
                    addDebugInfo("Character Variable: The specified variable \"" + identifier + "\" cannot be referenced." + Environment.NewLine);
                    errorFound = true;
                    stopProgram = true;
                    return false;
                }
                else if (temp.type == HighCTokenLibrary.CHARACTER)
                {
                    stringBuffer = (String)temp.data;
                    Console.WriteLine(currentToken + " <boolean variable> -> <id> -> " + identifier + " " + stringBuffer);
                    return true;
                }
            }
            
            return false;
        }


        private Boolean HC_choice()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_choice"); }
            int storeToken = currentToken;

            /*
             * choice ( <discrete – expr> ) <case>* other <block>
             */

            Boolean matchFound = false;
            List<String> labelsUsed = new List<String>();
            List<String> currentLabels = new List<String>();
            HighCData value;

            if (matchTerminal(HighCTokenLibrary.CHOICE) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
            {
                if (HC_discrete_expression(out value))
                {
                    if (matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true))
                    {
                        storeToken = currentToken;
                        while (matchFound == false &&
                            HC_case(value, out matchFound, out currentLabels))
                        {
                            storeToken = currentToken;

                            foreach (String label in currentLabels)
                            {
                                if (labelsUsed.Contains(label))
                                {
                                    addDebugInfo(HighCTokenLibrary.CHOICE + ": Each condition must be unique. \"" + label + "\" is used more than once." + Environment.NewLine);
                                    return false;
                                }
                                else
                                {
                                    labelsUsed.Add(label);
                                }
                            }
                        }
                        currentToken = storeToken;

                        while (skipCase(value, out currentLabels))
                        {
                            storeToken = currentToken;
                            foreach (String label in currentLabels)
                            {
                                if (labelsUsed.Contains(label))
                                {
                                    addDebugInfo(HighCTokenLibrary.CHOICE + ": Each condition must be unique. \"" + label + "\" is used more than once." + Environment.NewLine);
                                    return false;
                                }
                                else
                                {
                                    labelsUsed.Add(label);
                                }
                            }
                        }
                        currentToken = storeToken;

                        if (value.type == HighCTokenLibrary.BOOLEAN)
                        {
                            if (labelsUsed.Count > 2)
                            {
                                addDebugInfo(HighCTokenLibrary.CHOICE + ": Conditions cannot overlap." + Environment.NewLine);
                                return false;
                            }
                        }
                        else if (value.type == HighCTokenLibrary.INTEGER)
                        {
                            labelsUsed.Sort();
                            Int64 currentValue = 0;
                            Boolean firstListItem = true;
                            String previousLabel = "";
                            foreach (String label in labelsUsed)
                            {
                                if(label.Contains("..."))
                                {
                                    Int64 firstValue = Int64.Parse(label.Substring(0, label.IndexOf("...")));
                                    Int64 secondValue = Int64.Parse(label.Substring(label.IndexOf("...")+3));

                                    if(firstValue >= secondValue)
                                    {
                                        addDebugInfo(HighCTokenLibrary.CHOICE + ": "+ firstValue + " must be less than " + secondValue+ "."+Environment.NewLine);
                                        return false;
                                    }

                                    if (firstListItem == true)
                                    {
                                        currentValue = secondValue;
                                        firstListItem = false;
                                        previousLabel = label;
                                    }
                                    else if(currentValue<firstValue)
                                    {
                                        currentValue = secondValue;
                                        previousLabel = label;
                                    }
                                    else
                                    {
                                        addDebugInfo(HighCTokenLibrary.CHOICE + ": Conditions (\""+previousLabel+"\") and (\""+label+"\") cannot overlap." + Environment.NewLine);
                                        return false;
                                    }
                                }
                                else
                                {
                                    Int64 firstValue = Int64.Parse(label);

                                    if (firstListItem == true)
                                    {
                                        currentValue = Int64.Parse(label);
                                        firstListItem = false;
                                        previousLabel = label;
                                    }
                                    else if(currentValue < firstValue)
                                    {
                                        currentValue = firstValue;
                                        previousLabel = label;
                                    }
                                    else
                                    {
                                        addDebugInfo(HighCTokenLibrary.CHOICE + ": Conditions (\"" + previousLabel + "\") and (\"" + label + "\") cannot overlap." + Environment.NewLine);
                                        return false;
                                    }
                                }
                            }
                        }
                        else if (value.type == HighCTokenLibrary.CHARACTER)
                        {
                            labelsUsed.Sort();
                            Char currentValue = ' ';
                            Boolean firstListItem = true;
                            String previousLabel = "";
                            foreach (String label in labelsUsed)
                            {
                                if (label.Contains("..."))
                                {
                                    Char firstValue = label[0];
                                    Char secondValue = label[4];

                                    if (firstValue >= secondValue)
                                    {
                                        addDebugInfo(HighCTokenLibrary.CHOICE + ": " + firstValue + " must be less than " + secondValue + "." + Environment.NewLine);
                                        return false;
                                    }

                                    if (firstListItem == true)
                                    {
                                        currentValue = secondValue;
                                        firstListItem = false;
                                    }
                                    else if (currentValue < firstValue)
                                    {
                                        currentValue = secondValue;
                                    }
                                    else
                                    {
                                        addDebugInfo(HighCTokenLibrary.CHOICE + ": Conditions (\"" + previousLabel + "\") and (\"" + label + "\") cannot overlap." + Environment.NewLine);
                                        return false;
                                    }
                                }
                                else
                                {
                                    Char firstValue = label[0];

                                    if (firstListItem == true)
                                    {
                                        currentValue = firstValue;
                                        firstListItem = false;
                                    }
                                    else if (currentValue < firstValue)
                                    {
                                        currentValue = firstValue;
                                    }
                                    else
                                    {
                                        addDebugInfo(HighCTokenLibrary.CHOICE + ": Conditions (\"" + previousLabel + "\") and (\"" + label + "\") cannot overlap." + Environment.NewLine);
                                        return false;
                                    }
                                }
                            }
                        }
                        else if (value.type == HighCTokenLibrary.ENUMERATION)
                        {
                            return false;
                        }

                        if (matchTerminal(HighCTokenLibrary.OTHER, true))
                        {
                            if (matchFound == true)
                            {
                                if (skipBlock() == true)
                                {
                                    return true;
                                }
                            }
                            else if (HC_block())
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    addDebugInfo(HighCTokenLibrary.CHOICE + ": A discrete (boolean, character, enumeration, or integer) expression was expected inside the parenthesis." + Environment.NewLine);
                }
            }

            return false;
        }

        private Boolean HC_constant(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_constant"); }
            value = null;
            int storeToken = currentToken;
            Boolean boolTerm1 = false;
            String stringTerm1 = "";
            Double doubleTerm1 = 0.0;
            Int64 intTerm1 = 0;

            /*
             * boolean constant
             * character constant
             * string constant
             * integer constant
             * float constant
             * enum
             */

            if (HC_boolean_constant(ref boolTerm1))
            {
                value = new HighCData(HighCTokenLibrary.BOOLEAN, boolTerm1);
                Console.WriteLine(currentToken + " <constant> -> <boolean constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_character_constant(out stringTerm1))
            {
                value = new HighCData(HighCTokenLibrary.CHARACTER, stringTerm1);
                Console.WriteLine(currentToken + " <constant> -> <character constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_string_constant(out stringTerm1))
            {
                value = new HighCData(HighCTokenLibrary.STRING, stringTerm1);
                Console.WriteLine(currentToken + " <constant> -> <string constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_integer_constant(out intTerm1))
            {
                value = new HighCData(HighCTokenLibrary.INTEGER, intTerm1);
                Console.WriteLine(currentToken + " <constant> -> <integer constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_float_constant(out doubleTerm1))
            {
                value = new HighCData(HighCTokenLibrary.FLOAT, doubleTerm1);
                Console.WriteLine(currentToken + " <constant> -> <float constant>" + " -> " + value);
                return true;
            }

            //ENUM GOES HERE

            return false;
        }

        private Boolean HC_control_statement()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_control_statement"); }
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

            if (matchTerminal(HighCTokenLibrary.STOP))
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

        private Boolean HC_declaration()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_declaration"); }
            int storeToken = currentToken;
            /*
             * create <type> <initiated – var list>
                multi <class name> <initiated – var list>  Likely will not be implemented in this version
             */
            Boolean atLeastOneFound = false;
            Boolean needAnother = false;
            String type = "";
            String type2 = "";

            if (matchTerminal(HighCTokenLibrary.CREATE))
            {
                if (HC_type(out type))
                {
                    storeToken = currentToken;
                    while (HC_initiated_variable(type, out type2))
                    {
                        storeToken = currentToken;
                        atLeastOneFound = true;
                        needAnother = false;

                        /*
                        if (type.Contains(type2) == false)
                        {
                            addDebugInfo("Variable Declaration" + ": The type of the variable (\"" + type2 + "\") does not match the type indicated (\"" + type + "\")." + Environment.NewLine);
                            return false;
                        }
                        */
                        //Match types

                        if (matchTerminal(HighCTokenLibrary.COMMA))
                        {
                            needAnother = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                    currentToken = storeToken;

                    if (needAnother == true)
                    {
                        addDebugInfo("Variable Declaration" + ": another element was expected after the comma." + Environment.NewLine);
                        return false;
                    }

                    if (atLeastOneFound == false)
                    {
                        addDebugInfo("Variable Declaration" + ": at least one declaration (\"<identifier> = <value>\") was expected after the type." + Environment.NewLine);
                        return false;
                    }

                    Console.WriteLine(currentToken + " <declaration> -> <type><initiated variable>,...,<initiated variable> -> " + type);

                    return true;
                }
                else
                {
                    addDebugInfo(HighCTokenLibrary.CREATE + ": Expected a data or class type." + Environment.NewLine);
                }
            }

            return false;
        }
        
        private Boolean HC_direction(out Boolean inAllowed, out Boolean outAllowed)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_direction"); }
            int storeToken = currentToken;

            inAllowed = false;
            outAllowed = false;

            /*
            ε
            in
            out
            inout
            in out
             */

            if (matchTerminal(HighCTokenLibrary.IN))
            {
                inAllowed = true;
                storeToken = currentToken;
                if (matchTerminal(HighCTokenLibrary.OUT))
                {
                    outAllowed = true;
                    Console.WriteLine(currentToken + " <direction>" + " -> " + HighCTokenLibrary.IN + " " + HighCTokenLibrary.OUT);
                    return true;
                }
                else
                {
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <direction>" + " -> " + HighCTokenLibrary.IN);
                    return true;
                }
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.OUT))
            {
                outAllowed = true;
                Console.WriteLine(currentToken + " <direction>" + " -> " + HighCTokenLibrary.OUT);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.IN_OUT))
            {
                inAllowed = true;
                outAllowed = true;
                Console.WriteLine(currentToken + " <direction>" + " -> " + HighCTokenLibrary.IN_OUT);
                return true;
            }

            inAllowed = true; //Default option
            currentToken = storeToken;
            Console.WriteLine(currentToken + " <direction>" + " -> null");
            return true;
        }

        private Boolean HC_discrete_expression(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_discrete_expression"); }
            int storeToken = currentToken;
            
            Int64 term1 = 0;
            Boolean boolTerm = false;
            String stringBuffer = "";
            value = null;

            if (HC_boolean_expression(ref boolTerm) == true)
            {
                value = new HighCData(HighCTokenLibrary.BOOLEAN, boolTerm);
                Console.WriteLine(currentToken + " <discrete expression> -> <boolean expression>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_character_expression(out stringBuffer) == true)
            {
                value = new HighCData(HighCTokenLibrary.CHARACTER, stringBuffer);
                Console.WriteLine(currentToken + " <discrete expression> -> <character expression>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_integer_expression(out term1) == true)
            {
                value = new HighCData(HighCTokenLibrary.INTEGER, term1);
                Console.WriteLine(currentToken + " <discrete expression> -> <integer expression>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_enum_expression() == true)
            {
                Console.WriteLine(currentToken + " <discrete expression> -> <enum expression>" + " -> " + value);
                return true;
            }

            return false;
        }

        private Boolean HC_discrete_type(out String type)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_else_if"); }
            int storeToken = currentToken;
            type = "";
            Int64 intTerm1 = 0;
            Int64 intTerm2 = 0;
            String charBuffer1 = "";
            String charBuffer2 = "";

            /*
            BOOL
            INT
            INT: <int – constant> … <int – constant>
            CHAR
            CHAR: <char – constant> … <char – constant>
            <enum type>
            <enum type>: <enum – constant> … <enum constant>
             */

            if (matchTerminal(HighCTokenLibrary.BOOLEAN))
            {
                type = HighCTokenLibrary.BOOLEAN;
                Console.WriteLine(currentToken + " <discrete type> -> " + type);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.INTEGER) &&
                matchTerminal(HighCTokenLibrary.COLON))
            {
                if (HC_integer_constant(out intTerm1) == false)
                {
                    addDebugInfo(HighCTokenLibrary.INTEGER + HighCTokenLibrary.COLON + ": Expected an integer constant to follow the colon to indicate the lower bound of the variable." + Environment.NewLine);
                    return false;
                }
                if (matchTerminal(HighCTokenLibrary.ELLIPSES, true) == false)
                {
                    return false;
                }
                if (HC_integer_constant(out intTerm2) == false)
                {
                    addDebugInfo(HighCTokenLibrary.INTEGER + HighCTokenLibrary.COLON + ": Expected an integer constant to follow the colon to indicate the upper bound of the variable." + Environment.NewLine);
                    return false;
                }

                type = HighCTokenLibrary.INTEGER + HighCTokenLibrary.COLON + intTerm1.ToString() + HighCTokenLibrary.ELLIPSES + intTerm2.ToString();
                if (intTerm1 >= intTerm2)
                {
                    Console.WriteLine(currentToken + " <discrete type> -> " + type);
                }
                else
                {
                    addDebugInfo(HighCTokenLibrary.INTEGER + HighCTokenLibrary.COLON + ": The second range specification must be greater than or equal to the first." + Environment.NewLine);
                    return false;
                }
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.INTEGER))
            {
                type = HighCTokenLibrary.INTEGER;
                Console.WriteLine(currentToken + " <discrete type> -> " + type);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.CHARACTER) &&
                matchTerminal(HighCTokenLibrary.COLON))
            {
                if (HC_character_constant(out charBuffer1) == false)
                {
                    addDebugInfo(HighCTokenLibrary.CHARACTER + HighCTokenLibrary.COLON + ": Expected a character constant to follow the colon to indicate the lower bound of the variable." + Environment.NewLine);
                    return false;
                }
                if (matchTerminal(HighCTokenLibrary.ELLIPSES, true) == false)
                {
                    return false;
                }
                if (HC_character_constant(out charBuffer2) == false)
                {
                    addDebugInfo(HighCTokenLibrary.CHARACTER + HighCTokenLibrary.COLON + ": Expected a character constant to follow the colon to indicate the upper bound of the variable." + Environment.NewLine);
                    return false;
                }

                type = HighCTokenLibrary.CHARACTER + HighCTokenLibrary.COLON + charBuffer1 + HighCTokenLibrary.ELLIPSES + charBuffer2;
                if (charBuffer1[0] >= charBuffer2[0])
                {
                    Console.WriteLine(currentToken + " <discrete type> -> " + type);
                }
                else
                {
                    addDebugInfo(HighCTokenLibrary.CHARACTER + HighCTokenLibrary.COLON + ": The second range specification must be greater than or equal to the first." + Environment.NewLine);
                    return false;
                }
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.CHARACTER))
            {
                type = HighCTokenLibrary.CHARACTER;
                Console.WriteLine(currentToken + " <discrete type> -> " + type);
                return true;
            }

            //Enums go here

            return false;
        }

        private Boolean HC_else_if(ref Boolean blockEntered)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_else_if"); }
            /*
            elseif ( <bool – expr> ) <block>
            else if ( <bool – expr> ) <block>
             */
            int storeToken = currentToken;
            Boolean elseifFound = false;
            String elseifStyle = "";

            if (matchTerminal(HighCTokenLibrary.ELSE) &&
               matchTerminal(HighCTokenLibrary.IF))
            {
                elseifFound = true;
                elseifStyle = HighCTokenLibrary.ELSE + " " + HighCTokenLibrary.IF;
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

            if (elseifFound == true)
            {
                if (matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
                {
                    Boolean boolTerm1 = false;
                    if (HC_boolean_expression(ref boolTerm1))
                    {
                        if (matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true))
                        {
                            if (blockEntered == false &&
                                boolTerm1 == true)
                            {
                                blockEntered = true;
                                if (HC_block())
                                {
                                    Console.WriteLine(currentToken + " <else if> -> " + elseifStyle + " ( <boolean expression> ) <block> -> branch taken");
                                    return true;
                                }
                            }
                            else if (skipBlock())
                            {
                                Console.WriteLine(currentToken + " <else if> -> " + elseifStyle + " ( <boolean expression> ) <block> -> branch not taken");
                                return true;
                            }
                            else
                            {
                                addDebugInfo(elseifStyle + ": Must be followed by a block. Example: \"" + HighCTokenLibrary.LEFT_CURLY_BRACKET + " " + HighCTokenLibrary.RIGHT_CURLY_BRACKET + "\"." + Environment.NewLine);
                            }
                        }
                    }
                    else
                    {
                        addDebugInfo(elseifStyle + ": A boolean expression was expected inside the parenthesis." + Environment.NewLine);
                    }
                }

                error();
            }

            Console.WriteLine(currentToken + " <else if> -> Null");
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

            if (matchTerminal(HighCTokenLibrary.EQUAL))
            {
                opType = tokenList[currentToken - 1].Type;
                Console.WriteLine(currentToken + " <equality op> -> " + HighCTokenLibrary.EQUAL);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.NOT_EQUAL))
            {
                opType = tokenList[currentToken - 1].Type;
                Console.WriteLine(currentToken + " <equality op> -> " + HighCTokenLibrary.EQUAL);
                return true;
            }

            return false;
        }
        
        private Boolean HC_expression(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_expression"); }
            int storeToken = currentToken;
            value = null;
            /*
            ( <expr> )
            <object – expr>
            <array – expr>
            <list – expr>
            <scalar – expr>
             */

            if(matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS))
            {
                if (HC_expression(out value))
                {
                    if(matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS,true))
                    {
                        Console.WriteLine(currentToken + " <expression> -> ( <expression> )");
                        return true;
                    }
                }
                else
                {
                    addDebugInfo("Expression: An expression was expected inside the parenthesis." + Environment.NewLine);
                }
            }

            currentToken = storeToken;
            if(HC_object_expression())
            {
                Console.WriteLine(currentToken + " <expression> -> <object expression>");
                return true;
            }

            currentToken = storeToken;
            if (HC_array_expression())
            {
                Console.WriteLine(currentToken + " <expression> -> <array expression>");
                return true;
            }

            currentToken = storeToken;
            if (HC_list_expression())
            {
                Console.WriteLine(currentToken + " <expression> -> <list expression>");
                return true;
            }

            currentToken = storeToken;
            if (HC_scalar_expression(out value))
            {
                Console.WriteLine(currentToken + " <expression> -> <scalar expression>");
                return true;
            }

            return false;
        }

        private Boolean HC_float_constant(out Double value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_float_constant"); }
            value = 0.0;

            /*
            <int – constant>.<digit>* <exponent>
            <int – constant> e <int – constant>
            <sign>.<digit><digit>* <exponent>
             */

            if (matchTerminal(HighCTokenLibrary.FLOAT_LITERAL))
            {
                Double.TryParse(tokenList[currentToken - 1].Text, out value);

                int storeToken = currentToken;
                if (matchTerminal(HighCTokenLibrary.EXPONENT, true))
                {
                    if (matchTerminal(HighCTokenLibrary.INTEGER_LITERAL))
                    {
                        int shift;
                        int.TryParse(tokenList[currentToken - 1].Text, out shift);

                        while (shift > 0)
                        {
                            value = value * 10;
                            shift--;
                        }
                        Console.WriteLine(currentToken + " <float constant> -> " + tokenList[currentToken - 3].Text + HighCTokenLibrary.EXPONENT + tokenList[currentToken - 1].Text + " -> " + value);
                        return true;
                    }
                    else
                    {
                        addDebugInfo(HighCTokenLibrary.EXPONENT + ": An integer value was expected after the exponent." + Environment.NewLine);
                        currentToken = storeToken;
                    }
                }
                else
                {
                    currentToken = storeToken;
                }
                Console.WriteLine(currentToken + " <float constant> -> " + tokenList[currentToken - 1].Text);
                return true;
            }

            return false;
        }
        
        private Boolean HC_float_variable(out Double value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_float_variable"); }
            int storeToken = currentToken;
            value = 0.0;

            String identifier;

            if (HC_id(out identifier))
            {
                HighCData temp = currentEnvironment.getItem(identifier);

                if (temp == null)
                {
                    addDebugInfo("Integer Variable: The specified variable \"" + identifier + "\" could not be found." + Environment.NewLine);
                    return false;
                }
                else if (temp.readable == false)
                {
                    addDebugInfo("Float Variable: The specified variable \"" + identifier + "\" cannot be referenced." + Environment.NewLine);
                    errorFound = true;
                    stopProgram = true;
                    return false;
                }
                else if (temp.type == HighCTokenLibrary.FLOAT)
                {
                    value = (Double)temp.data;
                    Console.WriteLine(currentToken + " <integer variable> -> <id> -> " + identifier + " " + value);
                    return true;
                }
            }

            return false;
        }
        
        private Boolean HC_formal_parameters(out List<HighCParameter> parameters)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_formal_parameters"); }
            int storeToken = currentToken;

            /*
            ε
            <parameter list>
             */
             
            HighCParameter currentParameter;
            parameters = new List<HighCParameter>();

            if (HC_parameter(out currentParameter))
            {
                parameters.Add(currentParameter);
                storeToken = currentToken;
                while (matchTerminal(HighCTokenLibrary.COMMA))
                {
                    if (HC_parameter(out currentParameter))
                    {
                        parameters.Add(currentParameter);
                        storeToken = currentToken;
                    }
                    else
                    {
                        stopProgram = true;
                        errorFound = true;
                        return false;
                    }
                }
                currentToken = storeToken;
            }

            currentToken = storeToken;
            return true;
        }

        private Boolean HC_function()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_function"); }
            int storeToken = currentToken;
            /*
            <modifiers> func <id> <type parameters> ( <formal parameters> ) => <result> <block>
             */

            String functionName;
            Boolean pure;
            Boolean recursive;
            List<HighCParameter> parameters;
            String resultType;
            int startPosition;
            
            if (HC_modifiers(out pure, out recursive))
            {
                //This should only output an error message if a pure or recursive modifier was found
                if((pure==true || recursive == true) && matchTerminal(HighCTokenLibrary.FUNCTION, true)==false)
                {
                    //error
                    errorFound = true;
                    stopProgram = true;
                    return false;
                }

                if(matchTerminal(HighCTokenLibrary.FUNCTION))
                {
                    if(HC_id(out functionName))
                    {
                        if(HC_type_parameters()) //Will not be implemented yet
                        {
                            if(matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
                            {
                                if(HC_formal_parameters(out parameters))
                                {
                                    if (matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true))
                                    {
                                        if (matchTerminal(HighCTokenLibrary.ARROW, true))
                                        {
                                            if (HC_result(out resultType))
                                            {
                                                startPosition = currentToken;

                                                if (skipBlock())
                                                {
                                                    HighCFunctionDeclaration newFunction = new HighCFunctionDeclaration(functionName, pure, recursive, parameters, resultType, startPosition);
                                                    HighCData newData = new HighCData(HighCTokenLibrary.FUNCTION, newFunction);
                                                    if (currentEnvironment.contains(functionName) == false)
                                                    {
                                                        currentEnvironment.addNewItem(functionName, newData);
                                                        return true;
                                                    }
                                                    else
                                                    {
                                                        errorFound = true;
                                                        stopProgram = true;
                                                        addDebugInfo("Function declaration: \"" + functionName + "\" already exists." + Environment.NewLine);
                                                        return false;
                                                    }
                                                }
                                                else
                                                {
                                                    errorFound = true;
                                                    stopProgram = true;
                                                    addDebugInfo("Function declaration: \""+ functionName + "\" should be followed by a block of code (\"{ code }\")." + Environment.NewLine);
                                                    return false;
                                                }
                                            }
                                            else
                                            {
                                                errorFound = true;
                                                stopProgram = true;
                                                addDebugInfo("Function declaration: \"" + functionName + "\" should be followed by a return type or \"void\"." + Environment.NewLine);
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            errorFound = true;
                                            stopProgram = true;
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        errorFound = true;
                                        stopProgram = true;
                                        return false;
                                    }
                                }
                                else
                                {
                                    //error
                                    errorFound = true;
                                    stopProgram = true;
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            //error
                            errorFound = true;
                            stopProgram = true;
                            return false;
                        }
                    }
                    else
                    {
                        //error
                        errorFound = true;
                        stopProgram = true;
                        return false;
                    }
                }
            }
            else
            {
                //error
                errorFound = true;
                stopProgram = true;
                return false;
            }

            return false;
        }
        
        private Boolean HC_function_expression(out HighCFunctionDeclaration function)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_global_variable"); }
            int storeToken = currentToken;
            String identifier = "";
            HighCData data;
            function = null;

            /*
            <func – id>
            <method – id>
            <generic func – id> < <type list> >  Not implemented
            <object – name>.<method - id>
             */

            if(HC_id(out identifier))
            {
                if(currentEnvironment.contains(identifier))
                {
                    //FUNCTION
                    if(globalEnvironment.contains(identifier))
                    {
                        data=globalEnvironment.getItem(identifier);
                        if(data.type==HighCTokenLibrary.FUNCTION)
                        {
                            function = (HighCFunctionDeclaration)data.data;
                            return true;
                        }
                    }

                    //METHOD
                    //OBJECT METHOD
                }
                else
                {
                    //Error
                    return false;
                }
            }

            return false;
        }

        private Boolean HC_global_variable()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_global_variable"); }
            int storeToken = currentToken;

            /*
            global <declaration>
             */

            if (matchTerminal(HighCTokenLibrary.GLOBAL))
            {
                if (HC_declaration())
                {
                    Console.WriteLine(currentToken + " <global variable> -> " + HighCTokenLibrary.GLOBAL + " <declaration>");
                    return true;
                }
                else
                {
                    addDebugInfo(HighCTokenLibrary.GLOBAL + ": Expected a declaration statement." + Environment.NewLine);
                }
            }

            return false;
        }
        
        private Boolean HC_id(out String value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_id"); }
            value = "";

            /*
            <letter><alphanum>*
            ?<letter><alphanum>*
             */
             
            if(matchTerminal(HighCTokenLibrary.IDENTIFIER))
            {
                if (currentToken < tokenList.Count)
                {
                    value = tokenList[currentToken-1].Text;
                }
                
                Console.WriteLine(currentToken + " <id> -> " + value);

                return true;
            }
            else
            {
                //addDebugInfo(HighCTokenLibrary.IDENTIFIER + ": Expected an identifier beginning with a letter or question mark that does not match any reserved keywords." + Environment.NewLine);
            }

            return false;
        }

        private Boolean HC_if()
        {
            /*
             * if ( <bool – expr> ) <block> <else – if>* else <block>
             */
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_if"); }

            int storeToken = currentToken;
            Boolean blockEntered = false;

            if (matchTerminal(HighCTokenLibrary.IF) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
            {
                Boolean boolTerm1 = false;
                if (HC_boolean_expression(ref boolTerm1) &&
                    matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true))
                {
                    //-----IF-----
                    if (boolTerm1 == true)
                    {
                        blockEntered = true;
                        if (HC_block())
                        {
                            Console.WriteLine(currentToken + " <if> -> if ( <boolean expression> ) <block> <else_if>* else <block> -> if branch");
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (skipBlock() == false)
                    {
                        addDebugInfo(HighCTokenLibrary.IF + ": Must be followed by a block.  Example: \"" + HighCTokenLibrary.LEFT_CURLY_BRACKET + " " + HighCTokenLibrary.RIGHT_CURLY_BRACKET + "\"." + Environment.NewLine);
                        return false;
                    }

                    //-----ELSE IF*-----
                    storeToken = currentToken;
                    while (HC_else_if(ref blockEntered))
                    {
                        Console.WriteLine(currentToken + " <if> -> if ( <boolean expression> ) <block> <else_if>* else <block> -> else if branch");

                        storeToken = currentToken;
                    }

                    //-----ELSE-----
                    currentToken = storeToken;
                    if (matchTerminal(HighCTokenLibrary.ELSE))
                    {
                        if (blockEntered == false &&
                            HC_block())
                        {
                            Console.WriteLine(currentToken + " <if> -> if ( <boolean expression> ) <block> <else_if>* else <block> -> else branch");
                            return true;
                        }
                        else if (skipBlock() == false)
                        {
                            addDebugInfo(HighCTokenLibrary.ELSE + ": Must be followed by a block.  Example: \"" + HighCTokenLibrary.LEFT_CURLY_BRACKET + " " + HighCTokenLibrary.RIGHT_CURLY_BRACKET + "\"." + Environment.NewLine);
                            return false;
                        }
                    }
                    else
                    {
                        currentToken = storeToken;
                        addDebugInfo(HighCTokenLibrary.IF + ": Must include an \"" + HighCTokenLibrary.ELSE + "\" as part of its declaration." + Environment.NewLine);
                        return false;
                    }

                    if (blockEntered == false)
                    {
                        Console.WriteLine(currentToken + " <if> -> if ( <boolean expression> ) <block> <else_if>* else <block> -> no branch");
                    }

                    return true;
                }
                else
                {
                    addDebugInfo(HighCTokenLibrary.IF + ": A boolean expression was expected inside the parenthesis." + Environment.NewLine);
                }
            }

            return false;
        }

        private Boolean HC_initiated_variable(String expectedType, out String type, Boolean isConstant = false)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_initiated_variable"); }
            int storeToken = currentToken;
            type="";
            String variableName;
            String variableSubtype;
            HighCData constantValue;
            /*
            < var > = < constant >
            */
            
            if (HC_var(out variableName, out variableSubtype) &&
                matchTerminal(HighCTokenLibrary.EQUAL) &&
                HC_constant(out constantValue))
            {
                if (variableSubtype == "")
                {
                    if(currentEnvironment.directlyContains(variableName) ||
                        globalEnvironment.contains(variableName))
                    {
                        addDebugInfo("Declaration: The provided identifier \""+variableName+"\" already exists in this scope and cannot be redeclared." + Environment.NewLine);
                        return false;
                    }

                    HighCData variable = new HighCData(expectedType);
                    if(variable.setData(constantValue.type, constantValue.data)==false)
                    {
                        addDebugInfo("Declaration: \"" + variableName + "\" cannot be initialized with a value of type <"+constantValue.type+">."+ Environment.NewLine);
                        return false;
                    }

                    if (isConstant == true)
                    {
                        variable.writable = false;
                    }
                    
                    currentEnvironment.addNewItem(variableName, variable);
                    type = constantValue.type;
                    Console.WriteLine(currentToken + " <initiated variable> -> <variable> = <constant> ->" + variableSubtype + " " + variableName + " " + variable);
                    return true;
                }
                //Arrays
                //Lists
            }

            return false;
        }

        private Boolean HC_integer_constant(out Int64 value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_integer_constant"); }
            value = 0;
            /*
             * <sign><digit><digit>*
             */

            if (matchTerminal(HighCTokenLibrary.INTEGER_LITERAL))
            {
                Int64.TryParse(tokenList[currentToken - 1].Text, out value);
                Console.WriteLine(currentToken + " <integer constant> -> " + tokenList[currentToken - 1].Text);
                return true;
            }
            
            return false;
        }

        private Boolean HC_integer_expression(out Int64 integerValue)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_integer_expression"); }
            integerValue = 0;
            HighCData term1 = new HighCData(HighCTokenLibrary.INTEGER, 0);

            if (HC_arithmetic_expression_with_integer_tracking(ref term1))
            {
                if (term1.type == HighCTokenLibrary.INTEGER)
                {
                    integerValue = (Int64)term1.data;
                }
                else
                {
                    integerValue = (Int64)Math.Round((Double)term1.data);
                }
                Console.WriteLine(currentToken + " <integer expression> -> arithmetic expression with integer tracking" + " -> " + integerValue);
                return true;
            }
            return false;
        }


        private Boolean HC_integer_variable(out Int64 value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_integer_variable"); }

            int storeToken = currentToken;
            value = 0;
            /*
            <id>
            <string – variable> $ <slice>
            <array – variable> <subscript – expr>* [ <slice> ]
            <list – variable> @ <slice>
            <object – variable>.<variable>
             */

            String identifier;

            if (HC_id(out identifier))
            {
                HighCData temp = currentEnvironment.getItem(identifier);
                
                if(temp == null)
                {
                    addDebugInfo("Integer Variable: The specified variable \"" + identifier + "\" could not be found." + Environment.NewLine);
                    return false;
                }
                else if (temp.readable == false)
                {
                    addDebugInfo("Integer Variable: The specified variable \"" + identifier + "\" cannot be referenced." + Environment.NewLine);
                    errorFound = true;
                    stopProgram = true;
                    return false;
                }
                else if (temp.type==HighCTokenLibrary.INTEGER)
                {
                    value = (Int64)temp.data;
                    Console.WriteLine(currentToken + " <integer variable> -> <id> -> " + identifier+" "+ value);
                    return true;
                }
            }

            return false;
        }

        private Boolean HC_label(HighCData value, out Boolean matchFound, out String label)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_label"); }
            matchFound = false;
            int storeToken = currentToken;
            HighCData labelData;
            HighCData labelData2;
            label = "No Value";

            /*
             *  <constant>
                <constant> … <constant>
             */

            if (HC_constant(out labelData))
            {
                storeToken = currentToken;
                if (matchTerminal(HighCTokenLibrary.ELLIPSES))
                {
                    if (value.type != HighCTokenLibrary.BOOLEAN)
                    {
                        if (HC_constant(out labelData2))
                        {
                            if (value.type == labelData.type &&
                                value.type == labelData2.type)
                            {
                                if (value.type == HighCTokenLibrary.BOOLEAN)
                                {
                                    if (value.data == labelData.data ||
                                        value.data == labelData2.data)
                                    {
                                        Console.WriteLine(currentToken + " <label> -> <constant>" + " -> " + value + " match found.");
                                        matchFound = true;
                                        label = labelData.data + HighCTokenLibrary.ELLIPSES + labelData2.data;
                                        return true;
                                    }
                                    else
                                    {
                                        Console.WriteLine(currentToken + " <label> -> <constant>" + " -> " + value + " match not found.");
                                        label = labelData.data + HighCTokenLibrary.ELLIPSES + labelData2.data;
                                        return true;
                                    }
                                }

                                if (value.type == HighCTokenLibrary.INTEGER)
                                {
                                    if ((Int64)value.data >= (Int64)labelData.data &&
                                        (Int64)value.data <= (Int64)labelData2.data)
                                    {
                                        Console.WriteLine(currentToken + " <label> -> <constant>" + " -> " + value + " match found.");
                                        matchFound = true;
                                        label = labelData.data + HighCTokenLibrary.ELLIPSES + labelData2.data;
                                        return true;
                                    }
                                    else
                                    {
                                        Console.WriteLine(currentToken + " <label> -> <constant>" + " -> " + value + " match not found.");
                                        label = labelData.data + HighCTokenLibrary.ELLIPSES + labelData2.data;
                                        return true;
                                    }
                                }

                                if (value.type == HighCTokenLibrary.CHARACTER)
                                {
                                    if (Char.Parse((String)value.data) >= Char.Parse((String)labelData.data) &&
                                        Char.Parse((String)value.data) <= Char.Parse((String)labelData2.data))
                                    {
                                        Console.WriteLine(currentToken + " <label> -> <constant>" + " -> " + value + " match found.");
                                        matchFound = true;
                                        label = labelData.data + HighCTokenLibrary.ELLIPSES + labelData2.data;
                                        return true;
                                    }
                                    else
                                    {
                                        Console.WriteLine(currentToken + " <label> -> <constant>" + " -> " + value + " match not found.");
                                        label = labelData.data + HighCTokenLibrary.ELLIPSES + labelData2.data;
                                        return true;
                                    }
                                }
                                //ENUMERATION HERE
                            }
                            else
                            {
                                addDebugInfo(HighCTokenLibrary.ON + ": The label types (" + labelData.type + "), (" + labelData2.type + ") must match the type for \"" + HighCTokenLibrary.CHOICE + "\" (" + value.type + ")" + Environment.NewLine);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        addDebugInfo(HighCTokenLibrary.ON + ": " + HighCTokenLibrary.BOOLEAN+" types cannot utilize the range case specifier ("+HighCTokenLibrary.ELLIPSES+")."+ Environment.NewLine);

                        return false;
                    }
                }
                else
                {
                    currentToken = storeToken;
                    if (value.type == labelData.type)
                    {
                        Console.WriteLine("TESSSSSSSSTING " + value.data + " " + labelData.data);
                        if (labelData.data.Equals(value.data))
                        {
                            Console.WriteLine(currentToken + " <label> -> <constant>" + " -> " + value + " match found.");
                            matchFound = true;
                            label = (labelData.data).ToString();
                            return true;
                        }
                        else
                        {
                            Console.WriteLine(currentToken + " <label> -> <constant>" + " -> " + value + " match not found.");
                            label = (labelData.data).ToString();
                            return true;
                        }
                    }
                    else
                    {
                        addDebugInfo(HighCTokenLibrary.ON + ": The label type (" + labelData.type + ") must match the type for \"" + HighCTokenLibrary.CHOICE + "\" (" + value.type + ")" + Environment.NewLine);
                        label = "No Value";
                        return false;
                    }
                }
            }

            label = "No Value";
            return false;
        }

        private Boolean HC_loop()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_loop"); }

            /*
             * loop { <declaration>* <statement>* until ( <bool-expr> ) <statement>* }
             */
            int storeToken = currentToken;
            int loopStartToken = 0;

            Boolean continueLooping = true;

            if (matchTerminal(HighCTokenLibrary.LOOP) &&
                matchTerminal(HighCTokenLibrary.LEFT_CURLY_BRACKET, true))
            {
                Console.WriteLine(currentToken + " <loop> -> Starting Loop* }");

                while (continueLooping)
                {
                    //Declarations
                    loopStartToken = currentToken;
                    storeToken = currentToken;
                    while (HC_declaration())
                    {
                        storeToken = currentToken;
                    }
                    currentToken = storeToken;

                    //Statements
                    storeToken = currentToken;
                    while (HC_statement())
                    {
                        storeToken = currentToken;
                    }
                    currentToken = storeToken;

                    //Until
                    Boolean boolTerm1 = false;
                    if (matchTerminal(HighCTokenLibrary.UNTIL, true) &&
                        matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
                    {
                        if (HC_boolean_expression(ref boolTerm1))
                        {
                            if (matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true))
                            {
                                if (boolTerm1 == true)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            addDebugInfo(HighCTokenLibrary.UNTIL + ": A boolean expression was expected inside the parenthesis." + Environment.NewLine);
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    //Statements
                    //Statements
                    storeToken = currentToken;
                    while (HC_statement())
                    {
                        storeToken = currentToken;
                    }
                    currentToken = storeToken;

                    currentToken = loopStartToken;
                }

                currentToken = loopStartToken - 1;

                if (skipBlock() == false)
                {
                    return false;
                }

                currentToken--;
                if (matchTerminal(HighCTokenLibrary.RIGHT_CURLY_BRACKET, true))
                {
                    Console.WriteLine(currentToken + " <loop> -> loop { <declaration>* <statement>* until ( <boolean expression> ) <statement>* }");
                    return true;
                }
            }

            return false;
        }


        private Boolean HC_modifiers(out Boolean pure, out Boolean recursive)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_modifiers"); }
            int storeToken = currentToken;
            pure = false;
            recursive = false;

            /*
            ε
            pure
            recurs
            pure recurs
            recurs pure
            */

            if (matchTerminal(HighCTokenLibrary.PURE))
            {
                pure = true;
                storeToken = currentToken;
                if(matchTerminal(HighCTokenLibrary.RECURSIVE))
                {
                    recursive = true;
                    Console.WriteLine(currentToken + " <modifiers>" + " -> " + HighCTokenLibrary.PURE + " "+ HighCTokenLibrary.RECURSIVE);
                    return true;
                }
                else
                {
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <modifiers>" + " -> " + HighCTokenLibrary.PURE);
                    return true;
                }
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.RECURSIVE))
            {
                recursive = true;
                storeToken = currentToken;
                if (matchTerminal(HighCTokenLibrary.PURE))
                {
                    pure = true;
                    Console.WriteLine(currentToken + " <modifiers>" + " -> " + HighCTokenLibrary.PURE + " " + HighCTokenLibrary.RECURSIVE);
                    return true;
                }
                else
                {
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <modifiers>" + " -> " + HighCTokenLibrary.RECURSIVE);
                    return true;
                }
            }

            currentToken = storeToken;
            Console.WriteLine(currentToken + " <modifiers>" + " -> null");
            return true;
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
            HighCData value = null;
            stringBuffer = "";
            Int64 term1;
            Int64 term2;

            /*
            <scalar – expr>
            <scalar – expr> : <int - expr>
            <arith – expr> : <int – expr>.<int - expr>
            endl
             */

            //Ensures Minimum Length
            if (HC_scalar_expression(out value) &&
                matchTerminal(HighCTokenLibrary.COLON) &&
                HC_integer_expression(out term1))
            {
                stringBuffer = value.data.ToString();
                if (stringBuffer.Length < term1)
                {
                    stringBuffer = stringBuffer.PadRight((int)term1);
                }
                Console.WriteLine(currentToken + " <out element> -> <scalar expression>:<integer expression>" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (HC_scalar_expression(out value))
            {
                stringBuffer = value.data.ToString();
                Console.WriteLine(currentToken + " <out element> -> <scalar expression>" + " -> " + stringBuffer);
                return true;
            }

            currentToken = storeToken;
            if (HC_arithmetic_expression(ref value) &&
                matchTerminal(HighCTokenLibrary.COLON) &&
                HC_integer_expression(out term1) &&
                matchTerminal(HighCTokenLibrary.PERIOD) &&
                HC_integer_expression(out term2))
            {
                stringBuffer = Math.Round((Double)value.data, (int)term2).ToString();
                if (stringBuffer.Length < term1)
                {
                    stringBuffer = stringBuffer.PadRight((int)term1);
                }
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
            String stringBuffer = "";
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

            if (needAnotherOut == true)
            {
                addDebugInfo(HighCTokenLibrary.OUT + ": another element was expected after the comma." + Environment.NewLine);
                return false;
            }

            if (atLeastOneFound == false)
            {
                addDebugInfo(HighCTokenLibrary.OUT + ": at least one element was expected." + Environment.NewLine);
                return false;
            }
            else
            {
                if (needAnotherOut == true)
                {
                    Console.WriteLine(currentToken + " <output> -> <out element>,...,<out element>" + " -> " + stringBuffer);
                }
                else
                {
                    Console.WriteLine(currentToken + " <output> -> <out element>" + " -> " + stringBuffer);
                }

                consoleText += stringBuffer;
                return true;
            }
        }
        
        private Boolean HC_parameter(out HighCParameter parameter)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_parameter"); }
            int storeToken = currentToken;

            /*
            <direction> <type> <id> <subscript – parameter>*
            <direction> <type> <id> @
            */

            String id = "";
            String type = "";
            String subtype = "";
            Boolean inAllowed;
            Boolean outAllowed;
            List<String> parameterIDList = new List<String>();
            parameter = null;

            if (HC_direction(out inAllowed, out outAllowed))
            {
                if(HC_type(out type))
                {
                    if(HC_id(out id))
                    {
                        storeToken = currentToken;
                        if(matchTerminal(HighCTokenLibrary.AT_SIGN))
                        {
                            subtype = HighCTokenLibrary.LIST;
                            parameter = new HighCParameter(id, type, subtype, inAllowed, outAllowed, parameterIDList);
                            return true;
                        }

                        currentToken = storeToken;
                        
                        String currentID;
                        while(HC_subscript_parameter(out currentID))
                        {
                            subtype = HighCTokenLibrary.ARRAY;
                            storeToken = currentToken;
                            parameterIDList.Add(currentID);
                        }

                        currentToken = storeToken;

                        parameter = new HighCParameter(id, type, subtype, inAllowed, outAllowed, parameterIDList);
                        return true;
                    }
                    else
                    {
                        //error
                        return false;
                    }
                }
                else
                {
                    //error
                    return false;
                }
            }
            else
            {
                //error
                return false;
            }
        }

        private Boolean HC_relational_expression(ref Boolean value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_relational_expression"); }
            int storeToken = currentToken;

            /*
            <arith – expr> <rel – op> <arith – expr>
            <string – expr> <rel – op> <string – expr>
            <char – expr> <rel – op> <char – expr>
            <enum – expr> <rel – op> <enum – expr>

            <bool – expr> <eq – op> <bool – expr> moved to <boolean expression>
            <array – expr> <eq – op> <bool – expr>
            <list – expr> <eq – op> <list – expr>
            <object – expr> <eq – op> <object – expr>
            <object – expr> instof <class name>
             */

            String opType;
            HighCData value1 = new HighCData(HighCTokenLibrary.INTEGER,0);
            HighCData value2 = new HighCData(HighCTokenLibrary.INTEGER, 0); ;
            Double term1 = 0.0;
            Double term2 = 0.0;
            String stringTerm1 = "";
            String stringTerm2 = "";
            Boolean boolTerm2 = false;

            if (HC_arithmetic_expression(ref value1) &&
                HC_relational_op(out opType) &&
                HC_arithmetic_expression(ref value2))
            {
                if (value1.type == HighCTokenLibrary.FLOAT ||
                    value2.type == HighCTokenLibrary.FLOAT)
                {
                    term1 = (Double)value1.data;
                    term2 = (Double)value2.data;
                    if (opType == HighCTokenLibrary.EQUAL) { value = term1 == term2; }
                    else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = term1 != term2; }
                    else if (opType == HighCTokenLibrary.LESS_THAN) { value = term1 < term2; }
                    else if (opType == HighCTokenLibrary.GREATER_THAN) { value = term1 > term2; }
                    else if (opType == HighCTokenLibrary.LESS_THAN_EQUAL) { value = term1 <= term2; }
                    else if (opType == HighCTokenLibrary.GREATER_THAN_EQUAL) { value = term1 >= term2; }
                    Console.WriteLine(currentToken + " <relational expression> -> <arithmetic expression><relational op><arithmetic expression>" + " -> " + value);
                }
                else
                {
                    Int64 intTerm1 = (Int64)value1.data;
                    Int64 intTerm2 = (Int64)value2.data;
                    if (opType == HighCTokenLibrary.EQUAL) { value = intTerm1 == intTerm2; }
                    else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = intTerm1 != intTerm2; }
                    else if (opType == HighCTokenLibrary.LESS_THAN) { value = intTerm1 < intTerm2; }
                    else if (opType == HighCTokenLibrary.GREATER_THAN) { value = intTerm1 > intTerm2; }
                    else if (opType == HighCTokenLibrary.LESS_THAN_EQUAL) { value = intTerm1 <= intTerm2; }
                    else if (opType == HighCTokenLibrary.GREATER_THAN_EQUAL) { value = intTerm1 >= intTerm2; }
                    Console.WriteLine(currentToken + " <relational expression> -> <arithmetic expression><relational op><arithmetic expression>" + " -> " + value);
                }
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

            if (HC_equality_op(out opType))
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

            int storeToken = currentToken;
            
            while (HC_compiler_directive()) { storeToken = currentToken; }
            currentToken = storeToken;

            storeToken = currentToken;
            while (HC_user_constant()) { storeToken = currentToken; }
            currentToken = storeToken;
            
            storeToken = currentToken;
            while (HC_global_variable()) { storeToken = currentToken; }
            currentToken = storeToken;
            
            storeToken = currentToken;
            while (HC_class()) { storeToken = currentToken; }
            currentToken = storeToken;

            storeToken = currentToken;
            while (HC_function()) { storeToken = currentToken; }
            currentToken = storeToken;

            currentEnvironment = new HighCEnvironment(globalEnvironment);

            if (matchTerminal(HighCTokenLibrary.MAIN, true))
            {
                if (HC_block())
                {
                    Console.WriteLine(currentToken + " <program>");
                    return true;
                }
            }
            else
            {
                addDebugInfo("Expected to find a \"" + HighCTokenLibrary.MAIN + "\"" + Environment.NewLine, false);
            }

            if (stopProgram == true)
            {
                return true;
            }
            return false;
        }
        
        private Boolean HC_result(out String type)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_result"); }
            int storeToken = currentToken;

            /*
            void
            <return type>
             */

            type = "";

            if(matchTerminal(HighCTokenLibrary.VOID))
            {
                type = HighCTokenLibrary.VOID;
                Console.WriteLine(currentToken + " <result> -> " + type);
                return true;
            }

            currentToken = storeToken;
            if(HC_return_type(out type))
            {
                Console.WriteLine(currentToken + " <result> -> " + type);
                return true;
            }

            return false;
        }
        
        private Boolean HC_return_type(out String type)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_scalar_expression"); }
            int storeToken = currentToken;

            /*
            <type> <return – subscript>*
            <type> @
             */

            type = "";

            if(HC_type(out type))
            {
                return true;
            }

            return false;
        }

        private Boolean HC_scalar_expression(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_scalar_expression"); }
            int storeToken = currentToken;
            
            HighCData term1 = null;
            Boolean boolTerm = false;
            String stringTerm = "";

            if (HC_boolean_expression(ref boolTerm) == true)
            {
                value = new HighCData(HighCTokenLibrary.BOOLEAN, boolTerm);
                Console.WriteLine(currentToken + " <scalar expression> -> <boolean expression>" + " -> " + value.ToString());
                return true;
            }

            currentToken = storeToken;
            if (HC_arithmetic_expression(ref term1) == true)
            {
                value = term1;
                Console.WriteLine(currentToken + " <scalar expression> -> <arithmetic expression>" + " -> " + value.ToString());
                return true;
            }
            currentToken = storeToken;
            if (HC_string_expression(out stringTerm) == true)
            {
                value = new HighCData(HighCTokenLibrary.STRING, stringTerm);
                Console.WriteLine(currentToken + " <scalar expression> -> <string expression>" + " -> " + value.ToString());
                return true;
            }

            currentToken = storeToken;
            if (HC_character_expression(out stringTerm) == true)
            {
                value = new HighCData(HighCTokenLibrary.STRING, stringTerm);
                Console.WriteLine(currentToken + " <scalar expression> -> <character expression>" + " -> " + value.ToString());
                return true;
            }

            currentToken = storeToken;
            if (HC_enum_expression() == true)
            {
                value = new HighCData(HighCTokenLibrary.ENUMERATION, stringTerm);
                Console.WriteLine(currentToken + " <scalar expression> -> <enum expression>" + " -> " + value.ToString());
                return true;
            }

            value = null;
            return false;
        }

        private Boolean HC_scalar_type(out String type)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_scalar_type"); }
            int storeToken = currentToken;
            type = "";

            /*
            <discrete type>
            STRING
            FLOAT
            FLOAT: <positive – int – constant>
             */

            if (HC_discrete_type(out type))
            {
                Console.WriteLine(currentToken + " <scalar type> -> <discrete type> ->" + type);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.STRING))
            {
                type = HighCTokenLibrary.STRING;
                Console.WriteLine(currentToken + " <scalar type> -> " + type);
                return true;
            }

            currentToken = storeToken;
            Int64 intTerm1;
            if (matchTerminal(HighCTokenLibrary.FLOAT) &&
                matchTerminal(HighCTokenLibrary.COLON) &&
                HC_integer_constant(out intTerm1))
            {
                type = HighCTokenLibrary.FLOAT + HighCTokenLibrary.COLON + intTerm1.ToString();
                if (intTerm1 >= 0)
                {
                    Console.WriteLine(currentToken + " <scalar type> -> " + type);
                }
                else
                {
                    addDebugInfo(HighCTokenLibrary.FLOAT + HighCTokenLibrary.COLON + ": The number of decimals to show must be at least 0." + Environment.NewLine);
                    return false;
                }
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.FLOAT))
            {
                type = HighCTokenLibrary.FLOAT;
                Console.WriteLine(currentToken + " <scalar type> -> " + type);
                return true;
            }

            return false;
        }

        private Boolean HC_statement()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_statement"); }
            int storeToken = currentToken;

            if (stopProgram == true)
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

            String sBuffer1 = "";
            String sBuffer2 = "";

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

            /*
            <string – expr> $ <int – expr> … <int – expr>
            <char – expr>
            ( <string – expr> )
            <string – variable>
            <string – const>
            <string – func call>
             */

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
                if (HC_string_variable(out stringBuffer) == true)
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
            if (derivationFound)
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
                    int length = (int)(term2 - term1) + 1;
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

        private Boolean HC_string_variable(out String value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_string_variable"); }

            String identifier;
            value = "";
            if (HC_id(out identifier))
            {
                HighCData temp = currentEnvironment.getItem(identifier);

                if (temp == null)
                {
                    addDebugInfo("String Variable: The specified variable \"" + identifier + "\" could not be found." + Environment.NewLine);
                    return false;
                }
                else if (temp.readable == false)
                {
                    addDebugInfo("String Variable: The specified variable \"" + identifier + "\" cannot be referenced." + Environment.NewLine);
                    errorFound = true;
                    stopProgram = true;
                    return false;
                }
                else if (temp.type == HighCTokenLibrary.STRING)
                {
                    value = (String)temp.data;
                    Console.WriteLine(currentToken + " <string variable> -> <id> -> " + identifier + " " + value);
                    return true;
                }
            }

            return false;
        }

        private Boolean HC_subscript(out int value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_subscript"); }
            /*
            [ <positive – int – constant> ]
             */

            Int64 term1;
            value = 0;

            if(matchTerminal(HighCTokenLibrary.LEFT_SQUARE_BRACKET))
            {
                if (HC_integer_constant(out term1) &&
                    term1>0)
                {
                    if (matchTerminal(HighCTokenLibrary.RIGHT_SQUARE_BRACKET, true))
                    {
                        value = (int)term1;
                        Console.WriteLine(currentToken + " <subscript> -> [positive integer constant] ->" + term1);
                        return true;
                    }
                }
                else
                {
                    addDebugInfo(HighCTokenLibrary.ARRAY+": Expected a positive integer dimension specifier." + Environment.NewLine);
                }
            }

            return false;
        }

        private Boolean HC_subscript_parameter(out String id)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_subscript_parameter"); }
            int storeToken = currentToken;
            id = "";

            /*
            [ <id> ]
             */

            if (matchTerminal(HighCTokenLibrary.LEFT_SQUARE_BRACKET))
            {
                if (HC_id(out id))
                {
                    if (matchTerminal(HighCTokenLibrary.RIGHT_SQUARE_BRACKET, true))
                    {
                        return true;
                    }
                    else
                    {
                        //error
                        return false;
                    }
                }
                else
                {
                    //error
                    return false;
                }
            }

            return false;
        }

        private Boolean HC_type(out String type)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_type"); }
            int storeToken = currentToken;
            type = "";
            /*
            <scalar type>
            <class name>
            <type – parameter id> ??
             */

            if (HC_scalar_type(out type))
            {
                Console.WriteLine(currentToken + " <type> -> <scalar type> ->" + type);
                return true;
            }

            currentToken = storeToken;
            if (HC_class_name())
            {
                Console.WriteLine(currentToken + " <type> -> <class name> ->" + type);
                return true;
            }

            return false;
        }
        
        private Boolean HC_type_parameters()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_type_parameters"); }
            int storeToken = currentToken;

            /*
            ε
            < <type – specifier list> >
             */

            //This functionality is not implemented in this release and will be added at a future date.
            return true;
            /*
            if(matchTerminal(HighCTokenLibrary.LESS_THAN,true))
            {
                if(HC_type_specifier())
                {
                    storeToken = currentToken;
                    while(matchTerminal(HighCTokenLibrary.COMMA))
                    {
                        if(HC_type_specifier())
                        {
                            storeToken = currentToken;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    currentToken = storeToken;

                    if(matchTerminal(HighCTokenLibrary.GREATER_THAN,true))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
            */
        }
        
        private Boolean HC_type_specifier()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_type_specifier"); }
            int storeToken = currentToken;

            /*
            <type – group> <type – parameter id>
             */

            String parameterName;

            if(HC_type_group())
            {
                if(HC_id(out parameterName))
                {

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return false;
        }

        private Boolean HC_user_constant()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_user_constant"); }
            int storeToken = currentToken;

            /*
            enum <id> = { <id list> }
            const <type> <initiated – var list>
             */

            Boolean atLeastOneFound = false;
            Boolean needAnother = false;
            String type = "";
            String type2 = "";

            if(matchTerminal(HighCTokenLibrary.ENUMERATION))
            {
                return false;
            }

            currentToken = storeToken;

            if (matchTerminal(HighCTokenLibrary.CONSTANT))
            {
                if (HC_type(out type))
                {
                    storeToken = currentToken;
                    while (HC_initiated_variable(type, out type2, true))
                    {
                        storeToken = currentToken;
                        atLeastOneFound = true;
                        needAnother = false;

                        if (type.Contains(type2) == false)
                        {
                            addDebugInfo("Constant Declaration" + ": The type of the constant (\"" + type2 + "\") does not match the type indicated (\"" + type + "\")." + Environment.NewLine);
                            return false;
                        }

                        //Match types

                        if (matchTerminal(HighCTokenLibrary.COMMA))
                        {
                            needAnother = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                    currentToken = storeToken;

                    if (needAnother == true)
                    {
                        addDebugInfo("Constant Declaration" + ": another element was expected after the comma." + Environment.NewLine);
                        return false;
                    }

                    if (atLeastOneFound == false)
                    {
                        addDebugInfo("Constant Declaration" + ": at least one declaration (\"<identifier> = <value>\") was expected after the type." + Environment.NewLine);
                        return false;
                    }

                    Console.WriteLine(currentToken + " <declaration> -> const <type><initiated variable>,...,<initiated variable> -> " + type);

                    return true;
                }
                else
                {
                    addDebugInfo(HighCTokenLibrary.CREATE + ": Expected a data or class type." + Environment.NewLine);
                }
            }

            return false;
        }

        private Boolean HC_var(out String identifier, out String subtype)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_var"); }
            int storeToken = currentToken;
            identifier = "";
            subtype = "";

            /*
                <id> <subscript>*
                <id> @
             */

            if (HC_id(out identifier) &&
             matchTerminal(HighCTokenLibrary.AT_SIGN))
            {
                subtype = HighCTokenLibrary.LIST;
                Console.WriteLine(currentToken + " <var> -> <id> @ ->" + identifier+" "+ subtype);
                return true;
            }

            currentToken = storeToken;
            int dimensionSize = 0;
            if (HC_id(out identifier)&&
                HC_subscript(out dimensionSize))
            {
                storeToken = currentToken;
                subtype = HighCTokenLibrary.ARRAY +"[" + dimensionSize + "]";
                while (HC_subscript(out dimensionSize))
                {
                    subtype += "[" + dimensionSize + "]";
                    storeToken = currentToken;
                }
                Console.WriteLine(currentToken + " <var> -> <id> <subscript>* ->" + identifier + " " + subtype);
                currentToken = storeToken;
                return true;
            }

            currentToken = storeToken;
            if (HC_id(out identifier))
            {
                Console.WriteLine(currentToken + " <var> -> <id> ->" + identifier + " " + subtype);
                return true;
            }

            return false;
        }
        
        private Boolean HC_variable(out String identifier)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_variable"); }
            int storeToken = currentToken;
            identifier = "";

            /*
            <id>
            <string – variable> $ <slice>
            <array – variable> <subscript – expr>* [ <slice> ]
            <list – variable> @ <slice>
            <object – variable>.<variable>
             */

            if(HC_id(out identifier))
            {
                Console.WriteLine(currentToken + " <variable> -> <id> ->" + identifier);
                return true;
            }

            return false;
        }
        
        private Boolean HC_void_call()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_void_call"); }
            int storeToken = currentToken;

            /*
            call <void func – expr> ( <expr list>* )
             */

            HighCFunctionDeclaration function;
            HighCEnvironment functionEnvironment = new HighCEnvironment(globalEnvironment);
            HighCEnvironment storeEnvironment;
            List<HighCParameter> parameters;
            List<HighCData> parameterData = new List<HighCData>();
            List<String> outIdentifiers = new List<String>();
            Boolean firstParameter = true;

            if(matchTerminal(HighCTokenLibrary.CALL))
            {
                if (HC_function_expression(out function) &&
                    function.returnType == HighCTokenLibrary.VOID)
                {
                    if (matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
                    {
                        //Look for parameters
                        parameters = function.parameters;
                        
                        foreach (HighCParameter parameter in parameters)
                        {
                            HighCData value;

                            if(firstParameter==true)
                            {
                                firstParameter = false;
                            }
                            else if(firstParameter==false &&
                               matchTerminal(HighCTokenLibrary.COMMA,true)==false)
                            {
                                stopProgram = true;
                                errorFound = true;
                                return false;
                            }
                            
                            if (parameter.inAllowed == true &&
                                parameter.outAllowed == false)
                            {
                                if (HC_expression(out value))
                                {
                                    if (parameter.type == value.type)
                                    {
                                        HighCData newValue = value.Clone();
                                        newValue.writable = parameter.outAllowed;
                                        newValue.readable = parameter.inAllowed;
                                        parameterData.Add(newValue);
                                    }
                                    else
                                    {
                                        stopProgram = true;
                                        errorFound = true;
                                        addDebugInfo("Call: \"" + function.identifier + "\" the type of the parameter provided <"+value.type+"> does not match the expected type <"+parameter.type+">."+ Environment.NewLine);
                                        return false;
                                    }
                                }
                                else
                                {
                                    stopProgram = true;
                                    errorFound = true;
                                    addDebugInfo("Call: \"" + function.identifier + "\" no value specified for parameter of type <" + value.type + ">." + Environment.NewLine);
                                    return false;
                                }
                            }
                            else
                            {
                                String identifier;
                                if (HC_variable(out identifier))
                                {
                                    if (currentEnvironment.contains(identifier))
                                    {
                                        value = currentEnvironment.getItem(identifier);
                                        HighCData newValue = value.Clone();

                                        if (parameter.type == value.type)
                                        {
                                            newValue.writable = parameter.outAllowed;
                                            newValue.readable = parameter.inAllowed;
                                            parameterData.Add(newValue);
                                            outIdentifiers.Add(identifier);
                                        }
                                        else
                                        {
                                            stopProgram = true;
                                            errorFound = true;
                                            addDebugInfo("Call \"" + function.identifier + "\": the specified identifier \"" + identifier + "\" with type <"+ value.type+"> does not match the expected parameter type <"+parameter.type+">." + Environment.NewLine);
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        stopProgram = true;
                                        errorFound = true;
                                        addDebugInfo("Call \"" + function.identifier + "\": the specified identifier \"" + identifier + "\" could not be found." + Environment.NewLine);
                                        return false;
                                    }
                                }
                                else
                                {
                                    stopProgram = true;
                                    errorFound = true;
                                    addDebugInfo("Call \"" + function.identifier + "\": expected an identifier of type <" + parameter.type + "> as a parameter." + Environment.NewLine);
                                    return false;
                                }
                            }
                        }
                        
                        //Perform Function
                        storeToken = currentToken;
                        if (function != null)
                        {
                            currentToken = function.blockTokenPosition;
                            storeEnvironment = currentEnvironment;
                            currentEnvironment = functionEnvironment;
                            
                            int i = 0;
                            foreach(HighCParameter parameter in parameters)
                            {
                                currentEnvironment.addNewItem(parameter.identifier, parameterData[i]);
                                i++;
                            }

                            if (HC_block())
                            {

                            }

                            foreach (HighCParameter parameter in parameters)
                            {
                                i = 0;
                                if(parameter.outAllowed==true)
                                {
                                    HighCData temp;
                                    if (storeEnvironment.changeItem(outIdentifiers[i], currentEnvironment.getItem(parameter.identifier), out temp) == false)
                                    {
                                        stopProgram = true;
                                        errorFound = true;
                                        addDebugInfo("Call: \"" + outIdentifiers[i] + "\" could not be found or is not a variable." + Environment.NewLine);
                                        return false;
                                    }
                                    i++;
                                }
                            }

                            currentEnvironment = storeEnvironment;
                            currentToken = storeToken;
                        }
                        else
                        {
                            //ERROR
                            return false;
                        }

                        if (matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true))
                        {
                            Console.WriteLine(currentToken + " <void call> -> call <void func – expr> ( <expr list>* )");
                            return true;
                        }
                    }
                    else
                    {
                        stopProgram = true;
                        errorFound = true;
                        return false;
                    }
                }
                else
                {
                    stopProgram = true;
                    errorFound = true;
                    addDebugInfo("Call: \"" + function.identifier + "\" could not be found or is not a void function." + Environment.NewLine);
                    return false;
                }
            }

            return false;
        }

        private Boolean _______________Unimplemented_Functions_______________() { return false; }

        private Boolean HC_array_expression() { return false; }
        private Boolean HC_body() { return false; }
        private Boolean HC_boolean_function_call(ref Boolean value) { value = false; return false; }
        private Boolean HC_character() { return false; }
        private Boolean HC_character_function_call(out String stringBuffer) { stringBuffer = ""; return false; }
        private Boolean HC_class() { return false; }
        private Boolean HC_class_name() { return false; }
        private Boolean HC_compiler_directive() { return false; }
        private Boolean HC_data_field() { return false; }
        private Boolean HC_dir() { return false; }
        private Boolean HC_discrete_constant() { return false; }
        private Boolean HC_element() { return false; }
        private Boolean HC_element_constant() { return false; }
        private Boolean HC_element_expression() { return false; }
        private Boolean HC_element_or_list() { return false; }
        private Boolean HC_enum_expression() { return false; }
        private Boolean HC_enum_type() { return false; }
        private Boolean HC_field_assign() { return false; }
        private Boolean HC_field_constant() { return false; }
        private Boolean HC_float_function_call(out Double value) { value = 0.0; return false; }
        private Boolean HC_function_call() { return false; }
        private Boolean HC_initiated_field() { return false; }
        private Boolean HC_input() { return false; }
        private Boolean HC_integer_function_call(out Int64 value) { value = 0; return false; }
        private Boolean HC_iterator() { return false; }
        private Boolean HC_list_command() { return false; }
        private Boolean HC_list_constant() { return false; }
        private Boolean HC_list_expression() { return false; }
        private Boolean HC_method() { return false; }
        private Boolean HC_object_constant() { return false; }
        private Boolean HC_object_expression() { return false; }
        private Boolean HC_option() { return false; }
        private Boolean HC_parent() { return false; }
        private Boolean HC_prompt_variable() { return false; }
        private Boolean HC_qualifier() { return false; }
        private Boolean HC_return() { return false; }
        private Boolean HC_return_subscript() { return false; }
        private Boolean HC_sign() { return false; }
        private Boolean HC_slice() { return false; }
        private Boolean HC_string_function_call() { return false; }
        private Boolean HC_subscript_expression() { return false; }
        
        private Boolean HC_type_assignment() { return false; }
        private Boolean HC_type_group() { return false; }
    }

    class HighCEnvironment
    {
        HighCEnvironment parent;

        public List<String> identifiers = new List<String>();
        public List<HighCData> data = new List<HighCData>();
        public List<String> types = new List<String>();
        public List<HighCData> dataStorage = new List<HighCData>();

        public HighCEnvironment(){ }

        public HighCEnvironment(HighCEnvironment newParent)
        {
            parent = newParent;
        }

        public Boolean contains(String identifier)
        {
            if(identifiers.Contains(identifier))
            {
                return true;
            }

            if(parent!=null &&
               parent.contains(identifier))
            {
                return true;
            }

            return false;
        }

        public Boolean directlyContains(String identifier)
        {
            if (identifiers.Contains(identifier))
            {
                return true;
            }

            return false;
        }

        public void addNewItem(String identifier, HighCData newItem)
        {
            identifiers.Add(identifier);
            data.Add(newItem);

            if (newItem.type != "Keyword")
            {
                Console.WriteLine("Adding new item to environment: " + identifier + " " + newItem.ToString());
            }
        }
        
        public Boolean changeItem(String identifier, HighCData newValue, out HighCData currentItem)
        {
            currentItem = getItem(identifier);

            if(currentItem!=null && newValue != null && currentItem.writable==true)
            {
                if (currentItem.type == HighCTokenLibrary.INTEGER && newValue.isNumericType())
                {
                    if(newValue.type==HighCTokenLibrary.FLOAT)
                    {
                        Double temp = (Double)newValue.data;
                        currentItem.data = (Int64)Math.Round(temp);
                    }
                    else
                    {
                        currentItem.data = (Int64)newValue.data;
                    }
                }
                else if(currentItem.type == HighCTokenLibrary.FLOAT && newValue.isNumericType())
                {
                    currentItem.data = (Double)newValue.data;
                }
                else if (currentItem.type == HighCTokenLibrary.STRING && 
                    (newValue.type == HighCTokenLibrary.STRING || newValue.type == HighCTokenLibrary.CHARACTER))
                {
                    currentItem.data = (String)newValue.data;
                }
                else if (currentItem.type == HighCTokenLibrary.CHARACTER)
                {
                    currentItem.data = (String)newValue.data;
                }
                else if (currentItem.type == HighCTokenLibrary.BOOLEAN)
                {
                    currentItem.data = (Boolean)newValue.data;
                }
                else
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        public HighCData getItem(String identifier)
        {
            int location = identifiers.IndexOf(identifier);

            Console.WriteLine("Looking for item: " + identifier + " " + location);

            if (location > -1)
            {
                return data[location];
            }
            else if (parent != null)
            {
                return parent.getItem(identifier);
            }

            return null;
        }
    }

    class HighCData
    {
        //TYPES
        public const String INT_TYPE = HighCTokenLibrary.INTEGER;
        public const String FLOAT_TYPE = HighCTokenLibrary.FLOAT;
        public const String BOOLEAN_TYPE = HighCTokenLibrary.BOOLEAN;
        public const String CHARACTER_TYPE = HighCTokenLibrary.CHARACTER;
        public const String STRING_TYPE = HighCTokenLibrary.STRING;
        public const String FUNCTION_DECLARATION_TYPE = HighCTokenLibrary.FUNCTION;
        //SUB-TYPES
        public const String NO_SUBTYPE = "";
        public const String VARIABLE_SUBTYPE = HighCTokenLibrary.VARIABLE;
        public const String ARRAY_SUBTYPE = HighCTokenLibrary.ARRAY;
        public const String LIST_SUBTYPE = HighCTokenLibrary.LIST;

        public Object data;
        public String type;
        public String subtype;
        public Boolean writable;
        public Boolean readable;

        public HighCData(String newType)
        {
            type = newType;
            subtype = "";
            writable = true;
            readable = true;
        }

        public HighCData(String newType, Object newValue, Boolean isWriteEnabled=true, Boolean isReadEnabled=true, String newSubtype = NO_SUBTYPE)
        {
            type = newType;
            subtype = newSubtype;
            data = newValue;
            writable = isWriteEnabled;
            readable = isReadEnabled;
        }

        public override String ToString()
        {
            if(data==null)
            {
                return type + " " + null;
            }
            return type + " " + data.ToString();
        }

        public Boolean isNumericType()
        {
            if(type==HighCTokenLibrary.INTEGER ||
                type == HighCTokenLibrary.FLOAT)
            {
                return true;
            }

            return false;
        }

        public Boolean setValue(Int64 newValue)
        {
            if(type == INT_TYPE)
            {
                data = newValue;
                return true;
            }
            else if(type == FLOAT_TYPE)
            {
                data = (Double)newValue;
            }

            return false;
        }

        public Boolean setValue(Double newValue)
        {
            if (type == INT_TYPE)
            {
                data = (Int64)Math.Round(newValue);
                return true;
            }
            else if (type == FLOAT_TYPE)
            {
                data = newValue;
            }

            return false;
        }

        public Boolean setValue(String newValue)
        {
            if (type == STRING_TYPE)
            {
                data = newValue;
                return true;
            }
            else if (type == CHARACTER_TYPE &&
                newValue.Length==1)
            {
                data = newValue;
                return true;
            }

            return false;
        }

        public Boolean setValue(Char newValue)
        {
            if (type == STRING_TYPE)
            {
                data = ""+newValue;
                return true;
            }
            else if (type == CHARACTER_TYPE)
            {
                data = "" + newValue;
                return true;
            }

            return false;
        }

        public Boolean setValue(Boolean newValue)
        {
            if (type == BOOLEAN_TYPE)
            {
                data = newValue;
                return true;
            }

            return false;
        }

        public Boolean setData(String valueType, Object newValue)
        {
            
            switch(valueType)
            {
                case INT_TYPE: return setValue((Int64)newValue);
                case FLOAT_TYPE: return setValue((Double)newValue);
                case BOOLEAN_TYPE: return setValue((Boolean)newValue);
                case STRING_TYPE: return setValue((String)newValue);
                case CHARACTER_TYPE: return setValue((String)newValue);
                default:
                    break;
            }
            
            return false;
        }

        public HighCData Clone()
        {
            Object newValue=null;

            if (data == null) { }
            else
            {
                if (subtype == "")
                {
                    switch(type)
                    {
                        case INT_TYPE:
                            Int64 temp = ((Int64)data);
                            newValue = temp;
                            break;
                        case FLOAT_TYPE:
                            Double temp2 = ((Double)data);
                            newValue = temp2;
                            break;
                        case BOOLEAN_TYPE:
                            Boolean temp3 = ((Boolean)data);
                            newValue = temp3;
                            break;
                        case CHARACTER_TYPE:
                            String temp4 = ((String)data);
                            newValue = temp4;
                            break;
                        case STRING_TYPE:
                            newValue = ((String)data).Substring(0);
                            break;
                        case FUNCTION_DECLARATION_TYPE:
                            newValue = ((HighCFunctionDeclaration)data).Clone();
                            break;
                        default:
                            newValue = null;
                            break;
                    }
                }
                else if (subtype == ARRAY_SUBTYPE)
                {

                }
                else if (subtype == VARIABLE_SUBTYPE)
                {

                }
            }

            HighCData cloneData = new HighCData(type,newValue,writable,readable,subtype);
            return cloneData;
        }
    }

    class HighCFunctionDeclaration
    {
        public Boolean isPure;
        public Boolean isRecursive;
        public String identifier;
        public List<HighCParameter> parameters;
        public String returnType;
        public int blockTokenPosition;

        public HighCFunctionDeclaration(String name, Boolean purity, Boolean recursiveness, List<HighCParameter> newParameters, String returnValue, int startPosition)
        {
            identifier = name;
            isPure = purity;
            isRecursive = recursiveness;
            parameters = newParameters;
            returnType = returnValue;
            blockTokenPosition = startPosition;
        }

        public HighCFunctionDeclaration Clone()
        {
            return new HighCFunctionDeclaration(identifier, isPure, isRecursive, parameters, returnType, blockTokenPosition);
        }
    }

    class HighCParameter
    {
        public String identifier;
        public Boolean inAllowed;
        public Boolean outAllowed;
        public List<String> subscriptParameters;
        public String type;
        public String subtype;

        public HighCParameter(String name, String newType, String newSubtype, Boolean inStatus, Boolean outStatus, List<String> newSubscriptParameters)
        {
            identifier = name;
            type = newType;
            subtype = newSubtype;
            inAllowed = inStatus;
            outAllowed = outStatus;
            subscriptParameters = newSubscriptParameters;
        }
    }
}
