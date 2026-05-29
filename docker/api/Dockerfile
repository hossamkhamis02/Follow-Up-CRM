FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/FollowUpCrm.Api/FollowUpCrm.Api.csproj", "FollowUpCrm.Api/"]
COPY ["src/FollowUpCrm.Infrastructure/FollowUpCrm.Infrastructure.csproj", "FollowUpCrm.Infrastructure/"]
COPY ["src/FollowUpCrm.Shared/FollowUpCrm.Shared.csproj", "FollowUpCrm.Shared/"]

RUN dotnet restore "FollowUpCrm.Api/FollowUpCrm.Api.csproj"

COPY src/ .
RUN dotnet publish "FollowUpCrm.Api/FollowUpCrm.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FollowUpCrm.Api.dll"]