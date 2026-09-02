# Requisitos e rastreabilidade — GripenLink

Documento estilo DO-178C (nível de consciência): requisitos → testes → matriz de rastreabilidade.
A partir da Fase 3, toda entrega nova adiciona requisitos aqui e liga cada um a um caso de teste.

## Requisitos (Fase 0)

| ID | Requisito |
|---|---|
| REQ-001 | O sistema deve calcular distância geodésica (haversine) e rumo inicial entre dois pontos |
| REQ-002 | O sistema deve correlacionar amostras de telemetria em pistas por callsign |
| REQ-003 | Uma pista deve iniciar em `Tentative` e promover a `Confirmed` a partir da 2ª atualização |
| REQ-004 | O sistema deve marcar pistas como `Coasting` e removê-las (`Dropped`) por expiração |
| REQ-005 | O data link deve codificar/decodificar um relato de pista em mensagem binária de tamanho fixo |
| REQ-006 | A mensagem do data link deve detectar corrupção via CRC-16 |

## Matriz de rastreabilidade

| Requisito | Caso de teste |
|---|---|
| REQ-001 | `GeoMathTests.Distance_ToSelf_IsZero`, `Distance_SjcToSaoPaulo_IsAbout86Km`, `Bearing_IsInValidRange` |
| REQ-002 | `TrackManagerTests.Upsert_FirstReport_CreatesTentativeTrack` |
| REQ-003 | `TrackManagerTests.Upsert_SecondReport_ConfirmsTrack` |
| REQ-004 | `TrackManagerTests.RemoveDropped_RemovesStaleTracks` |
| REQ-005 | `DataLinkTests.EncodeDecode_RoundTrip_PreservesFields` |
| REQ-006 | `DataLinkTests.Decode_CorruptedMessage_ThrowsCrcError` |

## Como usar (a cada fase)

1. Escreva o requisito (o que o sistema deve fazer, sem dizer *como*)
2. Implemente
3. Escreva o teste que prova o requisito
4. Atualize a matriz acima
