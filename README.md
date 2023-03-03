# PLANTILLA BRIDGE SMS

Esta plantilla esta realizada en base al formulario 

## OBJETOS RELACIONADOS AL PROVEEDOR:

Se debe acondicionar estos cs segun las estructuras definidas por el proovedor de SMS.

1. Model/SMSEntrante.cs
2. Model/SMSNotificacion.cs
3. Model/SMSSaliente.cs

Actualizados los objetos del proveedor se debe modificar los archivos controladores:

1. Controllers/IncomingController.cs
2. Controllers/NotificationController.cs
3. Controllers/SendController.cs

Finalmente tambien se debe actualizar los datos del appsettings.json

Actualizar datos de base de datos:

```
  "BDIP": "PEOLIM015",
  "BDusr": "sa",
  "BDpss": "inc2001",
  "BDName": "BD_TEST",
```

Actualizar datos del proveedor:

```
  "urlSMSProveedor": "https://jdegdk.api.infobip.com/sms/2/text/advanced",
  "token": "App 2ea44d9b299ee8c80b827940080e281b-3c511c84-18dc-42dc-a47b-da17ef9f3cca",
```

Actualizar datos de las url de los webHook:

```
  "urlWebHookOCC": "http://sms-i6.abinbev-las.com/webhooks/new_messages/SMS_campcxsms@cbn_76C70BABD22DB33F022F825B028B8583",
  "urlWebHookNotiOCC": "http://sms-i6.abinbev-las.com/webhooks/messages_status/SMS_campcxsms@cbn_76C70BABD22DB33F022F825B028B8583",
```
