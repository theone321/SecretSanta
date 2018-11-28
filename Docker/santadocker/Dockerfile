FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY ./container/* ./
ENTRYPOINT ["dotnet", "SecretSanta.dll"]