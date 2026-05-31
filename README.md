# Yals — Yet Another Link Shortener

Yals turns long URLs into short, shareable links. Each link tracks how many times it has been visited, and access to those statistics is protected by a secret key that is issued at creation time and never stored in plain text.

## API

The service exposes two route groups.

### `/urls` — link management and stats

#### Create a shortened URL

```
POST /urls
Content-Type: application/json

{ "targetUri": "https://example.com/very/long/path?with=query" }
```

Returns `201 Created`:

```json
{
  "id": "01JJKCQE4Z1X8FGBHNQP3Y5W6D",
  "targetUri": "https://example.com/very/long/path?with=query",
  "statsKey": "a3f8...64-hex-chars...1b2c"
}
```

The `statsKey` is a 64-character hex string. **Save it** — it is the only way to read statistics for this link and cannot be retrieved later.

```bash
curl -s -X POST http://localhost:5090/urls \
  -H "Content-Type: application/json" \
  -d '{"targetUri": "https://example.com/very/long/path"}' | jq .
```

#### Read statistics

```
GET /urls/{id}/stats
Authorization: Bearer <statsKey>
```

Returns `200 OK`:

```json
[{ "type": "Lifetime", "hits": 42 }]
```

Error responses:

- `401 Unauthorized` — missing, malformed, or incorrect stats key
- `404 Not Found` — unknown ID

```bash
curl -s http://localhost:5090/urls/01JJKCQE4Z1X8FGBHNQP3Y5W6D/stats \
  -H "Authorization: Bearer a3f8...1b2c" | jq .
```

### `/go` — public-facing redirection

```
GET /go/{id}
```

Returns `308 Permanent Redirect` to the original URL and increments the lifetime hit counter. Returns `404 Not Found` for unknown IDs.

```bash
curl -L http://localhost:5090/go/01JJKCQE4Z1X8FGBHNQP3Y5W6D
```

An OpenAPI document is available at `/openapi/v1.json`.

## Architecture

The application is a single ASP.NET Core 10 Minimal API project, organised into layers:

```
Yals/
├── GoMapGroup.cs          ← route handler (redirect)
├── Urls/
│   ├── UrlsMapGroup.cs    ← route handlers (create, stats)
│   ├── UrlService.cs      ← business logic
│   ├── UrlDb.cs           ← EF Core DbContext (PostgreSQL)
│   ├── UrlEntity.cs       ← database entity
│   ├── Url.cs             ← domain record
│   ├── CreateUrlDto.cs    ← create URL request shape
│   └── CreatedUrlDto.cs   ← create URL response shape
├── Stats/
│   ├── StatsAuthenticator.cs   ← validates Bearer token against stored hash
│   ├── StatsKeyGenerator.cs    ← generates key + salt pair
│   └── Stat.cs / StatType.cs  ← stats value objects
└── Crypto/
    └── CryptoRandomNumberGenerator.cs  ← wraps RandomNumberGenerator
```

**Route handlers** (`*MapGroup.cs`) translate HTTP into domain calls and back — no business logic lives here.

**`UrlService`** owns the business rules: generating IDs (ULID), delegating key generation, and bulk-updating hit counts directly in the database to avoid EF Core change-tracker overhead.

**`UrlDb`** is an EF Core `DbContext` backed by PostgreSQL. ULIDs are stored as GUIDs. Migrations live in `Yals/Migrations/`.

**Stats** authentication follows a hash-then-compare pattern: the service stores a SHA-256 hash of `(key bytes ++ salt bytes)` and uses constant-time comparison when validating, protecting against timing attacks.

## Running locally

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) with the Compose plugin

### Start the stack

```bash
git clone <repo-url>
cd yals
docker compose up
```

Compose starts three services in order:

| Service   | What it does                                        | Port |
| --------- | --------------------------------------------------- | ---- |
| `db`      | PostgreSQL 17                                       | —    |
| `migrate` | Runs `dotnet ef database update`, then exits        | —    |
| `app`     | ASP.NET Core app with `dotnet watch` for hot reload | 5090 |

The application is available at `http://localhost:5090` once `app` prints that it is listening.

Source code is mounted into the container, so changes to `.cs` files trigger a hot reload automatically.

### Running the tests

```bash
dotnet test
```

The functional test suite uses [Testcontainers](https://testcontainers.com/) to spin up a real PostgreSQL instance for each test run — no manual setup required.
