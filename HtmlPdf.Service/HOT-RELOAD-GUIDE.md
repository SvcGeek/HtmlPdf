# Hot-Reload Configuration Guide

## 🔥 Overview

HtmlPdf.Service supports **hot-reload configuration** - you can change settings in `appsettings.json` while the application is running, and changes take effect immediately without restarting!

## 📋 What Can Be Configured

### 1. Concurrency Limit
**Setting**: `PdfRendering:MaxConcurrentRenderings`  
**Purpose**: Controls how many PDFs can be generated simultaneously  
**Default**: 10

### 2. Template Whitelist
**Setting**: `PdfRendering:AllowedTemplates`  
**Purpose**: Security whitelist of allowed template names  
**Default**: `["sample-endpoint-render"]`

---

## 🚀 Quick Start

### Step 1: View Current Configuration

```json
// appsettings.json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 10,
    "AllowedTemplates": [
      "sample-endpoint-render"
    ]
  }
}
```

### Step 2: Make Changes

**Example 1: Increase concurrency**
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 20  // ← Changed from 10 to 20
  }
}
```

**Example 2: Add new template**
```json
{
  "PdfRendering": {
    "AllowedTemplates": [
      "sample-endpoint-render",
      "invoice",           // ← Added
      "receipt"            // ← Added
    ]
  }
}
```

### Step 3: Save the File

The application will automatically:
1. Detect the file change
2. Reload configuration
3. Update runtime behavior
4. Log the changes

### Step 4: Check Logs

You'll see output like:
```
info: HtmlPdf.Service.Renderer.PuppeteerPdfRenderer[0]
      Concurrency limit changed from 10 to 20. Recreating semaphore...
info: HtmlPdf.Service.Renderer.PuppeteerPdfRenderer[0]
      Concurrency limiter updated successfully.
```

---

## 💡 Use Cases

### Use Case 1: Emergency Scaling
**Scenario**: Suddenly receiving high traffic

**Solution**: Increase concurrency limit:
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 25  // Temporarily scale up
  }
}
```

**Result**: More concurrent PDF generations without restarting

---

### Use Case 2: Memory Pressure
**Scenario**: Server running low on memory

**Solution**: Reduce concurrency limit:
```json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 5  // Reduce to prevent OOM
  }
}
```

**Result**: Lower memory footprint (but slower throughput)

---

### Use Case 3: Adding Templates On-The-Fly
**Scenario**: Need to enable a new template without downtime

**Solution**: Add to whitelist:
```json
{
  "PdfRendering": {
    "AllowedTemplates": [
      "sample-endpoint-render",
      "new-urgent-template"  // ← Just added
    ]
  }
}
```

**Result**: Template immediately available for requests

---

### Use Case 4: Temporarily Disable Template
**Scenario**: Found issue with a template, need to disable it quickly

**Solution**: Remove from whitelist:
```json
{
  "PdfRendering": {
    "AllowedTemplates": [
      "sample-endpoint-render"
      // "problematic-template" ← Removed/commented out
    ]
  }
}
```

**Result**: Template requests will be rejected immediately

---

## 🎯 Best Practices

### 1. Test Changes in Development First
```json
// appsettings.Development.json
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 5  // Lower for dev environment
  }
}
```

### 2. Document Your Changes
Keep a log of configuration changes:
```
2024-01-15 10:30: Increased MaxConcurrentRenderings to 20 (high traffic event)
2024-01-15 14:00: Restored to 10 (event completed)
```

### 3. Monitor After Changes
Watch logs and memory usage after changing `MaxConcurrentRenderings`:
```bash
# Monitor memory usage (Linux)
watch -n 1 'ps aux | grep HtmlPdf.Service'

# Monitor logs
tail -f /var/log/htmlpdf-service.log
```

### 4. Use Environment-Specific Settings
```json
// appsettings.json (Production)
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 15
  }
}

// appsettings.Development.json (Development)
{
  "PdfRendering": {
    "MaxConcurrentRenderings": 3  // Lower for local dev
  }
}
```

---

## ⚠️ Important Notes

### Configuration Changes vs. In-Flight Requests

**How it works**:
- Changes affect **new requests** only
- In-flight requests continue with the old configuration
- No requests are cancelled or interrupted

