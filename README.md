# mediTool — Backend

Sistema de gestión de turnos médicos y clínicos desarrollado en ASP.NET Core (.NET 10) con arquitectura hexagonal, PostgreSQL, EF Core y autenticación JWT con refresh tokens.

---

## 🧪 Cómo correr los tests

El backend cuenta con una suite completa de pruebas automatizadas enfocada en la lógica de dominio, servicios de aplicación y adaptadores de API, implementada con **xUnit**, **FluentAssertions** y **NSubstitute**.

### 1. Ejecutar toda la suite de tests

Desde la raíz del repositorio (`mediTool`):

```bash
dotnet test
```

### 2. Ejecutar tests por capa

Para ejecutar únicamente los tests de la capa de aplicación y reglas de negocio:

```bash
dotnet test Service.Tests
```

Para ejecutar los tests de adaptadores de API (middlewares de excepciones, seeders de inicialización):

```bash
dotnet test Api.Tests
```

### 3. Ejecutar con reporte de cobertura de código (Coverlet)

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Los resultados de cobertura se generarán en formato Cobertura XML dentro del directorio `TestResults` de cada proyecto de test (`coverage.cobertura.xml`).

### 4. Filtrar por método o escenario específico

```bash
dotnet test --filter "FullyQualifiedName~GenerarInstancias"
```

---

## 📐 Convenciones de los Tests

- **Patrón de Nomenclatura**: `Metodo_Escenario_ResultadoEsperado` (ej. `GenerarInstancias_TurnosYaMaterializados_NoDuplicaLasInstanciasExistentes`).
- **Estructura AAA**: Bloques claros de `Arrange`, `Act` y `Assert`.
- **Aislamiento e Idempotencia**: Cada test es independiente, no comparte estado mutable entre ejecuciones y utiliza dobles de prueba (`NSubstitute`) o bases de datos en memoria (`UseInMemoryDatabase`) con identificadores únicos (`Guid.NewGuid()`).