# Parking Garage API

## Projektru0151l

Ez a projekt egy parkolu00f3garu00e1zs kezelu0151 rendszer API-ju00e1t implementu00e1lja ASP.NET Core technolu00f3giu00e1val. Az API lehetu0151vu00e9 teszi a felhasznu00e1lu00f3k regisztru00e1ciu00f3ju00e1t, bejelentkezu00e9su00e9t u00e9s kijelentkezu00e9su00e9t, valamint az autu00f3k kezelu00e9su00e9t.

## Funkciu00f3k

- **Felhasznu00e1lu00f3kezelu00e9s**: Regisztru00e1ciu00f3, bejelentkezu00e9s, kijelentkezu00e9s
- **Autu00f3kezelu00e9s**: Autu00f3k hozzu00e1adu00e1sa, listu00e1zu00e1sa
- **Adatbu00e1zis**: In-memory adatbu00e1zis fejlesztu00e9si cu00e9lokra
- **Hitelesu00edtu00e9s**: Cookie alapu00fa hitelesu00edtu00e9s
- **API Dokumentu00e1ciu00f3**: Swagger integru00e1ciu00f3

## Telepu00edtu00e9s u00e9s futtatu00e1s

1. Clone-olja a repository-t
2. Nyissa meg a solution-t Visual Studio 2022-ben
3. u00c9pu00edtse fel a projektet
4. Futtassa a projektet, u00e9s lu00e1togasson el a Swagger UI-ra: `http://localhost:5000/swagger`

## API vu00e9gpontok

### Felhasznu00e1lu00f3k

- `POST /api/users/register` - u00daj felhasznu00e1lu00f3 regisztru00e1ciu00f3ja
- `POST /api/users/login` - Bejelentkezu00e9s
- `POST /api/users/logout` - Kijelentkezu00e9s

### Teszt

- `GET /api/test/userdata` - Bejelentkezett felhasznu00e1lu00f3 adatainak leku00e9rdezu00e9se (vu00e9dett vu00e9gpont)

## Technolu00f3giu00e1k

- ASP.NET Core 8.0
- Entity Framework Core
- Cookie Authentication
- Swagger / OpenAPI
