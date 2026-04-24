# 🖥️ NovaTech Store — Backend API

API REST para e-commerce construida con **ASP.NET Core 9**, **Entity Framework Core** y **PostgreSQL**, aplicando **Clean Architecture**.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-512BD4?logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![Entity Framework](https://img.shields.io/badge/EF%20Core-9.0-512BD4?logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-20.10-2496ED?logo=docker&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-Auth-000000?logo=jsonwebtokens&logoColor=white)

🔗 **Frontend:** [Repositorio del Frontend](https://github.com/AngelSpinazzola/hardware-store-frontend)
🔗 **Demo en vivo:** (https://novatech-store.vercel.app/)

---

## 📋 Tabla de Contenidos

- [Características](#-características)
- [Stack Tecnológico](#-stack-tecnológico)
- [Arquitectura](#-arquitectura)
- [Modelo de Datos](#-modelo-de-datos)
- [Endpoints Principales](#-endpoints-principales)
- [Seguridad](#-seguridad)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación](#-instalación)
- [Configuración](#-configuración)
- [Ejecución](#-ejecución)
- [Migraciones](#-migraciones)
- [Docker](#-docker)
- [Deploy](#-deploy)

---

## ✨ Características

### 🔐 Autenticación y usuarios
- Registro/login con **JWT** (tokens en cookies httpOnly)
- **Google OAuth 2.0** — login con cuentas de Google
- Hashing de contraseñas con **BCrypt** + políticas de complejidad
- Recuperación de contraseña vía email (tokens con expiración)
- Sistema de roles: **Admin** / **Customer**

### 🛒 E-commerce core
- **CRUD completo de productos** con filtros avanzados (categoría, marca, plataforma, precio, stock)
- **Gestión de imágenes múltiples** con Cloudinary (principal, orden, thumbnails WebP)
- **Sistema de carrito** con validación de stock
- **Gestión de órdenes** (`pending_payment → payment_submitted → payment_approved → shipped → delivered`)
- **Direcciones de envío** podes tener varias direcciones y asi elegir donde recibir el envio.
- **Snapshot histórico** — la orden almacena una copia inmutable de dirección y datos del receptor

### 💳 Pagos
- Integración completa con **MercadoPago API** (creación de preferencias)
- **Webhook IPN** con validación HMAC de firmas
- **Transferencia bancaria** con upload de comprobante (imagen o PDF) a Cloudinary
- Flujo de aprobación/rechazo por admin con notas

### 📊 Admin
- Dashboard con métricas agregadas
- Analytics de ventas por período, top productos, ventas por categoría
- Gestión completa de órdenes (aprobar, rechazar, enviar, entregar, cancelar)
- Visualización y descarga de comprobantes

### 🛡️ Seguridad y performance
- **Rate limiting** por endpoint (auth, search, upload, general)
- Headers de seguridad (HSTS, CSP, X-Frame-Options, X-XSS-Protection)
- **Validación estricta** con FluentValidation
- **Logging estructurado** con Serilog (console + file rolling)
- **Response compression** (Gzip) + Response caching + Memory cache
- Índices de base de datos estratégicos

### 🖼️ Gestión multimedia
- Upload automático a **Cloudinary** con optimización (WebP, thumbnails)
- Validación de archivos (extensión, MIME type, tamaño)
- Soporte de imágenes (JPG, PNG, GIF, WebP) y documentos (PDF)

---

## 🚀 Stack Tecnológico

### Core
- **ASP.NET Core 9** — framework web
- **.NET 9** — runtime
- **Entity Framework Core 9** — ORM
- **PostgreSQL 16** — base de datos relacional (Npgsql)

### Autenticación y seguridad
- **JWT Bearer** — autenticación con tokens
- **BCrypt.Net** — hashing de contraseñas
- **Google.Apis.Auth** — verificación de ID tokens de Google
- **Rate Limiting** nativo de ASP.NET Core

### Validación y logging
- **FluentValidation** — validaciones declarativas de DTOs
- **Serilog** — logging estructurado (Console + File sinks)

### Servicios externos
- **Cloudinary** — CDN y optimización de imágenes/PDFs
- **MercadoPago SDK** — pasarela de pagos
- **Brevo (Sendinblue) API** — envío de emails transaccionales

### DevOps
- **Docker** — containerización
- **Swagger / OpenAPI** — documentación
- **Railway** — hosting de producción

---

## 🏗️ Arquitectura

Proyecto estructurado bajo los principios de **Clean Architecture**, con 4 capas desacopladas:

```
┌──────────────────────────────────────────────────┐
│              HardwareStore.API                   │
│  Controllers · Middleware · Program.cs · DI      │
└──────────────────────────────────────────────────┘
                        ↓
┌──────────────────────────────────────────────────┐
│          HardwareStore.Application               │
│  DTOs · Validators · Interfaces de Servicios     │
│  Auth · Products · Orders · Payments · ...       │
└──────────────────────────────────────────────────┘
                        ↓
┌──────────────────────────────────────────────────┐
│            HardwareStore.Domain                  │
│  Entidades · Enums · Interfaces de Repos         │
│  Excepciones de dominio                          │
└──────────────────────────────────────────────────┘
                        ↑
┌──────────────────────────────────────────────────┐
│         HardwareStore.Infrastructure             │
│  EF Core · Repositorios · Migrations             │
│  JWT · Cloudinary · MercadoPago · Email          │
└──────────────────────────────────────────────────┘
```

### Patrones aplicados
- ✅ **Clean / Layered Architecture** — separación estricta de responsabilidades
- ✅ **Repository Pattern** — abstracción del acceso a datos
- ✅ **Service Layer** — encapsulación de lógica de negocio
- ✅ **Dependency Injection** — nativo de ASP.NET Core
- ✅ **DTO Pattern** + **FluentValidation** — validación declarativa
- ✅ **Middleware Pipeline** personalizado (excepciones, headers, cookie→Bearer)
- ✅ **Snapshot Pattern** — datos de envío copiados a la orden (histórico inmutable)

### Estructura de carpetas

```
src/
├── HardwareStore.API/              # Capa de presentación
│   ├── Controllers/                # Endpoints REST
│   │   ├── AuthController.cs
│   │   ├── ProductController.cs
│   │   ├── OrderController.cs
│   │   ├── PaymentController.cs
│   │   └── ...
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── appsettings.json
│   └── Program.cs
│
├── HardwareStore.Application/      # Casos de uso
│   ├── Auth/                       # DTOs, validators, GoogleAuth
│   ├── Products/
│   ├── Orders/
│   ├── Payments/                   # MercadoPago DTOs
│   ├── Customers/                  # Direcciones de envío
│   ├── Categories/
│   └── Common/                     # Interfaces compartidas
│
├── HardwareStore.Domain/           # Núcleo de negocio
│   ├── Entities/                   # User, Product, Order, etc.
│   ├── Enums/                      # OrderStatus, ProductStatus
│   ├── Interfaces/                 # IRepository<T>
│   └── Exceptions/
│
└── HardwareStore.Infrastructure/   # Detalles de infraestructura
    ├── Persistence/
    │   ├── ApplicationDbContext.cs
    │   ├── Repositories/           # Implementaciones EF
    │   └── Migrations/             # Migraciones EF Core
    ├── Identity/
    │   ├── JwtHelper.cs
    │   ├── PasswordHelper.cs       # BCrypt
    │   ├── SecurityHelper.cs
    │   └── FileValidationHelper.cs
    └── ExternalServices/
        ├── FileService.cs          # Cloudinary
        ├── MercadoPagoService.cs
        ├── MercadoPagoWebhookValidator.cs
        ├── GoogleAuthService.cs
        ├── EmailService.cs         # Brevo
        └── ...
```

---

## 🗄️ Modelo de Datos

Tablas principales en **PostgreSQL**:

| Tabla | Descripción |
|-------|-------------|
| `Users` | Usuarios con roles (Admin/Customer), Google OAuth opcional |
| `Products` | Productos con soft-delete (`Status` enum + `DeletedAt`) |
| `Categories` | Categorías de productos |
| `ProductImages` | Imágenes múltiples con orden y flag de principal |
| `Orders` | Órdenes con snapshot de dirección y receptor autorizado |
| `OrderItems` | Items de cada orden (snapshot de nombre, marca, modelo) |
| `ShippingAddresses` | Direcciones de envío del usuario |
| `PasswordResetTokens` | Tokens de recuperación con expiración |

**Índices estratégicos** en `Email` (único), `Category.Name` (único), `Product.Status/CategoryId/Name`, `Order.Status/CreatedAt`, `ShippingAddress.UserId`, `PasswordResetToken.Token/ExpiresAt`.

**Comportamientos de borrado**:
- `Cascade` en `ProductImages`, `OrderItems`, `ShippingAddresses`
- `SetNull` en `Order.User` y `Order.ShippingAddress` (preserva historial)
- `Restrict` en `Category` y `Product` (evita borrado accidental)

---

## 🌐 Endpoints Principales

### 🔐 Auth (`/api/auth`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/register` | Registro de usuario |
| POST | `/login` | Login con email/password |
| POST | `/google` | Login con Google ID token |
| POST | `/logout` | Cierre de sesión (limpia cookie) |
| GET | `/me` | Datos del usuario autenticado |
| PUT | `/profile` | Actualizar perfil |
| POST | `/forgot-password` | Solicitar reset de contraseña |
| POST | `/reset-password` | Confirmar reset con token |

### 📦 Productos (`/api/product`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/` | Listado paginado (público) |
| GET | `/{id}` | Detalle de producto |
| GET | `/search?term=...` | Búsqueda por texto |
| GET | `/filter` | Filtrado avanzado (categoría, marca, precio, stock) |
| GET | `/categories` | Categorías disponibles |
| GET | `/brands` | Marcas disponibles |
| GET | `/menu-structure` | Estructura jerárquica del menú |
| GET | `/stats` | Estadísticas (Admin) |
| POST | `/` | Crear producto (Admin) |
| PUT | `/{id}` | Actualizar producto (Admin) |
| DELETE | `/{id}` | Eliminar (soft-delete, Admin) |
| POST | `/{id}/images` | Agregar imágenes |
| PUT | `/{id}/images/order` | Reordenar imágenes |
| PUT | `/{id}/images/{imageId}/main` | Marcar como principal |
| DELETE | `/{id}/images/{imageId}` | Eliminar imagen |

### 📬 Órdenes (`/api/order`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/` | Crear orden (autenticado o invitado) |
| GET | `/{id}` | Detalle de orden |
| GET | `/my-orders` | Órdenes del usuario actual |
| GET | `/` | Todas las órdenes (Admin) |
| GET | `/status/{status}` | Filtrar por estado (Admin) |
| GET | `/pending-review` | Comprobantes pendientes (Admin) |
| POST | `/{id}/payment-receipt` | Upload de comprobante |
| GET | `/{id}/view-receipt` | Ver comprobante |
| GET | `/{id}/download-receipt` | Descargar comprobante |
| PUT | `/{id}/approve-payment` | Aprobar pago (Admin) |
| PUT | `/{id}/reject-payment` | Rechazar pago (Admin) |
| PUT | `/{id}/mark-shipped` | Marcar enviado (Admin) |
| PUT | `/{id}/mark-delivered` | Marcar entregado (Admin) |
| DELETE | `/{id}/cancel` | Cancelar orden |

### 💳 Pagos (`/api/payment`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/mercadopago/create` | Crear preferencia de pago |
| POST | `/mercadopago/webhook` | Webhook IPN (validado con HMAC) |

### 🏠 Direcciones (`/api/shippingaddress`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/my-addresses` | Direcciones del usuario |
| GET | `/{id}` | Detalle |
| POST | `/` | Crear dirección |
| PUT | `/{id}` | Actualizar |
| PUT | `/{id}/set-default` | Marcar como predeterminada |
| DELETE | `/{id}` | Eliminar |

### 🩺 Health
- `GET /api/health` — health check

---

## 🛡️ Seguridad

- ✅ **JWT** con validación estricta (Issuer, Audience, Lifetime, SigningKey) y `ClockSkew = 0`
- ✅ **Cookies httpOnly** para el token (mitigación XSS) con fallback automático al header `Authorization`
- ✅ **BCrypt** con políticas de complejidad de contraseña
- ✅ **Rate limiting** particionado por usuario/IP:
  - Global: 100 req/min
  - `auth`: 5 req/min (login, register, create order)
  - `search`: 50 req/min
  - `upload`: 10 req/min
  - `general`: 30 req/min
- ✅ **CORS** restrictivo por entorno con `AllowCredentials`
- ✅ **Headers de seguridad**: `X-Content-Type-Options`, `X-Frame-Options`, `X-XSS-Protection`, `Referrer-Policy`, CSP con allowlist para Google OAuth
- ✅ **HSTS** en producción (`max-age=31536000`)
- ✅ **HTTPS Redirection** en producción
- ✅ **Validación de archivos**: extensiones permitidas (JPG/PNG/GIF/WebP/PDF), MIME type, tamaño máximo (2 MB imágenes, 5 MB comprobantes)
- ✅ **Webhook de MercadoPago** con validación HMAC obligatoria
- ✅ **Logging estructurado** de intentos sospechosos (IPs, emails, validaciones fallidas)

---

## 📦 Requisitos Previos

- **.NET SDK 9.0+**
- **PostgreSQL 16+** (local o en la nube)
- **Docker** (opcional, para containerización)
- Cuentas/credenciales en servicios externos:
  - [Cloudinary](https://cloudinary.com/) — almacenamiento de imágenes
  - [MercadoPago](https://www.mercadopago.com.ar/developers) — pagos
  - [Google Cloud Console](https://console.cloud.google.com/apis/credentials) — OAuth Client ID
  - [Brevo](https://www.brevo.com/) — email transaccional

---

## 🔧 Instalación

```bash
# Clonar el repositorio
git clone https://github.com/AngelSpinazzola/hardware-store-backend-api.git
cd hardware-store-backend-api

# Restaurar dependencias
dotnet restore

# Configurar secretos (ver sección siguiente)
cp src/HardwareStore.API/appsettings.example.json src/HardwareStore.API/appsettings.Development.json
# Editar appsettings.Development.json con tus valores

# Aplicar migraciones
cd src/HardwareStore.API
dotnet ef database update

# Levantar la API
dotnet run
```

La API estará disponible en [http://localhost:10000](http://localhost:10000) y Swagger en [http://localhost:10000/swagger](http://localhost:10000/swagger).

---

## ⚙️ Configuración

Copiar `appsettings.example.json` a `appsettings.Development.json` y completar:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=hardwarestore;Username=postgres;Password=your-password"
  },
  "Jwt": {
    "SecretKey": "your-super-secret-key-minimum-32-characters",
    "Issuer": "HardwareStoreAPI",
    "Audience": "HardwareStoreApp",
    "ExpireDays": 7
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  },
  "Google": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com"
  },
  "Brevo": {
    "ApiKey": "your-brevo-api-key",
    "FromEmail": "noreply@yourdomain.com"
  },
  "MercadoPago": {
    "AccessToken": "your-mp-access-token",
    "WebhookSecret": "your-webhook-secret"
  },
  "Frontend": {
    "BaseUrl": "http://localhost:5173"
  }
}
```

> ⚠️ **Nunca commitees `appsettings.Development.json` ni credenciales reales.** Usá variables de entorno o **User Secrets** (`dotnet user-secrets`) para desarrollo local.

---

## ▶️ Ejecución

```bash
# Desarrollo (con hot reload)
dotnet watch run --project src/HardwareStore.API

# Run normal
dotnet run --project src/HardwareStore.API

# Build
dotnet build

# Publish para producción
dotnet publish -c Release -o ./publish
```

### Puertos por defecto
- **API**: `http://localhost:10000`
- **Swagger**: `http://localhost:10000/swagger` (solo en Development)
- **Health check**: `http://localhost:10000/api/health`

---

## 🗃️ Migraciones

```bash
# Instalar EF Core CLI (si no está instalado)
dotnet tool install --global dotnet-ef

# Crear una nueva migración
cd src/HardwareStore.API
dotnet ef migrations add NombreDeLaMigracion --project ../HardwareStore.Infrastructure

# Aplicar migraciones
dotnet ef database update

# Revertir última migración
dotnet ef migrations remove --project ../HardwareStore.Infrastructure

# Generar script SQL
dotnet ef migrations script
```

---

## 🐳 Docker

### Build y run local

```bash
# Build de la imagen
docker build -t hardware-store-api .

# Run del contenedor
docker run -d \
  -p 10000:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=hardwarestore;Username=postgres;Password=your-password" \
  -e Jwt__SecretKey="your-super-secret-key" \
  --name hardware-store-api \
  hardware-store-api

# Ver logs
docker logs -f hardware-store-api
```

### docker-compose (sugerido)

```yaml
services:
  api:
    build: .
    ports:
      - "10000:8080"
    environment:
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=hardwarestore;Username=postgres;Password=postgres
    depends_on:
      - db

  db:
    image: postgres:16
    environment:
      - POSTGRES_DB=hardwarestore
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    volumes:
      - pgdata:/var/lib/postgresql/data
    ports:
      - "5432:5432"

volumes:
  pgdata:
```

---

## 🚢 Deploy

### Infraestructura
- **Hosting**: [Railway](https://railway.app/) con Docker
- **Base de datos**: PostgreSQL gestionado

### Variables de entorno críticas
Configurar en el panel del host (Railway, Azure, AWS, etc.):

```
ConnectionStrings__DefaultConnection
Jwt__SecretKey
Jwt__Issuer
Jwt__Audience
Cloudinary__CloudName
Cloudinary__ApiKey
Cloudinary__ApiSecret
Google__ClientId
Brevo__ApiKey
Brevo__FromEmail
MercadoPago__AccessToken
MercadoPago__WebhookSecret
Frontend__BaseUrl
PORT
```

---

## 👤 Autor

**Angel Spinazzola**

- GitHub: [@AngelSpinazzola](https://github.com/AngelSpinazzola)

---

⭐️ Si este proyecto te resultó útil, considera darle una estrella en GitHub.
