using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;


namespace EasyCompare
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private List<ObservableCollection<Destination>> _operators;
        bool _UILoadingAnimation = false; //for logo
        string _destination_Excel_url = "";///////////////////////////////////////////
        private String _logviewer = "";
        public int TotalOperatorCount = 0;
        public String Title = "";

        public ViewModel()
        {
            this.PropertyChanged += ViewModel_PropertyChanged;

            _operators = new List<ObservableCollection<Destination>>();
        }

        public bool UILoadingAnimation //for logo
        {
            get { return _UILoadingAnimation; }
            set
            {
                _UILoadingAnimation = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("UILoadingAnimation");
            }
        }

        public string Destination_Excel_url //for logo
        {
            get { return _destination_Excel_url; }
            set
            {
                _destination_Excel_url = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("Destination_Excel_url");
            }
        }

        private bool _excelLoaded = false;

        public bool ExcelLoaded
        {
            get { return _excelLoaded; }
            set
            {
                _excelLoaded = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("ExcelLoaded");
            }
        }


        public String LogViewer
        {
            get { return _logviewer; }
            set
            {
                _logviewer = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("LogViewer");
            }
        }

        protected void OnPropertyChanged(string data)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(data));
            }
        }

        public List<String> OperatorName = new List<string>();
        public List<ObservableCollection<Destination>> Operators
        {
            get { return _operators; }
            set
            {
                _operators = value;
                OnPropertyChanged("NodesChanged");
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
        }

        public async void LoadingThread(bool withDestName)
        {
            Task tsk = LoadExcelDataAsync(withDestName);
            await Task.WhenAll(tsk);
            tsk.Dispose();
        }

        public Task LoadExcelDataAsync(bool withDestName)
        {
            return Task.Run(() => LoadExcelData(withDestName));
        }

        void LoadExcelData(bool withDestName)
        {
            UILoadingAnimation = true; //for logo

            int count = ImportExcelFile(withDestName);
            TotalOperatorCount++;
            if (this.Operators[TotalOperatorCount - 1].Count > 0)
            {
                LogViewer = "Excel file imported. Total number of Destinations for " + OperatorName[TotalOperatorCount - 1] + " is " + count.ToString();
                Write_logFile(LogViewer);

                ExcelLoaded = true;
                UILoadingAnimation = false; //for logo

            }
        }

        private int ImportExcelFile(bool withDestName)
        {
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet1;
            xlWorkBook = xlApp.Workbooks.Open(Destination_Excel_url, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet1 = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            int count = 0;

            try
            {
                Excel.Range last = xlWorkSheet1.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell, Type.Missing);


                int lastUsedRow = last.Row;
                int lastUsedColumn = last.Column;


                ObservableCollection<Destination> destinations = new ObservableCollection<Destination>();

                if(!withDestName)
                {
                    for (int i = 2; i <= lastUsedRow; i++)
                    {
                        Destination _nd = new Destination();
                        _nd.NumberPrefix = xlWorkSheet1.Cells[i, 1].Value2.ToString();
                        _nd.Rate = xlWorkSheet1.Cells[i, 2].Value2.ToString();
                        destinations.Add(_nd);
                    }
                }
                else
                {
                    for (int i = 2; i <= lastUsedRow; i++)
                    {
                        Destination _nd = new Destination();
                        _nd.NumberPrefix = xlWorkSheet1.Cells[i, 1].Value2.ToString();
                        _nd.Rate = xlWorkSheet1.Cells[i, 2].Value2.ToString();
                        _nd.DestName = xlWorkSheet1.Cells[i, 3].Value2.ToString();
                        destinations.Add(_nd);
                    }
                }

                Operators.Add(destinations);
                count = destinations.Count;                
            }
            catch (Exception ex)
            {
                this.LogViewer = "Error in importing excel: " + ex.Message + " <" + ex.GetType().ToString() + ">";
                Write_logFile("Error in importing excel: " + ex.Message + " <" + ex.GetType().ToString() + ">");
                MessageBox.Show("There may be wrong data in excel file. Excel may be partially loaded.\nTo load fully, please correct the excel and load again.", "MagnetQ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                object misValue = System.Reflection.Missing.Value;
                xlWorkBook.Close(false, misValue, misValue);
                xlApp.Quit();
                releaseObject(xlWorkSheet1);
                releaseObject(xlWorkBook);
                releaseObject(xlApp);
            }
            return count;
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                this.LogViewer = "Error in releasing object: " + ex.Message + " <" + ex.GetType().ToString() + ">";
                Write_logFile("Error in releasing object: " + ex.Message + " <" + ex.GetType().ToString() + ">");
            }
            finally
            {
                GC.Collect();
            }
        }


        private Object logFileLock = new Object();

        public void Write_logFile(String str)///////////////////////////////////////////////////////////////////////////////////////////////////
        {
            try
            {
                lock (logFileLock)
                {
                    // Write the string to a file.

                    if (!Directory.Exists(@"C:\\Users\\Public\\" + Title + " Log"))
                    {
                        Directory.CreateDirectory(@"C:\\Users\\Public\\" + Title + " Log");
                    }

                    System.IO.StreamWriter Logfile = new System.IO.StreamWriter(@"C:\\Users\\Public\\" + Title + " Log\\" + Title + "_" + DateTime.Now.ToString("MMMM") + "_" + DateTime.Now.Year.ToString() + ".log", true);
                    Logfile.WriteLine(DateTime.Now.ToString() + ":- " + str);
                    Logfile.Close();
                }
            }
            catch (Exception ex)
            {
                this.LogViewer = ex.Message + " <" + ex.GetType().ToString() + ">";
            }
        }
    }
}
