# Azure Storage Account
- the backbone of many Azure services like Virtual Machines, Functions, Event Grid, Event Hubs (capture), and more.
- An Azure Storage Account is a secure cloud-based container that holds all your Azure storage data objects:
  - Blobs (files, media, data)
  - Queues (messages)
  - Tables (NoSQL data)
  - Files (SMB shares)
  - Disks (VM storage) 
- It provides a unique namespace in Azure for storing and accessing data over the internet using HTTP/HTTPS and REST APIs.
- Each storage account:
  -  Has a unique name and endpoint:
    https://<yourstorageaccount>.blob.core.windows.net
  - Is highly available, durable, and scalable to exabytes of data.
 
# Core Components of Azure Storage
| **Service** | **Description** | **Typical Use Case** |
|--------------|-----------------|----------------------|
| **Blob Storage** | Stores unstructured data (text, images, videos, backups). | Store large files, logs, or static website content. |
| **File Storage** | SMB file shares accessible via the cloud or on-prem. | Lift-and-shift applications, shared drives, Azure Files with AD auth. |
| **Queue Storage** | Simple message queue system for decoupling components. | Asynchronous background task communication. |
| **Table Storage** | NoSQL key-value data store. | Lightweight structured data, logs, IoT metadata. |
| **Disk Storage** | Persistent storage for Azure VMs. | OS and data disks for virtual machines. |

# Storage Account Types:
| **Type** | **Supports** | **Performance** | **Use Case** |
|-----------|---------------|------------------|---------------|
| **General-purpose v2 (GPv2)** | Blobs, Files, Queues, Tables, Disks | Standard/Premium | ✅ Recommended for most workloads |
| **Blob Storage Account** | Only blobs (block, append, page) | Standard/Premium | Data lake, object storage |
| **FileStorage Account** | Only file shares | Premium | Low-latency file shares |
| **BlockBlobStorage Account** | Only block blobs | Premium SSD-backed | High-performance object storage |
| **Premium Storage (GPv1)** | Legacy | Premium | Older VMs, deprecated for new accounts |

# Performance Tiers
| **Tier** | **Description** | **Best for** |
|-----------|----------------|--------------|
| **Hot** | Frequently accessed data | Real-time apps, active files |
| **Cool** | Infrequently accessed (≥30 days) | Backups, medium-term storage |
| **Archive** | Rarely accessed (≥180 days) | Long-term archival, compliance |

# Redundancy (Replication) Options:
| **Type** | **Description** | **Durability** | **Geo-Replication** |
|-----------|----------------|----------------|----------------------|
| **LRS (Locally Redundant Storage)** | Copies data 3× in a single datacenter | 99.999999999% (11 nines) | ❌ No |
| **ZRS (Zone Redundant Storage)** | Copies data across 3 availability zones | 99.9999999999% (12 nines) | ❌ No |
| **GRS (Geo-Redundant Storage)** | LRS + replication to secondary region | 99.99999999999999% (16 nines) | ✅ Yes |
| **GZRS (Geo-Zone Redundant Storage)** | ZRS + replication to secondary region | 99.99999999999999% | ✅ Yes |
| **RA-GRS / RA-GZRS** | Adds read access to secondary region | Same | ✅ Yes (read secondary) |


💡 Tip:

- Use ZRS for high availability within a region.

- Use GZRS for mission-critical, multi-region durability.

  ## 🔒 Security Features

| **Feature** | **Description** |
|--------------|-----------------|
| **Encryption at rest** | Data encrypted using AES-256 (default). |
| **Encryption in transit** | HTTPS required for all operations. |
| **Private endpoints** | Access via private IP in your VNet. |
| **Azure AD Authentication** | RBAC-based identity access control. |
| **Shared Access Signatures (SAS)** | Time-limited access tokens for fine-grained permissions. |
| **Firewalls & IP restrictions** | Restrict access by IP or subnet. |
| **Managed Keys** | Use Microsoft-managed or customer-managed keys (CMK). |

