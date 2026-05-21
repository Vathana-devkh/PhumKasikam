FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PhumKasikam.csproj", "./"]
RUN dotnet restore "./PhumKasikam.csproj"
COPY . .
RUN dotnet publish "PhumKasikam.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "PhumKasikam.dll"]