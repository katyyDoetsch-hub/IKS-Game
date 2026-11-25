# Concrete Life – Unity-Prototyp (Projektbriefing für Claude Code)

> Diese Datei gehört in den **Hauptordner des Unity-Projekts** (dort, wo `Assets/` liegt).
> Claude Code liest sie automatisch. Stand: 24.11.2026.

---

## 1. Worum es geht

**Concrete Life** ist ein interaktives Lernspiel für eine Ausstellung (Semesterausstellung, HfG Schwäbisch Gmünd,
Studiengang Interaktionsgestaltung, Kurs „Interaktive Kommunikationssysteme 1“, Gruppenarbeit).

Thema ist das reale Projekt **Living Seawalls** im **Sydney Harbour**: Betonpanels, die an glatte Ufermauern
geschraubt werden und Strukturen eines natürlichen Felsufers nachbilden (Spalten, Gezeitentümpel, raue Flächen).
Dadurch kehrt Leben an die Mauern zurück.

**Kernbotschaft:** Küstenbauten sind glatt und bieten Meerestieren keinen Lebensraum. Living Seawalls geben ihnen
die fehlenden Strukturen zurück.

**Rahmenbedingungen**
- Wissensvermittlung steht im Vordergrund; Look **professionell, nicht kindlich**.
- Spieldauer im Ausstellungskontext: ca. **5–6 Minuten**.
- Alle Texte im Spiel auf **Deutsch**.
- Der Prototyp muss nicht perfekt sein, aber **3D bzw. 2.5D** wirken und nicht „gebastelt“.

**Zusammenarbeit mit Katja (Projektleitung dieser Datei)**
- Katja möchte **Schritt für Schritt** arbeiten: eins nach dem anderen, nichts vorwegnehmen, nichts Großes ungefragt umbauen.
- Vor größeren Entscheidungen (Architektur, Packages, Szenenaufbau) kurz Optionen nennen und fragen.
- Katja baut Figuren und Assets **selbst in Blender** (geriggt). Bis die Modelle da sind: **Platzhalter** (Primitives/ProBuilder),
  so aufgebaut, dass Modelle später nur ausgetauscht werden müssen.
- Keine Zeitpläne erstellen.

---

## 2. Installation in der Ausstellung (wichtig für die Steuerung)

- Begehbarer **Kasten** (Lattengerüst + dunkler Molton) mit **Frontprojektion von oben** auf die Rückwand (keine Rückprojektion).
- Gesteuert wird mit **zwei Eingaben**:
  1. **Zeiger/Licht** – IR- oder LED-Pointer bzw. Air-Mouse → verhält sich wie die **Maus** (zeigen + gedrückt halten).
  2. **Drehregler** – USB-Drehknopf → verhält sich wie das **Mausrad** (bzw. Pfeiltasten ← →).
- Es gibt **keine Tastatur** für Besucher. Alles muss mit „zeigen, halten, kurz drücken, drehen“ funktionieren.
- Deshalb: Eingabe von Anfang an **abstrahieren** (eine Klasse `PlayerInput` liefert `PointerPosition`, `IsPressed`,
  `PressedThisFrame`, `DialDelta`). Maus/Mausrad als erste Implementierung.
- Testtasten (nur für Entwickler): Zahlen zum Springen zwischen Stationen, `R` Neustart, `Leertaste` = weiter.

---

## 3. Ziel bis zur Zwischenpräsentation (Freitag): ein Vertical Slice

Das **ganze Spiel** existiert bereits als **HTML-Prototyp** (Ablauf, Texte, Logik – siehe Abschnitt 9).
In Unity soll **nicht** alles nachgebaut werden, sondern ein kurzer Ausschnitt, der zeigt, **wie das Spiel aussehen soll**.

### Umfang (nach Priorität – in dieser Reihenfolge bauen)
1. **Die dunkle Hafenmauer in 3D**
   - Glatte Betonmauer mit Fugen (Textur/Normal Map), davor Wasser.
   - Feste, leicht schräge Kamera (**2.5D**, perspektivisch, wenig Bewegung).
   - Unterwasser-Stimmung: dunkel, blau-grüner Nebel, Lichtstrahlen von oben (God Rays/Caustics-Cookie), Schwebeteilchen.
