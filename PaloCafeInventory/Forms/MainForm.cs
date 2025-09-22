using PaloCafeInventory.BLL;
using PaloCafeInventory.Forms;
using PaloCafeInventory.Models;
using PaloCafeInventory.Services;

namespace PaloCafeInventory.Forms
{
    public partial class MainForm : Form
    {
        private readonly TurnoService _turnoService;
        private Panel panelMenu;
        private Panel panelContent;
        private Label lblUsuario;
        private Label lblTurno;

        public MainForm()
        {
            InitializeComponent();
            _turnoService = new TurnoService();
            LoadUserInfo();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 700);
            this.Text = "Palo de Café - Sistema de Inventario";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 245, 245);

            CreateMenuPanel();
            CreateContentPanel();
            CreateStatusBar();
        }

        private void CreateMenuPanel()
        {
            panelMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.FromArgb(101, 67, 33)
            };

            // Logo/Título
            var lblLogo = new Label
            {
                Text = "PALO DE\nCAFÉ",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80
            };

            // Panel para botones
            var panelButtons = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(101, 67, 33)
            };

            var buttons = new List<(string text, string action, Color color)>
            {
                ("🏠 Inicio", "INICIO", Color.FromArgb(139, 91, 45)),
                ("🛒 Ventas", "VENTAS", Color.FromArgb(46, 125, 50)),
                ("📦 Inventario", "INVENTARIO", Color.FromArgb(30, 136, 229)),
                ("👥 Usuarios", "USUARIOS", Color.FromArgb(255, 152, 0)),
                ("🔄 Cierre Turno", "CIERRE_TURNO", Color.FromArgb(156, 39, 176)),
                ("📊 Reportes", "REPORTES", Color.FromArgb(244, 67, 54)),
                ("⚙️ Configuración", "CONFIGURACION", Color.FromArgb(96, 125, 139))
            };

            int yPos = 10;
            foreach (var (text, action, color) in buttons)
            {
                var btn = CreateMenuButton(text, action, color);
                btn.Location = new Point(10, yPos);
                panelButtons.Controls.Add(btn);
                yPos += 45;
            }

            // Botón cerrar sesión
            var btnLogout = new Button
            {
                Text = "🚪 Cerrar Sesión",
                Size = new Size(180, 35),
                Location = new Point(10, panelButtons.Height - 50),
                BackColor = Color.FromArgb(183, 28, 28),
                ForeColor = Color.White,
                Font = new Font("Arial", 10),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => 
            {
                SessionManager.CerrarSesion();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            panelButtons.Controls.Add(btnLogout);
            panelMenu.Controls.AddRange(new Control[] { lblLogo, panelButtons });
            this.Controls.Add(panelMenu);
        }

        private Button CreateMenuButton(string text, string action, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(180, 35),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Arial", 10),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Tag = action
            };
            btn.FlatAppearance.BorderSize = 0;
            
            btn.Click += MenuButton_Click;
            
            // Verificar permisos
            if (!SessionManager.TienePermiso(action) && action != "INICIO")
            {
                btn.Enabled = false;
                btn.BackColor = Color.Gray;
            }

            return btn;
        }

        private void CreateContentPanel()
        {
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            // Panel de bienvenida por defecto
            ShowWelcomePanel();
            
            this.Controls.Add(panelContent);
        }

        private void CreateStatusBar()
        {
            var statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(224, 224, 224)
            };

            lblUsuario = new Label
            {
                Dock = DockStyle.Left,
                Width = 200,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Arial", 9),
                Padding = new Padding(10, 0, 0, 0)
            };

            lblTurno = new Label
            {
                Dock = DockStyle.Right,
                Width = 300,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Arial", 9),
                Padding = new Padding(0, 0, 10, 0)
            };

            statusBar.Controls.AddRange(new Control[] { lblUsuario, lblTurno });
            this.Controls.Add(statusBar);
        }

        private async void LoadUserInfo()
        {
            var usuario = SessionManager.UsuarioActual;
            if (usuario != null)
            {
                lblUsuario.Text = $"Usuario: {usuario.Nombre} ({usuario.Rol})";
                
                // Cargar información del turno
                try
                {
                    var turnoActivo = await _turnoService.ObtenerTurnoActivo(usuario.Id);
                    if (turnoActivo != null)
                    {
                        SessionManager.TurnoActivo = turnoActivo;
                        lblTurno.Text = $"Turno #{turnoActivo.NumeroTurno} - Inicio: {turnoActivo.HoraInicio:HH:mm}";
                    }
                    else
                    {
                        lblTurno.Text = "Sin turno activo";
                    }
                }
                catch
                {
                    lblTurno.Text = "Error al cargar turno";
                }
            }
        }

        private async void MenuButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string action)
            {
                // Limpiar panel de contenido
                panelContent.Controls.Clear();

                switch (action)
                {
                    case "INICIO":
                        ShowWelcomePanel();
                        break;
                    case "VENTAS":
                        await ShowVentasPanel();
                        break;
                    case "INVENTARIO":
                        ShowInventarioPanel();
                        break;
                    case "USUARIOS":
                        ShowUsuariosPanel();
                        break;
                    case "CIERRE_TURNO":
                        await ShowCierreTurnoPanel();
                        break;
                    case "REPORTES":
                        ShowReportesPanel();
                        break;
                    case "CONFIGURACION":
                        ShowConfiguracionPanel();
                        break;
                }
            }
        }

        private void ShowWelcomePanel()
        {
            var welcomePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var lblBienvenida = new Label
            {
                Text = $"¡Bienvenido, {SessionManager.UsuarioActual?.Nombre}!",
                Font = new Font("Arial", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(101, 67, 33),
                Size = new Size(400, 40),
                Location = new Point(50, 50),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblInfo = new Label
            {
                Text = $"Rol: {SessionManager.UsuarioActual?.Rol}\nFecha: {DateTime.Now:dd/MM/yyyy}\nHora: {DateTime.Now:HH:mm:ss}",
                Font = new Font("Arial", 12),
                Size = new Size(300, 80),
                Location = new Point(50, 100)
            };

            // Panel de estadísticas rápidas
            var statsPanel = new Panel
            {
                Size = new Size(600, 200),
                Location = new Point(50, 200),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 248, 248)
            };

            var lblStats = new Label
            {
                Text = "Resumen del Día",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(200, 30)
            };

            statsPanel.Controls.Add(lblStats);
            welcomePanel.Controls.AddRange(new Control[] { lblBienvenida, lblInfo, statsPanel });
            panelContent.Controls.Add(welcomePanel);
        }

        private async Task ShowVentasPanel()
        {
            var ventasForm = new VentasForm();
            ventasForm.TopLevel = false;
            ventasForm.FormBorderStyle = FormBorderStyle.None;
            ventasForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(ventasForm);
            ventasForm.Show();
        }

        private void ShowInventarioPanel()
        {
            var inventarioForm = new InventarioForm();
            inventarioForm.TopLevel = false;
            inventarioForm.FormBorderStyle = FormBorderStyle.None;
            inventarioForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(inventarioForm);
            inventarioForm.Show();
        }

        private void ShowUsuariosPanel()
        {
            var usuariosForm = new UsuariosForm();
            usuariosForm.TopLevel = false;
            usuariosForm.FormBorderStyle = FormBorderStyle.None;
            usuariosForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(usuariosForm);
            usuariosForm.Show();
        }

        private async Task ShowCierreTurnoPanel()
        {
            var cierreForm = new CierreTurnoForm();
            cierreForm.TopLevel = false;
            cierreForm.FormBorderStyle = FormBorderStyle.None;
            cierreForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(cierreForm);
            cierreForm.Show();
        }

        private void ShowReportesPanel()
        {
            var reportesForm = new ReportesForm();
            reportesForm.TopLevel = false;
            reportesForm.FormBorderStyle = FormBorderStyle.None;
            reportesForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(reportesForm);
            reportesForm.Show();
        }

        private void ShowConfiguracionPanel()
        {
            var lblConfig = new Label
            {
                Text = "Panel de Configuración\n(En desarrollo)",
                Font = new Font("Arial", 14),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = Color.Gray
            };
            
            panelContent.Controls.Add(lblConfig);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            var result = MessageBox.Show("¿Está seguro que desea salir del sistema?", "Confirmar Salida", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            SessionManager.CerrarSesion();
            base.OnFormClosing(e);
        }
    }
}