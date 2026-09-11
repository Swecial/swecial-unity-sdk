# Swecial Unity SDK (`com.swecial.unity`)

Official Unity SDK for Swecial backend services, API communication, authentication, and data storage.

## 📦 Installation via Unity Package Manager (UPM)

Add the following to your Unity project's `Packages/manifest.json`.

### Local Development (Symlink)
For local development where you want to edit the SDK alongside the game:
```json
{
  "dependencies": {
    "com.swecial.unity": "file:../../swecial-unity-sdk"
  }
}
```
*(Adjust the relative path to match where you cloned the SDK in relation to your Unity project).*

### Git Repository (Production)
For production builds and CI/CD, pull the package directly from GitHub using HTTPS or SSH. Because this is a private repository, ensure your local environment or CI runner has Git credentials configured.

**HTTPS:**
```json
{
  "dependencies": {
    "com.swecial.unity": "https://github.com/Swecial/swecial-unity-sdk.git#master"
  }
}
```

**SSH:**
```json
{
  "dependencies": {
    "com.swecial.unity": "ssh://git@github.com/Swecial/swecial-unity-sdk.git#master"
  }
}
```
*Tip: Replace `#master` with a specific commit hash or release tag (e.g., `#v1.0.0`) to lock the version.*

## 🚀 Basic Usage

The SDK relies on the `com.swecial.unity` namespace. 

### 1. Initialization
The `Swecial` component must be attached to a persistent GameObject in your initial scene (e.g., `SwecialSettings`). It handles the connection to the Swecial Server (both production and development endpoints) and holds your API keys.

### 2. Cloud Code Invocation
To call server-side cloud code (`/server` in the app's repo), use `Swecial.code()`:

```csharp
using com.swecial.unity;

// Call a cloud function
Swecial.code("getTestMessage")
    .runAfter(request => {
        if (request.isSucceeded) {
            Debug.Log("Server says: " + request.response.message);
        } else {
            Debug.LogError("Error: " + request.response.error);
        }
    });
```
