namespace ExcellentTaste.WinForms;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.ThreadException += delegate (object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("Er is een onverwachte fout opgetreden.\r\n\r\nNeem contact op met de administrator.", "Fout", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };
        AppDomain.CurrentDomain.UnhandledException += delegate
        {
            MessageBox.Show("Er is een onverwachte fout opgetreden.\r\n\r\nNeem contact op met de administrator.", "Fout", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };
        Application.Run(new Form1());
    }
}
