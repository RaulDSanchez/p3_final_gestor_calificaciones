using Microsoft.Data.Sqlite;

namespace GestionCalificacion;

public static class Program
{
    // Tupla que representa el periodo académico actual.
    private static readonly (string Anio, string Cuatrimestre) Periodo = ("2026", "Cuatrimestre 2");

    /// Regla de negocio pura, sin I/O: valida que una nota esté en [0, 10].
    /// Separada de PedirNota() para poder probarla con pruebas unitarias
    /// sin depender de Console.ReadLine().
    public static void ValidarNota(double valor)
    {
        if (valor < 0 || valor > 10)
            throw new NotaInvalidaException("La nota debe estar entre 0 y 10.");
    }

    /// <summary>Pide una nota por consola y la valida en un bucle hasta que sea correcta.</summary>
    public static double PedirNota()
    {
        while (true)
        {
            Console.Write("Nota (0-10): ");
            var entrada = Console.ReadLine();

            if (!double.TryParse(entrada, out double valor))
            {
                Console.WriteLine("Debes ingresar un número.");
                continue;
            }

            try
            {
                ValidarNota(valor);
                return valor;
            }
            catch (NotaInvalidaException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    /// <summary>Registra un alumno o materia pidiendo el nombre por consola.</summary>
    public static void Registrar(BaseDatos bd, string tabla, string etiqueta)
    {
        try
        {
            Console.Write($"Nombre de {etiqueta}: ");
            var nombre = (Console.ReadLine() ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.");

            bd.Insertar(tabla, nombre);
            Console.WriteLine($"{char.ToUpper(etiqueta[0])}{etiqueta[1..]} registrado.");
        }
        catch (ArgumentException e)
        {
            Console.WriteLine($"Entrada inválida: {e.Message}");
        }
        catch (SqliteException e)
        {
            Console.WriteLine($"Error de base de datos: {e.Message}");
        }
        finally
        {
            Console.WriteLine("Operación finalizada.");
        }
    }

    /// <summary>Registra una nota asociada a un alumno y una materia existentes.</summary>
    public static void RegistrarNota(BaseDatos bd)
    {
        try
        {
            var alumnos = bd.Obtener("alumnos");
            var materias = bd.Obtener("materias");

            if (alumnos.Count == 0 || materias.Count == 0)
            {
                Console.WriteLine("Debes registrar al menos un alumno y una materia primero.");
                return;
            }

            Console.WriteLine("Alumnos: " + string.Join(", ", alumnos.Select(a => $"{a.Id}={a.Nombre}")));
            Console.Write("ID de alumno: ");
            int alumnoId = int.Parse(Console.ReadLine() ?? "0");

            Console.WriteLine("Materias: " + string.Join(", ", materias.Select(m => $"{m.Id}={m.Nombre}")));
            Console.Write("ID de materia: ");
            int materiaId = int.Parse(Console.ReadLine() ?? "0");

            double nota = PedirNota();
            bd.InsertarNota(alumnoId, materiaId, nota);
        }
        catch (FormatException)
        {
            Console.WriteLine("El ID debe ser un número entero.");
        }
        catch (SqliteException e)
        {
            Console.WriteLine($"Error de base de datos: {e.Message}");
        }
        finally
        {
            Console.WriteLine("Nota registrada.");
            Console.WriteLine("Operación finalizada.");
        }
    }

    /// <summary>Busca un alumno por nombre recorriendo la lista.</summary>
    public static void BuscarAlumno(BaseDatos bd)
    {
        Console.Write("Nombre a buscar: ");
        var objetivo = (Console.ReadLine() ?? string.Empty).Trim().ToLower();

        foreach (var (id, nombre) in bd.Obtener("alumnos"))
        {
            if (string.IsNullOrEmpty(nombre)) continue;

            if (nombre.ToLower() == objetivo)
            {
                Console.WriteLine($"Encontrado: id={id}, nombre={nombre}");
                return;
            }
        }

        Console.WriteLine("No encontrado.");
    }

    /// <summary>Detecta nombres de alumnos repetidos usando un HashSet.</summary>
    public static void AlumnosDuplicados(BaseDatos bd)
    {
        var vistos = new HashSet<string>();
        var duplicados = new HashSet<string>();

        foreach (var (_, nombre) in bd.Obtener("alumnos"))
        {
            var clave = nombre.ToLower();
            if (!vistos.Add(clave))
                duplicados.Add(nombre);
        }

        Console.WriteLine("Duplicados: " + (duplicados.Count > 0 ? string.Join(", ", duplicados) : "ninguno"));
    }

    /// <summary>Genera un reporte numerado de promedios por alumno.</summary>
    public static void ReportePromedios(BaseDatos bd)
    {
        var alumnos = bd.Obtener("alumnos");
        var promedios = bd.ObtenerPromedios().ToDictionary(p => p.AlumnoId, p => p.Promedio);

        var nombres = alumnos.Select(a => a.Nombre).ToList();
        var valores = alumnos.Select(a => promedios.GetValueOrDefault(a.Id, 0.0)).ToList();

        Console.WriteLine($"\n=== Reporte {Periodo.Cuatrimestre} {Periodo.Anio} ===");

        for (int i = 0; i < nombres.Count; i++)
            Console.WriteLine($"{i + 1}. {nombres[i]} - Promedio: {valores[i]:F2}");

        var aprobados = nombres.Where((_, i) => valores[i] >= 6).ToList();
        Console.WriteLine("Aprobados: " + string.Join(", ", aprobados));
    }

    /// <summary>Genera un reporte cruzado alumnos x materias.</summary>
    public static void ReporteCruzado(BaseDatos bd)
    {
        var alumnos = bd.Obtener("alumnos");
        var materias = bd.Obtener("materias");

        if (alumnos.Count == 0 || materias.Count == 0)
        {
            Console.WriteLine("Faltan alumnos o materias.");
            return;
        }

        var conteo = new Dictionary<string, int>();

        foreach (var (aId, aNombre) in alumnos)
        {
            var fila = $"{aNombre}: ";

            foreach (var (mId, mNombre) in materias)
            {
                var nota = bd.ObtenerNota(aId, mId);
                if (nota is not null)
                {
                    conteo[aNombre] = conteo.GetValueOrDefault(aNombre, 0) + 1;
                    fila += $"{mNombre}={nota:F1} ";
                }
                else
                {
                    fila += $"{mNombre}=- ";
                }
            }

            Console.WriteLine(fila);
        }

        Console.WriteLine("Notas cargadas por alumno: " +
            string.Join(", ", conteo.Select(kv => $"{kv.Key}={kv.Value}")));
    }

    /// <summary>Bucle principal del programa: muestra el menú y despacha cada opción.</summary>
    public static void Menu()
    {
        using var bd = new BaseDatos();

        try
        {
            bd.CrearTablas();

            while (true)
            {
                Console.WriteLine("\n=== MENÚ PRINCIPAL ===");
                Console.WriteLine("1. Registrar Alumno");
                Console.WriteLine("2. Registrar Materia");
                Console.WriteLine("3. Asignar Nota");
                Console.WriteLine("4. Buscar Alumno");
                Console.WriteLine("5. Ver Alumnos Duplicados");
                Console.WriteLine("6. Ver Reporte de Promedios");
                Console.WriteLine("7. Ver Reporte Cruzado");
                Console.WriteLine("0. Salir");

                var op = (Console.ReadLine() ?? string.Empty).Trim();

                switch (op)
                {
                    case "0":
                        return;
                    case "1":
                        Registrar(bd, "alumnos", "alumno");
                        break;
                    case "2":
                        Registrar(bd, "materias", "materia");
                        break;
                    case "3":
                        RegistrarNota(bd);
                        break;
                    case "4":
                        BuscarAlumno(bd);
                        break;
                    case "5":
                        AlumnosDuplicados(bd);
                        break;
                    case "6":
                        ReportePromedios(bd);
                        break;
                    case "7":
                        ReporteCruzado(bd);
                        break;
                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error inesperado: {e.Message}");
        }
        finally
        {
            bd.Cerrar();
            Console.WriteLine("Conexión cerrada.");
        }
    }

    public static void Main() => Menu();
}
