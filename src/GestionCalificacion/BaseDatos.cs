using Microsoft.Data.Sqlite;

namespace GestionCalificacion;

/// <summary>
/// Encapsula toda la interacción con SQLite: conexión, creación de tablas
/// y operaciones CRUD. Equivalente a la clase BaseDatos en la versión Python.
/// </summary>
public class BaseDatos : IDisposable
{
    private readonly SqliteConnection _conn;

    public BaseDatos(string ruta = "final.db")
    {
        _conn = new SqliteConnection($"Data Source={ruta}");
        _conn.Open();
    }

    /// <summary>Crea las 3 tablas relacionadas (alumnos, materias, notas) si no existen.</summary>
    public void CrearTablas()
    {
        Ejecutar(@"CREATE TABLE IF NOT EXISTS alumnos (
            id INTEGER PRIMARY KEY AUTOINCREMENT, nombre TEXT NOT NULL)");

        Ejecutar(@"CREATE TABLE IF NOT EXISTS materias (
            id INTEGER PRIMARY KEY AUTOINCREMENT, nombre TEXT NOT NULL)");

        Ejecutar(@"CREATE TABLE IF NOT EXISTS notas (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            alumno_id INTEGER, materia_id INTEGER, nota REAL,
            FOREIGN KEY (alumno_id) REFERENCES alumnos(id),
            FOREIGN KEY (materia_id) REFERENCES materias(id))");
    }

    private void Ejecutar(string sql)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static void ValidarTabla(string tabla)
    {
        if (tabla != "alumnos" && tabla != "materias")
            throw new ArgumentException($"Tabla inválida: {tabla}");
    }

    /// <summary>Inserta un alumno o una materia (según 'tabla') de forma parametrizada.</summary>
    public void Insertar(string tabla, string nombre)
    {
        ValidarTabla(tabla);
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = $"INSERT INTO {tabla} (nombre) VALUES ($nombre)";
        cmd.Parameters.AddWithValue("$nombre", nombre);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Inserta una fila en la tabla notas, de forma parametrizada.</summary>
    public void InsertarNota(int alumnoId, int materiaId, double nota)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "INSERT INTO notas (alumno_id, materia_id, nota) VALUES ($a, $m, $n)";
        cmd.Parameters.AddWithValue("$a", alumnoId);
        cmd.Parameters.AddWithValue("$m", materiaId);
        cmd.Parameters.AddWithValue("$n", nota);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Devuelve todas las filas (id, nombre) de 'alumnos' o 'materias'.</summary>
    public List<(int Id, string Nombre)> Obtener(string tabla)
    {
        ValidarTabla(tabla);
        var resultado = new List<(int, string)>();
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = $"SELECT id, nombre FROM {tabla} ORDER BY id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            resultado.Add((reader.GetInt32(0), reader.GetString(1)));
        return resultado;
    }

    /// <summary>Devuelve la nota de un alumno en una materia específica, o null si no existe.</summary>
    public double? ObtenerNota(int alumnoId, int materiaId)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT nota FROM notas WHERE alumno_id=$a AND materia_id=$m";
        cmd.Parameters.AddWithValue("$a", alumnoId);
        cmd.Parameters.AddWithValue("$m", materiaId);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? reader.GetDouble(0) : null;
    }

    /// <summary>Devuelve una lista de (alumno_id, promedio) agrupadas por alumno.</summary>
    public List<(int AlumnoId, double Promedio)> ObtenerPromedios()
    {
        var resultado = new List<(int, double)>();
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT alumno_id, AVG(nota) FROM notas GROUP BY alumno_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            resultado.Add((reader.GetInt32(0), reader.GetDouble(1)));
        return resultado;
    }

    public void Cerrar()
{
    SqliteConnection.ClearPool(_conn);
    _conn.Close();
}

public void Dispose()
{
    SqliteConnection.ClearPool(_conn);
    _conn.Dispose();
    GC.SuppressFinalize(this);
}
}
