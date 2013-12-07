using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using Fettrens;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace Fettrens3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool initializing = true;
        private bool stateChangeLock = true;
        internal Fettrens.FettApplication app;
        private DispatcherTimer Clock;
        private FrameworkElement NormalFrameworkElement = null;

        public MainWindow()
        {
            InitializeComponent();
            app = new Fettrens.FettApplication();

            InitComboBoxes();
            InitDataGrid();
            InitStackPanel();

            MainGrid.Background = Brushes.Ivory;
            initializing = false;
        }

        private void InitStackPanel()
        {
            NormalFrameworkElement = (FrameworkElement)GetBorder(StateMachine.States.Normal, "/Icons/Dad-icon.png");
            StatePanel.Children.Add(NormalFrameworkElement);
            StatePanel.Children.Add(GetBorder(StateMachine.States.ManualLabor, "/Icons/Snowman-icon.png"));
            StatePanel.Children.Add(GetBorder(StateMachine.States.PaiedStop, "/Icons/Knight-icon.png"));
            StatePanel.Children.Add(GetBorder(StateMachine.States.UnplannedRepair, "/Icons/Robot-icon.png"));
            StatePanel.Children.Add(GetBorder(StateMachine.States.Prepare, "/Icons/Mermaid-icon.png"));
            StatePanel.Children.Add(GetBorder(StateMachine.States.Service, "/Icons/Pirate-icon.png"));
            StatePanel.Children.Add(GetBorder(StateMachine.States.UnpaidStop, "/Icons/Devil-icon.png"));
            StatePanel.Children.Add(GetBorder(StateMachine.States.Transport, "/Icons/Leprechaun-icon.png"));
        }


        private UIElement GetBorder(StateMachine.States state, string img)
        {
            Border border = new Border();
            border.BorderThickness = new Thickness(2);
            border.BorderBrush = Brushes.Black;
            border.CornerRadius = new CornerRadius(12);
            border.Padding = new Thickness(4);
            border.Margin = new Thickness(4);
            Grid grid = new Grid();
            var statename = xmlManager.getStateName(state);
            grid.Children.Add(new TextBlock() { Text = statename, FontSize = 14, Tag = state });
            grid.Children.Add(GetImage(state, img));
            grid.Focusable = true;
            grid.MouseDown += new MouseButtonEventHandler(Grid_MouseDown);
            grid.Tag = state;
            border.Child = grid;
            resetBorder(border);
            return border;
        }

        private UIElement GetImage(StateMachine.States state, string img)
        {
            Image myImage = new Image();
            //myImage.Width = 150;
            myImage.Height = 80;
            myImage.Tag = state;

            // Create source
            BitmapImage myBitmapImage = new BitmapImage();

            // BitmapImage.UriSource must be in a BeginInit/EndInit block
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(img, UriKind.Relative);
            //myBitmapImage.DecodePixelWidth = 150;
            myBitmapImage.DecodePixelHeight = 80;
            myBitmapImage.EndInit();
            //set image source
            myImage.Source = myBitmapImage;
            return myImage;
        }

        private void InitComboBoxes()
        {
            DriverComboBox.ItemsSource = app.GetDrivers();
            DriverComboBox.SelectedItem = Properties.Settings.Default.Driver;
            StateComboBox.ItemsSource = app.GetAllStates();
            StateComboBox.SelectedItem = app.currentStateText;
            Tool1ComboBox.ItemsSource = app.GetTool1Array();
            Tool1ComboBox.SelectedItem = Properties.Settings.Default.Tool1;
            Tool2ComboBox.ItemsSource = app.GetTool2Array();
            Tool2ComboBox.SelectedItem = Properties.Settings.Default.Tool2;
            CostumerComboBox.ItemsSource = app.GetCostumers();
            CostumerComboBox.SelectedItem = Properties.Settings.Default.Costumer;
        }

        private void InitDataGrid()
        {
            EditDataGrid.ItemsSource = app.TimePostList;
            EditDataGrid.CanUserReorderColumns = true;

            Clock = new DispatcherTimer();
            Clock.Interval = new TimeSpan(10000000);
            Clock.Tick += new EventHandler(Clock_Tick);
            Clock.IsEnabled = true;

            var StateColumn = new DataGridComboBoxColumn();
            StateColumn.Header = "State";
            StateColumn.SelectedItemBinding = new Binding("State");
            StateColumn.ItemsSource = app.GetAllStates();
            EditDataGrid.Columns.Add(StateColumn);

            var DriverColumn = new DataGridComboBoxColumn();
            DriverColumn.Header = "Driver";
            DriverColumn.SelectedItemBinding = new Binding("Driver");
            DriverColumn.ItemsSource = app.GetDrivers();
            EditDataGrid.Columns.Add(DriverColumn);

            var CostumerColumn = new DataGridComboBoxColumn();
            CostumerColumn.Header = "Costumer";
            CostumerColumn.SelectedItemBinding = new Binding("Costumer");
            CostumerColumn.ItemsSource = app.GetCostumers();
            EditDataGrid.Columns.Add(CostumerColumn);

            var Tool1Column = new DataGridComboBoxColumn();
            Tool1Column.Header = "Tool1";
            Tool1Column.SelectedItemBinding = new Binding("Tool1");
            Tool1Column.ItemsSource = app.GetTool1Array();
            EditDataGrid.Columns.Add(Tool1Column);

            var Tool2Column = new DataGridComboBoxColumn();
            Tool2Column.Header = "Tool2";
            Tool2Column.SelectedItemBinding = new Binding("Tool2");
            Tool2Column.ItemsSource = app.GetTool2Array();
            EditDataGrid.Columns.Add(Tool2Column);

            var StartTimeColumn = new DataGridTextColumn();
            StartTimeColumn.Header = "Start Time";
            StartTimeColumn.Binding = new Binding("StartTimeText");
            EditDataGrid.Columns.Add(StartTimeColumn);

            var CommentColumn = new DataGridTextColumn();
            CommentColumn.Header = "Kommentar";
            var b = new Binding("Comment");
            b.Mode = BindingMode.TwoWay;
            CommentColumn.Binding = b;
            CommentColumn.Width = 200;
            EditDataGrid.Columns.Add(CommentColumn);
        }

        void Clock_Tick(object sender, EventArgs e)
        {
            TimeTextBox.Text = app.CurrentTime();
            PriceTextBox.Text = app.CurrentCost();
            checkForStart();
        }

        private Fettrens.Setting getSettings()
        {
            return new Fettrens.Setting(
                (string)DriverComboBox.SelectedItem ?? string.Empty,
                (string)Tool1ComboBox.SelectedItem ?? string.Empty,
                (string)Tool2ComboBox.SelectedItem ?? string.Empty,
                (string)CostumerComboBox.SelectedItem ?? string.Empty,
                (string)StateComboBox.SelectedItem ?? string.Empty);
        }

        private void ChangeState(StateMachine.States NewState)
        {
            if (initializing) return;
            if (stateChangeLock)
            {
                stateChangeLock = false;
                app.ChangeState(NewState);
                StateComboBox.SelectedValue = xmlManager.getStateName(NewState);
                stateChangeLock = true;
                NewStamp();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Setting finalsettings = getSettings();
            Properties.Settings.Default.Driver = finalsettings.driver;
            Properties.Settings.Default.Costumer = finalsettings.costumer;
            Properties.Settings.Default.Tool1 = finalsettings.tool1;
            Properties.Settings.Default.Tool2 = finalsettings.tool2;
            Properties.Settings.Default.Save();

            app.NewTimeStamp(finalsettings);
            app.UpdateXMLLogg();
        }

        private void NewStamp()
        {
            if (initializing)
            {
                app.UpdateSettings(getSettings());
            }
            else
            {
                app.NewTimeStamp(getSettings());
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!initializing)
            {
                app.NewTimeStamp(getSettings());
                TimeTextBox.Text = "0";
            }
        }

        private void CostumerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!initializing)
            {
                app.ChangeState(StateMachine.States.Normal);
                StateComboBox.SelectedItem = app.currentStateText;
                UpdateStackPanel(NormalFrameworkElement);
                app.NewTimeStamp(getSettings());
                TimeTextBox.Text = "0";
                PriceTextBox.Text = "0";
            }
            NextButton.Content = app.NextCostumer();
            PreviousButton.Content = app.PreviousCostumer();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            CostumerComboBox.SelectedValue = app.NextCostumer();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            CostumerComboBox.SelectedValue = app.PreviousCostumer();
        }

        private void checkForStart()
        {
            if (app.LongStart())
            {
                MainGrid.Background = Brushes.IndianRed;
            }
            else
            {
                MainGrid.Background = Brushes.Ivory;
            }
        }

        private void Grid_MouseDown(Object sender, MouseButtonEventArgs e)
        {
            StateMachine.States? clickedState = null;
            FrameworkElement clicksource = e.Source as FrameworkElement;
            if (clicksource != null)
            {
                clickedState = (StateMachine.States)clicksource.Tag;
                if (clickedState != null)
                {
                    ChangeState((StateMachine.States)clickedState);
                    UpdateStackPanel(clicksource);
                }
            }
        }

        private void UpdateStackPanel(FrameworkElement clicksource)
        {
            foreach (Border border in StatePanel.Children)
            {
                resetBorder(border);
            }
            if (clicksource == null) return;
            if (clicksource is Border)
            {
                focusBorder(clicksource as Border);
            }
            else
            {
                var theGrid = clicksource.Parent as FrameworkElement;
                var theBorder = theGrid.Parent as Border;
                focusBorder(theBorder);
            }
        }

        private void focusBorder(Border border)
        {
            if (border == null) return;
            border.Background = new SolidColorBrush(Colors.Thistle);
            border.Width = 100;
            border.Height = 140;
        }

        private void resetBorder(Border border)
        {
            border.Background = new SolidColorBrush(Colors.Wheat);
            border.Width = 90;
            border.Height = 130;
        }
    }
}