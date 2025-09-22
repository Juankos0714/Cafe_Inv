using PaloCafeInventory.BLL;
using PaloCafeInventory.Services;

namespace PaloCafeInventory.Forms
{
    public partial class LoginForm : Form
    {
        private readonly UsuarioService _usuarioService;

        public LoginForm()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
            
            // Configurar el formulario
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 245, 245);
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 300);
            this.Text = "Palo de Café - Login";
            
            // Panel principal
            var panelMain = new Panel
            {
                Size = new Size(350, 220),
                Location = new Point(25, 40),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Título
            var lblTitulo = new Label
            {
                Text = "PALO DE CAFÉ",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(101, 67, 33),
                Size = new Size(300, 30),
                Location = new Point(25, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblSubtitulo = new Label
            {
                Text = "Sistema de Inventario",
                Font = new Font("Arial", 10),
                ForeColor = Color.FromArgb(101, 67, 33),
                Size = new Size(300, 20),
                Location = new Point(25, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Usuario
            var lblUsuario = new Label
            {
                Text = "Usuario:",
                Font = new Font("Arial", 10),
                Size = new Size(80, 20),
                Location = new Point(20, 90)
            };

            var txtUsuario = new TextBox
            {
                Name = "txtUsuario",
                Size = new Size(200, 25),
                Location = new Point(100, 88),
                Font = new Font("Arial", 10)
            };

            // Contraseña
            var lblPassword = new Label
            {
                Text = "Contraseña:",
                Font = new Font("Arial", 10),
                Size = new Size(80, 20),
                Location = new Point(20, 125)
            };

            var txtPassword = new TextBox
            {
                Name = "txtPassword",
                Size = new Size(200, 25),
                Location = new Point(100, 123),
                Font = new Font("Arial", 10),
                PasswordChar = '*'
            };

            // Botones
            var btnLogin = new Button
            {
                Text = "Iniciar Sesión",
                Size = new Size(120, 30),
                Location = new Point(100, 165),
                BackColor = Color.FromArgb(101, 67, 33),
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.FlatAppearance.BorderSize = 0;

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Size = new Size(80, 30),
                Location = new Point(235, 165),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Arial", 10),
                FlatStyle = FlatStyle.Flat
            };
            btnCancelar.FlatAppearance.BorderSize = 0;

            // Eventos
            btnLogin.Click += async (s, e) => await BtnLogin_Click(txtUsuario.Text, txtPassword.Text);
            btnCancelar.Click += (s, e) => this.Close();
            txtPassword.KeyPress += (s, e) => 
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    btnLogin.PerformClick();
                }
            };

            // Agregar controles
            panelMain.Controls.AddRange(new Control[] 
            {
                lblTitulo, lblSubtitulo, lblUsuario, txtUsuario, 
                lblPassword, txtPassword, btnLogin, btnCancelar
            });

            this.Controls.Add(panelMain);

            // Focus inicial
            this.Shown += (s, e) => txtUsuario.Focus();
        }

        private async Task BtnLogin_Click(string usuario, string password)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Ingrese usuario y contraseña", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Deshabilitar botón mientras valida
                var btnLogin = this.Controls.OfType<Panel>().First().Controls.OfType<Button>()
                    .First(b => b.Text == "Iniciar Sesión");
                btnLogin.Enabled = false;
                btnLogin.Text = "Validando...";

                var usuarioValidado = await _usuarioService.ValidarLogin(usuario, password);

                if (usuarioValidado != null)
                {
                    SessionManager.UsuarioActual = usuarioValidado;
                    
                    // Mostrar ventana principal
                    var mainForm = new MainForm();
                    this.Hide();
                    
                    var result = mainForm.ShowDialog();
                    
                    if (result == DialogResult.OK || result == DialogResult.Cancel)
                    {
                        this.Close();
                    }
                    else
                    {
                        this.Show();
                        SessionManager.CerrarSesion();
                    }
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos", "Error de Autenticación", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar sesión: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                var btnLogin = this.Controls.OfType<Panel>().First().Controls.OfType<Button>()
                    .First(b => b.Text == "Validando...");
                btnLogin.Enabled = true;
                btnLogin.Text = "Iniciar Sesión";
            }
        }
    }
}