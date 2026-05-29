# Deployment

## Docker Compose

```bash
docker-compose up
```

Access:
- Frontend: http://localhost
- API: http://localhost:8080/swagger
- SQL Server: localhost:1433

## Production Notes

- Set `appsettings.Production.json` connection strings and JWT key via environment variables
- Never use dev secrets in production