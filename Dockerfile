# 🟢 ដំណាក់កាលទី ១: Build កូដ
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# ចម្លងឯកសារ csproj រួច Restore package
COPY ["PhumKasikam.csproj", "./"]
RUN dotnet restore "./PhumKasikam.csproj"

# ចម្លងកូដទាំងអស់ រួច Build
COPY . .
RUN dotnet publish "PhumKasikam.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 🟢 ដំណាក់កាលទី ២: រត់នៅលើ Production Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# បើក Port ឱ្យត្រូវនឹងប្រព័ន្ធ Render
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "PhumKasikam.dll"]