---

## 🧩 Important Concepts

- **Storage Tiers (Hot, Cool, Archive):** Manage cost vs. performance tradeoff  
- **Redundancy Options (LRS, ZRS, GRS, GZRS):** Choose data durability & replication scope  
- **SAS Tokens:** Provide temporary secure access to storage resources  
- **Access Policies:** Automate data lifecycle and retention rules  
- **Blob Versioning & Soft Delete:** Protect against accidental deletions  
- **Data Lake Gen2:** Adds hierarchical namespace for big data analytics

---

## ✅ Summary

| **Category** | **Details** |
|---------------|-------------|
| **Purpose** | Unified platform for object, file, queue, table, and disk storage |
| **Core Types** | Blob, File, Queue, Table, Disk |
| **Performance Tiers** | Hot, Cool, Archive |
| **Redundancy Options** | LRS, ZRS, GRS, GZRS, RA-GZRS |
| **Security** | Encryption, SAS, Private Endpoints, RBAC |
| **Best for** | Cloud-native storage, backups, big data, app integration |

# Sample Code:
```csharp
  using Azure.Storage.Blobs;

  var connectionString = "<your_connection_string>";
  var containerName = "mycontainer";

  BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
  BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

  await containerClient.CreateIfNotExistsAsync();
  Console.WriteLine("✅ Container created!");
```

## Append vs Block vs Page:

# 🎥 Uploading a Video File — Azure Blob Types Explained

When uploading a video to Azure Blob Storage, you can choose between **Block Blob**, **Append Blob**, and **Page Blob**. Each has a different purpose and internal behavior.

---

## 🧱 1️⃣ Block Blob — (✅ Recommended for Video Uploads)

### 💡 How It Works
- The **video is split into chunks** called **blocks**.
- Each block is uploaded individually (can be done in parallel).
- Once all blocks are uploaded, you send a **Commit Block List** request to assemble the final blob.
- You can **replace individual blocks** later if you need to update or resume uploads.

### ⚙️ Example Flow
```
[myvideo.mp4]
   ↓ (split into)
[Block 1] [Block 2] [Block 3] ... [Block N]
   ↓ (upload in parallel)
Commit → Final Video Blob (myvideo.mp4)
```

### ✅ Best For
- Uploading large video files (multi-GB)
- Streaming downloads (video hosting)
- Updating partial files (resume upload support)

### 💡 Key Benefits
- Parallel upload = faster performance  
- Reliable and resumable (can retry failed blocks)  
- Commonly used for **media content**, **backups**, and **static websites**

---

## 📜 2️⃣ Append Blob — (🚫 Not ideal for video files)

### 💡 How It Works
- You **can only append data** to the blob.
- You **cannot modify or replace existing blocks** once appended.
- Each upload **adds data at the end**, like appending to a log file.

### ⚙️ Example Flow
```
AppendBlob myvideo.log
   ↓
Append(Block 1)
Append(Block 2)
Append(Block 3)
```

If you try to update or resume in the middle (say Block 2 failed),  
you can’t replace it — you’d have to **recreate the blob** from scratch.

### ❌ Problems for Video Files
- No random or partial updates allowed.  
- No parallel block upload support.  
- Can’t “resume” from failed upload point — must start over.  

### ✅ Best For
- Log files (application logs, telemetry streams)  
- Continuous write scenarios (e.g., diagnostic logs)

---

## 📄 3️⃣ Page Blob — (⚙️ Technically possible but inefficient for video)

### 💡 How It Works
- Divides data into **512-byte pages**.
- Each page can be written or updated independently (random read/write access).
- Used for **frequent partial updates**, not sequential uploads.

### ⚙️ Example Flow
```
Page Blob (8 TB max)
   ├── Page 0: bytes 0–511
   ├── Page 1: bytes 512–1023
   ├── Page 2: bytes 1024–1535
   └── ...
```