2. **Taschenlampe = Scanner-Licht**
   - Spot Light, das per Raycast immer auf den Punkt der Mauer zeigt, auf den die Maus zeigt.
   - Weicher Lichtkegel, warmes Licht; wird beim Gedrückthalten etwas heller/größer.
3. **Ein verstecktes Tier finden und scannen**
   - Tier ist nur sichtbar, wenn es im Lichtkegel ist (Sichtbarkeit abhängig von Entfernung zum Lichtpunkt).
   - Tier **reagiert** aufs Licht (Fisch huscht kurz weg, Schnecke zieht sich ein, Auster schließt sich).
   - Gedrückt halten → Ring am Zeiger füllt sich (**1,2 s**) → Tier bekommt Kontur, Scanner meldet „1 / 3“.
   - Nach **6 s** ohne Fund: Ping/Richtungspfeil zum nächsten Tier.
4. **Die Taucherin** schwimmt im Bild, folgt dem Zeiger mit Abstand, hat ein **eigenes kleines Licht** und eine **helle Kontur**
   (Rim Light/Outline), damit sie sich vom dunklen Hintergrund abhebt.
5. **Scanner-Anzeige** unten rechts (UI): kleines Gerät mit Bildschirm (Status, „Tiere gefunden 0/3“, drei Tier-Silhouetten)
   und **Drehregler**, der ausgegraut ist, solange er nicht gebraucht wird.
6. **Optional, sehr wirkungsvoll:** Umschalten (z. B. Testtaste) auf **„2 Jahre später“**:
   dieselbe Mauer **dicht an dicht mit Panels** belegt, bewachsen (Algen, Seepocken, Austern), mehr Fische, etwas helleres Licht.

### Nicht im Slice (bleibt im HTML-Prototyp)
Briefing, Minispiele, Rückblick/Zeitreise, Lieferung der Panels, Montage, Ergebnis.

### Fertig ist der Slice, wenn
- man im Dunkeln mit dem Licht über die Mauer fährt, ein Tier entdeckt und es durch Halten scannt,
- die Taucherin sichtbar mitschwimmt,
- der Scanner in der Ecke den Fortschritt zeigt,
- es auf einem Beamer/großen Bildschirm stimmungsvoll und „echt 3D“ wirkt.

---

## 4. Technik & Empfehlungen

- **Unity mit URP** (Projekt ist bereits mit URP angelegt). Unity-Version bei Katja erfragen, falls relevant.
- **Kamera:** Perspective, fest, leicht von unten-schräg auf die Mauer. Kein freies Kamerasteuern.
  Für Zoom-Momente später: Cinemachine.
