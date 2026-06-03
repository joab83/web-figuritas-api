# Figuritas API

Backend en .NET 8 para consultar figuritas disponibles y registrar pedidos.

## Configuracion

En desarrollo la conexion a MySQL se define en `appsettings.Development.json`.

Para ejecutar:

```powershell
dotnet run
```

URL local por defecto:

```http
http://localhost:5080
```

## Endpoints

### Obtener datos de un album

```http
GET /api/albums?id_album=1
```

Devuelve los datos del album habilitado indicado por `id_album`.

### Obtener stickers por album

```http
GET /api/stickers?id_album=1
```

Devuelve todas las figuritas habilitadas del album indicado. Incluye SKU,
nombre, precio, stock disponible, grupo y flags de destacada/habilitada.

### Obtener grupos

```http
GET /api/stickers/groups
```

Devuelve el listado de grupos distintos existentes en la tabla de stickers,
ordenados alfabeticamente.

### Crear pedido

```http
POST /api/pedidos
Content-Type: application/json
```

Valida que todos los stickers existan, esten habilitados y tengan stock
suficiente. Si la validacion pasa, registra el pedido, descuenta el stock
disponible y devuelve el numero de pedido generado.

Ejemplo:

```json
{
  "nombre": "Juan Perez",
  "numero_telefono": "1123456789",
  "comentario": "Entregar por la tarde",
  "stickers": [
    {
      "id_album": 1,
      "sku": "ARG17",
      "cantidad": 1
    }
  ]
}
```

Respuesta:

```json
{
  "id_pedido": 123
}
```

El endpoint de pedidos valida datos obligatorios, limita el tamano del request
y aplica rate limiting para reducir abuso. Si algun sticker no puede venderse,
responde `400 Bad Request` con un mensaje indicando el SKU y el problema.
