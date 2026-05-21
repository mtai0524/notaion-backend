FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/src/NotaionWebApp/Notaion"
RUN dotnet restore "Notaion.csproj"
RUN dotnet build "Notaion.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/NotaionWebApp/Notaion"
RUN dotnet publish "Notaion.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notaion.dll"]