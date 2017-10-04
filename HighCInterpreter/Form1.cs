using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HighCInterpreterCore;

/*
 * To Do:
 * Rounding
 * Non-Recursive Tracking
 * Function Declaration
 */

namespace HighCInterpreterCore
{
    public partial class Form1 : Form
    {
        HighCTokenizer tokenizer;
        enum weekdays { Monday, Tuesday };

        public Form1()
        {
            InitializeComponent();
            
            //Default
            textbox_InputBox.Text=
                "//Compiler Directives"+ Environment.NewLine +
                "//User Constants" + Environment.NewLine +
                "//Global Variables" + Environment.NewLine +
                "//Classes" + Environment.NewLine +
                "//Functions" + Environment.NewLine +
                "//Main" +Environment.NewLine +
                Environment.NewLine +
                "main" + Environment.NewLine +
                "{" + Environment.NewLine +
                "" + Environment.NewLine +
                "}";

            textbox_InputBox.Text =
                "//Compiler Directives" + Environment.NewLine +
                "//User Constants" + Environment.NewLine + 
                "enum weekdays = {Sunday,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday}" + Environment.NewLine +
                "//Global Variables" + Environment.NewLine +
                "//Classes" + Environment.NewLine +
                "class helloWorldClass" + Environment.NewLine +
                "private{}" + Environment.NewLine +
                "public{ create STRING text }" + Environment.NewLine +
                "   " + Environment.NewLine +
                "final class testClass" + Environment.NewLine +
                "private" + Environment.NewLine +
                "{" + Environment.NewLine +
                "   //Data Fields" + Environment.NewLine +
                "   " + Environment.NewLine +
                "   //Methods" + Environment.NewLine +
                "   last pure recurs method calculateAreas(INT val1, INT val2) => INT { return 3 }" + Environment.NewLine +
                "}" + Environment.NewLine +
                "public" + Environment.NewLine +
                "{" + Environment.NewLine +
                "   //Data Fields" + Environment.NewLine +
                "   create BOOL boolTest" + Environment.NewLine +
                "   create BOOL boolTest2@" + Environment.NewLine +
                "   create BOOL boolTest3[2][3]" + Environment.NewLine +
                "   create weekdays enumTest" + Environment.NewLine +
                "   create weekdays enumTest2@" + Environment.NewLine +
                "   create weekdays enumTest3[3]" + Environment.NewLine +
                "   create INT intTest" + Environment.NewLine +
                "   create INT intTest2@" + Environment.NewLine +
                "   create INT intTest3[2][3]" + Environment.NewLine +
                "   create CHAR charTest" + Environment.NewLine +
                "   create CHAR charTest2@" + Environment.NewLine +
                "   create CHAR charTest3[2][3]" + Environment.NewLine +
                "   create STRING stringTest" + Environment.NewLine +
                "   create STRING stringTest2@" + Environment.NewLine +
                "   create STRING stringTest3[2][3]" + Environment.NewLine +
                "   create FLOAT floatTest" + Environment.NewLine +
                "   create FLOAT floatTest2@" + Environment.NewLine +
                "   create FLOAT floatTest3[2][3]" + Environment.NewLine +
                "   create helloWorldClass classTest" + Environment.NewLine +
                "   //Methods" + Environment.NewLine +
                "   method getArea() => INT { return areas[3] }" + Environment.NewLine +
                "}" + Environment.NewLine +
                "//Functions" + Environment.NewLine +
                "//Main" + Environment.NewLine +
                Environment.NewLine +
                "main" + Environment.NewLine +
                "{" + Environment.NewLine +
                "   create testClass test ={" + Environment.NewLine +
                "    boolTest=false, boolTest2={false,true,false}, boolTest3=Array(true)," + Environment.NewLine +
                "    intTest=-1, intTest2={0,5,10}, intTest3={{1,2,3},{4,5,6}}," + Environment.NewLine +
                "    charTest=\"a\", charTest2={\"b\",\"c\",\"d\"}, charTest3=Array(\"e\")," + Environment.NewLine +
                "    stringTest=\"alpha\", stringTest2={\"bravo\",\"charlie\",\"delta\"}, stringTest3=Array(\"echo\")," + Environment.NewLine +
                "    enumTest=Sunday, enumTest2={Monday, Tuesday, Wednesday}, enumTest3={Thursday, Friday, Saturday}," + Environment.NewLine +
                "    classTest = { text = \"Hello World!\"}," + Environment.NewLine +
                "    floatTest=-1.5, floatTest2={0.1,3.14,10.7e3}, floatTest3={{1.1,2.2,3.3},{4.4,5.5,6.6}}" + Environment.NewLine +
                "                          }" + Environment.NewLine +
                "   out \"Bool: \", \"Bool: \", test.boolTest, endl" + Environment.NewLine +
                "   out \"Int: \", test.intTest, endl" + Environment.NewLine +
                "   out \"Char: \", test.charTest, endl" + Environment.NewLine +
                "   out \"String: \", test.stringTest, endl" + Environment.NewLine +
                "   out \"Enum: \", test.enumTest, endl" + Environment.NewLine +
                "   out \"Float: \", test.floatTest, endl" + Environment.NewLine +
                "   out \"Class: \", test.classTest.text, endl" + Environment.NewLine +
                "   out \"List: \", test.enumTest2@2, endl" + Environment.NewLine +
                "   out \"Array: \", test.floatTest3[2][1], endl" + Environment.NewLine +
                "   out \"Expression: \", (test.intTest2@2)+test.floatTest3[1][3], endl" + Environment.NewLine +
                "   " + Environment.NewLine +
                "   " + Environment.NewLine +
                "}";
            //*/

            /*textbox_InputBox.Text =
                "//Compiler Directives" + Environment.NewLine +
                "//User Constants" + Environment.NewLine +
                "//Global Variables" + Environment.NewLine +
                "//Classes" + Environment.NewLine +
                "//Functions" + Environment.NewLine +
                "//Main" + Environment.NewLine +
                Environment.NewLine +
                "main" + Environment.NewLine +
                "{" + Environment.NewLine +
                "   for(INT val in 1...5)" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "      out val, endl" + Environment.NewLine +
                "   }" + Environment.NewLine +
                "}";
            //*/
            /*
            textbox_InputBox.Text =
                "//Compiler Directives" + Environment.NewLine +
                "//User Constants" + Environment.NewLine +
                "//Global Variables" + Environment.NewLine +
                "//Classes" + Environment.NewLine +
                "//Functions" + Environment.NewLine +
                "//Main" + Environment.NewLine +
                Environment.NewLine +
                "main" + Environment.NewLine +
                "{" + Environment.NewLine +
                "   create INT nums[3][2]={{1,2},{3,4},{5,6}}" + Environment.NewLine +
                "   for[](INT val in nums)" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "      out val, endl" + Environment.NewLine +
                "   }" + Environment.NewLine +
                "}";
            //*/

            /*
             textbox_InputBox.Text=
                "//Compiler Directives"+ Environment.NewLine +
                "//User Constants" + Environment.NewLine +
                "//Global Variables" + Environment.NewLine +
                "//Classes" + Environment.NewLine +
                "//Functions" + Environment.NewLine +
                "//Main" +Environment.NewLine +
                Environment.NewLine +
                "main" + Environment.NewLine +
                "{" + Environment.NewLine +
                "   create INT limit = 93" + Environment.NewLine +
                "   create INT current = 3" + Environment.NewLine +
                "   create INT fibonnaci@ = { 1,1}" + Environment.NewLine + Environment.NewLine +
                "   loop" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "      append fibonnaci = (fibonnaci@(current - 1))+(fibonnaci@(current - 2))" + Environment.NewLine +
                "      until(current = limit)" + Environment.NewLine +
                "      set current = current + 1" + Environment.NewLine +
                "   }" + Environment.NewLine + Environment.NewLine +
                "   set current = 1" + Environment.NewLine +
                "   for@(INT value in fibonnaci)" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "      out current: 3, \": \", value, endl" + Environment.NewLine +
                "      set current = current + 1" + Environment.NewLine +
                "   }" + Environment.NewLine +
            "}";
             //*/

            /*Range Specification Testing
             textbox_InputBox.Text =
                "enum weekdays = {Sunday,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday}" + Environment.NewLine +
                "main" + Environment.NewLine +
                "{" + Environment.NewLine +
                "     create weekdays:Monday ... Friday value = Saturday" + Environment.NewLine +
                "     out value" + Environment.NewLine +
                "}";
             //*/

            /*Enum Testing
            textbox_InputBox.Text =
                "enum weekdays = {Sunday,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday}" + Environment.NewLine +
                "enum colors = {Red,Green,Blue}" + Environment.NewLine +
                "func test () => weekdays { return Red }" + Environment.NewLine +
                "main" + Environment.NewLine +
                "{" + Environment.NewLine +
                "     create weekdays value = Monday" + Environment.NewLine +
                "     set value = test()" + Environment.NewLine +
                "     out value" + Environment.NewLine +
                "}";
            //*/

            /*
            textbox_InputBox.Text =
                "func test ( ) => FLOAT" + Environment.NewLine +
                "{" + Environment.NewLine +
                "     out \"Testing...\",endl" + Environment.NewLine +
                "     return 3.14" + Environment.NewLine +
                "}" + Environment.NewLine +
                Environment.NewLine +
                Environment.NewLine +
                "main" + Environment.NewLine +
                "{" + Environment.NewLine +
                "     out test()" + Environment.NewLine +
                "}";
            //*/

            /*
            textbox_InputBox.Text =
                "func test ( in STRING output, in STRING output2 ) => void" + Environment.NewLine +
                "{" + Environment.NewLine +
                "     out output,\" \", output2, endl" + Environment.NewLine +
                "}" + Environment.NewLine +
                Environment.NewLine +
                 "func test2 ( out STRING getString ) => void" + Environment.NewLine +
                "{" + Environment.NewLine +
                "     set getString = \"Test\"" + Environment.NewLine +
                "}" + Environment.NewLine +
                Environment.NewLine +
                "main" + Environment.NewLine +
                "{" + Environment.NewLine +
                "     create STRING stringBuffer = \"Initialized\"" + Environment.NewLine +
                "     call test ( \"Hello\", \"World\" )" +Environment.NewLine +
                "     call test2 ( stringBuffer )" + Environment.NewLine +
                "     out stringBuffer" + Environment.NewLine +
                "}";
            //*/

            /*
            textbox_InputBox.Text =
                "//User Constants" + Environment.NewLine +
                "const BOOL constBool = false" + Environment.NewLine +
                "const INT constInt = 3" + Environment.NewLine +
                "const FLOAT constFloat = 4.56" + Environment.NewLine +
                "const CHAR constChar = \"q\"" + Environment.NewLine +
                "const STRING constString = \"Constant String\"" + Environment.NewLine+ 
                Environment.NewLine +
                "//Global Variables" + Environment.NewLine +
                "global create BOOL varBool = true" + Environment.NewLine +
                "global create INT varInt = 4" + Environment.NewLine +
                "global create FLOAT varFloat = 1.23e1" + Environment.NewLine +
                "global create CHAR varChar = \"g\"" + Environment.NewLine +
                "global create STRING varString = \"Hello World\"" + Environment.NewLine +
                Environment.NewLine +
                "//Function Declarations" + Environment.NewLine +
                "func testFunction ( ) => void { }" + Environment.NewLine +
                "func testFunction2 ( ) => INT { out 2 }" + Environment.NewLine +
                "func testFunction3 ( in INT p1 ) => STRING { out 3 }" + Environment.NewLine +
                "func testFunction4 ( in INT p1, out BOOL p2, inout STRING p3, in out CHAR p4 ) => BOOL { out 3 }" + Environment.NewLine +
                "pure func testFunction5 ( ) => void { out 3 }" + Environment.NewLine +
                "recurs func testFunction6 ( ) => void { out 3 }" + Environment.NewLine +
                "pure recurs func testFunction7 ( ) => void { out 3 }" + Environment.NewLine +
                "recurs pure func testFunction8 ( ) => void { out 3 }" + Environment.NewLine +
                
                Environment.NewLine +
                "main" +Environment.NewLine+
                "{"+Environment.NewLine +
                "   create BOOL varBool2 = false" + Environment.NewLine +
                "   create INT varInt2 = 2" + Environment.NewLine +
                "   create FLOAT varFloat2 = 1.2345e3" + Environment.NewLine +
                "   create CHAR varChar2 = \"?\"" + Environment.NewLine +
                "   create STRING varString2 = \"Goodbye\"" + Environment.NewLine +
                Environment.NewLine +
                "   out \"Global Boolean Variable: \", varBool, endl" + Environment.NewLine +
                "   out \"Global Integer Variable: \", varInt, endl" + Environment.NewLine +
                "   out \"Global Float Variable: \", varFloat, endl" + Environment.NewLine +
                "   out \"Global Character Variable: \", varChar, endl" + Environment.NewLine +
                "   out \"Global String Variable: \", varString, endl" + Environment.NewLine +
                "   out \"--------------------\", endl" + Environment.NewLine +
                "   out \"Local Boolean Variable: \", varBool2, endl" + Environment.NewLine +
                "   out \"Local Integer Variable: \", varInt2, endl" + Environment.NewLine +
                "   out \"Local Float Variable: \", varFloat2, endl" + Environment.NewLine +
                "   out \"Local Character Variable: \", varChar2, endl" + Environment.NewLine +
                "   out \"Local String Variable: \", varString2, endl" + Environment.NewLine +
                "   out \"--------------------\", endl" + Environment.NewLine +
                "   out \"Boolean Constant: \", constBool, endl" + Environment.NewLine +
                "   out \"Integer Constant: \", constInt, endl" + Environment.NewLine +
                "   out \"Float Constant: \", constFloat, endl" + Environment.NewLine +
                "   out \"Character Constant: \", constChar, endl" + Environment.NewLine +
                "   out \"String Constant: \", constString, endl" + Environment.NewLine +
                "   out \"--------------------\", endl" + Environment.NewLine +
                Environment.NewLine +
                "   out \"Choice Test #1:\", endl" + Environment.NewLine +
                "   out \"--------------------\", endl" + Environment.NewLine +
                "   choice(varInt)" +Environment.NewLine+
                "   on (1)"+ Environment.NewLine +
                "   {" + Environment.NewLine +
                "     out 1, endl"+ Environment.NewLine +
                "   }" + Environment.NewLine +
                "   on (2 ... 4)" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "     out 2, endl" + Environment.NewLine +
                "   }" + Environment.NewLine +
                "   on (5)" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "     out 3, endl" + Environment.NewLine +
                "   }" + Environment.NewLine +
                "   other" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "     out false, endl" + Environment.NewLine +
                "   }" + Environment.NewLine +
                Environment.NewLine +
                "   out \"Loop Test #1:\", endl" + Environment.NewLine +
                "   out \"--------------------\", endl" + Environment.NewLine +
                "   set varInt2 = 1" + Environment.NewLine +
                "   loop" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "     out varInt2, endl" + Environment.NewLine +
                "     until(varInt2 = 5)" + Environment.NewLine +
                "     set varInt2 = varInt2 + 1" + Environment.NewLine +
                "   }" + Environment.NewLine +
                "}";
                //*/
        }

