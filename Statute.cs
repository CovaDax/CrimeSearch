using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Crime_Search
{
    [Serializable]
    public class Statute
    {
        public string statute;
        public int chapter; 
        public string title; 
        public  int wordCount;
        [XmlElement("favorite")]
        public bool favorite;

        public Statute()
        {

        }

        public Statute(string file)
        {
            statute = file;
            wordCount = 0;
            favorite = false;
        }


        public void setTitle(string title)
        {
            this.title = title;
        }

        public string getTitle()
        {
            return title;
        }

        public void setChapter(int chapter)
        {
            this.chapter = chapter;
        }

        public int getChapter()
        {
            return chapter;
        }

        public string getStatute()
        {
            return statute;
        }

        public void setFavorite(bool fave)
        {
            this.favorite = fave;  
        }

        public bool isFavorite()
        {
            return favorite;
        }

        public int getWordCount()
        {
            return wordCount;
        }

        public void setWordCount(string search)
        {
                string s = System.IO.File.ReadAllText(this.statute);
                string output;
                output = Regex.Replace(s, "<[^>]*>", string.Empty);

                //get rid of multiple blank lines
                output = Regex.Replace(output, @"^\s*$\n", string.Empty, RegexOptions.Multiline);

                string temp = output.ToLower();
                if (search != "")
                {
                    this.wordCount = CountWords(temp, search);
                }
        }

        private int CountWords(string text, string word)
        {
            int count = (text.Length - text.Replace(word, "").Length) / word.Length;
            return count;
        }
    }
}



