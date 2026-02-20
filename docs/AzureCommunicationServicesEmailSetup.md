# Azure Communication Services Email Setup Guide

This guide walks you through setting up Azure Communication Services Email for sending OTP (One-Time Password) authentication emails in the Yanitor application.

## Prerequisites

- An active Azure subscription ([Create a free account](https://azure.microsoft.com/free/))
- Owner or Contributor access to an Azure resource group
- Azure CLI installed (optional, for command-line setup)

---

## Step 1: Create an Azure Communication Services Resource

### Using Azure Portal

1. **Navigate to Azure Portal**
   - Go to [https://portal.azure.com](https://portal.azure.com)
   - Sign in with your Azure account

2. **Create Communication Services Resource**
   - Click **Create a resource** or search for "Communication Services"
   - Select **Communication Services** from the results
   - Click **Create**

3. **Configure Basic Settings**
   - **Subscription**: Select your Azure subscription
   - **Resource Group**: Create new or select existing (e.g., `yanitor-resources`)
   - **Resource Name**: Enter a unique name (e.g., `yanitor-communication`)
   - **Data Location**: Choose a region close to your users (e.g., `United States`, `Europe`)
   - Click **Review + create**
   - Click **Create** and wait for deployment to complete

4. **Navigate to Your Resource**
   - Click **Go to resource** when deployment completes
   - Keep this page open for later steps

---

## Step 2: Add Email Communication Service

### Option A: Azure-Managed Domain (Quickest Setup)

1. **Navigate to Email in Communication Services**
   - In your Communication Services resource, locate **Email** in the left menu
   - Click **Email** → **Try Email**
   - Click **Add a free azure subdomain** (provides a `*.azurecomm.net` domain)

2. **Configure Azure Subdomain**
   - **Subdomain**: Enter a unique prefix (e.g., `yanitor-emails`)
   - **Region**: Select same region as Communication Services resource
   - Click **Add**
   - Wait for provisioning (usually 1-2 minutes)

3. **Verify Domain Status**
   - Once provisioned, status should show **Verified**
   - Note the **MailFrom address** (e.g., `DoNotReply@yanitor-emails.azurecomm.net`)

4. **Connect Domain to Communication Services**
   - In your Communication Services resource, go to **Email** → **Domains**
   - Click **Connect domain**
   - Select your Azure-managed domain
   - Click **Connect**

---

### Option B: Custom Domain (Production Setup)

For production environments, use a custom domain (e.g., `noreply@yourdomain.com`):

1. **Add Email Communication Service**
   - In Azure Portal, search for **Email Communication Services**
   - Click **Create**
   - Configure:
     - **Subscription**: Same as Communication Services
     - **Resource Group**: Same as Communication Services
     - **Resource Name**: e.g., `yanitor-email-service`
     - **Region**: Same region as Communication Services
   - Click **Review + create** → **Create**

2. **Add Custom Domain**
   - Navigate to your Email Communication Service resource
   - Click **Provision domains** → **Add domain**
   - Select **Custom domains**
   - Enter your domain name (e.g., `yourdomain.com`)
   - Click **Add**

3. **Verify Domain Ownership**
   - Azure will provide DNS records to add to your domain:
     - **TXT record** for domain verification
     - **SPF record** (TXT) for sender authentication
     - **DKIM records** (CNAME) for email signing
   - Add these records in your DNS provider (e.g., GoDaddy, Cloudflare, Azure DNS)
   - Wait 15-30 minutes for DNS propagation
   - Click **Verify** in Azure Portal
   - Wait for verification status to show **Verified**

4. **Configure MailFrom Address**
   - Once verified, click **MailFrom addresses**
   - Click **Add MailFrom address**
   - Enter a subdomain (e.g., `noreply@yourdomain.com`)
   - Click **Add**

5. **Connect to Communication Services**
   - Navigate back to your Communication Services resource
   - Click **Email** → **Domains** → **Connect domain**
   - Select your custom domain
   - Click **Connect**

---

## Step 3: Get SMTP Credentials

Azure Communication Services Email supports **Azure Email SDK** (recommended) or **SMTP relay** (for compatibility).

### For SMTP Configuration (Used by Yanitor)

1. **Enable SMTP Authentication** (if not using Azure SDK)
   - Azure Communication Services doesn't provide direct SMTP credentials
   - **Alternative 1**: Use Azure Email SDK (requires code changes)
   - **Alternative 2**: Use a third-party SMTP service (SendGrid, Mailjet)
   - **Alternative 3**: Use Microsoft 365 SMTP relay

### Recommended: SendGrid on Azure (Free Tier Available)

1. **Create SendGrid Account**
   - In Azure Portal, search for **SendGrid**
   - Click **Create** and choose the **Free tier** (25,000 emails/month)
   - Configure with your Azure subscription and resource group
   - Click **Review + create** → **Create**

2. **Get SMTP Credentials**
   - Navigate to your SendGrid resource
   - Click **Manage** to open SendGrid portal
   - Go to **Settings** → **API Keys**
   - Click **Create API Key**
   - Select **Full Access** permissions
   - Copy the API key (this is your SMTP password)

3. **Configure SendGrid Domain Authentication**
   - In SendGrid, go to **Settings** → **Sender Authentication**
   - Click **Authenticate Your Domain**
   - Follow the wizard to add DNS records
   - Verify domain ownership

4. **SMTP Settings for SendGrid**
   ```
   SMTP Host: smtp.sendgrid.net
   SMTP Port: 587
   Username: apikey (literally the word "apikey")
   Password: <your-api-key>
   Use SSL: Yes
   ```

---

## Step 4: Update Yanitor Configuration

### appsettings.json (Production)

Update `src/Yanitor.Web/appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "UseSsl": true,
    "Username": "apikey",
    "Password": "",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "Yanitor"
  }
}
```

### appsettings.Development.json (Local Testing)

For local development, use a test SMTP server like **MailHog** or **Papercut SMTP**:

**MailHog** (recommended for macOS/Linux):
```bash
# Install MailHog
brew install mailhog  # macOS
# or download from https://github.com/mailhog/MailHog

# Run MailHog
mailhog
```

**Papercut SMTP** (recommended for Windows):
- Download from [https://github.com/ChangemakerStudios/Papercut-SMTP](https://github.com/ChangemakerStudios/Papercut-SMTP)
- Run `Papercut.exe`
- It will listen on port 25 by default

Configuration for MailHog/Papercut:
```json
{
  "EmailSettings": {
    "SmtpHost": "localhost",
    "SmtpPort": 1025,
    "UseSsl": false,
    "Username": "",
    "Password": "",
    "FromEmail": "noreply@localhost",
    "FromName": "Yanitor Dev"
  }
}
```

---

## Step 5: Store Secrets Securely

**NEVER** commit SMTP passwords or API keys to source control.

### Option 1: User Secrets (Local Development)

```bash
cd src/Yanitor.Web
dotnet user-secrets init
dotnet user-secrets set "EmailSettings:Password" "your-api-key-here"
```

### Option 2: Azure Key Vault (Production)

1. **Create Key Vault**
   ```bash
   az keyvault create \
     --name yanitor-keyvault \
     --resource-group yanitor-resources \
     --location eastus
   ```

2. **Store SMTP Password**
   ```bash
   az keyvault secret set \
     --vault-name yanitor-keyvault \
     --name EmailSettings--Password \
     --value "your-api-key-here"
   ```

3. **Configure App Service to Use Key Vault**
   - In Azure Portal, navigate to your App Service
   - Go to **Settings** → **Configuration**
   - Click **New application setting**
   - Name: `EmailSettings:Password`
   - Value: `@Microsoft.KeyVault(SecretUri=https://yanitor-keyvault.vault.azure.net/secrets/EmailSettings--Password/)`
   - Enable **Managed Identity** for your App Service
   - Grant App Service access to Key Vault:
     ```bash
     az keyvault set-policy \
       --name yanitor-keyvault \
       --object-id <app-service-managed-identity-object-id> \
       --secret-permissions get
     ```

### Option 3: Environment Variables (Docker/Azure App Service)

Set environment variables directly:
```bash
export EmailSettings__Password="your-api-key-here"
```

Or in Azure App Service **Configuration** → **Application settings**

---

## Step 6: Test Email Delivery

### 1. Run the Application
```bash
cd src/Yanitor.Web
dotnet run
```

### 2. Navigate to Sign-In Page
- Open browser to `https://localhost:5001/sign-in` (or your configured port)
- Enter your email address
- Click **Sign In**

### 3. Verify Email Receipt
- **Development**: Check MailHog web UI at `http://localhost:8025` or Papercut inbox
- **Production**: Check your email inbox

### 4. Enter OTP Code
- Copy the 6-digit code from the email
- Enter it on the verification page
- Click **Verify and Sign In**

### 5. Troubleshooting Failed Emails

If emails aren't being sent:

**Check Application Logs**
```bash
# Look for email sending errors
dotnet run | grep -i "email\|smtp"
```

**Common Issues**
- **SMTP authentication failed**: Verify username/password in configuration
- **Connection timed out**: Check firewall rules, ensure port 587 is open
- **TLS/SSL errors**: Verify `UseSsl: true` for port 587 or 465
- **Domain not verified**: Complete domain verification in SendGrid/Azure
- **Rate limiting**: Free tiers have daily limits (SendGrid free: 100/day)

**Test SMTP Connection Manually**
```bash
# Install swaks (SMTP testing tool)
# macOS: brew install swaks
# Linux: apt-get install swaks

swaks --to your-email@example.com \
      --from noreply@yourdomain.com \
      --server smtp.sendgrid.net:587 \
      --auth-user apikey \
      --auth-password your-api-key \
      --tls \
      --header "Subject: Test Email"
```

---

## Step 7: Monitor Email Delivery (Production)

### SendGrid Analytics

1. Navigate to SendGrid portal
2. Go to **Activity** → **Email Activity**
3. View delivery rates, bounces, and errors

### Azure Application Insights

1. Add Application Insights to your App Service
2. Query email logs:
   ```kusto
   traces
   | where message contains "Email"
   | order by timestamp desc
   | take 100
   ```

---

## Cost Considerations

### SendGrid on Azure
- **Free Tier**: 25,000 emails/month
- **Essentials ($19.95/month)**: 100,000 emails/month
- **Pro ($89.95/month)**: Unlimited emails

### Azure Communication Services Email
- **Azure-managed domain**: Free
- **Custom domain**: Free
- **Email sending**: Pay-as-you-go (~$0.25 per 1,000 emails)

### Recommendations
- **Development/Small Projects**: SendGrid Free Tier
- **Medium Projects**: Azure Communication Services (more cost-effective at scale)
- **Enterprise**: Azure Communication Services + Custom Domain

---

## Additional Resources

- [Azure Communication Services Email Overview](https://learn.microsoft.com/azure/communication-services/concepts/email/email-overview)
- [SendGrid Azure Documentation](https://learn.microsoft.com/azure/sendgrid-dotnet-how-to-send-email)
- [ASP.NET Core Email Configuration](https://learn.microsoft.com/aspnet/core/fundamentals/configuration)
- [Azure Key Vault for Secrets](https://learn.microsoft.com/azure/key-vault/general/overview)

---

## Support

For issues with:
- **Yanitor Application**: Open an issue on GitHub
- **Azure Services**: [Azure Support Center](https://azure.microsoft.com/support/)
- **SendGrid**: [SendGrid Support](https://support.sendgrid.com/)
