using PaloCafeInventory.DAL;
using PaloCafeInventory.Forms;

namespace PaloCafeInventory
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Configuración de la aplicación
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Inicializar base de datos
                DatabaseHelper.InitializeDatabase();

                // Mostrar formulario de login
                var loginForm = new LoginForm();
                Application.Run(loginForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar la aplicación:\n{ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}