# Concrete Life – Stil-Steckbrief (Unity)

> Ergebnis der Stil-Befragung vom 24.11.2026. Ergänzt/überschreibt Abschnitt 5 der CLAUDE.md, wo sie sich widersprechen.

## Referenzen
- **Caustic** – https://mrdoob.github.io/codevember-2026/10/ (hell, türkis, Caustics auf Flächen, Fischschwärme, Seegras)
- **Kelp** (auch Referenz für die Unterwasserpflanzen) – https://spite.github.io/codevember-2026/17/ (dunstig, filmisch, Gegenlicht, God Rays, Tiefenunschärfe, Filmkorn, Vignette)
- **Endzustand / Titelbild** – `Referenzen/endzustand_titelbild.webp` (Katja): Split-View auf der Wasserlinie, klares helles Türkis, Caustics auf Sandboden, gelbe Fische; sandfarbene, wabenartig poröse Panels plus Röhren-/Tümpel-Formen dicht an dicht, bewachsen mit Moos/Algen und lila Blüten; darüber alte Sandsteinmauer.
  - Gilt als **Farbwelt für „2 Jahre später“** und als **Vorlage für Titel/Start**.
  - Look aber **realistischer als das Bild** (weniger malerisch, nüchterneres Licht und Materialien).
  - Klippen/Steg im Bild ersetzen wir durch die **Sydney-Skyline** (Steg/Mauer optional im Vordergrund).

## Gesamtlook
- **Semi-realistisch**, mit Fokus auf Lichtstimmung.
- Gewünschte Elemente: **Caustics-Lichtmuster**, **Lichtstrahlen von oben**, **Dunst & Tiefe** (Far Blur, Grain, Vignette), **Pflanzen im Vordergrund**.
- Grundhelligkeit: **mittel** (Umgebung gut erkennbar, kein Stockdunkel).
- Gefühl: **neugierig, entdeckend**.
- **Viel Leben/Bewegung** im Bild (Partikel, Lichtmuster, schwankende Pflanzen, vorbeiziehende Fische).

## Heute vs. 2 Jahre später
- **Heute = Kelp-Stimmung:** dunstig, trüber, filmisch. Leben nur **am Rand** (Pflanzen am Bildrand/Boden, vorbeiziehende Fische), die Mauer selbst ist kahl.
- **2 Jahre später = Caustic-Stimmung:** klarer, heller, Caustics. Unterschied **deutlich, aber glaubwürdig** (bewachsene Panels, mehr Fische, klareres Wasser, kein Korallenriff).
- Farbe 2 Jahre später: **wie das Referenzbild** (klares Türkis, sonnig, satte Grüntöne).
- Farbe heute: dieselbe Grundwelt, aber **dunstiger, trüber, kühler** (Kelp-Stimmung), damit der Kontrast die Geschichte erzählt.

## Szene & Kamera
- **Split-View** auf der Wasserlinie (wie im Referenzbild) **nur für Titel/Start**.
- **Start über Wasser:** klarer Tag, Sydney-Skyline in 3D, einigermaßen realistisch und **lebendig** (Harbour Bridge, Opera House, Hochhäuser des CBD, Vögel, Stadtleben). Dann **Abtauchen**, danach bleibt alles unter Wasser.
- Unter Wasser: Kamera frontal und leicht von unten auf die Mauer (Vorschlag Claude, von Katja freigegeben).
- **Mauer:** verwitterter Beton (glatt, aber mit Flecken, Wasserlinien, Rostspuren von Ankern), Fugen.

