using GestionCalificacion;
using NUnit.Framework;

namespace GestionCalificacion.Tests;

/// <summary>
/// Cada prueba usa un archivo SQLite temporal distinto para no interferir
/// entre sí (equivalente a una base de datos "limpia" por prueba).
/// </summary>
public class BaseDatosTests
{
    private string _rutaDb = string.Empty;
    private BaseDatos _bd = null!;

    [SetUp]
    public void Setup()
    {
        _rutaDb = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.db");
        _bd = new BaseDatos(_rutaDb);
        _bd.CrearTablas();
    }

    [TearDown]
    public void TearDown()
    {
        _bd.Cerrar();
        _bd.Dispose();
        if (File.Exists(_rutaDb)) File.Delete(_rutaDb);
    }

    [Test]
    public void CrearTablas_NoLanzaExcepcion_SiSeLlamaDosVeces()
    {
        Assert.DoesNotThrow(() => _bd.CrearTablas());
    }

    [Test]
    public void Insertar_Alumno_QuedaDisponibleEnObtener()
    {
        _bd.Insertar("alumnos", "Ana Pérez");

        var alumnos = _bd.Obtener("alumnos");

        Assert.That(alumnos, Has.Count.EqualTo(1));
        Assert.That(alumnos[0].Nombre, Is.EqualTo("Ana Pérez"));
    }

    [Test]
    public void Insertar_Materia_QuedaDisponibleEnObtener()
    {
        _bd.Insertar("materias", "Programación III");

        var materias = _bd.Obtener("materias");

        Assert.That(materias, Has.Count.EqualTo(1));
        Assert.That(materias[0].Nombre, Is.EqualTo("Programación III"));
    }

    [Test]
    public void Insertar_TablaInvalida_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _bd.Insertar("otraCosa", "x"));
    }

    [Test]
    public void InsertarNota_YObtenerNota_DevuelveElValorCorrecto()
    {
        _bd.Insertar("alumnos", "Juan");
        _bd.Insertar("materias", "Matemática");
        var alumnoId = _bd.Obtener("alumnos")[0].Id;
        var materiaId = _bd.Obtener("materias")[0].Id;

        _bd.InsertarNota(alumnoId, materiaId, 8.5);

        var nota = _bd.ObtenerNota(alumnoId, materiaId);
        Assert.That(nota, Is.EqualTo(8.5));
    }

    [Test]
    public void ObtenerNota_SinRegistrar_DevuelveNull()
    {
        var nota = _bd.ObtenerNota(999, 999);
        Assert.That(nota, Is.Null);
    }

    [Test]
    public void ObtenerPromedios_CalculaElPromedioPorAlumno()
    {
        _bd.Insertar("alumnos", "Luis");
        _bd.Insertar("materias", "Materia A");
        _bd.Insertar("materias", "Materia B");
        var alumnoId = _bd.Obtener("alumnos")[0].Id;
        var materias = _bd.Obtener("materias");

        _bd.InsertarNota(alumnoId, materias[0].Id, 8.0);
        _bd.InsertarNota(alumnoId, materias[1].Id, 6.0);

        var promedios = _bd.ObtenerPromedios();

        Assert.That(promedios, Has.Count.EqualTo(1));
        Assert.That(promedios[0].AlumnoId, Is.EqualTo(alumnoId));
        Assert.That(promedios[0].Promedio, Is.EqualTo(7.0));
    }
}

/// <summary>
/// Pruebas de la regla de negocio "una nota debe estar entre 0 y 10",
/// sin dependencias de consola ni de base de datos.
/// </summary>
public class ValidacionNotaTests
{
    [TestCase(0)]
    [TestCase(5.5)]
    [TestCase(10)]
    public void ValidarNota_ConValoresDentroDelRango_NoLanzaExcepcion(double valor)
    {
        Assert.DoesNotThrow(() => Program.ValidarNota(valor));
    }

    [TestCase(-0.01)]
    [TestCase(-5)]
    [TestCase(10.01)]
    [TestCase(100)]
    public void ValidarNota_ConValoresFueraDeRango_LanzaNotaInvalidaException(double valor)
    {
        Assert.Throws<NotaInvalidaException>(() => Program.ValidarNota(valor));
    }
}
