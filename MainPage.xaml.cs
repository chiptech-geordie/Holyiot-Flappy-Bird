namespace ChristmasCodingChallange
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void PlayButtonEvent(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ConnectionPage());
        }
    }
}