### Szene „Slice_Mauer“ (heute), Antworten vom 24.11.2026
- Kamera: Claude entscheidet (Ausnahme, von Katja freigegeben). Sonst immer fragen.
- Ausschnitt **mittel** (einige Mauerplatten, Tiere gut erkennbar).
- **Grundregel: nichts darf sich sichtbar wiederholen** (Texturen, Flecken, Lichtmuster) – das gibt es in der Natur nicht.
- Mauer aus **hochkant stehenden Betonfertigteilen über die volle Höhe** (ca. 2,3–2,7 m breit, Precast Panels wie im Sydney Harbour), mit durchgehendem **Kopfbalken** oben. **Ungleichmäßige Fugen**, Anker/Hebepunkte **unregelmäßig** verteilt, Rostspuren verschieden lang. Verwitterung: **dunkle Wasserflecken, Rostspuren von Ankern, Abplatzungen** (kein Biofilm).
- Boden **schlammig-sandig mit etwas Müll** (Plastikflaschen/Dosen, Seil/Netzreste). In „2 Jahre später“ **ohne Müll**.
- Pflanzen: Vordergrund unten, seitlich am Bildrand, am Mauerfuß und im Hintergrund.
- Dunst und Lichtstrahlen **etwas schwächer als im Kelp-Beispiel**, Dunst **grünlich-trüb**.
- Wasseroberfläche als schmaler, heller, gewellter Streifen am **oberen Rand** sichtbar.
- Schwebeteilchen **mittel**. **Ab und zu ein Fischschwarm** im Hintergrund.

### Überarbeitung vom 24.11.2026 (nach Feedback zu v3)
- Licht wie im Kelp-Beispiel: **sichtbare Sonne** an der Oberfläche, **Strahlen fächern von dort auf**; keine harten, parallelen Lichtbalken.
- **Wasser muss als Wasser erkennbar sein**: Oberfläche von unten mit hellem Lichtfenster und dunklerem, spiegelndem Rand; oben heller, nach unten dunkler.
- **Caustics nur als leichter Schimmer**, nie als starkes Netz.
- Kamera **leicht von unten**, Blick an der Mauer hoch ins Licht.
- Die Mauer **endet nie sichtbar** (links/rechts läuft sie weiter, aktuell 60 m lang).
- Flaches Wasser ausprobiert und **verworfen** (Mauer wirkte zu niedrig). Wieder **7,6 m tief**, Mauer 7,2 m + Kopfbalken.
- Kamera weiter hinten (z = -11), Taucherin 1,3× größer.
- Kamera-Neigung folgt der Tiefe (unten ca. 6°, oben ca. 16° nach oben).
- **Auftauchen**: ganz oben fährt die Kamera zur Wasserlinie → geteiltes Bild (oben Himmel/Kai, unten Mauer unter Wasser). Skyline über Wasser folgt als eigener Schritt.
- Wasser-Dunst gilt nur für den Teil der Sichtlinie unter Wasser (über Wasser klar).
- Steuerung/Kamera (beschlossen): **Taucherin folgt dem Zeiger**, Kamera folgt ihr sanft entlang der Mauer (Mischung aus fester 2.5D-Ansicht und Verfolgerkamera). Keine freie WoW-Kamera (keine Tastatur in der Ausstellung).

## Figuren
- **Taucherin:** mittlere Größe (ca. 1/5 der Bildhöhe).
- Taucherin und Tiere **gleich realistisch** wie die Umgebung.

## Scanner
- **Scanner-Strahl** statt klassischer Taschenlampe: technischer Look mit **Scan-Linien/Raster** auf der beleuchteten Fläche, Cyan (#5FD3C6).
- **Fund-Effekt:** Hologramm-Overlay (kurzes Scan-Raster über dem Tier, dann Kontur + Label).
- **Scanner-UI:** physisches Gerät unten rechts (dunkelgrau, oranger Drehregler). Wenn es groß wird, **projiziert der Bildschirm als Hologramm** heraus.

## Pflanzen
- **Nur Arten aus dem Sydney Harbour** (z. B. Ecklonia-Kelp, Sargassum; vor dem Modellieren recherchieren).

## Offene Punkte
- Die Skyline ist aufwendig und nicht Teil des Vertical Slice: als eigenen Schritt planen oder mit fertigen Assets lösen
- Pflanzenarten recherchieren
