
# Sistema de Gestión de Biblioteca

Aplicación de escritorio desarrollada en C# y .NET 8 para administrar
los procesos de una biblioteca mediante una arquitectura por capas
y una base de datos SQL Server.

## Funcionalidades

- Gestión de libros, autores y editoriales.
- Asociación de múltiples autores a un libro.
- Administración de usuarios.
- Registro de préstamos y devoluciones.
- Control automático de ejemplares disponibles.
- Validación de operaciones y transacciones SQL.
- Búsquedas y filtros en los módulos principales.
- Dashboard de analítica con indicadores y gráfico mensual.
- Reportes de inventario, préstamos, usuarios y editoriales.
- Exportación de reportes a CSV compatible con Excel.
- Impresión de reportes con opción de guardar como PDF.

## Tecnologías

- C# y .NET 8
- Windows Forms
- SQL Server 2022
- Microsoft.Data.SqlClient
- Visual Studio
- Git y GitHub

## Arquitectura

El proyecto está organizado en cuatro capas:

| Proyecto | Responsabilidad |
|---|---|
| Biblioteca.Domain | Entidades del sistema |
| Biblioteca.Data | Conexión SQL y repositorios |
| Biblioteca.Business | Servicios y validaciones |
| Biblioteca.Presentation | Interfaz gráfica y formularios |

## Estructura del repositorio

- `Biblioteca.Business/`
- `Biblioteca.Data/`
- `Biblioteca.Domain/`
- `Biblioteca.Presentation/`
- `database/`
- `Documentacion/`

## Instalación

1. Instalar Visual Studio con soporte para desarrollo de escritorio .NET.
2. Instalar SQL Server y SQL Server Management Studio.
3. Preparar la base de datos utilizando `database/BibliotecaDB.sql`.
4. Configurar el servidor y la base de datos en
   `Biblioteca.Data/Database/ConnectionFactory.cs`.
5. Abrir `Biblioteca.Presentation/Biblioteca.Presentation.slnx`.
6. Restaurar las dependencias y compilar la solución.
7. Establecer `Biblioteca.Presentation` como proyecto de inicio.
8. Ejecutar la aplicación.

No ejecutar el script de creación sobre una base de datos existente
sin revisar previamente su contenido.

## Dashboard de analítica

El módulo de reportes permite consultar:

- Cantidad de libros registrados.
- Total de ejemplares y disponibilidad.
- Usuarios registrados.
- Préstamos activos y vencidos.
- Devoluciones realizadas.
- Historial de préstamos.
- Evolución mensual de préstamos.
- Libros más prestados.
- Usuarios más activos.
- Distribución de libros por editorial.

Los indicadores utilizan información almacenada en SQL Server.

## Pruebas realizadas

Se verificaron:

- Registro y consulta de libros.
- Selección y asociación de autores.
- Funcionamiento de búsquedas.
- Registro de préstamos con existencias disponibles.
- Descuento automático del inventario.
- Registro de devoluciones.
- Restauración de existencias.
- Prevención de devoluciones duplicadas.
- Consultas y exportación CSV del dashboard.
- Compilación y ejecución de la solución completa.

## Documentación

La documentación académica se encuentra en
[Documentacion ACA.pdf](Documentacion%20ACA.pdf).

Las capturas de funcionamiento se encuentran en
[Documentacion](Documentacion/).

## Alcance

Proyecto académico de gestión bibliotecaria. Utiliza una
base de datos local SQL Server y autenticación integrada
de Windows. No incluye un sistema de autenticación de
usuarios de la aplicación ni despliegue web.

## Autoría

Proyecto académico de Programación Avanzada, 2026.

