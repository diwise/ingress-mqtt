FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /app
COPY *.sln .
COPY Masarin.IoT.Sensor/*.csproj ./Masarin.IoT.Sensor/
COPY Masarin.IoT.Sensor.Tests/*.csproj ./Masarin.IoT.Sensor.Tests/

WORKDIR /app/Masarin.IoT.Sensor/
RUN dotnet restore

WORKDIR /app/Masarin.IoT.Sensor.Tests/
RUN dotnet restore

WORKDIR /app

# copy full solution over
COPY . .
RUN dotnet build

# run the unit tests
WORKDIR /app/Masarin.IoT.Sensor.Tests
RUN dotnet test --logger:trx

# publish the API
WORKDIR /app/Masarin.IoT.Sensor/
RUN dotnet publish -c Release -o out

# run the api
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/Masarin.IoT.Sensor/out ./
EXPOSE 80
ENTRYPOINT ["dotnet", "Masarin.IoT.Sensor.dll"]
