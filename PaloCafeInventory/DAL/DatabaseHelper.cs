using System.Data.SQLite;
using PaloCafeInventory.Models;

namespace PaloCafeInventory.DAL
{
    public class DatabaseHelper
    {
        private static readonly string connectionString = "Data Source=palo_cafe.db;Version=3;";
        
        public static void InitializeDatabase()
        {
            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            // Crear tabla Usuarios
            string createUsuarios = @"
                CREATE TABLE IF NOT EXISTS Usuarios (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    Rol INTEGER NOT NULL,
                    Activo INTEGER DEFAULT 1,
                    FechaCreacion TEXT DEFAULT CURRENT_TIMESTAMP
                );";

            // Crear tabla Productos
            string createProductos = @"
                CREATE TABLE IF NOT EXISTS Productos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Codigo TEXT NOT NULL UNIQUE,
                    Nombre TEXT NOT NULL,
                    Categoria TEXT,
                    Precio DECIMAL(10,2) NOT NULL,
                    StockActual INTEGER DEFAULT 0,
                    Activo INTEGER DEFAULT 1,
                    FechaCreacion TEXT DEFAULT CURRENT_TIMESTAMP
                );";

            // Crear tabla Movimientos
            string createMovimientos = @"
                CREATE TABLE IF NOT EXISTS Movimientos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Fecha TEXT DEFAULT CURRENT_TIMESTAMP,
                    Tipo INTEGER NOT NULL,
                    ProductoId INTEGER NOT NULL,
                    Cantidad INTEGER NOT NULL,
                    PrecioUnitario DECIMAL(10,2) NOT NULL,
                    Total DECIMAL(10,2) NOT NULL,
                    UsuarioId INTEGER NOT NULL,
                    TurnoId INTEGER,
                    Observaciones TEXT,
                    FOREIGN KEY (ProductoId) REFERENCES Productos(Id),
                    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
                    FOREIGN KEY (TurnoId) REFERENCES Turnos(Id)
                );";

            // Crear tabla Turnos
            string createTurnos = @"
                CREATE TABLE IF NOT EXISTS Turnos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UsuarioId INTEGER NOT NULL,
                    HoraInicio TEXT DEFAULT CURRENT_TIMESTAMP,
                    HoraCierre TEXT,
                    TotalVendido DECIMAL(10,2) DEFAULT 0,
                    Cerrado INTEGER DEFAULT 0,
                    NumeroTurno INTEGER NOT NULL,
                    Fecha TEXT NOT NULL,
                    Observaciones TEXT,
                    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
                );";

            // Crear tabla CierresDiarios
            string createCierresDiarios = @"
                CREATE TABLE IF NOT EXISTS CierresDiarios (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Fecha TEXT NOT NULL,
                    TotalGeneral DECIMAL(10,2) NOT NULL,
                    GeneradoPor INTEGER NOT NULL,
                    FechaGeneracion TEXT DEFAULT CURRENT_TIMESTAMP,
                    Resumen TEXT,
                    CantidadTurnos INTEGER DEFAULT 0,
                    FOREIGN KEY (GeneradoPor) REFERENCES Usuarios(Id)
                );";

            ExecuteCommand(connection, createUsuarios);
            ExecuteCommand(connection, createProductos);
            ExecuteCommand(connection, createMovimientos);
            ExecuteCommand(connection, createTurnos);
            ExecuteCommand(connection, createCierresDiarios);

            // Crear índices para mejor rendimiento
            ExecuteCommand(connection, "CREATE INDEX IF NOT EXISTS idx_movimientos_fecha ON Movimientos(Fecha);");
            ExecuteCommand(connection, "CREATE INDEX IF NOT EXISTS idx_movimientos_turno ON Movimientos(TurnoId);");
            ExecuteCommand(connection, "CREATE INDEX IF NOT EXISTS idx_turnos_fecha ON Turnos(Fecha);");
            ExecuteCommand(connection, "CREATE INDEX IF NOT EXISTS idx_productos_nombre ON Productos(Nombre);");

            CreateDefaultData(connection);
        }

        private static void ExecuteCommand(SQLiteConnection connection, string commandText)
        {
            using var command = new SQLiteCommand(commandText, connection);
            command.ExecuteNonQuery();
        }

        private static void CreateDefaultData(SQLiteConnection connection)
        {
            // Verificar si ya existen usuarios
            var checkUserCmd = new SQLiteCommand("SELECT COUNT(*) FROM Usuarios", connection);
            long userCount = (long)checkUserCmd.ExecuteScalar();

            if (userCount == 0)
            {
                // Crear usuario administrador por defecto
                var adminHash = BCrypt.Net.BCrypt.HashPassword("admin123");
                var insertAdmin = @"
                    INSERT INTO Usuarios (Nombre, PasswordHash, Rol) 
                    VALUES ('admin', @hash, 1)";
                
                using var cmd = new SQLiteCommand(insertAdmin, connection);
                cmd.Parameters.AddWithValue("@hash", adminHash);
                cmd.ExecuteNonQuery();

                // Crear productos de ejemplo
                var productos = new[]
                {
                    ("CAF001", "Café Americano", "Bebidas", 3500),
                    ("CAF002", "Café con Leche", "Bebidas", 4000),
                    ("CAF003", "Cappuccino", "Bebidas", 4500),
                    ("CAF004", "Espresso", "Bebidas", 3000),
                    ("TIN001", "Tinto", "Bebidas", 2000),
                    ("PAN001", "Croissant", "Panadería", 2500),
                    ("PAN002", "Pan Tostado", "Panadería", 3000),
                    ("DUL001", "Torta de Chocolate", "Dulces", 4000)
                };

                foreach (var (codigo, nombre, categoria, precio) in productos)
                {
                    var insertProducto = @"
                        INSERT INTO Productos (Codigo, Nombre, Categoria, Precio, StockActual) 
                        VALUES (@codigo, @nombre, @categoria, @precio, 100)";
                    
                    using var prodCmd = new SQLiteCommand(insertProducto, connection);
                    prodCmd.Parameters.AddWithValue("@codigo", codigo);
                    prodCmd.Parameters.AddWithValue("@nombre", nombre);
                    prodCmd.Parameters.AddWithValue("@categoria", categoria);
                    prodCmd.Parameters.AddWithValue("@precio", precio);
                    prodCmd.ExecuteNonQuery();
                }
            }
        }

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connectionString);
        }
    }
}