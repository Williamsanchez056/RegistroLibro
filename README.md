# Registro de Libros

**Nombre:** Williams Rafael Sanchez Acosta  
**Matricula:** 2023-0136  
**Asignatura:** Programacion Aplicada 1  
**Universidad:** Universidad Catolica Nordestana (UCNE)

## Sobre el proyecto

Aplicacion web desarrollada con C# y Blazor para administrar libros, estudiantes, prestamos y devoluciones. El proyecto utiliza Entity Framework Core para trabajar con SQL Server.

## Funciones principales

- Registrar, consultar, editar y eliminar libros.
- Registrar y consultar estudiantes.
- Crear prestamos y consultar su estado.
- Registrar la devolucion de un prestamo sin borrar el historial del prestamo.
- Consultar el promedio de dias que los libros devueltos estuvieron prestados.
- Buscar registros desde las pantallas de consulta.

La propiedad `CantidadDisponible` forma parte del modelo de libro. Su valor se registra, pero el flujo de prestamos y devoluciones todavia no lo modifica automaticamente.

## Manejo de resultados

Los servicios de libros, estudiantes, prestamos y devoluciones implementan `IServiceResult<T, int>` de `Aplicada1.Core`. Las operaciones CRUD devuelven objetos `Result` o `Result<T>`, lo que permite consultar `IsSuccess`, `IsFailure` y `Error.Description` para manejar errores de validacion y de acceso a datos.

## Tecnologias

- C#
- .NET 10
- Blazor con renderizado interactivo en servidor
- Entity Framework Core 10
- SQL Server
- Aplicada1.Core 1.1.0
- Bootstrap y QuickGrid

## Ejecutar el proyecto

1. Abre la solucion `RegistroLibro.slnx` con Visual Studio o abre una terminal en la carpeta del proyecto.
2. Verifica que SQL Server LocalDB este instalado y disponible.
3. Revisa la cadena `ConStr` en `appsettings.json` para confirmar que apunta a la instancia de base de datos de tu equipo.
4. Ejecuta:

   ```powershell
   dotnet restore
   dotnet build
   dotnet run
   ```

La aplicacion aplica las migraciones de Entity Framework Core al iniciar. Usa una base de datos local de desarrollo y no guardes archivos de base de datos locales en el repositorio.

## Estructura

- `Models`: modelos de libros, estudiantes, prestamos y devoluciones.
- `Context`: contexto de Entity Framework Core.
- `Services`: operaciones de datos, resultados y reglas de validacion.
- `Components/Pages`: pantallas de la aplicacion.
- `Migrations`: migraciones de la base de datos.
