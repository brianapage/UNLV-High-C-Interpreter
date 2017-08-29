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
        //Function State Variables
        private Boolean returnFlag = false;
        private HighCData returnValue;
        private List<HighCFunctionDeclaration> nonRecursiveFunctionCalls;
        private Boolean pureStatus = false;

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
            nonRecursiveFunctionCalls = new List<HighCFunctionDeclaration>();
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
                    newEntry = "(L" + tokenList[currentToken - 1].Line + ", C" + tokenList[currentToken - 1].Column + "): " + newEntry+Environment.NewLine;
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
                addDebugInfo("Expected to find: \"" + token + "\" but found \"" + tokenList[currentToken - 1].Text + "\" instead." );
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
                    addDebugInfo(HighCTokenLibrary.ON + ": at least one element was expected." );
                    return false;
                }

                if (needAnother == true)
                {
                    addDebugInfo(HighCTokenLibrary.ON + ": another element was expected after the comma." );
                    return false;
                }

                Console.WriteLine(currentToken + " on ( <label list> ) <block> -> " + value + " -> Block Not Taken");
                return skipBlock();
            }

            return false;
        }

        private void error(String errorMessage="")
        {
            if (errorMessage != "")
            {
                addDebugInfo(errorMessage, true);
            }
            else
            {
                Console.WriteLine("Empty Error Added: " + tokenList[currentToken]);
            }
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
            HighCData term1 = new HighCData(HighCType.INTEGER_TYPE, 0);
            String addOp;

            if (HC_add_op(out addOp) &&
                HC_arithmetic_term(ref term1))
            {
                if (addOp == HighCTokenLibrary.PLUS_SIGN)
                {
                    if (value.isFloat() &&
                        term1.isFloat())
                    {
                        value.setValue((Double)value.data + (Double)term1.data);
                    }
                    else if (value.isInteger() &&
                             term1.isFloat())
                    {
                        Double newValue = (Double)((Int64)value.data + (Double)term1.data);
                        value.type = new HighCType(HighCType.FLOAT_TYPE);
                        value.setValue(newValue);
                    }
                    else if (value.isFloat() &&
                             term1.isInteger())
                    {
                        Double newValue = (Double)((Double)value.data + (Int64)term1.data);
                        value.type = new HighCType(HighCType.FLOAT_TYPE);
                        value.setValue(newValue);
                    }
                    else
                    {
                        Int64 newValue = (Int64)value.data + (Int64)term1.data;
                        value.type = new HighCType(HighCType.INTEGER_TYPE);
                        value.setValue(newValue);
                    }
                    Console.WriteLine(currentToken + " <arithmetic expression'> -> + <arithmetic term>" + " -> " + value);
                }
                else if (addOp == HighCTokenLibrary.MINUS_SIGN)
                {
                    if (value.isFloat() &&
                        term1.isFloat())
                    {
                        value.setValue((Double)value.data - (Double)term1.data);
                    }
                    else if (value.isInteger() &&
                             term1.isFloat())
                    {
                        Double newValue = (Double)((Int64)value.data - (Double)term1.data);
                        value.type = new HighCType(HighCType.FLOAT_TYPE);
                        value.setValue(newValue);
                    }
                    else if (value.isFloat() &&
                             term1.isInteger())
                    {
                        Double newValue = (Double)((Double)value.data - (Int64)term1.data);
                        value.type = new HighCType(HighCType.FLOAT_TYPE);
                        value.setValue(newValue);
                    }
                    else
                    {
                        Int64 newValue = (Int64)value.data - (Int64)term1.data;
                        value.type = new HighCType(HighCType.INTEGER_TYPE);
                        value.setValue(newValue);
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
                if (value.isFloat() &&
                    term1.isFloat())
                {
                    value.data = Math.Pow((Double)value.data, (Double)term1.data);
                    value.type = new HighCType(HighCType.FLOAT_TYPE);
                }
                else if (value.isInteger() &&
                         term1.isFloat())
                {
                    value.data = Math.Pow((Int64)value.data , (Double)term1.data);
                    value.type = new HighCType(HighCType.FLOAT_TYPE);
                }
                else if (value.isFloat() &&
                         term1.isInteger())
                {
                    value.data = Math.Pow((Double)value.data , (Int64)term1.data);
                    value.type = new HighCType(HighCType.FLOAT_TYPE);
                }
                else if((Int64)term1.data < 0)
                {
                    value.data = Math.Pow((Int64)value.data, (Int64)term1.data);
                    value.type = new HighCType(HighCType.FLOAT_TYPE);
                }
                else
                {
                    value.data = Math.Pow((Int64)value.data, (Int64)term1.data);
                    value.type = new HighCType(HighCType.INTEGER_TYPE);
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
                    if(value.isInteger())
                    {
                        value.data = ((Int64)value.data) * -1;
                    }
                    else if(value.isFloat())
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
            HighCData data;
            if (HC_function_call(out data, new HighCType(HighCType.INTEGER_TYPE)))
            {
                value = new HighCData(HighCType.INTEGER_TYPE,(Int64)data.data);
                Console.WriteLine(currentToken + " <arithmetic primary> -> <integer function call>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            HighCData data2;
            if (HC_function_call(out data2, new HighCType(HighCType.FLOAT_TYPE)))
            {
                value = new HighCData(HighCTokenLibrary.FLOAT, (Double)data2.data);
                Console.WriteLine(currentToken + " <arithmetic primary> -> <float function call>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.LENGTH) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true) &&
               HC_list_expression(out data) &&
               matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS))
            {
                value = new HighCData(HighCType.INTEGER_TYPE, (Int64)((List<HighCData>)data.data).Count);
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
                    if (value.isFloat() &&
                        term1.isFloat())
                    {
                        value.data = (Double)value.data * (Double)term1.data;
                        value.type = new HighCType(HighCType.FLOAT_TYPE);
                    }
                    else if (value.isInteger() &&
                         term1.isFloat())
                    {
                        value.data = (Int64)value.data * (Double)term1.data;
                        value.type = new HighCType(HighCType.FLOAT_TYPE);
                    }
                    else if (value.isFloat() &&
                             term1.isInteger())
                    {
                        value.data = (Double)value.data * (Int64)term1.data;
                        value.type = new HighCType(HighCType.FLOAT_TYPE);
                    }
                    else
                    {
                        value.data = (Int64)value.data * (Int64)term1.data;
                        value.type = new HighCType(HighCType.INTEGER_TYPE);
                    }
                    Console.WriteLine(currentToken + " <arithmetic term'> -> * <arithmetic term'>" + " -> " + value);
                }
                else if (multOp == HighCTokenLibrary.SLASH)
                {
                    if (value.isFloat() &&
                        term1.isFloat())
                    {
                        value.data = (Double)value.data / (Double)term1.data;
                        value.type = new HighCType(HighCType.FLOAT_TYPE);
                    }
                    else if (value.isInteger() &&
                         term1.isFloat())
                    {
                        value.data = (Double)((Int64)value.data) / (Double)term1.data;
                        value.type = new HighCType(HighCType.FLOAT_TYPE);
                    }
                    else if (value.isFloat() &&
                             term1.isInteger())
                    {
                        value.data = (Double)value.data / (Int64)term1.data;
                        value.type = new HighCType(HighCType.FLOAT_TYPE);
                    }
                    else
                    {
                        value.data = (Double)((Int64)value.data) / (Double)((Int64)term1.data);
                        value.type = new HighCType(HighCType.FLOAT_TYPE);
                    }
                    
                    Console.WriteLine(currentToken + " <arithmetic term'> -> / <arithmetic term'>" + " -> " + value);
                }
                else if (multOp == HighCTokenLibrary.PERCENT_SIGN)
                {
                    if (value.isInteger() && 
                        term1.isInteger())
                    {
                        value.data = (Int64)value.data % (Int64)term1.data;
                        Console.WriteLine(currentToken + " <arithmetic term'> -> % <arithmetic term'>" + " -> " + value);
                    }
                    else
                    {
                        addDebugInfo("While performing a modulus operation one or more operands were not integer values." );
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

        private Boolean HC_array_constant(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_array_constant"); }
            int storeToken = currentToken;
            value = null;
            /*
            Array <subscript>* ( <element – constant> )
            { <element list> }
             */

            //Array <subscript>* ( <element – constant> )
            if (matchTerminal(HighCTokenLibrary.ARRAY))
            {
                int dimensionSize=0;
                List<int> dimensions = new List<int>();
                storeToken = currentToken;
                HighCData constantValue; 
                while(HC_subscript(out dimensionSize))
                {
                    storeToken = currentToken;
                    dimensions.Add(dimensionSize);
                }
                currentToken = storeToken;

                if (matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true) == false)
                {
                    error();
                    return false;
                }

                if(HC_element_constant(out constantValue)==false)
                {
                    error();
                    return false;
                }

                if (matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true) == false)
                {
                    error();
                    return false;
                }

                //The left hand array will fill itself with this value
                if(dimensions.Count==0)
                {
                    HighCData[] array = new HighCData[1];
                    array[0] = constantValue;
                    
                    HighCArray newArray = new HighCArray(array, dimensions, true);

                    value = new HighCData(new HighCType(HighCType.ARRAY_SUBTYPE, constantValue.type.dataType, constantValue.type.objectReference), newArray);

                    Console.WriteLine(currentToken + " <array constant> -> Array (element - constant) -> "+value);
                    return true;
                }
                else
                {
                    int size = 1;
                    foreach(int dimSize in dimensions)
                    {
                        size = size * dimSize;
                    }

                    HighCData[] array = new HighCData[size];
                    int i = 0;
                    while(i< array.Length)
                    {
                        array[i] = constantValue.getVariableOfType();
                        array[i].setData(constantValue);
                        i++;
                    }

                    HighCArray newArray = new HighCArray(array, dimensions);

                    value = new HighCData(new HighCType(HighCType.ARRAY_SUBTYPE, constantValue.type.dataType, constantValue.type.objectReference), newArray);
                    Console.WriteLine(currentToken + " <array constant> -> Array <subscript>* (element - constant) -> " + value);
                    return true;
                }
            }
            
            /*
            //{ <element list> }
            currentToken = storeToken;
            if(matchTerminal(HighCTokenLibrary.LEFT_CURLY_BRACKET))
            {
                if (matchTerminal(HighCTokenLibrary.RIGHT_CURLY_BRACKET, true))
                {
                    return true;
                }
                else
                {
                    error();
                    return false;
                }
            }
            */
            return false;
        }
        
        private Boolean HC_array_expression(out HighCData array)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_array_expression"); }
            int storeToken = currentToken;
            array = null;

            /*
            <array – constant>
            <array – variable>
            <array – func call>
            Array <subscript – expr>* ( <element – expr> )
            { <nested – element list> }
             */
            
            if(HC_array_constant(out array))
            {
                Console.WriteLine(currentToken + " <array expression> -> <array constant> -> "+array);
                return true;
            }

            currentToken = storeToken;
            if(HC_array_variable(out array))
            {
                Console.WriteLine(currentToken + " <array expression> -> <array variable> -> " + array);
                return true;
            }

            currentToken = storeToken;
            if (HC_function_call(out array, new HighCType(HighCType.ARRAY_SUBTYPE, null)))
            {
                Console.WriteLine(currentToken + " <array expression> -> <array function call> -> " + array);
                return true;
            }
            
            return false;
        }

        private Boolean HC_array_variable(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_array_variable"); }
            int storeToken = currentToken;
            value = null;
            String identifier;

            if (HC_id(out identifier))
            {
                HighCData temp = currentEnvironment.getItem(identifier);

                if (temp == null)
                {
                    error("Array Variable: The specified variable \"" + identifier + "\" could not be found.");
                    return false;
                }
                else if (temp.readable == false)
                {
                    error("Array Variable: The specified variable \"" + identifier + "\" cannot be referenced.");
                    return false;
                }
                else if (temp.isArray())
                {
                    
                    value = temp;
                    Console.WriteLine(currentToken + " <array variable> -> <id> -> " + identifier + " " + value);
                    return true;
                }
            }

            return false;
        }

        private Boolean HC_assignment()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_assignment"); }
            int storeToken = currentToken;
            HighCData value = null;
            HighCData foundItem = null;
            HighCData firstItem = null;
            Boolean isListSlice = false;
            int index=0, length=0;
            int arraySliceIndex = 0;
            int arraySliceLength = 0;
            String identifier = "";
            /*
            set <variable> = <expr>
             */

            if(matchTerminal(HighCTokenLibrary.SET))
            {
                if(HC_variable(out identifier))
                {
                    if (pureStatus == true)
                    {
                        HighCData globalItem = globalEnvironment.getItem(identifier);
                        HighCData localItem = currentEnvironment.getItem(identifier);
                        if (globalItem==localItem &&
                            localItem!=null)
                        {
                            error(HighCTokenLibrary.SET + ": Global scope data cannot be altered by a pure function." );
                            return false;
                        }
                    }
                    
                    List<int> dimensions = new List<int>();
                    if (currentEnvironment.contains(identifier))
                    {
                        //Check for sliced list
                        firstItem = currentEnvironment.getItem(identifier);
                        //Checking for List Slices
                        storeToken = currentToken;
                        if (firstItem.isList())
                        {
                            if (matchTerminal(HighCTokenLibrary.AT_SIGN))
                            {
                                if (HC_slice(out index, out length))
                                {
                                    isListSlice = true;
                                    if(firstItem.getCount()==0)
                                    {
                                        error("List Variable: Empty lists cannot be accessed in this way.");
                                        return false;
                                    }
                                    else if (index + length - 1 > firstItem.getCount())
                                    {
                                        error("List Variable: The specified slice must be between 1 and " + firstItem.getCount() + ".");
                                        return false;
                                    }
                                }
                                else
                                {
                                    error();
                                    return false;
                                }
                            }
                            else
                            {
                                currentToken = storeToken;
                            }
                        }
                        //Checking for Array Subscripts and/or a Slice
                        else if(firstItem.isArray())
                        {
                            int nextSubscript = 0;
                            while(HC_subscript(out nextSubscript))
                            {
                                storeToken = currentToken;
                                dimensions.Add(nextSubscript);
                            }
                            currentToken = storeToken;

                            if (matchTerminal(HighCTokenLibrary.LEFT_SQUARE_BRACKET))
                            {
                                if (HC_slice(out arraySliceIndex, out arraySliceLength))
                                {
                                    storeToken = currentToken;
                                    if(matchTerminal(HighCTokenLibrary.RIGHT_SQUARE_BRACKET,true)==false)
                                    {
                                        error();
                                        return false;
                                    }
                                }
                                else
                                {
                                    error();
                                    return false;
                                }
                            }
                            else
                            {
                                currentToken = storeToken;
                                //If no slice was found but there were subscripts, it transforms
                                //the last subscript into a slice of length 1.
                                if (dimensions.Count > 0)
                                {
                                    arraySliceIndex = dimensions.Last();
                                    arraySliceLength = 1;
                                    dimensions.RemoveAt(dimensions.Count - 1);
                                }
                            }
                        }
                    }

                    if (matchTerminal(HighCTokenLibrary.EQUAL,true))
                    {
                        if(HC_expression(out value))
                        {
                            if(isListSlice == true)
                            {
                                if (value.isList())
                                {
                                    if (value.getCount() == length)
                                    {
                                        int i = 0;
                                        while (i < length)
                                        {
                                            if (((List<HighCData>)(firstItem.data))[index - 1 + i].setData(((List<HighCData>)value.data)[i]) == true)
                                            {
                                                Console.WriteLine(currentToken + " <assignment> -> set <list variable>@<slice> = <list expression>");
                                            }
                                            else
                                            {
                                                if (((List<HighCData>)(firstItem.data))[index - 1].errorCode == HighCData.ERROR_CONSTANT_CHANGED)
                                                {
                                                    error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): The specified identifier is a constant which cannot be changed after declaration.");
                                                }
                                                else if (((List<HighCData>)(firstItem.data))[index - 1].errorCode == HighCData.ERROR_TYPE_MISMATCH)
                                                {
                                                    error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): This variable cannot be initialized with a value of type <" + value.type + ">, was expecting a <" + foundItem.type + ">.");
                                                }
                                                else if (((List<HighCData>)(firstItem.data))[index - 1].errorCode == HighCData.ERROR_OUT_OF_RANGE)
                                                {
                                                    error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): This variable can only accept values between " + (foundItem.type.minimum) + " and " + (foundItem.type.maximum) + ".");
                                                }
                                                return false;
                                            }
                                            i++;
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): The size of the two lists must match exactly.");
                                        return false;
                                    }
                                }
                                else if (value.isVariable() &&
                                         length == 1)
                                {
                                    if (((List<HighCData>)(firstItem.data))[index - 1].setData(value) == true)
                                    {
                                        Console.WriteLine(currentToken + " <assignment> -> set <list variable>@<slice> = <expression>");
                                        return true;
                                    }
                                    else
                                    {
                                        if (((List<HighCData>)(firstItem.data))[index - 1].errorCode == HighCData.ERROR_CONSTANT_CHANGED)
                                        {
                                            error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): The specified identifier is a constant which cannot be changed after declaration.");
                                        }
                                        else if (((List<HighCData>)(firstItem.data))[index - 1].errorCode == HighCData.ERROR_TYPE_MISMATCH)
                                        {
                                            error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): This variable cannot be initialized with a value of type <" + value.type + ">, was expecting a <" + foundItem.type + ">.");
                                        }
                                        else if (((List<HighCData>)(firstItem.data))[index - 1].errorCode == HighCData.ERROR_OUT_OF_RANGE)
                                        {
                                            error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): This variable can only accept values between " + (foundItem.type.minimum) + " and " + (foundItem.type.maximum) + ".");
                                        }
                                        return false;
                                    }
                                }
                            }
                            else if(arraySliceLength!=0)
                            {
                                HighCArray array = (HighCArray)firstItem.data;

                                if(dimensions.Count+1!=array.dimensions.Count)
                                {
                                    error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): Expected "+array.dimensions.Count+" number of dimensions specified.");
                                    return false;
                                }

                                int i = 0;
                                while(i<dimensions.Count)
                                {
                                    if(dimensions[i] > array.dimensions[i])
                                    {
                                        error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): Dimension index #"+(i+1)+" can be at most "+array.dimensions[i]+".");
                                        return false;
                                    }
                                    i++;
                                }

                                if(arraySliceIndex-1+arraySliceLength>array.dimensions[i])
                                {
                                    error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): Dimension index #" + (i + 1) + " can be at most " + array.dimensions[i] + ".");
                                    return false;
                                }

                                if (arraySliceLength > 1)
                                {
                                    if (value.isVariable())
                                    {
                                        error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): Slices of size greater than one can only be assigned lists or single dimension arrays.");
                                        return false;
                                    }
                                    else if(value.isList())
                                    {
                                        if (value.getCount() == arraySliceLength)
                                        {
                                            i = 0;
                                            while(i<arraySliceLength)
                                            {
                                                List<int> nextIndex = new List<int>();
                                                foreach(int dim in dimensions)
                                                {
                                                    nextIndex.Add(dim - 1);
                                                }
                                                nextIndex.Add(arraySliceIndex - 1 + i);

                                                if(array.getItemAt(nextIndex).setData(((List<HighCData>)value.data)[i])==false)
                                                {
                                                    if (array.getItemAt(nextIndex).errorCode == HighCData.ERROR_CONSTANT_CHANGED)
                                                    {
                                                        error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): The specified identifier is a constant which cannot be changed after declaration.");
                                                    }
                                                    else if (array.getItemAt(nextIndex).errorCode == HighCData.ERROR_TYPE_MISMATCH)
                                                    {
                                                        error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): This variable cannot be initialized with a value of type <" + ((List<HighCData>)value.data)[i].type + ">, was expecting a <" + array.getItemAt(nextIndex).type + ">.");
                                                    }
                                                    else if (array.getItemAt(nextIndex).errorCode == HighCData.ERROR_OUT_OF_RANGE)
                                                    {
                                                        error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): This variable can only accept values between " + (array.getItemAt(nextIndex).type.minimum) + " and " + (array.getItemAt(nextIndex).type.maximum) + ".");
                                                    }
                                                    return false;
                                                }
                                                i++;
                                            }

                                            Console.WriteLine(currentToken + " <assignment> -> set <variable>[]* = <list expression> -> " + foundItem);
                                            return true;
                                        }
                                        else
                                        {
                                            error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): The length of the list ("+value.getCount()+") must match the length of the slice ("+arraySliceLength+").");
                                            return false;
                                        }
                                    }
                                    else if(value.isArray())
                                    {
                                        HighCArray valueArray = (HighCArray)value.data;
                                        if (valueArray.array.Length == arraySliceLength)
                                        {
                                            i = 0;
                                            while (i < arraySliceLength)
                                            {
                                                List<int> nextIndex = new List<int>();
                                                foreach (int dim in dimensions)
                                                {
                                                    nextIndex.Add(dim - 1);
                                                }
                                                nextIndex.Add(arraySliceIndex - 1 + i);

                                                if (array.getItemAt(nextIndex).setData(valueArray.array[i]) == false)
                                                {
                                                    if (array.getItemAt(nextIndex).errorCode == HighCData.ERROR_CONSTANT_CHANGED)
                                                    {
                                                        error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): The specified identifier is a constant which cannot be changed after declaration.");
                                                    }
                                                    else if (array.getItemAt(nextIndex).errorCode == HighCData.ERROR_TYPE_MISMATCH)
                                                    {
                                                        error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): This variable cannot be initialized with a value of type <" + valueArray.array[i].type + ">, was expecting a <" + array.getItemAt(nextIndex).type + ">.");
                                                    }
                                                    else if (array.getItemAt(nextIndex).errorCode == HighCData.ERROR_OUT_OF_RANGE)
                                                    {
                                                        error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): This variable can only accept values between " + (array.getItemAt(nextIndex).type.minimum) + " and " + (array.getItemAt(nextIndex).type.maximum) + ".");
                                                    }
                                                    return false;
                                                }
                                                i++;
                                            }

                                            Console.WriteLine(currentToken + " <assignment> -> set <variable>[]* = <array expression> -> " + foundItem);
                                            return true;
                                        }
                                        else
                                        {
                                            error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): The length of the array (" + valueArray.array.Length + ") must match the length of the slice (" + arraySliceLength + ").");
                                            return false;
                                        }
                                    }
                                }
                                else if (arraySliceLength==1)
                                {
                                    if(value.isArray())
                                    {
                                        if (((HighCArray)value.data).array.Length == 1)
                                        {
                                            value = ((HighCArray)value.data).array[0];
                                        }
                                        else
                                        {
                                            error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): The length of the array (" + ((HighCArray)value.data).array.Length + ") must match the length of the slice (" + arraySliceLength + ").");
                                            return false;
                                        }
                                    }
                                    else if (value.isList())
                                    {
                                        if(((List<HighCData>)value.data).Count==1)
                                        {
                                            value = ((List<HighCData>)value.data)[0];
                                        }
                                        else
                                        {
                                            error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): The length of the list (" + ((List<HighCData>)value.data).Count + ") must match the length of the slice (" + arraySliceLength + ").");
                                            return false;
                                        }
                                    }

                                    List<int> nextIndex = new List<int>();
                                    foreach (int dim in dimensions)
                                    {
                                        nextIndex.Add(dim - 1);
                                    }
                                    nextIndex.Add(arraySliceIndex - 1);

                                    if (array.getItemAt(nextIndex).setData(value) == false)
                                    {
                                        if (array.getItemAt(nextIndex).errorCode == HighCData.ERROR_CONSTANT_CHANGED)
                                        {
                                            error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): The specified identifier is a constant which cannot be changed after declaration.");
                                        }
                                        else if (array.getItemAt(nextIndex).errorCode == HighCData.ERROR_TYPE_MISMATCH)
                                        {
                                            error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): This variable cannot be initialized with a value of type <" + value.type + ">, was expecting a <" + array.getItemAt(nextIndex).type + ">.");
                                        }
                                        else if (array.getItemAt(nextIndex).errorCode == HighCData.ERROR_OUT_OF_RANGE)
                                        {
                                            error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): This variable can only accept values between " + (array.getItemAt(nextIndex).type.minimum) + " and " + (array.getItemAt(nextIndex).type.maximum) + ".");
                                        }
                                        return false;
                                    }

                                    Console.WriteLine(currentToken + " <assignment> -> set <variable>[]* = <expression> -> " + foundItem);
                                    return true;
                                }
                            }

                            if (currentEnvironment.changeItem(identifier, value, out foundItem))
                            {
                                Console.WriteLine(currentToken + " <assignment> -> set <variable> = <expression> -> " + foundItem);
                                return true;
                            }
                            else
                            {
                                if (foundItem == null)
                                {
                                    error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): The specified identifier could not be found.");
                                }
                                else if (foundItem.errorCode == HighCData.ERROR_ARRAY_DIMENSION_MISMATCH)
                                {
                                    error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): Array dimensions do not match.");
                                }
                                else if(foundItem.errorCode == HighCData.ERROR_CONSTANT_CHANGED)
                                {
                                    error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): The specified identifier is a constant which cannot be changed after declaration.");
                                }
                                else if(foundItem.errorCode == HighCData.ERROR_TYPE_MISMATCH)
                                {
                                    error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): This variable cannot be initialized with a value of type <" + value.type + ">, was expecting a <" + foundItem.type + ">.");
                                }
                                else if (foundItem.errorCode == HighCData.ERROR_OUT_OF_RANGE)
                                {
                                    error(HighCTokenLibrary.SET + " (\"" + identifier + "\"): This variable must be initialized with a value between " + (foundItem.type.minimum) + " and " + (foundItem.type.maximum) + ".");
                                }
                                else
                                { 
                                    error(HighCTokenLibrary.SET + ": The specified variable could not be altered due to a type mismatch: expected <"+foundItem.type+"> but given <"+value.type+">." );
                                }
                                return false;
                            }
                        }
                        else
                        {
                            error(HighCTokenLibrary.SET + ": An expression was expected after the equal sign." );
                        }
                    }
                }
                else
                {
                    error(HighCTokenLibrary.SET + ": An identifier name was expected." );
                }
            }

            return false;
        }

        private Boolean HC_block(HighCEnvironment newEnvironment = null)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_block"); }
            int storeToken = currentToken;

            if (matchTerminal(HighCTokenLibrary.LEFT_CURLY_BRACKET, true) == false)
            {
                return false;
            }

            HighCEnvironment storeEnvironment = currentEnvironment;
            if (newEnvironment==null)
            {
                currentEnvironment = new HighCEnvironment(storeEnvironment);
            }
            else
            {
                currentEnvironment = newEnvironment;
            }

            storeToken = currentToken;
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
            
            if(returnFlag==true)
            {
                Console.WriteLine(currentToken + " <block> -> Ending early due to return statement.");
                return true;
            }
            
            if (matchTerminal(HighCTokenLibrary.RIGHT_CURLY_BRACKET, true) && stopProgram == false)
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
            HighCData data;
            if (HC_function_call(out data, new HighCType(HighCType.BOOLEAN_TYPE)))
            {
                if (data.isBoolean())
                {
                    value = (Boolean)data.data;
                    Console.WriteLine(currentToken + " <boolean factor> -> <boolean function call>" + " -> " + value);
                    return true;
                }
            }
            /*
            currentToken = storeToken;
            if (HC_relational_expression(ref value))
            {
                Console.WriteLine(currentToken + " <boolean factor> -> <relational expression>" + " -> " + value);
                return true;
            }
            */
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
                storeToken = currentToken;

                if (temp == null)
                {
                    error("Boolean Variable: The specified variable \"" + identifier + "\" could not be found." );
                    return false;
                }
                else if(temp.readable == false)
                {
                    error("Boolean Variable: The specified variable \"" + identifier + "\" cannot be referenced." );
                    return false;
                }
                else if (temp.isBoolean())
                {
                    if (temp.isList())
                    {
                        if (matchTerminal(HighCTokenLibrary.AT_SIGN))
                        {
                            int index;
                            int length;
                            if (HC_slice(out index, out length))
                            {
                                if (length != 1)
                                {
                                    error("Boolean List: The length of the slice from the list must be 1.");
                                    return false;
                                }

                                if(index+length-1>temp.getCount())
                                {
                                    error("Boolean List: The slice is not a subset of the list.");
                                    return false;
                                }

                                value = (Boolean)((List<HighCData>)(temp.data))[index-1].data;
                                Console.WriteLine(currentToken + " <boolean variable> -> <id>@<slice> -> " + identifier + "@"+index+" "+ value);
                                return true;
                            }
                            else
                            {
                                error();
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //<array – variable> <subscript – expr>*
                    else if (temp.isArray())
                    {
                        storeToken = currentToken;
                        int intBuffer;
                        List<int> index = new List<int>();
                        while (HC_subscript_expression(out intBuffer))
                        {
                            storeToken = currentToken;
                            index.Add(intBuffer);
                        }
                        currentToken = storeToken;

                        if (((HighCArray)(temp.data)).indexInBounds(index))
                        {
                            value = (Boolean)(((HighCArray)(temp.data)).getItemAt(index).data);
                            Console.WriteLine(currentToken + " <boolean variable> -> <id><subscript>* -> " + identifier + " " + value);
                            return true;
                        }
                        else
                        {
                            error("Boolean Array: The specified array index goes outside the bounds of the array.");
                            return false;
                        }
                    }

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
                    addDebugInfo(HighCTokenLibrary.ON + ": at least one element was expected." );
                    return false;
                }

                if (needAnother == true)
                {
                    addDebugInfo(HighCTokenLibrary.ON + ": another element was expected after the comma." );
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
            HighCData data;
            if (HC_function_call(out data, new HighCType(HighCType.CHARACTER_TYPE)))
            {
                if (data.isCharacter())
                {
                    stringBuffer = (String)data.data;
                    Console.WriteLine(currentToken + " <character expression> -> <char function call>" + " -> " + stringBuffer);
                    return true;
                }
            }

            return false;
        }
        
        private Boolean HC_character_variable(out String value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_character_variable"); }
            int storeToken = currentToken;
            value = "";
            String identifier;

            if (HC_id(out identifier))
            {
                HighCData temp = currentEnvironment.getItem(identifier);

                if (temp == null)
                {
                    error("Character Variable: The specified variable \"" + identifier + "\" could not be found." );
                    return false;
                }
                else if (temp.readable == false)
                {
                    error("Character Variable: The specified variable \"" + identifier + "\" cannot be referenced." );
                    return false;
                }
                else if (temp.isCharacter())
                {
                    if (temp.isList())
                    {
                        if (matchTerminal(HighCTokenLibrary.AT_SIGN))
                        {
                            int index;
                            int length;
                            if (HC_slice(out index, out length))
                            {
                                if (length != 1)
                                {
                                    error("Character List: The length of the slice from the list must be 1.");
                                    return false;
                                }

                                if (index + length - 1 > temp.getCount())
                                {
                                    error("Character List: The slice is not a subset of the list.");
                                    return false;
                                }

                                value = (String)((List<HighCData>)(temp.data))[index - 1].data;
                                Console.WriteLine(currentToken + " <character variable> -> <id>@<slice> -> " + identifier + "@" + index + " " + value);
                                return true;
                            }
                            else
                            {
                                error();
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //<array – variable> <subscript – expr>*
                    else if (temp.isArray())
                    {
                        storeToken = currentToken;
                        int intBuffer;
                        List<int> index = new List<int>();
                        while (HC_subscript_expression(out intBuffer))
                        {
                            storeToken = currentToken;
                            index.Add(intBuffer);
                        }
                        currentToken = storeToken;

                        if (((HighCArray)(temp.data)).indexInBounds(index))
                        {
                            value = (String)(((HighCArray)(temp.data)).getItemAt(index).data);
                            Console.WriteLine(currentToken + " <character variable> -> <id><subscript>* -> " + identifier + " " + value);
                            return true;
                        }
                        else
                        {
                            error("Character Array: The specified array index goes outside the bounds of the array.");
                            return false;
                        }
                    }
                    value = (String)temp.data;
                    Console.WriteLine(currentToken + " <character variable> -> <id> -> " + identifier + " " + value);
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
                                    addDebugInfo(HighCTokenLibrary.CHOICE + ": Each condition must be unique. \"" + label + "\" is used more than once." );
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
                                    addDebugInfo(HighCTokenLibrary.CHOICE + ": Each condition must be unique. \"" + label + "\" is used more than once." );
                                    return false;
                                }
                                else
                                {
                                    labelsUsed.Add(label);
                                }
                            }
                        }
                        currentToken = storeToken;

                        if (value.isBoolean())
                        {
                            if (labelsUsed.Count > 2)
                            {
                                addDebugInfo(HighCTokenLibrary.CHOICE + ": Conditions cannot overlap." );
                                return false;
                            }
                        }
                        else if (value.isInteger())
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
                                        addDebugInfo(HighCTokenLibrary.CHOICE + ": "+ firstValue + " must be less than " + secondValue+ ".");
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
                                        addDebugInfo(HighCTokenLibrary.CHOICE + ": Conditions (\""+previousLabel+"\") and (\""+label+"\") cannot overlap." );
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
                                        addDebugInfo(HighCTokenLibrary.CHOICE + ": Conditions (\"" + previousLabel + "\") and (\"" + label + "\") cannot overlap." );
                                        return false;
                                    }
                                }
                            }
                        }
                        else if (value.isCharacter())
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
                                        addDebugInfo(HighCTokenLibrary.CHOICE + ": " + firstValue + " must be less than " + secondValue + "." );
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
                                        addDebugInfo(HighCTokenLibrary.CHOICE + ": Conditions (\"" + previousLabel + "\") and (\"" + label + "\") cannot overlap." );
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
                                        addDebugInfo(HighCTokenLibrary.CHOICE + ": Conditions (\"" + previousLabel + "\") and (\"" + label + "\") cannot overlap." );
                                        return false;
                                    }
                                }
                            }
                        }
                        else if (value.isEnumeration())
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
                    addDebugInfo(HighCTokenLibrary.CHOICE + ": A discrete (boolean, character, enumeration, or integer) expression was expected inside the parenthesis." );
                }
            }

            return false;
        }

        private Boolean HC_constant(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_constant"); }
            value = null;
            int storeToken = currentToken;

            /*
             * <object – constant>
                <array – constant>
                <list – constant>
                <scalar – constant>
             */
             
            if(HC_scalar_constant(out value))
            {
                Console.WriteLine(currentToken + " <constant> -> <scalar constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            List<HighCData> values;
            if(HC_list_constant(out values))
            {
                if(values.Count > 0)
                {
                    value = new HighCData(new HighCType(HighCType.LIST_SUBTYPE, values[0].type.dataType,values[0].type.objectReference), values);
                }
                else
                {
                    value = new HighCData(new HighCType(HighCType.LIST_SUBTYPE, null), values);
                }
                Console.WriteLine(currentToken + " <constant> -> <list constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if(HC_array_constant(out value))
            {
                Console.WriteLine(currentToken + " <constant> -> <array constant>" + " -> " + value);
                return true;
            }
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
            HighCType type;
            HighCType type2;

            if (matchTerminal(HighCTokenLibrary.CREATE))
            {
                if (HC_type(out type))
                {
                    storeToken = currentToken;
                    while (HC_initiated_variable(type, out type2, false))
                    {
                        storeToken = currentToken;
                        atLeastOneFound = true;
                        needAnother = false;
                        
                        if (type.dataType == HighCTokenLibrary.INTEGER &&
                            type2.dataType == HighCTokenLibrary.FLOAT)
                        {
                            error("Variable Declaration" + ": The type of the variable (\"" + type2 + "\") does not match the type indicated (\"" + type + "\")." );
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
                        error("Variable Declaration" + ": another element was expected after the comma." );
                        return false;
                    }

                    if (atLeastOneFound == false)
                    {
                        error("Variable Declaration" + ": at least one declaration (\"<identifier> = <value>\") was expected after the type." );
                        return false;
                    }

                    Console.WriteLine(currentToken + " <declaration> -> <type><initiated variable>,...,<initiated variable> -> " + type);

                    return true;
                }
                else
                {
                    error(HighCTokenLibrary.CREATE + ": Expected a data or class type." );
                }
            }

            return false;
        }

        private Boolean HC_dir(out Boolean direction)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_direction"); }
            int storeToken = currentToken;
            /*
            in
            inrev
             */

            if(matchTerminal(HighCTokenLibrary.IN))
            {
                direction = true;
                Console.WriteLine(currentToken + " <dir>" + " -> " + HighCTokenLibrary.IN);
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.IN_REVERSE))
            {
                direction = false;
                Console.WriteLine(currentToken + " <dir>" + " -> " + HighCTokenLibrary.IN_REVERSE);
                return true;
            }

            direction = false;
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
        
        private Boolean HC_discrete_constant(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_discrete_constant"); }
            int storeToken = currentToken;
            value = null;

            /*
            <int – constant>
            <char – constant>
            <bool – constant>
            <enum – constant>
             */

            Int64 intTerm1 = 0;
            if (HC_integer_constant(out intTerm1))
            {
                value = new HighCData(HighCTokenLibrary.INTEGER, intTerm1);
                Console.WriteLine(currentToken + " <discrete constant> -> <integer constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            String stringTerm1 = "";
            if (HC_character_constant(out stringTerm1))
            {
                value = new HighCData(HighCTokenLibrary.CHARACTER, stringTerm1);
                Console.WriteLine(currentToken + " <discrete constant> -> <character constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            Boolean boolTerm1 = false;
            if (HC_boolean_constant(ref boolTerm1))
            {
                value = new HighCData(HighCTokenLibrary.BOOLEAN, boolTerm1);
                Console.WriteLine(currentToken + " <discrete constant> -> <boolean constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            HighCEnumeration enumeration;
            if (HC_enumeration_constant(out enumeration))
            {
                value = new HighCData(new HighCType(HighCType.VARIABLE_SUBTYPE,HighCType.ENUMERATION_INSTANCE,enumeration.type), enumeration);
                Console.WriteLine(currentToken + " <discrete constant> -> <enum constant>" + " -> " + value);
                return true;
            }

            return false;
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

                storeToken = currentToken;
                if (HC_relational_expression(value, out boolTerm))
                {
                    value = new HighCData(HighCType.BOOLEAN_TYPE, boolTerm);
                    Console.WriteLine(currentToken + " <discrete expression> -> <character expression><relational op><character expression>" + " -> " + value.ToString());
                    return true;
                }
                else
                {
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <discrete expression> -> <character expression>" + " -> " + value.ToString());
                    return true;
                }
            }

            currentToken = storeToken;
            if (HC_integer_expression(out term1) == true)
            {
                value = new HighCData(HighCTokenLibrary.INTEGER, term1);

                storeToken = currentToken;
                if (HC_relational_expression(value, out boolTerm))
                {
                    value = new HighCData(HighCType.BOOLEAN_TYPE, boolTerm);
                    Console.WriteLine(currentToken + " <discrete expression> -> <arithmetic expression><relational op><arithmetic expression>" + " -> " + value.ToString());
                    return true;
                }
                else
                {
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <discrete expression> -> <arithmetic expression>" + " -> " + value.ToString());
                    return true;
                }
            }

            currentToken = storeToken;
            HighCEnumeration enumBuffer;
            if (HC_enumeration_expression(out enumBuffer) == true)
            {
                value = new HighCData(new HighCType(HighCType.VARIABLE_SUBTYPE, HighCType.ENUMERATION_INSTANCE, enumBuffer.type), enumBuffer);

                storeToken = currentToken;
                if (HC_relational_expression(value, out boolTerm))
                {
                    value = new HighCData(HighCType.BOOLEAN_TYPE, boolTerm);
                    Console.WriteLine(currentToken + " <discrete expression> -> <enum expression><relational op><arithmetic expression>" + " -> " + value.ToString());
                    return true;
                }
                else
                {
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <discrete expression> -> <enum expression>" + " -> " + value.ToString());
                    return true;
                }
            }

            return false;
        }

        private Boolean HC_discrete_type(out HighCType type)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_discrete_type"); }
            int storeToken = currentToken;
            type = null;

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
                type = new HighCType(HighCType.BOOLEAN_TYPE);
                Console.WriteLine(currentToken + " <discrete type> -> " + type);
                return true;
            }
            
            Int64 intTerm1 = 0;
            Int64 intTerm2 = 0;
            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.INTEGER))
            {
                storeToken = currentToken;
                if (matchTerminal(HighCTokenLibrary.COLON))
                {
                    if (HC_integer_constant(out intTerm1) == false)
                    {
                        error(HighCTokenLibrary.INTEGER+": Expected an integer constant to follow the colon to indicate the lower bound of the variable." );
                        return false;
                    }
                    if (matchTerminal(HighCTokenLibrary.ELLIPSES, true) == false)
                    {
                        error();
                        return false;
                    }
                    if (HC_integer_constant(out intTerm2) == false)
                    {
                        error(HighCTokenLibrary.INTEGER+": Expected an integer constant to follow the colon to indicate the upper bound of the variable." );
                        return false;
                    }
                    
                    if (intTerm1 <= intTerm2)
                    {
                        type = new HighCType(HighCType.VARIABLE_SUBTYPE,
                                            HighCType.INTEGER_TYPE,
                                            null,
                                            new HighCData(HighCType.INTEGER_TYPE, intTerm1),
                                            new HighCData(HighCType.INTEGER_TYPE, intTerm2));
                        Console.WriteLine(currentToken + " <discrete type> -> " + type + ": "+intTerm1+HighCTokenLibrary.ELLIPSES+intTerm2);
                        return true;
                    }
                    else
                    {
                        error(HighCTokenLibrary.INTEGER+": The second range specification must be greater than or equal to the first." );
                        return false;
                    }
                }
                else
                {
                    type = new HighCType(HighCType.INTEGER_TYPE);
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <discrete type> -> " + type);
                    return true;
                }
            }
            
            String charBuffer1 = "";
            String charBuffer2 = "";
            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.CHARACTER))
            {
                storeToken = currentToken;
                if (matchTerminal(HighCTokenLibrary.COLON))
                {
                    if (HC_character_constant(out charBuffer1) == false)
                    {
                        error(HighCTokenLibrary.CHARACTER + ": Expected a character constant to follow the colon to indicate the lower bound of the variable." );
                        return false;
                    }
                    if (matchTerminal(HighCTokenLibrary.ELLIPSES, true) == false)
                    {
                        error();
                        return false;
                    }
                    if (HC_character_constant(out charBuffer2) == false)
                    {
                        error(HighCTokenLibrary.CHARACTER + ": Expected a character constant to follow the colon to indicate the upper bound of the variable." );
                        return false;
                    }

                    if (charBuffer1[0] <= charBuffer2[0])
                    {
                        type = new HighCType(HighCType.VARIABLE_SUBTYPE,
                                            HighCType.CHARACTER_TYPE,
                                            null,
                                            new HighCData(HighCType.CHARACTER_TYPE, (String)("" + charBuffer1[0])),
                                            new HighCData(HighCType.CHARACTER_TYPE, (String)("" + charBuffer2[0])));
                        
                        Console.WriteLine(currentToken + " <discrete type> -> " + type);
                        return true;
                    }
                    else
                    {
                        error(HighCTokenLibrary.CHARACTER + ": The second range specification must be greater than or equal to the first." );
                        return false;
                    }
                }
                else
                {
                    type = new HighCType(HighCType.CHARACTER_TYPE);
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <discrete type> -> " + type);
                    return true;
                }
            }

            HighCEnumeration enumBuffer1;
            HighCEnumeration enumBuffer2;
            HighCEnumerationType enumType;
            currentToken = storeToken;
            if (HC_enumeration_type(out enumType))
            {
                storeToken = currentToken;
                if (matchTerminal(HighCTokenLibrary.COLON))
                {
                    if (HC_enumeration_constant(out enumBuffer1) == false)
                    {
                        error(HighCTokenLibrary.ENUMERATION + ": Expected an enumeration constant to follow the colon to indicate the lower bound of the variable." );
                        return false;
                    }
                    if (matchTerminal(HighCTokenLibrary.ELLIPSES, true) == false)
                    {
                        error();
                        return false;
                    }
                    if (HC_enumeration_constant(out enumBuffer2) == false)
                    {
                        error(HighCTokenLibrary.ENUMERATION + ": Expected an enumeration constant to follow the colon to indicate the upper bound of the variable." );
                        return false;
                    }

                    if (enumBuffer1.type == enumType)
                    {
                        if (enumBuffer2.type == enumType)
                        {
                            if (enumBuffer1.rank <= enumBuffer2.rank)
                            {
                                type = new HighCType(HighCType.VARIABLE_SUBTYPE, 
                                                    HighCType.ENUMERATION_INSTANCE, 
                                                    enumType,
                                                    new HighCData(HighCType.ENUMERATION_INSTANCE, enumBuffer1),
                                                    new HighCData(HighCType.ENUMERATION_INSTANCE, enumBuffer2));
                                
                                Console.WriteLine(currentToken + " <discrete type> -> " + type);
                                return true;
                            }
                            else
                            {
                                error(HighCTokenLibrary.ENUMERATION + ": The second range specification must be greater than or equal to the first." );
                                return false;
                            }
                        }
                        else
                        {
                            error(HighCTokenLibrary.ENUMERATION + ": The upper range specification does not match the type of <" + enumType.identifier + ">." );
                            return false;
                        }
                    }
                    else
                    {
                        error(HighCTokenLibrary.ENUMERATION + ": The lower range specification does not match the type of <"+enumType.identifier+">." );
                        return false;
                    }
                    
                }
                else
                {
                    type = new HighCType(HighCType.VARIABLE_SUBTYPE, HighCType.ENUMERATION_INSTANCE, enumType);
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <discrete type> -> " + type);
                    return true;
                }
            }
            
            return false;
        }

        private Boolean HC_element_constant(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_element_constant"); }
            int storeToken = currentToken;
            value = null;
            /*
            <scalar – constant>
            <object – constant>
             */

            if (HC_scalar_constant(out value))
            {
                return true;
            }

            return false;
        }
        
        private Boolean HC_element_expression(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_element_expression"); }
            int storeToken = currentToken;
            value = null;

            /*
            <scalar – expr>
            <object – expr>
             */
            
            if(HC_scalar_expression(out value))
            {
                Console.WriteLine(currentToken + " <element - expr> -> <scalar - expr> ->" + value);
                return true;
            }

            currentToken = storeToken;
            if(HC_object_expression(out value))
            {
                Console.WriteLine(currentToken + " <element - expr> -> <object - expr> ->" + value);
                return true;
            }

            return false;
        }

        private Boolean HC_element_or_list(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_element_or_list"); }
            int storeToken = currentToken;
            value = null;
            /*
            <element – expr>
            <list – expr>
             */

            HighCData newItem;
            if(HC_element_expression(out newItem))
            {
                List<HighCData> newList = new List<HighCData>();
                newList.Add(newItem);
                
                value = new HighCData(new HighCType(HighCType.LIST_SUBTYPE, 
                                                    newItem.type.dataType, 
                                                    newItem.type.objectReference),
                                      newList);
                Console.WriteLine(currentToken + " <element or list> -> <element expression> ->" + value);
                return true;
            }

            currentToken = storeToken;
            if(HC_list_expression(out value))
            {
                Console.WriteLine(currentToken + " <element or list> -> <list expression> ->" + value);
                return true;
            }

            return false;
        }

        private Boolean HC_else_if(ref Boolean blockEntered)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_else_if"); }
            int storeToken = currentToken;
            /*
            elseif ( <bool – expr> ) <block>
            else if ( <bool – expr> ) <block>
             */
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
                    HighCData term;
                    Boolean boolTerm1 = false;
                    if (HC_scalar_expression(out term) &&
                        term.isBoolean())
                    {
                        boolTerm1 = (Boolean)term.data;
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
                                error(elseifStyle + ": Must be followed by a block. Example: \"" + HighCTokenLibrary.LEFT_CURLY_BRACKET + " " + HighCTokenLibrary.RIGHT_CURLY_BRACKET + "\"." );
                            }
                        }
                        else { error(); return false; }
                    }
                    else
                    {
                        error(elseifStyle + ": A boolean expression was expected inside the parenthesis." );
                    }
                }
                else{ error(); return false; }
            }

            Console.WriteLine(currentToken + " <else if> -> Null");
            return false;
        }
        
        private Boolean HC_enumeration_constant(out HighCEnumeration value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_enumeration_constant"); }
            int storeToken = currentToken;

            value = null;

            /*
            <id>
             */
             
            String stringBuffer;

            if (HC_id(out stringBuffer))
            {
                if(globalEnvironment.contains(stringBuffer))
                {
                    HighCData itemBuffer = globalEnvironment.getItem(stringBuffer);
                    if(itemBuffer.isEnumeration() &&
                       itemBuffer.isVariable())
                    {
                        value = (HighCEnumeration)itemBuffer.data;
                        return true;
                    }
                }
            }

            return false;
        }

        private Boolean HC_enumeration_expression(out HighCEnumeration value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_enumeration_expression"); }
            int storeToken = currentToken;

            value = null;
            /*
            <enum – variable>
            <enum constant>
            Next ( <enum – expr> )
            Prev ( <enum – expr> )
            <enum – func call>
             */

            if (HC_enumeration_variable(out value))
            {
                Console.WriteLine(currentToken + " <enum expression> -> <enum – variable> ->"+value);
                return true;
            }

            currentToken = storeToken;
            if(HC_enumeration_constant(out value))
            {
                Console.WriteLine(currentToken + " <enum expression> -> <enum – constant> ->" + value);
                return true;
            }

            currentToken = storeToken;
            if(matchTerminal(HighCTokenLibrary.NEXT))
            {
                if(matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS,true))
                {
                    if (HC_enumeration_expression(out value))
                    {
                        if (matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true))
                        {
                            HighCEnumeration enumBuffer = value.type.getNext(value.identifier);
                            
                            if(enumBuffer != null)
                            {
                                value = enumBuffer;
                                Console.WriteLine(currentToken + " Next(<enum expression>) -> <enum expression> ->" + value);
                                return true;
                            }
                            else
                            {
                                error("Next(Enumeration Constant): The enumeration constant \""+value.identifier+"\" does not have a higher value.");
                                return false;
                            }
                        }
                        else
                        {
                            error();
                            return false;
                        }
                    }
                }
                else
                {
                    error();
                    return false;
                }
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.PREVIOUS))
            {
                if (matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
                {
                    if (HC_enumeration_expression(out value))
                    {
                        if (matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true))
                        {
                            HighCEnumeration enumBuffer = value.type.getPrevious(value.identifier);

                            if (enumBuffer != null)
                            {
                                value = enumBuffer;
                                Console.WriteLine(currentToken + " Prev(<enum expression>) -> <enum expression> ->" + value);
                                return true;
                            }
                            else
                            {
                                error("Prev(Enumeration Constant): The enumeration constant \"" + value.identifier + "\" does not have a lower value." );
                                return false;
                            }
                        }
                        else
                        {
                            error();
                            return false;
                        }
                    }
                }
                else
                {
                    error();
                    return false;
                }
            }

            currentToken = storeToken;
            HighCData data;
            if (HC_function_call(out data, new HighCType(HighCType.ENUMERATION_INSTANCE)))
            {
                value = (HighCEnumeration)data.data;
                Console.WriteLine(currentToken + " <enum expression> -> <enum function call> ->" + value);
                return true;
            }

            return false;
        }
        
        private Boolean HC_enumeration_type(out HighCEnumerationType value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_ENUMERATION_TYPE"); }
            int storeToken = currentToken;
            String identifier;
            value = null;

            currentToken = storeToken;
            if (HC_id(out identifier))
            {
                if (globalEnvironment.contains(identifier))
                {
                    HighCData itemBuffer = globalEnvironment.getItem(identifier);
                    if (itemBuffer.isEnumerationType())
                    {
                        value = (HighCEnumerationType)itemBuffer.data;
                        Console.WriteLine(currentToken + " <discrete type> -> " + identifier);
                        return true;
                    }
                }
            }

            return false;
        }

        private Boolean HC_enumeration_variable(out HighCEnumeration value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_enumeration_variable"); }
            int storeToken = currentToken;

            /*
            <id>
            <array – variable> <subscript – expr>* [ <slice> ]
            <list – variable> @ <slice>
            <object – variable>.<variable>
             */
            value = null;
            String identifier = "";

            if (HC_id(out identifier))
            {
                if (currentEnvironment.contains(identifier))
                {
                    HighCData temp = currentEnvironment.getItem(identifier);
                    if (temp.isEnumeration())
                    {
                        if (temp.isList())
                        {
                            if (matchTerminal(HighCTokenLibrary.AT_SIGN))
                            {
                                int index;
                                int length;
                                if (HC_slice(out index, out length))
                                {
                                    if (length != 1)
                                    {
                                        error("Enumeration List: The length of the slice from the list must be 1.");
                                        return false;
                                    }

                                    if (index + length - 1 > temp.getCount())
                                    {
                                        error("Enumeration List: The slice is not a subset of the list.");
                                        return false;
                                    }

                                    value = (HighCEnumeration)((List<HighCData>)(temp.data))[index - 1].data;
                                    Console.WriteLine(currentToken + " <enumeration variable> -> <id>@<slice> -> " + identifier + "@" + index + " " + value);
                                    return true;
                                }
                                else
                                {
                                    error();
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        //<array – variable> <subscript – expr>*
                        else if (temp.isArray())
                        {
                            storeToken = currentToken;
                            int intBuffer;
                            List<int> index = new List<int>();
                            while (HC_subscript_expression(out intBuffer))
                            {
                                storeToken = currentToken;
                                index.Add(intBuffer);
                            }
                            currentToken = storeToken;

                            if (((HighCArray)(temp.data)).indexInBounds(index))
                            {
                                value = (HighCEnumeration)(((HighCArray)(temp.data)).getItemAt(index).data);
                                Console.WriteLine(currentToken + " <enumeration variable> -> <id><subscript>* -> " + identifier + " " + value);
                                return true;
                            }
                            else
                            {
                                error("Enumeration Array: The specified array index goes outside the bounds of the array.");
                                return false;
                            }
                        }
                        value = (HighCEnumeration)(temp.data);
                        Console.WriteLine(currentToken + " <enumeration variable> -> <id> -> " + identifier + " " + value);
                        return true;
                    }
                }
            }
            return false;
        }

        private Boolean HC_equality_op(out String opType)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_equality_op"); }
            int storeToken = currentToken;

            /*
            =
            ~=
             */

            opType = "";

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
                    addDebugInfo("Expression: An expression was expected inside the parenthesis." );
                }
            }

            currentToken = storeToken;
            if(HC_object_expression(out value))
            {
                Console.WriteLine(currentToken + " <expression> -> <object expression>");
                return true;
            }

            currentToken = storeToken;
            if (HC_array_expression(out value))
            {
                Console.WriteLine(currentToken + " <expression> -> <array expression>");
                return true;
            }

            currentToken = storeToken;
            if (HC_list_expression(out value))
            {
                storeToken = currentToken;
                Boolean boolTerm;
                if (HC_relational_expression(value, out boolTerm))
                {
                    value = new HighCData(HighCType.BOOLEAN_TYPE, boolTerm);
                    Console.WriteLine(currentToken + " <expression> -> <list expression><equality op><list expression>" + " -> " + value.ToString());
                    return true;
                }
                else
                {
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <expression> -> <list expression>");
                    return true;
                }
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
            int sign;
            /*
            <int – constant>.<digit>* <exponent>
            <int – constant> e <int – constant>
            <sign>.<digit><digit>* <exponent>
             */
            if (HC_sign(out sign))
            { 
                if (matchTerminal(HighCTokenLibrary.FLOAT_LITERAL))
                {
                    Double.TryParse(tokenList[currentToken - 1].Text, out value);
                    value = value * sign;

                    int storeToken = currentToken;
                    if (matchTerminal(HighCTokenLibrary.EXPONENT))
                    {
                        if (matchTerminal(HighCTokenLibrary.INTEGER_LITERAL, true))
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
                            addDebugInfo(HighCTokenLibrary.EXPONENT + ": An integer value was expected after the exponent.");
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
                    error("Float Variable: The specified variable \"" + identifier + "\" could not be found." );
                    return false;
                }
                else if (temp.readable == false)
                {
                    error("Float Variable: The specified variable \"" + identifier + "\" cannot be referenced." );
                    return false;
                }
                else if (temp.isFloat())
                {
                    if (temp.isList())
                    {
                        if (matchTerminal(HighCTokenLibrary.AT_SIGN))
                        {
                            int index;
                            int length;
                            if (HC_slice(out index, out length))
                            {
                                if (length != 1)
                                {
                                    error("Float List: The length of the slice from the list must be 1.");
                                    return false;
                                }

                                if (index + length - 1 > temp.getCount())
                                {
                                    error("Float List: The slice is not a subset of the list.");
                                    return false;
                                }

                                value = (Double)((List<HighCData>)(temp.data))[index - 1].data;
                                Console.WriteLine(currentToken + " <float variable> -> <id>@<slice> -> " + identifier + "@" + index + " " + value);
                                return true;
                            }
                            else
                            {
                                error();
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //<array – variable> <subscript – expr>*
                    else if (temp.isArray())
                    {
                        storeToken = currentToken;
                        int intBuffer;
                        List<int> index = new List<int>();
                        while (HC_subscript_expression(out intBuffer))
                        {
                            storeToken = currentToken;
                            index.Add(intBuffer);
                        }
                        currentToken = storeToken;

                        if (((HighCArray)(temp.data)).indexInBounds(index))
                        {
                            value = (Double)(((HighCArray)(temp.data)).getItemAt(index).data);
                            Console.WriteLine(currentToken + " <float variable> -> <id><subscript>* -> " + identifier + " " + value);
                            return true;
                        }
                        else
                        {
                            error("Float Array: The specified array index goes outside the bounds of the array.");
                            return false;
                        }
                    }
                    value = (Double)temp.data;
                    Console.WriteLine(currentToken + " <float variable> -> <id> -> " + identifier + " " + value);
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
                        error("Function declaration: another parameter was expected after the comma." );
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
            HighCType resultType;
            int startPosition;

            if (HC_modifiers(out pure, out recursive))
            {
                storeToken = currentToken;
                
                //This should only output an error message if a pure or recursive modifier was found
                if((pure==true || recursive == true) && matchTerminal(HighCTokenLibrary.FUNCTION, true)==false)
                {
                    //error
                    errorFound = true;
                    stopProgram = true;
                    return false;
                }

                currentToken = storeToken;
                if (matchTerminal(HighCTokenLibrary.FUNCTION))
                {
                    if(HC_id(out functionName))
                    {
                        if(HC_type_parameters()) //Will not be implemented yet
                        {
                            if(matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
                            {
                                if(HC_formal_parameters(out parameters))
                                {
                                    if(pure==true)
                                    {
                                        foreach(HighCParameter parameter in parameters)
                                        {
                                            if(parameter.outAllowed==true)
                                            {
                                                error("Function declaration \"" + functionName + "\": Pure functions cannot have \"out\" parameters." );
                                                return false;
                                            }
                                        }
                                    }

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
                                                        error("Function declaration: \"" + functionName + "\" already exists." );
                                                        return false;
                                                    }
                                                }
                                                else
                                                {
                                                    error("Function declaration: \""+ functionName + "\" should be followed by a block of code (\"{ code }\")." );
                                                    return false;
                                                }
                                            }
                                            else
                                            {
                                                error("Function declaration: \"" + functionName + "\" should be followed by a return type or \"void\"." );
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            error();
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        error();
                                        return false;
                                    }
                                }
                                else
                                {
                                    error("Function declaration \"" + functionName + "\": One or more parameters were incorrectly specified.  Parameters must start with \""+HighCTokenLibrary.IN+"\", \""+HighCTokenLibrary.OUT+"\", or \""+HighCTokenLibrary.IN_OUT+"\" followed by a type and then an identifier.");
                                    return false;
                                }
                            }
                            else
                            {
                                error();
                                return false;
                            }
                        }
                    }
                    else
                    {
                        error("Function declaration: Expected an identifier for the function.");
                        return false;
                    }
                }
            }
            
            return false;
        }
        
        private Boolean HC_function_call(out HighCData outValue, HighCType expectedReturnType)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_function_call"); }
            int storeToken = currentToken;
            outValue = null;

            /*
            <func – expr> ( <expr list> )
            <func – expr> ( )
             */
             
            HighCFunctionDeclaration function;
            HighCEnvironment functionEnvironment = new HighCEnvironment(globalEnvironment);
            HighCEnvironment storeEnvironment;
            List<HighCParameter> parameters;
            List<HighCData> parameterData = new List<HighCData>();
            List<String> outIdentifiers = new List<String>();
            Boolean firstParameter = true;

            if (HC_function_expression(out function))
            {
                if(expectedReturnType.dataType==null)
                {
                    expectedReturnType.dataType = function.returnType.dataType;
                }

                //Check return type and only run the function if they match
                if (function.returnType.dataType != expectedReturnType.dataType ||
                    function.returnType.memoryType != expectedReturnType.memoryType)
                {
                    //addDebugInfo("(Warning) Function \""+function.identifier+"\" did not match the expected type <"+expectedReturnType+">");
                    return false;
                }
                
                if (matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
                {
                    //Look for parameters
                    parameters = function.parameters;

                    foreach (HighCParameter parameter in parameters)
                    {
                        HighCData value;

                        if (firstParameter == true)
                        {
                            firstParameter = false;
                        }
                        else if (firstParameter == false &&
                            matchTerminal(HighCTokenLibrary.COMMA, true) == false)
                        {
                            error();
                            return false;
                        }

                        if (parameter.inAllowed == true &&
                            parameter.outAllowed == false)
                        {
                            if (HC_expression(out value))
                            {
                                HighCData newValue = new HighCData(parameter.type, null, true, true);
                                if (newValue.setData(value.type, value.data) == true)
                                {
                                    newValue.writable = parameter.outAllowed;
                                    newValue.readable = parameter.inAllowed;
                                    parameterData.Add(newValue);
                                }
                                else
                                {
                                    error("Call: \"" + function.identifier + "\" the type of the parameter provided <" + value.type + "> does not match the expected type <" + parameter.type + ">." );
                                    return false;
                                }
                            }
                            else
                            {
                                error("Call: \"" + function.identifier + "\" no value specified for parameter of type <" + parameter.type + ">." );
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

                                    HighCData newValue = new HighCData(parameter.type, null, true, true);
                                    if (newValue.setData(value.type, value.data) == true)
                                    {
                                        newValue.writable = parameter.outAllowed;
                                        newValue.readable = parameter.inAllowed;
                                        parameterData.Add(newValue);
                                        outIdentifiers.Add(identifier);
                                    }
                                    else
                                    {
                                        error("Call \"" + function.identifier + "\": the specified identifier \"" + identifier + "\" with type <" + value.type + "> does not match the expected parameter type <" + parameter.type + ">." );
                                        return false;
                                    }
                                }
                                else
                                {
                                    error("Call \"" + function.identifier + "\": the specified identifier \"" + identifier + "\" could not be found." );
                                    return false;
                                }
                            }
                            else
                            {
                                error("Call \"" + function.identifier + "\": expected an identifier of type <" + parameter.type + "> as a parameter." );
                                return false;
                            }
                        }
                    }

                    if (function.isRecursive == false)
                    {
                        if (nonRecursiveFunctionCalls.Contains(function))
                        {
                            error("Call \"" + function.identifier + "\": This function is non-recursive and cannot call itself, it may require the \"" + HighCTokenLibrary.RECURSIVE + "\" modifier in its declaration." );
                            return false;
                        }
                        else
                        {
                            nonRecursiveFunctionCalls.Add(function);
                        }
                    }

                    if (pureStatus == true &&
                        function.isPure == false)
                    {
                        error("Call \"" + function.identifier + "\": Impure functions cannot be called by pure functions." );
                        return false;
                    }

                    //Perform Function
                    storeToken = currentToken;
                    if(function != null)
                    {
                        currentToken = function.blockTokenPosition;
                        storeEnvironment = currentEnvironment;
                        currentEnvironment = functionEnvironment;

                        int i = 0;
                        foreach (HighCParameter parameter in parameters)
                        {
                            currentEnvironment.addNewItem(parameter.identifier, parameterData[i]);
                            i++;
                        }

                        Boolean oldPureStatus = pureStatus;
                        if (function.isPure == true)
                        {
                            pureStatus = true;
                        }

                        if (HC_block())
                        {

                        }

                        if (function.isRecursive == false)
                        {
                            nonRecursiveFunctionCalls.Remove(function);
                        }

                        if (expectedReturnType.isVoid())
                        {

                        }
                        else if (returnValue == null)
                        {
                            error("Function: \"" + function.identifier + "\" expected a return value of type <" + function.returnType + ">." );
                            return false;
                        }
                        
                        if(expectedReturnType.isVoid())
                        {

                        }
                        else
                        {
                            outValue = new HighCData(function.returnType, null, true, true);
                            if (outValue.setData(returnValue) == false)
                            {
                                error("Function: \"" + function.identifier + "\" expected a return value of type <" + function.returnType + "> but instead had a <" + returnValue.type + ">." );
                                return false;
                            }
                        }

                        returnValue = null;
                        returnFlag = false;

                        foreach (HighCParameter parameter in parameters)
                        {
                            i = 0;
                            if (parameter.outAllowed == true)
                            {
                                HighCData temp;
                                if (storeEnvironment.changeItem(outIdentifiers[i], currentEnvironment.getItem(parameter.identifier), out temp) == false)
                                {
                                    stopProgram = true;
                                    errorFound = true;
                                    addDebugInfo("Call: \"" + outIdentifiers[i] + "\" could not be found or is not a variable." );
                                    return false;
                                }
                                i++;
                            }
                        }

                        pureStatus = oldPureStatus;
                        currentEnvironment = storeEnvironment;
                        currentToken = storeToken;
                    }
                    else
                    {
                        error("Function: Expected a function.");
                        return false;
                    }

                    if (matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true))
                    {
                        Console.WriteLine(currentToken + " <function call> -> <func – expr> ( <expr list>* )");
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
                        if(data.isFunction())
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
                    addDebugInfo(HighCTokenLibrary.GLOBAL + ": Expected a declaration statement." );
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
                //addDebugInfo(HighCTokenLibrary.IDENTIFIER + ": Expected an identifier beginning with a letter or question mark that does not match any reserved keywords." );
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
            HighCData term;

            if (matchTerminal(HighCTokenLibrary.IF) &&
                matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
            {
                Boolean boolTerm1 = false;
                if (HC_scalar_expression(out term) &&
                    matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true) &&
                    term.isBoolean())
                {
                    boolTerm1 = (Boolean)term.data;

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
                        addDebugInfo(HighCTokenLibrary.IF + ": Must be followed by a block.  Example: \"" + HighCTokenLibrary.LEFT_CURLY_BRACKET + " " + HighCTokenLibrary.RIGHT_CURLY_BRACKET + "\"." );
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
                            addDebugInfo(HighCTokenLibrary.ELSE + ": Must be followed by a block.  Example: \"" + HighCTokenLibrary.LEFT_CURLY_BRACKET + " " + HighCTokenLibrary.RIGHT_CURLY_BRACKET + "\"." );
                            return false;
                        }
                    }
                    else
                    {
                        currentToken = storeToken;
                        addDebugInfo(HighCTokenLibrary.IF + ": Must include an \"" + HighCTokenLibrary.ELSE + "\" as part of its declaration." );
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
                    addDebugInfo(HighCTokenLibrary.IF + ": A boolean expression was expected inside the parenthesis." );
                }
            }

            return false;
        }

        private Boolean HC_initiated_variable(HighCType expectedType, out HighCType type, Boolean isConstant = false)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_initiated_variable"); }
            int storeToken = currentToken;
            type=null;
            String variableName;
            List<int> variableSubtype;
            HighCData constantValue;
            /*
            < var > = < constant >
            */

            if (HC_var(out variableName, out variableSubtype) &&
                matchTerminal(HighCTokenLibrary.EQUAL) &&
                HC_constant(out constantValue))
            {
                if (currentEnvironment.directlyContains(variableName) ||
                        globalEnvironment.contains(variableName))
                {
                    error("Declaration: The provided identifier \"" + variableName + "\" already exists in this scope and cannot be redeclared.");
                    return false;
                }
                
                HighCData variable = new HighCData(expectedType, constantValue.data, !isConstant, true);
                
                if (expectedType.isEnumeration())
                {
                    if (constantValue.isEnumeration())
                    {
                        if (constantValue.type.objectReference != expectedType.objectReference)
                        {
                            error("Constant Value: " + constantValue.type + "     " + constantValue.type.objectReference);
                            error("Expected Type: " + expectedType + "     " + expectedType.objectReference);
                            error("Declaration \"" + variableName + "\": This variable cannot be initialized with a value of type <" + constantValue.type + ">, was expecting a <" + expectedType + ">.");
                            return false;
                        }
                    }
                    else
                    {
                        error("Declaration: \"" + variableName + "\" cannot be initialized with a value of type <" + constantValue.type + ">, was expecting a <" + expectedType + ">.");
                        return false;
                    }
                }
                
                //Variable
                if (variableSubtype[0] == 0)
                {
                    variable.type.memoryType = HighCType.VARIABLE_SUBTYPE;
                    if (variable.setData(constantValue.type, constantValue.data, true) == false)
                    {
                        if (variable.errorCode == HighCData.ERROR_TYPE_MISMATCH)
                        {
                            error("Declaration \"" + variableName + "\": This variable cannot be initialized with a value of type <" + constantValue.type + ">, was expecting a <" + expectedType + ">.");
                        }
                        else if (variable.errorCode == HighCData.ERROR_OUT_OF_RANGE)
                        {
                            error("Declaration \"" + variableName + "\": This variable must be initialized with a value between " + (variable.type.minimum) + " and " + (variable.type.maximum) + ".");
                        }
                        return false;
                    }

                    currentEnvironment.addNewItem(variableName, variable);
                    type = constantValue.type;
                    Console.WriteLine(currentToken + " <initiated variable> -> <variable> = <constant> ->" + variableSubtype + " " + variableName + " " + variable);
                    return true;
                }

                //List
                if (variableSubtype[0] == -1)
                {
                    variable.type.memoryType = HighCType.LIST_SUBTYPE;
                    variable.type.dataType = expectedType.dataType;
                    Boolean floatFound=false;
                    List<HighCData> storeValues = new List<HighCData>();
                    
                    foreach (HighCData value in (List<HighCData>)constantValue.data)
                    {
                        HighCData newValue = new HighCData(new HighCType(HighCType.VARIABLE_SUBTYPE,expectedType.dataType,expectedType.objectReference), null);
                        newValue.type.minimum = expectedType.minimum;
                        newValue.type.maximum = expectedType.maximum;

                        if(value.isFloat())
                        {
                            floatFound = true;
                        }

                        if(newValue.setData(value)==false)
                        {
                            if (newValue.errorCode == HighCData.ERROR_TYPE_MISMATCH)
                            {
                                error("Declaration \"" + variableName + "\": This variable cannot be initialized with a value of type <" + constantValue.type + ">, was expecting a <" + expectedType + ">.");
                            }
                            else if (newValue.errorCode == HighCData.ERROR_OUT_OF_RANGE)
                            {
                                error("Declaration \"" + variableName + "\": This variable must be initialized with a value between " + (variable.type.minimum) + " and " + (variable.type.maximum) + ".");
                            }
                            else
                            {
                                error("Unknown Error 1."+ expectedType +" ");
                            }
                            return false;
                        }
                        storeValues.Add(newValue);
                    }

                    variable.data = storeValues;
                    type = variable.type;
                    if (floatFound==true)
                    {
                        type = new HighCType(HighCType.LIST_SUBTYPE, HighCType.FLOAT_TYPE);
                    }

                    currentEnvironment.addNewItem(variableName, variable);
                    Console.WriteLine(currentToken + " <initiated variable> -> <variable> = <constant> ->" + variableSubtype + " " + variableName + " " + variable);
                    return true;
                }

                //Array
                if(variableSubtype[0]>0)
                {
                    variable.type.memoryType = HighCType.ARRAY_SUBTYPE;
                    variable.type.dataType = expectedType.dataType;
                    Boolean floatFound = false;
                    
                    //Unspecified Array Cast
                    if(constantValue.isArray())
                    {
                        if(((HighCArray)constantValue.data).isSeedValue==true)
                        {
                            int size = 1;
                            foreach(int dimensionLength in variableSubtype)
                            {
                                size = size * dimensionLength;
                            }

                            if(((HighCArray)constantValue.data).array[0].isFloat())
                            {
                                floatFound = true;
                            }

                            HighCData[] array = new HighCData[size];
                            int i = 0;
                            while(i<array.Length)
                            {
                                HighCType itemType = new HighCType(HighCType.VARIABLE_SUBTYPE,
                                                                    expectedType.dataType,
                                                                    expectedType.objectReference);
                                itemType.minimum = expectedType.minimum;
                                itemType.maximum = expectedType.maximum;

                                array[i] = new HighCData(itemType, null, !isConstant, true);
                                if(array[i].setData(((HighCArray)constantValue.data).array[0])==false)
                                {
                                    if (array[i].errorCode == HighCData.ERROR_TYPE_MISMATCH)
                                    {
                                        error("Declaration \"" + variableName + "\": This variable cannot be initialized with a value of type <" + constantValue.type + ">, was expecting a <" + expectedType + ">.");
                                    }
                                    else if (array[i].errorCode == HighCData.ERROR_OUT_OF_RANGE)
                                    {
                                        error("Declaration \"" + variableName + "\": This variable must be initialized with a value between " + (variable.type.minimum) + " and " + (variable.type.maximum) + ".");
                                    }
                                    else
                                    {
                                        error("Unknown Error 2." + array[0]  + " " + ((HighCArray)constantValue.data).array[0]);
                                    }
                                    return false;
                                }
                                i++;
                            }

                            if(floatFound==true)
                            {
                                type = new HighCType(HighCType.ARRAY_SUBTYPE, HighCType.FLOAT_TYPE);
                            }
                            else
                            {
                                type = ((HighCArray)constantValue.data).array[0].type;
                            }

                            HighCArray newArray = new HighCArray(array, variableSubtype);

                            variable.data = newArray;
                            currentEnvironment.addNewItem(variableName, variable);

                            Console.WriteLine(currentToken + " <initiated variable> -> <variable> = <constant> ->" + variableSubtype + " " + variableName + " " + variable);
                            return true;
                        }
                        else
                        {
                            int size = 1;
                            int j = 0;
                            HighCArray constantArray = (HighCArray)constantValue.data;
                            
                            //Ensure Same Dimension Lengths
                            if(constantArray.dimensions.Count != variableSubtype.Count)
                            {
                                error("Declaration \"" + variableName + "\": The number of dimensions of the left hand declaration ("+ variableSubtype.Count + ") must match the right side ("+ constantArray.dimensions.Count + ").");
                                return false;
                            }

                            while (j<variableSubtype.Count)
                            {
                                if (constantArray.dimensions[j] != variableSubtype[j])
                                {
                                    error("Declaration \"" + variableName + "\": The size of each dimension must match exactly.");
                                    return false;
                                }
                                j++;
                            }

                            foreach (int dimensionLength in variableSubtype)
                            {
                                size = size * dimensionLength;
                            }

                            
                            
                            HighCData[] array = new HighCData[size];
                            int i = 0;
                            while (i < array.Length)
                            {
                                HighCType itemType = new HighCType(HighCType.VARIABLE_SUBTYPE,
                                                                    expectedType.dataType,
                                                                    expectedType.objectReference);
                                itemType.minimum = expectedType.minimum;
                                itemType.maximum = expectedType.maximum;

                                if (((HighCArray)constantValue.data).array[i].isFloat())
                                {
                                    floatFound = true;
                                }

                                array[i] = new HighCData(itemType, null, !isConstant, true);
                                if (array[i].setData(((HighCArray)constantValue.data).array[i]) == false)
                                {
                                    if (array[i].errorCode == HighCData.ERROR_TYPE_MISMATCH)
                                    {
                                        error("Declaration \"" + variableName + "\": This variable cannot be initialized with a value of type <" + constantValue.type + ">, was expecting a <" + expectedType + ">.");
                                    }
                                    else if (array[i].errorCode == HighCData.ERROR_OUT_OF_RANGE)
                                    {
                                        error("Declaration \"" + variableName + "\": This variable must be initialized with a value between " + (variable.type.minimum) + " and " + (variable.type.maximum) + ".");
                                    }
                                    else
                                    {
                                        error("Unknown Error 3." + array[i] + " " + ((HighCArray)constantValue.data).array[i]);
                                    }
                                    return false;
                                }
                                i++;
                            }

                            if (floatFound == true)
                            {
                                type = new HighCType(HighCType.ARRAY_SUBTYPE, HighCType.FLOAT_TYPE);
                            }
                            else
                            {
                                type = ((HighCArray)constantValue.data).array[0].type;
                            }

                            HighCArray newArray = new HighCArray(array, variableSubtype);

                            variable.data = newArray;
                            currentEnvironment.addNewItem(variableName, variable);

                            Console.WriteLine(currentToken + " <initiated variable> -> <variable> = <constant> ->" + variableSubtype + " " + variableName + " " + variable);
                            return true;
                        }
                    }
                    else
                    {
                        error("Declaration \"" + variableName + "\": This variable must be initialized with a list (or nested list) of elements or an Array() cast.");
                        return false;
                    }
                    //Specified Array Cast
                    //Element List
                }
            }
            return false;
        }

        private Boolean HC_integer_constant(out Int64 value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_integer_constant"); }
            value = 0;
            int sign;
            /*
             * <sign><digit><digit>*
             */

            if (HC_sign(out sign))
            {
                if (matchTerminal(HighCTokenLibrary.INTEGER_LITERAL))
                {
                    Int64.TryParse(tokenList[currentToken - 1].Text, out value);
                    value = value * sign;
                    Console.WriteLine(currentToken + " <integer constant> -> " + tokenList[currentToken - 1].Text);
                    return true;
                }
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
                if (term1.isInteger())
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
            <array – variable> <subscript – expr>*
            <list – variable> @ <slice>
            <object – variable>.<variable>
             */

            String identifier;

            if (HC_id(out identifier))
            {
                HighCData temp = currentEnvironment.getItem(identifier);
                
                if(temp == null)
                {
                    error("Integer Variable: The specified variable \"" + identifier + "\" could not be found." );
                    return false;
                }
                else if (temp.readable == false)
                {
                    error("Integer Variable: The specified variable \"" + identifier + "\" cannot be referenced." );
                    return false;
                }
                else if (temp.isInteger())
                {
                    //< list – variable > @ < slice >
                    if (temp.isList())
                    {
                        if (matchTerminal(HighCTokenLibrary.AT_SIGN))
                        {
                            int index;
                            int length;
                            if (HC_slice(out index, out length))
                            {
                                if (length != 1)
                                {
                                    error("Integer List: The length of the slice from the list must be 1.");
                                    return false;
                                }

                                if (index + length - 1 > temp.getCount())
                                {
                                    error("Integer List: The slice is not a subset of the list.");
                                    return false;
                                }

                                value = (Int64)((List<HighCData>)(temp.data))[index - 1].data;
                                Console.WriteLine(currentToken + " <integer variable> -> <id>@<slice> -> " + identifier + "@" + index + " " + value);
                                return true;
                            }
                            else
                            {
                                error();
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //<array – variable> <subscript – expr>*
                    else if (temp.isArray())
                    {
                        storeToken = currentToken;
                        int intBuffer;
                        List<int> index = new List<int>();
                        while(HC_subscript_expression(out intBuffer))
                        {
                            storeToken = currentToken;
                            index.Add(intBuffer);
                        }
                        currentToken = storeToken;
                        
                        if(((HighCArray)(temp.data)).indexInBounds(index))
                        {
                            value = (Int64)(((HighCArray)(temp.data)).getItemAt(index).data);
                            Console.WriteLine(currentToken + " <integer variable> -> <id><subscript>* -> " + identifier + " " + value);
                            return true;
                        }
                        else
                        {
                            error("Integer Array: The specified array index goes outside the bounds of the array.");
                            return false;
                        }
                    }

                    value = (Int64)temp.data;
                    Console.WriteLine(currentToken + " <integer variable> -> <id> -> " + identifier+" "+ value);
                    return true;
                }
            }

            return false;
        }

        private Boolean HC_iterator()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_iterator"); }
            int storeToken = currentToken;
            /*
            for ( <discrete type> <id> <dir> <expr> … <expr> ) <block>  
            for[] ( <type> <id> <dir> <array expr> ) <block>            Foreach Array
            for@ ( <type> <id> <dir> <list expr> ) <block>              Foreach List
             */

            HighCType type;
            String identifier;
            Boolean direction;

            if (matchTerminal(HighCTokenLibrary.FOR))
            {
                //for@ ( <type> <id> <dir> <list expr> ) <block>              Foreach List
                storeToken = currentToken;
                if (matchTerminal(HighCTokenLibrary.AT_SIGN))
                {
                    if (matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
                    {
                        if (HC_type(out type))
                        {
                            if (HC_id(out identifier))
                            {
                                if(globalEnvironment.contains(identifier))
                                {
                                    error("For@: The identifier \""+identifier+"\" cannot be reused, it is already declared in the global scope.");
                                    return false;
                                }

                                if (HC_dir(out direction))
                                {
                                    HighCData list;
                                    if (HC_list_expression(out list))
                                    {
                                        if (matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true))
                                        {
                                            List<HighCData> items = (List<HighCData>)list.data;
                                            if(items.Count==0)
                                            {
                                                if(skipBlock()==false)
                                                {
                                                    error("For@: Expected a block \"{ }\".");
                                                    return false;
                                                }
                                            }
                                            else
                                            {
                                                int i = items.Count-1;
                                                int startOfBlock = currentToken;
                                                int limit = -1;
                                                int offset = -1;
                                                if(direction==true)
                                                {
                                                    i = 0;
                                                    limit = items.Count;
                                                    offset = 1;
                                                }
                                                while (i != limit)
                                                {
                                                    currentToken = startOfBlock;
                                                    HighCEnvironment newEnvironment = new HighCEnvironment(currentEnvironment);
                                                    HighCData newItem = new HighCData(type);
                                                    if(newItem.setData(items[i])==false)
                                                    {
                                                        if (newItem.errorCode == HighCData.ERROR_TYPE_MISMATCH)
                                                        {
                                                            error("For@ \"" + identifier + "\": This variable cannot be initialized with a value of type <" + items[i].type + ">, was expecting a <" + type + ">.");
                                                        }
                                                        else if (newItem.errorCode == HighCData.ERROR_OUT_OF_RANGE)
                                                        {
                                                            error("For@ \"" + identifier + "\": This variable must be initialized with a value between " + (type.minimum) + " and " + (type.maximum) + ".");
                                                        }
                                                        error();
                                                        return false;
                                                    }
                                                    newEnvironment.addNewItem(identifier, newItem);
                                                    if(HC_block(newEnvironment)==false)
                                                    {
                                                        error("For@: Expected a block \"{ }\".");
                                                        return false;
                                                    }
                                                    
                                                    i+=offset;
                                                }
                                            }
                                            
                                            return true;
                                        }
                                        else
                                        {
                                            error();
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        error("For@: Expected a list.");
                                        return false;
                                    }
                                }
                                else
                                {
                                    error("For@: Expected \"" + HighCTokenLibrary.IN + "\" or \"" + HighCTokenLibrary.IN_REVERSE + "\".");
                                    return false;
                                }
                            }
                            else
                            {
                                error("For@: Expected an identifier.");
                                return false;
                            }
                        }
                        else
                        {
                            error();
                            return false;
                        }
                    }
                    else
                    {
                        error();
                        return false;
                    }
                }
                else
                {
                    currentToken = storeToken;
                }

                //for[] ( <type> <id> <dir> <array expr> ) <block>            Foreach Array
                if (matchTerminal(HighCTokenLibrary.LEFT_SQUARE_BRACKET))
                {
                    if(matchTerminal(HighCTokenLibrary.RIGHT_SQUARE_BRACKET,true)==false)
                    {
                        error();
                        return false;
                    }
                    if (matchTerminal(HighCTokenLibrary.LEFT_PARENTHESIS, true))
                    {
                        if (HC_type(out type))
                        {
                            if (HC_id(out identifier))
                            {
                                if (globalEnvironment.contains(identifier))
                                {
                                    error("For[]: The identifier \"" + identifier + "\" cannot be reused, it is already declared in the global scope.");
                                    return false;
                                }

                                if (HC_dir(out direction))
                                {
                                    HighCData array;
                                    if (HC_array_expression(out array))
                                    {
                                        if (matchTerminal(HighCTokenLibrary.RIGHT_PARENTHESIS, true))
                                        {
                                            HighCData[] items = ((HighCArray)array.data).array;

                                            if (items.Length == 0)
                                            {
                                                if (skipBlock() == false)
                                                {
                                                    error("For@: Expected a block \"{ }\".");
                                                    return false;
                                                }
                                            }
                                            else
                                            {
                                                int i = items.Length - 1;
                                                int startOfBlock = currentToken;
                                                int limit = -1;
                                                int offset = -1;
                                                if (direction == true)
                                                {
                                                    i = 0;
                                                    limit = items.Length;
                                                    offset = 1;
                                                }
                                                while (i != limit)
                                                {
                                                    currentToken = startOfBlock;
                                                    HighCEnvironment newEnvironment = new HighCEnvironment(currentEnvironment);
                                                    HighCData newItem = new HighCData(type);
                                                    if (newItem.setData(items[i]) == false)
                                                    {
                                                        if (newItem.errorCode == HighCData.ERROR_TYPE_MISMATCH)
                                                        {
                                                            error("For[] \"" + identifier + "\": This variable cannot be initialized with a value of type <" + items[i].type + ">, was expecting a <" + type + ">.");
                                                        }
                                                        else if (newItem.errorCode == HighCData.ERROR_OUT_OF_RANGE)
                                                        {
                                                            error("For[] \"" + identifier + "\": This variable must be initialized with a value between " + (type.minimum) + " and " + (type.maximum) + ".");
                                                        }
                                                        error();
                                                        return false;
                                                    }
                                                    newEnvironment.addNewItem(identifier, newItem);
                                                    if (HC_block(newEnvironment) == false)
                                                    {
                                                        error("For[]: Expected a block \"{ }\".");
                                                        return false;
                                                    }

                                                    i += offset;
                                                }
                                            }

                                            return true;
                                        }
                                        else
                                        {
                                            error();
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        error("For[]: Expected an array.");
                                        return false;
                                    }
                                }
                                else
                                {
                                    error("For[]: Expected \"" + HighCTokenLibrary.IN + "\" or \"" + HighCTokenLibrary.IN_REVERSE + "\".");
                                    return false;
                                }
                            }
                            else
                            {
                                error("For[]: Expected an identifier.");
                                return false;
                            }
                        }
                        else
                        {
                            error();
                            return false;
                        }
                    }
                    else
                    {
                        error();
                        return false;
                    }
                }
                else
                {
                    currentToken = storeToken;
                }
            }
         

            return false;
        }

        private Boolean HC_label(HighCData value, out Boolean matchFound, out String label)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_label"); }
            int storeToken = currentToken;
            matchFound = false;
            HighCData labelData;
            HighCData labelData2;
            label = "No Value";

            /*
             <constant>
             <constant> … <constant>
             */

            if (HC_constant(out labelData))
            {
                storeToken = currentToken;
                if (matchTerminal(HighCTokenLibrary.ELLIPSES))
                {
                    if (value.isBoolean() == false)
                    {
                        if (HC_constant(out labelData2))
                        {
                            if (value.type.isEqualToType(labelData.type) &&
                                value.type.isEqualToType(labelData2.type))
                            {
                                if (value.isBoolean())
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

                                if (value.isInteger())
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

                                if (value.isCharacter())
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
                                addDebugInfo(HighCTokenLibrary.ON + ": The label types (" + labelData.type + "), (" + labelData2.type + ") must match the type for \"" + HighCTokenLibrary.CHOICE + "\" (" + value.type + ")" );
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        addDebugInfo(HighCTokenLibrary.ON + ": " + HighCTokenLibrary.BOOLEAN+" types cannot utilize the range case specifier ("+HighCTokenLibrary.ELLIPSES+").");

                        return false;
                    }
                }
                else
                {
                    currentToken = storeToken;
                    if (value.type.isEqualToType(labelData.type))
                    {
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
                        error(HighCTokenLibrary.ON + ": The label type (" + labelData.type + ") must match the type for \"" + HighCTokenLibrary.CHOICE + "\" (" + value.type + ")" );
                        label = "No Value";
                        return false;
                    }
                }
            }

            label = "No Value";
            return false;
        }
        
        private Boolean HC_list_command()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_list_command"); }
            int storeToken = currentToken;

            /*
            append <list – id> = <element or list>
            insert <list – id> @ <int – expr> = <element or list>
            remove <list – id> @ <slice>
             */
            
            if(matchTerminal(HighCTokenLibrary.APPEND))
            {
                String identifier;
                if(HC_id(out identifier))
                {
                    if (currentEnvironment.contains(identifier))
                    {
                        HighCData list = currentEnvironment.getItem(identifier);

                        if(list.isList()==false)
                        {
                            error(HighCTokenLibrary.APPEND + ": The provided identifier \"" + identifier + "\" is not a list.");
                            return false;
                        }

                        if(matchTerminal(HighCTokenLibrary.EQUAL,true)==false)
                        {
                            error();
                            return false;
                        }

                        HighCData newItems;
                        if(HC_element_or_list(out newItems))
                        {
                            if(list.type.dataType==newItems.type.dataType &&
                               list.type.objectReference==newItems.type.objectReference)
                            {
                                foreach(HighCData value in ((List<HighCData>)newItems.data))
                                {
                                    HighCData newValue = new HighCData(new HighCType(HighCType.VARIABLE_SUBTYPE, list.type.dataType, list.type.objectReference), null);
                                    newValue.type.minimum = list.type.minimum;
                                    newValue.type.maximum = list.type.maximum;

                                    if(newValue.setData(value)==false)
                                    {
                                        if (newValue.errorCode == HighCData.ERROR_CONSTANT_CHANGED)
                                        {
                                            error(HighCTokenLibrary.APPEND + " (\"" + identifier + "\"): The specified identifier is a constant which cannot be changed after declaration.");
                                        }
                                        else if (newValue.errorCode == HighCData.ERROR_TYPE_MISMATCH)
                                        {
                                            error(HighCTokenLibrary.APPEND + " (\"" + identifier + "\"): This variable cannot be initialized with a value of type <" + value.type + ">, was expecting a <" + list.type + ">.");
                                        }
                                        else if (newValue.errorCode == HighCData.ERROR_OUT_OF_RANGE)
                                        {
                                            error(HighCTokenLibrary.APPEND + " (\"" + identifier + "\"): This variable must be initialized with a value between " + (newValue.type.minimum) + " and " + (newValue.type.maximum) + ".");
                                        }
                                        return false;
                                    }

                                    ((List<HighCData>)(list.data)).Add(newValue);
                                }

                                Console.WriteLine(currentToken + " <list command> -> append <list – id> = <element or list> -> " + list);

                                return true;
                            }
                            else
                            {
                                error(HighCTokenLibrary.APPEND + ": The type of the specified list <" + list.type+"> does not match <"+newItems.type+">.");
                                return false;
                            }
                        }
                        else
                        {
                            error(HighCTokenLibrary.APPEND + ": No list or item found to append.");
                            return false;
                        }
                    }
                    else
                    {
                        error(HighCTokenLibrary.APPEND + ": The provided identifier \"" + identifier+"\" could not be found.");
                        return false;
                    }
                }
                else
                {
                    error(HighCTokenLibrary.APPEND + ": Expected an identifier.");
                    return false;
                }
            }

            //insert < list – id > @ < int – expr > = < element or list>
            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.INSERT))
            {
                String identifier;
                if (HC_id(out identifier))
                {
                    if (currentEnvironment.contains(identifier))
                    {
                        HighCData list = currentEnvironment.getItem(identifier);

                        if (list.isList() == false)
                        {
                            error(HighCTokenLibrary.INSERT + ": The provided identifier \"" + identifier + "\" is not a list.");
                            return false;
                        }

                        if (matchTerminal(HighCTokenLibrary.AT_SIGN, true) == false)
                        {
                            error();
                            return false;
                        }

                        Int64 intBuffer;
                        if(HC_integer_expression(out intBuffer)==false)
                        {
                            error(HighCTokenLibrary.INSERT + ": Expected an integer value to specify the list position to insert after.");
                            return false;
                        }

                        if(intBuffer<1 || (intBuffer > list.getCount() && intBuffer>1 && list.getCount()!=0))
                        {
                            error(HighCTokenLibrary.INSERT + ": Integer value must be between 1 and "+ ((List<HighCData>)list.data).Count + ".");
                            return false;
                        }

                        if (matchTerminal(HighCTokenLibrary.EQUAL, true) == false)
                        {
                            error();
                            return false;
                        }

                        HighCData newItems;
                        if (HC_element_or_list(out newItems))
                        {
                            if (list.type.dataType == newItems.type.dataType &&
                               list.type.objectReference == newItems.type.objectReference)
                            {
                                int offset = 0;
                                int index = (int)intBuffer - 1;
                                foreach (HighCData value in ((List<HighCData>)newItems.data))
                                {
                                    HighCData newValue = new HighCData(new HighCType(HighCType.VARIABLE_SUBTYPE, list.type.dataType, list.type.objectReference), null);
                                    newValue.type.minimum = list.type.minimum;
                                    newValue.type.maximum = list.type.maximum;

                                    if (newValue.setData(value) == false)
                                    {
                                        if (newValue.errorCode == HighCData.ERROR_CONSTANT_CHANGED)
                                        {
                                            error(HighCTokenLibrary.INSERT + " (\"" + identifier + "\"): The specified identifier is a constant which cannot be changed after declaration.");
                                        }
                                        else if (newValue.errorCode == HighCData.ERROR_TYPE_MISMATCH)
                                        {
                                            error(HighCTokenLibrary.INSERT + " (\"" + identifier + "\"): This variable cannot be initialized with a value of type <" + value.type + ">, was expecting a <" + list.type + ">.");
                                        }
                                        else if (newValue.errorCode == HighCData.ERROR_OUT_OF_RANGE)
                                        {
                                            error(HighCTokenLibrary.INSERT + " (\"" + identifier + "\"): This variable must be initialized with a value between " + (newValue.type.minimum) + " and " + (newValue.type.maximum) + ".");
                                        }
                                        return false;
                                    }

                                    ((List<HighCData>)(list.data)).Insert(index+offset, value);
                                    offset++;
                                }

                                Console.WriteLine(currentToken + " <list command> -> append <list – id> = <element or list> -> " + list);

                                return true;
                            }
                            else
                            {
                                error(HighCTokenLibrary.INSERT + ": The type of the specified list <" + list.type + "> does not match <" + newItems.type + ">.");
                                return false;
                            }
                        }
                        else
                        {
                            error(HighCTokenLibrary.INSERT + ": No list or item found to insert.");
                            return false;
                        }
                    }
                    else
                    {
                        error(HighCTokenLibrary.INSERT + ": The provided identifier \"" + identifier + "\" could not be found.");
                        return false;
                    }
                }
                else
                {
                    error(HighCTokenLibrary.INSERT + ": Expected an identifier.");
                    return false;
                }
            }

            //remove < list – id > @ < slice >
            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.REMOVE))
            {
                String identifier;
                if (HC_id(out identifier))
                {
                    if (currentEnvironment.contains(identifier))
                    {
                        HighCData list = currentEnvironment.getItem(identifier);

                        if (list.isList() == false)
                        {
                            error(HighCTokenLibrary.REMOVE + ": The provided identifier \"" + identifier + "\" is not a list.");
                            return false;
                        }

                        if (matchTerminal(HighCTokenLibrary.AT_SIGN, true) == false)
                        {
                            error();
                            return false;
                        }

                        int startingPosition;
                        int length;
                        if (HC_slice(out startingPosition, out length))
                        {
                            if(list.getCount()==0)
                            {
                                error(HighCTokenLibrary.REMOVE + ": An empty list cannot have items removed from it.");
                                return false;
                            }
                            else if(startingPosition<=0 || startingPosition > list.getCount())
                            {
                                error(HighCTokenLibrary.REMOVE + ": The starting position to remove from the list must be between 1 and "+ list.getCount() + ".");
                                return false;
                            }
                            else if(startingPosition+length-1 > list.getCount())
                            {
                                error(HighCTokenLibrary.REMOVE + ": The second index must be less than " + list.getCount() + ".");
                                return false;
                            }
                            ((List<HighCData>)(list.data)).RemoveRange(startingPosition - 1, length);
                            Console.WriteLine(currentToken + " <list command> -> remove <list – id> @ <slice> -> " + list);
                            return true;
                        }
                        else
                        {
                            error("Huh");
                            return false;
                        }
                    }
                    else
                    {
                        error(HighCTokenLibrary.REMOVE + ": The provided identifier \"" + identifier + "\" could not be found.");
                        return false;
                    }
                }
                else
                {
                    error(HighCTokenLibrary.REMOVE + ": Expected an identifier.");
                    return false;
                }
            }

            return false;
        }

        private Boolean HC_list_constant(out List<HighCData> values)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_list_constant"); }
            int storeToken = currentToken;
            values = null;

            /*
             * { }
               { <element – constant list> }
             */

            if (matchTerminal(HighCTokenLibrary.LEFT_CURLY_BRACKET))
            {
                values = new List<HighCData>();

                storeToken = currentToken;
                HighCData newElement;
                if (HC_element_constant(out newElement))
                { 
                    storeToken = currentToken;
                    values.Add(newElement);

                    while (matchTerminal(HighCTokenLibrary.COMMA))
                    {
                        storeToken = currentToken;
                        if (HC_element_constant(out newElement))
                        {
                            storeToken = currentToken;
                            values.Add(newElement);
                        }
                        else
                        {
                            error("List: another element was expected after the comma.");
                            return false;
                        }
                    }
                }

                currentToken = storeToken;
                if (matchTerminal(HighCTokenLibrary.RIGHT_CURLY_BRACKET,true))
                {
                    Console.WriteLine(currentToken + " <list constant> -> { <element constant list> } -> " + values);
                    return true;
                }
                else
                {
                    error();
                    return false;
                }
            }
            
            return false;
        }

        private Boolean HC_list_expression(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_list_expression"); }
            int storeToken = currentToken;
            value = null;

            /*
            <list – constant>
            <list – variable>
            <list – function call>
            {<element – expression list>}
             */
            
            List<HighCData> values;
            if(HC_list_constant(out values))
            {
                if(values.Count>0)
                {
                    value = new HighCData(new HighCType(HighCType.LIST_SUBTYPE, values[0].type.dataType,values[0].type.objectReference), values);
                }
                else
                {
                    value = new HighCData(new HighCType(HighCType.LIST_SUBTYPE, HighCType.VOID_TYPE), values);
                }
                Console.WriteLine(currentToken + " <list expression> -> <list constant>" + " -> " + value);
                return true;
            }

            currentToken = storeToken;
            if (HC_list_variable(out values))
            {
                if (values.Count > 0)
                {
                    value = new HighCData(new HighCType(HighCType.LIST_SUBTYPE, values[0].type.dataType, values[0].type.objectReference), values);
                }
                else
                {
                    value = new HighCData(new HighCType(HighCType.LIST_SUBTYPE, HighCType.VOID_TYPE), values);
                }
                Console.WriteLine(currentToken + " <list expression> -> <list variable>" + " -> " + value);
                return true;
            }
            
            currentToken = storeToken;
            if (HC_function_call(out value, new HighCType(HighCType.LIST_SUBTYPE, null)))
            {
                Console.WriteLine(currentToken + " <list expression> -> <list function call>" + " -> " + value);
                return true;
            }
            /*

            currentToken = storeToken;
            if(matchTerminal(HighCTokenLibrary.LEFT_CURLY_BRACKET))
            {
                storeToken = currentToken;
                List<HighCData> dataItems = new List<HighCData>();
                HighCData itemBuffer;
                if(HC_element_expression(out itemBuffer))
                {
                    dataItems.Add(itemBuffer);
                }
                else
                {
                    currentToken = storeToken;
                }

                storeToken = currentToken;
                while (matchTerminal(HighCTokenLibrary.COMMA))
                {
                    if (HC_element_expression(out itemBuffer))
                    {
                        storeToken = currentToken;
                        dataItems.Add(itemBuffer);
                    }
                    else
                    {
                        error("List: Another item was expected after the comma.");
                        return false;
                    }
                }
                currentToken = storeToken;

                if(matchTerminal(HighCTokenLibrary.RIGHT_CURLY_BRACKET))
                {

                    Console.WriteLine(currentToken + " <list expression> -> {<Element Expression List>}" + " -> " + value);
                    return true;
                }
                else
                {
                    error();
                    return false;
                }
            }
            */
            return false;
        }

        private Boolean HC_list_variable(out List<HighCData> values)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_integer_variable"); }

            int storeToken = currentToken;
            values = null;
            /*
            <id>
            <array – variable> <subscript – expr>* [ <slice> ]
            <list – variable> @ <slice>
            <object – variable>.<variable>
             */

            String identifier;

            if (HC_id(out identifier))
            {
                HighCData temp = currentEnvironment.getItem(identifier);

                if (temp == null)
                {
                    error("List Variable: The specified variable \"" + identifier + "\" could not be found.");
                    return false;
                }
                else if (temp.readable == false)
                {
                    error("Integer Variable: The specified variable \"" + identifier + "\" cannot be referenced.");
                    return false;
                }
                else if (temp.isList())
                {
                    values = (List<HighCData>)temp.data;
                    storeToken = currentToken;
                    if(matchTerminal(HighCTokenLibrary.AT_SIGN))
                    {
                        int index, length;
                        if(HC_slice(out index, out length))
                        {
                            if(index+length-1<=values.Count)
                            {
                                values = values.GetRange(index-1, length);
                                Console.WriteLine(currentToken + " <list variable> -> <id>@<slice> -> " + identifier + " " + values);
                                return true;
                            }
                            else if(values.Count==0)
                            {
                                error("List Variable: Empty lists cannot be accessed in this way.");
                                return false;
                            }
                            else
                            {
                                error("List Variable: The specified slice must be between 1 and "+values.Count+".");
                                return false;
                            }
                        }
                    }
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <list variable> -> <id> -> " + identifier + " " + values);
                    return true;
                }
            }

            return false;
        }

        private Boolean HC_loop()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_loop"); }
            int storeToken = currentToken;

            /*
             * loop { <declaration>* <statement>* until ( <bool-expr> ) <statement>* }
             */
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
                        HighCData term;
                        if (HC_scalar_expression(out term) &&
                            term.isBoolean())
                        {
                            boolTerm1 = (Boolean)term.data;
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
                            error(HighCTokenLibrary.UNTIL + ": A boolean expression was expected inside the parenthesis." );
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
            if (HC_scalar_expression(out value))
            {
                stringBuffer = value.data.ToString();

                storeToken = currentToken;
                if (matchTerminal(HighCTokenLibrary.COLON))
                {
                    if(HC_integer_expression(out term1))
                    {
                        if (stringBuffer.Length < term1)
                        {
                            stringBuffer = stringBuffer.PadRight((int)term1);
                        }
                        Console.WriteLine(currentToken + " <out element> -> <scalar expression>:<integer expression>" + " -> " + stringBuffer);
                        return true;
                    }
                }
                else
                {
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <out element> -> <scalar expression>" + " -> " + stringBuffer);
                    return true;
                }
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
                Console.WriteLine(currentToken + " <out element> -> endl");
                return true;
            }

            //MAY CAUSE UNEXPECTED SIDE EFFECTS, KEEP AN EYE OUT
            currentToken = storeToken;
            if(HC_expression(out value))
            {
                if(value.isBoolean())
                {
                    stringBuffer = value.data.ToString();
                    return true;
                }
                else
                {
                    currentToken = storeToken;
                }
            }

            return false;
        }

        private Boolean HC_output()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_output"); }
            int storeToken = currentToken;

            /*
            out <out – element list>
             */

            if (matchTerminal(HighCTokenLibrary.OUT) == false) { return false; }
            
            if(pureStatus==true)
            {
                error(HighCTokenLibrary.OUT + ": This statement cannot be used in a pure function." );
                return false;
            }

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
                error(HighCTokenLibrary.OUT + ": another element was expected after the comma." );
                return false;
            }

            if (atLeastOneFound == false)
            {
                error(HighCTokenLibrary.OUT + ": at least one element was expected." );
                return false;
            }
            else
            {
                Console.WriteLine(currentToken + " <output> -> <out element>" + " -> " + stringBuffer.Replace(Environment.NewLine,HighCTokenLibrary.END_OF_LINE));
                
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
            HighCType type = null;
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
                            type.memoryType = HighCType.LIST_SUBTYPE;
                            parameter = new HighCParameter(id, type, inAllowed, outAllowed, parameterIDList);
                            Console.WriteLine(currentToken + " <parameter> -> <direction><type><id>@ ->"+parameter);
                            return true;
                        }

                        currentToken = storeToken;
                        
                        String currentID;
                        while(HC_subscript_parameter(out currentID))
                        {
                            type.memoryType = HighCType.ARRAY_SUBTYPE;
                            storeToken = currentToken;
                            parameterIDList.Add(currentID);
                            Console.WriteLine(currentToken + " <parameter> -> <direction><type><id><subscript parameter>* ->" + parameter);
                        }

                        currentToken = storeToken;

                        parameter = new HighCParameter(id, type, inAllowed, outAllowed, parameterIDList);
                        Console.WriteLine(currentToken + " <parameter> -> <direction><type><id> ->" + parameter);
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

        private Boolean HC_relational_expression(HighCData term, out Boolean value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_relational_expression"); }
            int storeToken = currentToken;

            /*
            <arith – expr> <rel – op> <arith – expr>
            <string – expr> <rel – op> <string – expr>
            <char – expr> <rel – op> <char – expr>
            <enum – expr> <rel – op> <enum – expr>
            <list – expr> <eq – op> <list – expr>

            <bool – expr> <eq – op> <bool – expr> moved to <boolean expression>

            <array – expr> <eq – op> <bool – expr>
            <object – expr> <eq – op> <object – expr>
            <object – expr> instof <class name>
             */

            String opType;
            HighCData term2 = null;
            value = false;

            if (term==null)
            {
                return false;
            }

            if (HC_relational_op(out opType))
            {
                if (term.isList())
                {
                    if (opType == HighCTokenLibrary.EQUAL ||
                        opType == HighCTokenLibrary.NOT_EQUAL)
                    {
                        if (HC_list_expression(out term2))
                        {
                            List<HighCData> list1 = (List<HighCData>)term.data;
                            List<HighCData> list2 = (List<HighCData>)term2.data;

                            if (list1.Count != list2.Count)
                            {
                                error("Relation Expression: The length of the left list (" + list1.Count + ") does not match the length of the right list (" + list2.Count + ").");
                                return false;
                            }

                            int i = 0;
                            while (i < list1.Count)
                            {
                                if (list1[i].compare(list2[i], opType, out value))
                                {
                                    if (value == false)
                                    {
                                        return true;
                                    }
                                }
                                else
                                {
                                    if (list1[i].errorCode == HighCData.ERROR_TYPE_MISMATCH)
                                    {
                                        error("Relation Expression: The type <" + list1[i].type + "> cannot be compared to <" + list2[i].type + ">.");
                                    }
                                    else
                                    {
                                        error("Relation Expression: Unknown Error.");
                                    }
                                    return false;
                                }
                                i++;
                            }

                            return true;
                        }
                        else
                        {
                            error("Relational Expression: A second " + HighCTokenLibrary.LIST + " value was expected after \"" + opType + "\".");
                            return false;
                        }
                    }
                    else
                    {
                        error("Relation Expression: Lists can only be compared with the \"" + HighCTokenLibrary.EQUAL + "\" or \"" + HighCTokenLibrary.NOT_EQUAL + " operators.");
                        return false;
                    }
                }
                else if (term.isNumericType())
                {
                    if (HC_arithmetic_expression(ref term2) == false)
                    {
                        error("Relational Expression: A second numeric value was expected after \"" + opType + "\".");
                        return false;
                    }

                    term.compare(term2, opType, out value);

                    Console.WriteLine(currentToken + " <relational expression> -> <arithmetic expression><relational op><arithmetic expression>" + " -> " + value);
                    return true;
                }
                else if (term.isString())
                {
                    String stringTerm;
                    if (HC_string_expression(out stringTerm))
                    {
                        term2 = new HighCData(HighCType.STRING_TYPE, stringTerm);

                        term.compare(term2, opType, out value);

                        Console.WriteLine(currentToken + " <relational expression> -> <string expression><relational op><string expression>" + " -> " + value);
                        return true;
                    }
                    else
                    {
                        error("Relational Expression: A second " + HighCTokenLibrary.STRING + " value was expected after \"" + opType + "\".");
                        return false;
                    }
                }
                else if (term.isCharacter())
                {
                    String stringTerm;
                    if (HC_character_expression(out stringTerm))
                    {
                        term2 = new HighCData(HighCType.STRING_TYPE, stringTerm);

                        term.compare(term2, opType, out value);

                        Console.WriteLine(currentToken + " <relational expression> -> <character expression><relational op><character expression>" + " -> " + value);
                        return true;
                    }
                    else
                    {
                        error("Relational Expression: A second " + HighCTokenLibrary.CHARACTER + " value was expected after \"" + opType + "\".");
                        return false;
                    }
                }
                else if (term.isEnumeration())
                {
                    HighCEnumeration enumTerm;
                    if (HC_enumeration_expression(out enumTerm))
                    {
                        term2 = new HighCData(new HighCType(HighCType.VARIABLE_SUBTYPE, HighCType.ENUMERATION_INSTANCE, enumTerm.type), enumTerm);

                        if (term.compare(term2, opType, out value) == true)
                        {
                            Console.WriteLine(currentToken + " <relational expression> -> <character expression><relational op><character expression>" + " -> " + value);
                            return true;
                        }
                        else
                        {
                            error("Relational Expression (Enumeration): The type of the first argument <" + term.type + "> does not match the type of second <" + term2.type + ">.");
                            return false;
                        }
                    }
                    else
                    {
                        error("Relational Expression: A second " + HighCTokenLibrary.CHARACTER + " value was expected after \"" + opType + "\".");
                        return false;
                    }
                }
                else
                {
                    error("Relation Expression: Unknown Type.");
                    return false;
                }
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
        
        private Boolean HC_result(out HighCType type)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_result"); }
            int storeToken = currentToken;

            /*
            void
            <return type>
             */

            type = null;

            if(matchTerminal(HighCTokenLibrary.VOID))
            {
                type = new HighCType(HighCType.VOID_TYPE);
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
        
        private Boolean HC_return()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_return"); }
            int storeToken = currentToken;
            HighCData value;

            /*
            return <expr>
            return
             */

            if(matchTerminal(HighCTokenLibrary.RETURN) &&
                HC_expression(out value))
            {
                returnValue = value;
                returnFlag = true;
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.RETURN))
            {
                returnValue = null;
                returnFlag = true;
                return true;
            }

            return false;
        }

        private Boolean HC_return_type(out HighCType type)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_scalar_expression"); }
            int storeToken = currentToken;

            /*
            <type> <return – subscript>*
            <type> @
             */

            type = null;

            if(HC_type(out type))
            {
                storeToken = currentToken;
                if(matchTerminal(HighCTokenLibrary.AT_SIGN))
                {
                    type.memoryType = HighCType.LIST_SUBTYPE;    
                }

                return true;
            }

            return false;
        }

        private Boolean HC_scalar_constant(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_scalar_expression"); }
            int storeToken = currentToken;
            value = null;

            /*
            <discrete – constant>
            <string – constant>
            <float – constant>
             */
             
            String stringTerm1 = "";
            Double doubleTerm1 = 0.0;
            
            if (HC_discrete_constant(out value))
            {
                Console.WriteLine(currentToken + " <scalar constant> -> <discrete constant>" + " -> " + value);
                return true;
            }
            
            currentToken = storeToken;
            if (HC_string_constant(out stringTerm1))
            {
                value = new HighCData(HighCTokenLibrary.STRING, stringTerm1);
                Console.WriteLine(currentToken + " <scalar constant> -> <string constant>" + " -> " + value);
                return true;
            }
            
            currentToken = storeToken;
            if (HC_float_constant(out doubleTerm1))
            {
                value = new HighCData(HighCTokenLibrary.FLOAT, doubleTerm1);
                Console.WriteLine(currentToken + " <scalar constant> -> <float constant>" + " -> " + value);
                return true;
            }
            
            return false;
        }

        private Boolean HC_scalar_expression(out HighCData value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_scalar_expression"); }
            int storeToken = currentToken;

            /*
            <discrete expression>
            <arith – expr> - Should be float only
            <string – expr>

            Due to some issues with multiple function calls, relational expressions will be initiated
            from here for these types.
             */

            HighCData term1 = null;
            Boolean boolTerm = false;
            String stringTerm = "";
            
            if (HC_arithmetic_expression(ref term1) == true)
            {
                value = term1;

                storeToken = currentToken;
                if (HC_relational_expression(value, out boolTerm))
                {
                    value = new HighCData(HighCType.BOOLEAN_TYPE, boolTerm);
                    Console.WriteLine(currentToken + " <scalar expression> -> <arithmetic expression><relational op><arithmetic expression>" + " -> " + value.ToString());
                    return true;
                }
                else
                {
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <scalar expression> -> <arithmetic expression>" + " -> " + value.ToString());
                    return true;
                }
            }
            
            currentToken = storeToken;
            if (HC_discrete_expression(out value))
            {
                Console.WriteLine(currentToken + " <scalar expression> -> <discrete expression>" + " -> " + value.ToString());
                return true;
            }

            currentToken = storeToken;
            if (HC_string_expression(out stringTerm) == true)
            {
                value = new HighCData(HighCTokenLibrary.STRING, stringTerm);

                storeToken = currentToken;
                if (HC_relational_expression(value, out boolTerm))
                {
                    value = new HighCData(HighCType.BOOLEAN_TYPE, boolTerm);
                    Console.WriteLine(currentToken + " <scalar expression> -> <string expression><relational op><string expression>" + " -> " + value.ToString());
                    return true;
                }
                else
                {
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <scalar expression> -> <string expression>" + " -> " + value.ToString());
                    return true;
                }
            }

            /*
            


            currentToken = storeToken;
            if (HC_character_expression(out stringTerm) == true)
            {
                value = new HighCData(HighCTokenLibrary.STRING, stringTerm);

                storeToken = currentToken;
                if (HC_relational_expression(value, out boolTerm))
                {
                    value = new HighCData(HighCType.BOOLEAN_TYPE, boolTerm);
                    Console.WriteLine(currentToken + " <scalar expression> -> <character expression><relational op><character expression>" + " -> " + value.ToString());
                    return true;
                }
                else
                {
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <scalar expression> -> <character expression>" + " -> " + value.ToString());
                    return true;
                }
            }

            currentToken = storeToken;
            HighCEnumeration enumBuffer;
            if (HC_enumeration_expression(out enumBuffer) == true)
            {

                value = new HighCData(new HighCType(HighCType.VARIABLE_SUBTYPE, HighCType.ENUMERATION_INSTANCE, enumBuffer.type), enumBuffer);

                storeToken = currentToken;
                if (HC_relational_expression(value, out boolTerm))
                {
                    value = new HighCData(HighCType.BOOLEAN_TYPE, boolTerm);
                    Console.WriteLine(currentToken + " <scalar expression> -> <enum expression><relational op><character expression>" + " -> " + value.ToString());
                    return true;
                }
                else
                {
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <scalar expression> -> <enum expression>" + " -> " + value.ToString());
                    return true;
                }
            }

            currentToken = storeToken;
            if (HC_boolean_expression(ref boolTerm) == true)
            {
                value = new HighCData(HighCTokenLibrary.BOOLEAN, boolTerm);
                Console.WriteLine(currentToken + " <scalar expression> -> <boolean expression>" + " -> " + value.ToString());
                return true;
            }
            */
            value = null;
            return false;
        }

        private Boolean HC_scalar_type(out HighCType type)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_scalar_type"); }
            int storeToken = currentToken;
            type = null;

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
                type = new HighCType(HighCType.STRING_TYPE);
                Console.WriteLine(currentToken + " <scalar type> -> " + type);
                return true;
            }

            currentToken = storeToken;
            Int64 intTerm1;
            if (matchTerminal(HighCTokenLibrary.FLOAT))
            {
                type = new HighCType(HighCType.FLOAT_TYPE);
                storeToken = currentToken;

                if (matchTerminal(HighCTokenLibrary.COLON))
                {
                    if (HC_integer_constant(out intTerm1))
                    {
                        if (intTerm1 > 0)
                        {
                            type.precision = (int)intTerm1;
                            //term1 = new HighCData(HighCData.INTEGER_TYPE, intTerm1);
                            Console.WriteLine(currentToken + " <scalar type> -> " + type);
                            return true;
                        }
                        else
                        {
                            error(HighCTokenLibrary.FLOAT + HighCTokenLibrary.COLON + ": The number of decimals to show must be at least 1." );
                            return false;
                        }
                    }
                    else
                    {
                        error(HighCTokenLibrary.FLOAT+": Expected a positive <"+HighCTokenLibrary.INTEGER+"> value after the colon to indicate the precision of the preceding float variable.");
                        return false;
                    }
                }
                else
                {
                    currentToken = storeToken;
                    Console.WriteLine(currentToken + " <scalar type> -> " + type);
                    return true;
                }
            }
            
            return false;
        }

        private Boolean HC_sign(out int signType)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_sign"); }
            int storeToken = currentToken;
            signType = 1;
            /*
            ε
            +
            -
             */

            if(matchTerminal(HighCTokenLibrary.PLUS_SIGN))
            {
                return true;
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.MINUS_SIGN))
            {
                signType = -1;
                return true;
            }

            currentToken = storeToken;
            return true;
        }

        private Boolean HC_slice(out int startingPosition, out int length)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_slice"); }
            int storeToken = currentToken;
            startingPosition = 0;
            length = 1;
            /*
            <int – expr>
            <int – expr> … <int - expr>
             */

            Int64 intTerm1;
            Int64 intTerm2;
            if(HC_integer_expression(out intTerm1))
            {
                startingPosition = (int)intTerm1;

                /*
                if(startingPosition<1)
                {
                    error("Slice: The specified term must be greater than 0.");
                }
                */

                storeToken = currentToken;
                if (matchTerminal(HighCTokenLibrary.ELLIPSES))
                {
                    if (HC_integer_expression(out intTerm2))
                    {
                        if(intTerm2 < intTerm1)
                        {
                            error("Slice (<int> ... <int>): The second term must be greater than or equal to the first.");
                            return false;
                        }
                        length = (int)(intTerm2 - intTerm1)+1;
                        return true;
                    }
                    else
                    {
                        error("Slice (<int> ... <int>): No second term found.");
                        return false;
                    }
                }
                else
                {
                    currentToken = storeToken;
                    return true;
                }
            }

            return false;
        }

        private Boolean HC_statement()
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_statement"); }
            int storeToken = currentToken;

            /*
            <block>
            <assignment>
            <type assignment>
            <list command>
            <input>
            <output>
            <control statement>
             */

            if (stopProgram == true ||
                returnFlag == true)
            {
                return false;
            }

            if (HC_assignment() == true) { Console.WriteLine(currentToken + " <statement> -> <assignment>"); return true; }

            /*
            currentToken = storeToken;
            if (HC_type_assignment() == true) { Console.WriteLine(currentToken + " <statement> -> <type assignment>"); return true; }
            */

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
                HighCData data;
                if (HC_function_call(out data, new HighCType(HighCType.STRING_TYPE)))
                {
                    if (data.isString())
                    {
                        stringBuffer = (String)data.data;
                        Console.WriteLine(currentToken + " <string term> -> <string function call>" + " -> " + stringBuffer);
                        derivationFound = true;
                    }
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
            int storeToken = currentToken;
            String identifier;
            value = "";
            if (HC_id(out identifier))
            {
                HighCData temp = currentEnvironment.getItem(identifier);

                if (temp == null)
                {
                    error("String Variable: The specified variable \"" + identifier + "\" could not be found." );
                    return false;
                }
                else if (temp.readable == false)
                {
                    error("String Variable: The specified variable \"" + identifier + "\" cannot be referenced." );
                    return false;
                }
                else if (temp.isString())
                {
                    if (temp.isList())
                    {
                        if (matchTerminal(HighCTokenLibrary.AT_SIGN))
                        {
                            int index;
                            int length;
                            if (HC_slice(out index, out length))
                            {
                                if (length != 1)
                                {
                                    error("String List: The length of the slice from the list must be 1.");
                                    return false;
                                }

                                if (index + length - 1 > temp.getCount())
                                {
                                    error("String List: The slice is not a subset of the list.");
                                    return false;
                                }

                                value = (String)((List<HighCData>)(temp.data))[index - 1].data;
                                Console.WriteLine(currentToken + " <string variable> -> <id>@<slice> -> " + identifier + "@" + index + " " + value);
                                return true;
                            }
                            else
                            {
                                error();
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //<array – variable> <subscript – expr>*
                    else if (temp.isArray())
                    {
                        storeToken = currentToken;
                        int intBuffer;
                        List<int> index = new List<int>();
                        while (HC_subscript_expression(out intBuffer))
                        {
                            storeToken = currentToken;
                            index.Add(intBuffer);
                        }
                        currentToken = storeToken;

                        if (((HighCArray)(temp.data)).indexInBounds(index))
                        {
                            value = (String)(((HighCArray)(temp.data)).getItemAt(index).data);
                            Console.WriteLine(currentToken + " <string variable> -> <id><subscript>* -> " + identifier + " " + value);
                            return true;
                        }
                        else
                        {
                            error("String Array: The specified array index goes outside the bounds of the array.");
                            return false;
                        }
                    }
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
                    if (matchTerminal(HighCTokenLibrary.RIGHT_SQUARE_BRACKET))
                    {
                        value = (int)term1;
                        Console.WriteLine(currentToken + " <subscript> -> [positive integer constant] ->" + term1);
                        return true;
                    }
                    else
                    {
                        //error();
                        return false;
                    }
                }
                else
                {
                    error(HighCTokenLibrary.ARRAY+": Expected a positive integer dimension specifier." );
                }
            }
            
            return false;
        }
        
        private Boolean HC_subscript_expression(out int value)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_subscript_expression"); }
            /*
            [ <int – expr> ]
             */
            value = 0;
            if (matchTerminal(HighCTokenLibrary.LEFT_SQUARE_BRACKET))
            {
                Int64 intBuffer;
                if(HC_integer_expression(out intBuffer))
                {
                    if(intBuffer<1)
                    {
                        error("Subscript: The value must be greater than 0.");
                        return false;
                    }

                    if(matchTerminal(HighCTokenLibrary.RIGHT_SQUARE_BRACKET,true))
                    {
                        value = (int)intBuffer;
                        return true;
                    }
                    else
                    {
                        error();
                        return false;
                    }
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

        private Boolean HC_type(out HighCType type)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_type"); }
            int storeToken = currentToken;
            type = null;

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
            HighCType declaredType = null;
            HighCType valueType = null;

            if(matchTerminal(HighCTokenLibrary.ENUMERATION))
            {
                String identifier;
                if(HC_id(out identifier))
                {
                    if(matchTerminal(HighCTokenLibrary.EQUAL,true) &&
                        matchTerminal(HighCTokenLibrary.LEFT_CURLY_BRACKET,true))
                    {
                        List<String> idList = new List<String>();
                        String stringBuffer;
                        if(HC_id(out stringBuffer))
                        {
                            idList.Add(stringBuffer);
                            storeToken = currentToken;
                            while(matchTerminal(HighCTokenLibrary.COMMA))
                            {
                                if(HC_id(out stringBuffer))
                                {
                                    if(idList.Contains(stringBuffer)==false)
                                    {
                                        idList.Add(stringBuffer);
                                        storeToken = currentToken;
                                    }
                                    else
                                    {
                                        error("Enum Declaration (" + identifier + "): The identifier \""+stringBuffer+"\" cannot be used more than once." + HighCTokenLibrary.ENUMERATION + "\"." );
                                        return false;
                                    }
                                }
                                else
                                {
                                    error("Enum Declaration (" + identifier + "): Expected another identifier to be assigned after comma." + HighCTokenLibrary.ENUMERATION + "\"." );
                                    return false;
                                }
                            }

                            currentToken = storeToken;
                            if(matchTerminal(HighCTokenLibrary.RIGHT_CURLY_BRACKET,true))
                            {
                                if(globalEnvironment.contains(identifier)==false)
                                {
                                    HighCEnumerationType enumeration = new HighCEnumerationType(identifier, idList);
                                    globalEnvironment.addNewItem(identifier, new HighCData(new HighCType(HighCType.VARIABLE_SUBTYPE,HighCType.ENUMERATION_TYPE, enumeration), enumeration, false, true));

                                    int i = 0;
                                    foreach (String enumConst in idList)
                                    {
                                        if (globalEnvironment.contains(enumConst) == false)
                                        {
                                            i++;
                                            globalEnvironment.addNewItem(enumConst, new HighCData(new HighCType(HighCType.VARIABLE_SUBTYPE, HighCType.ENUMERATION_INSTANCE, enumeration), new HighCEnumeration(enumConst, enumeration,i),false,true));
                                        }
                                        else
                                        {
                                            error("Enum Declaration (" + identifier + "): The enumeration constant \"" + enumConst + "\" is already being used and cannot be redeclared." );
                                            return false;
                                        }
                                    }

                                    return true;
                                }
                                else
                                {
                                    error("Enum Declaration (" + identifier + "): The enum identifier \""+identifier+"\" is already being used and cannot be redeclared." );
                                    return false;
                                }
                            }
                            else
                            {
                                error();
                                return false;
                            }
                        }
                        else
                        {
                            error("Enum Declaration ("+identifier+"): Expected at least one identifier to be assigned after the equal sign." + HighCTokenLibrary.ENUMERATION + "\"." );
                            return false;
                        }
                    }
                    else
                    {
                        error();
                        return false;
                    }
                }
                else
                {
                    error("Enum Declaration: Expected an identifier after \""+HighCTokenLibrary.ENUMERATION+"\"." );
                    return false;
                }
            }

            currentToken = storeToken;
            if (matchTerminal(HighCTokenLibrary.CONSTANT))
            {
                if (HC_type(out declaredType))
                {
                    storeToken = currentToken;
                    while (HC_initiated_variable(declaredType, out valueType, true))
                    {
                        storeToken = currentToken;
                        atLeastOneFound = true;
                        needAnother = false;

                        if (declaredType.isInteger() &&
                            valueType.isFloat())
                        {
                            addDebugInfo("Variable Declaration" + ": The type of the variable <" + valueType + "> does not match the type indicated <" + declaredType + ">." );
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
                        addDebugInfo("Constant Declaration" + ": another element was expected after the comma." );
                        return false;
                    }

                    if (atLeastOneFound == false)
                    {
                        addDebugInfo("Constant Declaration" + ": at least one declaration (\"<identifier> = <value>\") was expected after the type." );
                        return false;
                    }

                    Console.WriteLine(currentToken + " <declaration> -> const <type><initiated variable>,...,<initiated variable> -> " + declaredType);

                    return true;
                }
                else
                {
                    addDebugInfo(HighCTokenLibrary.CREATE + ": Expected a data or class type." );
                }
            }

            return false;
        }

        private Boolean HC_var(out String identifier, out List<int> subtype)
        {
            if (fullDebug == true) { Console.WriteLine("Attempting: " + "HC_var"); }
            int storeToken = currentToken;
            identifier = "";
            subtype = new List<int>();

            /*
                <id> <subscript>*   Array Subtype: >0
                <id> @              List Subtype: -1
                <id>                Variable Subtype: 0
             */

            if (HC_id(out identifier))
            {
                //LIST
                storeToken = currentToken;
                if (matchTerminal(HighCTokenLibrary.AT_SIGN))
                {
                    subtype.Add(-1);
                    Console.WriteLine(currentToken + " <var> -> <id> @ ->" + identifier + " " + subtype);
                    return true;
                }

                //ARRAY
                currentToken = storeToken;
                int dimensionSize = 0;
                if (HC_subscript(out dimensionSize))
                {
                    storeToken = currentToken;
                    subtype.Add(dimensionSize);
                    while (HC_subscript(out dimensionSize))
                    {
                        storeToken = currentToken;
                        subtype.Add(dimensionSize);
                    }

                    Console.WriteLine(currentToken + " <var> -> <id> <subscript>* ->" + identifier + " " + subtype);
                    currentToken = storeToken;
                    return true;
                }

                //VARIABLE
                currentToken = storeToken;
                subtype.Add(0);
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
            <list – variable> @ <slice> <- Performed where variable is called
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
            call <void function call>
             */
             
            if (matchTerminal(HighCTokenLibrary.CALL))
            {
                HighCData outBuffer;
                if (HC_function_call(out outBuffer, new HighCType(HighCType.VOID_TYPE)))
                {
                    if (outBuffer != null)
                    {
                        error("Call: Can only call functions with return type void.");
                        return false;
                    }
                    return true;
                }
                else
                {
                    error("Call: Could not find a void function with specified identifier.");
                    return false;
                }
            }

            return false;
        }

        private Boolean _______________Unimplemented_Functions_______________() { return false; }

        private Boolean HC_body() { return false; }
        private Boolean HC_class() { return false; }
        private Boolean HC_class_name() { return false; }
        private Boolean HC_compiler_directive() { return false; }
        private Boolean HC_data_field() { return false; }
        private Boolean HC_element() { return false; }
        private Boolean HC_field_assign() { return false; }
        private Boolean HC_field_constant() { return false; }
        private Boolean HC_initiated_field() { return false; }
        private Boolean HC_input() { /*Ensure Purity Check*/return false; }
        private Boolean HC_method() { return false; }
        private Boolean HC_object_constant() { return false; }
        private Boolean HC_object_expression(out HighCData value) { value = null; return false; }
        private Boolean HC_option() { return false; }
        private Boolean HC_parent() { return false; }
        private Boolean HC_prompt_variable() { return false; }
        private Boolean HC_qualifier() { return false; }
        private Boolean HC_return_subscript() { return false; }
        /*Generic Type Functions - Currently Unimplemented
        private Boolean HC_type_assignment() { return false; }
        private Boolean HC_type_group() { return false; }
        private Boolean HC_type_specifier() { return false; }
        //*/
    }

    class HighCEnvironment
    {
        HighCEnvironment parent;

        public List<String> identifiers = new List<String>();
        public List<HighCData> data = new List<HighCData>();

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
        }
        
        public Boolean changeItem(String identifier, HighCData newValue, out HighCData currentItem)
        {
            currentItem = getItem(identifier);

            if(currentItem!=null && newValue != null)
            {
                return currentItem.setData(newValue.type, newValue.data);
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

    public class HighCData
    {
        //Error Codes
        public const int ERROR_TYPE_MISMATCH = 1, ERROR_OUT_OF_RANGE = 2, ERROR_EMPTY_STRING = 3, ERROR_CONSTANT_CHANGED=4, ERROR_ARRAY_DIMENSION_MISMATCH=5;

        public Object data;
        public HighCType type;
        public Boolean writable;
        public Boolean readable;

        public int errorCode = -1;

        public HighCData(HighCType newType)
        {
            type = newType;
            writable = true;
            readable = true;
        }

        public HighCData(String newType, Object newValue, Boolean isWriteEnabled = true, Boolean isReadEnabled = true)
        {
            type = (new HighCType(newType));
            data = newValue;
            writable = isWriteEnabled;
            readable = isReadEnabled;
        }

        public HighCData(HighCType newType, Object newValue, Boolean isWriteEnabled=true, Boolean isReadEnabled=true)
        {
            type = newType;
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

            if(type.isList()==true)
            {
                String buffer = type + " {";
                foreach(HighCData item in ((List<HighCData>)data))
                {
                    buffer += item+", ";

                }
                return buffer.Substring(0,buffer.Length-2) + "}";
            }
            else
            {
                return type + " " + data.ToString();
            }
        }

        public Boolean isNumericType()
        {
            if(type.dataType == HighCTokenLibrary.INTEGER ||
                type.dataType == HighCTokenLibrary.FLOAT)
            {
                return true;
            }

            return false;
        }

        public Boolean isBoolean()
        {
            if (type.dataType == HighCType.BOOLEAN_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isInteger()
        {
            if (type.dataType == HighCType.INTEGER_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isFloat()
        {
            if (type.dataType == HighCType.FLOAT_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isCharacter()
        {
            if (type.dataType == HighCType.CHARACTER_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isString()
        {
            if (type.dataType == HighCType.STRING_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isEnumeration()
        {
            if (type.dataType == HighCType.ENUMERATION_INSTANCE)
            {
                return true;
            }
            return false;
        }

        public Boolean isEnumerationType()
        {
            if (type.dataType == HighCType.ENUMERATION_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isClass()
        {
            if (type.dataType == HighCType.CLASS_INSTANCE)
            {
                return true;
            }
            return false;
        }

        public Boolean isClassType()
        {
            if (type.dataType == HighCType.CLASS_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isFunction()
        {
            if (type.dataType == HighCType.FUNCTION_DECLARATION_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isVariable()
        {
            if (type.memoryType == HighCType.VARIABLE_SUBTYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isArray()
        {
            if (type.memoryType == HighCType.ARRAY_SUBTYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isList()
        {
            if (type.memoryType == HighCType.LIST_SUBTYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean setValue(Int64 newValue)
        {
            if (type.dataType == HighCType.INTEGER_TYPE)
            {
                if (newValue >= (Int64)type.minimum &&
                    newValue <= (Int64)type.maximum)
                {
                    data = newValue;
                    return true;
                }
                else
                {
                    errorCode = ERROR_OUT_OF_RANGE;
                    return false;
                }
            }
            else if (type.dataType == HighCType.FLOAT_TYPE)
            {
                if (newValue >= (Double)type.minimum &&
                    newValue <= (Double)type.maximum)
                {
                    data = (Double)newValue;
                    return true;
                }
                else
                {
                    errorCode = ERROR_OUT_OF_RANGE;
                    return false;
                }
            }

            errorCode = ERROR_TYPE_MISMATCH;
            return false;
        }

        public Boolean setValue(Double newValue)
        {
            if (type.dataType == HighCType.INTEGER_TYPE)
            {
                if (newValue >= (Int64)type.minimum &&
                    newValue <= (Int64)type.maximum)
                {
                    data = (Int64)Math.Round(newValue);
                    return true;
                }
                else
                {
                    errorCode = ERROR_OUT_OF_RANGE;
                    return false;
                }
            }
            else if (type.dataType == HighCType.FLOAT_TYPE)
            {
                if (newValue >= (Double)type.minimum &&
                    newValue <= (Double)type.maximum)
                {
                    data = newValue;
                    return true;
                }
                else
                {
                    errorCode = ERROR_OUT_OF_RANGE;
                    return false;
                }
            }

            errorCode = ERROR_TYPE_MISMATCH;
            return false;
        }

        public Boolean setValue(String newValue)
        {
            if (type.dataType == HighCType.STRING_TYPE)
            {
                data = newValue;
                return true;
            }
            else if (type.dataType == HighCType.CHARACTER_TYPE)
            {
                if (newValue.Length >= 1)
                {
                    if (newValue[0] >= ((String)type.minimum)[0] &&
                        newValue[0] <= ((String)type.maximum)[0])
                    {
                        data = "" + newValue[0];
                        return true;
                    }
                    else
                    {
                        errorCode = ERROR_OUT_OF_RANGE;
                        return false;
                    }
                }
                else
                {
                    errorCode = ERROR_EMPTY_STRING;
                    return false;
                }
            }

            errorCode = ERROR_TYPE_MISMATCH;
            return false;
        }

        public Boolean setValue(Char newValue)
        {
            if (type.dataType == HighCType.STRING_TYPE)
            {
                data = "" + newValue;
                return true;
            }
            else if (type.dataType == HighCType.CHARACTER_TYPE)
            {
                if (newValue >= ((String)type.minimum)[0] &&
                    newValue <= ((String)type.maximum)[0])
                {
                    data = "" + newValue;
                    return true;
                }
                else
                {
                    errorCode = ERROR_OUT_OF_RANGE;
                    return false;
                }
            }
            errorCode = ERROR_TYPE_MISMATCH;
            return false;
        }

        public Boolean setValue(Boolean newValue)
        {
            if (type.dataType == HighCType.BOOLEAN_TYPE)
            {
                data = newValue;
                return true;
            }

            errorCode = ERROR_TYPE_MISMATCH;
            return false;
        }
        
        public Boolean setValue(HighCEnumeration newValue)
        {
            if (type.isEnumeration())
            {
                if (newValue.type == type.objectReference)
                {
                    if (type.minimum != null && type.maximum != null)
                    {
                        if (newValue.rank >= ((HighCEnumeration)type.minimum).rank &&
                            newValue.rank <= ((HighCEnumeration)type.maximum).rank)
                        {
                            data = newValue;
                            return true;
                        }
                        else
                        {
                            errorCode = ERROR_OUT_OF_RANGE;
                            return false;
                        }
                    }
                    else
                    {
                        data = newValue;
                        return true;
                    }
                }
            }

            errorCode = ERROR_TYPE_MISMATCH;
            return false;
        }

        public Boolean setData(HighCType valueType, Object newValue, Boolean overrideConstant=false)
        {
            if (writable == true || overrideConstant==true)
            {
                if (type.isVariable()==true)
                {
                    if(valueType.isVariable()==false)
                    {
                        errorCode = ERROR_TYPE_MISMATCH;
                        return false;
                    }

                    switch (valueType.dataType)
                    {
                        case HighCType.INTEGER_TYPE: return setValue((Int64)newValue);
                        case HighCType.FLOAT_TYPE: return setValue((Double)newValue);
                        case HighCType.BOOLEAN_TYPE: return setValue((Boolean)newValue);
                        case HighCType.STRING_TYPE: return setValue((String)newValue);
                        case HighCType.CHARACTER_TYPE: return setValue((String)newValue);
                        case HighCType.ENUMERATION_INSTANCE: return setValue((HighCEnumeration)newValue);
                        default:
                            break;
                    }
                }
                else if(type.isList()==true)
                {
                    if (valueType.isList()==true)
                    {
                        List<HighCData> newList = new List<HighCData>();

                        foreach(HighCData item in ((List<HighCData>)newValue))
                        {
                            HighCData newItem = new HighCData(new HighCType(HighCType.VARIABLE_SUBTYPE,
                                                                            type.dataType,
                                                                            type.objectReference),
                                                              null);
                            newItem.type.minimum = type.minimum;
                            newItem.type.maximum = type.maximum;
                            if(newItem.setData(item)==false)
                            {
                                errorCode = ERROR_TYPE_MISMATCH;
                                return false;
                            }
                            newList.Add(newItem);
                        }

                        data = newList;
                        return true;
                    }
                    else
                    {
                        errorCode = ERROR_TYPE_MISMATCH;
                    }
                }
                else if(type.isArray()==true)
                {
                    if(valueType.isArray()==true)
                    {
                        if(((HighCArray)newValue).dimensions.Count !=
                           ((HighCArray)data).dimensions.Count)
                        {
                            errorCode = ERROR_ARRAY_DIMENSION_MISMATCH;
                            return false;
                        }

                        int i = 0;
                        while(i<((HighCArray)newValue).dimensions.Count)
                        {
                            if(((HighCArray)newValue).dimensions[i]!= 
                                ((HighCArray)data).dimensions[i])
                            {
                                errorCode = ERROR_ARRAY_DIMENSION_MISMATCH;
                                return false;
                            }
                            i++;
                        }

                        i = 0;
                        while(i < ((HighCArray)newValue).array.Length)
                        {
                            if(((HighCArray)data).array[i].setData(((HighCArray)newValue).array[i]) == false)
                            {
                                errorCode = ((HighCArray)data).array[i].errorCode;
                                return false;
                            }
                            i++;
                        }

                        return true;
                    }
                    else
                    {
                        errorCode = ERROR_TYPE_MISMATCH;
                    }
                }
                else
                {
                    Console.WriteLine("Should not see this: "+type);
                }
            }
            else
            {
                errorCode = ERROR_CONSTANT_CHANGED;
            }

            return false;
        }

        public Boolean setData(HighCData newValue, Boolean overrideConstant = false)
        {
            return setData(newValue.type, newValue.data, overrideConstant);
        }

        public Boolean compare(HighCData value2, String opType, out Boolean value)
        {
            Double term1 = 0.0;
            Double term2 = 0.0;
            String stringTerm1 = "";
            String stringTerm2 = "";
            value = false;

            if (isInteger())
            {
                term1 = (Double)((Int64)data);
                if (value2.isFloat())
                {
                    term2 = (Double)value2.data;
                }
                else if (value2.isInteger())
                {
                    term2 = (Double)((Int64)value2.data);
                }
                else
                {
                    errorCode = ERROR_TYPE_MISMATCH;
                    return false;
                }

                if (opType == HighCTokenLibrary.EQUAL) { value = term1 == term2; }
                else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = term1 != term2; }
                else if (opType == HighCTokenLibrary.LESS_THAN) { value = term1 < term2; }
                else if (opType == HighCTokenLibrary.GREATER_THAN) { value = term1 > term2; }
                else if (opType == HighCTokenLibrary.LESS_THAN_EQUAL) { value = term1 <= term2; }
                else if (opType == HighCTokenLibrary.GREATER_THAN_EQUAL) { value = term1 >= term2; }
                return true;
            }

            if (isFloat())
            {
                term1 = (Double)data;
                if (value2.isFloat())
                {
                    term2 = (Double)value2.data;
                }
                else if (value2.isInteger())
                {
                    term2 = (Double)((Int64)value2.data);
                }

                if (opType == HighCTokenLibrary.EQUAL) { value = term1 == term2; }
                else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = term1 != term2; }
                else if (opType == HighCTokenLibrary.LESS_THAN) { value = term1 < term2; }
                else if (opType == HighCTokenLibrary.GREATER_THAN) { value = term1 > term2; }
                else if (opType == HighCTokenLibrary.LESS_THAN_EQUAL) { value = term1 <= term2; }
                else if (opType == HighCTokenLibrary.GREATER_THAN_EQUAL) { value = term1 >= term2; }

                return true;
            }


            if (isString() || isCharacter())
            {
                stringTerm1 = (String)data;
                if (value2.isString() ||
                    value2.isCharacter())
                {
                    stringTerm2 = (String)value2.data;
                }
                else
                {
                    errorCode = ERROR_TYPE_MISMATCH;
                    return false;
                }
                
                int order = String.Compare(stringTerm1, stringTerm2);

                if (opType == HighCTokenLibrary.EQUAL) { value = order == 0; }
                else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = order != 0; }
                else if (opType == HighCTokenLibrary.LESS_THAN) { value = order < 0; }
                else if (opType == HighCTokenLibrary.GREATER_THAN) { value = order > 0; }
                else if (opType == HighCTokenLibrary.LESS_THAN_EQUAL) { value = order <= 0; }
                else if (opType == HighCTokenLibrary.GREATER_THAN_EQUAL) { value = order >= 0; }
                    
                return true;
            }
           
            if (isEnumeration())
            {
                HighCEnumeration enumTerm1 = (HighCEnumeration)data;
                HighCEnumeration enumTerm2;
                if (value2.isEnumeration() &&
                    type.objectReference==value2.type.objectReference)
                {
                    enumTerm2 = (HighCEnumeration)value2.data;
                }
                else
                {
                    errorCode = ERROR_TYPE_MISMATCH;
                    return false;
                }
                
                if (opType == HighCTokenLibrary.EQUAL) { value = enumTerm1.rank == enumTerm2.rank; }
                else if (opType == HighCTokenLibrary.NOT_EQUAL) { value = enumTerm1.rank != enumTerm2.rank; }
                else if (opType == HighCTokenLibrary.LESS_THAN) { value = enumTerm1.rank < enumTerm2.rank; }
                else if (opType == HighCTokenLibrary.GREATER_THAN) { value = enumTerm1.rank > enumTerm2.rank; }
                else if (opType == HighCTokenLibrary.LESS_THAN_EQUAL) { value = enumTerm1.rank <= enumTerm2.rank; }
                else if (opType == HighCTokenLibrary.GREATER_THAN_EQUAL) { value = enumTerm1.rank >= enumTerm2.rank; }
                
                return true;
            }

            value = false;
            return false;
        }

        public int getCount()
        {
            if(isList())
            {
                return ((List<HighCData>)(data)).Count;
            }
            else
            {
                return 1;
            }
        }

        public HighCData getVariableOfType()
        {
            HighCData value=null;

            if(isVariable())
            {
                value = new HighCData(type, null, writable, readable);
            }
            else if(isList() || isArray())
            {
                value = new HighCData(new HighCType(HighCType.VARIABLE_SUBTYPE, type.dataType, type.objectReference), null, writable, readable);
                value.type.minimum = type.minimum;
                value.type.maximum = type.maximum;
            }

            return value;
        }

        public HighCData Clone()
        {
            Object newValue=null;

            if (data == null) { }
            else
            {
                if (type.memoryType == HighCType.VARIABLE_SUBTYPE)
                {
                    switch(type.dataType)
                    {
                        case HighCType.INTEGER_TYPE:
                            Int64 temp = ((Int64)data);
                            newValue = temp;
                            break;
                        case HighCType.FLOAT_TYPE:
                            Double temp2 = ((Double)data);
                            newValue = temp2;
                            break;
                        case HighCType.BOOLEAN_TYPE:
                            Boolean temp3 = ((Boolean)data);
                            newValue = temp3;
                            break;
                        case HighCType.CHARACTER_TYPE:
                            String temp4 = ((String)data);
                            newValue = temp4;
                            break;
                        case HighCType.STRING_TYPE:
                            newValue = ((String)data).Substring(0);
                            break;
                        case HighCType.FUNCTION_DECLARATION_TYPE:
                            newValue = ((HighCFunctionDeclaration)data).Clone();
                            break;
                        default:
                            newValue = null;
                            break;
                    }
                }
                else if (type.memoryType == HighCType.ARRAY_SUBTYPE)
                {

                }
                else if (type.memoryType == HighCType.VARIABLE_SUBTYPE)
                {

                }
            }

            HighCData cloneData = new HighCData(type,newValue,writable,readable);
            return cloneData;
        }
    }

    public class HighCFunctionDeclaration
    {
        public Boolean isPure;
        public Boolean isRecursive;
        public String identifier;
        public List<HighCParameter> parameters;
        public HighCType returnType;
        public int blockTokenPosition;

        public HighCFunctionDeclaration(String name, Boolean purity, Boolean recursiveness, List<HighCParameter> newParameters, HighCType returnValue, int startPosition)
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

    public class HighCParameter
    {
        public String identifier;
        public Boolean inAllowed;
        public Boolean outAllowed;
        public HighCType type;
        List<String> subscriptParameters;

        public HighCParameter(String name, HighCType newType, Boolean inStatus, Boolean outStatus, List<String> newSubscriptParameters)
        {
            identifier = name;
            type = newType;
            inAllowed = inStatus;
            outAllowed = outStatus;
            subscriptParameters = newSubscriptParameters;
        }
    }

    public class HighCEnumerationType
    {
        public String identifier;
        public HighCEnumeration[] enumList;

        public HighCEnumerationType(String name, List<String> newEnumList)
        {
            identifier = name;
            enumList = new HighCEnumeration[newEnumList.Count];
            int i = 0;
            foreach (String enumConst in newEnumList)
            {
                enumList[i] = new HighCEnumeration(enumConst, this, i + 1);
                i++;
            }
        }

        public HighCData getIndex(String id)
        {
            Int64 i = 1;
            foreach (HighCEnumeration enumEntry in enumList)
            {
                if (enumEntry.identifier == id)
                {
                    return new HighCData(new HighCType(HighCType.INTEGER_TYPE), i);
                }
                i++;
            }
            return null;
        }

        public HighCEnumeration getNext(String id)
        {
            int i = 0;
            while (i < enumList.Length)
            {
                if (enumList[i].identifier == id)
                {
                    if (i + 1 < enumList.Length)
                    {
                        return enumList[i + 1];
                    }
                    else
                    {
                        return null;
                    }
                }
                i++;
            }
            return null;
        }

        public HighCEnumeration getPrevious(String id)
        {
            int i = 0;
            while (i < enumList.Length)
            {
                if (enumList[i].identifier == id)
                {
                    if (i - 1 >= 0)
                    {
                        return enumList[i - 1];
                    }
                    else
                    {
                        return null;
                    }
                }
                i++;
            }
            return null;
        }

        public HighCEnumeration getMinimum()
        {
            return enumList[0];
        }

        public HighCEnumeration getMaximum()
        {
            return enumList[enumList.Length - 1];
        }

        public override String ToString()
        {
            return identifier;
        }
    }

    public class HighCEnumeration
    {
        public String identifier;
        public HighCEnumerationType type;
        public int rank;

        public HighCEnumeration(String name, HighCEnumerationType enumerationType, int newRank)
        {
            identifier = name;
            type = enumerationType;
            rank = newRank;
        }

        public override String ToString()
        {
            return identifier;
        }
    }

    public class HighCArray
    {
        public HighCData[] array;
        public List<int> dimensions;
        public Boolean isSeedValue;

        public HighCArray(HighCData[] newArray, List<int> newSizes, Boolean isSeed=false)
        {
            array = newArray;
            dimensions = newSizes;
            isSeedValue = isSeed;
        }

        public HighCData getItemAt(List<int> index)
        {
            int offset = 0;
            int dimensionSizeBuffer = 1;
            int i = index.Count - 1;
            while(i>-1)
            {
                offset = offset+index[i] * dimensionSizeBuffer;
                dimensionSizeBuffer = dimensionSizeBuffer * dimensions[i];
                i--;
            }

            if (offset < array.Length &&
                offset >= 0)
            {
                return array[offset];
            }
            else
            {
                return null;
            }
        }

        public Boolean indexInBounds(List<int> index)
        {
            if(dimensions.Count!=index.Count)
            {
                return false;
            }

            int i = 0;
            while(i<index.Count)
            {
                if(index[i]<1 || index[i]>dimensions[i])
                {
                    return false;
                }
                i++;
            }
            return true;
        }
    }

    public class HighCType
    {
        //TYPES
        public const String INTEGER_TYPE = HighCTokenLibrary.INTEGER;
        public const String FLOAT_TYPE = HighCTokenLibrary.FLOAT;
        public const String BOOLEAN_TYPE = HighCTokenLibrary.BOOLEAN;
        public const String CHARACTER_TYPE = HighCTokenLibrary.CHARACTER;
        public const String STRING_TYPE = HighCTokenLibrary.STRING;
        public const String FUNCTION_DECLARATION_TYPE = HighCTokenLibrary.FUNCTION;
        public const String ENUMERATION_TYPE = HighCTokenLibrary.ENUMERATION;
        public const String ENUMERATION_INSTANCE = HighCTokenLibrary.ENUMERATION + " " + HighCTokenLibrary.VARIABLE;
        public const String CLASS_TYPE = HighCTokenLibrary.CLASS;
        public const String CLASS_INSTANCE = HighCTokenLibrary.CLASS + " " + HighCTokenLibrary.VARIABLE;
        public const String VOID_TYPE = HighCTokenLibrary.VOID;

        //MEMORY TYPE
        public const String VARIABLE_SUBTYPE = HighCTokenLibrary.VARIABLE;
        public const String ARRAY_SUBTYPE = HighCTokenLibrary.ARRAY;
        public const String LIST_SUBTYPE = HighCTokenLibrary.LIST;

        public String dataType;
        public Object objectReference=null;
        public String memoryType;
        public Object minimum = null;
        public Object maximum = null;
        public Boolean minMaxSet = false;
        public int precision = 0;

        public HighCType(String singleVariableType)
        {
            dataType = singleVariableType;
            memoryType = VARIABLE_SUBTYPE;
            setRange();
        }

        public HighCType(String newMemoryType, String newDataType, Object newObjectReference=null, HighCData newMin = null, HighCData newMax = null)
        {
            dataType = newDataType;
            memoryType = newMemoryType;
            objectReference = newObjectReference;
            setRange(newMin, newMax);
        }
        
        private void setRange(HighCData newMin = null, HighCData newMax = null)
        {
            if (newMin == null)
            {
                switch (dataType)
                {
                    case INTEGER_TYPE: minimum = Int64.MinValue; break;
                    case FLOAT_TYPE: minimum = Double.MinValue; break;
                    case CHARACTER_TYPE: minimum = ""+Char.MinValue; break;
                    case ENUMERATION_INSTANCE:
                        if (objectReference != null)
                        {
                            minimum = ((HighCEnumerationType)objectReference).getMinimum(); 
                        }
                        break;
                    default: break;
                }
            }
            else
            {
                minMaxSet = true;
                if (dataType == FLOAT_TYPE)
                {
                    minimum = Double.MinValue;
                    precision = (int)((Int64)newMin.data);
                }
                else
                {
                    minimum = newMin.data;
                }
            }

            if (newMax == null)
            {
                switch (dataType)
                {
                    case INTEGER_TYPE: maximum = Int64.MaxValue; break;
                    case FLOAT_TYPE: maximum = Double.MaxValue; break;
                    case CHARACTER_TYPE: maximum = "" + Char.MaxValue; break;
                    case ENUMERATION_INSTANCE:
                        if (objectReference != null)
                        {
                            maximum = ((HighCEnumerationType)objectReference).getMaximum();
                        }
                        break;
                    default: break;
                }
            }
            else
            {
                minMaxSet = true;
                maximum = newMax.data;
            }
        }
        
        public override String ToString()
        {
            String stringBuffer="";
            
            stringBuffer += dataType;

            if (memoryType == LIST_SUBTYPE)
            {
                stringBuffer += "@";
            }

            if (objectReference!=null)
            {
                stringBuffer += " "+objectReference.ToString();
            }

            if(memoryType==ARRAY_SUBTYPE)
            {
                stringBuffer += "[]";
            }

            if (minMaxSet == true)
            {
                if (dataType == FLOAT_TYPE && precision != 0)
                {
                    stringBuffer += " Precision(" + precision + ")";
                }

                if (minimum != null)
                {
                    if (dataType == CHARACTER_TYPE &&
                       (String)minimum == "" + char.MinValue)
                    {
                        stringBuffer += ": NULL";
                    }
                    else
                    {
                        stringBuffer += ": " + minimum.ToString();
                    }
                }

                if (maximum != null)
                {
                    stringBuffer += "..." + maximum.ToString();
                }
            }
            return stringBuffer;
        }

        public Boolean isEqualToType(HighCType type)
        {
            if(memoryType==type.memoryType &&
                dataType==type.dataType &&
                objectReference==type.objectReference)
            {
                return true;
            }

            return false;
        }

        public Boolean isNumericType()
        {
            if (dataType == HighCTokenLibrary.INTEGER ||
                dataType == HighCTokenLibrary.FLOAT)
            {
                return true;
            }

            return false;
        }

        public Boolean isBoolean()
        {
            if (dataType == HighCType.BOOLEAN_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isInteger()
        {
            if (dataType == HighCType.INTEGER_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isFloat()
        {
            if (dataType == HighCType.FLOAT_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isCharacter()
        {
            if (dataType == HighCType.CHARACTER_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isString()
        {
            if (dataType == HighCType.STRING_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isEnumeration()
        {
            if (dataType == HighCType.ENUMERATION_INSTANCE)
            {
                return true;
            }
            return false;
        }

        public Boolean isEnumerationType()
        {
            if (dataType == HighCType.ENUMERATION_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isClass()
        {
            if (dataType == HighCType.CLASS_INSTANCE)
            {
                return true;
            }
            return false;
        }

        public Boolean isClassType()
        {
            if (dataType == HighCType.CLASS_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isFunction()
        {
            if (dataType == HighCType.FUNCTION_DECLARATION_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isVoid()
        {
            if (dataType == HighCType.VOID_TYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isVariable()
        {
            if (memoryType == HighCType.VARIABLE_SUBTYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isArray()
        {
            if (memoryType == HighCType.ARRAY_SUBTYPE)
            {
                return true;
            }
            return false;
        }

        public Boolean isList()
        {
            if (memoryType == HighCType.LIST_SUBTYPE)
            {
                return true;
            }
            return false;
        }
    }
}
