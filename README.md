# DOTNET_Servicios_Hex_MSCambioDireccionOnPrem

## Introducción

Este es un microservicio en .NET 8.0 diseñado para manejar operaciones de cambio de dirección (CambioDireccion). El proyecto sigue la Arquitectura Hexagonal (también conocida como Puertos y Adaptadores) para asegurar una separación clara de responsabilidades, testabilidad y mantenibilidad.

El servicio está actualmente implementado como una plantilla/boilerplate con un endpoint de ejemplo. Proporciona una base para construir funcionalidad de cambio de dirección, incluyendo validación, lógica de negocio e integración con sistemas externos.

Características clave:
- **Arquitectura Hexagonal**: Separación clara entre dominio, aplicación, infraestructura y capas API
- **Patrón CQRS**: Uso de MediatR para separación de comandos/consultas
- **Observabilidad**: Integrado con OpenTelemetry y Dynatrace para logging y tracing
- **Documentación API**: Swagger/OpenAPI para exploración interactiva de la API
- **Configuración de Entorno**: Soporte para variables de entorno vía archivos .env

## Arquitectura

El proyecto sigue la Arquitectura Hexagonal con cuatro capas principales:

```
┌─────────────────────────────────────┐
│           CambioDireccion.Api       │  ← Capa de Presentación/API
│  - Controladores, Middleware, Config│
├─────────────────────────────────────┤
│       CambioDireccion.Aplicacion    │  ← Capa de Aplicación
│  - Casos de Uso, Comandos, Consultas│
├─────────────────────────────────────┤
│         CambioDireccion.Dominio     │  ← Capa de Dominio
│  - Entidades, Reglas de Negocio     │
├─────────────────────────────────────┤
│    CambioDireccion.Infraestructura  │  ← Capa de Infraestructura
│  - Servicios Externos, Persistencia │
└─────────────────────────────────────┘
```

### Responsabilidades de las Capas

- **Api**: Maneja solicitudes HTTP, enrutamiento, autenticación y formateo de respuestas. Incluye middleware para logging, manejo de errores y CORS.
- **Aplicacion**: Contiene casos de uso, comandos y consultas. Orquesta la lógica de negocio y se comunica con la capa de dominio.
- **Dominio**: Entidades de negocio core, objetos de valor y reglas de dominio. Independiente de frameworks externos.
- **Infraestructura**: Adaptadores para sistemas externos (bases de datos, APIs, colas de mensajes). Implementa puertos definidos en las capas de dominio y aplicación.

## Prerrequisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git (para clonar el repositorio)
- Opcional: Cuenta de Dynatrace para observabilidad (configurada vía variables de entorno)

## Primeros Pasos

1. **Clonar el repositorio**
   ```bash
   git clone <url-del-repositorio>
   cd DOTNET_Servicios_Hex_MSCambioDireccionOnPrem
   ```

2. **Navegar al proyecto API**
   ```bash
   cd DOTNET_Servicios_Hex_MSCambioDireccionOnPrem/CambioDireccion.Api
   ```

3. **Instalar dependencias**
   ```bash
   dotnet restore
   ```

4. **Configurar variables de entorno** (ver sección de Configuración abajo)

5. **Ejecutar la aplicación**
   ```bash
   dotnet run
   ```


El servicio iniciará en `https://localhost:5001` (o el puerto configurado en launchSettings.json).

## Instalación de la plantilla (dotnet new)

Para instalar y usar esta solución como plantilla local de `dotnet new` sigueestos pasos:

1. Clona el repositorio (si no lo has hecho ya) y sitúate en la carpeta raíz donde se clonó:
   ```git clone <url-del-repositorio> 
      cd DOTNET_Servicios_Hex_MSCambioDireccionOnPrem
   ```
2. Instala la plantilla localmente ejecutandodesde la raíz del repositorio: 
   ```dotnet new --install . --force```

3. Crear un proyecto con la plantilla:
- Desde la CLI:
  ```bash
  dotnet new HexaMSTemplate -n MiServicio --Entities "Cliente,Producto"
  ```
- Desde Visual Studio:
  1. Abrir __Visual Studio__.
  2. __File > New > Project__ (Archivo > Nuevo > Proyecto).
  3. Buscar la plantilla "Hexagonal Architecture Microservice Template" o `HexaMSTemplate`.
  4. La plantilla solicita parámetros: 
     - Nombre del proyecto (ej.: `MiServicio`)
     - Nombre de la Entidad (ej.: `Cliente`)
  5. Completar y crear el proyecto.

  
## Configuración

La aplicación usa variables de entorno para configuración. Copie el archivo `.env` y actualice los valores según sea necesario.

### Variables de Entorno Requeridas