### ❌ Not Recommended For
- Large sequential uploads (like videos, archives)
- Streaming media (no sequential optimization)
- Public content delivery

### ✅ Best For
- Azure VM disks (`.vhd`)
- Databases with random I/O
- Applications that need frequent partial writes

---

## 🧠 Comparison Table

| **Feature** | **Block Blob** | **Append Blob** | **Page Blob** |
|--------------|----------------|-----------------|----------------|
| **Purpose** | General-purpose object storage | Append-only data (logs) | Random read/write (VM disks) |
| **Data structure** | Data stored in blocks | Data appended as blocks | Data stored in 512-byte pages |
| **Write operations** | Upload blocks, then commit | Append data to end only | Write at specific offsets |
| **Modify existing data** | ✅ Yes (replace blocks) | ❌ No | ✅ Yes (by page range) |
| **Max size** | ~200 GB (Standard), larger in Premium | 195 GB | 8 TB |
| **Performance tier** | Standard/Premium | Standard | Premium (SSD-backed) |
| **Common usage** | Files, backups, media | Logs, telemetry, append-only data | VM disks, databases |
| **Access pattern** | Sequential or random | Sequential (append-only) | Random read/write |

---

## 🎬 Practical Scenario: Uploading a 2 GB Video

| Step | **Block Blob** | **Append Blob** | **Page Blob** |
|------|----------------|-----------------|----------------|
| **Upload** | Split into multiple blocks and upload in parallel | Append one block at a time sequentially | Write 512-byte aligned pages sequentially |
| **Speed** | 🚀 Fast (parallel upload supported) | 🐢 Slow (append-only) | 🐌 Very slow (page writes) |
| **Resumable Upload** | ✅ Yes (retry specific blocks) | ❌ No (must restart) | ⚙️ Possible but complex |
| **Cost Efficiency** | ✅ Optimized | ⚠️ Acceptable for small files | ❌ Expensive for large sequential data |
| **Use Case Fit** | 🎥 Perfect for large video files | 🚫 Not suitable | 🚫 Not suitable |

---

## ✅ Best Practice Recommendation

👉 **Always use Block Blob for videos, images, large files, or backups.**

Because:
- It supports **parallelism**, **resumability**, and **optimized throughput**.
- You can **upload partially**, **retry**, and **replace blocks** without re-uploading the entire file.
- It’s natively integrated with **Azure CDN** for video delivery.

---

## ⚙️ Example C# Upload for Video (Block Blob)

```csharp
using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string connectionString = "<STORAGE_CONNECTION_STRING>";
        string containerName = "videos";
        string blobName = "myvideo.mp4";
        string filePath = "C:\Videos\myvideo.mp4";

        BlobServiceClient service = new BlobServiceClient(connectionString);
        BlobContainerClient container = service.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync();

        BlobClient blobClient = container.GetBlobClient(blobName);

        Console.WriteLine("Uploading video...");
        await blobClient.UploadAsync(filePath, overwrite: true);
        Console.WriteLine("✅ Upload complete (Block Blob used).");
    }
}
```

---

## 🧾 TL;DR Summary

| **Blob Type** | **Best For** | **Why / Behavior** |
|----------------|--------------|--------------------|
| **Block Blob** | Videos, large files | Split into blocks, parallel upload, resumable |
| **Append Blob** | Logs, telemetry | Append-only, no modification |
| **Page Blob** | VM disks | Random I/O, 512-byte pages |


<img width="1910" height="872" alt="image" src="https://github.com/user-attachments/assets/eaecc30d-dd76-4eef-9ccc-57a8cbc0b8bb" />

<img width="1897" height="925" alt="image" src="https://github.com/user-attachments/assets/5e86e341-0bda-479a-b537-ac4821728f44" />

<img width="1820" height="858" alt="image" src="https://github.com/user-attachments/assets/d56d9f06-707a-4798-9be1-bae8cdecf601" />

