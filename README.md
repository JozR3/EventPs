# 📌 EventoPS - Sistema de gestion de eventos

## 📖 Descripción
El proyecto fue realizado (hasta el momento) en el entorno de desarrollo integrado (IDE) de Visual Studio y no se utilizo un software externo para la compilacion de proyecto.

---

## 🛠️ Tecnologías utilizadas

- **C#** → Back End  
- **HTML + CSS** → Front End  
- **JavaScript** → Lógica de interacción (Front y Back)  
- **SQL Server** → Base de datos relacional  

---

## 🧱 Arquitectura del Proyecto

El sistema está organizado en múltiples capas:

- **Domain** → Entidades del negocio  
- **Application** → Lógica de aplicación  
- **Infrastructure** → Acceso a datos  
- **EventoPS** → Capa principal (API + Front End)  

---

## ⚙️ Detalles del Software utilizado

- Visual Studio  
- SQL Server Management Studio
- Framework .NET 8.0 

---

## 🗄️ Configuración de Base de Datos

El sistema utiliza **SQL Server**, asi que, para levantar/crear la base de datos correctamente se debe modificar los caracteres del "ConnectionString" del archivo "appsetting.json"

📍 Ubicación del archivo: ***EventoPS/appsettings.json***

<img width="346" height="472" alt="image" src="https://github.com/user-attachments/assets/22f975f6-e2a7-4849-b55a-fc86ac2b8f79" />


### 🔧 Configuración del Connection String

Se debe modificar el User Id y Password:

```json
"ConnectionString": "Server=localhost;Database=EventoPs;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True;"

```
---
## 🔄 Migración
Con la ejecucion del programa ya se realiza la migracion automaticamente con el comando context.Database.Migrate() o puede optar por escribirla en la consola del Administrador de paquetes con el comando add-migration init (El programa ya posee el paquete necesario para aceptar el comando).

---

## ▶️ Ejecución 
Para la ejecucion del programa se debe tener en cuenta que el perfil de lanzamiento de Visual Studio sea "http" para que el front end se vea correctamente.

<img width="727" height="543" alt="image" src="https://github.com/user-attachments/assets/3d4f4ba4-3cc0-4013-92cc-2ee3ac98713d" />

---

## 📌 Consideraciones 
Tenga en cuenta que el programa puede sufrir modificaciones futuras que cambien el aspecto del Front end y difieran del aspecto que tendra en la entrega 2. 
No obstante, se informa que el primer repositorio subido contiene los puntos minimos para la primera entrega, pero ya se comenzo con los preparativos para la segunda entrega, por lo que puede que ciertas partes del codigo se encuentren "sin terminar" o "sin sentido" que ya corresponden a preliminares de funciones futuras.

## Autores
- Ruiz Jose
- Tomas Bandiera
