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
using System.IO;
using System.Reflection;
using System.Windows.Markup;
using System.Collections;

namespace Z1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<WritingSpace> openWindows = new List<WritingSpace>();
        WritingSpace lastOpenWindow = null;

        public MainWindow()
        {
            InitializeComponent();
            var path = @"C:\Users\SparrowHawk\Documents\GitHub\ZenWriter\Z1\Themes\";
            var myThemes =
                from dir in Directory.GetDirectories(path)
                select new ThemeData()
                {
                    ThemeName = new FileInfo(dir).Name,
                    ThemePath = dir
                };

            foreach (var td in myThemes)
            {
                themes.Items.Add(td);
            }
        }

        void New()
        {
            WritingSpace w = new WritingSpace();

            // set settings
            w.CHANGE_OPACITY = this.CHANGE_OPACITY;
            w.FONTSIZE = this.FONTSIZE;

            // add to collection of windows
            w.Owner = this;
            openWindows.Add(w);
            w.Activated += (o, z) =>
            {
                lastOpenWindow = o as WritingSpace;
            };

            // show the window
            w.Show();
        }
        private void New_Click(object sender, RoutedEventArgs e)
        {
            New();
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            ToggleOptions();
        }

        void ToggleOptions()
        {
            if (this.options.IsVisible)
                this.options.Visibility = Visibility.Hidden;
            else
                this.options.Visibility = Visibility.Visible;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        #region typing options

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var x = this.font.SelectedIndex;
            if (x == 0)
                this.FONTSTYLE = "serif";
            else
                this.FONTSTYLE = "sans serif";

        }

        private void fontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // make value an integer
            fontSize.Value = 2 * Math.Round(fontSize.Value / 2);

            this.FONTSIZE = (int)fontSize.Value;
        }

        private void opacity_CheckChanged(object sender, RoutedEventArgs e)
        {
            this.CHANGE_OPACITY = (bool)opacity.IsChecked;
        }

        string _fontstyle = "sans serif";
        public string FONTSTYLE
        {
            get { return this._fontstyle; }
            set
            {
                this._fontstyle = value;
                foreach (WritingSpace w in this.openWindows)
                    w.FONTSTYLE = value;
            }
        }
        int _fontsize = 14;
        public int FONTSIZE
        {
            get { return this._fontsize; }
            set
            {
                this._fontsize = value;
                foreach (WritingSpace w in this.openWindows)
                    w.FONTSIZE = value;
            }
        }

        bool _opacity = true;
        public bool CHANGE_OPACITY
        {
            get { return this._opacity; }
            set
            {
                this._opacity = value;
                foreach (WritingSpace w in this.openWindows)
                    w.CHANGE_OPACITY = value;
            }
        }

        #endregion

        private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (themes.SelectedIndex == -1) return;

            var files = Directory.GetFiles(((ThemeData)themes.SelectedValue).ThemePath).Where(x => x.EndsWith(".xaml"));

            foreach (var command in files)
            {
                var stream = new FileStream(command, FileMode.Open);

                foreach (DictionaryEntry dictionaryEntry in (ResourceDictionary)XamlReader.Load(stream))
                {
                        Application.Current.Resources[dictionaryEntry.Key] = dictionaryEntry.Value;
                   
                }

            }
        }

    }
}
