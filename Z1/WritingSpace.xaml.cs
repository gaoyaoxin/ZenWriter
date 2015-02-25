using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;

namespace Z1
{
    public partial class WritingSpace : Window
    {
        public WritingSpace()
        {
            InitializeComponent();
            this.text.Focus();

            text.Text = "";

            defaultLocation = System.IO.Path.Combine(path,
                    DateTime.Now.ToLocalTime().ToShortTimeString().Replace(':', '-') + ".txt");

            FileLocation = defaultLocation;

            CreateSaveTimer();

            // drag move
            this.text.PreviewMouseLeftButtonDown += (o, e) => { this.DragMove(); };

            // get out of maximized
            this.text.MouseDoubleClick += (o, e) =>
            {
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
            };

            // close
            this.close.Click += (o, e) => { this.Close(); };
        }

        int size = 14;
        string font = "sans serif";

        public int FONTSIZE { get { return size; } set { size = value; text.FontSize = size; } }
        public string FONTSTYLE
        {
            get { return this.font; }
            set
            {
                font = value; switch (value)
                {
                    case "serif":
                        this.text.FontFamily = new FontFamily("Times New Roman");
                        break;
                    default:
                        this.text.FontFamily = new FontFamily("Calibri");
                        break;
                }
            }
        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            this.title.Text = CountWords(text.Text) + " words";
            ShowChangesPending();
        }

        private void ShowChangesPending()
        {
            this.title.FontStyle = FontStyles.Italic;
        }
        private void ShowNoChangesPending()
        {
            this.title.FontStyle = FontStyles.Normal;
        }

        int CountWords(string s)
        {
            MatchCollection collection = Regex.Matches(s, @"[\S]+");
            return collection.Count;
        }
                
        //----------------------------------------------------------------------
        //----------------- toggle options -------------------------------------

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ToggleOptions();
            }
        }

        void ToggleOptions()
        {
            if (options.Visibility == Visibility.Hidden)
                options.Visibility = Visibility.Visible;
            else
                options.Visibility = Visibility.Hidden;
        }

        //---------------------------------------------------------------------
        //------------------ background windows are more transparent ----------

        bool change = true;
        public bool CHANGE_OPACITY
        {
            get { return change; }
            set
            {
                change = value;
                if (!change)
                {
                    // change all windows to not transparent
                    Opacity = 1;
                }
                if (change && !this.IsActive)
                {
                    // change inactive windows to transparent
                    Opacity = .5;
                }
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (CHANGE_OPACITY)
                Opacity = 1;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (CHANGE_OPACITY)
                Opacity = .5;
        }


        #region save and load

        // default location is temporary datetime location
        string path = @"C:\Users\SparrowHawk\Desktop";
        string defaultLocation;

        string FileLocation;
        string SaveLocation
        {
            get
            {
                if (isSavingToTemp)
                    return defaultLocation;
                else
                    return FileLocation;
            }
            set
            {
                isSavingToTemp = false;
                FileLocation = value;
            }
        }

        bool isSavingToTemp = true;

        private void options_Click(object sender, RoutedEventArgs e)
        {
            var x = sender as Button;
            if (x == null)
                return;

            if (x.Name == "load")
                Load();
            else
                Save();

            options.Visibility = Visibility.Hidden;
        }

        public void Load()
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Text documents (.txt)|*.txt";
            if (o.ShowDialog() == true)
            {
                // delete temp file
                File.Delete(SaveLocation);

                SaveLocation = o.FileName;
                text.Text = File.ReadAllText(o.FileName);
            }
        }

        private void Save()
        {
            SaveFileDialog s = new SaveFileDialog();
            s.Filter = "Text documents (.txt)|*.txt";
            if (s.ShowDialog() == true)
            {
                // delete temp file
                File.Delete(SaveLocation);

                // update save location   
                SaveLocation = s.FileName;
            }
            File.WriteAllText(SaveLocation, text.Text);
        }


        //-------------------------------------------------------------------------
        //--------------------- prompt save when exiting ---------------------------

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // since we're not providing a way to cancel it
            dispatcherTimer.Stop();

            // save current text
            File.WriteAllText(SaveLocation, text.Text);

            // don't keep if it's less than 10 words
            if (CountWords(text.Text) < 10)
                File.Delete(SaveLocation);
            else
            {
                if (isSavingToTemp)
                {
                    var result = MessageBox.Show("Do you want to save your work first?", "Hey!", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        Save();
                    }
                    else
                    {
                        // delete temporary file
                        File.Delete(SaveLocation);
                    }
                }
            }
        }

        //-------------------------------------------------------------------
        //-------------------- autosave -------------------------------------

        System.Windows.Threading.DispatcherTimer dispatcherTimer;

        void CreateSaveTimer()
        {
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);  // every 2 seconds
            dispatcherTimer.IsEnabled = true;
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // save current text to last known location
            File.WriteAllText(SaveLocation, text.Text);

            // show ok
            ShowNoChangesPending();
        }

        

        #endregion

    }
}
