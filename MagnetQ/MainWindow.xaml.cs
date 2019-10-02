using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace EasyCompare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel VM = new ViewModel();
        string _operatorName = "";
        bool WithDestName = false;
        List<ListView> LVList = new List<ListView>();
        CollectionView view;

        public MainWindow()
        {
            InitializeComponent();
            timerforPopup.Interval = TimeSpan.FromSeconds(5);
            timerforPopup.Tick += timer_TickForPopup;
            DispatcherTimerLogoAnimation();   //for logo
            VM.PropertyChanged += View_PropertyChanged;
            VM.Title = this.Title;

            Application.Current.MainWindow.Closing += MainWindow_Closing;
        }



        private void CheckLog_btn_Click(object sender, RoutedEventArgs e)
        {
            Popup_Status.IsOpen = true;
        }

        private void StatusCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Popup_Status.IsOpen = false;
        }

        private void WithDestination_Radbtn_Checked(object sender, RoutedEventArgs e)
        {
            WithDestName = true;
        }

        private void WithoutDestination_Radbtn_Checked(object sender, RoutedEventArgs e)
        {
            WithDestName = false;
        }

        private void SelectFile_function_Click_1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".xls";
            dlg.Filter = "Excel Worksheets|*.xls;*.xlsx";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                VM.Destination_Excel_url = filename;
                Show_LogTextblock("File has been selected successfully. Path is " + VM.Destination_Excel_url);
                VM.Write_logFile("File has been selected successfully. Path is " + VM.Destination_Excel_url);
            }
        }

        private Object thisLock = new Object();
        void Show_LogTextblock(String str)
        {
            try
            {
                lock (thisLock)
                {

                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        log_textblock.Text = log_textblock.Text + "# " + DateTime.Now.ToString() + ":- " + str + "\n";
                        _scrollbar_log.ScrollToBottom();
                    }));
                }
            }
            catch (Exception ex)
            {
                VM.Write_logFile(ex.Message + " <" + ex.GetType().ToString() + ">");
            }
        }

        private void exit_function_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Closing application will delete all data, do you want to continue?", "MagnetQ", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                e.Cancel = true;

            }
            else if (MessageBox.Show("Are you sure?", "MagnetQ", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void View_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LogViewer")
            {
                Show_LogTextblock(VM.LogViewer);
            }
            else if (e.PropertyName == "UILoadingAnimation") //for logo
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (!AnimationTimer.IsEnabled)
                        AnimationTimer.Start();
                }));
            }
            else if (e.PropertyName == "ExcelLoaded")
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (VM.ExcelLoaded)
                    {
                        OperatorNameLoad(VM.TotalOperatorCount);
                        ListViewLoad(VM.TotalOperatorCount);
                        VM.Destination_Excel_url = "";
                        RemoveGrid.IsEnabled = true;
                    }
                }));
            }
            else if(e.PropertyName == "Destination_Excel_url")
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (VM.Destination_Excel_url == "")
                    {
                        FileNameTxtblk.Text = "";
                    }
                    else
                        FileNameTxtblk.Text = "Excel file path is " + VM.Destination_Excel_url;
                }));
            }
        }

        private void LoadExcel_btn_Click(object sender, RoutedEventArgs e)
        {
            if (NameBox.Text == "")
            {
                MessageBox.Show("Please give an operator name which you want to load at 'Operator Name' box.", "MagnetQ", MessageBoxButton.OK, MessageBoxImage.Question);
            }
            else if (VM.Destination_Excel_url == "")
            {
                MessageBox.Show("Please select an Excel file for the operator: " + NameBox.Text, "MagnetQ", MessageBoxButton.OK, MessageBoxImage.Question);
            }
            else
            {
                _stackpanel.IsEnabled = false;
                VM.OperatorName.Add(_operatorName);
                VM.LoadingThread(WithDestName);
            }
        }
        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            _operatorName = NameBox.Text;
        }

        private void OperatorNameLoad(int num)
        {
            ColumnDefinition _gridCol = new ColumnDefinition();
            _grid1.ColumnDefinitions.Add(_gridCol);

            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = (num).ToString() + ". " + VM.OperatorName[num - 1];
            txtBlock1.FontSize = 14;
            txtBlock1.FontWeight = FontWeights.Bold;
            txtBlock1.HorizontalAlignment = HorizontalAlignment.Center;
            txtBlock1.Foreground = new SolidColorBrush(Colors.Green);
            txtBlock1.VerticalAlignment = VerticalAlignment.Top;
            txtBlock1.Background = new SolidColorBrush(Colors.AntiqueWhite);
            Grid.SetColumn(txtBlock1, num - 1);
            txtBlock1.Tag = num;
            txtBlock1.Margin = new Thickness(5, 0, 5, 0);

            txtBlock1.MouseEnter += TxtBlock_MouseEnter;
            txtBlock1.MouseLeave += TxtBlock_MouseLeave;

            _grid1.Children.Add(txtBlock1);
            NameBox.Text = "";
        }

        private void TxtBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock txtblk = sender as TextBlock;
            int num = Convert.ToInt32(txtblk.Tag.ToString());
            Popup_CountBtn_textblock.Text = " Total count = " + VM.Operators[num - 1].Count.ToString() + " ";
            Popup_Count.IsOpen = true;
            timerforPopup.Start();
        }

        private void TxtBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup_Count.IsOpen = false;
            timerforPopup.Stop();
        }

        private void ListViewLoad(int num)
        {
            ListView lv = new ListView();

            lv.SetValue(Grid.ColumnProperty, num - 1);

            ColumnDefinition _gridCol = new ColumnDefinition();
            _grid2.ColumnDefinitions.Add(_gridCol);
            //lv.HorizontalAlignment = HorizontalAlignment.Stretch;

            _grid2.Children.Add(lv);

            GridView gv = new GridView();

            GridViewColumn gvc1 = new GridViewColumn();
            gvc1.DisplayMemberBinding = new Binding("NumberPrefix");
            gvc1.Header = "Prefix";
            gvc1.Width = System.Double.NaN;

            gv.Columns.Add(gvc1);

            GridViewColumn gvc2 = new GridViewColumn();
            gvc2.DisplayMemberBinding = new Binding("Rate");
            gvc2.Header = "Rate";
            gvc2.Width = System.Double.NaN;

            gv.Columns.Add(gvc2);

            if (WithDestName)
            {
                GridViewColumn gvc3 = new GridViewColumn();
                gvc3.DisplayMemberBinding = new Binding("DestName");
                gvc3.Header = "Destination";
                gv.Columns.Add(gvc3);
            }

            LVList.Add(lv);
            LVList[num - 1].ItemsSource = VM.Operators[num - 1];
            lv.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));
            lv.View = gv;

            view = (CollectionView)CollectionViewSource.GetDefaultView(LVList[num - 1].ItemsSource);

            view.Filter = UserFilter;
        }


        int removePosition = 0;
        private void RemoveBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (RemoveBox.Text != "")
                    removePosition = Convert.ToInt32(this.RemoveBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " Please give a valid number.", "MagnetQ", MessageBoxButton.OK, MessageBoxImage.Error);
                VM.Write_logFile(ex.Message);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (removePosition > 0 && removePosition < VM.Operators.Count + 1)
            {
                if (MessageBox.Show("Do you want to remove the Operator: " + VM.OperatorName[removePosition-1] + "?", "MagnetQ", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    {
                        string op = VM.OperatorName[removePosition - 1];
                        VM.Operators.RemoveAt(removePosition - 1);
                        VM.OperatorName.RemoveAt(removePosition - 1);
                        LVList.Clear();

                        _grid1.ColumnDefinitions.Clear();
                        _grid2.ColumnDefinitions.Clear();

                        _grid1.Children.Clear();
                        _grid2.Children.Clear();

                        VM.TotalOperatorCount--;
                        RemoveBox.Text = "";

                        for (int i = 1; i <= VM.TotalOperatorCount; i++)
                        {
                            OperatorNameLoad(i);
                            ListViewLoad(i);
                        }
                        if (VM.TotalOperatorCount == 0)
                        {
                            _stackpanel.IsEnabled = true;
                            RemoveGrid.IsEnabled = false;
                        }
                        removePosition = 0;
                        Show_LogTextblock("Successfully removed the operator: " + op);
                        VM.Write_logFile("Successfully removed the operator: " + op);
                    }
                }
            }
            else
            {
                MessageBox.Show("No operator present with this serial number: " + removePosition.ToString(), "MagnetQ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private void Sort(string sortBy, ListSortDirection direction)
        {
            for (int i = 0; i < VM.TotalOperatorCount; i++)
            {
                ICollectionView dataView = CollectionViewSource.GetDefaultView(LVList[i].ItemsSource);

                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription();//(sortBy.ToString(), direction);  
                sd.PropertyName = sortBy;
                sd.Direction = direction;

                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;
            try
            {
                if (headerClicked != null)
                {
                    if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                    {
                        if (headerClicked != _lastHeaderClicked)
                        {
                            direction = ListSortDirection.Ascending;
                        }
                        else
                        {
                            if (_lastDirection == ListSortDirection.Ascending)
                            {
                                direction = ListSortDirection.Descending;
                            }
                            else
                            {
                                direction = ListSortDirection.Ascending;
                            }
                        }

                        var sortBy = "";
                        var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                        sortBy = (string)((Binding)((GridViewColumnHeader)e.OriginalSource).Column.DisplayMemberBinding).Path.Path;




                        Sort(sortBy, direction);

                        _lastHeaderClicked = headerClicked;
                        _lastDirection = direction;
                    }
                }
            }
            catch (Exception ex)
            {
                Show_LogTextblock(ex.Message + " <" + ex.GetType().ToString() + ">");
                VM.Write_logFile(ex.Message + " <" + ex.GetType().ToString() + ">");
            }
        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(Search_Textbox.Text))
                return true;
            else
            {
                return ((item as Destination).NumberPrefix.StartsWith(Search_Textbox.Text));
                
            }
        }

        DispatcherTimer timerforPopup = new DispatcherTimer();

        private void timer_TickForPopup(object sender, EventArgs e)
        {
            timerforPopup.Stop();
            AllPopupClose();
        }

        private void LoadBtn_MouseEnter_1(object sender, MouseEventArgs e)
        {
            if (VM.Destination_Excel_url == "")
                Popup_LoadBtn_textblock.Text = "No Excel file has been selected.";
            else
                Popup_LoadBtn_textblock.Text = "Excel file path is " + VM.Destination_Excel_url;

            Popup_Load.IsOpen = true;
            timerforPopup.Start();
        }

        private void AllPopupClose()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Popup_Load.IsOpen = false;
                Popup_Remove.IsOpen = false;
                Popup_Count.IsOpen = false;
            }));
        }

        private void LoadBtn_MouseLeave_1(object sender, MouseEventArgs e)
        {
            Popup_Load.IsOpen = false;
            timerforPopup.Stop();
        }

        private void Instructions_MouseEnter_1(object sender, MouseEventArgs e)
        {
            _InstructRun1.Text = "Instructions of using this app:";
            _InstructRun2.Text = Show_Instructions();
            Popup_Instruct.IsOpen = true;
        }

        private void Instructions_MouseLeave_1(object sender, MouseEventArgs e)
        {
            Popup_Instruct.IsOpen = false;
        }



        private string Show_Instructions()
        {
            return
                "\n  1. Select Excel file from File menu to insert rate list." +
                "\n  2. Select option 'with or without destinatio name' if needed." +
                "\n  3. Give operator name then click 'Load Excel Data'." +
                "\n  4. Do the same for each operator." +
                "\n  5. Search with number prefix." +
                "\n  6. Click on the column header to sort data." +
                "\n  7. You can check the status by click on 'Check Status' button." +
                "\n  8. Each log data will be saved to this directory:- C:\\Users\\Public\\" + Title + " Log\\" +
                "\n  9. You can remove any operator data. To do it, give the serial number of the operator at the upper right corner box and then click the button.";
        }

        private void timer_TickLogoAnimation(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (VM.UILoadingAnimation == true)
                {
                    if (QLogo1.Visibility == Visibility.Visible)
                    {
                        QLogo1.Visibility = Visibility.Collapsed;
                        QLogo2.Visibility = Visibility.Visible;
                        QLogo3.Visibility = Visibility.Collapsed;
                        QLogo4.Visibility = Visibility.Collapsed;
                    }
                    else if (QLogo2.Visibility == Visibility.Visible)
                    {
                        QLogo1.Visibility = Visibility.Collapsed;
                        QLogo2.Visibility = Visibility.Collapsed;
                        QLogo3.Visibility = Visibility.Visible;
                        QLogo4.Visibility = Visibility.Collapsed;
                    }
                    else if (QLogo3.Visibility == Visibility.Visible)
                    {
                        QLogo1.Visibility = Visibility.Collapsed;
                        QLogo2.Visibility = Visibility.Collapsed;
                        QLogo3.Visibility = Visibility.Collapsed;
                        QLogo4.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        QLogo1.Visibility = Visibility.Visible;
                        QLogo2.Visibility = Visibility.Collapsed;
                        QLogo3.Visibility = Visibility.Collapsed;
                        QLogo4.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    QLogo1.Visibility = Visibility.Collapsed;
                    QLogo2.Visibility = Visibility.Collapsed;
                    QLogo3.Visibility = Visibility.Collapsed;
                    QLogo4.Visibility = Visibility.Collapsed;
                    AnimationTimer.Stop();
                }
            }));
        }


        DispatcherTimer AnimationTimer = new DispatcherTimer();  //for logo

        void DispatcherTimerLogoAnimation()
        {
            AnimationTimer.Interval = TimeSpan.FromMilliseconds(250);
            AnimationTimer.Tick += timer_TickLogoAnimation;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch (Exception ex)
            {
                Show_LogTextblock(ex.Message + " <" + ex.GetType().ToString() + ">");
                VM.Write_logFile(ex.Message + " <" + ex.GetType().ToString() + ">");
            }
        }


        private void Search_Textbox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            for (int i = 0; i < VM.TotalOperatorCount; i++)
            {
                CollectionViewSource.GetDefaultView(LVList[i].ItemsSource).Refresh();
            }
        }

        private void RemoveBox_MouseEnter(object sender, MouseEventArgs e)
        {
            Popup_Remove_textblock.Text = "Give the serial number of the operator which you want to remove.";
            Popup_Remove.IsOpen = true;
            timerforPopup.Start();
        }

        private void RemoveBox_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup_Remove.IsOpen = false;
            timerforPopup.Stop();
        }
    }
}