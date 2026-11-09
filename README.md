# Sistema de Gestión de Biblioteca

Aplicación de escritorio desarrollada para el ACA final de Programación Avanzada. Permite gestionar libros, autores, editoriales y usuarios; registrar préstamos y devoluciones; consultar reportes.

## Tecnologías
- C# / .NET 8 / Windows Forms
- SQL Server 2022
- Microsoft.Data.SqlClient
- Visual Studio

## Arquitectura
- `Biblioteca.Domain`: entidades
- `Biblioteca.Data`: conexión y repositorios
- `Biblioteca.Business`: servicios y validaciones
- `Biblioteca.Presentation`: formularios y menú

## Instalación
1. Instalar Visual Studio con desarrollo de escritorio .NET y SQL Server.
2. Ejecutar `BibliotecaDB.sql` en una **base nueva** con SSMS. **No ejecutar el script sobre la base existente con información**: las instrucciones `CREATE TABLE` generarían errores.
3. Configurar `Biblioteca.Data/Database/ConnectionFactory.cs` con el nombre del servidor local y `Database=BibliotecaDB`.
4. Restaurar paquetes NuGet y compilar la solución.
5. Establecer `Biblioteca.Presentation` como proyecto de inicio y ejecutar con `F5`.

## Uso
Desde el menú principal, abrir Libros, Autores, Editoriales, Usuarios, Préstamos y Devoluciones o Reportes. Registrar primero una editorial, luego libros y usuarios; los préstamos requieren stock disponible. Seleccionar un préstamo activo para registrar su devolución.

## Documentación
Consultar `Documentacion_Biblioteca.md` para objetivos, casos de uso, diagramas, diccionario de datos, pruebas y limitaciones.

## Capturas
Agregar evidencias reales de ejecución a `capturas/` y enlazarlas aquí.

## Limitaciones conocidas
Pendiente vincular autores a libros desde interfaz, búsqueda específica, campo Documento del usuario y edición/eliminación de préstamos. El reporte accede a la capa de datos directamente.

## Autoría
[Nombre(s) del estudiante] — 2026.
