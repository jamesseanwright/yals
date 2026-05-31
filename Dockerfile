FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY Yals/Yals.csproj Yals/
RUN dotnet restore Yals/Yals.csproj
COPY Yals/ Yals/
RUN dotnet publish Yals/Yals.csproj -c Release -o /publish --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS prod
WORKDIR /app
EXPOSE 5090
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "Yals.dll"]