- **Licht:**
  - Directional Light sehr schwach, bläulich (Licht von oben durchs Wasser), mit Caustics als Light Cookie.
  - Taschenlampe: Spot Light (warm, ca. #FFF2D0), weicher Rand, Schatten an.
  - Taucherin: kleines Point Light am Kopf/Körper.
  - Ambient Light dunkel (Blau-Petrol).
- **Post-Processing (URP Volume):** Fog (Exponential, Petrol), Bloom (dezent), Vignette, Color Adjustments
  (leicht kühl, Kontrast), ggf. Depth of Field leicht für Tiefe.
- **Wasser-Atmosphäre:** Particle System für Schwebeteilchen und Luftblasen der Taucherin; leichte Licht-Strahlen (Quads mit Additive-Material).
- **UI:** Unity UI (Canvas, Screen Space Overlay) oder UI Toolkit – einfache Lösung bevorzugen.
  Schrift: eine klare Sans (z. B. Inter) + Mono für Gerätetexte (z. B. JetBrains Mono).
- **Input:** zunächst klassischer Input (Maus + Mausrad), hinter der eigenen `PlayerInput`-Abstraktion.
- **Blender → Unity:** Export als FBX, Transforms vor dem Export anwenden, Scale 1, „Apply Transform“,
  Forward −Z / Up Y; Rigs mit Animationen als eigene Clips. Platzhalter immer so benennen/strukturieren,
  dass das echte Modell als Kind-Objekt eingesetzt werden kann.

### Vorgeschlagene Ordnerstruktur
```
Assets/
  Scripts/        (Gameplay, UI, Input)
  Scenes/         (Slice_Mauer.unity)
  Materials/  Textures/  Models/  Prefabs/
  UI/             (Scanner-Grafiken, Schriften)
```

### Vorgeschlagene Skripte (Slice)
| Skript | Aufgabe |
|---|---|
| `PlayerInput` | Zeigerposition, Drücken/Halten, Drehregler (Mausrad). Austauschbar für Pointer/Drehknopf. |
| `FlashlightController` | Raycast vom Zeiger auf die Mauer, Spot Light zeigt dorthin, Intensität steigt beim Halten. |
| `HoldToScan` | Haltezeit messen, Ring am Zeiger füllen, Event auslösen (Dauer pro Aktion einstellbar). |
| `HiddenAnimal` | Sichtbarkeit nach Abstand zum Lichtpunkt, Reaktion aufs Licht, Zustand gefunden/gescannt. |
| `DiverController` | Taucherin folgt dem Zeiger mit Abstand und Trägheit, schaut in Bewegungsrichtung, Blasen. |
| `ScannerUI` | Scanner unten rechts: Status, Zähler, Tier-Silhouetten, Regler (aktiv/ausgegraut), später „Weiter ▶“. |
| `RadioDialog` | Funk-Untertitel (Schreibmaschinen-Effekt), läuft **nicht** automatisch weiter: Druck = Satz fertig, nächster Druck = weiter; wippender Pfeil ▼. |
| `SearchHint` | Nach 6 s ohne Fund Ping/Pfeil zum nächsten Tier. |
| `GameFlow` | Einfache Zustandsmaschine (für den Slice: Licht an → Suchen → Tier gescannt → fertig). |
| `TimeStateSwitcher` (optional) | Umschalten heute ↔ 2 Jahre später (Panelfeld + Bewuchs ein/aus). |

### Wichtige Werte aus dem HTML-Prototyp
- Halten: Start 1,0 s · Scannen 1,2 s · Montage 1,0 s · „Weiter“ (kurz drücken) 0,3 s
- Ping nach 6 s ohne Fund
- Dunkelheit unter Wasser ~0,6 (bei Ebbe heller); Lichtkegel-Radius ~270 px bei 1600×900
- Reset nach 120 s ohne Eingabe (Ausstellung), auf dem Endbildschirm nach 40 s
- Farben: Cyan `#5FD3C6` (Scanner/positiv), Amber `#F0B072` (Aufgabe/Achtung), Rot-Orange `#E8703C` (Problem),
  Sand `#E8B07A` (Archiv/Vergangenheit), Hintergrund `#03161A`, Text `#DDF3F0`

---

## 5. Gestaltung

- **Stil:** stylized 3D, ruhig und hochwertig, nicht comicartig. Dunkle Unterwasserwelt, das Licht macht die Welt sichtbar.
- **Taucherin:** Forscherin mit Tauchausrüstung (dunkler Anzug mit orangen Akzenten, orange Flossen), immer mit heller Kontur.
- **Scanner:** robustes Gerät (dunkelgrau, abgerundet) mit Bildschirm links und Drehregler rechts (orangener Zeiger).
  Er ist immer unten rechts sichtbar und **wächst** für Briefing/Archiv/Ergebnis bis fast auf Vollbild.
  Alle Infos (Status, Diagramme, Zähler) stehen **im Scanner**, nicht verteilt über den Bildschirm.
- **Aufgabe** oben mittig als Pille; leuchtet bei jeder neuen Aufgabe kurz auf.
- **Prinzipien aus dem Professoren-Feedback:** wenig UI, Infos langsam und in einem Panel (dem Scanner),
  Diagramme über die Zeit statt Wörter, Bedienelemente hervorheben, Taucherin vom Hintergrund abheben,
  langsame Übergänge, jede*r klickt im eigenen Tempo weiter.

---

## 6. Tiere und Panels

| Tier | braucht | Problem an der glatten Mauer | passendes Panel |
|---|---|---|---|
| **Austernschleimfisch** (Fisch) | Versteck (Spalte/Höhle) | kein Versteck – flieht vor dem Raubfisch und verlässt die Wand | **Höhlen-Panel** |
| **Gestreifte Kreiselschnecke** | Feuchtigkeit bei Ebbe | Wand trocknet bei Ebbe aus und heizt sich auf | **Tümpel-Panel** (Gezeitentümpel) |
| **Sydney-Felsenauster** | raue, zerklüftete Fläche | Larven kleben sich mit dem Fuß und biologischem „Zement“ fest – auf glattem Beton hält der Kleber nicht, sie rutschen ab | **Rillen-Panel** |

- Die große Auster sitzt allein in einer **Fuge** der Mauer (die einzige raue Stelle).
- **Panels:** Form wie Katjas `LS.svg` (gewelltes, rundliches Panel mit drei übereinanderliegenden Mulden).
  Real ca. **60 cm** Durchmesser, mit ca. **10 cm Abstand** zur Mauer montiert, Montage **bei Ebbe**.
  An echten Mauern hängen sie **dicht an dicht** (viele Panels nebeneinander), nicht einzeln.

---

## 7. Der komplette Ablauf (Referenz für später)

1. **Ankunft:** Forschungsboot im Sydney Harbour (Harbour Bridge im Hintergrund). Scanner wird groß → **Briefing** (4 Bilder, selbst weiterklicken):
   1. „Küstenbauten“ – „Mauern, Häfen, Molen, Wellenbrecher – alles, was Menschen an und ins Meer bauen.“ Zähler 32.000 km² (2018) → 39.400 km² (2028), Belgien-Vergleich.
   2. „Gebaut wird an den Küsten – dort, wo das Meer am artenreichsten ist.“
   3. „Im Sydney Harbour ist über die Hälfte des Ufers mit Küstenbauten verbaut.“
   4. „Küstenbauten sind glatt: keine Verstecke, kein Halt.“ (glatte Mauer vs. Felsufer)
   Danach Sprung ins Wasser, Fahrt an der leeren Mauer („Gar nichts los hier. Nur glatter Beton.“), **Licht einschalten** durch Halten.
2. **Entdecken:** Mit dem Licht suchen. Jedes gefundene Tier → direkt seine **Beobachtung** (kurze, nicht spielbare Sequenz, ca. 8–10 s,
   Label „BEOBACHTUNG“) → Abschlusskarte im Scanner (links „braucht“, rechts „Wand bietet ✕“) → weiter suchen.
3. **Übergang Archiv:** Funk „Drei Tiere, dreimal dasselbe Problem.“ / „War das hier schon immer so?“ / „Dein Scanner hat Archivdaten. Schau nach.“
   Scanner groß: „Drei Tiere, drei Probleme – Wie sah dieses Ufer früher aus?“
4. **Rückblick:** Regler zurückdrehen. Haltepunkte **heute** (glatter Beton, > 50 % verbaut) · **1844** (Sandsteinmauer Circular Quay: rau, mit Fugen;
   Austernriffe für Kalkmörtel verbrannt) · **1788** (Felsufer mit Spalten, Tümpeln, Austernriffen; alle drei Bedürfnisse ✓).
   Zeitachse nur mit Jahreszahlen (1800 · 1850 · 1900 · 1950 · heute), Betonmauern als schattierter Bereich „20. Jh.“, zwei Kurven (verbautes Ufer, Artenvielfalt).
   Zurück ins Heute → Taucherin per Funk: „Was wäre, wenn die Mauer wieder mehr Lebensraum bieten könnte?“
5. **Living Seawalls:** Boot lässt eine **Kiste** herab, darin **drei Panels** (Höhle, Tümpel, Rillen). Mit dem Licht untersuchen (je ein Bedürfnis-Symbol leuchtet),
   scannen → „PROJEKT ERKANNT · Living Seawalls · Forschungsprojekt des SIMS mit dem Reef Design Lab · seit 2018“ → Panels werden dicht an dicht montiert.
   **Ebbe:** an den drei Tierstellen selbst das passende Panel wählen (Drehregler) und montieren (halten). Die Zuordnung wird **nicht** vorher verraten.
   Dann montiert „das Team“ den Rest der Wand Panel an Panel. Flut, auftauchen.
6. **Zwei Jahre später:** Auf dem Boot „Langzeit-Monitoring“, Zeit vorwärts drehen (frei hin und her), „2 Jahre später“, Sprung, bewachsene Wand.
   **„Jetzt bist du dran“** → drei **spielbare Minispiele** mit Ziel-Karte und Countdown 3-2-1:
   - Fisch: in die Höhle des Panels, bevor der Raubfisch da ist (10 s)
   - Schnecke: in den Tümpel, bevor das Wasser weg ist (12 s)
   - Auster: mind. 20 Austernlarven auf das Rillen-Panel (14 s)
   Bei Misserfolg Neustart mit Hinweis, z. B. „Moment mal – da ist doch ein Versteck im Panel! Versuch es noch einmal.“
   → **Ergebnis:** +20 % Arten, 115 Arten in zwei Jahren (so viele wie am natürlichen Felsriff), Weltkarte (Sydney, Singapur, Gibraltar, Wales), „Mauern müssen nicht leblos sein.“

---

## 8. Fakten & Quellen (nur diese Werte verwenden)

| Fakt | Quelle |
|---|---|
| Über 50 % des Sydney-Harbour-Ufers sind Ufermauern | City of Sydney / SIMS: https://news.cityofsydney.nsw.gov.au/media-releases/living-seawalls-bring-new-life-to-our-city-harbour |
| Mauern halten bei Ebbe kaum Wasser; Beton trägt weniger Arten als Sandstein | NSW „Environmentally Friendly Seawalls“: https://www.environment.nsw.gov.au/sites/default/files/environmentally-friendly-seawalls-090328.pdf |
| Künstliche Strukturen: weniger Vielfalt, mehr gebietsfremde Arten | Mayer-Pinto et al. 2015, Marine and Freshwater Research |
| Circular-Quay-Mauer aus Sandstein, 1837–1844 | Engineering Heritage Australia: https://heritage.engineersaustralia.org.au/wiki/Place:Circular_Quay_Sea_Wall |
| Austern für Kalkmörtel verbrannt; ca. 99 % der Austernriffe in NSW verschwunden | NSW Marine Estate: https://www.marine.nsw.gov.au/news-and-more/newsroom/news/pre-2023/sydneys-forgotten-oyster-reefs |
| Muschelriffe = am stärksten bedrohtes Meeresökosystem Australiens | Gillies et al. 2018 (PLOS ONE), JCU-Meldung |
| Living Seawalls: SIMS + Reef Design Lab, seit 2018 | NSW Seabirds to Seascapes: https://www.environment.nsw.gov.au/topics/water/coasts/seabirds-to-seascapes |
| 115 Arten in 2 Jahren, ca. 20 % mehr als unveränderte Mauer, vergleichbar mit natürlichen Felsriffen; Panels ca. 60 cm, 10 cm Abstand; bis zu 10 °C kühler; > 1.000 Panels in Australien, Singapur, Gibraltar, Wales | Australian Geographic 2022: https://www.australiangeographic.com.au/news/2022/07/two-year-study-shows-living-seawalls-promote-regeneration-in-sydney-harbour/ |
| 32.000 km² Meeresfläche verbaut (2018), 39.400 km² erwartet (2028) | aus der Gruppenrecherche (vermutlich Bugnot et al. 2021) – Primärquelle noch prüfen |

**Noch Platzhalter:** Wassertemperatur 19 °C, Wandtemperatur bei Ebbe 34 °C (recherchieren). Artenvielfalt vor dem Bau: keine Zahl → Kurven ohne Achsenwerte.
Die frühere Zahl „+36 %“ nicht mehr verwenden.

---

## 9. Referenzdateien bei Katja

Ordner: `C:\Users\katyy\OneDrive\Dokumente\01_Uni\2. Semester\IKS\`
- `Simulation_Katja\concrete-life-spiel-v9.html` – aktueller HTML-Prototyp (kompletter Ablauf, alle Texte und Werte im `CONFIG`-Block oben im Skript).
  Im Browser öffnen; Tasten 1–8 springen zu den Stationen, H zeigt die Steuerung.
- `taucherin.svg`, `schleimfisch.svg`, `schnecke.svg`, `auster.svg` – 2D-Entwürfe der Figuren (Farben/Proportionen als Vorlage für Blender).
- `LS.svg` – Form des Living-Seawall-Panels (Vorlage zum Extrudieren in Blender).
- `LIVING-SEAWALLS-PRODUCT-INFORMATION-JULY-2023.pdf` – Produktinfo zu den echten Panels.

---

## 10. Erster Schritt mit Claude Code (Vorschlag)

1. Kurz bestätigen lassen: Unity-Version, Input (klassisch oder neues Input System).
2. Szene `Slice_Mauer` anlegen: Platzhalter-Mauer (Cube/Plane mit Fugen-Textur), Kamera, Licht, Volume mit Nebel.
3. `PlayerInput` + `FlashlightController` – erst wenn das Licht gut aussieht, weiter.
4. Dann Schritt für Schritt: Tier (`HiddenAnimal` + `HoldToScan`), Taucherin, Scanner-UI, Funk, optional „2 Jahre später“.
