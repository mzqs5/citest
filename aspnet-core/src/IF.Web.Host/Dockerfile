FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
COPY /IF.Web.Host .
ENTRYPOINT ["dotnet", "IF.Web.Host.dll"]