<img width="1836" height="872" alt="image" src="https://github.com/user-attachments/assets/4d3a45ff-dff6-42fb-8100-6301ccc2057c" />



# 🌐 Deploying a Static Website in Azure Storage Account

Hosting a **static website** in Azure Storage is one of the simplest and most cost-effective ways to deploy frontend apps like **HTML, CSS, JavaScript, React, Angular, or Vue** — with global availability and no servers to manage.

---

## ☁️ What Is a Static Website in Azure Storage?

A **static website** in Azure Storage serves static content (HTML, CSS, JS, images) directly from a **Blob container** via a public endpoint:

```
https://<storage-account-name>.z6.web.core.windows.net/
```

No backend servers or VMs required — just your files, securely served by Azure.

---

## ⚙️ Step-by-Step: Deploy a Static Website

### 🧩 Step 1: Create a Storage Account

#### ✅ Using Azure Portal
1. Go to **Azure Portal → Storage Accounts → + Create**  
2. Choose:
   - **Performance:** Standard  
   - **Redundancy:** LRS (or ZRS for HA)
   - **Account kind:** General-purpose v2 (GPv2)
3. Click **Create**.

#### 💻 Using Azure CLI
```bash
az storage account create   --name mystaticwebappstore   --resource-group myResourceGroup   --location eastus   --sku Standard_LRS   --kind StorageV2
```

---

### 🧩 Step 2: Enable Static Website Hosting

#### ✅ Using Azure Portal
1. Go to your **Storage Account**.
2. In the left menu, select **“Static website”** under **Data management**.
3. Click **Enable**.
4. Enter:
   - **Index document name:** `index.html`
   - **Error document path:** `error.html`
5. Click **Save**.

📍 Azure will create:
- **$web** container (for your files)
- **Primary endpoint:** `https://<storage-account>.z6.web.core.windows.net`

#### 💻 Using Azure CLI
```bash
az storage blob service-properties update   --account-name mystaticwebappstore   --static-website   --index-document index.html   --error-document 404.html
```

---

### 🧩 Step 3: Upload Your Website Files

Your site files (from `/dist` or `/build`) must go into the **$web** container.

#### 💻 Using Azure CLI
```bash
az storage blob upload-batch   --account-name mystaticwebappstore   --destination '$web'   --source ./dist
```

#### 🖥️ Using Azure Portal
1. Go to **Storage Account → Containers → $web**.
2. Click **Upload** → select files (e.g., `index.html`, CSS, JS).
3. Click **Upload**.

---

### 🧩 Step 4: Browse the Static Website

Once uploaded, open your endpoint:

```
https://<storage-account>.z6.web.core.windows.net/
```

🎉 Your website is now live!

---

### 🧩 Step 5: (Optional) Add Custom Domain + HTTPS

#### Custom Domain
```bash
az storage account update   --name mystaticwebappstore   --custom-domain mywebsite.com
```

#### HTTPS via CDN
Use **Azure CDN** or **Front Door** for:
- HTTPS support
- Global caching
- Custom domains

Example:
```bash
az cdn endpoint create   --resource-group myResourceGroup   --name mystaticcdnendpoint   --profile-name mycdnprofile   --origin mystaticwebappstore.z6.web.core.windows.net   --origin-host-header mystaticwebappstore.z6.web.core.windows.net
```

---

## 🧠 Folder Structure Example

```
/dist (or /build)
│
├── index.html
├── error.html
├── /assets
│   ├── style.css
│   └── app.js
└── /images
    └── logo.png
```

All files inside `/dist` or `/build` go into `$web`.

---

## 🧩 Static Website URLs

| **Type** | **Example URL** | **Description** |
|-----------|-----------------|-----------------|
| **Primary endpoint** | `https://mystaticwebappstore.z6.web.core.windows.net/` | Default public site URL |
| **Custom domain** | `https://www.mywebsite.com` | Added via CDN or Azure DNS |
| **Blob endpoint** | `https://mystaticwebappstore.blob.core.windows.net/$web/` | Internal blob path (not for direct browsing) |

