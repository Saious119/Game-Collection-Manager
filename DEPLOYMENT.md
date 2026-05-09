# Deployment Guide — Game Collection Manager

This app has two deployable components:

- **API** — ASP.NET Core 10 backend (`GameCollectionManager.Server`)
- **Client** — Blazor WebAssembly frontend served by nginx (`GameCollectionManager.Client`)

Each gets its own Docker image and Kubernetes Deployment.

---

## Prerequisites

- Docker installed locally
- A container registry (Docker Hub, GitHub Container Registry, a private registry, etc.)
- A k3s cluster with kubectl configured
- Vault Secrets Operator installed on the cluster, with a `VaultAuth` resource named `vault-auth` in the `game-collection` namespace
- (Optional) cert-manager installed on the cluster for automatic TLS

---

## File Overview

| File | What it does |
|------|-------------|
| `Dockerfile` | Builds the API server image. Multi-stage: compiles with the .NET SDK, runs on the lighter ASP.NET runtime. Runs as a non-root user on port 8080. |
| `GameCollectionManager.Client/Dockerfile` | Builds the Blazor WASM client. Multi-stage: compiles with the .NET SDK, outputs static files, serves them with nginx. |
| `nginx.conf` | nginx config used inside the client image. Enables SPA routing (all unknown paths fall back to `index.html` so Blazor's router handles them). |
| `.dockerignore` | Prevents `bin/`, `obj/`, and other build artifacts from being sent to the Docker daemon, keeping builds fast. |
| `k8s/deployment.yaml` | All Kubernetes resources in one file: Namespace, VaultStaticSecret, ConfigMap, two Deployments, two Services, a Traefik Middleware, and a Traefik IngressRoute. |

---

## Step 1 — Configure the Deployment Manifest

Open `k8s/deployment.yaml` and make the following changes before deploying anything.

### 1a. Create the Vault Secret

The `VaultStaticSecret` in the manifest points to `secret/game-collection/api` in Vault (KV-v2). Create that secret in Vault with exactly these field names:

```
gamedb-connect-string     Host=...;Database=...;Username=...;Password=...
igdb-client-id            <your IGDB client ID>
igdb-client-secret        <your IGDB client secret>
discord-client-id         <your Discord app client ID>
discord-client-secret     <your Discord app client secret>
discord-redirect-uri      https://api.your-domain.com/auth/discord/callback
jwt-secret-key            <random string, minimum 32 characters>
jwt-issuer                GameCollectionManager
jwt-audience              GameCollectionManager
```

Using the Vault CLI:

```bash
vault kv put secret/game-collection/api \
  gamedb-connect-string="Host=...;..." \
  igdb-client-id="..." \
  igdb-client-secret="..." \
  discord-client-id="..." \
  discord-client-secret="..." \
  discord-redirect-uri="https://api.your-domain.com/auth/discord/callback" \
  jwt-secret-key="..." \
  jwt-issuer="GameCollectionManager" \
  jwt-audience="GameCollectionManager"
```

VSO will sync these fields into a Kubernetes Secret named ` game-collection-manager` in the `game-collection` namespace, refreshing every 60 seconds. The Deployment reads from that secret via `secretKeyRef` — no manual `kubectl create secret` needed.

### 1b. Ensure `vault-auth` Exists in the Namespace

The `VaultStaticSecret` references `vaultAuthRef: vault-auth`. If your existing `vault-auth` `VaultAuth` resource lives in a different namespace (e.g. `discord-bots`), you need one in `game-collection` too. Copy it across:

```bash
kubectl get vaultauth vault-auth -n discord-bots -o yaml \
  | sed 's/namespace: discord-bots/namespace: game-collection/' \
  | kubectl apply -f -
```

### 1d. Local Network Access — DNS Setup

The manifest is configured for `whattoplay.local`. You need one DNS entry pointing that hostname at your k3s node's IP.

**Option 1 — Router DNS (recommended, works for every device on the network)**

Add a static DNS entry in your router's admin panel:
```
whattoplay.local → <k3s node IP>
```

**Option 2 — hosts file (per device)**

On each device that needs access, add a line to:
- Linux/Mac: `/etc/hosts`
- Windows: `C:\Windows\System32\drivers\etc\hosts`

```
192.168.x.x  whattoplay.local
```

Replace `192.168.x.x` with your k3s node's IP (`kubectl get nodes -o wide` shows it).

**Routing**

The IngressRoute routes all traffic on `whattoplay.local`:
- `whattoplay.local/` → Blazor WASM client (what you open in a browser)
- `whattoplay.local/api/*` → ASP.NET Core API (called internally by the client; Traefik strips the `/api` prefix before forwarding)

### 1e. Set Your Image Names

Replace `your-registry/game-collection-api:latest` and `your-registry/game-collection-client:latest` with the actual image paths you'll push to your registry.

---

## Step 2 — Build and Push Images

Run these from the **solution root** (the directory containing `GameCollectionManager.sln`).

```bash
# Build and push the API image
docker build -t your-registry/game-collection-api:latest .
docker push your-registry/game-collection-api:latest

# Build and push the client image
docker build -f GameCollectionManager.Client/Dockerfile -t your-registry/game-collection-client:latest .
docker push your-registry/game-collection-client:latest
```

Both builds use the solution root as the Docker build context so they can access all three projects (Server, Client, Shared).

---

## Step 3 — Deploy to k3s

```bash
kubectl apply -f k8s/deployment.yaml
```

This creates the `game-collection` namespace and all resources inside it. To check that everything came up:

```bash
kubectl get pods -n game-collection
kubectl get ingress -n game-collection
```

The API pod takes 20–30 seconds to start. If a pod stays in `CrashLoopBackOff`, check logs:

```bash
kubectl logs -n game-collection deployment/game-collection-api
kubectl logs -n game-collection deployment/game-collection-client
```

---

## Step 4 — TLS (When You Move to a Public Domain)

When you're ready to expose the app publicly with a real domain and HTTPS, replace the `IngressRoute` in `k8s/deployment.yaml` with one that uses the `websecure` entrypoint and references a TLS secret, then add cert-manager to issue Let's Encrypt certificates automatically. That's a separate step for when you get there.

---

## Updating the App

To deploy a new version:

```bash
# Rebuild and push the updated image(s)
docker build -t your-registry/game-collection-api:latest .
docker push your-registry/game-collection-api:latest

# Roll out the update
kubectl rollout restart deployment/game-collection-api -n game-collection
```

Use a specific tag (e.g., `:v1.2.3`) instead of `:latest` in production so rollbacks are reliable:

```bash
kubectl set image deployment/game-collection-api \
  api=your-registry/game-collection-api:v1.2.3 \
  -n game-collection
```

---

## Changing the API URL Without Rebuilding

The client reads its API URL from `appsettings.json` at runtime. This file is injected via the `client-appsettings` ConfigMap, so you can change the URL without rebuilding the client image:

```bash
kubectl edit configmap client-appsettings -n game-collection
# Change ApiBaseUrl, save and exit

kubectl rollout restart deployment/game-collection-client -n game-collection
```

---

## Environment Variables Reference

These are all injected into the API container by `k8s/deployment.yaml`.

| Variable | Source | Description |
|----------|--------|-------------|
| `gamedb_connect_string` | Vault → Secret | PostgreSQL/CockroachDB connection string |
| `IGDB_CLIENT_ID` | Vault → Secret | IGDB API client ID |
| `IGDB_CLIENT_SECRET` | Vault → Secret | IGDB API client secret |
| `DISCORD_CLIENT_ID` | Vault → Secret | Discord OAuth app client ID |
| `DISCORD_CLIENT_SECRET` | Vault → Secret | Discord OAuth app client secret |
| `DISCORD_REDIRECT_URI` | Vault → Secret | Discord OAuth callback URL |
| `JWT_SECRET_KEY` | Vault → Secret | Signing key for JWTs (min 32 chars) |
| `JWT_ISSUER` | Vault → Secret | JWT issuer claim |
| `JWT_AUDIENCE` | Vault → Secret | JWT audience claim |
| `CLIENT_URL` | Inline in manifest | Client origin added to CORS allowlist |
| `ASPNETCORE_ENVIRONMENT` | Inline in manifest | Set to `Production` |

"Vault → Secret" means the value lives in Vault at `secret/game-collection/api` and VSO syncs it into the ` game-collection-manager` Kubernetes Secret automatically.

---

## Adding a Health Check Endpoint (Recommended)

The API probes currently use a TCP socket check (just verifies the port is open). For a proper HTTP health check, add this one line to `Program.cs` before `app.Run()`:

```csharp
app.MapHealthChecks("/health");
```

And add the NuGet package if not already present:

```bash
dotnet add GameCollectionManager.Server package Microsoft.AspNetCore.Diagnostics.HealthChecks
```

Then update the liveness and readiness probes in `k8s/deployment.yaml` from `tcpSocket` to:

```yaml
livenessProbe:
  httpGet:
    path: /health
    port: 8080
```
