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

namespace TucLoggbok
{
    /// <summary>
    /// Interaction logic for AdminProfilePage.xaml
    /// </summary>
    public partial class AdminProfilePage : Window
    {
        public AdminProfilePage()
        {
            InitializeComponent();
        }

        private void ProfileIcon_Click(object sender, RoutedEventArgs e)
        {
            // Den här metoden hanterar klick på profilikonen (visas som kugghjul på loginpage)
            // sen uppdateras bilden och öppnar inloggningssidan igen, '?' är till för att kontrollera om det är null.
            Button? clickedButton = sender as Button;
            Image? selectedImage = (clickedButton?.Content as Border)?.Child as Image;

            string? imageSource = selectedImage?.Source?.ToString();
            if (string.IsNullOrEmpty(imageSource))
            {
                return;
            }

            LoginPage loginPage = new LoginPage();
            loginPage.UpdateProfileImage(imageSource, isAdmin: true);

            loginPage.Show();

            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Denna kod gör så att man kan flytta på fönstret.
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove(); 
            }
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            // Stänger fönstret.
            this.Close();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigerar till LoginPage (startsidan)
            LoginPage loginPage = new LoginPage();
            loginPage.Show();
            this.Close();
        }

    }
}
