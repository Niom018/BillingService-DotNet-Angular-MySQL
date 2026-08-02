#!/bin/bash
set -e
exec > /var/log/billing-bootstrap.log 2>&1

echo "=== Updating system ==="
apt-get update -y
apt-get upgrade -y

echo "=== Installing nginx ==="
apt-get install -y nginx

echo "=== Installing MySQL server ==="
apt-get install -y mysql-server
systemctl enable mysql
systemctl start mysql

echo "=== Configuring MySQL ==="
mysql -e "ALTER USER 'root'@'localhost' IDENTIFIED WITH mysql_native_password BY '${mysql_root_password}';"
mysql -u root -p'${mysql_root_password}' -e "CREATE DATABASE IF NOT EXISTS billing_service;"
mysql -u root -p'${mysql_root_password}' -e "FLUSH PRIVILEGES;"

echo "=== Installing .NET 10 ASP.NET Core runtime ==="
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
dpkg -i /tmp/packages-microsoft-prod.deb
rm /tmp/packages-microsoft-prod.deb
apt-get update -y
apt-get install -y aspnetcore-runtime-10.0

echo "=== Preparing app directories ==="
mkdir -p /var/www/billing-api
mkdir -p /var/www/billing-ui
chown -R www-data:www-data /var/www/billing-api /var/www/billing-ui

echo "=== Writing systemd service (will start once you upload the app) ==="
cat > /etc/systemd/system/billing-api.service << 'SERVICE_EOF'
[Unit]
Description=Billing Service API
After=network.target mysql.service

[Service]
WorkingDirectory=/var/www/billing-api
ExecStart=/usr/bin/dotnet /var/www/billing-api/BillingService.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=billing-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
SERVICE_EOF

systemctl daemon-reload

echo "=== Configuring nginx ==="
cat > /etc/nginx/sites-available/billing-service << 'NGINX_EOF'
server {
    listen 80;
    server_name _;

    root /var/www/billing-ui;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /health {
        proxy_pass http://localhost:5000/health;
    }

    location /swagger/ {
        proxy_pass http://localhost:5000/swagger/;
    }
}
NGINX_EOF

rm -f /etc/nginx/sites-enabled/default
ln -sf /etc/nginx/sites-available/billing-service /etc/nginx/sites-enabled/billing-service
systemctl restart nginx

echo "=== Bootstrap complete ==="
