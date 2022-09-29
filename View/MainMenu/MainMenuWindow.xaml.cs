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
using System.Windows.Shapes;
using ViewModel;
using ViewModel.MainMenu;

namespace View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainMenuWindow : Window
    {
        private MainMenuViewModel VM => DataContext as MainMenuViewModel ?? throw new Exception("Data contrext is not of type MainMenuViewModel");

        public MainMenuWindow()
        {
            InitializeComponent();
        }

        private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var newParkWindow = new NewParkWindow()
            {
                Owner = this
            };
            
            var result = newParkWindow.ShowDialog();

            if(result == true)
            {
                var parkName = newParkWindow.ParkName;
                VM.NewGameCommand.Execute(parkName.Text) ;
            }
        }

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            VM.OpenGameCommand.Execute(null);
        }

        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            VM.ExitCommand.Execute(null);
        }
    }
}