**Example**:
```
10:00:00 - Request 1 starts (using limit=10)
10:00:05 - You change limit to 20
10:00:05 - Request 1 continues (still using limit=10)
10:00:06 - Request 2 starts (using limit=20) ✓
```

### Concurrency Limit Changes

When `MaxConcurrentRenderings` changes:
1. A new `SemaphoreSlim` is created with the new limit
2. The old semaphore is disposed
3. Existing waiters on the old semaphore complete normally
4. New requests use the new semaphore

**Edge case**: If you decrease the limit while requests are running, those requests finish normally.

### Template Whitelist Changes

Template validation happens **per request**:
- Each request checks `optionsMonitor.CurrentValue.AllowedTemplates`
- Changes are effective immediately
- No caching of the whitelist

---

## 🔧 Troubleshooting

### Issue: Changes Not Taking Effect

**Check 1**: Is the correct appsettings.json being loaded?
```bash
# Check which file is being used
curl http://localhost:5000/debug/config  # If you add a debug endpoint
```

**Check 2**: JSON syntax valid?
```bash
# Validate JSON
cat appsettings.json | jq .
```

**Check 3**: Environment override?
`appsettings.Development.json` overrides `appsettings.json` in Development environment.

### Issue: Application Doesn't Log Config Changes

**Solution**: Ensure logging level allows Info messages:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",  // ← Must be Information or Debug
      "HtmlPdf.Service.Renderer.PuppeteerPdfRenderer": "Information"
    }
  }
}
```

---

## 📊 Configuration Change Examples

### Example 1: Scale Up for Event

**Before**:
```json
{"PdfRendering": {"MaxConcurrentRenderings": 10}}
```

**Load testing shows capacity for more**:
```json
{"PdfRendering": {"MaxConcurrentRenderings": 25}}
```

**Expected result**: 2.5x more throughput (if server has resources)

### Example 2: Add Multiple Templates

**Before**:
```json
{
  "PdfRendering": {
    "AllowedTemplates": ["sample-endpoint-render"]
  }
}
```

**After adding new templates**:
```json
{
  "PdfRendering": {
    "AllowedTemplates": [
      "sample-endpoint-render",
      "invoice",
      "receipt",
      "delivery-note",
      "packing-slip"
    ]
  }
}
```

**Result**: All 5 templates immediately available

### Example 3: Environment-Specific Limits

**Production** (appsettings.json):
```json
{"PdfRendering": {"MaxConcurrentRenderings": 20}}
```

**Development** (appsettings.Development.json):
```json
{"PdfRendering": {"MaxConcurrentRenderings": 3}}
```

**Result**: Different limits per environment, no code changes needed

---

## 🎓 Technical Details

### How ASP.NET Core Detects Changes

1. `FileSystemWatcher` monitors appsettings.json
2. File modification detected
3. Configuration providers reload
4. `IOptionsMonitor<T>` compares old vs. new values
5. If changed, `OnChange()` callbacks fire
6. Your code receives new configuration

### Why IOptionsMonitor?

| Feature | IOptions | IOptionsSnapshot | IOptionsMonitor |
|---------|----------|------------------|-----------------|
| Reload on change | ❌ | ✅ (per request) | ✅ (immediate) |
| Singleton compatible | ✅ | ❌ | ✅ |
| Change notifications | ❌ | ❌ | ✅ |
| **Best for hot-reload** | | | **✅** |

### Thread Safety

The implementation is thread-safe:
- `_updateLock` prevents concurrent semaphore updates
- `OnChange()` callbacks are serialized
- In-flight requests continue safely

---

## 🎯 Summary

**Benefits**:
✅ No downtime for configuration changes  
✅ A/B test different concurrency limits  
✅ Quickly respond to changing load patterns  
✅ Manage template whitelist without code changes  

**How to use**:
1. Edit `appsettings.json`
2. Save
3. Done! 🎉

**What to monitor**:
- Application logs for change confirmations
- Memory usage after concurrency changes
- Response times and throughput

---

**Quick Reference**:
- Concurrency → `PdfRendering:MaxConcurrentRenderings`
- Templates → `PdfRendering:AllowedTemplates`
- Changes apply to **new requests** immediately
- In-flight requests **continue normally**
