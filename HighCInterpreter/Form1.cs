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
            textbox_InputBox.Text ="main{if(1=2){out \"Hello World\"}}";
        }

        private void Form1_Load(object sender, EventArgs e) { }
        
        private void button_Tokenize_click(object sender, EventArgs e)
        {
            tokenizer = new HighCTokenizer();
            List<HighCToken> tokens;
            textbox_OutputBox.Text = "";
            textbox_BuildBox.Text = "";

            if (tokenizer.tokenize(textbox_InputBox.Text) == true)
            {
                tokenizer.finalizeTokenization();
                tokens = tokenizer.getTokens();

                HighCTokenAnalyzer.analyzeTokens(tokens);

                textbox_BuildBox.Text = "Generating tokens..."+Environment.NewLine;
                foreach (HighCToken token in tokens)
                {
                    textbox_BuildBox.Text += token + Environment.NewLine;
                }
            }
            else
            {
                Tabs_Output.SelectTab("tabBuild");
                textbox_BuildBox.Text = "Tokenizer has encountered an error:"+Environment.NewLine+tokenizer.getDebugLog();
                return;
            }

            HighCParser parser = new HighCParser(tokens);
            if (parser.parse() == true)
            {
                textbox_BuildBox.Text += Environment.NewLine + "Parse Status: Success";
                Tabs_Output.SelectTab("tabOutput");
                textbox_OutputBox.Text = parser.getConsoleText();
            }
            else
            {
                Tabs_Output.SelectTab("tabBuild");
                textbox_OutputBox.Text = parser.getConsoleText();
                textbox_BuildBox.Text = "Parser has encountered an error:" + Environment.NewLine + parser.getDebugLog();
            }
        }
        
    }
}
