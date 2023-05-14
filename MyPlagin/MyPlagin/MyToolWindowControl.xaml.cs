using EnvDTE;
using EnvDTE80;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace MyPlagin
{
    /// <summary>
    /// Interaction logic for MyToolWindowControl.
    /// </summary>
    public partial class MyToolWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MyToolWindowControl"/> class.
        /// </summary>
        public MyToolWindowControl()
        {
            this.InitializeComponent();
        }

        void initInfoTable(int size)
        {

            while (InfoTable.Items.Count > 0)
                InfoTable.Items.RemoveAt(0);

            for (int i = 0; i < size; i++)
            {
                InfoTable.Items.Add(new { Func = "", Lines = "", LinesWithoutComm = "", KeyWords = "", EmptyLines = "" });
            }
        }
        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void Update(object sender, RoutedEventArgs e)
        {
            DTE2 dte = MyPlaginPackage.GetGlobalService(typeof(DTE)) as DTE2;
            Document doc = dte.ActiveDocument;

            if (!((doc.FullName.EndsWith(".cpp")) || (doc.FullName.EndsWith(".c"))))
            {
                throw new System.Exception("Incorrect file!");
            }
            
            GetStatistic statistic = new GetStatistic(doc);
            List<Model> modelList = statistic.GetListModelFunctions();

            int count = 0;
            initInfoTable(modelList.Count);
            foreach (Model model in modelList)
            {
                InfoTable.Items[count++] = new
                {
                    Func = model.getNameFunction(),
                    Lines = model.getCountLines().ToString(),
                    LinesWithoutComm = model.getCountLinesWithoutComments().ToString(),
                    KeyWords = model.getCountEmptyLines().ToString(),
                    EmptyLines = model.getCountKeyWords().ToString()
                };
            }
        }
    }
}