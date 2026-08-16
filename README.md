# Sistema de Gestión de Calificaciones

Aplicación de consola en C# (.NET 8) para gestionar alumnos, materias y notas, usando SQLite como base de datos.

## Funcionalidades

- Registrar alumnos y materias
- Asignar notas (0–10) con validación de rango
- Buscar alumno por nombre
- Detectar alumnos con nombres duplicados
- Reporte de promedios por alumno (con listado de aprobados)
- Reporte cruzado alumno X materia

## Tecnologías

- C# / .NET 8
- SQLite (`Microsoft.Data.Sqlite`)
- NUnit (pruebas automatizadas)

## Estructura del proyecto

```
├── src/
│   └── GestionCalificacion/       # Proyecto principal
│       ├── Modelos.cs
│       ├── BaseDatos.cs
│       └── Program.cs
└── tests/
    └── GestionCalificacion.Tests/ # Pruebas unitarias (NUnit)
        └── GestionCalificacionTests.cs
```

## Cómo ejecutar el programa

```bash
dotnet run --project src/GestionCalificacion/GestionCalificacion.csproj
```

## Cómo ejecutar las pruebas

```bash
dotnet test tests/GestionCalificacion.Tests/GestionCalificacion.Tests.csproj
```

## Metodología

Proyecto desarrollado bajo Scrum. Épicas, historias de usuario y sprint gestionados en Jira (ver documento del proyecto para el link).