        private void Form1_Load(object sender, EventArgs e) { }
        
        private void button_Tokenize_click(object sender, EventArgs e)
        {
            tokenizer = new HighCTokenizer();
            List<HighCToken> tokens;
            textbox_OutputBox.Text = "";
            textbox_BuildBox.Text = "";
            String tokenStrings = "";

            textbox_BuildBox.Text = "Generating tokens..." + Environment.NewLine;
            if (tokenizer.tokenize(textbox_InputBox.Text) == true)
            {
                tokenizer.finalizeTokenization();
                tokens = tokenizer.getTokens();

                HighCTokenAnalyzer.analyzeTokens(tokens);

                foreach (HighCToken token in tokens)
                {
                    tokenStrings += token + Environment.NewLine;
                }
            }
            else
            {
                Tabs_Output.SelectTab("tabBuild");
                textbox_BuildBox.Text += "Tokenizer has encountered an error:"+Environment.NewLine+tokenizer.getDebugLog();
                return;
            }

            HighCParser parser = new HighCParser(tokens);
            if (parser.parse() == true)
            {
                textbox_BuildBox.Text += tokenStrings;
                textbox_BuildBox.Text += Environment.NewLine + "Parse successful...";
                Tabs_Output.SelectTab("tabOutput");
                textbox_OutputBox.Text = parser.getConsoleText();
            }
            else
            {
                Tabs_Output.SelectTab("tabBuild");
                textbox_OutputBox.Text = parser.getConsoleText();
                textbox_BuildBox.Text += "Parser has encountered an error:" + Environment.NewLine + parser.getDebugLog();
            }
        }
        
    }
}
