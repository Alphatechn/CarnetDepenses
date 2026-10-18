namespace CarnetDepenses
{
    public partial class AppShell : Shell
    {
        public AppShell(MainPage mainPage)
        {
            InitializeComponent();
            Items.Add(new ShellContent
            {
                Title = "Accueil",
                Route = "MainPage",
                Content = mainPage
            });
        }
    }
}
