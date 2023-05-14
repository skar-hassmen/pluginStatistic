using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPlagin
{
    public class Model
    {
        private string nameFunction;
        private int countLines;
        private int countLinesWithoutComments;
        private int countEmptyLines;
        private int countKeyWords;

        public void setValues(string pNameFunction, int pCountLines, int pCountLinesWithoutComments, int pCountEmptyLines, int pCountKeyWords)
        {
            nameFunction = pNameFunction;
            countLines = pCountLines;
            countLinesWithoutComments = pCountLinesWithoutComments;
            countEmptyLines = pCountEmptyLines;
            countKeyWords = pCountKeyWords;
        }

        public string getNameFunction()
        {
            return nameFunction;
        }
        public int getCountLines()
        {
            return countLines;
        }

        public int getCountLinesWithoutComments()
        {
            return countLinesWithoutComments;
        }

        public int getCountEmptyLines()
        {
            return countEmptyLines;
        }

        public int getCountKeyWords()
        {
            return countKeyWords;
        }
    }
}