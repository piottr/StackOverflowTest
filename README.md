# StackOverflowTest

Projekt API do synchronizacji tagów ze Stack Overflow.

## Struktura projektu

- `src/API/` - Główny projekt ASP.NET Core API
- `frontend/` - Aplikacja Angular z PrimeNG
- `tests/API.Tests/` - Testy jednostkowe
- `docker-compose.yml` - Konfiguracja Docker dla bazy danych i API

## Uruchamianie

### Z Docker Compose (zalecane):
`docker-compose up --build`

Frontend będzie dostępny na `http://localhost:4200`, API na `http://localhost:8080`.

### Lokalnie:
1. Uruchom bazę danych: `docker-compose up db`
2. Uruchom API: `dotnet run --project src/API`
3. Uruchom frontend: `cd frontend && npm install && npm start`

## Testy

`dotnet test tests/API.Tests/`

## Migracje

`dotnet ef database update --project src/API`