# Parking Garage API

## Projektről

Ez a projekt egy parkológarázs-kezelő rendszer API-ját implementálja **ASP.NET Core** technológiával. Az API lehetővé teszi a felhasználók regisztrációját, bejelentkezését és kijelentkezését, valamint az autók és parkolóhelyek kezelését.

## Funkciók

- **Felhasználókezelés**: Regisztráció, bejelentkezés, kijelentkezés  
- **Autókezelés**: Autók hozzáadása, listázása, törlése
- **Parkolókezelés**: Parkolóhelyek kezelése, parkolás kezdése és befejezése, parkolási díj számítása
- **Adatbázis**: In-memory adatbázis fejlesztési célokra  
- **Hitelesítés**: Cookie alapú hitelesítés  
- **API Dokumentáció**: Swagger integráció  

## Telepítés és futtatás

1. Clone-olja a repository-t  
2. Nyissa meg a solution-t **Visual Studio 2022-ben**  
3. Építse fel a projektet  
4. Futtassa a projektet, és látogasson el a **Swagger UI-ra**:  
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

### Teszt

- `GET /api/test/userdata` – Bejelentkezett felhasználó adatainak lekérdezése (**védett végpont**)  

## Technológiák

- **ASP.NET Core 8.0**  
- **Entity Framework Core**  
- **Cookie Authentication**  
- **Swagger / OpenAPI**  