Desarrollado por [braianmoreno100-spec](https://github.com/braianmoreno100-spec).




## Cómo ejecutar el proyecto paso a paso

### Requisitos
- Windows 10 u 11.
- Visual Studio con desarrollo de escritorio .NET.
- .NET 8 SDK.
- SQL Server 2022.
- SQL Server Management Studio (SSMS).

### 1. Descargar el proyecto
Descargar el ZIP desde GitHub y extraerlo en una carpeta.

### 2. Preparar la base de datos
1. Abrir SQL Server Management Studio.
2. Conectarse a la instancia de SQL Server.
3. Abrir el archivo `database/BibliotecaDB.sql`.
4. Revisar y ejecutar el script para crear la base de datos.
5. Confirmar que exista `BibliotecaDB`.

### 3. Configurar la conexión
Abrir:

`Biblioteca.Data/Database/ConnectionFactory.cs`

Verificar que el nombre del servidor corresponda
a la instancia local de SQL Server.

La conexión utiliza autenticación de Windows.

### 4. Abrir la solución
1. Abrir Visual Studio.
2. Seleccionar "Abrir un proyecto o una solución".
3. Abrir:
   `Biblioteca.Presentation/Biblioteca.Presentation.slnx`
4. Esperar a que se carguen los cuatro proyectos.

### 5. Seleccionar el proyecto de inicio
IMPORTANTE:

1. Abrir el Explorador de soluciones.
2. Hacer clic derecho en `Biblioteca.Presentation`.
3. Seleccionar "Establecer como proyecto de inicio".

No ejecutar Biblioteca.Domain, Biblioteca.Data o
Biblioteca.Business directamente, ya que son
bibliotecas de clases.

### 6. Compilar y ejecutar
1. Presionar Ctrl + Shift + B para compilar.
2. Verificar que no existan errores.
3. Presionar F5 para ejecutar.
4. Se abrirá el menú principal del sistema.

### 7. Utilizar el sistema
Desde el menú principal es posible acceder a:

- Gestión de libros y autores.
- Gestión de editoriales.
- Gestión de usuarios.
- Préstamos y devoluciones.
- Dashboard de analítica y reportes.

NOTA: Para utilizar la aplicación, SQL Server debe
estar instalado, configurado y accesible.




## Cómo ejecutar el proyecto paso a paso

### Requisitos
- Windows 10 u 11.
- Visual Studio con desarrollo de escritorio .NET.
- .NET 8 SDK.
- SQL Server 2022.
- SQL Server Management Studio (SSMS).

### 1. Descargar el proyecto
Descargar el ZIP desde GitHub y extraerlo en una carpeta.

### 2. Preparar la base de datos
1. Abrir SQL Server Management Studio.
2. Conectarse a la instancia de SQL Server.
3. Abrir el archivo `database/BibliotecaDB.sql`.
4. Revisar y ejecutar el script para crear la base de datos.
5. Confirmar que exista `BibliotecaDB`.

### 3. Configurar la conexión
Abrir:

`Biblioteca.Data/Database/ConnectionFactory.cs`

Verificar que el nombre del servidor corresponda
a la instancia local de SQL Server.

La conexión utiliza autenticación de Windows.

### 4. Abrir la solución
1. Abrir Visual Studio.
2. Seleccionar "Abrir un proyecto o una solución".
3. Abrir:
   `Biblioteca.Presentation/Biblioteca.Presentation.slnx`
4. Esperar a que se carguen los cuatro proyectos.

### 5. Seleccionar el proyecto de inicio
IMPORTANTE:

1. Abrir el Explorador de soluciones.
2. Hacer clic derecho en `Biblioteca.Presentation`.
3. Seleccionar "Establecer como proyecto de inicio".

No ejecutar Biblioteca.Domain, Biblioteca.Data o
Biblioteca.Business directamente, ya que son
bibliotecas de clases.

### 6. Compilar y ejecutar
1. Presionar Ctrl + Shift + B para compilar.
2. Verificar que no existan errores.
3. Presionar F5 para ejecutar.
4. Se abrirá el menú principal del sistema.

### 7. Utilizar el sistema
Desde el menú principal es posible acceder a:

- Gestión de libros y autores.
- Gestión de editoriales.
- Gestión de usuarios.
- Préstamos y devoluciones.
- Dashboard de analítica y reportes.

NOTA: Para utilizar la aplicación, SQL Server debe
estar instalado, configurado y accesible.
