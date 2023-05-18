using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Text.Differencing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Match = System.Text.RegularExpressions.Match;


namespace MyPlagin
{
    public class GetStatistic
    {
        public readonly Document mDocument;

        protected readonly Regex mFunctionBeginningPattern = new Regex(@"([_a-zA-Z0-9*]+)\s*\**\s*([a-zA-Z0-9*]+)\s*\((.*\n*)*?.*\)\s*\n*\s*\{");
        protected readonly Regex mFunctionParaments = new Regex("\\(\".*\\n*\"\\)");

        protected readonly Regex mTextInsideQuot = new Regex(@"("".*\n*?.*"")"); 
        protected readonly Regex mTextInsideQuot1 = new Regex(@"(\'.*\n*?.*\')");
        
        protected readonly Regex mMultilineComment = new Regex("\\/\\*[\\s\\S]*?\\*\\/\\n?");
        protected readonly Regex mOnelineComment = new Regex(@"\/\/(?:(?:\\\n)|\\|(?!\\).)*\n");


        private string[] mKeyWords = {
            "alignas", "alignof", "and", "and_eq", "asm", "auto", "bitand", "bitor", "bool", "break", "case", "catch", "char",
            "char16_t", "char32_t", "class", "compl", "const", "constexpr", "const_cast", "continue", "decltype", "default", "delete", "do", "double",
            "dynamic_cast", "else", "enum", "explicit", "export", "extern", "false", "float", "for", "friend", "goto", "if", "inline", "int", "long",
            "mutable", "namespace", "new", "noexcept", "not", "not_eq", "nullptr", "operator", "or", "or_eq", "private", "protected", "public",
            "register", "reinterpret_cast", "return", "short", "signed", "sizeof", "static", "static_assert", "static_cast", "struct", "switch", "template",
            "this", "thread_local", "throw", "true", "try", "typedef", "typeid", "typename", "union", "unsigned", "using", "virtual", "void", "volatile",
            "wchar_t", "while", "xor", "xor_eq", "struct", "reinterpret_cast"
        };

        public GetStatistic(Document document)
        {
            mDocument = document;
        }

        public List<Model> GetListModelFunctions()
        {
            FileCodeModel fileCM = mDocument.ProjectItem.FileCodeModel;
            CodeElements elts = fileCM.CodeElements;
            List<Model> data = MainProcessor(elts);
            return data;
        }

        private List<Model> MainProcessor(CodeElements elts)
        {
            List<Model> funcs = new List<Model>();
            for (int i = 1, j = 0; i <= elts.Count; ++i)
            {
                CodeElement elt_1 = elts.Item(i);
                if (elt_1.Kind == vsCMElement.vsCMElementFunction)
                {
                    var elt = elts.Item(i) as CodeFunction;
                    string functionName = elt.FullName; //name
                    int functionLines = FunctionLineCounter(elt);
                    string functionText = CodeFunctionToString(elt);

                    Tuple<int, int> countsCommentsAndKeyName = GetCommentsLineCount(functionText);
                    int functionLinesWithoutComments = countsCommentsAndKeyName.Item1 + 1;
                    int emptyLines = GetEmptyLines(functionText);

                    Model obj = new Model();
                    obj.setValues(functionName, functionLines, functionLinesWithoutComments, countsCommentsAndKeyName.Item2, emptyLines);
                    funcs.Add(obj);
                }
            }
            return funcs;
        }
        private int FunctionLineCounter(CodeFunction elt)
        {
            return elt.GetEndPoint(vsCMPart.vsCMPartBodyWithDelimiter).Line - elt.GetStartPoint(vsCMPart.vsCMPartHeader).Line + 1;
        }

        private int GetEmptyLines(string functioText)
        {
            int emptyLines = 0;
            string[] lines = functioText.Split('\n');
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line.Trim()))
                    emptyLines++;
            }
            return emptyLines;
        }


        private Tuple<int, int> GetCommentsLineCount(string functionText)
        {
            functionText = functionText.Replace("\r", "");
            functionText = functionText.Replace("\t", "");

            int countComments = 0, countNoComments = 0;

            MatchCollection insideQuot = mFunctionParaments.Matches(functionText);
            foreach (Match match in insideQuot)
                functionText = ReplaceFirstOccurrence(functionText, match.Value, "");

            MatchCollection insideQuotForString = mTextInsideQuot.Matches(functionText);
            foreach (Match match in insideQuotForString)
            {
                int k = match.Value.IndexOf('\n');
                if (k > -1) countComments++;
                functionText = ReplaceFirstOccurrence(functionText, match.Value, "");
            }

            MatchCollection insideQuotForString1 = mTextInsideQuot1.Matches(functionText);
            foreach (Match match in insideQuotForString1)
            {
                int k = match.Value.IndexOf('\n');
                if (k > -1) countComments++;
                functionText = ReplaceFirstOccurrence(functionText, match.Value, "");
            }


            MatchCollection multiLineComments = mMultilineComment.Matches(functionText);
            foreach (Match match in multiLineComments)
            {
                int j = functionText.IndexOf(match.Value) - 1;
                int tmp = 0;
                int ttt = 0;
                while (j >= 0)
                {
                    if (functionText[j] == '\n')
                    {
                        if (j > 0 && functionText[j - 1] == '\\') tmp = 1;
                        break;
                    }
                    if (ttt == 0 && functionText[j] != ' ') ttt = 1;
                    if (functionText[j] == '/')
                    {
                        tmp = 1;
                        break;
                    }
                    j--;
                }
                if (tmp == 0)
                {
                    if (ttt == 1) countComments++;
                    functionText = ReplaceFirstOccurrence(functionText, match.Value, "");
                }
            }


            MatchCollection oneLineComment = mOnelineComment.Matches(functionText);
            foreach (Match match in oneLineComment)
            {
                int j = functionText.IndexOf(match.Value) - 1;
                if (j >= 0 && functionText[j] == '\n')  
                {
                    countNoComments++;
                }
                else
                {
                    while (j >= 0 && functionText[j] != '\n')
                    {
                        if (functionText[j] != ' ')
                        {
                            countNoComments++;
                            break;
                        }

                        j--;
                    }
                }
                functionText = ReplaceFirstOccurrence(functionText, match.Value, "");
            }


            

            countComments += functionText.Count(v => v == '\n') + countNoComments;

            int countKeyName = 0;
            for (int i = 0; i < mKeyWords.Length; ++i)
            {
                string pattern = "";
                pattern += mKeyWords[i] + "\\W";
                countKeyName += Regex.Matches(functionText, @pattern).Count;
                pattern = null;
                pattern = "(_";
                pattern += mKeyWords[i] + "\\W" + ")|(\\w" + mKeyWords[i] + "\\W)";
                countKeyName -= Regex.Matches(functionText, @pattern).Count;
            }

            return Tuple.Create(countComments, countKeyName);

        }

        private string CodeFunctionToString(CodeFunction elt)
        {
            TextPoint start = elt.GetStartPoint(vsCMPart.vsCMPartHeader);
            TextPoint finish = elt.GetEndPoint();
            return start.CreateEditPoint().GetText(finish);
        }


        public static string ReplaceFirstOccurrence(string Source, string Find, string Replace)
        {
            int Place = Source.IndexOf(Find);
            if (Place == -1)
            {
                return Source;
            }
            string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }
    }
}
