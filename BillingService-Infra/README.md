# Deploying BillingService to AWS

Single EC2 instance running nginx (reverse proxy + static Angular files),
your .NET API (systemd service on localhost:5000), and MySQL - all on one
free-tier-eligible box. No RDS, no load balancer, cheapest possible while
still being a real deployment.

## 0. One-time prerequisites

- AWS CLI configured (`aws sts get-caller-identity` should print your account)
- [Terraform](https://developer.hashicorp.com/terraform/install) installed (`terraform version`)
- An EC2 key pair in `eu-north-1` (create one if you don't have one):
  ```powershell
  aws ec2 create-key-pair --region eu-north-1 --key-name billing-service-key --query "KeyMaterial" --output text | Out-File -Encoding ascii billing-service-key.pem
  ```
  Keep this `.pem` file safe - you'll need it to SSH in. Don't commit it.

## 1. Provision the server

```powershell
cd BillingService-Infra
cp terraform.tfvars.example terraform.tfvars
```
Edit `terraform.tfvars`: set `key_pair_name` to the key pair you just made,
and pick a real `mysql_root_password`.

```powershell
terraform init
terraform apply
```
Type `yes` when prompted. Takes a few minutes (EC2 boot + the bootstrap
script installing nginx/MySQL/.NET runtime). Note the `public_ip` output -
that's your live URL.

Give the bootstrap script another minute or two after `apply` finishes
before moving on - it's still installing things in the background. You can
watch it finish by SSHing in and running `tail -f /var/log/billing-bootstrap.log`
(look for "Bootstrap complete").

## 2. Build your app locally

**Backend:**
```powershell
cd ..\BillingService\src\BillingService.Api
dotnet publish -c Release -o .\publish
```

**Create `appsettings.Production.json`** in that same folder (gitignored,
just like your Development one) with real production values:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=billing_service;user=root;password=SAME_PASSWORD_AS_TERRAFORM_TFVARS"
  },
  "Jwt": {
    "Key": "a-different-long-random-32-plus-character-secret"
  },
  "SeedAdmin": {
    "Email": "admin@billingservice.local",
    "Password": "Admin@12345"
  }
}
```
Copy it into the publish folder too: `copy appsettings.Production.json publish\`

**Frontend:**
```powershell
cd ..\..\billing-ui
```
Edit `src/environments/environment.ts` - change `apiUrl` to `/api` (relative,
since nginx now serves both from the same origin):
```typescript
export const environment = {
  production: true,
  apiUrl: '/api'
};
```
```powershell
npm run build -- --configuration production
```

## 3. Upload to the server

Replace `<PUBLIC_IP>` with your Terraform output, and the path to your
`.pem` key:

```powershell
scp -i billing-service-key.pem -r .\BillingService\src\BillingService.Api\publish\* ubuntu@<PUBLIC_IP>:/tmp/billing-api/
scp -i billing-service-key.pem -r .\billing-ui\dist\billing-ui\browser\* ubuntu@<PUBLIC_IP>:/tmp/billing-ui/
```

## 4. Set up the database and start everything

Generate a migration SQL script locally (keeps MySQL's port closed to the
internet - no need to expose 3306 publicly):
```powershell
cd BillingService\src\BillingService.Api
dotnet ef migrations script --idempotent -o migrate.sql --project ..\BillingService.Infrastructure --startup-project .
scp -i ..\..\..\billing-service-key.pem migrate.sql ubuntu@<PUBLIC_IP>:/tmp/migrate.sql
```

SSH into the server:
```powershell
ssh -i billing-service-key.pem ubuntu@<PUBLIC_IP>
```

Then, on the server:
```bash
sudo mv /tmp/billing-api/* /var/www/billing-api/
sudo mv /tmp/billing-ui/* /var/www/billing-ui/
sudo chown -R www-data:www-data /var/www/billing-api /var/www/billing-ui

# Apply the database schema
mysql -u root -p'YOUR_MYSQL_PASSWORD' billing_service < /tmp/migrate.sql

# Start the API
sudo systemctl enable billing-api
sudo systemctl start billing-api
sudo systemctl status billing-api   # confirm it says "active (running)"
```

## 5. Verify

Open `http://<PUBLIC_IP>` in your browser - you should see your Angular
login page, served live from AWS. Log in with your seeded admin and click
through the whole flow like you did locally.

If something's not loading, check:
```bash
sudo journalctl -u billing-api -n 50   # API logs
sudo tail -50 /var/log/nginx/error.log # nginx logs
```

## Tearing it down

When you're done demoing (or to avoid any charges), destroy everything:
```powershell
cd BillingService-Infra
terraform destroy
```
