# Design Patterns - FSM Viewer

## Creatiepatronen

### Builder
**Locatie:** `Builders/`

Elk state-type en `Transition` heeft een eigen concrete builder die stapsgewijs het object opbouwt.  
De `FSMDirector` orkestreert de bouw: hij accepteert een specifieke builder en configureert deze met de gegeven data, waarna `Build()` het eindproduct oplevert.

```
StateBuilder<TState,TBuilder>  (abstract)
  ├── InitialStateBuilder   → InitialState
  ├── FinalStateBuilder     → FinalState
  ├── SimpleStateBuilder    → SimpleState
  └── CompoundStateBuilder  → CompoundState
TransitionBuilder             → Transition
FSMDirector  (Director)
```

**Voordeel:** De constructie van complexe modelobjecten (met acties, children, triggers) is volledig losgekoppeld van de dataverwerking in de Factory.

---

### Factory (FSMFactory)
**Locatie:** `Factory/FSMFactory.cs`

`FSMFactory.Create(FSMDTO)` zet een volledig geparsed DTO om in een compleet `FiniteStateMachine`-object. De Factory kent het domein (welk builder-type bij welk state-type hoort) en gebruikt de `FSMDirector` intern.

**Verband met Builder:** Factory + Builder werken samen; de Factory beslíst wát er gebouwd wordt, de Director/Builder beslíst hóe.

---

## Structuurpatronen

### Composite
**Locatie:** `Model/States/`, `Model/IElement.cs`

De `State`-hiërarchie vormt een Composite:

```
IElement          (Component interface)
  State (abstract)
    InitialState  (Leaf)
    FinalState    (Leaf)
    SimpleState   (Leaf)
    CompoundState (Composite) ─── children: List<State>
                                  + AddChild / RemoveChild
  Transition      (Leaf, ook IElement)
```

`CompoundState` bevat een lijst van `State`-kinderen die zelf ook `CompoundState` kunnen zijn (onbeperkte nestdiepte). De abstracte klasse `State` bevat gedeelde logica (naam, acties) waardoor subklassen compact blijven.

**Voordeel:** Partiële rendering werkt automatisch — elk element kan onafhankelijk worden bezocht via `Accept(IVisitor)`.

---

## Gedragspatronen

### Visitor
**Locatie:** `Visitors/`

```
IVisitor
  ├── TextVisitor      → console-output
  └── GraphicalVisitor → PNG-bestand (System.Drawing)
```

Elk `IElement` implementeert `Accept(IVisitor)` en delegeert naar de bijbehorende `Visit`-methode op de visitor. Hierdoor kan dezelfde modelstructuur op twee totaal verschillende manieren worden weergegeven zonder het model te wijzigen.

**Partiële rendering:** Omdat elk element `Accept` implementeert, kan een enkel state, compound state of transitie onafhankelijk worden getekend (vereiste uit de opdracht).

---

### Strategy
**Locatie:** `FileHandling/`

De `FileHandler` kent drie verwisselbare strategieën:

| Interface          | Implementatie    | Verantwoordelijkheid                   |
|--------------------|------------------|----------------------------------------|
| `IFileReader`      | `TextFileReader` | Leest bytes van schijf                 |
| `IFileInterpreter` | `FileInterpreter`| Parset tekst naar `FSMDTO`             |
| `IFileValidator`   | `FileValidator`  | Controleert syntactische correctheid   |

Door afhankelijkheidsinjectie (constructor) kunnen alle drie onafhankelijk worden vervangen of uitgebreid (bijv. voor JSON-bestanden of remote reads).

---

## Overzicht

| Pattern   | Categorie  | Locatie                               |
|-----------|------------|---------------------------------------|
| Builder   | Creatie    | `Builders/`                           |
| Factory   | Creatie    | `Factory/FSMFactory.cs`               |
| Composite | Structuur  | `Model/States/`, `Model/IElement.cs`  |
| Visitor   | Gedrag     | `Visitors/`                           |
| Strategy  | Gedrag     | `FileHandling/`                       |

## Laagscheiding

- **Model-laag:** `Model/`, `Validators/` — geen UI-afhankelijkheden
- **Presentatielaag:** `Visitors/` — alleen visitor-implementaties kennen output-formaten
- **Applicatielaag:** `FSMApplication.cs`, `Program.cs` — koppelt alles samen

De presentatielaag kan volledig worden vervangen door een nieuwe `IVisitor`-implementatie toe te voegen zonder één regel modelcode te wijzigen.
