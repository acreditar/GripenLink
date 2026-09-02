# GripenLink

Mini **estação de solo / C2** alimentada por telemetria de voo. O nome vem do **F-39 Gripen** (caça da FAB, montado pela parceria Embraer/Saab em Gavião Peixoto, vizinho de SJC).

> **Por quê?** Software de simulação de operações aéreas militares e estações de solo são produtos reais feitos no cluster aeroespacial de SJC (Atech/SOpM). Este projeto replica esse tipo de sistema com a stack que domino (C#/.NET), e ao mesmo tempo ensina conceitos aeroespaciais (protocolos, CRC, V&V, ciclo de vida de pista).

## O que já existe (Fase 0 — fundação)

| Projeto | Papel |
|---|---|
| `GripenLink.Core` | Domínio: `Track`, `TrackManager` (ciclo de vida), `GeoMath` (haversine/bearing) |
| `GripenLink.DataLink` | Data link tático "de brinquedo": `TrackReport` (mensagem binária de 28 bytes) + `Crc16` |
| `GripenLink.Ingest` | `UdpTelemetryReceiver` (UDP) + `DcsTelemetryParser` (JSON do Export.lua) |
| `GripenLink.Api` | Minimal API + Swagger + SQLite (EF Core) |
| `GripenLink.Tests` | 9 testes (geodésica, data link, track manager) |

## Rodar

```powershell
dotnet run --project src/GripenLink.Api
```

- Swagger: `https://localhost:xxxx/swagger`
- `GET /tracks` — lista pistas em memória
- `POST /telemetry` — envia uma amostra (JSON) e correlaciona em pista

Exemplo de `POST /telemetry`:

```json
{
  "callsign": "GRIPEN01",
  "latitude": -23.1791,
  "longitude": -45.8870,
  "altitudeMeters": 3500,
  "headingDegrees": 125.5,
  "speedMetersPerSecond": 250,
  "timestampUtc": "2026-09-01T12:00:00Z"
}
```

Testes:

```powershell
dotnet test
```

## Roadmap do projeto (caminho de estudo just-in-time)

| Fase | Entrega | O que estudar na hora |
|---|---|---|
| 0. Fundação ✅ | Solution, Minimal API, EF+SQLite, xUnit, Swagger, CI | DI, migrations, fundamentos de teste |
| 1. Ingestão DCS | UDP recebendo telemetria real do DCS (Export.lua) | UDP em C#, Export.lua, coordenadas WGS84 |
| 2. Tracks | Fusão: filtro alpha-beta, dead reckoning | Máquina de estados, vetores/trig (haversine, bearing) |
| 3. Display | Mapa ao vivo (React + Leaflet) via SignalR | SignalR, Leaflet, projeções |
| 4. Data link tático | `TrackReport` evoluído + pub/sub | Bit packing, CRC, framing (família ARINC 429) |
| 5. Simulação | Mini-wargame: patrulha × engajamento | Simulação de eventos discretos, máquinas de estado |

## Regras do projeto

- Um repositório só, fases sequenciais (não começar a Fase N+1 sem a N testada e publicada)
- DCS é **laboratório**, não jogo
- Toda fase fecha com testes verdes + README atualizado + (a partir da Fase 3) matriz de rastreabilidade em `docs/`
