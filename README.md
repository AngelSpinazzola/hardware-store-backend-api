# Hardware Store - E-commerce API

  API REST para e-commerce de productos tecnológicos, construida con .NET 9 Web API, EntityFramework, Postgresql y Clean Architecture.

  ## 🎯 Sobre el proyecto

  API backend completa para un e-commerce de hardware tech, desarrollada aplicando principios de Clean Architecture para garantizar escalabilidad, mantenibilidad y testing eficiente.

  El proyecto nació como una solución integral para gestionar productos, usuarios, órdenes y pagos online, integrándose con servicios externos como MercadoPago para procesamiento de pagos.

  ## 🏗️ Arquitectura

  El proyecto sigue los principios de **Clean Architecture** con separación clara de responsabilidades:

  ├── Domain/          → Entidades y lógica de negocio pura
  
  ├── Application/     → DTOs, interfaces y casos de uso
  
  ├── Infrastructure/  → Repositorios, servicios externos, DB
  
  └── API/            → Controllers, autenticación, configuración

  ### ¿Por qué Clean Architecture?

  - **Independencia de frameworks**: La lógica de negocio no depende de EF Core o ASP.NET
  - **Testeable**: Cada capa puede testearse de forma aislada
  - **Mantenible**: Cambios en UI o DB no afectan la lógica de negocio
  - **Escalable**: Fácil agregar nuevas funcionalidades sin romper código existente

  ## ✨ Características principales

  ### 🔐 Autenticación y seguridad
  - JWT con refresh tokens
  - Autenticación OAuth con Google
  - Rate limiting por endpoint
  - Validación y sanitización de inputs
  - Recuperación de contraseña por email

  ### 💳 Integración con MercadoPago
  - Creación de preferencias de pago
  - Webhooks con validación HMAC-SHA256
  - Idempotencia para prevenir procesamiento duplicado
  - Retry logic con Polly (reintentos con backoff exponencial)
  - Manejo de estados de pago (pending, approved, rejected)

  ### 📦 Gestión de productos
  - CRUD completo de productos
  - Categorías y filtros
  - Gestión de stock con transacciones serializables
  - Múltiples imágenes por producto (Cloudinary)
  - Estados de producto (Active, Inactive, Discontinued)

  ### 🛒 Sistema de órdenes
  - Creación de órdenes con validación de stock
  - Snapshot de precios al momento de la compra
  - Direcciones de envío múltiples por usuario
  - Estados de orden (PendingPayment, Paid, Shipped, Delivered, Cancelled)
  - Expiración automática de órdenes pendientes (24hs)

  ### 📧 Notificaciones
  - Emails transaccionales con Brevo
  - Confirmación de registro
  - Recuperación de contraseña
  - Confirmación de órdenes

  ## 🛠️ Stack tecnológico

  **Backend**
  - .NET 9
  - Entity Framework Core
  - PostgreSQL
  - Polly (resiliencia)
  - Serilog (logging)

  **Servicios externos**
  - MercadoPago (pagos)
  - Cloudinary (imágenes)
  - Brevo (emails)
  - Google OAuth

  **Deployment**
  - Railway (backend)
  - Supabase (PostgreSQL)

  ## 🔍 Decisiones técnicas destacadas

  ### Transacciones serializables para stock
  Para evitar race conditions cuando múltiples usuarios compran el último producto simultáneamente, implementé transacciones con isolation level `Serializable`:

  ```csharp
  using var transaction = await _context.Database
      .BeginTransactionAsync(IsolationLevel.Serializable);

  Idempotencia en webhooks

  Los webhooks de MercadoPago pueden llegar duplicados. Implementé validación para ignorar notificaciones ya procesadas:

  if (order.MercadoPagoPaymentId == paymentId &&
      order.MercadoPagoStatus == paymentInfo.Status)
  {
      return; // Webhook duplicado
  }

  Validación obligatoria de firma

  Todos los webhooks deben incluir firma HMAC-SHA256 válida, caso contrario se rechazan (previene ataques):

  var validator = new MercadoPagoWebhookValidator(secretKey);
  if (!validator.ValidateSignature(xSignature, xRequestId, paymentId))
  {
      return Unauthorized();
  }
```

  📊 Funcionalidades de seguridad

  - ✅ Validación de firma en webhooks de MercadoPago
  - ✅ Rate limiting (5 requests/min en auth, 100/min general)
  - ✅ JWT con expiración y refresh tokens
  - ✅ Sanitización de inputs (prevención XSS/SQL Injection)
  - ✅ CORS configurado solo para dominios específicos
  - ✅ Logging estructurado con Serilog

  📈 Lo que aprendí

  - La importancia de la idempotencia en sistemas distribuidos
  - Cómo prevenir race conditions con transacciones
  - Implementación de Clean Architecture en un proyecto real
  - Integración segura con APIs de terceros (MercadoPago)
  - Uso de Polly para resiliencia ante fallos transitorios



  🚀 Estado del proyecto: en producción pero con mejoras continuas en el frontend.
