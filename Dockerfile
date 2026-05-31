FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY Yaurs/Yaurs.csproj Yaurs/
RUN dotnet restore Yaurs/Yaurs.csproj
COPY Yaurs/ Yaurs/
RUN dotnet publish Yaurs/Yaurs.csproj -c Release -o /publish --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS prod
WORKDIR /app
EXPOSE 5090
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "Yaurs.dll"]
