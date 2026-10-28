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

---

## 🚀 Deploy

El backend está pensado para correr en Railway con un contenedor Docker y PostgreSQL gestionado. El TLS lo termina el proxy de Railway, por eso la redirección HTTPS queda desactivada en producción.

### Variables de entorno requeridas

| Variable | Descripción |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Connection string de PostgreSQL (`Host=...;Port=...;Database=...;Username=...;Password=...`) |
| `Jwt__Key` | Clave secreta de al menos 32 caracteres |
| `Jwt__Issuer` | Emisor del token |
| `Jwt__Audience` | Audiencia del token |
| `Jwt__AccessTokenExpirationMinutes` | Vigencia del access token en minutos |
| `Jwt__RefreshTokenExpirationDays` | Vigencia del refresh token en días |
| `Cors__AllowedOrigins__0` | Origen permitido del frontend (agregar `__1`, `__2`, ... para más orígenes) |
| `Seed__AdminEmail` | Email del administrador inicial |
| `Seed__AdminPassword` | Contraseña del administrador inicial |

Generar una clave segura para `Jwt__Key`:

```bash
openssl rand -base64 64
```

Las migraciones se aplican solas al arrancar la aplicación (`Database.Migrate()`). Conviene hacer un backup de la base antes de deployar versiones que incluyan migraciones nuevas.

El endpoint `GET /health` responde `{ "status": "ok" }` sin autenticación y sirve como health check del servicio.