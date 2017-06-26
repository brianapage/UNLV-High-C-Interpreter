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

namespace HighCInterpreterCore
{
    public partial class Form1 : Form
    {
        HighCTokenizer tokenizer;

        public Form1()
        {
            InitializeComponent();
            //textbox_InputBox.Text = "main"+Environment.NewLine+"{"+ Environment.NewLine+"out \"Hello World\""+Environment.NewLine+"}";
            textbox_InputBox.Text =
                "global create BOOL varBool = true" + Environment.NewLine +
                "global create INT varInt = 1" + Environment.NewLine +
                "global create FLOAT varFloat = 1.23e1" + Environment.NewLine +
                "global create CHAR varChar = \"g\"" + Environment.NewLine +
                "global create STRING varString = \"Hello World\"" + Environment.NewLine +
                "main" +Environment.NewLine+
                "{"+Environment.NewLine +
                "   create BOOL varBool2 = false" + Environment.NewLine +
                "   create INT varInt2 = 2" + Environment.NewLine +
                "   create FLOAT varFloat2 = 1.2345e3" + Environment.NewLine +
                "   create CHAR varChar2 = \"?\"" + Environment.NewLine +
                "   create STRING varString2 = \"Goodbye\"" + Environment.NewLine +

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
                "   out \"Choice Test #1:\", endl" + Environment.NewLine +
                "   out \"--------------------\", endl" + Environment.NewLine +
                "   choice(varInt)" +Environment.NewLine+
                "   on (1)"+ Environment.NewLine +
                "   {" + Environment.NewLine +
                "     out 1, endl"+ Environment.NewLine +
                "   }" + Environment.NewLine +
                "   on (2)" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "     out 2, endl" + Environment.NewLine +
                "   }" + Environment.NewLine +
                "   on (3)" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "     out 3, endl" + Environment.NewLine +
                "   }" + Environment.NewLine +
                "   other" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "     out false, endl" + Environment.NewLine +
                "   }" + Environment.NewLine +
                "   out \"Loop Test #1:\", endl" + Environment.NewLine +
                "   out \"--------------------\", endl" + Environment.NewLine +
                "   loop" + Environment.NewLine +
                "   {" + Environment.NewLine +
                "     out varInt2, endl" + Environment.NewLine +
                "     until(varInt2 = 5)" + Environment.NewLine +
                "     set varInt2 = varInt2 + 1" + Environment.NewLine +
                "   }" + Environment.NewLine +
                "}";
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
