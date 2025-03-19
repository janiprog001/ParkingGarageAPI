# Parking Garage API

## Projektről

Ez a projekt egy parkológarázs-kezelő rendszer API-ját implementálja **ASP.NET Core** technológiával. Az API lehetővé teszi a felhasználók regisztrációját, bejelentkezését és kijelentkezését, valamint az autók kezelését.

## Funkciók

- **Felhasználókezelés**: Regisztráció, bejelentkezés, kijelentkezés  
- **Autókezelés**: Autók hozzáadása, listázása  
- **Adatbázis**: In-memory adatbázis fejlesztési célokra  
- **Hitelesítés**: Cookie alapú hitelesítés  
- **API Dokumentáció**: Swagger integráció  

## Telepítés és futtatás

1. Clone-olja a repository-t  
2. Nyissa meg a solution-t **Visual Studio 2022-ben**  
3. Építse fel a projektet  
4. Futtassa a projektet, és látogasson el a **Swagger UI-ra**:  
   [`http://localhost:5000/swagger`](http://localhost:5000/swagger)  

## API végpontok

### Felhasználók

- `POST /api/users/register` – Új felhasználó regisztrációja  
- `POST /api/users/login` – Bejelentkezés  
- `POST /api/users/logout` – Kijelentkezés  

### Teszt

- `GET /api/test/userdata` – Bejelentkezett felhasználó adatainak lekérdezése (**védett végpont**)  

## Technológiák

- **ASP.NET Core 8.0**  
- **Entity Framework Core**  
- **Cookie Authentication**  
- **Swagger / OpenAPI**  
