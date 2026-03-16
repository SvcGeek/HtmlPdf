# Configuration Examples

This document provides practical examples of configuring HtmlPdf.Service via `appsettings.json`.

## 📝 Basic Configuration

### Minimal Configuration
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 10,
    "AllowedTemplates": [
      "sample-endpoint-render"
    ]
  }
}
```

---

## 🎯 Environment-Specific Configurations

### Development Environment
**File**: `appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",  // More verbose logging
      "Microsoft.AspNetCore": "Information"
    }
  },
  "PdfRendering": {
    "MaxConcurrentRenderings": 3,  // Lower for local development
    "AllowedTemplates": [
      "sample-endpoint-render",
      "test-template"  // Additional templates for testing
    ]
  }
}
```

**Why lower limit**: Development machines have less resources, and you typically test one request at a time.

---

### Production Environment
**File**: `appsettings.Production.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "PdfRendering": {
    "MaxConcurrentRenderings": 20,  // Higher for production servers
    "AllowedTemplates": [
      "sample-endpoint-render",
      "invoice",
      "receipt",
      "delivery-note"
    ]
  },
  "Browser": {
    "DownloadPath": "/app/chromium"  // Custom path for containers
  }
}
```

**Why higher limit**: Production servers have more RAM and handle higher concurrency.

---

### Staging Environment
**File**: `appsettings.Staging.json`

```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 15,  // Between dev and prod
    "AllowedTemplates": [
      "sample-endpoint-render",
      "invoice",
      "receipt",
      "beta-template"  // Test new templates before production
    ]
  }
}
```

---

## 🚀 Scaling Scenarios

### Scenario 1: High-Memory Server (32 GB RAM)
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 30  // Aggressive scaling
  }
}
```

**Expected**: ~200 PDFs/minute throughput

---

### Scenario 2: Low-Memory Container (2 GB RAM)
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 4  // Conservative to prevent OOM
  }
}
```

**Expected**: ~40 PDFs/minute throughput, but stable

---

### Scenario 3: Load Balancer with Multiple Instances
**Per instance**:
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 10
  }
}
```

**With 5 instances**: 5 × 10 = 50 concurrent PDF generations across the cluster

---

## 🔐 Security Configurations

### Strict Whitelist (High Security)
```json
{
  "PdfRendering": {
    "AllowedTemplates": [
      "invoice"  // Only one template allowed
    ]
  }
}
```

Use when: Templates contain sensitive business logic.

---

### Permissive Whitelist (Development)
```json
{
  "PdfRendering": {
    "AllowedTemplates": [
      "sample-endpoint-render",
      "test-invoice",
      "test-receipt",
      "debug-template",
      "experimental-layout"
    ]
  }
}
```

Use when: Rapid development and testing.

---

### Multi-Tenant Configuration
```json
{
  "PdfRendering": {
    "AllowedTemplates": [
      "tenant-a-invoice",
      "tenant-a-receipt",
      "tenant-b-invoice",
      "tenant-b-receipt",
      "shared-delivery-note"
    ]
  }
}
```

Use when: Serving multiple clients with different branding.

---

## 🎨 Custom PDF Settings (Future Enhancement)

While not currently implemented, you could extend `PdfRenderingOptions` to support:

```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 10,
    "AllowedTemplates": ["invoice"],
    "DefaultPaperFormat": "A4",  // A4, Letter, Legal
    "DefaultMargins": {
      "Top": "20mm",
      "Bottom": "20mm",
      "Left": "15mm",
      "Right": "15mm"
    },
    "NetworkTimeout": 30000,  // 30 seconds
    "WaitForNetworkIdle": true
  }
}
```

---

## 📊 Performance Tuning Examples

### Maximize Throughput (High-End Server)
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 25
  }
}
```

**Server specs needed**:
- 16+ GB RAM
- 4+ CPU cores
- Fast disk I/O

---

### Minimize Memory (Constrained Environment)
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 3
  }
}
```

