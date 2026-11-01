# Registro de Libros

**Estudiante:** Williams Rafael Sanchez Acosta  
**Matricula:** 2023-0136  
**Asignatura:** Programacion Aplicada 1  
**Universidad:** Universidad Catolica Nordestana (UCNE)

## Descripcion

Aplicacion web desarrollada con C# y Blazor para administrar un catalogo de libros, estudiantes, prestamos, devoluciones y entradas de inventario. Utiliza Entity Framework Core y SQL Server.

## Funciones principales

- Registrar, consultar, editar y eliminar libros, respetando el historial de movimientos.
- Registrar, consultar, editar y eliminar estudiantes.
- Registrar prestamos seleccionando estudiantes y libros existentes.
- Evitar que un estudiante tenga mas de un prestamo pendiente.
- Verificar que existan ejemplares disponibles antes de registrar un prestamo.
- Descontar automaticamente un ejemplar al registrar un prestamo.
- Aumentar el inventario al registrar una devolucion.
- Mantener el historial de prestamos y devoluciones.
- Registrar entradas de inventario con varios libros y cantidades.
- Actualizar las existencias al crear, editar o eliminar una entrada.
- Buscar registros desde las pantallas de consulta.
- Consultar el promedio de dias que los libros devueltos estuvieron prestados.

La cantidad disponible se administra desde las entradas, prestamos y devoluciones. No se modifica manualmente desde el formulario de edicion del libro.

## API REST de estudiantes

La API utiliza las mismas tablas y el mismo contexto de Entity Framework de la aplicacion.

| Metodo | Ruta | Descripcion |
| --- | --- | --- |
| GET | `/api/estudiantes` | Consultar todos los estudiantes |
| GET | `/api/estudiantes/{id}` | Consultar un estudiante |
| POST | `/api/estudiantes` | Registrar un estudiante |
| PUT | `/api/estudiantes/{id}` | Actualizar un estudiante |
| DELETE | `/api/estudiantes/{id}` | Eliminar un estudiante si no tiene prestamos |

Ejemplo de JSON para registrar o actualizar un estudiante:

```json
{
  "nombres": "Juan Perez",
  "direccion": "San Francisco de Macoris",
  "email": "juan@example.com",
  "fechaNacimiento": "2004-05-20"
}
```

La API valida los campos obligatorios, el formato del correo, la fecha de nacimiento y los nombres duplicados. Responde con `201 Created` al registrar, `404 Not Found` si el estudiante no existe y `409 Conflict` cuando el nombre ya esta registrado o no se permite eliminar el estudiante.

## Tecnologias

- C#
- .NET 10
- Blazor con renderizado interactivo en servidor
- Minimal APIs de ASP.NET Core
- Entity Framework Core 10
- SQL Server
- Aplicada1.Core
- Blazored.Toast
- QuickGrid
- Bootstrap

## Requisitos

- .NET 10 SDK
- SQL Server LocalDB o una instancia de SQL Server compatible
- Visual Studio 2026 o una terminal con soporte para .NET

## Ejecutar el proyecto

1. Clona el repositorio:

   ```bash
   git clone https://github.com/Williamsanchez056/RegistroLibro.git
   ```

2. Abre la carpeta del proyecto.
3. Revisa la cadena `ConStr` en `appsettings.json` para confirmar que apunta a tu instancia de SQL Server.
4. Ejecuta:

   ```powershell
   dotnet restore
   dotnet build
   dotnet run
   ```

La aplicacion aplica las migraciones pendientes al iniciar. La configuracion incluida utiliza SQL Server LocalDB para desarrollo local.

## Estructura

- `Models`: modelos de libros, estudiantes, prestamos, devoluciones y entradas.
- `Context`: contexto de Entity Framework Core.
- `Services`: operaciones de datos, reglas de negocio y validaciones.
- `Endpoints`: rutas de la API REST.
- `Components/Pages`: pantallas de la aplicacion.
- `Migrations`: migraciones de la base de datos.
