# Export do DCS — Fase 1 (telemetria)

O DCS exporta telemetria da aeronave do jogador através do hook `Export.lua`,
enviando dados via UDP para a estação GripenLink.

## O que falta fazer (quando o DCS estiver instalado)

1. Identificar o caminho `Saved Games\DCS\Scripts\Export.lua` (criar se não existir)
2. Fazer o `Export.lua` capturar posição/proa/velocidade/altitude do `LoGetSelfData()`
3. Serializar em JSON e enviar via UDP (ex.: `127.0.0.1:5310`)
4. Apontar o `UdpTelemetryReceiver` da `GripenLink.Ingest` para a mesma porta
5. Configurar o `GripenLink.Api` para ligar o receiver → `TrackManager`

Contrato de JSON (esperado pelo `DcsTelemetryParser`):

```json
{
  "callsign": "GRIPEN01",
  "latitude": -23.1791,
  "longitude": -45.8870,
  "altitude": 3500,
  "heading": 125.5,
  "speed": 250,
  "timestamp": "2026-09-01T12:00:00Z"
}
```

> Nota: DCS roda nativamente só em Windows. O receiver/backend deve rodar na mesma máquina (localhost) durante o desenvolvimento.