**Works on**:
- 2 GB containers
- Shared hosting
- Low-cost VPS

---

### Balanced (Recommended Default)
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 10
  }
}
```

**Good for**:
- 4-8 GB servers
- Typical workloads
- Starting point for tuning

---

## 🧪 Testing Configurations

### Load Testing Configuration
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",  // Reduce log noise during tests
      "HtmlPdf.Service.Renderer.PuppeteerPdfRenderer": "Information"
    }
  },
  "PdfRendering": {
    "MaxConcurrentRenderings": 20  // Test with high concurrency
  }
}
```

---

### Debug Configuration
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",  // Maximum verbosity
      "HtmlPdf.Service": "Debug"
    }
  },
  "PdfRendering": {
    "MaxConcurrentRenderings": 1,  // Single-threaded for debugging
    "AllowedTemplates": [
      "debug-template"
    ]
  }
}
```

---

## 🐳 Docker/Kubernetes Configurations

### Docker Compose
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 8  // Match container memory limits
  },
  "Browser": {
    "DownloadPath": "/app/chromium"  // Persistent volume for Chromium
  }
}
```

---

### Kubernetes (Pod Resource Limits)

**Small pods** (1 GB memory limit):
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 3
  }
}
```

**Medium pods** (4 GB memory limit):
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 12
  }
}
```

**Large pods** (8 GB memory limit):
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 25
  }
}
```

---

## 🔄 Hot-Reload Testing

### Test 1: Change Concurrency

1. **Start with**:
```json
{"PdfRendering": {"MaxConcurrentRenderings": 5}}
```

2. **Send 10 concurrent requests** - 5 process immediately, 5 queue

3. **Change to**:
```json
{"PdfRendering": {"MaxConcurrentRenderings": 15}}
```

4. **Send 10 more concurrent requests** - All 10 process immediately ✓

---

### Test 2: Add Template

1. **Start with**:
```json
{
  "PdfRendering": {
    "AllowedTemplates": ["sample-endpoint-render"]
  }
}
```

2. **Try to request "invoice"** → 400 Bad Request ❌

3. **Add to config**:
```json
{
  "PdfRendering": {
    "AllowedTemplates": [
      "sample-endpoint-render",
      "invoice"
    ]
  }
}
```

4. **Try to request "invoice" again** → 200 OK ✓

---

## 📝 Complete Example

### Full Production Configuration

**File**: `appsettings.Production.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "Browser": {
    "DownloadPath": "/var/opt/chromium"
  },
  "PdfRendering": {
    "MaxConcurrentRenderings": 20,
    "AllowedTemplates": [
      "sample-endpoint-render",
      "invoice",
      "receipt",
      "delivery-note",
      "packing-slip",
      "return-label"
    ]
  }
}
```

### Environment Variables Override

You can also override via environment variables:
```bash
# Docker/Kubernetes
PdfRendering__MaxConcurrentRenderings=15
PdfRendering__AllowedTemplates__0=invoice
PdfRendering__AllowedTemplates__1=receipt
```

In `docker-compose.yml`:
```yaml
environment:
  - PdfRendering__MaxConcurrentRenderings=15
  - PdfRendering__AllowedTemplates__0=invoice
  - PdfRendering__AllowedTemplates__1=receipt
```

---

## ⚡ Quick Reference

| Setting | Default | Range | Impact |
|---------|---------|-------|--------|
| `MaxConcurrentRenderings` | 10 | 1-50 | Memory vs. throughput |
| `AllowedTemplates` | `["sample-endpoint-render"]` | Any valid string array | Security |

**Memory formula**: ~150 MB + (MaxConcurrentRenderings × 150 MB)

Examples:
- Limit=5: ~900 MB
- Limit=10: ~1.6 GB
- Limit=20: ~3.1 GB
- Limit=30: ~4.6 GB

**Recommended**:
- **2 GB server**: Limit = 5
- **4 GB server**: Limit = 10
- **8 GB server**: Limit = 20
- **16 GB server**: Limit = 30