- `OTEL_EXPORTER_OTLP_ENDPOINT_LOGS`: Endpoint de logs de Dynatrace
- `OTEL_EXPORTER_OTLP_ENDPOINT_TRACES`: Endpoint de traces de Dynatrace
- `OTEL_EXPORTER_OTLP_HEADERS`: Token de APIde Dynatrace
- `NOMBRE_SERVICIO`: Nombre del servicio (por defecto: DOTNET_Servicios_Hex_MSCambioDireccionOnPrem)
- `ORIGENES_CORS_PERMITIDOS`: Orígenes CORS permitidos (por defecto: *)

### Archivo .env de ejemplo



## Configuración

La aplicación usa variables de entorno para configuración. Copie el archivo `.env` y actualice los valores según sea necesario.

### Variables de Entorno Requeridas

- `OTEL_EXPORTER_OTLP_ENDPOINT_LOGS`: Endpoint de logs de Dynatrace
- `OTEL_EXPORTER_OTLP_ENDPOINT_TRACES`: Endpoint de traces de Dynatrace
- `OTEL_EXPORTER_OTLP_HEADERS`: Token de API de Dynatrace
- `NOMBRE_SERVICIO`: Nombre del servicio (por defecto: DOTNET_Servicios_Hex_MSCambioDireccionOnPrem)
- `ORIGENES_CORS_PERMITIDOS`: Orígenes CORS permitidos (por defecto: *)

### Archivo .env de ejemplo

```env
# Dynatrace
OTEL_EXPORTER_OTLP_ENDPOINT_LOGS=https://su-instancia-dynatrace.live.dynatrace.com/api/v2/otlp/v1/logs
OTEL_EXPORTER_OTLP_ENDPOINT_TRACES=https://su-instancia-dynatrace.live.dynatrace.com/api/v2/otlp/v1/traces
OTEL_EXPORTER_OTLP_HEADERS=dt0c01.SU_TOKEN_AQUI

# General
NOMBRE_SERVICIO=DOTNET_Servicios_Hex_MSCambioDireccionOnPrem

# CORS
ORIGENES_CORS_PERMITIDOS=*
```

## Construir y Ejecutar

### Desarrollo

```bash
# Restaurar paquetes
dotnet restore

# Construir la solución
dotnet build

# Ejecutar la API
dotnet run --project DOTNET_Servicios_Hex_MSCambioDireccionOnPrem/CambioDireccion.Api
```

### Producción

```bash
# Publicar para producción
dotnet publish DOTNET_Servicios_Hex_MSCambioDireccionOnPrem/CambioDireccion.Api -c Release -o ./publish

# Ejecutar la app publicada
./publish/CambioDireccion.Api
```

## Documentación de la API

El servicio incluye documentación Swagger/OpenAPI. Al ejecutar localmente, acceda en:
- Interfaz Swagger: `https://localhost:5001/swagger`
- JSON OpenAPI: `https://localhost:5001/swagger/v1/swagger.json`

### Endpoints Actuales

#### GET /plantilla
Un endpoint de plantilla/prueba para demostración.

**Parámetros de Consulta:**
- `prueba` (int): Parámetro de prueba

**Respuesta:**
```json
{
  "exitoso": true,
  "datos": true,
  "mensaje": null
}
```

**Solicitud de Ejemplo:**
```
GET /plantilla?prueba=1
```

Este endpoint es parte de la plantilla del proyecto. Reemplácelo con endpoints reales de cambio de dirección conforme se implementen los requerimientos de negocio.

## Pruebas

El proyecto incluye una estructura para pruebas unitarias e integración. Actualmente, no hay pruebas implementadas.

Para ejecutar pruebas (cuando se agreguen):
```bash
dotnet test
```

### Estructura de Pruebas
- Las pruebas unitarias deben colocarse en proyectos de prueba siguiendo la convención de nomenclatura `*Tests.csproj`
- Las pruebas de integración pueden agregarse para endpoints de API e integraciones externas

## Contribuyendo

¡Damos la bienvenida a las contribuciones! Por favor siga estas directrices:

1. Haga fork del repositorio
2. Cree una rama de funcionalidad (`git checkout -b feature/caracteristica-increible`)
3. Confirme sus cambios (`git commit -m 'Agregar caracteristica increible'`)
4. Suba a la rama (`git push origin feature/caracteristica-increible`)
5. Abra un Pull Request

### Estándares de Código
- Siga las convenciones de codificación de C#
- Use nombres significativos para variables y métodos
- Agregue comentarios de documentación XML a las APIs públicas
- Escriba pruebas unitarias para nueva funcionalidad
- Asegúrese de que todas las pruebas pasen antes de enviar

### Flujo de Desarrollo
1. Implemente entidades de dominio y reglas de negocio en la capa `Dominio`
2. Cree casos de uso y comandos en la capa `Aplicacion`
3. Implemente adaptadores de infraestructura en la capa `Infraestructura`
4. Agregue controladores y endpoints de API en la capa `Api`
5. Actualice este README con nueva documentación de API

## Licencia

Este proyecto está licenciado bajo la Licencia MIT - vea el archivo LICENSE para detalles.

## Soporte

Para preguntas o problemas, abra un issue en el repositorio o contacte al equipo de desarrollo.