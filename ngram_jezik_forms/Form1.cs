using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ngram_jezik_forms
{
    public partial class Form1 : Form
    {
        NGramBasedTextCategorization ngtc;
        public Form1()
        {
            InitializeComponent();
            ngtc = new NGramBasedTextCategorization();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openedFile = new OpenFileDialog();
            if (openedFile.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            else {
                ngtc.generate_ngram_frequency_profile_from_file(openedFile.FileName, textBox1.Text + ".txt");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label2.Text = ngtc.guess_language(textBox2.Text).Substring(0, 3);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openedFile = new OpenFileDialog();

            if (openedFile.ShowDialog() != DialogResult.OK) return;
            else {
                richTextBox1.Text = "";
                XDocument doc = XDocument.Load(openedFile.FileName);
                var pages = doc.Descendants("page");
                foreach (var page in pages)
                {
                    var paragraphs = page.Descendants("p");
                    foreach (var paragraph in paragraphs)
                    {
                        richTextBox1.Text += paragraph.FirstAttribute.Value + " - " + ngtc.guess_language(paragraph.Value).Substring(0, 3) + "\n";
                    }
                }
                var documents = doc.Descendants("document");
                richTextBox1.SaveFile(documents.First().FirstAttribute.Value + ".res", RichTextBoxStreamType.PlainText);
            }
        }
        //http://lit.ijs.si/leposl.html (slo)
        //http://english-e-books.net/ (eng)
        //http://learnoutlive.com/easy-german-novels-beginners-intermediate-learners/ (ger)
    }
}
