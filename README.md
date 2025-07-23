Backend API - Hardware Ecommerce 🖥️ 

API REST para tienda de hardware

Stack técnologico: 

- ASP.NET Core web API | .NET 9
- Entity Framework Core 
- PostgreSQL 
- JWT 
- Docker 
- BCrypt
- Cloudinary 

## 🔐 Autenticación Avanzada  
- Registro/login con tokens JWT (refresh tokens implementados)  
- Hashing de contraseñas con BCrypt + políticas de complejidad  
- Roles (Admin/Customer)

## 🛒 E-Commerce Core

- CRUD completo de productos con filtros avanzados
- Sistema de carrito con validación de stock
- Gestión de pedidos (Creación → Pago → Envío → Entrega)

## 🛡️ Seguridad & Performance

- Rate limiting por endpoint
- Headers de seguridad (HSTS, XSS Protection)
- Validación de inputs
- Logging estructurado con Serilog

## 🖼️ Gestión Multimedia  
- Upload automático a Cloudinary con optimización de imágenes (thumbnails, formato WebP)  
- Metadata embebida en productos (peso, dimensiones técnicas)  

## ⚙️ Arquitectura Limpia  
- Layered Architecture con separación de responsabilidades y Patrón Repository
- Middlewares personalizados (logging, manejo de errores)
