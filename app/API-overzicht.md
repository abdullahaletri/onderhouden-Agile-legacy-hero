# API-overzicht Excellent Taste

Start eerst `ExcellentTaste.Api`.

Standaard basisadres:

```text
http://localhost:5000
```

Belangrijke routes:

```text
GET    /api/status
GET    /api/reserveringen
GET    /api/reserveringen/{id}
POST   /api/reserveringen
PUT    /api/reserveringen/{id}
DELETE /api/reserveringen/{id}

GET    /api/klanten
GET    /api/menuitems
GET    /api/gegevens/drinken
GET    /api/gegevens/eten

GET    /api/bestellingen/{reserveringId}
POST   /api/bestellingen/{reserveringId}/items/{menuItemCode}
POST   /api/bestellingen/{reserveringId}/items/{menuItemCode}/plus
POST   /api/bestellingen/{reserveringId}/items/{menuItemCode}/min
DELETE /api/bestellingen/{reserveringId}/items/{menuItemCode}
GET    /api/bestellingen/{reserveringId}/bon

GET    /api/overzichten/kok
GET    /api/overzichten/ober
```

Deze API is bewust nog niet perfect. Sommige fouten horen bij de onderhoudsopdracht.
