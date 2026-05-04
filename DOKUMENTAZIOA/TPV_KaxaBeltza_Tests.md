# TPV - Test de caja negra

## Objetivo

Validar el TPV desde el comportamiento visible del usuario, sin depender de la implementacion interna.

## Casos

| ID | Funcion | Entrada / accion | Resultado esperado |
| --- | --- | --- | --- |
| TPV-BB-01 | Login | Usuario y contrasena validos | Entra al menu principal y se muestra el logo de Musinelli correctamente. |
| TPV-BB-02 | Login | Credenciales incorrectas | Se muestra error y no se entra al TPV. |
| TPV-BB-03 | Eskaerak - productos | Abrir una mesa y pulsar un producto con stock | El producto aparece al momento en "Momentuko eskaera" y el total sube. |
| TPV-BB-04 | Eskaerak - platos | Abrir la pestana "Platerak" | Cada plato aparece con su imagen cargada desde la API. |
| TPV-BB-05 | Quitar linea | Pulsar el boton de quitar en una linea | La linea desaparece y el total baja. |
| TPV-BB-06 | Guardar pedido | Guardar una mesa con productos/platos | El pedido se guarda, aparece en "Azken eskaerak" y el stock se descuenta en la DB. |
| TPV-BB-07 | Stock insuficiente | Pedir mas unidades que el stock disponible | Se muestra aviso de stock insuficiente y no se guarda el pedido. |
| TPV-BB-08 | Descuentos | Pulsar "Deskontuak" | Se abre una ventana para introducir el codigo de Odoo. |
| TPV-BB-09 | Descuento valido | Introducir un codigo valido | El total cambia y se muestra el descuento aplicado. |
| TPV-BB-10 | Descuento invalido | Introducir un codigo no valido | Se muestra error y el total no se modifica. |
| TPV-BB-11 | Pago | Seleccionar un servicio no pagado y pulsar "Ordaindu" | El servicio queda pagado, se genera factura y no se puede editar. |
| TPV-BB-12 | Edicion de pedido | Doble clic en un servicio no pagado | Sus lineas vuelven al pedido actual y se puede actualizar. |

## Evidencia de ejecucion

Estos casos quedan preparados para ejecutarse manualmente durante la defensa. Las comprobaciones de stock se verifican tambien en Gerente, cuya tabla de productos refresca automaticamente cada 3 segundos.
