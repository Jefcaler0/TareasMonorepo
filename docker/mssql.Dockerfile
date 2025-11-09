FROM mcr.microsoft.com/mssql/server:2022-latest

ENV MSSQL_PID=Developer
ENV ACCEPT_EULA=Y
ENV MSSQL_ENABLE_HADR=0

# Values can be overridden via environment variables when running the container.
ENV SA_PASSWORD=YourStrong!Passw0rd
ENV MSSQL_TCP_PORT=1433

EXPOSE 1433

# Create a directory for optional initialization scripts.
RUN mkdir -p /var/opt/mssql/scripts
VOLUME ["/var/opt/mssql/scripts"]

# Start SQL Server when the container launches.
CMD ["/opt/mssql/bin/sqlservr"]