---

## 🔒 Security & Access

| **Option** | **Description** |
|-------------|-----------------|
| **Public access** | `$web` container automatically made public for website access |
| **Private access** | Restrict using Front Door + Private Endpoint |
| **RBAC / SAS tokens** | For controlled upload and management |
| **Azure CDN** | Adds HTTPS, caching, and better performance |

---

## 💡 Tips & Best Practices

- ✅ Use **GPv2 Storage Accounts** (required for static websites)
- ✅ Set both **index.html** and **error.html** correctly (case-sensitive)
- ✅ For SPAs (React, Angular, Vue): use `index.html` as both index and error file  
  ```text
  Index document: index.html
  Error document: index.html
  ```
- ✅ Automate deployment with GitHub Actions or Azure DevOps

---

## ⚙️ Example CI/CD Workflow (GitHub Actions)

```yaml
name: Deploy to Azure Storage

on:
  push:
    branches:
      - main

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v2

      - name: Build project
        run: npm install && npm run build

      - name: Upload to Azure Blob
        uses: azure/cli@v1
        with:
          inlineScript: |
            az storage blob upload-batch               --account-name mystaticwebappstore               --destination '$web'               --source ./build               --overwrite
```

---

## ✅ TL;DR Summary

| **Step** | **Action** | **Command / Option** |
|-----------|-------------|----------------------|
| 1️⃣ | Create storage account | `az storage account create` |
| 2️⃣ | Enable static website | `az storage blob service-properties update --static-website` |
| 3️⃣ | Upload build files | `az storage blob upload-batch --destination '$web'` |
| 4️⃣ | Access website | `https://<account>.z6.web.core.windows.net/` |
| 5️⃣ | (Optional) Add custom domain + HTTPS | Azure CDN / Front Door |



# 🧱 Azure File Share and SFTP Setup Guide

This guide explains how to configure **Azure File Shares (SMB)** and **SFTP access** in Azure Storage — allowing you to use Azure as both a **network drive** and a **secure file transfer endpoint**.

---

## 1️⃣ Azure File Share (Drive Setup)

Azure **File Storage** provides **SMB (Server Message Block)** file shares that you can **mount as a network drive** on Windows, macOS, or Linux. It’s ideal for lift-and-shift applications and shared file access.

---

### ⚙️ Step 1: Create a Storage Account

```bash
az storage account create   --name myfilestorage123   --resource-group myResourceGroup   --location eastus   --sku Standard_LRS   --kind StorageV2
```

---

### ⚙️ Step 2: Create a File Share

```bash
az storage share create   --account-name myfilestorage123   --name myshare   --quota 100
```

---

### ⚙️ Step 3: Get Connection Details

```bash
az storage account keys list   --account-name myfilestorage123   --resource-group myResourceGroup
```
Take note of:
- **Storage Account Name**
- **Key**
- **File Share Name**

---

### ⚙️ Step 4: Mount File Share (Windows)

```powershell
net use Z: \myfilestorage123.file.core.windows.net\myshare /u:Azure\myfilestorage123 <storage-account-key>
```
✅ This maps your Azure File Share to drive `Z:`.

Add `/persistent:yes` to auto-connect after reboot.

---

### ⚙️ Step 5: Mount File Share (Linux)

```bash
sudo apt install cifs-utils
sudo mount -t cifs //myfilestorage123.file.core.windows.net/myshare /mnt/myshare   -o vers=3.0,username=myfilestorage123,password=<storage-account-key>,dir_mode=0777,file_mode=0777,serverino
```

---

### ⚙️ Step 6: Mount File Share (macOS)

```bash
sudo mkdir /Volumes/myshare
sudo mount_smbfs //myfilestorage123:<storage-account-key>@myfilestorage123.file.core.windows.net/myshare /Volumes/myshare
```

