namespace GestionCalificacion;

/// <summary>
/// Excepción personalizada que representa una regla de negocio propia:
/// ninguna nota puede ser menor a 0 ni mayor a 10.
/// Equivalente a NotaInvalidaError en la versión Python.
/// </summary>
public class NotaInvalidaException : Exception
{
    public NotaInvalidaException(string mensaje) : base(mensaje) { }
}

/// <summary>Representa un alumno en memoria (id + nombre).</summary>
public class Estudiante
{
    public int Id { get; }
    public string Nombre { get; }

    public Estudiante(int id, string nombre)
    {
        Id = id;
        Nombre = nombre;
    }
}

/// <summary>Representa una materia en memoria (id + nombre).</summary>
public class Materia
{
    public int Id { get; }
    public string Nombre { get; }

    public Materia(int id, string nombre)
    {
        Id = id;
        Nombre = nombre;
    }
}
