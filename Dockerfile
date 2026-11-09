# ---------- Stage 1: build the app ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project file and restore the packages first.
# Docker remembers this step, so rebuilding after a code change is much faster.
COPY RaceDay.Web.csproj ./
RUN dotnet restore

# Now copy the rest of the source code and publish a Release build
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Stage 2: the small image that actually runs the app ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Bring over only the published files from stage 1 (the big SDK is left behind)
COPY --from=build /app/publish .

# The app listens on port 8080 inside the container
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Run as a normal user instead of root (safer)
USER $APP_UID

ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]