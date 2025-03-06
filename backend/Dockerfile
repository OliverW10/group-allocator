FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:1025bed126a7b85c56b960215ab42a99db97a319a72b5d902383ebf6c6e62bbe AS build
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine@sha256:36b581c9812089fa977395f6951cd08064b10da98b45653ea43e75b8e247e8fa
WORKDIR /App
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "GroupAllocator.dll"]

# Base aspnet image sets ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
