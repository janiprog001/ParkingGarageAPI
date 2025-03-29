# Parking Garage API

## A Projektről

Ez a projekt egy parkológarázs-kezelő rendszer API-ját implementálja **ASP.NET Core** technológiával. Az API lehetővé teszi a felhasználók regisztrációját, bejelentkezését és kijelentkezését, valamint az autók és parkolóhelyek kezelését.

## Funkciók

- **Felhasználókezelés**: Regisztráció, bejelentkezés, kijelentkezés  
- **Autókezelés**: Autók hozzáadása, listázása, törlése
- **Parkolókezelés**: Parkolóhelyek kezelése, parkolás kezdése és befejezése, parkolási díj számítása
- **Admin felület**: a rendszer kezeléséhez 
- **Email** értesítések küldése
- **PDF** számlák generálása
- **Adatbázis**: In-memory adatbázis fejlesztési célokra  
- **Hitelesítés**: Cookie alapú hitelesítés  
- **API Dokumentáció**: Swagger integráció

## Telepítés és futtatás

1. Clone-olja a repository-t  
```bash
git clone https://github.com/yourusername/ParkingGarageAPI.git
```
2. Nyissa meg a solution-t **Visual Studio 2022-ben**  
3. Telepítse a függőségeket:
```bash
dotnet restore
```

4. Állítsa be a környezeti változókat:
```bash
cp .env.example .env
```
Módosítsa a .env fájlt a saját beállításaival.

5. Futtassa az alkalmazást:
```bash
dotnet run
```    
  
6. Látogasson el a **Swagger UI-ra**:  
   [`http://localhost:5025/swagger`](http://localhost:5025/swagger)

## API végpontok

### Felhasználók

- `POST /api/users/register` – Új felhasználó regisztrációja  
- `POST /api/users/login` – Bejelentkezés  
- `POST /api/users/logout` – Kijelentkezés  

### Autók

- `GET /api/cars` - Felhasználó autóinak lekérdezése
- `POST /api/cars` - Új autó hozzáadása
- `DELETE /api/cars/{id}` - Autó törlése

### Parkolás

- `GET /api/parking/spots` - Összes parkolóhely lekérdezése
- `GET /api/parking/spots/available` - Szabad parkolóhelyek lekérdezése
- `POST /api/parking/start` - Parkolás kezdése
- `POST /api/parking/end` - Parkolás befejezése
- `GET /api/parking/my` - Felhasználó parkoló autóinak lekérdezése

### Statisztikák

- `GET /api/statistics/history` - Felhasználó parkolási előzményei
- `GET /api/statistics/summary` - Parkolási összesítő
- `GET /api/statistics/by-car` - Parkolási statisztikák autónként
- `GET /api/statistics/monthly` - Havi parkolási statisztikák

### Admin Statisztikák

- `GET /api/admin/statistics/all-history` - Összes parkolási előzmény
- `GET /api/admin/statistics/revenue` - Bevételi statisztikák
- `GET /api/admin/statistics/occupancy` - Parkolóház kihasználtsági statisztikák
- `GET /api/admin/statistics/user-activity` - Felhasználói aktivitás kimutatás
- `GET /api/admin/statistics/monthly-revenue` - Havi bevételi kimutatás

### Számlázás

- `GET /api/invoices` - Felhasználó számláinak lekérdezése
- `GET /api/invoices/{id}` - Számla részleteinek lekérdezése
- `GET /api/invoices/{id}/download` - Számla letöltése PDF formátumban
- `POST /api/invoices/{id}/resend` - Számla újraküldése emailben (csak admin)
- `PUT /api/invoices/{id}/status` - Számla státuszának módosítása (csak admin)

### Teszt

- `GET /api/test/userdata` – Bejelentkezett felhasználó adatainak lekérdezése (**védett végpont**)  

## Technológiák

- **ASP.NET Core 8.0**  
- **Entity Framework Core**  
- **MySQL (Aiven)**
- **Cookie Authentication**  
- **Swagger / OpenAPI**  
- **JWT Authentication**
- **iTextSharp (PDF generálás)**
- **SMTP (Email küldés)**

## Környezeti változók

Az alkalmazás a következő környezeti változókat használja:

- `MYSQL_HOST`: MySQL szerver címe
- `MYSQL_PORT`: MySQL port
- `MYSQL_DATABASE`: Adatbázis neve
- `MYSQL_USER`: Adatbázis felhasználónév
- `MYSQL_PASSWORD`: Adatbázis jelszó
- `MYSQL_SSL_MODE`: SSL mód (REQUIRED/OPTIONAL)

## API Dokumentáció

Az API dokumentáció elérhető a Swagger UI-on:
```
http://localhost:5025/swagger
```

## Deployment

Az alkalmazás sikeresen deployolva van a következő platformokon:

- Backend: Render (https://parking-garage.onrender.com)
- Frontend: Netlify (https://parking-garage-app.netlify.app)
- Adatbázis: Aiven MySQL

<!-- A deployment sikeresen működik, és minden szolgáltatás megfelelően kommunikál egymással:
- A Render-en futó API sikeresen kapcsolódik az Aiven MySQL adatbázishoz
- A Netlify-on futó frontend sikeresen kommunikál a Render-en futó API-val
- A CORS beállítások megfelelően konfigurálva vannak a különböző domainek közötti kommunikációhoz

## Fejlesztői környezet

A fejlesztői környezet beállításához:

1. Telepítse a .NET 8 SDK-t
2. Telepítse a MySQL-t lokálisan vagy használjon egy távoli MySQL szervert
3. Állítsa be a környezeti változókat
4. Futtassa az alkalmazást a `dotnet run` paranccsal -->

## Licensz

MIT