---

### 🔒 Security Options

| **Feature** | **Description** |
|--------------|-----------------|
| **SMB 3.0 Encryption** | Secures data in transit (default). |
| **Azure AD Authentication** | Integrate with AD DS or Azure AD DS. |
| **Private Endpoints** | Restrict file share access to VNets. |
| **Snapshots** | Create point-in-time backups for recovery. |

---

## 2️⃣ SFTP (Secure File Transfer Protocol) Setup

Azure **Blob Storage** now supports **native SFTP access**, allowing you to securely upload/download files without deploying a VM or external SFTP server.

---

### ⚙️ Step 1: Create a Storage Account with SFTP Support

SFTP requires **hierarchical namespace (HNS)** enabled.

```bash
az storage account create   --name mysftpstorage123   --resource-group myResourceGroup   --location eastus   --sku Standard_LRS   --kind StorageV2   --hierarchical-namespace true   --enable-sftp true
```

---

### ⚙️ Step 2: Create a Container

```bash
az storage container create   --account-name mysftpstorage123   --name sftp-data
```

---

### ⚙️ Step 3: Create Local SFTP User

```bash
az storage account local-user create   --account-name mysftpstorage123   --username sftpuser1   --home-directory sftp-data   --permission-scope permissions=rw container=sftp-data
```

---

### ⚙️ Step 4: Configure Authentication

#### SSH Key Authentication
```bash
az storage account local-user update   --account-name mysftpstorage123   --username sftpuser1   --ssh-authorized-key key="$(cat ~/.ssh/id_rsa.pub)"
```

#### Password Authentication
```bash
az storage account local-user update   --account-name mysftpstorage123   --username sftpuser1   --has-ssh-password true
```

---

### ⚙️ Step 5: Get SFTP Endpoint

```bash
az storage account show   --name mysftpstorage123   --query "primaryEndpoints.sftp"
```
Example output:
```
"sftp://mysftpstorage123.blob.core.windows.net"
```

---

### ⚙️ Step 6: Connect via SFTP Client

Use any standard client (WinSCP, FileZilla, or `sftp` command):

```bash
sftp sftpuser1@mysftpstorage123.blob.core.windows.net
sftp> put ./upload.txt
sftp> get /data/report.csv
```

---

### 🔒 Security Highlights

| **Feature** | **Description** |
|--------------|-----------------|
| **Port 22 (SSH)** | Secure access over SFTP |
| **No VM Required** | Fully managed by Azure |
| **User Isolation** | Each user has its own home directory |
| **ACL & RBAC Support** | Fine-grained access control |
| **Private Endpoints** | Restrict SFTP access to internal VNets |
| **Activity Logs** | Monitor uploads/downloads via Azure Monitor |

---

## 🧠 Comparison: Azure File Share vs SFTP

| **Feature** | **Azure File Share (SMB)** | **Azure Blob SFTP** |
|--------------|-----------------------------|----------------------|
| **Protocol** | SMB 3.0 | SFTP (SSH) |
| **Primary Use** | Mounted file drives | Secure file transfer |
| **Access Method** | Windows/Linux/macOS (mount) | Any OS with SFTP client |
| **Authentication** | AD / Storage Key | SSH Key or Password |
| **Data Location** | File share | Blob container |
| **Security** | SMB encryption, AD DS | SSH encryption |
| **Best For** | Shared storage, lift-and-shift | Partner integration, automation |

---

## ✅ TL;DR Summary

| **Goal** | **Recommended Option** | **How to Enable** |
|-----------|------------------------|-------------------|
| Mount a shared drive | **Azure File Share (SMB)** | Create file share and mount via SMB |
| Secure file uploads | **Blob SFTP** | Enable SFTP and create local users |
| Enterprise authentication | **AD DS Integration (SMB)** | Join to AD DS or Azure AD DS |
| Private network access | **Private Endpoints** | Restrict access to VNets |

---








